using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brass9.Data.Entity.B9EF
{
	/// <summary>
	/// Marker attribute. Forces a subclass that inherits an int Id property to be treated as the PK, with
	/// Identity Insert turned on.
	/// </summary>
	public class ForcePKIdAttribute : Attribute
	{
	}
}
