using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CSC.Common.Service.Docker
{
	/// <summary>
	/// A host that allows the starting of sibling docker containers.
	/// </summary>
	public class DockerHost : IDockerHost
    {
		/// <summary>
		/// The configuration of the docker host.
		/// </summary>
		private IDockerHostConfig _hostConfig;

		/// <summary>
		/// The configuration of the docker container to start.
		/// </summary>
		private IDockerContainerConfig _containerConfig;

		/// <summary>
		/// The request file path environment variable.
		/// </summary>
		private const string c_requestFilePathVar = "REQUEST_FILE_PATH";

		/// <summary>
		/// The response file path environment variable.
		/// </summary>
		private const string c_responseFilePathVar = "RESPONSE_FILE_PATH";

		/// <summary>
		/// Constructor.
		/// </summary>
		public DockerHost(IDockerHostConfig hostConfig, IDockerContainerConfig containerConfig)
		{
			_hostConfig = hostConfig;
			_containerConfig = containerConfig;
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
		public async Task<IDockerResult> RunImageInNewContainerAsync(
			string requestContents,
			IDictionary<string, string> environmentVariables)
		{
			var folderName = CreateRequestResponseFolder();

			WriteRequestFile(folderName, requestContents);
			var processResult = await RunContainerAsync(folderName, environmentVariables);
			var responseContents = processResult.Completed ? ReadResponseFile(folderName) : null;

			CleanupFolder(folderName);

			return new DockerResult(processResult.Completed, processResult.Output, responseContents);
		}

		/// <summary>
		/// Creates a folder that will store the request and response files,
		/// and returns the folder name.
		/// </summary>
		private string CreateRequestResponseFolder()
		{
			var folderName = Guid.NewGuid().ToString();
			Directory.CreateDirectory($"{_hostConfig.ContainerTempFolderPath}/{folderName}");

			return folderName;
		}

		/// <summary>
		/// Writes the request file to the file system.
		/// </summary>
		private void WriteRequestFile(string folderName, string requestFileContents)
		{
			var filePath = $"{_hostConfig.ContainerTempFolderPath}/{folderName}/{_containerConfig.RequestFileName}";

			File.WriteAllText(filePath, requestFileContents);
		}

		/// <summary>
		/// Runs the container. Returns the merged contents of stdout/stderr.
		/// </summary>
		private async Task<ProcessResult> RunContainerAsync(
			string folderName,
			IDictionary<string, string> environmentVariables)
		{
			var dockerArgs = GetDockerArguments(folderName, environmentVariables);
			var output = new ConcurrentQueue<string>();
			var processTcs = new TaskCompletionSource<bool>();

			var process = new Process()
			{
				StartInfo = new ProcessStartInfo()
				{
					FileName = _hostConfig.DockerLibraryPath,
					Arguments = string.Join(" ", dockerArgs),
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					UseShellExecute = false,
					CreateNoWindow = true
				},

				EnableRaisingEvents = true
			};

			process.OutputDataReceived += (sender, e) =>
			{
				output.Enqueue(e.Data);
			};

			process.ErrorDataReceived += (sender, e) =>
			{
				output.Enqueue(e.Data);
			};

			process.Exited += (sender, e) =>
			{
				processTcs.TrySetResult(true);
			};

			process.Start();
			process.BeginOutputReadLine();
			process.BeginErrorReadLine();

			await Task.WhenAny(processTcs.Task, Task.Delay(_containerConfig.MaxLifetime));
			await Task.Delay(20);

			if (!process.HasExited)
			{
				process.Kill();
				return new ProcessResult(false /*succeeded*/, string.Join("\n", output));
			}
			else
			{
				return new ProcessResult(true /*succeeded*/, string.Join("\n", output));
			}
		}

		/// <summary>
		/// Returns an array of command line arguments to docker.
		/// </summary>
		private string[] GetDockerArguments(
			string folderName, 
			IDictionary<string, string> environmentVariables)
		{
			return new[]
			{
				// Run the container
				"run",

				// Limit the relative CPU weight to one eighth of a default container
				"--cpu-shares 128", 

				// Mount the subfolder that will contain the request and response files
				$"-v /{_hostConfig.HostTempFolderPath}/{folderName}:{_containerConfig.RequestResponseMountPoint}",
				
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
		private string ReadResponseFile(string folderName)
		{
			var filePath = $"{_hostConfig.ContainerTempFolderPath}/{folderName}/{_containerConfig.ResponseFileName}";

			try
			{
				return File.ReadAllText(filePath);
			}
			catch (FileNotFoundException)
			{
				return null;
			}
		}

		/// <summary>
		/// Removes the folder we created.
		/// </summary>
		private void CleanupFolder(string folderName)
		{
			Directory.Delete($"{_hostConfig.ContainerTempFolderPath}/{folderName}", recursive: true);
		}
	}
}
