using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.Async;
using CSC.Common.Infrastructure.System;
using Microsoft.Extensions.Logging;

namespace CSC.BuildService.Service.Docker
{
	/// <summary>
	/// A host that allows the starting of sibling docker containers.
	/// </summary>
	public class DockerHostFactory : IDockerHostFactory
	{
		/// <summary>
		/// The file system.
		/// </summary>
		private readonly IFileSystem _fileSystem;

		/// <summary>
		/// The process runner.
		/// </summary>
		private readonly IProcessRunner _processRunner;

		/// <summary>
		/// The operation runner.
		/// </summary>
		private readonly IOperationRunner _operationRunner;

		/// <summary>
		/// The host configuration.
		/// </summary>
		private readonly DockerHostConfig _hostConfig;

		/// <summary>
		/// Container configurations for each supported image.
		/// </summary>
		private readonly IList<DockerContainerConfig> _containerConfigs;

		/// <summary>
		/// The logger factory.
		/// </summary>
		private readonly ILoggerFactory _loggerFactory;

		/// <summary>
		/// Constructor.
		/// </summary>
		public DockerHostFactory(
			ILoggerFactory loggerFactory,
			IFileSystem fileSystem,
			IProcessRunner processRunner,
			IOperationRunner operationRunner,
			DockerHostConfig hostConfig,
			IList<DockerContainerConfig> containerConfigs)
		{
			_loggerFactory = loggerFactory;
			_fileSystem = fileSystem;
			_processRunner = processRunner;
			_operationRunner = operationRunner;
			_hostConfig = hostConfig;
			_containerConfigs = containerConfigs;
		}

		/// <summary>
		/// Pulls all container images, so the containers are ready to launch.
		/// </summary>
		public async Task PullContainerImagesAsync()
		{
			foreach (var containerConfig in _containerConfigs)
			{
				await _processRunner.RunProcessAsync
				(
					_hostConfig.DockerLibraryPath,
					new[] {$"pull {containerConfig.ImageName}"},
					timeout: null
				);
			}
		}

		/// <summary>
		/// Creates a docker host, for creating containers with the given 
		/// container config id.
		/// </summary>
		public IDockerHost CreateDockerHost(string containerConfigId)
		{
			return new DockerHost
			(
				_loggerFactory.CreateLogger<DockerHost>(),
				_fileSystem,
				_processRunner,
				_operationRunner,
				_hostConfig,
				_containerConfigs.Single(c => c.Id == containerConfigId),
				() => Guid.NewGuid().ToString()
			);
		}
	}
}
