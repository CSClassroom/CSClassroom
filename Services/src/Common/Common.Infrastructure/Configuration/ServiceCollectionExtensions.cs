using System;
using CSC.Common.Infrastructure.Logging;
using CSC.Common.Infrastructure.Queue;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CSC.Common.Infrastructure.Configuration
{
	/// <summary>
	/// Extension methods for configuring the logger factory.
	/// </summary>
	public static class ServiceCollectionExtensions
	{
		/// <summary>
		/// Configures logging.
		/// </summary>
		public static void AddTelemetry(
			this IServiceCollection services,
			IConfigurationRoot configuration,
			params Type[] telemetryInitializers)
		{
			services.AddApplicationInsightsTelemetry(configuration);

			services.AddScoped<IOperationIdProvider, OperationIdProvider>();

			services.AddSingleton(typeof(ITelemetryInitializer), typeof(OperationIdTelemetryInitializer));
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

					var storage = new PostgreSqlStorage
					(
						connectionString,
						new PostgreSqlStorageOptions()
						{
							QueuePollInterval = TimeSpan.FromSeconds(1)
						}
					);

					config
						.UseLogProvider(logProvider)
						.UseStorage(storage);
				}
			);
		}
	}
}
