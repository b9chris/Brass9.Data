using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Linq.Expressions;


namespace Brass9.Data.Entity.DbSetExtensions
{
	public static class DbSetExtension
	{
		/// <summary>
		/// TODO: Assess whether this is a performance nightmare loading the entire db into memory.
		/// Probably should not be used anywhere until we're sure.
		/// 
		/// Remove all items in a list, for example the children related to an object.
		/// 
		/// Avoids basic EF gotchas in doing this kind of removal.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="_this"></param>
		/// <param name="list"></param>
		public static void RemoveItems<T>(this DbSet<T> _this, IQueryable<T> list)
			where T : class
		{
			// TODO: Use EFHelper.EnumeratePaged()
			// TODO: This is probably loading entire tables from the db into memory, slowly, synchronously
			foreach (var item in list.ToArray())
			{
				_this.Remove(item);
			}
		}
		public static async Task RemoveItemsAsync<T>(this DbSet<T> _this, IQueryable<T> list)
			where T : class
		{
			// Note: Some calls into this might not support IDbAsyncEnumerable - IQueryable doesn't guarantee it.
			// TODO: Figure out how we can guarantee that, or better communicate to the caller that their async call is not
			// being performed async...
			T[] arr;
			if (list is System.Data.Entity.Infrastructure.IDbAsyncEnumerable<T>)
				arr = await list.ToArrayAsync();
			else
				arr = list.ToArray();

			foreach (var item in arr)
			{
				_this.Remove(item);
			}
		}

		/// <summary>
		/// Remove items from a table by a given criteria. To remove items related to a given object, use RemoveItems()
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="_this"></param>
		/// <param name="where"></param>
		public static void RemoveWhere<T>(this DbSet<T> _this, Expression<Func<T, bool>> where)
			where T : class
		{
			// TODO: Use EFHelper.EnumeratePaged()
			_this.RemoveItems(_this.Where(where));
		}
		public static async Task RemoveWhereAsync<T>(this DbSet<T> _this, Expression<Func<T, bool>> where)
			where T : class
		{
			// TODO: Use EFHelper.EnumeratePaged()
			await _this.RemoveItemsAsync(_this.Where(where));
		}

		/// <summary>
		/// Remove everything in a table. Not everything related to an object - all data in the table will be destroyed.
		/// 
		/// Caution: Loads the entire table into memory before deleting it all, to be lazy. TODO: Optimize for streaming.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="_this"></param>
		public static void RemoveAll<T>(this DbSet<T> _this)
			where T : class
		{
			// TODO: Use EFHelper.EnumeratePaged()
			_this.RemoveItems(_this.ToArray().AsQueryable());
		}
		public static async Task RemoveAllAsync<T>(this DbSet<T> _this)
			where T : class
		{
			// TODO: Use EFHelper.EnumeratePaged()
			await _this.RemoveItemsAsync(_this);
		}
	}
}
