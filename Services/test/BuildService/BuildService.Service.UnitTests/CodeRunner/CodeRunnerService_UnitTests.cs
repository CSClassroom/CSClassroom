using System.Threading.Tasks;
using CSC.Common.Infrastructure.Serialization;
using CSC.BuildService.Model.CodeRunner;
using CSC.BuildService.Service.CodeRunner;
using CSC.BuildService.Service.Docker;
using CSC.BuildService.Service.UnitTests.TestDoubles;
using Moq;
using Xunit;

namespace CSC.BuildService.Service.UnitTests.CodeRunner
{
	/// <summary>
	/// Unit tests for the CodeRunnerService class.
	/// </summary>
	public class CodeRunnerService_UnitTests
	{
		/// <summary>
		/// Ensures that the code runner makes the correct request when
		/// executing a method job.
		/// </summary>
		[Fact]
		public async Task ExecuteMethodJobAsync_MakesCorrectRequest()
		{
			var dockerHost = GetMockDockerHost();
			var methodJob = new MethodJob();
			var serializer = GetMockJsonSerializer(methodJob, "SerializedMethodJob");
			var codeRunnerService = GetCodeRunnerService
			(
				serializer.Object,
				dockerHost
			);

			await codeRunnerService.ExecuteMethodJobAsync(methodJob);

			Assert.True(dockerHost.RanImage);
			Assert.Equal("SerializedMethodJob", dockerHost.RequestContents);
			Assert.Equal(1, dockerHost.EnvironmentVariables.Count);
			Assert.Equal("methodJob", dockerHost.EnvironmentVariables["JOB_TYPE"]);
		}

		/// <summary>
		/// Ensures that a timeout result is returned when the code runner
		/// times out on a method job.
		/// </summary>
		[Fact]
		public async Task ExecuteMethodJobAsync_JobTimesOut_ReturnsTimeoutResult()
		{
			var dockerHost = GetMockDockerHost
			(
				new DockerResult
				(
					completed: false,
					output: "DiagnosticOutput",
					response: null
				)
			);

			var methodJob = new MethodJob();
			var serializer = GetMockJsonSerializer(methodJob, "SerializedMethodJob");
			var codeRunnerService = GetCodeRunnerService
			(
				serializer.Object,
				dockerHost
			);

			var result = await codeRunnerService.ExecuteMethodJobAsync(methodJob);

			Assert.Equal(CodeJobStatus.Timeout, result.Status);
			Assert.Equal("DiagnosticOutput", result.DiagnosticOutput);
		}
		
		/// <summary>
		/// Ensures that an error result is returned when the code runner
		/// completes a method job, but fails to return a response.
		/// </summary>
		[Fact]
		public async Task ExecuteMethodJobAsync_NoResponse_ReturnsErrorResult()
		{
			var dockerHost = GetMockDockerHost
			(
				new DockerResult
				(
					completed: true,
					output: "DiagnosticOutput",
					response: null
				)
			);

			var methodJob = new MethodJob();
			var serializer = GetMockJsonSerializer(methodJob, "SerializedMethodJob");
			var codeRunnerService = GetCodeRunnerService
			(
				serializer.Object,
				dockerHost
			);

			var result = await codeRunnerService.ExecuteMethodJobAsync(methodJob);

			Assert.Equal(CodeJobStatus.Error, result.Status);
			Assert.Equal("DiagnosticOutput", result.DiagnosticOutput);
		}

		/// <summary>
		/// Ensures that an error result is returned when the code runner
		/// completes a method job and successfully returns a response.
		/// </summary>
		[Fact]
		public async Task ExecuteMethodJobAsync_JobReturnsResponse_ReturnsSuccessResult()
		{
			var dockerHost = GetMockDockerHost
			(
				new DockerResult
				(
					completed: true,
					output: "DiagnosticOutput",
					response: "SerializedMethodJobResult"
				)
			);

			var methodJob = new MethodJob();
			var serializer = GetMockJsonSerializer<MethodJob, MethodJobResult>
			(
				methodJob, 
				"SerializedMethodJob", 
				"SerializedMethodJobResult"
			);

			var codeRunnerService = GetCodeRunnerService
			(
				serializer.Object,
				dockerHost
			);

			var result = await codeRunnerService.ExecuteMethodJobAsync(methodJob);

			Assert.Equal(CodeJobStatus.Completed, result.Status);
			Assert.Null(result.DiagnosticOutput);
		}

		/// <summary>
		/// Ensures that the code runner makes the correct request when
		/// executing a class job.
		/// </summary>
		[Fact]
		public async Task ExecuteClassJobAsync_MakesCorrectRequest()
		{
			var dockerHost = GetMockDockerHost
			(
				new DockerResult
				(
					completed: true,
					output: "DiagnosticOutput",
					response: null
				)
			);

			var classJob = new ClassJob();
			var serializer = GetMockJsonSerializer(classJob, "SerializedClassJob");
			var codeRunnerService = GetCodeRunnerService
			(
				serializer.Object,
				dockerHost
			);

			await codeRunnerService.ExecuteClassJobAsync(classJob);

			Assert.Equal("SerializedClassJob", dockerHost.RequestContents);
			Assert.Equal(1, dockerHost.EnvironmentVariables.Count);
			Assert.Equal("classJob", dockerHost.EnvironmentVariables["JOB_TYPE"]);
		}

