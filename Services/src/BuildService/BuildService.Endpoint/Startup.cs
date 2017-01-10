using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using CSC.BuildService.Endpoint.Queue;
using CSC.BuildService.Service.Configuration;
using CSC.BuildService.Service.Docker;
using CSC.Common.Infrastructure.Configuration;
using CSC.Common.Infrastructure.Serialization;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CSC.BuildService.Endpoint
{
	/// <summary>
	/// Startup class for the build service.
	/// </summary>
	public class Startup
	{
		/// <summary>
		/// The configuration.
		/// </summary>
		public IConfigurationRoot Configuration { get; }

		/// <summary>
		/// The logger factory.
		/// </summary>
		private readonly ILoggerFactory _loggerFactory;

		/// <summary>
		/// The IOC container.
		/// </summary>
		private IContainer _container;

		/// <summary>
		/// Constructor.
		/// </summary>
		public Startup(IHostingEnvironment env, ILoggerFactory loggerFactory)
		{
			_loggerFactory = loggerFactory;

			var builder = new ConfigurationBuilder()
				.SetBasePath(env.ContentRootPath)
				.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
				.AddJsonFile($"appsettings.Environment.json", optional: true)
				.AddEnvironmentVariables();

			Configuration = builder.Build();
		}

		/// <summary>
		/// Called by the runtime to configure services.
		/// </summary>
		public IServiceProvider ConfigureServices(IServiceCollection services)
		{
			services.AddMvc().AddJsonOptions(ApplyJsonOptions);
			services.AddTelemetry(Configuration);
			services.AddSingleton(typeof(IHttpContextAccessor), typeof(HttpContextAccessor));
			services.AddHangfireQueue(DatabaseConnectionString, _loggerFactory);
			
			var builder = new ContainerBuilder();

			builder.RegisterDockerHostFactory(GetSection("DockerHost"), GetSection("DockerContainers"));
			builder.RegisterJsonSerialization(new TypeMapCollection());
			builder.RegisterBuildService(Configuration.GetSection("ProjectRunner"));
			builder.RegisterSystem();
			builder.RegisterOperationRunner();

			builder.Populate(services);

			_container = builder.Build();

			return new AutofacServiceProvider(_container);
		}

		/// <summary>
		/// Called by the runtime to configure the HTTP request pipeline.
		/// </summary>
		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			app.UseTelemetry(_loggerFactory, Configuration, logEvent => true);
			app.UseExceptionHandler("/Error");
			app.UseHangfireServer(GetBackgroundJobServerOptions());
			app.UseMvc();

			_container
				.Resolve<IDockerHostFactory>()
				.PullContainerImagesAsync()
				.Wait();
		}

		/// <summary>
		/// The database connection string.
		/// </summary>
		private string DatabaseConnectionString
			=> GetSection("ConnectionStrings")["PostgresDefaultConnection"];

		/// <summary>
		/// Returns the given configuration section.
		/// </summary>
		private IConfigurationSection GetSection(string sectionName)
		{
			return Configuration.GetSection(sectionName);
		}

		/// <summary>
		/// Applies json options to all requets to the service.
		/// </summary>
		private static void ApplyJsonOptions(MvcJsonOptions jsonOptions)
		{
			var jsonSettingsProvider = new JsonSettingsProvider(new TypeMapCollection());
			jsonSettingsProvider.PopulateSettings(jsonOptions.SerializerSettings);
		}

		/// <summary>
		/// Returns options for the queue worker.
		/// </summary>
		private BackgroundJobServerOptions GetBackgroundJobServerOptions()
		{
			return new BackgroundJobServerOptions()
			{
				WorkerCount = 1,
				Activator = new ContainerJobActivator(_container)
			};
		}
	}
}
