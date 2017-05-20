using System;
using System.Collections.Generic;
using System.Linq;

namespace Brass9
{
	/// <summary>
	/// Shorthand class for coping with common null situations
	/// </summary>
	public class NN
	{
		// Methods that offer shorthand for a.b where a might be null (or a.b.c where a or b might be null)

		public static TValue N<TParent, TValue>(TParent o, Func<TParent, TValue> accessor, TValue defaultValue)
		{
			if (o == null)
				return defaultValue;

			return accessor(o);
		}

		/// <summary>
		/// If the string argument is null, returns an empty string.
		/// If it's not, runs formatter on it and returns the result.
		/// Useful for situations like:
		/// 
		/// @Model.Address.City where Address could be null. Use:
		/// 
		/// @NN.S(Model.Address, a => a.City)
		/// 
		/// Will return an empty string or City, depending on whether Address is null.
		/// 
		/// You can make it even shorter if you want by aliasing in the page like:
		/// 
		/// var nn = NN.S;
		/// @nn(Model.Address, a => a.City)
		/// 
		/// "blah " + myString + " blah" where myString could be null and blow up the whole thing.
		/// Becomes:
		/// "blah" + NN.S(myString, s => " " + s) + " blah"
		/// 
		/// Shorthand for:
		/// "blah" + (myString == null ? "" : " " + myString) + " blah"
		/// </summary>
		/// <param name="s"></param>
		/// <param name="accessor"></param>
		/// <returns></returns>
		public static string S<T>(T o, Func<T, string> accessor)
		{
			return NN.N(o, accessor, "");
		}

		/// <summary>
		/// Returns 0 if parent is null
		/// </summary>
		public static int Z<T>(T o, Func<T, int> accessor)
		{
			return NN.N(o, accessor, 0);
		}

		/// <summary>
		/// Guarantees return of null or value, given a prop you want to access, even if the parent in the first argument
		/// is null (instead of throwing).
		/// 
		/// Usage:
		/// 
		/// NN.N(myObjThatCouldBeNull, o => o.ChildPropIWant)
		/// </summary>
		/// <returns>Null if that prop is null, or if the child prop is null. The value of the prop if both are set.</returns>
		public static TValue N<TParent, TValue>(TParent o, Func<TParent, TValue> accessor)
			where TValue : class
		{
			return NN.N(o, accessor, null);
		}

		// Chainable version
		public static TValue N<TParent, TMiddle, TValue>(
			TParent o,
			Func<TParent, TMiddle> midAccessor,
			Func<TMiddle, TValue> valAccessor
		)
			where TMiddle : class
			where TValue : class
		{
			var mid = NN.N(o, midAccessor);
			return NN.N(mid, valAccessor);
		}

		public static string PrependSpace<T>(T o)
		{
			return NN.S(o, _ => " " + _);
		}

		public static string AppendSpace<T>(T o)
		{
			return NN.S(o, _ => _ + " ");
		}



		/// <summary>
		/// Shorthand like Javascript
		/// a || b use-b-if-a-is-null
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static string AorB(string a, string b)
		{
			switch (a)
			{
				case null:
				case "":
					return b;
			}

			return a;
		}

		public static T AorB<T>(T a, T b)
		{
			if (a == null)
				return b;

			return a;
		}
	}
}
