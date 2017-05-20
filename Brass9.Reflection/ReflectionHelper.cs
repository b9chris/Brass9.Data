using System;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web.Compilation;


namespace Brass9.Reflection
{
	public class ReflectionHelper
	{
		public static IEnumerable<Assembly> GetAllAssemblies()
		{
			var coll = BuildManager.GetReferencedAssemblies();
			var assemblies = coll.Cast<Assembly>();
			return assemblies;
			// http://stackoverflow.com/questions/2477787/difference-between-appdomain-getassemblies-and-buildmanager-getreferencedassembl
			//return AppDomain.CurrentDomain.GetAssemblies();
		}

		public static IEnumerable<Type> GetAllPublicClasses()
		{
			var assemblies = GetAllAssemblies().ToArray();
#if DEBUG
			try
			{
#endif
				return assemblies.SelectMany(a => a.GetExportedTypes()).ToArray();
#if DEBUG
			}
			catch (System.IO.FileLoadException ex)
			{
				if (ex.Message.StartsWith("Could not load file or assembly"))
				{
					throw new Exception("Nuget packages out of date or out of sync, update to synced versions.", ex);
				}

				throw;
			}
#endif
		}

		public static string GetRootNamespace()
		{
			StackTrace stackTrace = new StackTrace();
			StackFrame[] stackFrames = stackTrace.GetFrames();
			string ns = null;
			foreach(var frame in stackFrames)
			{
				string _ns = frame.GetMethod().DeclaringType.Namespace;
				int indexPeriod = _ns.IndexOf('.');
				string rootNs = _ns;
				if (indexPeriod > 0)
					rootNs = _ns.Substring(0, indexPeriod);

				if (rootNs == "System")
					break;
				ns = _ns;
			}

			return ns;
		}


		public static IEnumerable<Type> GetAllClassesInNamespace(string classNamespace, bool includeChildNamespaces)
		{
			var project = Assembly.GetCallingAssembly();
			return GetAllClassesInNamespace(project, classNamespace, includeChildNamespaces);
		}
		public static IEnumerable<Type> GetAllClassesInNamespace(Assembly project, string classNamespace, bool includeChildNamespaces)
		{
			var projectTypes = project.GetTypes().Where(c => c.Namespace != null);
			Func<Type, bool> nsFilter;
			if (includeChildNamespaces)
				nsFilter = new Func<Type, bool>(c => c.Namespace.StartsWith(classNamespace));
			else
				nsFilter = new Func<Type, bool>(c => c.Namespace == classNamespace);
				
			var types = projectTypes
				.Where(nsFilter)
				// Classes that declare async methods have a hidden generated class,
				// with .IsVisible = false and inheriting from ValueType (a struct basically)
				// We shouldn't be returning these confusing classes. Filter them here.
				.Where(c => c.IsVisible && c.IsSubclassOf(typeof(object)))
				.ToArray();
			return types;
		}



		public static PropertyInfo[] GetPublicProperties(Type type)
		{
			var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
			return props;
		}
		public static PropertyInfo[] GetPublicProperties<T>()
		{
			return GetPublicProperties(typeof(T));
		}

		public static PropertyInfo[] GetPublicProperties(Type type, string[] skipProperties)
		{
			var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => !skipProperties.Contains(p.Name)).ToArray();
			return props;
		}
		public static PropertyInfo[] GetPublicProperties<T>(string[] skipProperties)
		{
			return GetPublicProperties(typeof(T), skipProperties);
		}
		public static PropertyInfo[] GetPublicProperties<T>(params Expression<Func<T, object>>[] skipProperties)
		{
			var skipProps = skipProperties.Select(p => GetPropName(p)).ToArray();
			return GetPublicProperties(typeof(T), skipProps);
		}

		public static FieldInfo[] GetPublicConstants(Type type)
		{
			var consts = type.GetFields(BindingFlags.Public | BindingFlags.Static);
			return consts;
		}

