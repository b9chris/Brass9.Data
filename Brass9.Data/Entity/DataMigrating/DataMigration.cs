using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brass9.Data.Entity.DataMigrating
{
	public abstract class DataMigration<TDb>
		where TDb : DbContext
	{
		public abstract DateTime DateTime { get; }

		public abstract Task MigrateAsync(TDb db);
	}
}
