using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Concurrent;
using System.Threading;


namespace Brass9.Collections.Concurrent.Simplified
{
	/// <summary>
	/// Useful for simplified Message Queues - Setup a ConcurrentQueueSimple as a simple message queue, put messages in, spin up a worker
	/// on a regular Thread to process them periodically.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class ConcurrentQueueSimple<T> : ConcurrentQueue<T>
	{
		public T Dequeue()
		{
			T item;
			var spinWait = new SpinWait();
			while (!TryDequeue(out item))
			{
				spinWait.SpinOnce();
			}
			return item;
		}

		public T Peek()
		{
			T item;
			var spinWait = new SpinWait();
			while (!TryPeek(out item))
			{
				spinWait.SpinOnce();
			}
			return item;
		}
	}
}
