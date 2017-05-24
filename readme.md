# Entity Framework 6 Library - Brass9
At Brass Nine Design we use Entity Framework 6 for data storage, and that's led to developing extensions to the framework itself. Many of them are planned features that were canceled in favor of moving on to Entity Framework 7, and returning to them at some future date.

## Usage

Add the 4 projects (Brass9, Brass9.Collections, Brass9.Reflection, Brass9.Data) to your Solution, and reference them from your Project(s), including any projects with a DbContext in them.

### Guidelines
The library is built around the premise you put your Models in a separate Project from the majority of your code (eg B9, B9.Models), simply because [EF performs a lot better in this arrangement](https://msdn.microsoft.com/en-us/library/hh949853.aspx#2-4-2-Moving-your-model-to-a-separate-assembly). You don't have to, but it's a good idea.

It also assumes you have some factory mechanism for creating Db instances. A base class for doing so is provided in this library - `DbFactory`. But you generally don't have to use it, in case you're using some IoC container.

## Data Migrations

Data Migrations were long planned for EF but never arrived. This library makes them reasonably simple to perform.

### Usage:
1. Create a Db class as usual, that extends EF's `DbContext` or some subclass of `DbContext` (OWin's IdentityContext is fine, for example).
2. Create a Model class to store the status of DataMigrations. I typically call this `SystemSettings`, but any existing single-row table you use for storing configuration data etc will do. It should implement the `IDataMigrationStatus` interface. (The interface is simple; it has one DateTime property, LastMigrationDate).
3. In your regular Project (not the Models project, if using one), add a folder, WebApp, and another, DataMigrations. Your data migrations will live here.
4. To code your first, create a class and extend Brass9.Data.Entity.DataMigration<Db>
5. You'll need to implement 2 methods in your DataMigration - implementing `DataMigration<Db>` should outline it for you. First, set the date of the Migration in UTC, like `get { return new DateTime(... DateTimeKind.Utc)` (full sample code below). The system is going to use this the same way EF Migrations figures out where it's at in the schema.
6. Implement `Task MigrateAsync(Db)`, applying whatever data changes you need.
7. Finally, you need to ensure data migrations wakes up and runs at app start. You can do this however your IoC does it, or if you don't have one, run it from `Global.asax`

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

It's wise to wrap the Migration in some logging code, so if it fails you get the error and log it somewhere. There are logging tools in this library as well if you'd like to log to the Db.


## Other Features

### Database Logging

`Brass9.Data.Logging.Log`, and `Brass9.Data.Logging.Tagged.Log`, provide 2 different Logging base classes. `Logging.Log` is simple, with convenience methods for writing exceptions and log entries to the Db. `Logging.Tagged.Log` adds the ability to tag your log entries. Both feature some smart Exception logging, breaking apart multiply-wrapped Exceptions as EF's Sql Exceptions tend to present as (no more "See Inner Exception" with no inner exception logged).

### Fluent-to-Attributes
Allows you to use Fluent-only EF features in Attributes; for example normally Table-Per-Concrete requires a bunch of boilerplate code for each class that uses it, in your Db's OnModelCreating code, calling like `modelBuilder... m.MapInheritedProperties... m.ToTable(...` - [more details in this StackOverflow Q&A](https://stackoverflow.com/a/33896580/176877)

### Sql Server Geo/GPS Helper

`Brass9.Data.Spatial.GeoHelper` helps map GPS coordinates to Sql Server's Geo/GPS spatial search format.

### EF Schema Migrations Extensions

The normal class used for EF Migrations hides the main methods you'd use to extend what's available; in particular, the `Sql()` method is protected rather than public. `Brass9.Data.Entity.Migrations.B9DbMigration` provides an alternate base class for your migrations, which you can then pass to library code you write that calls `.Sql()` on your behalf. See the next feature for an example.

### Advanced Indexing

Advanced Query Plan optimizing usually involves adding several multi-column indexes, with directionality (asc/desc) and included columns (where the Index itself keeps a partial copy of data in it for faster queries). Neither of these features are available in the Index features that come with EF. `Brass9.Data.Entity.Migrations.IndexAdvancedMigration` provides both, and lets you define the index once (for example as a protected variable in the migration); just call its `Up()` and `Down()` methods in the Up and Down methods of your migration and it will figure out the Sql for you.

## Underlying Libraries

`Brass9.Reflection` is included; its best parts are likely `ReflectionHelper`, which has a lot of methods to help get methods and properties off of types and objects. `GetAllAssemblies()` simplifies the task of getting all the Projects in a Solution, and `GetAllPublicClasses()` builds on this to get every class in a Solution.

`AttributeHelper` helps build code like the Fluent-to-Attributes above, making it easy to find classes with a given attribute using `GetTypesWithAttribute()`.

`Brass9` adds C# 6's `?.` get-property-if-not-null accessor to older versions of C#, with the syntax `NN.N`. For example, in C# 6 you'd write:

    return obj?.Message;
    
In C# 5 without the library, you'd write

    if (obj == null)
        return null;
    
    return obj.Message;
    
With the library:

    return NN.N(obj, o => o.Message);