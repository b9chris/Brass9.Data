using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brass9.Data.Entity.RuntimeModels
{
	public class BaseRuntimeModel
	{
	}

	/// <summary>
	/// We know at compile-time the result of our custom Select/Expression Tree will be an Anonymous object, except,
	/// it will at least have T Model as a property, where T is the DbSet/IQueryable type you passed it.
	/// We can simplify the overall process by defining this Generic at compile-time and then having the
	/// Anonymous object actually be a run-time defined subclass of this Generic.
	/// </summary>
	/// <typeparam name="TModel"></typeparam>
	public class BaseRuntimeModel<TModel> : BaseRuntimeModel
	{
		public TModel Model { get; set; }
		// Child IEnumerable props will appear here in run-time Emit subclasses
	}
}