		/// <summary>
		/// Ensures that a timeout result is returned when the code runner
		/// times out on a class job.
		/// </summary>
		[Fact]
		public async Task ExecuteClassJobAsync_JobTimesOut_ReturnsTimeoutResult()
		{
			var dockerHost = GetMockDockerHost
			(
				new DockerResult
				(
					completed: false,
					output: "DiagnosticOutput",
					response: null
				)
			);

			var classJob = new ClassJob();
			var serializer = GetMockJsonSerializer(classJob, "SerializedClassJob");
			var codeRunnerService = GetCodeRunnerService
			(
				serializer.Object,
				dockerHost
			);

			var result = await codeRunnerService.ExecuteClassJobAsync(classJob);

			Assert.Equal(CodeJobStatus.Timeout, result.Status);
			Assert.Equal("DiagnosticOutput", result.DiagnosticOutput);
		}

		/// <summary>
		/// Ensures that an error result is returned when the code runner
		/// completes a class job, but fails to return a response.
		/// </summary>
		[Fact]
		public async Task ExecuteClassJobAsync_NoResponse_ReturnsErrorResult()
		{
			var dockerHost = GetMockDockerHost
			(
				new DockerResult
				(
					completed: true,
					output: "DiagnosticOutput",
					response: null
				)
			);

			var classJob = new ClassJob();
			var serializer = GetMockJsonSerializer(classJob, "SerializedClassJob");
			var codeRunnerService = GetCodeRunnerService
			(
				serializer.Object,
				dockerHost
			);

			var result = await codeRunnerService.ExecuteClassJobAsync(classJob);

			Assert.Equal(CodeJobStatus.Error, result.Status);
			Assert.Equal("DiagnosticOutput", result.DiagnosticOutput);
		}

		/// <summary>
		/// Ensures that an error result is returned when the code runner
		/// completes a class job and successfully returns a response.
		/// </summary>
		[Fact]
		public async Task ExecuteClassJobAsync_JobReturnsResponse_ReturnsSuccessResult()
		{
			var dockerHost = GetMockDockerHost
			(
				new DockerResult
				(
					completed: true,
					output: "DiagnosticOutput",
					response: "SerializedClassJobResult"
				)
			);

			var classJob = new ClassJob();
			var serializer = GetMockJsonSerializer<ClassJob, ClassJobResult>
			(
				classJob,
				"SerializedClassJob",
				"SerializedClassJobResult"
			);

			var codeRunnerService = GetCodeRunnerService
			(
				serializer.Object,
				dockerHost
			);

			var result = await codeRunnerService.ExecuteClassJobAsync(classJob);

			Assert.Equal(CodeJobStatus.Completed, result.Status);
			Assert.Null(result.DiagnosticOutput);
		}

		/// <summary>
		/// Returns a mock docker host factory.
		/// </summary>
		private MockDockerHost GetMockDockerHost(
			DockerResult dockerResult = null)
		{
			return new MockDockerHost
			(
				dockerResult ?? new DockerResult
				(
					completed: true,
					output: "DiagnosticOutput",
					response: null
				)
			);
		}

		/// <summary>
		/// Returns a mock docker host factory.
		/// </summary>
		private Mock<IDockerHostFactory> GetMockDockerHostFactory(
			MockDockerHost dockerHost)
		{
			var dockerHostFactory = new Mock<IDockerHostFactory>();

			dockerHostFactory
				.Setup(factory => factory.CreateDockerHost("CodeRunner"))
				.Returns(dockerHost);

			return dockerHostFactory;
		}

		/// <summary>
		/// Returns a mock Json serializer that does not expect to
		/// deserialize anything.
		/// </summary>
		private Mock<IJsonSerializer> GetMockJsonSerializer<TCodeJob>(
			TCodeJob codeJobToSerialize,
			string serializedJob)
		{
			return GetMockJsonSerializer<TCodeJob, object>
			(
				codeJobToSerialize,
				serializedJob,
				serializedResult: null
			);
		}

		/// <summary>
		/// Returns a mock Json serializer that expects to serialize a job,
		/// and deserialize the job result.
		/// </summary>
		private Mock<IJsonSerializer> GetMockJsonSerializer<TCodeJob, TCodeJobResult>(
			TCodeJob codeJobToSerialize,
			string serializedJob,
			string serializedResult) 
				where TCodeJobResult : class, new()
		{
			var jsonSerializer = new Mock<IJsonSerializer>();

			jsonSerializer
				.Setup(s => s.Serialize(codeJobToSerialize))
				.Returns(serializedJob);

			if (serializedResult != null)
			{
				jsonSerializer
					.Setup(s => s.Deserialize<TCodeJobResult>(serializedResult))
					.Returns(new TCodeJobResult());
			}

			return jsonSerializer;
		}

		/// <summary>
		/// Returns the code runner service.
		/// </summary>
		private ICodeRunnerService GetCodeRunnerService(
			IJsonSerializer serializer,
			MockDockerHost dockerHost)
		{
			return new CodeRunnerService
			(
				serializer,
				GetMockDockerHostFactory(dockerHost).Object
			);
		}
	}
}
