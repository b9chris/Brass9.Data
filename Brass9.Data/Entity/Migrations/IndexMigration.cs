using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brass9.Data.Entity.Migrations
{
	/// <summary>
	/// Define an index once, Up() and Down() are provided for you.
	/// </summary>
	public class IndexMigration
	{
		public string Table;
		public string[] Columns;

		public B9DbMigration Migration;

		/// <summary>
		/// Optional
		/// </summary>
		public string Name;


		public IndexMigration()
		{
		}

		public IndexMigration(B9DbMigration migration, string table, string[] columns, string name = null)
		{
			Migration = migration;
			Table = table;
			Columns = columns;
			Name = name;
		}

		public void Up()
		{
			init();
			Migration.CreateIndex_(Table, Columns, false, Name);
		}

		public void Down()
		{
			init();
			Migration.DropIndex_(Table, Name);
		}

		protected void init()
		{
			if (Name == null)
				Name = "IX_" + Table + "_" + String.Join("_", Columns);
		}



		public static void UpAll(IEnumerable<IndexMigration> migrations)
		{
			foreach (var migration in migrations)
				migration.Up();
		}

		public static void DownAll(IEnumerable<IndexMigration> migrations)
		{
			foreach (var migration in migrations)
				migration.Down();
		}
	}
}
