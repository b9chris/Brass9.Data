using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Brass9
{
	public class CoreApp
	{
		public string AppRoot { get; protected set; }
		public string RootNs { get; protected set; }
		public Assembly RootAssembly { get; protected set; }



		public static CoreApp O { get; protected set; }

		public static void Init(CoreApp coreApp)
		{
			O = coreApp;
		}
	}
}
