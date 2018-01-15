using System.IO;
using CSC.Common.Infrastructure.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace CSC.BuildService.Endpoint
{
	/// <summary>
	/// The program to run that hosts the endpoint.
	/// </summary>
	public class Program 
	{
		/// <summary>
		/// Starts the endpoint.
		/// </summary>
		public static void Main(string[] args)
		{
			var configuration = GetConfiguration();

			var host = new WebHostBuilder()
				.UseKestrel()
				.UseContentRoot(Directory.GetCurrentDirectory())
				.UseIISIntegration()
				.UseConfiguration(configuration)
				.UseSerilog(CreateLogger(configuration))
				.UseStartup<Startup>()
				.Build();

			host.Run();
		}

		/// <summary>
		/// Returns the configuration for the build service.
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
				.CreateLoggerConfiguration(config, logEvent => true)
				.CreateLogger();
		}
	}
}
