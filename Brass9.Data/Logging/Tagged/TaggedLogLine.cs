using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brass9.Data.Logging.Tagged
{
	[Table("LogLine")]
	public class TaggedLogLine : Brass9.Data.Logging.LogLine
	{
		public ICollection<LogTag> Tags { get; set; }



		public TaggedLogLine() : base()
		{
		}

		public TaggedLogLine(string line, params string[] tags)
			: base(line)
		{
			Tags = tags.Select(tag => new LogTag { Tag = tag }).ToArray();
		}
	}
}
