using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brass9.Data.Entity;

namespace Brass9.Data.Logging.Tagged
{
	public class Log<TDb> : ILog
		where TDb : DbContext, ITaggedLogDb, new()
	{
		protected BaseDbFactory<TDb> dbFactory;

		public Log()
			: this(new BaseDbFactory<TDb>())
		{
		}

		public Log(BaseDbFactory<TDb> dbFactory)
		{
			this.dbFactory = dbFactory;
		}

		public void WriteLineBg(string line, params string[] tags)
		{
			Task.Run(async () =>
			{
				await WriteLineAsync(line, tags);
			}).ConfigureAwait(false);
		}

		public async Task WriteLineAsync(string line)
		{
			await WriteLineAsync(line, new string[0]);
		}

		public async Task WriteLineAsync(string line, params string[] tags)
		{
			if (line != null && line.Length > 2000)
				line = line.Substring(0, 2000);

			using (var db = dbFactory.CheapWrites())
			{
				var logLine = new TaggedLogLine(line, tags);
				db.LogLines.Add(logLine);
				await db.SaveChangesAsync();
			}
		}

		public void LogExceptionBg(Exception ex, string message = null, params string[] tags)
		{
			Task.Run(async () =>
			{
				string s = "";
				if (message != null)
					s += message + "\n";

				s += ExceptionHelper.FullExceptionString(ex);
				if (tags == null)
					tags = new[] { "error" };
				else
					tags = new[] { "error" }.Concat(tags).ToArray();

				await WriteLineAsync(s, tags);
			}).ConfigureAwait(false);
		}

		public async Task LogExceptionAsync(Exception ex)
		{
			await WriteLineAsync(ExceptionHelper.FullExceptionString(ex), "error");
		}

		public async Task LogShortExceptionAsync(Exception ex)
		{
			await WriteLineAsync(ExceptionHelper.ShortExceptionString(ex), "error");
		}
	}
}
