using System;


namespace Brass9
{
	/// <summary>
	/// Like ThreadStart and MethodInvoker - just a parameterless delegate that returns void.
	/// Frequently useful.
	/// </summary>
	public delegate void VoidDelegate();

	public class SystemExt
	{
		public static bool IsNullOrDefaultValue(object o)
		{
			// For nullable types - simple
			if (o == null)
				return true;

			if (!o.GetType().IsValueType)
				return false;

			// For value types - .Net makes this unnecessarily hard
			object defaultValue = Activator.CreateInstance(o.GetType());
			return o.Equals(defaultValue);
		}

		/// <summary>
		/// Returns a string from the passed-in value's underlying type:
		/// If the underlying type is:
		/// Nullable and the value is null, returns null.
		/// DateTime and the value is MinValue, returns null.
		/// A ValueType, and set to its default value,
		/// returns empty string. TODO - only handles DateTimes, all other ValueTypes are handled via default ToString() behavior.
		/// Otherwise returns the value of default .ToString()
		/// </summary>
		/// <param name="o"></param>
		/// <returns></returns>
		public static string ToStringExtended(object o)
		{
			if (o == null || o is DBNull)
				return null;

			if (o is DateTime)
			{
				DateTime d = (DateTime)o;
				if (d == DateTime.MinValue)
					return null;

				return d.ToShortDateString();
			}

			return o.ToString();
		}

		/// <summary>
		/// Returns a string from the passed-in value's underlying type:
		/// If the underlying type is Nullable and the value is null,
		/// returns empty string.
		/// If the underlying type is a ValueType, and set to its default value,
		/// returns empty string.
		/// Otherwise returns the value of .ToString()
		/// </summary>
		/// <param name="o"></param>
		/// <returns></returns>
		public static string ToStringNoNull(object o)
		{
			var v = ToStringExtended(o);
			if (v == null)
				return "";

			return v;
		}

		/// <summary>
		/// Converts null to false, false to false, true to true
		/// Does not convert string "false" to false! Use bool.Parse for that
		/// </summary>
		public static bool ToBool(object o)
		{
			if (IsNullOrDefaultValue(o))
				return false;

			return (bool)o;
		}


		protected const int HASH32MASK = 0x7FFFFFFF;

		/// <summary>
		/// Hashes a string of any length into a single 32-bit int. Uses sdbm algorithm from here:
		/// http://blade.nagaokaut.ac.jp/cgi-bin/scat.rb/ruby/ruby-talk/142054
		/// 
		/// Fast, and good for avoiding collisions for small piles of data - likely piles smaller than 2^16 = ~ 65k
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public static int Hash32(string s)
		{
			int hash = 0;
			int l = s.Length;
			for (int i = 0; i < l; i++)
			{
				hash = s[i] + (hash << 6) + (hash << 16) - hash;
			}
			return hash & HASH32MASK;
		}
	}
}
