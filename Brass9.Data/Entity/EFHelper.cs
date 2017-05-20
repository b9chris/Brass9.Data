using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Data.Entity;
using System.Threading.Tasks;
using Brass9.Reflection;
using Brass9.Collections.HasProp;

namespace Brass9.Data.Entity
{
	public static class EFHelper
	{
		/// <summary>
		/// Convert a DbEntityValidationException to a string
		/// </summary>
		/// <param name="ex"></param>
		/// <returns></returns>
		public static string DbContextValidationErrorsToString(System.Data.Entity.Validation.DbEntityValidationException ex)
		{
			var sb = new StringBuilder();
			foreach (var err in ex.EntityValidationErrors.Where(e => !e.IsValid))
			{
				foreach (var v in err.ValidationErrors)
				{
					sb.Append(v.PropertyName).Append(": ").AppendLine(v.ErrorMessage);
				}
			}

			return sb.ToString();
		}

		/// <summary>
		/// Attach an Entity to the DbContext that has not yet been loaded into this
		/// DbContext - typically the object comes from another, now closed, DbContext.
		/// 
		/// http://stackoverflow.com/a/15694357/176877
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="entity"></param>
		/// <param name="db"></param>
		public static void ReattachModified<T>(T entity, DbContext db)
			where T : class
		{
			var entry = db.Entry<T>(entity);
			entry.State = EntityState.Modified;
		}

		/// <summary>
		/// Update an Entity in the DbContext that has been loaded, but that loaded Entity
		/// hasn't been modified - instead another Entity from outside this DbContext contains
		/// updated values (for example, loading one from the db, and the other having
		/// come in from a user submitting a form).
		/// 
		/// http://stackoverflow.com/a/15694357/176877
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="loaded"></param>
		/// <param name="modified"></param>
		/// <param name="db"></param>
		/// <returns></returns>
		public static void UpdateLoaded<T>(T loaded, T modified, DbContext db)
			where T : class
		{
			db.Entry<T>(loaded).CurrentValues.SetValues(modified);
		}

		/// <summary>
		/// Trivially, slowly copies modified values to the loaded Entity.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="loaded"></param>
		/// <param name="modified"></param>
		public static void CopyToLoaded<T>(T loaded, T modified)
		{
			ModelTransform.Copy(modified, loaded);
		}

		/// <summary>
		/// Update a loaded Model from the db to match a modified one, including updating immediate child
		/// lists. Does not work recursively - lists inside lists will not be updated, and if included in
		/// modified will probably cause this to explode.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="loaded"></param>
		/// <param name="modified"></param>
		/// <param name="db"></param>
		public static void UpdateLoadedAndChildren<T>(T loaded, T modified, DbContext db)
			where T : class
		{
			var props = ReflectionHelper.GetPublicProperties(typeof(T));
			foreach(var prop in props)
			{
				// Only bother with lists, that are present in modified.
				var modifiedValue = prop.GetValue(modified);
				if (modifiedValue == null || !(modifiedValue is IList))
					continue;

				var loadedValue = prop.GetValue(loaded);
				if (loadedValue == null)
					throw new NullReferenceException("Child list " + prop.Name + " needs to be loaded from the db to be updated.");

				// Build a generic method up for syncing loaded and modified together, and call it.
				var syncListMethod = typeof(EFHelper).GetMethod("SyncModifiedToLoadedList")
					.MakeGenericMethod(modifiedValue.GetType().GetGenericArguments()[0]);
				syncListMethod.Invoke(null, new[] { loadedValue, modifiedValue, db });
			}

			UpdateLoaded(loaded, modified, db);
		}

		/// <summary>
		/// Trims the loaded collection to the length of the modified one.
		/// 
		/// Use before modifying the rest of the entries in a loaded collection.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="loaded"></param>
		/// <param name="modified"></param>
		/// <param name="db"></param>
		public static void DeleteExtras<T>(ICollection<T> loaded, ICollection<T> modified, DbContext db)
			where T : class
		{
			int lengthDifference = loaded.Count - modified.Count;
			// 1) Remove items from the loaded list until it's at most the length of modified (if it's shorter we'll deal with that in Step 3)
			foreach (var item in loaded.ToArray())
			{
				if (lengthDifference > 0)
				{
					lengthDifference--;
					db.Entry(item).State = EntityState.Deleted;
					loaded.Remove(item);
				}
				else
				{
					break;
				}
			}
		}

