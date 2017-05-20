using System.Collections.Generic;


namespace Brass9.Collections.HashSetExtensions
{
	public static class HashSetExtension
	{
		public static bool TryAdd<T>(this HashSet<T> _this, T t)
		{
			if (_this.Contains(t))
				return false;

			_this.Add(t);
			return true;
		}

		public static bool TryRemove<T>(this HashSet<T> _this, T t)
		{
			if (!_this.Contains(t))
				return false;

			_this.Remove(t);
			return true;
		}
	}
}
