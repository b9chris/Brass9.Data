using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Threading.Tasks;


namespace Brass9.Data.Entity
{
	public class BaseDbFactory<TDb>
		where TDb : DbContext, new()
	{
		public TDb DefaultDb()
		{
			var db = new TDb();
			return db;
		}

		public TDb NewDb()
		{
			return DefaultDb();
		}


		public TDb NewDb(EFConfig config)
		{
			var db = new TDb();
			db.Configuration.AutoDetectChangesEnabled = config.AutoDetectChangesEnabled;
			db.Configuration.LazyLoadingEnabled = config.LazyLoadingEnabled;
			db.Configuration.ProxyCreationEnabled = config.ProxyCreationEnabled;
			db.Configuration.ValidateOnSaveEnabled = config.ValidateOnSaveEnabled;
			return db;
		}

		public TDb CheapReads()
		{
			return NewDb(EFConfig.CheapReads);
		}

		public TDb CheapWrites()
		{
			return NewDb(EFConfig.CheapWrites);
		}



		protected async Task usingAsync(Func<TDb> createDb, TDb db, Func<TDb, Task> action)
		{
			if (db == null)
			{
				using (db = createDb())
				{
					await action(db);
				}
			}
			else
			{
				await action(db);
			}
		}
		protected async Task<TResult> usingAsync<TResult>(Func<TDb> createDb, TDb db, Func<TDb, Task<TResult>> fn)
		{
			if (db == null)
			{
				using (db = createDb())
				{
					return await fn(db);
				}
			}
			else
			{
				return await fn(db);
			}
		}

		public async Task UsingAsync(TDb db, Func<TDb, Task> action)
		{
			await usingAsync(NewDb, db, action);
		}

		public async Task UsingCheapReadsAsync(TDb db, Func<TDb, Task> action)
		{
			await usingAsync(CheapReads, db, action);
		}
		public async Task<TResult> UsingCheapReadsAsync<TResult>(TDb db, Func<TDb, Task<TResult>> fn)
		{
			return await usingAsync(CheapReads, db, fn);
		}

		public async Task UsingCheapWritesAsync(TDb db, Func<TDb, Task> action)
		{
			await usingAsync(CheapWrites, db, action);
		}
	}
}