		public static IEnumerable<PropertyInfo> GetSetterProperties(Type type)
		{
			var props = GetPublicProperties(type).Where(p => p.GetSetMethod() != null);
			return props;
		}

		public static ReflectionHelper<T> For<T>()
		{
			return new ReflectionHelper<T>();
		}



		public static MethodInfo GetPublicStaticMethod<T>(string methodName)
		{
			return GetPublicStaticMethod(typeof(T), methodName);
		}
		public static MethodInfo GetPublicStaticMethod(Type type, string methodName)
		{
			return type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
		}
		

		public static MethodInfo GetProtectedInstanceMethod<T>(string methodName)
		{
			return GetProtectedInstanceMethod(typeof(T), methodName);
		}
		public static MethodInfo GetProtectedInstanceMethod(Type type, string methodName)
		{
			return type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
		}

		/// <summary>
		/// Gets a property's MemberInfo (for example .Name gets you the property name) from a Property Accessor like
		/// t => t.Id
		/// </summary>
		/// <typeparam name="TParentClass">The parent class (t in the above example)</typeparam>
		/// <typeparam name="TProperty">The property's type</typeparam>
		/// <param name="propAccessor"></param>
		/// <returns></returns>
		public static PropertyInfo GetPropertyInfo<TParentClass, TProperty>(Expression<Func<TParentClass, TProperty>> propAccessor)
		{
			// http://stackoverflow.com/questions/2789504/get-the-property-as-a-string-from-an-expressionfunctmodel-tproperty
			// http://stackoverflow.com/questions/767733/converting-a-net-funct-to-a-net-expressionfunct
			// http://stackoverflow.com/questions/793571/why-would-you-use-expressionfunct-rather-than-funct
			MemberExpression expr;

			if (propAccessor.Body is MemberExpression)
				// .Net interpreted this code trivially like t => t.Id
				expr = (MemberExpression)propAccessor.Body;
			else
				// .Net wrapped this code in Convert to reduce errors, meaning it's t => Convert(t.Id) - get at the
				// t.Id inside
				expr = (MemberExpression)((UnaryExpression)propAccessor.Body).Operand;

			return (PropertyInfo)expr.Member;
		}

		/// <summary>
		/// Returns the name of a property like:
		/// 
		/// GetPropName MyModel (x => x.Id)
		/// </summary>
		/// <typeparam name="TParentClass">MyModel</typeparam>
		/// <param name="prop">x => x.Id</param>
		/// <returns>"Id"</returns>
		public static string GetPropName<TParentClass>(Expression<Func<TParentClass, object>> prop)
		{
			return GetPropName<TParentClass, object>(prop);
		}
		public static string GetPropName<TParentClass, TProperty>(Expression<Func<TParentClass, TProperty>> prop)
		{
			var memberInfo = ReflectionHelper.GetPropertyInfo<TParentClass, TProperty>(prop);
			return memberInfo.Name;
		}

		/// <summary>
		/// Get a public, instance property, by name.
		/// 
		/// Returns null if no such property exists.
		/// </summary>
		public static PropertyInfo GetProp(Type type, string name)
		{
			var prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
			return prop;
		}

		/// <summary>
		/// Get the value of a property from an object, by name.
		/// 
		/// Only works for public non-static (instance) properties.
		/// 
		/// Throws an Exception if no such property exists.
		/// </summary>
		public static object GetPropValue(object o, string name)
		{
			var prop = GetProp(o.GetType(), name);
			var v = prop.GetValue(o);
			return v;
		}
		public static T GetPropValue<T>(object o, string name)
		{
			var prop = GetProp(o.GetType(), name);
			T v = (T)prop.GetValue(o);
			return v;
		}

		public static void SetPropValue(object o, string name, object value)
		{
			var prop = o.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
			prop.SetValue(o, value);
		}
		public static void SetPropValue<T>(T o, Expression<Func<T, object>> prop, object value)
		{
			SetPropValue(o, GetPropName<T>(prop), value);
		}

