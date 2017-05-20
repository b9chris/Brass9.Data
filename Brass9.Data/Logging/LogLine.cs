using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace Brass9.Data.Logging
{
	public class LogLine
	{
		public int Id { get; set; }

		// Avoid using "Timestamp", a reserved word in SQL/annoying to select/order by on
		public DateTime LogTime { get; set; }

		[MaxLength(2000)]
		public string Line { get; set; }



		public LogLine()
		{
		}

		public LogLine(string line)
		{
			Line = line;
			LogTime = DateTime.UtcNow;
		}
	}
}
