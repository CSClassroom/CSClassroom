using System;
using CSC.CSClassroom.WebApp.Providers;

namespace CSC.CSClassroom.WebApp.Extensions
{
	/// <summary>
	/// Extension methods for DateTime objects.
	/// </summary>
	public static class DateTimeExtensions
	{
		/// <summary>
		/// Formats the given date time, in the correct timezone.
		/// </summary>
		public static string FormatLongDateTime(
			this DateTime dateTime, 
			ITimeZoneProvider timeZoneProvider)
		{
			var userTime = timeZoneProvider.ToUserLocalTime(dateTime);

			return $"{userTime.ToString("M/d/yyyy")} at {userTime.ToString("h:mm tt")}";
		}

		/// <summary>
		/// Formats the given date time, in the correct timezone.
		/// </summary>
		public static string FormatShortDateTime(
			this DateTime dateTime,
			ITimeZoneProvider timeZoneProvider)
		{
			var userTime = timeZoneProvider.ToUserLocalTime(dateTime);

			return userTime.ToString("M/d/yyyy h:mm tt");
		}
	}
}
