using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brass9
{
	public interface ITaggedLog : ILog
	{
		Task WriteLineAsync(string line, params string[] tags);
	}
}
