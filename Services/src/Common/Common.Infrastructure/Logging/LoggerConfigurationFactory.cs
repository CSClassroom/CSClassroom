using System;
using CSC.Common.Infrastructure.Logging;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.ExtensionMethods;

namespace CSC.Common.Infrastructure.Configuration
{
	/// <summary>
	/// Creates a logger configuration.
	/// </summary>
	public static class LoggerConfigurationFactory
	{
		/// <summary>
		/// Creates a logger for Serilog.
		/// </summary>
		public static LoggerConfiguration CreateLoggerConfiguration(
			IConfigurationRoot configurationRoot, 
			Func<LogEvent, bool> includeLogEvent)
		{
			var loggerConfiguration = new LoggerConfiguration()
				.MinimumLevel.Information()
				.Enrich.FromLogContext()
				.Enrich.With(new AsyncFriendlyStackTraceEnricher());

			// Always write to the console
			loggerConfiguration.WriteTo.ColoredConsole();

			// Write to disk if requested
			var rollingFilePath = configurationRoot["RollingLogPath"];
			if (rollingFilePath != null)
			{
				loggerConfiguration.WriteTo.RollingFile(rollingFilePath);
			}

			// Write to application insights if requested
			var appInsightsKey = configurationRoot
				.GetSection("ApplicationInsights")?["InstrumentationKey"];
			if (appInsightsKey != null)
			{
				loggerConfiguration.WriteTo.ApplicationInsightsTraces
				(
					appInsightsKey,
					LogEventLevel.Information,
					null /*formatProvider*/,
					(logEvent, formatProvider) => 
						ConvertLogEventsToCustomTraceTelemetry(logEvent, formatProvider, includeLogEvent)
				);
			}

			return loggerConfiguration;
		}

		/// <summary>
		/// Converts Serilog traces/exceptions to application insights traces/exceptions. 
		/// </summary>
		private static ITelemetry ConvertLogEventsToCustomTraceTelemetry(
			LogEvent logEvent, 
			IFormatProvider formatProvider, 
			Func<LogEvent, bool> includeLogEvent)
		{
			if (logEvent.Exception == null && includeLogEvent != null && !includeLogEvent(logEvent))
			{
				return null;
			}

			// first create a default TraceTelemetry using the sink's default logic
			// .. but without the log level, and (rendered) message (template) included in the Properties
			ITelemetry telemetry = logEvent.Exception == null
				? (ITelemetry)logEvent.ToDefaultTraceTelemetry(
					formatProvider,
					includeLogLevelAsProperty: false,
					includeRenderedMessageAsProperty: false,
					includeMessageTemplateAsProperty: false)
				: (ITelemetry)logEvent.ToDefaultExceptionTelemetry(
					formatProvider,
					includeLogLevelAsProperty: false,
					includeRenderedMessageAsProperty: false,
					includeMessageTemplateAsProperty: false);

			// and remove RequestId from the telemetry properties
			if (logEvent.Properties.ContainsKey("RequestId"))
			{
				((ISupportProperties)telemetry).Properties.Remove("RequestId");
			}
			if (logEvent.Properties.ContainsKey("OperationId"))
			{
				var operationId = logEvent.Properties["OperationId"].ToString();
				((ISupportProperties)telemetry).Properties.Remove("OperationId");
				telemetry.Context.Operation.Id = operationId;
			}

			return telemetry;
		}
	}
}
