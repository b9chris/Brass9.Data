using System;
using System.Text;
using System.Reflection;
using System.Linq;

namespace Brass9.Reflection.Enums
{
	public static class EnumFullNames
	{
		// Needed for parsing full namespace enums...
		/*
		public static object ParseWithNamespace(string fullName)
		{
			// Determine the classname and value name
			// They may have passed a full namespace in, so we need the last 2 parts and nothing else.
			// TODO: If it is a full namespace there's probably a shortcut here.
			string[] parts = fullName.Split('.');
			string valName = parts[parts.Length - 1];
			string typeName = parts[parts.Length - 2];

			// Parse it with standard .Net Framework call
			Type type = enumTypes[typeName];
			object parsed = Enum.Parse(type, valName);
			return parsed;
		}
		*/

		/// <summary>
		/// Parses an enum that is in the calling method's Project
		/// Note that if the enum is outside (for example in X.Models, not X), this will throw an error.
		/// </summary>
		/// <param name="fullName"></param>
		/// <returns></returns>
		public static object Parse(string fullName)
		{
			// TODO: This definitely doesn't work in Production, only in Debug, presumably because of how the Assemblies/
			// modules happen to arrange themselves in the 2 different scenarios. Very shitty.
			// Come up with a better plan.

			string[] parts = fullName.Split('.');
			int lastIndex = parts.Length - 1;
			string typeName = parts[lastIndex - 1];

			var project = Assembly.GetCallingAssembly();
			var enumTypes = project.GetTypes()
				.Where(t => t.IsEnum)
				.ToDictionary(t => t.Name);

			if (!enumTypes.ContainsKey(typeName))
			{
				project = Assembly.GetExecutingAssembly();
				enumTypes = project.GetTypes()
					.Where(t => t.IsEnum)
					.ToDictionary(t => t.Name);

				if (!enumTypes.ContainsKey(typeName))
					throw new Exception("Type not found: " + typeName);
			}

			Type type = enumTypes[typeName];
			object parsed = Enum.Parse(type, parts[lastIndex]);
			return parsed;
		}

		public static TEnum Parse<TEnum>(string fullName)
			where TEnum : struct
		{
			string[] parts = fullName.Split('.');
			//var project = Assembly.GetCallingAssembly();
			//var enumTypes = project.GetTypes()
			//	.Where(t => t.IsEnum)
			//	.ToDictionary(t => t.Name);
			Type type = typeof(TEnum);
			TEnum parsed = (TEnum)Enum.Parse(type, parts[1]);
			return parsed;
		}


		public static string FullName<T>(T enumVal)
			where T : struct
		{
			return typeof(T).ToString() + '.' + enumVal.ToString();
		}

		/*
		/// <summary>
		/// Performs an Action (if any specified) based on the Enum Type and its Value
		/// </summary>
		/// <param name="fullName"></param>
		public static void Switch(string fullName)
		{
			// TODO - should we require they pass in a class? A Dictionary of string, Dict string, Actions? Other?
		}
		*/
	}
}
