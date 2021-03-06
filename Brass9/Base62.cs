﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Brass9
{
	/// <summary>
	/// For when Base64 is too much for the encoder on an external system.
	/// Uses just numbers, lower/upper letters. 10+26+26=62
	/// </summary>
	public class Base62
	{
		protected static char[] base62CharMap;
		public static char[] Base62CharMap
		{
			get
			{
				if (base62CharMap == null)
				{
					base62CharMap = new char[62];

					int i, j;
					for (i = 0, j = 0; i < 10; i++, j++)
						base62CharMap[i] = (char)('0' + j);

					for (j = 0; j < 26; i++, j++)
						base62CharMap[i] = (char)('A' + j);

					for (j = 0; j < 26; i++, j++)
						base62CharMap[i] = (char)('a' + j);
				}
				return base62CharMap;
			}
		}

		public static bool IsBase62String(string s)
		{
			Regex regex = new Regex(@"^[\w\d]+$");
			return regex.IsMatch(s);
		}

		/// <summary>
		/// Warning: Does not verify input is a valid Base62 char - assumes it's been checked already, for example with
		/// IsBase62String
		/// </summary>
		/// <param name="c"></param>
		/// <returns></returns>
		public static int MapCharBackToNumber(char c)
		{
			// Map back in ASCII value order to avoid double-bound checking
			if (c <= '9')
				return c - '0';

			if (c <= 'Z')
				return c - 'A' + 10;

			//if (c <= 'z')
				return c - 'a' + 36;
		}


		/// <summary>
		/// Base 62 encodes a number or guid (passed in as bytes) with no =, ampersands, or other characters that would
		/// violate URL or HTML entity rules or cause them to be HTML/URL encoded.
		/// Specifically, uses upper and lower, and numbers.
		/// Normal .Net base 64 uses =, which can end up causing intermittent trouble in querystrings, and some systems
		/// won't tolerate dashes.
		/// 
		/// Note that the resulting encoded string is big-endian, meaning the most significant digits are at the end.
		/// This really doesn't matter to most people reading an already obtuse base 62 string, but, FYI. It's cheaper.
		/// </summary>
		/// <param name="bytes">A byte array representing a positive number - typically a GUID</param>
		/// <returns>A string that is ~4/3 as long as the byte array. 21-22 chars long for a GUID.</returns>
		public static string Base62Encode(byte[] bytes)
		{
			// http://stackoverflow.com/a/3265796/176877
			var value = new BigInteger(bytes);
			var sb = new StringBuilder(bytes.Length);	// Guess 1:1 length, to over-provision slightly

			// BigInteger irritatingly treats some Guid-generated byte arrays as negative. Don't put up with it.
			if (value < 0)
				value = value * -2;

			var charMap = Base62CharMap;	// Skip the getter in the loop

			do
			{
				BigInteger rem;
				value = BigInteger.DivRem(value, 62, out rem);
				sb.Append(charMap[(int)rem]);
			} while (value > 0);

			return sb.ToString();
		}

		/// <summary>
		/// Returns a base62-encoded Guid. Convenience method.
		/// 21-22 chars long.
		/// </summary>
		public static string NewGuid()
		{
			return Base62Encode(Guid.NewGuid().ToByteArray());
		}

		public static byte[] Base62Decode(string encoded)
		{
			throw new NotImplementedException("TODO");
		}
	}
}
