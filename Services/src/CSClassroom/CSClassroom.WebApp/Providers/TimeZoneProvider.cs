using System;
using Microsoft.AspNetCore.Http;
using NodaTime;

namespace CSC.CSClassroom.WebApp.Providers
{
	/// <summary>
	/// Provides the user's current time zone from the current request headers.
	/// </summary>
	public class TimeZoneProvider : ITimeZoneProvider
	{
        /// <summary>
        /// The current time zone.
        /// </summary>
        private static readonly DateTimeZone LocalTimeZone 
            = DateTimeZoneProviders.Tzdb["America/Los_Angeles"];

        /// <summary>
        /// Returns the given UTC datetime, converted to the user's local timezone.
        /// </summary>
        public DateTime ToUserLocalTime(DateTime utcDateTime)
        {
            if (utcDateTime == DateTime.MinValue)
                return DateTime.MinValue;

            return Instant
                .FromDateTimeUtc(DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc))
                .InZone(LocalTimeZone)
                .ToDateTimeUnspecified();
        }

        /// <summary>
        /// Returns the given user local time, converted to UTC.
        /// </summary>
        public DateTime ToUtcTime(DateTime userLocalTime)
        {
            if (userLocalTime == DateTime.MinValue)
                return DateTime.MinValue;

            return LocalTimeZone
                .AtLeniently(LocalDateTime.FromDateTime(userLocalTime))
                .ToDateTimeUtc();
        }
	}
}