		public static void SyncModifiedToLoadedList<T>(ICollection<T> loaded, ICollection<T> modified, DbContext db)
			where T : class
		{
			if (modified == null)
				modified = new T[0];

			DeleteExtras(loaded, modified, db);

			// 2) Copy values over between loaded and modified
			int iModified = 0;
			var modArray = modified.ToArray();
			foreach(var item in loaded)
			{
				CopyToLoaded(item, modArray[iModified]);
				iModified++;
			}

			// 3) Add items to loaded until it's at least the length of modified
			for (int i = loaded.Count; i < modArray.Length; i++)
				loaded.Add(modArray[i]);
		}

		/// <summary>
		/// Used to optionally open a new db connection or reuse an existing one.
		/// 
		/// Takes a Db as first argument, and code (in the form of an action) to run against it as second argument.
		/// 
		/// db argument can be null.
		/// 
		/// If db is null:
		/// 
		/// Creates a new db inside a using block and passes that to the Action to use.
		/// 
		/// If db is not null:
		/// 
		/// Passes that to the Action, and leaves the db open (not Disposed) after the Action has completed.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="db"></param>
		/// <param name="action"></param>
		public static void Using<T>(T db, Action<T> action)
			where T : DbContext, new()
		{
			if (db == null)
			{
				using (db = new T())
				{
					action(db);
				}
			}
			else
			{
				action(db);
			}
		}

		/// <summary>
		/// Enumerates an EF query in pages, opening and closing its own DbContext for each page. Allows you to use a separate
		/// DbContext inside the loop operating on these objects - for example so you can open a web connection and wait on it without
		/// leaving an idle db connection/partially loaded reader open in the meantime, and without running into the failed SaveChanges()
		/// EF issue.
		/// </summary>
		/// <typeparam name="TDb"></typeparam>
		/// <typeparam name="TModel"></typeparam>
		/// <param name="pageSize"></param>
		/// <param name="query"></param>
		/// <returns></returns>
		public static IEnumerable<TModel> EnumeratePaged<TDb, TModel>(int pageSize, Func<TDb, IOrderedQueryable<TModel>> query)
			where TDb : DbContext, new()
		{
			return EnumeratePaged<TDb, TModel>(pageSize, new BaseDbFactory<TDb>(), query);
		}

		/// <summary>
		/// Enumerates through pages of items in the query, but leaves the DbContexts it creates open while the body of the foreach is executing,
		/// meaning virtual navigation properties will succeed.
		/// </summary>
		/// <typeparam name="TDb"></typeparam>
		/// <typeparam name="TModel"></typeparam>
		/// <param name="pageSize"></param>
		/// <param name="dbFactory"></param>
		/// <param name="query"></param>
		/// <returns></returns>
		public static IEnumerable<TModel> EnumeratePaged<TDb, TModel>(int pageSize, BaseDbFactory<TDb> dbFactory, Func<TDb, IOrderedQueryable<TModel>> query)
			where TDb : DbContext, new()
		{
			int length = pageSize;
			for (int page = 0; length == pageSize; page++)
			{
				TModel[] items;
				using (var db = dbFactory.NewDb())
				{
					items = query(db).Skip(page * pageSize).Take(pageSize).ToArray();

					foreach (var item in items)
						yield return item;
				}

				length = items.Length;
			}
		}

		/// <summary>
		/// Enumerates over a long list asynchronously, in pages, taking action on each item via an Action argument, rather than leaving it to
		/// IEnumerable, foreach, IObservable, Rx Extensions, or other complexities.
		/// 
		/// Note that the synchronous approach uses yield return, which unrolls loops; that's incompatible with Task unless we use this Action
		/// approach or the more complicated options above.
		/// </summary>
		/// <typeparam name="TDb"></typeparam>
		/// <typeparam name="TModel"></typeparam>
		/// <param name="pageSize"></param>
		/// <param name="db"></param>
		/// <param name="query"></param>
		/// <param name="action">loop body</param>
		/// <param name="pageAction">An optional action to take at the end of each Page's worth of work</param>
		/// <returns></returns>
		public static async Task EnumeratePagedAsync<TDb, TModel>(int pageSize, TDb db,
			Func<TDb, IOrderedQueryable<TModel>> query,
			Action<TModel> action,
			Func<IEnumerable<TModel>, Task> pageAction = null)
		{
			int size = pageSize;
			int iRow = 0;
			do
			{
				var items = await query(db)
					.Skip(iRow).Take(pageSize).ToArrayAsync();

				size = items.Length;
				iRow += size;

				foreach (var item in items)
				{
					action(item);
				}

				if (pageAction != null)
					await pageAction(items);
			} while (size == pageSize);
		}