		/// <summary>
		/// Gets the classname of an object, without any generic postfixes - for example returns
		/// SimpleProperty instead of SimpleProperty`2
		/// </summary>
		/// <param name="o"></param>
		/// <returns></returns>
		public static string GetClassName(object o)
		{
			return GetClassName(o.GetType());
		}
		public static string GetClassName(Type t)
		{
			var name = t.Name;
			int index = name.IndexOf("`");
			if (index > 0)
				return name.Substring(0, index);

			return name;
		}

		/// <summary>
		/// Perform an action based on the Type of the first argument, ignoring Generics
		/// 
		/// For example given a class of type SimpleProperty int, string - which returns a class .Name
		/// of SimpleProperty`2, execute Action code for typeof(SimpleProperty object, object) like:
		/// 
		/// ReflectionHelper.GenericTypeSwitch(obj.GetType(), new Dictionary Type, Action  {
		///		{ typeof(SimpleProperty object, object ), () => {...} }
		/// }
		/// </summary>
		/// <param name="type"></param>
		/// <param name="actions"></param>
		public static void GenericTypeSwitch(Type type, Dictionary<Type, Action> actions)
		{
			var name = ReflectionHelper.GetClassName(type);
			var key = actions.Keys.FirstOrDefault(t => ReflectionHelper.GetClassName(t) == name);
			if (key == null)
				return;

			var action = actions[key];
			action();
		}


		/// <summary>
		/// Given an existing value, and a target type it will be translated to, before calling Convert.ChangeType() on it,
		/// call this method to avoid empty string issues with numbers (or perhaps in the future, other things ChangeType() refuses
		/// to handle gracefully).
		/// </summary>
		/// <param name="value"></param>
		/// <param name="targetType"></param>
		/// <returns></returns>
		public static object GetDefaultTranslatedValue(object value, Type targetType)
		{
			if (value == null || (value.GetType() == typeof(String) && ((String)value) == String.Empty))
			{
				switch(targetType.Name)
				{
					case "Decimal":
					case "Double":
					case "Single":
					case "Byte":
					case "SByte":
					case "Int32":
					case "UInt32":
					case "Int64":
					case "UInt64":
					case "Int16":
					case "UInt16":
						return 0;

					case "Boolean":
						return false;
				}
			}

			return value;
		}

		/// <summary>
		/// Gets the generic argument to IEnumerable for a type that implements
		/// System.Collections.Generic.IEnumerable
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static Type GetIEnumerableType(Type type)
		{
			// We use `1 to indicate 1 generic argument (as opposed to none) http://msdn.microsoft.com/en-us/library/ayfa0fcd.aspx
			var ienumerable = type.GetInterface(typeof(System.Collections.Generic.IEnumerable<>).FullName);
			var generics = ienumerable.GetGenericArguments();
			return generics[0];
		}
	}

	/// <summary>
	/// Helps with Reflection for a specific class, using Linq Expressions to refer to the method.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class ReflectionHelper<T>
	{
		public MethodInfo GetMethod<TA1>(Expression<Func<T, Func<TA1>>> expr)
		{
			return ((MethodCallExpression)expr.Body).Method;
		}
		public MethodInfo GetMethod<TA1, TA2>(Expression<Func<T, Action<TA1, TA2>>> expr)
		{
			return ((MethodCallExpression)expr.Body).Method;
		}
		public MethodInfo GetMethod<TA1, TA2, TA3>(Expression<Func<T, Action<TA1, TA2, TA3>>> expr)
		{
			return ((MethodCallExpression)expr.Body).Method;
		}

		public MemberInfo GetPropertyInfo<TProperty>(Expression<Func<T, TProperty>> propAccessor)
		{
			return ReflectionHelper.GetPropertyInfo<T, TProperty>(propAccessor);
		}
	}
}
