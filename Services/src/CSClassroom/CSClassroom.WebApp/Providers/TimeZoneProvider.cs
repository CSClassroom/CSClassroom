using System;
using Microsoft.AspNetCore.Http;

namespace CSC.CSClassroom.WebApp.Providers
{
	/// <summary>
	/// Provides the user's current time zone from the current request headers.
	/// </summary>
	public class TimeZoneProvider : ITimeZoneProvider
	{
		/// <summary>
		/// The HTTP context accessor.
		/// </summary>
		private readonly IHttpContextAccessor _httpContextAccessor;

		/// <summary>
		/// Constructor.
		/// </summary>
		public TimeZoneProvider(IHttpContextAccessor httpContextAccessor)
		{
			_httpContextAccessor = httpContextAccessor;
		}

		/// <summary>
		/// Returns the given timezone offset.
		/// </summary>
		private TimeSpan GetTimeZoneOffset()
		{
			var timeZoneOffsetStr = _httpContextAccessor?.HttpContext?.Request?.Cookies["_timeZoneOffset"];
			double timeZoneOffset;
			if (timeZoneOffsetStr != null && double.TryParse(timeZoneOffsetStr, out timeZoneOffset))
			{
				return TimeSpan.FromMinutes(timeZoneOffset);
			}
			else
			{
				return TimeSpan.Zero;
			}
		}

		/// <summary>
		/// Returns the given UTC datetime, converted to the user's local timezone.
		/// </summary>
		public DateTime ToUserLocalTime(DateTime utcDateTime)
		{
			return utcDateTime != DateTime.MinValue
				? utcDateTime + GetTimeZoneOffset()
				: utcDateTime;
		}

		/// <summary>
		/// Returns the given user local time, converted to UTC.
		/// </summary>
		public DateTime ToUtcTime(DateTime userLocalTime)
		{
			return userLocalTime != DateTime.MinValue
				? userLocalTime - GetTimeZoneOffset()
				: userLocalTime;
		}
	}
}
