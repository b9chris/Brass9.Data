using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Brass9.Reflection;
//using Brass9.Reflection.GetMethodExtensions;


namespace Brass9.Data
{
	public static class ModelTransform
	{
		/// <summary>
		/// Copies values from "from" to "to" for each matching property name
		/// </summary>
		/// <param name="from"></param>
		/// <param name="to"></param>
		public static void Copy(object from, object to)
		{
			var fromType = from.GetType();
			var toType = to.GetType();

			Copy(from, fromType, to, toType);
		}
		public static void Copy(object from, object to, string[] skipProperties)
		{
			var fromType = from.GetType();
			var toType = to.GetType();

			Copy(from, fromType, to, toType, skipProperties);
		}

		public static void Copy(object from, object to, Type toType)
		{
			var fromType = from.GetType();
			Copy(from, fromType, to, toType);
		}
		
		// TODO: Consider making the list of properties to skip be mapped like t => t.MyProp so they survive property renaming
		// Would be a Func<TFrom, object>[] presumably? Not sure of second type in specification. Need to fetch prop name from it - must be examples online.
		// Might also be smart to just include individual translations in the list, so the translators both handle the properties and tell the copier
		// to ignore them, but this would eliminate functionality where you just want to ignore some, split others, etc - should be additional for
		// convenience if added.
		public static void Copy(object from, object to, Type toType, string[] skipProperties)
		{
			var fromType = from.GetType();
			Copy(from, fromType, to, toType, skipProperties);
		}

		public static void Copy(object from, Type fromType, object to, Type toType)
		{
			Copy(from, fromType, to, toType, null);
		}

		public static void Copy(object from, Type fromType, object to, Type toType, string[] skipProperties)
		{
			IEnumerable<PropertyInfo> fromProps;
			if (skipProperties == null)
				fromProps = ReflectionHelper.GetPublicProperties(fromType);
			else
				fromProps = ReflectionHelper.GetPublicProperties(fromType, skipProperties);

			foreach (var fromProp in fromProps)
			{
				var toProp = toType.GetProperty(fromProp.Name);
				if (toProp != null && toProp.GetSetMethod() != null)
					toProp.SetValue(to, fromProp.GetValue(from, null), null);
			}
		}

		/// <summary>
		/// Complex Copy operation between 2 objects.
		/// 
		/// 
		/// </summary>
		/// <param name="from">Object to copy from</param>
		/// <param name="fromType">Type of object to copy from</param>
		/// <param name="to">Object to copy to</param>
		/// <param name="toType">Type of object to copy to</param>
		/// <param name="where">An optional filter</param>
		/// <param name="copiedValue"></param>
		public static void Copy(object from, Type fromType, object to, Type toType, Func<PropertyInfo, bool> where, Func<string, object, object> copiedValue)
		{
			IEnumerable<PropertyInfo> fromProps;
			if (where == null)
				fromProps = ReflectionHelper.GetPublicProperties(fromType);
			else
				fromProps = ReflectionHelper.GetPublicProperties(fromType).Where(where);

			foreach (var fromProp in fromProps)
			{
				var toProp = toType.GetProperty(fromProp.Name);
				if (toProp != null && toProp.GetSetMethod() != null)
				{
					object fromValue = fromProp.GetValue(from, null);
					object toValue = copiedValue(fromProp.Name, fromValue);
					toProp.SetValue(to, toValue, null);
				}
			}
		}

		public static void Copy(object from, object to, Type toType, Dictionary<Type, Type> typeMap)
		{
			Copy(from, from.GetType(), to, toType, null, typeMap);
		}
		public static void Copy(object from, Type fromType, object to, Type toType, Func<PropertyInfo, bool> where, Dictionary<Type, Type> typeMap
			//, Func<string, object, object> copiedValue
			)
		{
			IEnumerable<PropertyInfo> fromProps;
			if (where == null)
				fromProps = ReflectionHelper.GetPublicProperties(fromType);
			else
				fromProps = ReflectionHelper.GetPublicProperties(fromType).Where(where);

			foreach (var fromProp in fromProps)
			{
				var toProp = toType.GetProperty(fromProp.Name);
				if (toProp == null && toProp.GetSetMethod() != null)
				{
					object fromValue = fromProp.GetValue(from, null);

					if (fromValue is System.Collections.IEnumerable)
					{
						// Copy lists
						var listTo = ReflectionHelper.GetIEnumerableType(fromValue.GetType());
						//typeof(ModelTransform).GetMethodExt(
						var fn = new Func<IEnumerable<object>, Dictionary<Type, Type>, IEnumerable<object>>(ModelTransform.CopyList<object>);
						var copyListMethod = fn.GetMethodInfo().MakeGenericMethod(listTo);
						copyListMethod.Invoke(null, new object[] { fromValue, typeMap });
					}
					else
					{
						// Copy values and objects

						//object toValue = copiedValue(fromProp.Name, fromValue);
						//toProp.SetValue(to, toValue, null);
						toProp.SetValue(to, fromValue, null);
					}
				}
			}
		}

