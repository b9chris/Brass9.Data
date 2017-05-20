using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brass9.Collections
{
	public static class Lists
	{
		/// <summary>
		/// Returns an array with one more, blank item appended to the end. Useful for Views with lists that need a blank
		/// entry.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="items"></param>
		/// <returns></returns>
		public static T[] OneMore<T>(IEnumerable<T> items)
			where T : new()
		{
			var newItemArray = new[] { new T() };
			if (items == null)
				return newItemArray;

			return items.Concat(newItemArray).ToArray();
		}

		public static IEnumerable<T> NoNulls<T>(IEnumerable<T> items)
		{
			if (items == null)
				return new T[0];

			return items;
		}
	}
}
