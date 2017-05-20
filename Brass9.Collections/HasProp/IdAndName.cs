using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brass9.Collections.HasProp
{
	/// <summary>
	/// Just a useful, lazy class
	/// </summary>
	public class IdAndName : IIdAndName
	{
		public int Id { get; set; }
		public string Name { get; set; }
	}
}
