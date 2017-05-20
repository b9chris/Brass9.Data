/*
Copyright 2012 Chris Moschini, Brass Nine Design

This code is licensed under the LGPL or MIT license, whichever you prefer.
*/

using System;
using System.Collections.Generic;
using System.Reflection;


namespace Brass9.Reflection.Enums
{
	/// <summary>
	/// Allows you to map a string as metadata to an enum item or property, like:
	/// 
	/// public enum TimeZoneId {
	///		[StringValue("Pacific Standard Time")]
	///		PST,
	///		. . .
	///	}
	///	
	/// You can then use StringValueAttribute.Map to get a 2 way dictionary to get these string values out easily.
	/// 
	/// TODO: Add simpler methods that get just a simple value out one at a time instead of making you build an entire dictionary.
	/// </summary>
	public class StringValueAttribute : Attribute
	{
		public string Value { get; protected set; }

		public StringValueAttribute(string value)
		{
			Value = value;
		}



		// TODO: Could be public, but since enum isn't really a type we have no type safety here.
		public static string GetStringValue<T>(T enumValue)
			where T : struct
		{
			var type = enumValue.GetType();
			var fieldInfo = type.GetField(enumValue.ToString());
			var attr = fieldInfo.GetCustomAttribute<StringValueAttribute>();
			if (attr == null)
				throw new ArgumentException("Enum value " + type.Name + "." + enumValue.ToString() + " does not have StringValueAttribute applied.");

			return attr.Value;
		}

		/// <summary>
		/// Call this to map custom strings to enum values and back
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="enumToString">An empty Dictionary to be filled with a map of enum values to the string value in the attribute applied to them</param>
		/// <param name="stringToEnum">An empty dictionary to be filled with a map of attribute string values to their enum values</param>
		public static void Map<T>(Dictionary<T, string> enumToString, Dictionary<string, T> stringToEnum)
			where T : struct
		{
			Type enumType = typeof(T);
			if (!enumType.IsEnum)
				throw new Exception("Type must be Enum.");

			var enumValues = (T[])Enum.GetValues(enumType);
			foreach (var enumVal in enumValues)
			{
				string stringVal = GetStringValue(enumVal);
				enumToString.Add(enumVal, stringVal);
				stringToEnum.Add(stringVal, enumVal);
			}
		}
	}
}
