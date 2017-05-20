using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Brass9.Data.Entity;


namespace Brass9.Data.Logging
{
	/// <summary>
	/// Provides basic logging
	/// 
	/// Use like:
	/// 
	/// public static class Log
	/// singleton nested Brass9.Data.Logging.Log TDb ...
	/// </summary>
	/// <typeparam name="TDb"></typeparam>
	public class Log<TDb> : ILog
		where TDb : DbContext, ILogDb, new()
	{
		public const int MaxLogLineLength = 2000;

		protected BaseDbFactory<TDb> dbFactory;

		public Log()
			: this(new BaseDbFactory<TDb>())
		{
		}

		public Log(BaseDbFactory<TDb> dbFactory)
		{
			this.dbFactory = dbFactory;
		}


		public async Task WriteLineAsync(string line)
		{
			if (line != null && line.Length > MaxLogLineLength)
				line = line.Substring(0, MaxLogLineLength);

			using (var db = dbFactory.CheapWrites())
			{
				var logLine = new LogLine(line);
				db.LogLines.Add(logLine);
				await db.SaveChangesAsync();
			}
		}

		public void WriteLine(string line)
		{
			if (line != null && line.Length > MaxLogLineLength)
				line = line.Substring(0, MaxLogLineLength);

			using (var db = dbFactory.CheapWrites())
			{
				var logLine = new LogLine(line);
				db.LogLines.Add(logLine);
				db.SaveChanges();
			}
		}

		public async Task LogExceptionAsync(Exception ex)
		{
			await WriteLineAsync(ExceptionHelper.FullExceptionString(ex));
		}

		public async Task LogShortExceptionAsync(Exception ex)
		{
			await WriteLineAsync(ExceptionHelper.ShortExceptionString(ex));
		}

		public void LogException(Exception ex)
		{
			WriteLine(ExceptionHelper.FullExceptionString(ex));
		}
	}
}
