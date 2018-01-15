using System;
using CSC.Common.Infrastructure.Logging;
using CSC.Common.Infrastructure.Queue;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Threading;

namespace CSC.Common.Infrastructure.Configuration
{
	/// <summary>
	/// Extension methods for configuring the logger factory.
	/// </summary>
	public static class ServiceCollectionExtensions
	{
		/// <summary>
		/// How long to wait before retrying if the database does not exist.
		/// </summary>
		private static TimeSpan c_storageRetryDelay = TimeSpan.FromMinutes(1);

		/// <summary>
		/// The code PostgreSql returns if the database does not exist.
		/// </summary>
		private static string c_databaseNotFoundCode = "3D000";

		/// <summary>
		/// Configures logging.
		/// </summary>
		public static void AddTelemetry(
			this IServiceCollection services,
			IConfiguration configuration,
			params Type[] telemetryInitializers)
		{
			services.AddApplicationInsightsTelemetry(configuration);

			// Disable exception logging (as Serilog already sends exceptions
			// to Application Insights).
			var telemetryConfiguration = services.BuildServiceProvider()
				.GetService<TelemetryConfiguration>();
			var builder = telemetryConfiguration.TelemetryProcessorChainBuilder;
			builder.Use((next) => new ExceptionFilterTelemetryProcessor(next));
			builder.Build();

			// Allow code to retrieve the current operation ID. This is useful
			// for sending the operation ID to backend requests.
			services.AddScoped<IOperationIdProvider, OperationIdProvider>();

			// Uses the operation ID in the request header, if any.
			services.AddSingleton(typeof(ITelemetryInitializer), typeof(OperationIdTelemetryInitializer));
			
			// Includes the hostname with each log entry.
			services.AddSingleton(typeof(ITelemetryInitializer), typeof(HostnameTelemetryInitializer));

			foreach (var type in telemetryInitializers)
			{
				services.AddSingleton(typeof(ITelemetryInitializer), type);
			}
		}

		/// <summary>
		/// Registers the hangfire queue.
		/// </summary>
		public static void AddHangfireQueue(
			this IServiceCollection services,
			string connectionString,
			ILoggerFactory loggerFactory)
		{
			services.AddHangfire
			(
				config =>
				{
					var logProvider = new HangfireLogProvider(loggerFactory);
					var storage = RetryGetHangfireStorage(connectionString);

					config
						.UseLogProvider(logProvider)
						.UseStorage(storage);
				}
			);
		}

		/// <summary>
		/// Makes one attempt to retry connecting to the database after a failed attempt,
		/// before giving up.
		/// </summary>
		private static PostgreSqlStorage RetryGetHangfireStorage(string connectionString)
		{
			try
			{
				return GetHangfireStorage(connectionString);
			}
			catch (PostgresException ex) when (ex.SqlState == c_databaseNotFoundCode)
			{
				// This could happen if both the webapp and buildservice are starting 
				// simultaneously, if the buildservice tries to connect before the webapp 
				// creates the database. Give the webapp time to create the database.
				Thread.Sleep(c_storageRetryDelay);

				return GetHangfireStorage(connectionString);
			}
		}

		/// <summary>
		/// Returns the storage configuration for Hangfire.
		/// </summary>
		private static PostgreSqlStorage GetHangfireStorage(string connectionString)
		{
			return new PostgreSqlStorage
			(
				connectionString,
				new PostgreSqlStorageOptions()
				{
					QueuePollInterval = TimeSpan.FromSeconds(1)
				}
			);
		}
	}
}
