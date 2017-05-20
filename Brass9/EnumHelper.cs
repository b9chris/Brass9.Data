using System;
using System.Collections.Generic;

namespace Brass9
{
	public static class EnumHelper
	{
		public static T Parse<T>(string name)
			where T : struct
		{
			return (T)Enum.Parse(typeof(T), name);
		}
	}
}
