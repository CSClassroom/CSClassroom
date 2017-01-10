using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSC.BuildService.Service.Docker;
using CSC.Common.Infrastructure.Async;
using CSC.Common.Infrastructure.System;
using CSC.Common.TestDoubles;
using Moq;
using Microsoft.Extensions.Logging;
using Xunit;

namespace CSC.BuildService.Service.UnitTests.Docker
{
	/// <summary>
	/// UnitTests for the DockerHost class.
	/// </summary>
	public class DockerHost_UnitTests
	{
		/// <summary>
		/// Ensures that RunImageInNewContainerAsync always creates a folder
		/// that contains the request (if any) and will contain the response,
		/// before it runs the container.
		/// </summary>
		[Fact]
		public async Task RunImageInNewContainerAsync_CreatesFolderBeforeRunningContainer()
		{
			bool folderCreated = false;
			var fileSystem = GetMockFileSystem();

			var processRunner = GetMockProcessRunner
			(
				() =>
				{
					if (fileSystem.Folders.ContainsKey("ContainerTempFolder/ContainerName")
						&& fileSystem.Folders.Count == 1)
					{
						folderCreated = true;
					}
				}
			);

			var dockerHost = GetDockerHost(processRunner, fileSystem);
			await dockerHost.RunImageInNewContainerAsync
			(
				"RequestContents",
				new Dictionary<string, string>()
			);

			Assert.True(folderCreated);
		}

		/// <summary>
		/// Ensures that RunImageInNewContainerAsync writes the request 
		/// to a file specified by the docker host config, if a request
		/// is passed in.
		/// </summary>
		[Fact]
		public async Task RunImageInNewContainerAsync_RequestExists_WritesRequestBeforeRunningContainer()
		{
			string requestContents = null;
			var fileSystem = GetMockFileSystem();

			var processRunner = GetMockProcessRunner
			(
				async () =>
				{
					requestContents = await fileSystem.ReadFileContentsAsync
					(
						"ContainerTempFolder/ContainerName/RequestFileName"
					);
				}
			);

			var dockerHost = GetDockerHost(processRunner, fileSystem);
			await dockerHost.RunImageInNewContainerAsync
			(
				"RequestContents",
				new Dictionary<string, string>()
			);

			Assert.Equal("RequestContents", requestContents);
		}

		/// <summary>
		/// Ensures that RunImageInNewContainerAsync does not write any file
		/// to the file system before running the container if no request is 
		/// passed in.
		/// </summary>
		[Fact]
		public async Task RunImageInNewContainerAsync_NoRequestExists_NoFileWrittenBeforeRunningContainer()
		{
			bool confirmedFilesNotWritten = false;
			var fileSystem = GetMockFileSystem();

			var processRunner = GetMockProcessRunner
			(
				() =>
				{
					confirmedFilesNotWritten = fileSystem
						.Folders["ContainerTempFolder/ContainerName"]
						.Count == 0;
				}
			);

			var dockerHost = GetDockerHost(processRunner, fileSystem);
			await dockerHost.RunImageInNewContainerAsync
			(
				null /*requestContents*/,
				new Dictionary<string, string>()
			);

			Assert.True(confirmedFilesNotWritten);
		}

		/// <summary>
		/// Ensures that RunImageInNewContainerAsync runs a container
		/// with the correct image
		/// </summary>
		[Fact]
		public async Task RunImageInNewContainerAsync_RunsContainerWithCorrectImage()
		{
			var processRunner = GetMockProcessRunner();

			var dockerHost = GetDockerHost(processRunner);
			await dockerHost.RunImageInNewContainerAsync
			(
				"RequestContents",
				new Dictionary<string, string>()
			);

			Assert.Equal(1, processRunner.Launches.Count);
			Assert.Equal("DockerLib", processRunner.Launches[0].ProcessPath);
			Assert.True(processRunner.Launches[0].Args.First() == "run");
			Assert.True(processRunner.Launches[0].Args.Last() == "ImageName");
		}

