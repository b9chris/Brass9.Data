using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brass9.Data.UniqueCodeGenerating
{
	public class UniqueCodeGenerator
	{
		/// <summary>
		/// Safe characters to use, readability-wise, when generating random codes.
		/// Things like O's or 0's aren't present to mistake for each other.
		/// </summary>
		public const string CodeChars = "AaCcDdEeFfGgHhiJjKkLMmNnPpQqRrsTtUuVvWwXxYyz4679";	// 48 chars

	
		/// <summary>
		/// Generates a batch of unique codes.
		/// 
		/// They're checked for uniqueness - guaranteed unique - within this generated group. They are not checked for
		/// uniqueness against any existing codes in the db.
		/// </summary>
		/// <param name="numCodes">Number of codes to generate.</param>
		/// <param name="codeLength">Number of digits to use in the generated codes.</param>
		public string[] GenerateCodes(int numCodes, int codeLength)
		{
			// Set of allowed characters. We split this and use its length as powBase, below, to determine the number of possibilities.
			var codeBase = CodeChars.ToCharArray();

			int powBase = codeBase.Length;
			double ceiling = Math.Pow(powBase, codeLength);

			int codesMade = 0;
			double[] codes = new double[numCodes];
			var random = new Random();
			while(codesMade < numCodes)
			{
				// gen code
				double r = random.NextDouble();
				double code = Math.Floor(ceiling * r);
				if (!codes.Contains(code))
					codes[codesMade++] = code;
			}

			var strings = codes.Select(c =>
			{
				char[] chars = new char[codeLength];
				for (int i = codeLength - 1; i >= 0; i--)
				{
					int remainder = (int)(c % powBase);
					chars[i] = codeBase[remainder];
					c -= remainder;
					c /= powBase;
				}
				return new String(chars);
			}).ToArray();

			return strings;
		}

		public string GenerateCode(int codeLength)
		{
			return GenerateCodes(1, codeLength)[0];
		}
	}
}
