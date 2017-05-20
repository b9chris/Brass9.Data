using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlTypes;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Brass9.Reflection;

namespace Brass9.Data.Entity.DataMigrating
{
	public class DataMigrator
	{
		public static DataMigrator<TDb> ForDb<TDb>(Func<TDb> makeDb)
			where TDb : DbContext
		{
			return new DataMigrator<TDb>(makeDb);
		}
	}

	public class DataMigrator<TDb> : DataMigrator
		where TDb : DbContext
	{
		protected Func<TDb> makeDb;
		
		public DataMigrator(Func<TDb> makeDb)
		{
			this.makeDb = makeDb;
		}

		public DataMigrator<TDb, TDataMigrationStatus> WithDataMigrationStatus<TDataMigrationStatus>(
			Func<TDb, DbSet<TDataMigrationStatus>> getMigrationStatus)
			where TDataMigrationStatus : class, IDataMigrationStatus, new()
		{
			return new DataMigrator<TDb, TDataMigrationStatus>(makeDb, getMigrationStatus);
		}
	}

	public class DataMigrator<TDb, TDataMigrationStatus>
		: DataMigrator<TDb>
		where TDb : DbContext
		where TDataMigrationStatus : class, IDataMigrationStatus, new()
	{
		protected Func<TDb, DbSet<TDataMigrationStatus>> getMigrationStatusTable;

		public DataMigrator(
			Func<TDb> makeDb,
			Func<TDb, DbSet<TDataMigrationStatus>> getMigrationStatusTable
		)
			: base(makeDb)
		{
			this.getMigrationStatusTable = getMigrationStatusTable;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ns">The Project Namespace, like PfcDay</param>
		/// <returns></returns>
		public async Task<string> RunPendingMigrationsAsync(Assembly project, string ns)
		{
			using(var db = makeDb())
			{
				var settings = await GetAsync(db);

				var classNamespace = ns + ".WebApp.DataMigrations";
				var migrationClasses = ReflectionHelper.GetAllClassesInNamespace(project, classNamespace, false);
				
				//var migrations = migrationClasses
					//.Select(c => Activator.CreateInstance(c))
					//.Cast<DataMigration>()
					//.OrderBy(m => m.DateTime)
					//.ToArray();
				
				var pendingMigrations = migrationClasses
					.Select(c => Activator.CreateInstance(c))
					.Cast<DataMigration<TDb>>()
					.Where(m => m.DateTime > settings.LastMigrationDate)
					.OrderBy(m => m.DateTime)
					.ToArray();

				if (pendingMigrations.Length == 0)
					return null;

				DateTime dt = DateTime.MinValue;

				try
				{
					foreach (var migration in pendingMigrations)
					{
						dt = migration.DateTime;
						await migration.MigrateAsync(db);
					}

					var lastMigration = pendingMigrations.Last();
					settings.LastMigrationDate = lastMigration.DateTime;
					await db.SaveChangesAsync();
				}
				catch(Exception ex)
				{
					string fullEx = ExceptionHelper.FullExceptionString(ex);
					return "Data Migration " + dt.ToShortDateString() + " failed:\n" + fullEx;
				}

				return null;
			}
		}

		/*
		public void Test()
		{
			string ns = this.GetType().Namespace;
			ns = "Parasol.WebApp.DataMigrating.Migrations";
			var migrationClasses = ReflectionHelper.GetAllClassesInNamespace(ns, false);

			var list = new List<DataMigration>();
			foreach (var c in migrationClasses)
			{
				object x = Activator.CreateInstance(c);
				list.Add((DataMigration)x);
			}
		}
		*/

		public async Task<TDataMigrationStatus> GetAsync(TDb db)
		{
			var systemSettings = await getMigrationStatusTable(db).FirstOrDefaultAsync();

			if (systemSettings != null)
				return systemSettings;

			return await CreateAsync(db);
		}

		public async Task<TDataMigrationStatus> CreateAsync(TDb db)
		{
			var systemSettings = new TDataMigrationStatus
			{
				LastMigrationDate = (DateTime)SqlDateTime.MinValue
			};

			getMigrationStatusTable(db).Add(systemSettings);
			await db.SaveChangesAsync();

			return systemSettings;
		}
	}
}