		/// <summary>
		/// Ensures that RunImageInNewContainerAsync creates a container with
		/// a specific unique name (separate from the image name). The name allows
		/// the container to be easily killed if it runs for too long.
		/// </summary>
		[Fact]
		public async Task RunImageInNewContainerAsync_ContainerNameSet()
		{
			var processRunner = GetMockProcessRunner();

			var dockerHost = GetDockerHost(processRunner);
			await dockerHost.RunImageInNewContainerAsync
			(
				"RequestContents",
				new Dictionary<string, string>()
			);

			Assert.True(processRunner.Launches[0].Args.Contains($"--name ContainerName"));
		}

		/// <summary>
		/// Ensures that RunImageInNewContainerAsync mounts the correct
		/// folder, from which it will read the request file, and to which
		/// it will write the response file.
		/// </summary>
		[Fact]
		public async Task RunImageInNewContainerAsync_MountsRequestResponseFolder()
		{
			var processRunner = GetMockProcessRunner();

			var dockerHost = GetDockerHost(processRunner);
			await dockerHost.RunImageInNewContainerAsync
			(
				"RequestContents",
				new Dictionary<string, string>()
			);

			var expectedMountPoint = "/HostTempFolder/ContainerName:MountPoint";

			Assert.True(processRunner.Launches[0].Args.Contains($"-v {expectedMountPoint}"));
		}

		/// <summary>
		/// Ensures that RunImageInNewContainerAsync sets specific environment
		/// variables that contain the paths to the request file and desired
		/// response file, so the container can know where to find the request 
		/// and where to put its response.
		/// </summary>
		[Fact]
		public async Task RunImageInNewContainerAsync_SetsRequestResponseEnvironmentVariables()
		{
			var processRunner = GetMockProcessRunner();

			var dockerHost = GetDockerHost(processRunner);
			await dockerHost.RunImageInNewContainerAsync
			(
				"RequestContents",
				new Dictionary<string, string>()
			);

			var requestVar = "REQUEST_FILE_PATH=MountPoint/RequestFileName";
			var responseVar = "RESPONSE_FILE_PATH=MountPoint/ResponseFileName";

			Assert.True(processRunner.Launches[0].Args.Contains($"-e \"{requestVar}\""));
			Assert.True(processRunner.Launches[0].Args.Contains($"-e \"{responseVar}\""));
		}


		/// <summary>
		/// Ensures that RunImageInNewContainerAsync sets any custom 
		/// environment variables that are passed to it.
		/// </summary>
		[Fact]
		public async Task RunImageInNewContainerAsync_SetsCustomEnvironmentVariables()
		{
			var processRunner = GetMockProcessRunner();

			var dockerHost = GetDockerHost(processRunner);
			await dockerHost.RunImageInNewContainerAsync
			(
				"RequestContents",
				new Dictionary<string, string>()
				{
					["Var1"] = "Var1Value",
					["Var2"] = "Var2Value"
				}
			);

			Assert.True(processRunner.Launches[0].Args.Contains($"-e \"Var1=Var1Value\""));
			Assert.True(processRunner.Launches[0].Args.Contains($"-e \"Var2=Var2Value\""));
		}

		/// <summary>
		/// Ensures that RunImageInNewContainerAsync returns the output
		/// of the container.
		/// </summary>
		[Fact]
		public async Task RunImageInNewContainerAsync_ContainerCompletes_ReturnsOutput()
		{
			var processRunner = GetMockProcessRunner
			(
				completed: true,
				output: "Output"	
			);

			var dockerHost = GetDockerHost(processRunner);
			var result = await dockerHost.RunImageInNewContainerAsync
			(
				"RequestContents",
				new Dictionary<string, string>()
			);

			Assert.Equal("Output", result.Output);
		}

