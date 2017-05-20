using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brass9.Data.Entity
{
	public class EFOps
	{
		public static EFOps O { get; protected set; }

		public abstract async Task RemoveAsync<T>(int id);
	}

	public class EFOps<TDbFactory, TDb> : EFOps
		where TDbFactory : BaseDbFactory<TDb>
	{
		protected TDbFactory dbFactory;

		public EFOps(TDbFactory dbFactory)
		{
			this.dbFactory = dbFactory;
		}
	}
}
