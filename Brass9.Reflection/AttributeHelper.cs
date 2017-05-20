using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;


namespace Brass9.Reflection
{
	public class AttributeHelper
	{
		/// <summary>
		/// Finds all classes with the TAttribute applied in all loaded Assemblies.
		/// From:
		/// http://stackoverflow.com/questions/607178/c-sharp-how-enumerate-all-classes-with-custom-class-attribute
		/// </summary>
		/// <typeparam name="TAttribute">The Attribute to search for</typeparam>
		/// <returns>All loaded classes with TAttribute applied</returns>
		public static IEnumerable<Type> GetTypesWithAttribute<TAttribute>()
			where TAttribute : Attribute
		{
			foreach (var assembly in ReflectionHelper.GetAllAssemblies())
			{
				foreach (Type type in assembly.GetTypes())
				{
					if (type.GetCustomAttributes(typeof(TAttribute), true).Length > 0)
					{
						yield return type;
					}
				}
			}
		}

		/// <summary>
		/// Finds all classes with the TAttribute applied in the given Assembly.
		/// </summary>
		/// <typeparam name="TAttribute">The Attribute to search for</typeparam>
		/// <param name="assembly">The Assembly/Project to search</param>
		/// <returns>Classes with TAttribute applied</returns>
		public static IEnumerable<Type> GetTypesWithAttribute<TAttribute>(Assembly assembly)
			where TAttribute : Attribute
		{
			foreach (Type type in assembly.GetTypes())
			{
				if (type.GetCustomAttributes(typeof(TAttribute), true).Length > 0)
				{
					yield return type;
				}
			}
		}


		public static void ForAllTypesWithAttribute<TAttribute>(Action<Type> action, string exMessage, Assembly assembly = null)
			where TAttribute : Attribute
		{
			IEnumerable<Type> types;
			if (assembly == null)
				types = AttributeHelper.GetTypesWithAttribute<TAttribute>();
			else
				types = AttributeHelper.GetTypesWithAttribute<TAttribute>(assembly);

			foreach (var type in types)
			{
#if DEBUG
				try
				{
#endif
					action(type);
#if DEBUG
				}
				catch (Exception ex)
				{
					// "Brass9.Web.IoC.AttributeHelper: Cannot initialize singleton class - are you missing a method?"
					throw new Exception(exMessage, ex);
				}
#endif
			}
		}

		public static AttributeHelper<TModel, TProp> ForProp<TModel, TProp>(Expression<Func<TModel, TProp>> propAccessor)
		{
			return new AttributeHelper<TModel, TProp>(propAccessor);
		}
	}

	public class AttributeHelper<TModel, TProp>
	{
		protected Expression<Func<TModel, TProp>> propAccessor;

		public AttributeHelper(Expression<Func<TModel, TProp>> propAccessor)
		{
			this.propAccessor = propAccessor;
		}

		public T GetAttribute<T>()
			where T : System.Attribute
		{
			var prop = ReflectionHelper.GetPropertyInfo<TModel, TProp>(propAccessor);
			var attr = prop.GetCustomAttribute<T>();
			return attr;
		}

		public System.Attribute[] GetAttributes()
		{
			var prop = ReflectionHelper.GetPropertyInfo<TModel, TProp>(propAccessor);
			var attrs = prop.GetCustomAttributes().ToArray();
			return attrs;
		}
	}
}