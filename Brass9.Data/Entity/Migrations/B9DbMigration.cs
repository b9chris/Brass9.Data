using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.IO;
using System.Text.RegularExpressions;


namespace Brass9.Data.Entity.Migrations
{
	/// <summary>
	/// Inherit from this instead of the automatically inserted DbMigration to get additional Migrations commands.
	/// 
	/// http://stackoverflow.com/a/25347200/176877
	/// </summary>
	public abstract class B9DbMigration : System.Data.Entity.Migrations.DbMigration
	{
		/// <summary>
		/// Drops an index of the name specified, on the table specified.
		/// 
		/// The built-in DropIndex command in Migrations interprets your command in a variety of ways that can translate into
		/// multiple SQL statements, or dropping an index with an expanded name based on Conventions. This command cuts past those
		/// interpretations and drops exactly what you requested.
		/// 
		/// Useful on existing databases where indices pre-existed, or where indices have been created by an outside tool like
		/// Sql Server Missing Indexes.
		/// </summary>
		public void DropIndexExact(string table, string indexName)
		{
			Sql("drop index [" + indexName + "] on [" + table + "]");
		}

		public void DropConstraint(string table, string constraintName)
		{
			Sql("alter table [" + table + "] drop constraint [" + constraintName + "]");
		}

		/// <summary>
		/// Drop a Primary Key by name. See DropIndexExact for an explanation of how Exact calls differ from EF Migrations built-ins.
		/// </summary>
		public void DropPrimaryKeyExact(string table, string pkName)
		{
			DropConstraint(table, pkName);
		}

		/// <summary>
		/// Drop a Foreign Key by name. See DropIndexExact for an explanation of how Exact calls differ from EF Migrations built-ins.
		/// </summary>
		public void DropForeignKeyExact(string table, string fkName)
		{
			DropConstraint(table, fkName);
		}

		/// <summary>
		/// Reads in a .sql file filled with SQL statements and emits them as part of a Migration's data changes ("data motion").
		/// 
		/// Usage: SqlFile(@"Migrations\Sql\2013-08-15 FillTable.sql");
		/// 
		/// Note: All statements will presumably operate on specific table names, which will need to exist in this database
		/// already to work properly.
		/// 
		/// Note: Any error will cause the entire Migration to fail, as EF Migrations always do.
		/// 
		/// Note: Many statements common in SQL scripts are illegal in Migrations commands, especially "GO".
		/// To compensate for this, this call tries to filter out common problem statements - but it might be wise to clean out
		/// your sql script beforehand. Or, just run it and see what happens - the migration always runs in a Transaction so if it
		/// fails no harm done.
		/// </summary>
		/// <param name="path">The path to the .sql script relative to the Project that contains this Migration.</param>
		public void SqlFile(string path)
		{
			var cleanAppDir = new Regex(@"\\bin.+");
			var dir = AppDomain.CurrentDomain.BaseDirectory;
			dir = cleanAppDir.Replace(dir, "") + @"\";
			var sql = File.ReadAllLines(dir + path);

			string[] ignore = new string[]
			{
				"GO",	// Migrations doesn't support GO
				"/*",	// Migrations might not support comments
				"print"	// Migrations might not support print
			};

			foreach (var line in sql)
			{
				if (ignore.Any(ig => line.StartsWith(ig)))
					continue;	

				Sql(line);
			}
		}

		// For external calls, since DbMigration hides all its important methods in protected
		public void Sql_(string sql)
		{
			Sql(sql);
		}

		public void CreateIndex_(string table, string column, bool unique = false, string name = null, bool clustered = false)
		{
			CreateIndex(table, column, unique, name, clustered);
		}
		public void CreateIndex_(string table, string[] columns, bool unique = false, string name = null, bool clustered = false)
		{
			CreateIndex(table, columns, unique, name, clustered);
		}

		public void DropIndex_(string table, string name)
		{
			DropIndex(table, name);
		}
	}
}
