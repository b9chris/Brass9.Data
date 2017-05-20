using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brass9.Data.Entity
{
	/// <summary>
	/// For compile-safe use in ColumnAttribute(TypeName=...), like:
	/// 
	/// [Column(TypeName=SqlDataType.VarChar)]
	/// instead of
	/// [Column(TypeName=SqlDataType.VarChar)]
	/// </summary>
	public class SqlDataType
	{
		public const string VarChar = "VarChar";
		public const string Char = "Char";
	}
}
