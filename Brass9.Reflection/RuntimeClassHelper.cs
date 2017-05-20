using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Brass9.Reflection
{
	/// <summary>
	/// Methods to help build a class at Runtime
	/// </summary>
	public class RuntimeClassHelper
	{
		#region Singleton
		// http://codereview.stackexchange.com/questions/79/implementing-a-singleton-pattern-in-c
		public static RuntimeClassHelper O { get { return Nested.instance; } }

		class Nested
		{
			static Nested()
			{
			}

			internal static readonly RuntimeClassHelper instance = new RuntimeClassHelper();
		}
		#endregion
		


		/// <summary>
		/// Defines a Property in a runtime-generated class.
		/// 
		/// Keeps it simple Public property, { get; set; } no special accessors, protected or other nonsense.
		/// 
		/// Note that TypeBuilder already has a method called DefineProperty, but it's deceptive because it doesn't
		/// actually do what you think. It defines just a little piece of the overall Property implementation you're
		/// used to seeing in .Net - there's a ton of syntactic sugar Visual Studio is doing for you you must now
		/// write out yourself, because Microsoft couldn't be bothered to put that sugar in a Framework call somewhere.
		/// </summary>
		/// <param name="typeBuilder">The typeBuilder being used to build the Runtime Model</param>
		/// <param name="name">Name of property. Since it's public, make sure it's uppercase camelcase like TheThing</param>
		/// <param name="type">Type of property</param>
		public void DefineProperty(TypeBuilder typeBuilder, string name, Type type)
		{
			// http://stackoverflow.com/a/20175642/176877

			// Property stub
			var propertyBuilder = typeBuilder.DefineProperty(name, PropertyAttributes.None, type, null);

			// backing private field
			var backingFieldName = '_' + name.Substring(0, 1).ToLower() + name.Substring(1);
			var fieldBuilder = typeBuilder.DefineField(backingFieldName, type, FieldAttributes.Private);

			// getter
			// Note that the prefix "get_" seems to be special in C# + Visual Studio
			var getterBuilder = typeBuilder.DefineMethod("get_" + name,
				MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
				type, Type.EmptyTypes
			);

			var getterIL = getterBuilder.GetILGenerator();
			getterIL.Emit(OpCodes.Ldarg_0);
			getterIL.Emit(OpCodes.Ldfld, fieldBuilder);
			getterIL.Emit(OpCodes.Ret);

			// setter
			var setterBuilder = typeBuilder.DefineMethod("set_" + name,
				MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
				null, new Type[] { type }
			);

			var setterIL = setterBuilder.GetILGenerator();
			setterIL.Emit(OpCodes.Ldarg_0);
			setterIL.Emit(OpCodes.Ldarg_1);
			setterIL.Emit(OpCodes.Stfld, fieldBuilder);
			setterIL.Emit(OpCodes.Ret);

			propertyBuilder.SetGetMethod(getterBuilder);
			propertyBuilder.SetSetMethod(setterBuilder);
		}
	}
}
