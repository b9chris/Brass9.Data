using System;


namespace Brass9
{
	/// <summary>
	/// Wraps a value or immutable (like an int or a string) in a mutable reference to get around C#'s lack of syntax
	/// for storing boxed value types and references to immutables. Useful for, for example, performing a lock on an int.
	/// </summary>
	/// <typeparam name="T">A value type or immutable type. Reference types are fine, they're just not useful.</typeparam>
	public class Ref<T>
	{
		public Ref(T value)
		{
			Value = value;
		}

		public T Value { get; set; }
	}
}
