using System;
using System.Collections.Generic;
using System.Linq;

using System.Reflection;


namespace Brass9.Data
{
	public class LabelAttribute : Attribute
	{
		public string Label { get; set; }

		public LabelAttribute()
		{
		}

		public LabelAttribute(string label)
		{
			Label = label;
		}



		public static string For<T>(T enumValue)
			where T : struct
		{
			var attr = typeof(T).GetField(enumValue.ToString()).GetCustomAttribute<LabelAttribute>();
			return attr.Label;
		}
	}
}
