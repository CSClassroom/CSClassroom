using Hangfire.Logging;
using Microsoft.Extensions.Logging;

namespace CSC.Common.Infrastructure.Queue
{
	/// <summary>
	/// A HangFire log provider that uses system logging.
	/// </summary>
	public class HangfireLogProvider : ILogProvider
	{
		/// <summary>
		/// The logger factory.
		/// </summary>
		private readonly ILoggerFactory _loggerFactory;

		/// <summary>
		/// Constructor.
		/// </summary>
		public HangfireLogProvider(ILoggerFactory loggerFactory)
		{
			_loggerFactory = loggerFactory;
		}

		/// <summary>
		/// Returns a Hangfire logger.
		/// </summary>
		public ILog GetLogger(string name)
		{
			return new HangfireLogger(_loggerFactory.CreateLogger(name));
		}
	}
}
