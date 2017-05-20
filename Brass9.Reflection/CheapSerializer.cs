using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;



namespace Brass9.Reflection
{
	public static class CheapSerializer
	{
		/// <summary>
		/// Serializes a shallow copy of an object to the passed-in StringBuilder
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="sb"></param>
		public static void SerializeToLines(object obj, StringBuilder sb)
		{
			var props = ReflectionHelper.GetPublicProperties(obj.GetType());
			foreach (var prop in props)
			{
				sb.AppendLine(prop.Name + ": " + prop.GetValue(obj, null));
			}
		}

		/// <summary>
		/// Serializes a shallow copy of an object to the passed-in StringBuilder
		/// Does not serialize properties in the ignore list
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="sb"></param>
		/// <param name="ignore"></param>
		public static void SerializeToLines(object obj, StringBuilder sb, IEnumerable<string> ignoreProps)
		{
			var props = ReflectionHelper.GetPublicProperties(obj.GetType()).Where(p => !ignoreProps.Contains(p.Name)).ToArray();
			foreach (var prop in props)
			{
				sb.AppendLine(prop.Name + ": " + prop.GetValue(obj, null));
			}
		}
	}
}
