using System;

namespace Brass9.Collections.HasProp
{
	/// <summary>
	/// Marks a class as having a string Key property
	/// </summary>
	public interface IKey
	{
		string Key { get; set; }
	}
}
