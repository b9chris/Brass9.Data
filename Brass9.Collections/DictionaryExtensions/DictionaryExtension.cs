using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Brass9.Collections.DictionaryExtensions
{
	public static class DictionaryExtension
	{
		public static Dictionary<TKey, TValue> Merge<TKey, TValue>(this Dictionary<TKey, TValue> _this, Dictionary<TKey, TValue> dict)
		{
			// From http://stackoverflow.com/questions/294138/merging-dictionaries-in-c-sharp
			return _this.Concat(dict).ToDictionary(n => n.Key, n => n.Value);
		}
	}
}
