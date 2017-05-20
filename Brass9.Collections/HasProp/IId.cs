using System;

namespace Brass9.Collections.HasProp
{
	/// <summary>
	/// Marks a class as having an int Id property
	/// </summary>
	public interface IId
	{
		int Id { get; set; }
	}
}
