using System;
using Autofac;
using CSC.CSClassroom.Service.Identity;
using Hangfire;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Builder;

namespace CSC.CSClassroom.WebApp.Configuration
{
	/// <summary>
	/// An authorization filter for the Hangfire dashboard.
	/// </summary>
	public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
	{
		/// <summary>
		/// The user provider.
		/// </summary>
		private readonly IContainer _container;

		/// <summary>
		/// Constructor.
		/// </summary>
		public HangfireAuthorizationFilter(IContainer container)
		{
			_container = container;
		}

		/// <summary>
		/// Authorizes access to the hangfire dashboard.
		/// </summary>
		public bool Authorize(DashboardContext context)
		{
			using (var lifetimeScope = _container.BeginLifetimeScope())
			{
				return lifetimeScope
					.Resolve<IUserProvider>()
					.GetCurrentIdentityStateAsync()
					.Result == IdentityState.SuperUser;
			}
		}
	}

	/// <summary>
	/// Extension methods for building the IOC container on application start.
	/// </summary>
	public static class ApplicationBuilderExtensions
	{
		/// <summary>
		/// Registers dependencies for CSClassroom services.
		/// </summary>
		public static void UseHangfireQueueDashboard(this IApplicationBuilder builder, IContainer container)
		{
			builder.UseHangfireDashboard("/BuildQueue", new DashboardOptions()
			{
				Authorization = new[] { new HangfireAuthorizationFilter(container) }
			});
		}
	}
}