		/// <summary>
		/// Ensures that RunImageInNewContainerAsync returns the response file
		/// written by the container, if such a response was written.
		/// </summary>
		[Fact]
		public async Task RunImageInNewContainerAsync_ContainerCompletesWithResponse_ReturnsResponse()
		{
			var fileSystem = GetMockFileSystem();

			var processRunner = GetMockProcessRunner
			(
				async () =>
				{
					await fileSystem.WriteFileContentsAsync
					(
						"ContainerTempFolder/ContainerName/ResponseFileName",
						"ResponseContents"
					);

					return new ProcessResult(completed: true, output: "Output");
				}
			);

			var dockerHost = GetDockerHost(processRunner, fileSystem);
			var result = await dockerHost.RunImageInNewContainerAsync
			(
				"RequestContents",
				new Dictionary<string, string>()
			);

			Assert.Equal("ResponseContents", result.Response);
		}

		/// <summary>
		/// Ensures that RunImageInNewContainerAsync returns a null response,
		/// if no response was written by the container.
		/// </summary>
		[Fact]
		public async Task RunImageInNewContainerAsync_ContainerCompletesWithoutResponse_NoResponseReturned()
		{
			var processRunner = GetMockProcessRunner(completed: true, output: "Output");

			var dockerHost = GetDockerHost(processRunner);
			var result = await dockerHost.RunImageInNewContainerAsync
			(
				"RequestContents",
				new Dictionary<string, string>()
			);

			Assert.Null(result.Response);
		}
		
		/// <summary>
		/// Ensures that RunImageInNewContainerAsync creates a container
		/// that will automatically be destroyed if it completes in the
		/// desired timeout.
		/// </summary>
		[Fact]
		public async Task RunImageInNewContainerAsync_ContainerCompletes_AutoDestructed()
		{
			var processRunner = GetMockProcessRunner();

			var dockerHost = GetDockerHost(processRunner);
			await dockerHost.RunImageInNewContainerAsync
			(
				"RequestContents",
				new Dictionary<string, string>()
			);

			Assert.True(processRunner.Launches[0].Args.Contains($"--rm"));
		}

		/// <summary>
		/// Ensures that RunImageInNewContainerAsync manually kills
		/// a container that times out.
		/// </summary>
		[Fact]
		public async Task RunImageInNewContainerAsync_ContainerTimesOut_ManuallyDestructed()
		{
			var processRunner = GetMockProcessRunner
			(
				() => Task.FromResult
				(
					new ProcessResult(completed: false, output: "Output1")
				),
				() => Task.FromResult
				(
					new ProcessResult(completed: true, output: "Output2")
				)
			);

			var dockerHost = GetDockerHost(processRunner);
			await dockerHost.RunImageInNewContainerAsync
			(
				"RequestContents",
				new Dictionary<string, string>()
			);

			Assert.Equal(2, processRunner.Launches.Count);
			Assert.Equal("DockerLib", processRunner.Launches[1].ProcessPath);
			Assert.Equal(1, processRunner.Launches[1].Args.Length);
			Assert.Equal("rm -f ContainerName", processRunner.Launches[1].Args[0]);
		}

		/// <summary>
		/// Ensures that RunImageInNewContainerAsync returns the output from
		/// a container that times out.
		/// </summary>
		[Fact]
		public async Task RunImageInNewContainerAsync_ContainerTimesOut_ReturnsOutput()
		{
			var processRunner = GetMockProcessRunner
			(
				() => Task.FromResult
				(
					new ProcessResult(completed: false, output: "Output1")
				),
				() => Task.FromResult
				(
					new ProcessResult(completed: true, output: "Output2")
				)
			);

			var dockerHost = GetDockerHost(processRunner);
			var result = await dockerHost.RunImageInNewContainerAsync
			(
				"RequestContents",
				new Dictionary<string, string>()
			);

			Assert.Equal("Output1", result.Output);
		}

