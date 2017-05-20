# Entity Framework 6 Library - Brass9
At Brass Nine Design we use Entity Framework 6 for data storage, and that's led to developing extensions to the framework itself. Many of them are planned features that were canceled in favor of moving on to Entity Framework 7, and returning to them at some future date.

### Guidelines
The library is built around the premise you put your Models in a separate Project from the majority of your code (eg B9, B9.Models), simply because EF performs a lot better in this arrangement. You don't have to, but it's a good idea.

It also assumes you have some factory mechanism for creating Db instances. A base class for doing so is provided in this library - DbFactory. But you generally don't have to use it, in case you're using some IoC container.

## Data Migrations

Data Migrations were long planned for EF but never arrived. This library makes them reasonably simple to perform.

### Usage:
1. Create a Db class as usual, that extends EF's `DbContext` or some subclass of `DbContext`.
2. Create a Model class to store the status of DataMigrations. I typically call this SystemSettings, but any existing single-row table you use for storing configuration data etc will do. It should implement the `IDataMigrationStatus` interface. (The interface is simple; it has one DateTime property, LastMigrationDate).
3. In your regular Project (not the Models project, if using one), add a folder, WebApp, and another, DataMigrations. Your data migrations will live here.
4. To code your first, create a class and extend Brass9.Data.Entity.DataMigration<Db>
5. You'll need to implement 2 methods in your DataMigration. First, make a constructor that simply sets the date of the Migration. The system is going to use this the same way EF Migrations figures out where it's at in the schema.
6. Implement `Task MigrateAsync(Db)`, applying whatever data changes you need.
7. Finally, you need to ensure data migrations wakes up and runs at app start. You can do this however your IoC does it, or if you don't have one, run it from Global.asax

Code below.

Running your DataMigrations at app startup:

	public class MvcApplication : System.Web.HttpApplication
	{
		protected void Application_Start()
		{
	var project = Assembly.GetExecutingAssembly();
	string rootNs = this.GetType().Namespace;
	rootNs = rootNs.Substring(0, rootNs.IndexOf('.'));
	
	Task.Run(async () => {
	string error = await Brass9.Data.Entity.DataMigrating.DataMigrator
		.ForDb<Db>(DbFactory.O.NewDb)
		.WithDataMigrationStatus(db => db.SystemSettings)
		.RunPendingMigrationsAsync(project, rootNs);
	}).ConfigureAwait(false);

A sample Db

	public class Db : DbContext
	{
		public DbSet<SystemSettings> SystemSettings { get; set; }
		
A sample data migration status class

	public class SystemSettings : Brass9.Data.Entity.DataMigrating.IDataMigrationStatus
	{
		public int Id { get; set; }
		public DateTime LastMigrationDate { get; set; }

		public SystemSettings()
		{
			LastMigrationDate = (DateTime)SqlDateTime.MinValue;
		}
	}

A sample data migration

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Data.Entity;
	using System.Threading.Tasks;
	using ConservX.Models;
	using ConservX.Models.Posting;
	
	namespace ConservX.WebApp.DataMigrations
	{
		public class M20161216AddTopicPropose : Brass9.Data.Entity.DataMigrating.DataMigration<Db>
		{
			public override DateTime DateTime
			{
				get { return new DateTime(2016, 12, 16, 6,0,0, DateTimeKind.Utc); }
			}
	
			public override async Task MigrateAsync(Db db)
			{
				var highestOrderIndex = await db.Topics.AsNoTracking()
					.Where(t => t.ParentKey == null)
					.Select(t => t.OrderIndex)
					.OrderByDescending(t => t)
					.FirstAsync();
	
				db.Topics.Add(new Topic
				{
					Key = "propose",
					Name = "Propose a Project or Topic",
					Subtitle = "Propose a new section of discussion - a new project, or a topic.",
					OrderIndex = highestOrderIndex + 1,
					IsSpecial = true
				});
	
				await db.SaveChangesAsync();			
			}
		}
	}
    
As you can see you just throw a date on there and get to use existing EF like you're used to, to query, update and insert new data, instead of having to resort to raw SQL.

### More Guidelines

It's wise to wrap the Migration in some logging code, so if it fails you get the error and log it somewhere. There are logging tools in this library as well if you'd like to log to the Db - that I'll document later.
