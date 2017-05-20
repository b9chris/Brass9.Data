using System;
using System.Collections.Generic;
using System.Linq;


namespace Brass9.Data
{
	/// <summary>
	/// Generic holders for data; useful as replacements for Anonymous Classes in Select expressions, like:
	/// .Select(s => new Bag<int, string> { P1 = s.Id, P2 = s.Name })
	/// </summary>
	public class Bag<T1>
	{
		public T1 P1 { get; set; }
	}

	/// <summary>
	/// Generic holders for data; useful as replacements for Anonymous Classes in Select expressions, like:
	/// .Select(s => new Bag<int, string> { P1 = s.Id, P2 = s.Name })
	/// </summary>
	public class Bag<T1, T2>
	{
		public T1 P1 { get; set; }
		public T2 P2 { get; set; }
	}
}