		/// <summary>
		/// Ensures that RunImageInNewContainerAsync does not return a response
		/// from a container that times out, even if the container wrote such
		/// a response. (If the container timed out, it might have been in the
		/// middle of writing a response, so we want to ignore it.)
		/// </summary>
		[Fact]
		public async Task RunImageInNewContainerAsync_ContainerTimesOut_NoResponseReturned()
		{
			var fileSystem = GetMockFileSystem();

			var processRunner = GetMockProcessRunner
			(
				async () =>
				{
					await fileSystem.WriteFileContentsAsync
					(
						"ContainerTempFolder/ContainerName/ResponseFileName",
						"ResponseContents"
					);

					return new ProcessResult(completed: false, output: "Output");
				},
				() => Task.FromResult
				(
					new ProcessResult(completed: true, output: "Output2")
				)
			);

			var dockerHost = GetDockerHost(processRunner, fileSystem);
			var result = await dockerHost.RunImageInNewContainerAsync
			(
				"RequestContents",
				new Dictionary<string, string>()
			);

			Assert.Null(result.Response);
		}

		/// <summary>
		/// Returns a docker host.
		/// </summary>
		private DockerHost GetDockerHost(
			IProcessRunner processRunner,
			IFileSystem fileSystem = null)
		{
			return new DockerHost
			(
				new Mock<ILogger<DockerHost>>().Object,
				fileSystem ?? GetMockFileSystem(),
				processRunner,
				new MockOperationRunner(), 
				GetDockerHostConfig(),
				GetDockerContainerConfig(),
				() => "ContainerName"
			);
		}

		/// <summary>
		/// Returns a mock file system.
		/// </summary>
		private MockFileSystem GetMockFileSystem()
		{
			return new MockFileSystem();
		}

		/// <summary>
		/// Returns a mock process runner.
		/// </summary>
		private MockProcessRunner GetMockProcessRunner(
			bool completed = true, 
			string output = null)
		{
			Func<Task<ProcessResult>> simulatedProcess = 
				() => Task.FromResult
				(
					new ProcessResult
					(
						completed: completed, 
						output: output
					)
				);

			return GetMockProcessRunner(simulatedProcess);
		}

		/// <summary>
		/// Returns a mock process runner.
		/// </summary>
		private MockProcessRunner GetMockProcessRunner(
			Action simulatedProcess)
		{
			return GetMockProcessRunner
			(
				() =>
				{
					simulatedProcess();
					return Task.CompletedTask;
				}
			);
		}

		/// <summary>
		/// Returns a mock process runner.
		/// </summary>
		private MockProcessRunner GetMockProcessRunner(
			Func<Task> simulatedProcess)
		{
			return new MockProcessRunner
			(
				async () =>
				{
					await simulatedProcess();

					return new ProcessResult
					(
						completed: true, 
						output: "Output"
					);
				}
			);
		}

		/// <summary>
		/// Returns a mock process runner.
		/// </summary>
		private MockProcessRunner GetMockProcessRunner(
			params Func<Task<ProcessResult>>[] simulatedProcesses)
		{
			return new MockProcessRunner(simulatedProcesses);
		}

		/// <summary>
		/// Returns a docker host config object.
		/// </summary>
		private DockerHostConfig GetDockerHostConfig()
		{
			return new DockerHostConfig
			(
				"DockerLib",
				"HostTempFolder",
				"ContainerTempFolder"	
			);
		}

		/// <summary>
		/// Returns a docker container config object.
		/// </summary>
		private DockerContainerConfig GetDockerContainerConfig()
		{
			return new DockerContainerConfig
			(
				"Id",
				"ImageName",
				"MountPoint",
				"RequestFileName",
				"ResponseFileName",
				Timeout
			);
		}

		/// <summary>
		/// The timeout for the container.
		/// </summary>
		private static readonly TimeSpan Timeout 
			= TimeSpan.FromSeconds(10);
	}
}
