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
	public class IndexAdvancedMigration
	{
		public string Table;

		public IndexColumn[] Columns;

		public B9DbMigration Migration;

		/// <summary>
		/// Optional
		/// </summary>
		public string Name;

		public string[] Include;


		public IndexAdvancedMigration()
		{
		}

		public void Up()
		{
			init();
			var sql = upSql();
			Migration.Sql_(sql);
		}

		protected string upSql()
		{
			// Build a string like
//@"create nonclustered index IX_IsPublished_OrderIndex
//on Project (IsPublished desc, OrderIndex asc)
//include [Key]"
			var sb = new StringBuilder();
			sb.Append("create nonclustered index [")
				.Append(Name)
				.Append("] on [")
				.Append(Table)
				.Append("] (")
				.Append(String.Join(", ", Columns
					.Select(col => "[" + col.Name + "] " + (col.IsAsc ? "asc" : "desc"))
				))
				.Append(")");

			if (Include != null && Include.Length > 0)
			{
				sb.Append(" include (")
					.Append(String.Join(", ", Include.Select(c => "[" + c + "]")))
					.Append(")");
			}

			return sb.ToString();
		}

		public void Down()
		{
			init();
			Migration.DropIndex_(Table, Name);
		}

		protected void init()
		{
			if (Name == null)
				Name = "IX_" + String.Join("_", Columns.Select(c => c.Name));
		}
	}
}
