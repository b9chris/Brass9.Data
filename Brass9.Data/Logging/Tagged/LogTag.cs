using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Brass9.Data.Logging.Tagged
{
	public class LogTag
	{
		public int Id { get; set; }

		public TaggedLogLine Line { get; set; }
		[ForeignKey("Line")]
		public int LogId { get; set; }

		[MaxLength(30)]
		public string Tag { get; set; }
	}
}
