using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Brass9
{
	public class ExceptionHelper
	{
		public static string FullExceptionString(Exception ex, int maxLength = 0)
		{
			StringBuilder sb;
			if (maxLength == 0)
				sb = new StringBuilder();
			else
				sb = new StringBuilder(maxLength + 1000);

			AppendFullExceptionString(ex, sb, maxLength);
			return sb.ToString();
		}

		public static void AppendFullExceptionString(Exception ex, StringBuilder sb, int maxLength = 0)
		{
			if (maxLength == 0)
				maxLength = 200000;	// Limit to 200000 - arbitrary but we've gotta stop somewhere.

			while (ex != null && sb.Length < maxLength)
			{
				AppendExceptionString(ex, sb);
				ex = ex.InnerException;
			}

			if (sb.Length > maxLength)
				sb.Length = maxLength;
			// This, surprisingly, is the correct way to truncate a StringBuilder.
			// http://stackoverflow.com/a/5701216/176877
		}

		public static void AppendExceptionString(Exception ex, StringBuilder sb)
		{
			string exType = ex.GetType().Name;

			if (ex.Message.Contains("See the inner exception for details."))
				return;	// Filter out/don't bother with useless wrapper exceptions

			sb.Append(exType).Append(": ")
				.AppendLine(ex.Message)
				// TODO: Make this class configurable, decide this filter via dependency injection
				.AppendLine(FilterFrameworkBoilerplate(ex.StackTrace));
		}


		/// <summary>
		/// Return at most 30 chars from the Exception message. Don't bother looping through nested Exceptions.
		/// </summary>
		/// <param name="ex"></param>
		/// <returns></returns>
		public static string ShortExceptionString(Exception ex)
		{
			var sb = new StringBuilder();
			AppendShortExceptionString(ex, sb);
			return sb.ToString();
		}

		/// <summary>
		/// Append at most 30 chars from the Exception message. Don't bother looping through nested Exceptions.
		/// </summary>
		/// <param name="ex"></param>
		/// <param name="sb"></param>
		public static void AppendShortExceptionString(Exception ex, StringBuilder sb)
		{
			sb.Append(ex.GetType().Name)
				.Append(": ");

			if (!String.IsNullOrEmpty(ex.Message))
			{
				if (ex.Message.Length < 30)
					sb.Append(ex.Message);
				else
					sb.Append(ex.Message.Substring(0, 30));
			}
		}

		/// <summary>
		/// Filters stacktraces return by Exceptions to minimize character waste on repeated namespaces of noisy
		/// framework components like Task and WCF
		/// </summary>
		/// <param name="stackTrace"></param>
		/// <returns></returns>
		public static string FilterFrameworkBoilerplate(string stackTrace)
		{
			// TODO: Make this more performant, it wastes a ton of string-memory.
			// Why aren't there "This is useless throw this away" strings in managed frameworks?
			var lines = stackTrace.Replace("\r", "").Split('\n');

			filterBoilerplateInLines(lines);
			lines = mergeLines(lines);

			return String.Join("\n", lines);
		}

		/// <summary>
		/// Operates directly on the array - returns nothing
		/// </summary>
		protected static void filterBoilerplateInLines(string[] lines)
		{
			for(int i = 0; i < lines.Length; i++)
			{
				string line = lines[i];
				lines[i] = filterBoilerplateLine(line);
			}
		}

		protected static string filterBoilerplateLine(string line)
		{
			if (line.Contains("System.Runtime.CompilerServices.TaskAwaiter"))
				return "task";

			if (line.Contains("System.ServiceModel.Channels.ServiceChannel"))
				return "wcf";

			if (line.Contains("--- End of stack trace from previous location where exception was thrown ---"))
				return "---";

			return line;
		}

		protected static string[] mergeLines(string[] lines)
		{
			var mergeable = new LinkedList<string>(lines);

			var prevNode = mergeable.First;
			var nextNode = prevNode.Next;
			while (nextNode != null)
			{
				if (String.Equals(prevNode.Value, nextNode.Value))
				{
					// Eat away duplicate lines
					mergeable.Remove(nextNode);
					nextNode = prevNode.Next;
				}
				else
				{
					// If no dupe, move on to the next line pair
					prevNode = nextNode;
					nextNode = nextNode.Next;
				}
			}

			return mergeable.ToArray();
		}
	}
}
