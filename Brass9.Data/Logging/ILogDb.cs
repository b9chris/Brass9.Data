using System;
using System.Collections.Generic;
using System.Data.Entity;


namespace Brass9.Data.Logging
{
	public interface ILogDb
	{
		DbSet<LogLine> LogLines { get; set; }
	}
}
