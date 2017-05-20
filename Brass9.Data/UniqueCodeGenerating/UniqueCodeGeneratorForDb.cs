using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brass9.Data.UniqueCodeGenerating
{
	public abstract class UniqueCodeGenerator<TDb, TUniqueCodeEntity> : UniqueCodeGenerator
		where TDb : DbContext
		where TUniqueCodeEntity : class, IUniqueCode, new()
	{
		public abstract int CodeLength { get; }



		public string GenerateCode()
		{
			return GenerateCode(CodeLength);
		}

		public string[] GenerateCodes(int count)
		{
			return GenerateCodes(count, CodeLength);
		}

		/// <summary>
		/// Generate a code, guaranteeing uniqueness in the db.
		/// </summary>
		/// <returns></returns>
		public async Task<TUniqueCodeEntity> GenerateCodeAsync(TDb db)
		{
			TUniqueCodeEntity newCodeEntity = null;

			var dbSet = db.Set<TUniqueCodeEntity>();

			// Check, write, inside Transaction to guarantee uniqueness
			// TODO: Is it possible to ask the db to write-if-unique internally?
			// Could just declare the col Unique, let the Index kick it out with an Exception, might be
			// faster under the hood?
			using (var atomic = db.Database.BeginTransaction())
			{
				string code = null;
				bool exists = true;
				while (exists)
				{
					code = GenerateCode();
					exists = await dbSet
						.Where(c => c.Code == code)
						.AnyAsync();
				}

				newCodeEntity = new TUniqueCodeEntity
				{
					Code = code
				};
				dbSet.Add(newCodeEntity);
				await db.SaveChangesAsync();
				atomic.Commit();
			}

			return newCodeEntity;
		}

		/// <summary>
		/// Generate multiple codes, guaranteed to be unique in the db.
		/// 
		/// TODO: The performance for this in large batches is likely terrible.
		/// </summary>
		/// <param name="count"></param>
		/// <returns></returns>
		public async Task<TUniqueCodeEntity[]> GenerateCodesAsync(int count, TDb db)
		{
			var codes = new TUniqueCodeEntity[count];
			for (int i = count - 1; i >= 0; i--)
				codes[i] = await GenerateCodeAsync(db);

			return codes;
		}
	}
}
