using System;
using System.Data;
using System.Transactions;
using System.Linq;
using System.Text;


namespace Brass9.Data
{
	/// <summary>
	/// General Sql/Data helpers
	/// </summary>
	public static class DataHelper
	{
		/// <summary>
		/// Works around the fact that an object from a DB might be null, DBNull, a string, or something else
		/// 
		/// Coalesces DBNull and null to just plain null.
		/// 
		/// Strings returned as strings.
		/// </summary>
		public static string ToString(object value)
		{
			if (value == null || value is DBNull)
				return null;

			return (string)value;
		}

		public static decimal ToDecimal(object value)
		{
			if (value == null || value is DBNull)
				return 0;

			return Convert.ToDecimal(value);
		}

		public static float ToFloat(object value)
		{
			if (value == null || value is DBNull)
				return 0;

			return Convert.ToSingle(value);
		}

		public static int ToInt(object value)
		{
			if (value == null || value is DBNull)
				return 0;

			return Convert.ToInt32(value);
		}


		/// <summary>
		/// Warning: Do not use this with EF6, especially async. It appears to make things implode.
		/// 
		/// Use this style instead:
		/// http://msdn.microsoft.com/en-us/data/dn456843#several
		/// </summary>
		/// <returns></returns>
		public static TransactionScope SqlServerTransaction()
		{
			var transaction = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions {
				IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted,
				Timeout = TimeSpan.MaxValue
			}, EnterpriseServicesInteropOption.Automatic);

			return transaction;
		}
	}
}