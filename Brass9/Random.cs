using System;
using System.Collections.Generic;
using System.Linq;


namespace Brass9
{
	/// <summary>
	/// A thread-safe Random number generator you don't have to worry about instantiating to use.
	/// </summary>
	public class Random
	{
		#region Singleton
		// http://codereview.stackexchange.com/questions/79/implementing-a-singleton-pattern-in-c
		public static Random Current { get { return Nested.instance; } }

		class Nested
		{
			static Nested()
			{
			}

			internal static readonly Random instance = new Random();
		}
		#endregion


		[ThreadStatic]
		protected System.Random random = new System.Random();

		#region Wrapper for Random class
		public int Next()
		{
			return random.Next();
		}

		public int Next(int maxValue)
		{
			return random.Next(maxValue);
		}

		public int Next(int minValue, int maxValue)
		{
			return random.Next(minValue, maxValue);
		}

		public double NextDouble()
		{
			return random.NextDouble();
		}

		public void NextBytes(byte[] buffer)
		{
			random.NextBytes(buffer);
		}
		#endregion

		public bool NextBool()
		{
			return Next(2) == 0;	// Not that it matters, but on most CPUs comparison with zero is a single-tick operation, while comparison with any other number involves LOD, BFE
		}
	}
}
