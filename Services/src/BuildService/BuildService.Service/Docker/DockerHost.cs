using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
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
	public class DockerHost : IDockerHost
	{
		/// <summary>
		/// The logger.
		/// </summary>
		private readonly ILogger _logger;

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
		/// The configuration of the docker host.
		/// </summary>
		private readonly DockerHostConfig _hostConfig;

		/// <summary>
		/// The configuration of the docker container to start.
		/// </summary>
		private readonly DockerContainerConfig _containerConfig;

		/// <summary>
		/// Generates unique container IDs.
		/// </summary>
		private readonly Func<string> _containerNameGenerator;

		/// <summary>
		/// The request file path environment variable.
		/// </summary>
		private const string c_requestFilePathVar = "REQUEST_FILE_PATH";

		/// <summary>
		/// The response file path environment variable.
		/// </summary>
		private const string c_responseFilePathVar = "RESPONSE_FILE_PATH";

		/// <summary>
		/// The number of times we will attempt to read the response file.
		/// </summary>
		private const int c_readResponseRetryAttempts = 5;

		/// <summary>
		/// The number of times we will attempt to kill the docker container.
		/// </summary>
		private const int c_killDockerContainerRetryAttempts = 15;

		/// <summary>
		/// The delay after the process ends.
		/// </summary>
		private const int c_waitForProcessExitDelay = 100;

		/// <summary>
		/// The delay between read attempts.
		/// </summary>
		private readonly TimeSpan c_retryAttemptDelay = TimeSpan.FromSeconds(1);

		/// <summary>
		/// Constructor.
		/// </summary>
		public DockerHost(
			ILogger<DockerHost> logger, 
			IFileSystem fileSystem,
			IProcessRunner processRunner,
			IOperationRunner operationRunner,
			DockerHostConfig hostConfig, 
			DockerContainerConfig containerConfig,
			Func<string> containerNameGenerator)
		{
			_logger = logger;
			_fileSystem = fileSystem;
			_processRunner = processRunner;
			_operationRunner = operationRunner;
			_hostConfig = hostConfig;
			_containerConfig = containerConfig;
			_containerNameGenerator = containerNameGenerator;
		}

		/// <summary>
		/// Runs a container. The request is written to a folder mounted in
		/// the container, at a location determined by the configuration. 
		/// The container is expected to write the response file prior to 
		/// completion. This method returns a task that completes when the 
		/// container finishes running.
		/// </summary>
		/// <param name="requestContents">The request contents.</param>
		/// <param name="environmentVariables">The environment variables.</param>
		/// <returns>The response file (if any), and the contents of stdout/stderr.</returns>
		public async Task<DockerResult> RunImageInNewContainerAsync(
			string requestContents,
			IDictionary<string, string> environmentVariables)
		{
			var containerName = _containerNameGenerator();

			CreateRequestResponseFolder(containerName);

			if (requestContents != null)
			{
				await WriteRequestFile(containerName, requestContents);
			}

			var processResult = await RunContainerAsync(containerName, environmentVariables);

			var responseContents = processResult.Completed 
				? await ReadResponseFileAsync(containerName)
				: null;

			CleanupFolder(containerName);

			return new DockerResult
			(
				processResult.Completed, 
				processResult.Output, 
				!string.IsNullOrEmpty(responseContents)
					? responseContents
					: null
			);
		}

		/// <summary>
		/// Creates a folder that will store the request and response files,
		/// and returns the folder name.
		/// </summary>
		private void CreateRequestResponseFolder(string containerName)
		{
			_fileSystem.CreateFolder($"{_hostConfig.ContainerTempFolderPath}/{containerName}");
		}

		/// <summary>
		/// Writes the request file to the file system.
		/// </summary>
		private async Task WriteRequestFile(string containerName, string requestFileContents)
		{
			var filePath = $"{_hostConfig.ContainerTempFolderPath}/{containerName}/{_containerConfig.RequestFileName}";

			await _fileSystem.WriteFileContentsAsync(filePath, requestFileContents);
		}

		/// <summary>
		/// Runs the container. Returns the merged contents of stdout/stderr.
		/// </summary>
		private async Task<ProcessResult> RunContainerAsync(
			string containerName,
			IDictionary<string, string> environmentVariables)
		{
			var dockerArgs = GetDockerArguments(containerName, environmentVariables);

			var processResult = await _processRunner.RunProcessAsync
			(
				_hostConfig.DockerLibraryPath,
				dockerArgs,
				_containerConfig.MaxLifetime
			);

			await Task.Delay(c_waitForProcessExitDelay);

			if (!processResult.Completed)
			{
				await _operationRunner.RetryOperationIfNeededAsync
				(
					async () => await _processRunner.RunProcessAsync
					(
						_hostConfig.DockerLibraryPath,
						new[] {$"rm -f {containerName}"},
						timeout: null
					),
					exception => true,
					c_killDockerContainerRetryAttempts,
					c_retryAttemptDelay,
					defaultResultIfFailed: false
				);
			}

			return processResult;
		}

		/// <summary>
		/// Returns an array of command line arguments to docker.
		/// </summary>
		private string[] GetDockerArguments(
			string containerName, 
			IDictionary<string, string> environmentVariables)
		{
			return new[]
			{
				// Run the container
				"run",

				// Name the container, so we can kill it if necessary
				$"--name {containerName}",

				// Limit the relative CPU weight to one eighth of a default container
				"--cpu-shares 128", 

				// Mount the subfolder that will contain the request and response files
				$"-v /{_hostConfig.HostTempFolderPath}/{containerName}:{_containerConfig.RequestResponseMountPoint}",
				
				// Set an environment variable containing the request file path
				$"-e \"{c_requestFilePathVar}={_containerConfig.RequestResponseMountPoint}/{_containerConfig.RequestFileName}\"",

				// Set the environment variable containing the response file path
				$"-e \"{c_responseFilePathVar}={_containerConfig.RequestResponseMountPoint}/{_containerConfig.ResponseFileName}\"",

				// Remove the container once it finishes executing
				"--rm"

			}.Concat
			(
				environmentVariables.Select
				(
					// Set all passed-in environment variables.
					kvp => $"-e \"{kvp.Key}={kvp.Value}\""
				)
			).Concat
			(
				new[]
				{
					// Set the image name.
					_containerConfig.ImageName
				}
			).ToArray();
		}

		/// <summary>
		/// Reads the response file from the file system. Returns 
		/// null if the file was not written by the container.
		/// </summary>
		private async Task<string> ReadResponseFileAsync(string folderName)
		{
			var filePath = $"{_hostConfig.ContainerTempFolderPath}/{folderName}/{_containerConfig.ResponseFileName}";

			return await _operationRunner.RetryOperationIfNeededAsync
			(
				async () => await _fileSystem.ReadFileContentsAsync(filePath),
				exception => true,
				c_readResponseRetryAttempts,
				c_retryAttemptDelay,
				defaultResultIfFailed: true
			);
		}

		/// <summary>
		/// Removes the folder we created.
		/// </summary>
		private void CleanupFolder(string folderName)
		{
			_fileSystem.DeleteFolder
			(
				$"{_hostConfig.ContainerTempFolderPath}/{folderName}"
			);
		}
	}
}
