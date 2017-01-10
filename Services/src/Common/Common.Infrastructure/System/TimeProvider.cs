using System;

namespace CSC.Common.Infrastructure.System
{
	/// <summary>
	/// TimeProvider implementation that returns the current time,
	/// as reported by the system.
	/// </summary>
	public class TimeProvider : ITimeProvider
	{
		/// <summary>
		/// The current time, in UTC.
		/// </summary>
		public DateTime UtcNow => DateTime.UtcNow;
	}
}
