using System;

namespace CSC.Common.Infrastructure.System
{
	/// <summary>
	/// Provides the current time.
	/// </summary>
	public interface ITimeProvider
	{
		/// <summary>
		/// The current date/time in UTC.
		/// </summary>
		DateTime UtcNow { get; }
	}
}
