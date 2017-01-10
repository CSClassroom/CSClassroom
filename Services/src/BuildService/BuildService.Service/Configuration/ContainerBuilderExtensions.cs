using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using CSC.BuildService.Service.CodeRunner;
using CSC.BuildService.Service.Docker;
using CSC.BuildService.Service.ProjectRunner;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CSC.BuildService.Service.Configuration
{
	/// <summary>
	/// Extension methods for building the IOC container on application start.
	/// </summary>
	public static class ContainerBuilderExtensions
	{
		/// <summary>
		/// Registers dependencies for the build service.
		/// </summary>
		public static void RegisterBuildService(
			this ContainerBuilder builder,
			IConfigurationSection projectRunnerSettings)
		{
			builder.RegisterType<CodeRunnerService>().As<ICodeRunnerService>();
			builder.RegisterType<ProjectJobResultNotifier>().As<IProjectJobResultNotifier>();
			builder.RegisterType<ProjectRunnerService>().As<IProjectRunnerService>();
			builder.RegisterInstance(ReadProjectRunnerConfiguration(projectRunnerSettings))
				.As<IProjectRunnerServiceConfig>();
		}


		/// <summary>
		/// Registers dependencies for a docker host.
		/// </summary>
		/// <param name="builder">The IOC container builder.</param>
		/// <param name="dockerHostSettings">The settings for the docker host.</param>
		/// <param name="dockerContainerSettings">The settings for containers that
		/// will be launched on the docker host.</param>
		public static void RegisterDockerHostFactory(
			this ContainerBuilder builder,
			IConfigurationSection dockerHostSettings,
			IConfigurationSection dockerContainerSettings)
		{
			var hostSettings = ReadDockerHostSettings(dockerHostSettings);
			var containerSettings = dockerContainerSettings
				.GetChildren()
				.Select(ReadDockerContainerSettings)
				.ToList();

			builder.RegisterInstance(hostSettings).As<DockerHostConfig>();
			builder.RegisterType<DockerHostFactory>().As<IDockerHostFactory>().SingleInstance();
			builder.RegisterInstance(containerSettings).As<IList<DockerContainerConfig>>();
		}

		/// <summary>
		/// Reads the Docker host configuration.
		/// </summary>
		private static DockerHostConfig ReadDockerHostSettings(
			IConfigurationSection hostSettings)
		{
			return new DockerHostConfig
			(
				hostSettings["DockerLibraryPath"],
				hostSettings["HostTempFolderPath"],
				hostSettings["ContainerTempFolderPath"]
			);
		}

		/// <summary>
		/// Reads the Docker container configuration.
		/// </summary>
		private static DockerContainerConfig ReadDockerContainerSettings(
			IConfigurationSection containerSettings)
		{
			return new DockerContainerConfig
			(
				containerSettings["Id"],
				containerSettings["ImageName"],
				containerSettings["RequestResponseMountPoint"],
				containerSettings["RequestFileName"],
				containerSettings["ResponseFileName"],
				TimeSpan.Parse(containerSettings["MaxLifetime"])
			);
		}

		/// <summary>
		/// Reads the project runner service configuration.
		/// </summary>
		private static IProjectRunnerServiceConfig ReadProjectRunnerConfiguration(
			IConfigurationSection projectRunnerSettings)
		{
			return new ProjectRunnerServiceConfig
			(
				projectRunnerSettings["GitHubOAuthToken"],
				projectRunnerSettings["ProjectJobResultHost"]
			);
		}
	}
}
