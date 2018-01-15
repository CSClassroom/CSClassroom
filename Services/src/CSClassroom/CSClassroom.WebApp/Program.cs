using System.IO;
using CSC.Common.Infrastructure.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;

namespace CSC.CSClassroom.WebApp
{
	/// <summary>
	/// The program to run that hosts the webapp.
	/// </summary>
	public class Program
	{
		/// <summary>
		/// Starts the webapp.
		/// </summary>
		public static void Main(string[] args)
		{
			var configuration = GetConfiguration();

			var host = new WebHostBuilder()
				.UseKestrel()
				.UseContentRoot(Directory.GetCurrentDirectory())
				.UseConfiguration(configuration)
				.UseSerilog(CreateLogger(configuration))
				.UseStartup<Startup>()
				.Build();

			host.Run();
		}

		/// <summary>
		/// Returns the configuration for the webapp.
		/// </summary>
		private static IConfigurationRoot GetConfiguration()
		{
			return new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
				.AddJsonFile($"appsettings.Environment.json", optional: true)
				.AddEnvironmentVariables()
				.Build();
		}

		/// <summary>
		/// Creates a logger to use for the build service.
		/// </summary>
		private static ILogger CreateLogger(IConfigurationRoot config)
		{
			return LoggerConfigurationFactory
				.CreateLoggerConfiguration(config, IncludeLogEvent)
				.CreateLogger();
		}

		/// <summary>
		/// Returns whether or not to include a given log event.
		/// </summary>
		private static bool IncludeLogEvent(LogEvent logEvent)
		{
			if (logEvent.Properties.ContainsKey("RequestPath"))
			{
				string requestPath = logEvent.Properties["RequestPath"]
					.ToString()
					.Substring(1);

				if (requestPath.StartsWith("/css")
					|| requestPath.StartsWith("/images")
					|| requestPath.StartsWith("/js")
					|| requestPath.StartsWith("/lib")
					|| requestPath.StartsWith("/markdown")
					|| requestPath.StartsWith("/favicon.ico"))
				{
					return false;
				}
			}

			if (logEvent.Properties.ContainsKey("SourceContext")
					&& logEvent.Properties["SourceContext"]
						.ToString()
						.StartsWith("\"Microsoft.EntityFrameworkCore")
					&& logEvent.Level == LogEventLevel.Information)
			{
				return false;
			}

			return true;
		}
	}
}
