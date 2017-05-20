using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brass9
{
	/// <summary>
	/// Marker interface to indicate a class has a static void Init(string appRoot) that needs to be called,
	/// and that this class would like to opt-in to automatic configuration.
	/// </summary>
	public interface IAppRootInit
	{
	}
}