		/// <summary>
		/// Identical to EnumeratePagedAsync, but runs actions on each Page, not on the individual items.
		/// </summary>
		public static async Task EnumeratePagesAsync<TDb, TModel>(int pageSize, TDb db,
			Func<TDb, IOrderedQueryable<TModel>> query,
			Func<IEnumerable<TModel>, Task> pageAction)
		{
			int size = pageSize;
			int iRow = 0;
			do
			{
				var items = await query(db)
					.Skip(iRow).Take(pageSize).ToArrayAsync();

				size = items.Length;
				iRow += size;

				if (pageAction != null)
					await pageAction(items);
			} while (size == pageSize);
		}


		/// <summary>
		/// Alternative signature that allows an asynchronous action on each item.
		/// 
		/// Enumerates over a long list asynchronously, in pages, taking action on each item via an Action argument, rather than leaving it to
		/// IEnumerable, foreach, IObservable, Rx Extensions, or other complexities.
		/// 
		/// Note that the synchronous approach uses yield return, which unrolls loops; that's incompatible with Task unless we use this Action
		/// approach or the more complicated options above.
		/// </summary>
		/// <typeparam name="TDb"></typeparam>
		/// <typeparam name="TModel"></typeparam>
		/// <param name="pageSize"></param>
		/// <param name="db"></param>
		/// <param name="query"></param>
		/// <param name="action">loop body</param>
		/// <returns></returns>
		public static async Task EnumeratePagedAsync<TDb, TModel>(int pageSize, TDb db,
			Func<TDb, IOrderedQueryable<TModel>> query,
			Func<TModel, Task> action,
			Func<IEnumerable<TModel>, Task> pageAction = null)
		{
			int size = pageSize;
			int iRow = 0;
			do
			{
				var items = await query(db)
					.Skip(iRow).Take(pageSize).ToArrayAsync();

				size = items.Length;
				iRow += size;

				foreach(var item in items)
				{
					await action(item);
				}

				if (pageAction != null)
					await pageAction(items);
			} while (size == pageSize);
		}

		/// <summary>
		/// Calls db.SaveChangesAsync() after running the loop body action on each page of items.
		/// 
		/// Paging is hidden behind the scenes - action is taken per-item, not per-page.
		/// </summary>
		/// <param name="query">An Ordered query on the items to process, written as a Func, like
		/// _db => _db.Blog.OrderBy(b => b.Id)</param>
		/// <param name="action">A non-async action to take on each item</param>
		public static async Task ProcessPagedAsync<TDb, TModel>(int pageSize, TDb db,
			Func<TDb, IOrderedQueryable<TModel>> query,
			Action<TModel> action)
			where TDb : DbContext
		{
			await EFHelper.EnumeratePagedAsync(pageSize, db, query, action, async (items) =>
			{
				await db.SaveChangesAsync();
			});
		}

		/// <summary>
		/// Calls db.SaveChangesAsync() after running the loop body action on each page of items.
		/// 
		/// Differs from the one-Async version in that the action is an async Task, allowing for additional
		/// queries, web requests, etc.
		/// </summary>
		/// <param name="query">An Ordered query on the items to process, written as a Func, like
		/// _db => _db.Blog.OrderBy(b => b.Id)</param>
		/// <param name="action">An async action to take on each item</param>
		public static async Task ProcessPagedAsyncAsync<TDb, TModel>(int pageSize, TDb db,
			Func<TDb, IOrderedQueryable<TModel>> query,
			Func<TModel, Task> action)
			where TDb : DbContext
		{
			await EFHelper.EnumeratePagedAsync(pageSize, db, query, action, async (items) =>
			{
				await db.SaveChangesAsync();
			});
		}



		public static async Task<int> DeletePagedAsync<TDb, TModel>(int pageSize, TDb db, DbSet<TModel> dbSet, IOrderedQueryable<TModel> query)
			where TDb : DbContext where TModel : class
		{
			int count = 0;
			var arr = await query.Take(pageSize).ToArrayAsync();

			while(arr.Length > 0)
			{
				count += arr.Length;

				foreach (var item in arr)
				{
					dbSet.Remove(item);
				}
				await db.SaveChangesAsync();

				arr = await query.Take(pageSize).ToArrayAsync();
			}

			return count;
		}



		/// <summary>
		/// Pages through items, and detaches from the DbContext for each page; the body of your foreach loop will execute detached, meaning
		/// no lazy loading or other EF access is possible - only the detached objects as-is are readable (no virtual navigation properties)
		/// and no db is available for actions like .SaveChanges();
		/// </summary>
		/// <typeparam name="TDb"></typeparam>
		/// <typeparam name="TModel"></typeparam>
		/// <param name="pageSize"></param>
		/// <param name="dbFactory"></param>
		/// <param name="query"></param>
		/// <returns></returns>
		public static IEnumerable<TModel> EnumeratePagedDetached<TDb, TModel>(int pageSize, BaseDbFactory<TDb> dbFactory, Func<TDb, IOrderedQueryable<TModel>> query)
			where TDb : DbContext, new()
		{
			int length = pageSize;
			for (int page = 0; length == pageSize; page++)
			{
				TModel[] items;
				using (var db = dbFactory.NewDb())
				{
					items = query(db).Skip(page * pageSize).Take(pageSize).ToArray();
				}

				foreach (var item in items)
					yield return item;

				length = items.Length;
			}
		}


