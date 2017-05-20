using System;
using System.Collections.Generic;
using System.Linq;


namespace Brass9.Data.Entity
{
	public class EFConfig
	{
		public bool AutoDetectChangesEnabled { get; set; }
		public bool LazyLoadingEnabled { get; set; }
		public bool ProxyCreationEnabled { get; set; }
		public bool ValidateOnSaveEnabled { get; set; }



		public static EFConfig CheapReads
		{
			get
			{
				return new EFConfig
				{
					AutoDetectChangesEnabled = false,
					ProxyCreationEnabled = false,
				};
			}
		}

		public static EFConfig CheapWrites
		{
			get
			{
				return new EFConfig
				{
					// http://stackoverflow.com/questions/5940225/fastest-way-of-inserting-in-entity-framework
					AutoDetectChangesEnabled = false,
					ValidateOnSaveEnabled = false,
				};
			}
		}
	}
}
