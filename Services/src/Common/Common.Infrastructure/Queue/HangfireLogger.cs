using System;
using Hangfire.Logging;
using Microsoft.Extensions.Logging;

namespace CSC.Common.Infrastructure.Queue
{
	using HangfireLogLevel = Hangfire.Logging.LogLevel;
	using SystemLogLevel = Microsoft.Extensions.Logging.LogLevel;

	/// <summary>
	/// A job activator using an AutoFac container.
	/// </summary>
	public class HangfireLogger : ILog
	{
		/// <summary>
		/// The system logger.
		/// </summary>
		private ILogger _logger;

		/// <summary>
		/// Constructor.
		/// </summary>
		public HangfireLogger(ILogger logger)
		{
			_logger = logger;
		}

		/// <summary>
		/// Logs a message from Hangfire.
		/// </summary>
		public bool Log(HangfireLogLevel logLevel, Func<string> messageFunc, Exception exception = null)
		{
			var systemLogLevel = GetSystemLogLevel(logLevel);

			if (!_logger.IsEnabled(systemLogLevel))
				return false;

			var message = messageFunc != null ? messageFunc() : string.Empty;

			_logger.Log(GetSystemLogLevel(logLevel), 0, message, exception, MessageFormatter);

			return true;
		}

		/// <summary>
		/// Returns the system log level corresponding to a Hangfire log level.
		/// </summary>
		private static SystemLogLevel GetSystemLogLevel(HangfireLogLevel logLevel)
		{
			switch (logLevel)
			{
				case HangfireLogLevel.Debug:
					return SystemLogLevel.Debug;

				case HangfireLogLevel.Trace:
					return SystemLogLevel.Trace;

				case HangfireLogLevel.Info:
					return SystemLogLevel.Information;

				case HangfireLogLevel.Warn:
					return SystemLogLevel.Warning;

				case HangfireLogLevel.Error:
					return SystemLogLevel.Error;

				case HangfireLogLevel.Fatal:
					return SystemLogLevel.Critical;

				default:
					throw new ArgumentOutOfRangeException(nameof(logLevel));
			}
		}

		/// <summary>
		/// The message formatter.
		/// </summary>
		private static string MessageFormatter(object state, Exception error)
		{
			return state.ToString();
		}
	}
}