		/// <summary>
		/// Copies between 2 objects, not necessarily of the same class/Type, using reflection
		/// to copy properties of matching name and Type.
		/// </summary>
		/// <typeparam name="TTo"></typeparam>
		/// <param name="from"></param>
		/// <returns></returns>
		public static TTo New<TTo>(object from)
			where TTo : new()
		{
			var to = new TTo();
			Copy(from, to, typeof(TTo));
			return to;
		}

		public static TTo New<TTo>(object from, Dictionary<Type, Type> typeMap)
			where TTo : new()
		{
			var to = new TTo();
			Copy(from, to, typeof(TTo), typeMap);
			return to;
		}

		public static TTo New<TTo>(object from, string[] skipProperties)
			where TTo : new()
		{
			var to = new TTo();
			Copy(from, to, typeof(TTo), skipProperties);
			return to;
		}

		/// <summary>
		/// Clones an object to a new one of the same Type, via Reflection, including
		/// reflecting the Type.
		/// </summary>
		/// <param name="from"></param>
		/// <returns></returns>
		public static object Clone(object from)
		{
			var type = from.GetType();
			var method = typeof(ModelTransform).GetMethods(BindingFlags.Public | BindingFlags.Static).First(m => m.Name == "New")
				.MakeGenericMethod(type);
			var to = method.Invoke(null, new object[] { from });
			return to;
		}


		/// <summary>
		/// Translates an IEnumerable of one type to a strongly typed IEnumerable of another
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="from"></param>
		/// <returns></returns>
		public static IEnumerable<TTo> CopyList<TTo>(IEnumerable<object> from)
			where TTo : new()
		{
			var toType = typeof(TTo);

			var toList = new List<TTo>();
			foreach (var fromObj in from)
			{
				var fromType = fromObj.GetType();

				var to = new TTo();
				Copy(fromObj, fromType, to, toType);
				toList.Add(to);
			}

			return toList;
		}

		public static IEnumerable<TTo> CopyList<TFrom, TTo>(IEnumerable<TFrom> fromList, string[] skipProperties, Action<TFrom, TTo> translate)
			where TTo : new()
		{
			var toType = typeof(TTo);
			var fromType = typeof(TFrom);

			var toList = new List<TTo>();
			foreach (var from in fromList)
			{
				var to = new TTo();
				Copy(from, fromType, to, toType, skipProperties);
				translate(from, to);
				toList.Add(to);
			}

			return toList;
		}

		public static IEnumerable<TTo> CopyList<TTo>(IEnumerable<object> from, Dictionary<Type, Type> typeMap)
			where TTo : new()
		{
			var toType = typeof(TTo);

			var toList = new List<TTo>();
			foreach (var fromObj in from)
			{
				var fromType = fromObj.GetType();

				var to = new TTo();
				Copy(fromObj, fromType, to, toType, null, typeMap);
				toList.Add(to);
			}

			return toList;
		}


		public static void CopyCaseInsensitive(object from, object to)
		{
			var fromType = from.GetType();
			var toType = to.GetType();
			var fromProps = fromType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
			foreach (var fromProp in fromProps)
			{
				string propName = fromProp.Name;
				var toProp = toType.GetProperties().Where(p => String.Equals(propName, p.Name, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

				if (toProp != null && toProp.GetSetMethod() != null)
					toProp.SetValue(to, fromProp.GetValue(from, null), null);
			}
		}
	}
}
