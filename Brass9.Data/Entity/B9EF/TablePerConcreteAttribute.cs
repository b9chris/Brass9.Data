using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brass9.Data.Entity.B9EF
{
	/// <summary>
	/// Marker attribute. Tells B9 EF Extensions to force this class to be Table-Per-Concrete Type
	/// </summary>
	public class TablePerConcreteAttribute : Attribute
	{
	}
}
