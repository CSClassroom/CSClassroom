using System;

namespace CSC.CSClassroom.WebApp.Providers
{
	/// <summary>
	/// Provides the current timezone offset.
	/// </summary>
	public interface ITimeZoneProvider
	{
		/// <summary>
		/// Returns the given UTC datetime, in the user's local timezone.
		/// </summary>
		DateTime ToUserLocalTime(DateTime utcDateTime);

		/// <summary>
		/// Returns the given user local time, converted to UTC.
		/// </summary>
		DateTime ToUtcTime(DateTime userLocalTime);
	}
}
