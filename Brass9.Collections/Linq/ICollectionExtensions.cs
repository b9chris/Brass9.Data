using System;
using System.Collections.Generic;
using System.Linq;


namespace Brass9.Collections.Linq
{
	public static class ICollectionExtensions
	{
		public static void RemoveWhere<T>(this ICollection<T> collection, Func<T, bool> where)
		{
			var removes = collection.Where(where).ToArray();
			foreach (var remove in removes)
				collection.Remove(remove);
		}
	}
}
