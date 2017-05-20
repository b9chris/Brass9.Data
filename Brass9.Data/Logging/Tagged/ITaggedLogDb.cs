using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brass9.Data.Logging.Tagged
{
	public interface ITaggedLogDb
	{
		DbSet<TaggedLogLine> LogLines { get; set; }
	}
}
