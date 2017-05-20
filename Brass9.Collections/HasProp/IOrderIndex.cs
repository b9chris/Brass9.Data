using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brass9.Collections.HasProp
{
	/// <summary>
	/// Marks a class as having an OrderIndex property, that indicates its order relative to either all other objects
	/// like it, or some subset.
	/// 
	/// In a db that's the entire table, or its peers, usually based on a filter property like a Foreign Key Id
	/// </summary>
	public interface IOrderIndex
	{
		int OrderIndex { get; set; }
	}
}
