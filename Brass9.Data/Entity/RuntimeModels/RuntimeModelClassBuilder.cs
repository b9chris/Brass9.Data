using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Brass9.Data.Entity.RuntimeModels
{
	// http://stackoverflow.com/a/723018/176877
	// TODO: Just use this? https://github.com/thiscode/DynamicSelectExtensions/blob/master/DynamicSelectExtensions/DynamicTypeBuilder.cs
	public class RuntimeModelClassBuilder
	{
		#region Singleton
		// http://codereview.stackexchange.com/questions/79/implementing-a-singleton-pattern-in-c
		public static RuntimeModelClassBuilder O { get { return Nested.instance; } }

		class Nested
		{
			static Nested()
			{
			}

			internal static readonly RuntimeModelClassBuilder instance = new RuntimeModelClassBuilder();
		}
		#endregion



		protected AssemblyName assemblyName = new AssemblyName() { Name = "RuntimeModel" };
		protected ModuleBuilder moduleBuilder = null;

		/// <summary>
		/// A simplistic cache of built classes/Types, to avoid building them over and over
		/// </summary>
		protected Dictionary<string, Type> builtTypes = new Dictionary<string, Type>();

		protected RuntimeModelClassBuilder()
		{
			moduleBuilder = Thread.GetDomain().DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run).DefineDynamicModule(assemblyName.Name);
		}

		public Type GetRuntimeModel(Dictionary<string, Type> fields, Type parentClass = null)
		{
			if (null == fields)
				throw new ArgumentNullException("fields");
			if (0 == fields.Count)
				throw new ArgumentOutOfRangeException("fields", "fields must have at least 1 field definition");

			lock (builtTypes)
			{
				string className = makeClassName(fields, parentClass);

				if (builtTypes.ContainsKey(className))
					return builtTypes[className];

				TypeBuilder typeBuilder = moduleBuilder.DefineType(className, TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Serializable, parentClass);
				var classHelper = Brass9.Reflection.RuntimeClassHelper.O;

				foreach (var field in fields)
				{
					// Translate more complex list-types to simple generic IEnumerable.
					// Allows us to assign typical select mappings like .prop.OrderBy... which is IOrderedEnumerable, not ICollection

					var fieldType = field.Value;

					var genericArgs = fieldType.GetGenericArguments();
					if (
						genericArgs.Length == 1
						&& typeof(ICollection<>).MakeGenericType(genericArgs[0]).IsAssignableFrom(fieldType)
					)
					{
						// is ICollection<T>, change to IEnumerable<T>
						fieldType = typeof(IEnumerable<>).MakeGenericType(genericArgs[0]);
					}

					classHelper.DefineProperty(typeBuilder, field.Key, fieldType);
				}

				builtTypes[className] = typeBuilder.CreateType();

				return builtTypes[className];
			}
		}

		public Type GetRuntimeModel(IEnumerable<PropertyInfo> fields)
		{
			return GetRuntimeModel(fields.ToDictionary(f => f.Name, f => f.PropertyType));
		}



		/// <summary>
		/// The key for the Dictionary cache of built types
		/// </summary>
		/// <param name="fields"></param>
		/// <returns></returns>
		protected string makeClassName(Dictionary<string, Type> fields)
		{
			string key = String.Join("_", new[] { "Model" }.Concat(fields.Select(f => f.Value.Name).OrderBy(f => f)));
			return key;
		}

		protected string makeClassName(Dictionary<string, Type> fields, Type parentClass)
		{
			// TODO: This is blatant specialized code for the wider Model Builder library.
			// We should probably offer a getClassName Func when the GetRuntimeModel method is called, to allow nice
			// ClassNames to be generated, if they even really matter - all that really matters is strong uniqueness.
			var typeIEnumerable = typeof(System.Collections.IEnumerable);
			string key = String.Join("_", new[] { "Model", parentClass.GetGenericArguments()[0].Name }.Concat(
				fields.Select(f =>
					typeIEnumerable.IsAssignableFrom(f.Value)
					? "coll" + f.Value.GetGenericArguments()[0].Name	// If it's ICollection we'd like the type name inside for debug
					: f.Value.Name
				).OrderBy(f => f)
			));
			return key;
		}

		protected string makeClassName(IEnumerable<PropertyInfo> fields)
		{
			return makeClassName(fields.ToDictionary(f => f.Name, f => f.PropertyType));
		}
	}
}
