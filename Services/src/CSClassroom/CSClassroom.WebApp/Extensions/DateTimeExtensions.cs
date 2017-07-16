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

		/// <summary>
		/// Formats the given date, in the correct timezone.
		/// </summary>
		public static string FormatShortDate(
			this DateTime dateTime,
			ITimeZoneProvider timeZoneProvider)
		{
			var userTime = timeZoneProvider.ToUserLocalTime(dateTime);

			return userTime.ToString("M/d");
		}

		/// <summary>
		/// Converts a DateTime to epoch time (the number of seconds since 1/1/1970)
		/// </summary>
		public static long ToEpoch(this DateTime dateTimeUtc)
		{
			var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			return Convert.ToInt64((dateTimeUtc - epoch).TotalMilliseconds);
		}

		/// <summary>
		/// Converts a DateTime to epoch time (the number of seconds since 1/1/1970)
		/// </summary>
		public static DateTime FromEpoch(this long dateTimeEpoch)
		{
			var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			return epoch.AddMilliseconds(dateTimeEpoch);
		}
	}
}
