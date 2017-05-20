using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Brass9;


namespace Brass9.Collections
{
	public static class DictionaryHelper
	{
		/// <summary>
		/// Converts a string-keyed Dictionary to an enum-keyed dictionary
		/// </summary>
		/// <typeparam name="TKey">An enum</typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="dict"></param>
		/// <returns></returns>
		public static Dictionary<TKey, TValue> StringKeysToEnumKeys<TKey, TValue>(Dictionary<string, TValue> dict) where TKey : struct, IConvertible
		{
			var dictOut = new Dictionary<TKey, TValue>(dict.Count);
			foreach (string key in dict.Keys)
			{
				dictOut.Add(EnumHelper.Parse<TKey>(key), dict[key]);
			}
			return dictOut;
		}

		public static Dictionary<string, TValue> EnumKeysToStringKeys<TKey, TValue>(Dictionary<TKey, TValue> dict)
		{
			var tkey = typeof(TKey);
			var dictOut = dict.Select(kv => new KeyValuePair<string, TValue>(Enum.GetName(tkey, kv.Key), kv.Value)).ToDictionary(kv => kv.Key, kv => kv.Value);
			return dictOut;
		}
	}
}
