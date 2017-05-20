using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brass9
{
	public abstract class EventArgsRelated : EventArgs
	{
		public abstract object RelatedObject { get; set; }
	}

	public class EventArgsRelated<T> : EventArgsRelated
	{
		public override object RelatedObject
		{
			get
			{
				return RelatedGeneric;
			}
			set
			{
				RelatedGeneric = (T)value;
			}
		}
		public T RelatedGeneric { get; set; }
	}
}
