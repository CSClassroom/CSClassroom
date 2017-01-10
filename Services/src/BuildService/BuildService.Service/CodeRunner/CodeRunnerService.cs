using System.Collections.Generic;
using System.Threading.Tasks;
using CSC.BuildService.Model.CodeRunner;
using CSC.BuildService.Service.Docker;
using CSC.Common.Infrastructure.Extensions;
using CSC.Common.Infrastructure.Serialization;

namespace CSC.BuildService.Service.CodeRunner
{
	/// <summary>
	/// The implementation of the code runner service.
	/// </summary>
	public class CodeRunnerService : ICodeRunnerService
	{
		/// <summary>
		/// Serializes/deserializes Json objects.
		/// </summary>
		private readonly IJsonSerializer _jsonSerializer;

		/// <summary>
		/// Runs docker images in sibling containers.
		/// </summary>
		private readonly IDockerHostFactory _dockerHostFactory;

		/// <summary>
		/// The name of the image to run.
		/// </summary>
		private const string c_containerKey = "CodeRunner";

		/// <summary>
		/// The container environment variable name to set 
		/// for the job type.
		/// </summary>
		private const string c_jobTypeVar = "JOB_TYPE";

		/// <summary>
		/// Constructor.
		/// </summary>
		public CodeRunnerService(IJsonSerializer jsonSerializer, IDockerHostFactory dockerHostFactory)
		{
			_jsonSerializer = jsonSerializer;
			_dockerHostFactory = dockerHostFactory;
		}

		/// <summary>
		/// Executes a method job, and returns the result.
		/// </summary>
		public async Task<MethodJobResult> ExecuteMethodJobAsync(MethodJob methodJob)
		{
			return await ExecuteCodeJobAsync<MethodJob, MethodJobResult>(methodJob);
		}

		/// <summary>
		/// Executes a class job, and returns the result.
		/// </summary>
		public async Task<ClassJobResult> ExecuteClassJobAsync(ClassJob classJob)
		{
			return await ExecuteCodeJobAsync<ClassJob, ClassJobResult>(classJob);
		}

		/// <summary>
		/// Executes a code job, and returns the result.
		/// </summary>
		private async Task<TCodeJobResult> ExecuteCodeJobAsync<TCodeJob, TCodeJobResult>(
			TCodeJob codeJob)
				where TCodeJob : CodeJob
				where TCodeJobResult: CodeJobResult, new()
		{
			var serializedCodeJob = _jsonSerializer.Serialize(codeJob);

			var dockerHost = _dockerHostFactory.CreateDockerHost(c_containerKey);

			var dockerResult = await dockerHost.RunImageInNewContainerAsync(
				serializedCodeJob,
				new Dictionary<string, string>()
				{
					[c_jobTypeVar] = codeJob.GetType().Name.ToCamelCase()
				});

			if (dockerResult.Response == null)
			{
				return new TCodeJobResult()
				{
					Status = dockerResult.Completed ? CodeJobStatus.Error : CodeJobStatus.Timeout,
					DiagnosticOutput = dockerResult.Output
				};
			}

			var codeJobResult = _jsonSerializer.Deserialize<TCodeJobResult>(dockerResult.Response);
			codeJobResult.Status = CodeJobStatus.Completed;

			return codeJobResult;
		}
	}
}