		/// <summary>
		/// Enumerates an EF query in pages, but reuses the passed in DbContext. Works around the failed SaveChanges() EF issue when
		/// calling it with an open DataReader (like a normal foreach loop on a query with SaveChanges inside would), but does not close
		/// the Db connection between page loads - if what happens inside your loop can run long, consider the other overload.
		/// </summary>
		/// <typeparam name="TDb"></typeparam>
		/// <typeparam name="TModel"></typeparam>
		/// <param name="pageSize"></param>
		/// <param name="db"></param>
		/// <param name="query"></param>
		/// <returns></returns>
		public static IEnumerable<TModel> EnumeratePaged<TModel>(int pageSize, IOrderedQueryable<TModel> query)
		{
			int length = pageSize;
			for (int page = 0; length == pageSize; page++)
			{
				TModel[] items;
				items = query.Skip(page * pageSize).Take(pageSize).ToArray();

				foreach (var item in items)
					yield return item;

				length = items.Length;
			}
		}

		/// <summary>
		/// Page through a query, handing back enumerable pages of objects, so you can not only loop through the objects themselves, but
		/// also take action between pages.
		/// </summary>
		/// <typeparam name="TModel">Model type</typeparam>
		/// <param name="pageSize">Page size</param>
		/// <param name="query">A query, that may include where, include, but must finish with an OrderBy call.</param>
		/// <returns>An IEnumerable of pages, which are themselves IEnumerables of TModel objects.</returns>
		public static IEnumerable<IEnumerable<TModel>> PagesOf<TModel>(int pageSize, IOrderedQueryable<TModel> query)
		{
			int length = pageSize;
			for (int page = 0; length == pageSize; page++)
			{
				TModel[] items;
				items = query.Skip(page * pageSize).Take(pageSize).ToArray();

				yield return items;

				length = items.Length;
			}
		}

		/// <summary>
		/// Gets a page of size pageSize items, inside a db from dbFactory, and passes that to foreachBody(page, db).
		/// 
		/// Then closes the DbContext to keep EF from freaking out on long jobs, and does the above steps again.
		/// 
		/// Turns out to be the best way to handle processing a lot of items in EF. As an extra tweak, set a dirty flag at the top of the
		/// loop body you pass in, flag it when you change anything, and call .SaveChanges() just the once as the loop body is ending if
		/// the flag is set.
		/// </summary>
		/// <typeparam name="TDb"></typeparam>
		/// <typeparam name="TModel"></typeparam>
		/// <param name="pageSize"></param>
		/// <param name="dbFactory"></param>
		/// <param name="query"></param>
		/// <param name="foreachBody">The body you'd normally place in a foreach loop for pages of items.</param>
		public static void PagesOf<TDb, TModel>(int pageSize, BaseDbFactory<TDb> dbFactory, Func<TDb, IOrderedQueryable<TModel>> query, Action<IEnumerable<TModel>, TDb> foreachBody)
			where TDb : DbContext, new()
		{
			int length = pageSize;
			for (int page = 0; length == pageSize; page++)
			{
				using (var db = dbFactory.NewDb())
				{
					var items = query(db).Skip(page * pageSize).Take(pageSize).ToArray();
					foreachBody(items, db);
					length = items.Length;
				}
			}
		}

		// Note: This does not work.
		/*public static string GetSql(IQueryable query)
		{
			// http://stackoverflow.com/a/1412902/176877
			var sql = ((System.Data.Entity.Core.Objects.ObjectQuery)query).ToTraceString();
			return sql;
		}*/
	}

	public class EFHelper<TDb>
		where TDb : DbContext, new()
	{
		/// <summary>
		/// Cheaply remove 
		/// </summary>
		/// <typeparam name="TModel"></typeparam>
		/// <param name="dbSetSelector"></param>
		/// <param name="id"></param>
		/// <returns></returns>
		public async static Task RemoveAsync<TModel>(Func<TDb, DbSet<TModel>> dbSetSelector, int id)
			where TModel : class, IId
		{
			using (var db = new TDb())
			{
				var dbSet = dbSetSelector(db);
				var model = await dbSet.FindAsync(id);
				dbSet.Remove(model);
				await db.SaveChangesAsync();
			}
		}
	}
}
