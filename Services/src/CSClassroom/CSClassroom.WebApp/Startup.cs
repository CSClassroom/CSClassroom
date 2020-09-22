using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using CSC.Common.Infrastructure.Configuration;
using CSC.Common.Infrastructure.Serialization;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Repository;
using CSC.CSClassroom.Repository.Configuration;
using CSC.CSClassroom.Service.Configuration;
using CSC.CSClassroom.WebApp.Configuration;
using CSC.CSClassroom.WebApp.Logging;
using CSC.CSClassroom.WebApp.ModelBinders;
using CSC.CSClassroom.WebApp.Providers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using ReflectionIT.Mvc.Paging;
using Serilog.Events;

namespace CSC.CSClassroom.WebApp
{
	/// <summary>
	/// Startup class for the web application.
	/// </summary>
	public class Startup
	{
		/// <summary>
		/// The DI container for the application.
		/// </summary>
		private IContainer _container;

		/// <summary>
		/// The logger factory.
		/// </summary>
		private readonly ILoggerFactory _loggerFactory;

		/// <summary>
		/// The configuration for the application.
		/// </summary>
		public readonly IConfiguration _configuration;

		/// <summary>
		/// Constructor.
		/// </summary>
		public Startup(IConfiguration configuration, ILoggerFactory loggerFactory)
		{
			_configuration = configuration;
			_loggerFactory = loggerFactory;
		}

		/// <summary>
		/// Registers all services required for the web application.
		/// This method is called by the runtime.
		/// </summary>
		public IServiceProvider ConfigureServices(IServiceCollection services)
		{
			services
				.AddAuthentication(SetupAuthentication)
				.AddCookie(SetupCookieAuthentication)
				.AddOpenIdConnect(SetupOpenIdConnectAuthentication);

			services.AddTelemetry(_configuration, typeof(CSClassroomTelemetryInitializer));
			services.AddMvc(SetupMvc);

			var builder = new ContainerBuilder();
			
			builder.RegisterJsonSerialization(GetTypeMaps());
			builder.RegisterCSClassroomWebApp(GetSection("CSClassroom"), GetSection("GitHub"));
			builder.RegisterCSClassroomService(GetSection("CSClassroom"));
			builder.RegisterGitHubClients(GetSection("GitHub"));
			builder.RegisterRemoteBuildService(GetSection("BuildService"));
			builder.RegisterPostmarkMailProvider(GetSection("Postmark"), GetSection("CSClassroom"));
			builder.RegisterJobQueueClient();
			builder.RegisterSecurity();
			builder.RegisterSystem();
			builder.RegisterOperationRunner();
			builder.RegisterImageProcessor();
			services.AddCSClassroomDatabase(DatabaseConnectionString);
			services.AddHangfireQueue(DatabaseConnectionString, _loggerFactory);

			builder.Populate(services);
			_container = builder.Build();

			return new AutofacServiceProvider(_container);
		}

		/// <summary>
		/// Configures the HTTP request pipeline. 
		/// This method is called by the runtime.
		/// </summary>
		public void Configure(IApplicationBuilder app)
		{
			ApplyDatabaseMigrations(app);

			app.UseStaticFiles();
			app.UseStatusCodePages();
			app.UseExceptionHandler("/Error");
			app.UseForwardedHeaders(GetForwardedHeadersOptions());
			app.UseAuthentication();
			app.UseMvc();
			app.UseHangfireQueueDashboard(_container);
		}

		/// <summary>
		/// Applies any new database migrations, if needed (creating 
		/// and initializing the database if it does not yet exist).
		/// </summary>
		private static void ApplyDatabaseMigrations(IApplicationBuilder app)
		{
			using (var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
			{
				scope.ServiceProvider.GetService<DatabaseContext>().Database.Migrate();
			}
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
			return _configuration.GetSection(sectionName);
		}

		/// <summary>
		/// Sets up authentication schemes.
		/// </summary>
		private void SetupAuthentication(AuthenticationOptions options)
		{
			options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
			options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
		}

		/// <summary>
		/// Sets up cookie authentication.
		/// </summary>
		private void SetupCookieAuthentication(CookieAuthenticationOptions options)
		{
			options.ExpireTimeSpan = TimeSpan.FromHours(8);
		}

		/// <summary>
		/// Sets up OpenId Connect authentication.
		/// </summary>
		private void SetupOpenIdConnectAuthentication(OpenIdConnectOptions options)
		{
			options.ClientId = _configuration["Authentication:AzureAd:ClientId"];
			options.Authority = _configuration["Authentication:AzureAd:AADInstance"] + "Common";
			options.CallbackPath = _configuration["Authentication:AzureAd:CallbackPath"];
			options.TokenValidationParameters = new TokenValidationParameters
			{
				ValidateIssuer = false,
			};
			options.Events = new OpenIdConnectEvents
			{
				OnTicketReceived = (context) =>
				{
					context.Properties.ExpiresUtc = null;

					return Task.FromResult(0);
				},
				OnAuthenticationFailed = (context) =>
				{
					context.Response.Redirect("/Home/Error");
					context.HandleResponse(); // Suppress the exception
					return Task.FromResult(0);
				},
			};
		}

		/// <summary>
		/// Adds model binders to the MVC configuration.
		/// </summary>
		private void SetupMvc(MvcOptions options)
		{
			// Support binding to abstract models for a set of whitelisted types.
			options.ModelBinderProviders.Insert(0, new AbstractModelBinderProvider
			(
				new List<Type>()
				{
						typeof(Question),
						typeof(CodeQuestionTest),
						typeof(QuestionSubmission)
				})
			);

			options.ModelBinderProviders.Insert(0, new DateTimeModelBinderProvider
			(
				_container.Resolve<ITimeZoneProvider>())
			);
		}

		/// <summary>
		/// Returns a set of type maps that permit deserializing JSON objects into
		/// abstract types.
		/// </summary>
		private ITypeMapCollection GetTypeMaps()
		{
			return new TypeMapCollection()
			{
				[typeof(Question)] = GetTypeMap(typeof(Question)),
				[typeof(QuestionSubmission)] = GetTypeMap(typeof(QuestionSubmission))
			};
		}

		/// <summary>
		/// Returns a type map for a given base type.
		/// </summary>
		private IReadOnlyDictionary<string, Type> GetTypeMap(Type baseType)
		{
			return baseType.GetTypeInfo()
				.Assembly
				.GetTypes()
				.Where
				(
					type => !type.GetTypeInfo().IsAbstract
							&& baseType.IsAssignableFrom(type)
				).ToDictionary(type => type.Name, type => type);
		}

		/// <summary>
		/// Returns options for forwarded headers.
		/// </summary>
		private static ForwardedHeadersOptions GetForwardedHeadersOptions()
		{
			return new ForwardedHeadersOptions
			{
				ForwardedHeaders = ForwardedHeaders.XForwardedProto
			};
		}
	}
}
