using System;
using System.Collections.Generic;
using System.Linq;


namespace Brass9.Data.Linq
{
	/// <summary>
	/// Helper methods for ICollections
	/// </summary>
	public static class CollectionHelper
	{
		public static void RemoveItems<T>(ICollection<T> icollection, IEnumerable<T> removes)
		{
			foreach (var remove in removes)
				icollection.Remove(remove);
		}

		public static void RemoveWhere<T>(ICollection<T> icollection, Func<T, bool> where)
		{
			CollectionHelper.RemoveItems(icollection, icollection.Where(where).ToArray());
		}

		public static void AddAll<T>(ICollection<T> icollection, IEnumerable<T> adds)
		{
			foreach (var add in adds)
				icollection.Add(add);
		}

		public static void AddDiff<T>(ICollection<T> icollection, IEnumerable<T> adds)
		{
			CollectionHelper.AddAll(icollection, adds.Except(icollection));
		}
	}
}
