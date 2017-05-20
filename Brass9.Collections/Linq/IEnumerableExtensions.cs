using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;


namespace Brass9.Collections.Linq
{
	public static class IEnumerableExtensions
	{
		/// <summary>
		/// Chooses an item in a list. If it exists, returns a given property of that item.
		/// If it doesn't exist, returns null, rather than throwing a NullReferenceException as naively accessing it would.
		/// 
		/// Example:
		/// ContactInfos.FirstOrDefault(i => i.Type == "Phone").Value;
		/// 
		/// This can throw an exception if there is no phone number in the db. Safe alternative:
		/// 
		/// ContactInfos.FirstProperty(i => i.Type == "Phone", i => i.Value);
		/// </summary>
		/// <typeparam name="TSource"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="list"></param>
		/// <param name="selector">A filter that selects for one item in this Enumerable - the one whose property you want the value of.</param>
		/// <param name="propSelector">The property you want to access on the selected item.</param>
		/// <returns>The selected property, or null if the item doesn't exist.</returns>
		public static TValue FirstProperty<TSource, TValue>(this IEnumerable<TSource> list, Func<TSource, bool> selector, Func<TSource, TValue> propSelector)
		{
			return list.Where(selector).Select(propSelector).FirstOrDefault();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="TSource1">Type of this list.</typeparam>
		/// <typeparam name="TSource2">Type of the sublist containing the item whose property you want to select.</typeparam>
		/// <typeparam name="TValue">Property type.</typeparam>
		/// <param name="list"></param>
		/// <param name="selector1">A filter that selects the item with a child list you want to select from.</param>
		/// <param name="selector1List">Access to the child list.</param>
		/// <param name="selector2">Filter the child list to one item.</param>
		/// <param name="propSelector">Access the property on the selected child item.</param>
		/// <returns>The selected property, or null if the item doesn't exist.</returns>
		public static TValue FirstPropOfFirstItem<TSource1, TSource2, TValue>(this IEnumerable<TSource1> list, Func<TSource1, bool> selector1, Func<TSource1, IEnumerable<TSource2>> selector1List, Func<TSource2, bool> selector2, Func<TSource2, TValue> propSelector)
		{
			return list.Where(selector1).SelectMany(selector1List).Where(selector2).Select(propSelector).FirstOrDefault();
		}



		public static HashSet<T> ToHashSet<T>(this IEnumerable<T> enumerable)
		{
			HashSet<T> map = new HashSet<T>();
			foreach (T item in enumerable)
				map.Add(item);

			return map;
		}

		public static HashSet<T> ToHashSetIgnoreNulls<T>(this IEnumerable<Nullable<T>> enumerable)
			where T : struct
		{
			HashSet<T> map = new HashSet<T>();
			foreach (Nullable<T> item in enumerable)
			{
				if (item.HasValue)
					map.Add(item.Value);
			}

			return map;
		}

		public static HashSet<TKey> ToHashSet<TSource, TKey>(this IEnumerable<TSource> enumerable, Func<TSource, TKey> keyMapFn)
		{
			HashSet<TKey> map = new HashSet<TKey>();
			foreach (TSource item in enumerable)
				map.Add(keyMapFn(item));

			return map;
		}

		public static Dictionary<TKey, TValue> ToDictionaryIgnoreDupesAndNulls<TKey, TValue>(this IEnumerable<TValue> enumerable, Func<TValue, TKey> keyMapFn)
		{
			Dictionary<TKey, TValue> dict = new Dictionary<TKey,TValue>();

			foreach (TValue item in enumerable)
			{
				if (item == null)
					continue;

				TKey key = keyMapFn(item);

				if (key == null || dict.ContainsKey(key))
					continue;

				dict.Add(key, item);
			}

			return dict;
		}

		/// <summary>
		/// Fence post foreach.
		/// 
		/// Runs the before action before calling the loop action each time, except the first time.
		/// </summary>
		/// <typeparam name="T">IEnumerable type</typeparam>
		/// <param name="enumerable">this</param>
		/// <param name="before">A call to make before loopBody is called for all items except the first.</param>
		/// <param name="loopBody">The body you would normally put in a foreach statement.</param>
		public static void FencePostBefore<T>(this IEnumerable<T> enumerable, Action<T> before, Action<T> loopBody)
		{
			var enumerator = enumerable.GetEnumerator();
			if (!enumerator.MoveNext())
				return;

			loopBody(enumerator.Current);

			while (enumerator.MoveNext())
			{
				before(enumerator.Current);
				loopBody(enumerator.Current);
			}
		}

		/// <summary>
		/// Naively merges 2 lists in order. Basically a cheaper .Join() that performs no comparisons.
		/// 
		/// Avoids the following construct:
		/// 
		/// var itemsFromDb1 = ...
		/// var itemsFromDb2 = ...
		/// 
		/// // Now operate on each item from each list, in order, as a pair
		/// for(int i = 0; i < itemsFromDb1.Count(); i++)
		/// {
		///   var item1 = itemsFromDb1[i];
		///   var item2 = itemsFromDb2[i];
		///   // Now we can finally do some work... ugly
		///   
		/// Instead use:
		/// 
		/// foreach(var item in itemsFromDb1.Merge(itemsFromDb2, (one, two) => new { Item1 = one, Item2 = two }))
		/// {
		/// ...
		/// </summary>
		/// <param name="select">A Select function that emits merged objects from the 2 inputs</param>
		public static IEnumerable<TSelect> Merge<T1, T2, TSelect>(this IEnumerable<T1> enumerable1,
			IEnumerable<T2> enumerable2, Func<T1, T2, TSelect> select)
		{
			var enumer2 = enumerable2.GetEnumerator();
			foreach(var item in enumerable1)
			{
				enumer2.MoveNext();
				yield return select(item, enumer2.Current);
			}
		}
		public static IEnumerable<TSelect> Merge<T1, T2, T3, TSelect>(this IEnumerable<T1> enumerable1,
			IEnumerable<T2> enumerable2, IEnumerable<T3> enumerable3, Func<T1, T2, T3, TSelect> select)
		{
			var enumer2 = enumerable2.GetEnumerator();
			var enumer3 = enumerable3.GetEnumerator();
			foreach(var item in enumerable1)
			{
				enumer2.MoveNext();
				enumer3.MoveNext();
				yield return select(item, enumer2.Current, enumer3.Current);
			}
		}
	}
}
