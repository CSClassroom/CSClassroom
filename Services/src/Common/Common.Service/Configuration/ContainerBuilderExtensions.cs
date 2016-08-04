using Autofac;
using CSC.Common.Service.Docker;
using CSC.Common.Service.Serialization;
using Microsoft.Extensions.Configuration;

namespace CSC.Common.Service.Configuration
{
	/// <summary>
	/// Extension methods for building the IOC container on application start.
	/// </summary>
    public static class ContainerBuilderExtensions
    {
		/// <summary>
		/// Registers dependencies for a docker host.
		/// </summary>
		/// <param name="builder">The IOC container builder.</param>
		/// <param name="dockerHostSettings">The settings for the docker host.</param>
		/// <param name="dockerContainerSettings">The settings for containers that
		/// will be launched on the docker host.</param>
		public static void RegisterDockerHost(
			this ContainerBuilder builder,
			IConfigurationSection dockerHostSettings,
			IConfigurationSection dockerContainerSettings)
		{
			builder.RegisterType<DockerHost>().As<IDockerHost>();
			builder.RegisterInstance(new DockerHostConfig(dockerHostSettings)).As<IDockerHostConfig>();
			builder.RegisterInstance(new DockerContainerConfig(dockerContainerSettings)).As<IDockerContainerConfig>();
		}

		/// <summary>
		/// Registers dependencies for json serialization.
		/// </summary>
		public static void RegisterJsonSerialization(this ContainerBuilder builder)
		{
			builder.RegisterType<JsonSettingsProvider>().As<IJsonSettingsProvider>();
			builder.RegisterType<JsonConverter>().As<IJsonConverter>();
		}
    }
}
