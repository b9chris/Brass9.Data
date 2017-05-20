using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brass9.Data.Entity
{
	public static class DbHelper
	{
		// Usage:
		// public override int SaveChanges()
		// {
		//		return WrapSaveChanges(base.SaveChanges);
		// }

		public static int WrapSaveChanges(Func<int> baseSaveChanges)
		{
			// Dump EF Validation Errors into the Exception message
			// http://stackoverflow.com/questions/15820505/dbentityvalidationexception-how-can-i-easily-tell-what-caused-the-error
			try
			{
				return baseSaveChanges();
			}
			catch (DbEntityValidationException ex)
			{
				var tuple = ParseEntityValidationException(ex);
				// Throw a new DbEntityValidationException with the improved exception message.
				throw new DbEntityValidationException(tuple.Item1, tuple.Item2);
			}
		}

		public static async Task<int> WrapSaveChangesAsync(Func<Task<int>> baseSaveChangesAsync)
		{
			try
			{
				return await baseSaveChangesAsync();
			}
			catch (DbEntityValidationException ex)
			{
				var tuple = ParseEntityValidationException(ex);
				// Throw a new DbEntityValidationException with the improved exception message.
				throw new DbEntityValidationException(tuple.Item1, tuple.Item2);
			}
		}



		public static Tuple<string, IEnumerable<DbEntityValidationResult>> ParseEntityValidationException(DbEntityValidationException ex)
		{
			// Retrieve the error messages as a list of strings.
			var errorMessages = ex.EntityValidationErrors
					.SelectMany(x => x.ValidationErrors)
					.Select(x => x.PropertyName + ": " + x.ErrorMessage);
	
			// Join the list to a single string.
			var fullErrorMessage = string.Join(" ", errorMessages);
	
			// Combine the original exception message with the new one.
			var exceptionMessage = string.Concat(ex.Message, " ", fullErrorMessage);

			// Pass the details for a new DbEntityValidationException with the improved exception message.
			return new Tuple<string, IEnumerable<DbEntityValidationResult>>(exceptionMessage, ex.EntityValidationErrors);
		}
	}
}
