using System;
using System.Linq;
using Autofac;
using CSC.Common.Infrastructure.Async;
using CSC.Common.Infrastructure.Email;
using CSC.Common.Infrastructure.GitHub;
using CSC.Common.Infrastructure.Image;
using CSC.Common.Infrastructure.Queue;
using CSC.Common.Infrastructure.Security;
using CSC.Common.Infrastructure.Serialization;
using CSC.Common.Infrastructure.System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Octokit;

namespace CSC.Common.Infrastructure.Configuration
{
	/// <summary>
	/// Extension methods for building the IOC container on application start.
	/// </summary>
	public static class ContainerBuilderExtensions
	{
		/// <summary>
		/// Registers dependencies for json serialization.
		/// </summary>
		public static void RegisterJsonSerialization(this ContainerBuilder builder, ITypeMapCollection typeMaps)
		{
			builder.RegisterType<JsonSettingsProvider>().As<IJsonSettingsProvider>().InstancePerLifetimeScope();
			builder.RegisterType<ModelSerializer>().As<IJsonSerializer>().InstancePerLifetimeScope();
			builder.RegisterInstance(typeMaps).As<ITypeMapCollection>();
		}

		/// <summary>
		/// Registers the operation runner.
		/// </summary>
		public static void RegisterOperationRunner(this ContainerBuilder builder)
		{
			builder.RegisterType<OperationRunner>().As<IOperationRunner>().InstancePerLifetimeScope();
		}

		/// <summary>
		/// Registers the operation runner.
		/// </summary>
		public static void RegisterJobQueueClient(this ContainerBuilder builder)
		{
			builder.RegisterType<JobQueueClient>().As<IJobQueueClient>().InstancePerLifetimeScope();
		}

		/// <summary>
		/// Registers classes that interact with the system.
		/// </summary>
		public static void RegisterSystem(this ContainerBuilder builder)
		{
			builder.RegisterType<ArchiveFactory>().As<IArchiveFactory>().InstancePerLifetimeScope();
			builder.RegisterType<ProcessRunner>().As<IProcessRunner>().InstancePerLifetimeScope();
			builder.RegisterType<FileSystem>().As<IFileSystem>().InstancePerLifetimeScope();
			builder.RegisterType<TimeProvider>().As<ITimeProvider>().InstancePerLifetimeScope();
		}

		/// <summary>
		/// Registers security-related dependencies.
		/// </summary>
		public static void RegisterSecurity(this ContainerBuilder builder)
		{
			builder.RegisterType<HtmlSanitizer>().As<IHtmlSanitizer>().InstancePerLifetimeScope();
		}

		/// <summary>
		/// Registers an e-mail provider.
		/// </summary>
		public static void RegisterEmailProvider(
			this ContainerBuilder builder,
			IConfigurationSection postmarkSettings,
			IConfigurationSection sendGridSettings,
			IConfigurationSection csClassroomSettings)
		{
			if (postmarkSettings.Exists())
            {
				builder.RegisterInstance
				(
					GetPostmarkConfig(postmarkSettings, csClassroomSettings)
				).As<PostmarkEmailProviderConfig>();
				builder.RegisterType<PostmarkEmailProvider>().As<IEmailProvider>();
			}
			else if (sendGridSettings.Exists())
            {
				builder.RegisterInstance
				(
					CreateSendGridMailProvider(sendGridSettings, csClassroomSettings)
				).As<IEmailProvider>();
			}
			else
            {
				builder.RegisterType<DisabledEmailProvider>().As<IEmailProvider>();
			}
		}

		/// <summary>
		/// Registers an image processor.
		/// </summary>
		public static void RegisterImageProcessor(this ContainerBuilder builder)
		{
			builder.RegisterType<ImageProcessor>().As<IImageProcessor>().InstancePerLifetimeScope();
		}

		/// <summary>
		/// Creates a new SendGrid mail provider.
		/// </summary>
		private static IEmailProvider CreateSendGridMailProvider(
			IConfigurationSection sendGridSettings,
			IConfigurationSection csClassroomSettings)
		{
			return new SendGridEmailProvider(sendGridSettings["ApiKey"], csClassroomSettings["EmailAddress"]);
		}

		/// <summary>
		/// Returns the configuration for the Postmark service.
		/// </summary>
		private static PostmarkEmailProviderConfig GetPostmarkConfig(
			IConfigurationSection postmarkSettings,
			IConfigurationSection csClassroomSettings)
		{
			return new PostmarkEmailProviderConfig
			(
				apiKey: postmarkSettings["ApiKey"],
				defaultFromEmail: csClassroomSettings["EmailAddress"],
				transactionalMessageStream: postmarkSettings["TransactionalMessageStream"],
				broadcastMessageStream: postmarkSettings["BroadcastMessageStream"]
			);

		}

		/// <summary>
		/// Registers GitHub clients.
		/// </summary>
		public static void RegisterGitHubClients(
			this ContainerBuilder builder,
			IConfigurationSection gitHubSettings)
		{
			builder.RegisterInstance(CreateGitHubClient(gitHubSettings)).As<GitHubClient>();
			builder.RegisterInstance(GetGitHubWebhookSecret(gitHubSettings)).As<GitHubWebhookSecret>();
			builder.RegisterType<GitHubUserClient>().As<IGitHubUserClient>().InstancePerLifetimeScope();
			builder.RegisterType<GitHubOrganizationClient>().As<IGitHubOrganizationClient>().InstancePerLifetimeScope();
			builder.RegisterType<GitHubTeamClient>().As<IGitHubTeamClient>().InstancePerLifetimeScope();
			builder.RegisterType<GitHubRepositoryClient>().As<IGitHubRepositoryClient>().InstancePerLifetimeScope();
			builder.RegisterType<GitHubWebhookValidator>().As<IGitHubWebhookValidator>().InstancePerLifetimeScope();
		}

		/// <summary>
		/// Reads the GitHub client configuration.
		/// </summary>
		private static GitHubClient CreateGitHubClient(IConfigurationSection gitHubClientSettings)
		{
			return new GitHubClient(new ProductHeaderValue("CSClassroom"))
			{
				Credentials = new Credentials(gitHubClientSettings["OAuthToken"])
			};
		}

		/// <summary>
		/// Registers the GitHub webhook secret.
		/// </summary>
		private static GitHubWebhookSecret GetGitHubWebhookSecret(IConfigurationSection gitHubSettings)
		{
			return new GitHubWebhookSecret(gitHubSettings["WebhookSecret"]);
		}
	}
}
