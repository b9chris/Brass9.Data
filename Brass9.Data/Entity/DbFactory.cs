using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Threading.Tasks;


namespace Brass9.Data.Entity
{
	public class DbFactory<TDb>
		where TDb : DbContext, new()
	{
		protected EFConfig defaultConfig;

		public DbFactory()
			: this(DefaultConfig)
		{
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="config">The default configuration to initialize new Dbs with</param>
		public DbFactory(EFConfig config)
		{
			this.defaultConfig = config;
		}

		// We can't use parameter defaults syntax here to use as a Func in Using() below.
		public TDb NewDb(EFConfig config)
		{
			if (config == null)
				config = defaultConfig;

			var db = new TDb();
			db.Configuration.AutoDetectChangesEnabled = config.AutoDetectChangesEnabled;
			db.Configuration.LazyLoadingEnabled = config.LazyLoadingEnabled;
			db.Configuration.ProxyCreationEnabled = config.ProxyCreationEnabled;
			db.Configuration.ValidateOnSaveEnabled = config.ValidateOnSaveEnabled;
			return db;
		}
		public TDb NewDb()
		{
			return NewDb(null);
		}


		public void Using(TDb db, Action<TDb> action)
		{
			useDb(NewDb, db, action);
		}

		public void UsingCheapReads(TDb db, Action<TDb> action)
		{
			useDb(CheapReads, db, action);
		}

		public void UsingCheapWrites(TDb db, Action<TDb> action)
		{
			useDb(CheapWrites, db, action);
		}

		protected void useDb(Func<TDb> createDb, TDb db, Action<TDb> action)
		{
			if (db == null)
			{
				using (db = createDb())
				{
					action(db);
				}
			}
			else
			{
				action(db);
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

		public async Task UsingCheapWritesAsync(TDb db, Func<TDb, Task> action)
		{
			await usingAsync(CheapWrites, db, action);
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


		public async Task<TModel> UsingAsync<TModel>(TDb db, Func<TDb, Task<TModel>> action)
		{
			return await usingAsync(NewDb, db, action);
		}

		public async Task<TModel> UsingCheapReadsAsync<TModel>(TDb db, Func<TDb, Task<TModel>> action)
		{
			return await usingAsync(CheapReads, db, action);
		}

		public async Task<TModel> UsingCheapWritesAsync<TModel>(TDb db, Func<TDb, Task<TModel>> action)
		{
			return await usingAsync(CheapWrites, db, action);
		}

		protected async Task<TModel> usingAsync<TModel>(Func<TDb> createDb, TDb db, Func<TDb, Task<TModel>> action)
		{
			if (db == null)
			{
				using (db = createDb())
				{
					return await action(db);
				}
			}
			else
			{
				return await action(db);
			}
		}


		public TDb CheapReads()
		{
			return NewDb(EFConfig.CheapReads);
		}

		public TDb CheapWrites()
		{
			return NewDb(EFConfig.CheapWrites);
		}

		


		public static EFConfig DefaultConfig
		{
			get
			{
				return new EFConfig
				{
					AutoDetectChangesEnabled = true,
					LazyLoadingEnabled = true,
					ProxyCreationEnabled = true,
					ValidateOnSaveEnabled = true
				};
			}
		}
	}
}
