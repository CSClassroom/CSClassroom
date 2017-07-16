using Autofac;
using CSC.BuildService.Service.CodeRunner;
using CSC.Common.Infrastructure.Utilities;
using CSC.CSClassroom.Service.Identity;
using CSC.CSClassroom.WebApp.Controllers;
using CSC.CSClassroom.WebApp.Logging;
using CSC.CSClassroom.WebApp.Providers;
using CSC.CSClassroom.WebApp.ServiceClients;
using CSC.CSClassroom.WebApp.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;

namespace CSC.CSClassroom.WebApp.Configuration
{
	/// <summary>
	/// Extension methods for building the IOC container on application start.
	/// </summary>
	public static class ContainerBuilderExtensions
	{
		/// <summary>
		/// Registers dependencies for the CSClassroom webapp.
		/// </summary>
		public static void RegisterCSClassroomWebApp(
			this ContainerBuilder builder, 
			IConfigurationSection webAppSettings,
			IConfigurationSection githubSettings)
		{
			builder.RegisterInstance(GetHostName(webAppSettings)).As<WebAppHost>();
			builder.RegisterInstance(GetEmailAddress(webAppSettings)).As<WebAppEmail>();
			builder.RegisterInstance(GetErrorSettings(webAppSettings)).As<ErrorSettings>();
			builder.RegisterType<HttpContextAccessor>().As<IHttpContextAccessor>().SingleInstance();
			builder.RegisterType<ActionContextAccessor>().As<IActionContextAccessor>();
			builder.RegisterType<IdentityProvider>().As<IIdentityProvider>().InstancePerLifetimeScope();
			builder.RegisterType<TimeZoneProvider>().As<ITimeZoneProvider>().InstancePerLifetimeScope();
			builder.RegisterType<RandomNumberProvider>().As<IRandomNumberProvider>().InstancePerLifetimeScope();
			builder.RegisterType<BaseControllerArgs>().As<BaseControllerArgs>().InstancePerLifetimeScope();
			builder.RegisterType<LogContext>().As<ILogContext>().InstancePerLifetimeScope();
		}

		/// <summary>
		/// Registers dependencies for the code runner service.
		/// </summary>
		public static void RegisterRemoteBuildService(
			this ContainerBuilder builder,
			IConfigurationSection buildServiceSettings)
		{
			builder.RegisterInstance(GetBuildServiceSettings(buildServiceSettings)).As<BuildServiceSettings>();
			builder.RegisterType<CodeRunnerClient>().As<ICodeRunnerService>().InstancePerLifetimeScope();
		}

		/// <summary>
		/// Returns the hostname of the web app.
		/// </summary>
		private static WebAppHost GetHostName(IConfigurationSection webAppSettings)
		{
			return new WebAppHost(webAppSettings["HostName"]);
		}

		/// <summary>
		/// Returns the from address for e-mails sent from the web-app.
		/// </summary>
		private static WebAppEmail GetEmailAddress(IConfigurationSection webAppSettings)
		{
			return new WebAppEmail(webAppSettings["EmailAddress"]);
		}

		/// <summary>
		/// Returns settings about how to display errors to users.
		/// </summary>
		private static ErrorSettings GetErrorSettings(IConfigurationSection webAppSettings)
		{
			return new ErrorSettings(webAppSettings.GetValue<bool>("ShowExceptions"));
		}

		/// <summary>
		/// Returns settings for calling the CodeRunner service.
		/// </summary>
		private static BuildServiceSettings GetBuildServiceSettings(
			IConfigurationSection buildServiceSettings)
		{
			return new BuildServiceSettings(buildServiceSettings["Host"]);
		}
	}
}
