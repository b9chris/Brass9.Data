using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brass9
{
	public class EventArgs<T1> : EventArgs
	{
		public T1 Value { get; set; }

		public EventArgs(T1 value)
		{
			Value = value;
		}
	}

	public class EventArgs<T1, T2> : EventArgs
	{
		public T1 Prop1 { get; set; }
		public T2 Prop2 { get; set; }
	}
}
