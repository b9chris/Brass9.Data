using System;
using System.Collections.Generic;


namespace Brass9.Reflection.Enums
{
	public abstract class EnumStringValueMap<TSubclass, TEnum>
		where TEnum : struct where TSubclass : new()
	{
		#region Singleton
		// http://codereview.stackexchange.com/questions/79/implementing-a-singleton-pattern-in-c
		public static TSubclass Current { get { return Nested.instance; } }

		class Nested
		{
			static Nested()
			{
			}

			internal static readonly TSubclass instance = new TSubclass();
		}
		#endregion

		protected Dictionary<TEnum, string> enumToString = new Dictionary<TEnum, string>();
		protected Dictionary<string, TEnum> stringToEnum = new Dictionary<string, TEnum>();

		protected EnumStringValueMap()
		{
			StringValueAttribute.Map<TEnum>(enumToString, stringToEnum);
		}

		public string EnumToString(TEnum value)
		{
			return enumToString[value];
		}

		public TEnum StringToEnum(string s)
		{
			return stringToEnum[s];
		}
	}
}
