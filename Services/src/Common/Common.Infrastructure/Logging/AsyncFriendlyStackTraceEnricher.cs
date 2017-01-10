using AsyncFriendlyStackTrace;
using Serilog.Core;
using Serilog.Events;

namespace CSC.Common.Infrastructure.Logging
{
	/// <summary>
	/// Adds a readable stack trace to all exceptions.
	/// </summary>
	public class AsyncFriendlyStackTraceEnricher : ILogEventEnricher
	{
		/// <summary>
		/// Enriches the log event.
		/// </summary>
		public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
		{
			if (logEvent.Exception == null)
				return;

			var readableStackTrace = propertyFactory.CreateProperty(
				"ReadableStackTrace",
				logEvent.Exception.ToAsyncString(),
				destructureObjects: true);

			logEvent.AddPropertyIfAbsent(readableStackTrace);

			var fullExceptionString = propertyFactory.CreateProperty(
				"FullExceptionString",
				logEvent.Exception.ToString(),
				destructureObjects: true);

			logEvent.AddPropertyIfAbsent(fullExceptionString);
		}
	}
}