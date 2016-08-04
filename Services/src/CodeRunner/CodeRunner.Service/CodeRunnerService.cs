using System.Collections.Generic;
using System.Threading.Tasks;
using CSC.CodeRunner.Model;
using CSC.Common.Service.Docker;
using CSC.Common.Service.Serialization;

namespace CSC.CodeRunner.Service
{
	/// <summary>
	/// The implementation of the code runner service.
	/// </summary>
    public class CodeRunnerService : ICodeRunnerService
    {
		/// <summary>
		/// Runs docker images in sibling containers.
		/// </summary>
		private IDockerHost _dockerHost;

		/// <summary>
		/// Serializes/deserializes Json objects.
		/// </summary>
		private IJsonConverter _jsonConverter;

		/// <summary>
		/// The environment variable name for the job type.
		/// </summary>
		private const string c_jobTypeVar = "JOB_TYPE";

		/// <summary>
		/// The job type for a class job.
		/// </summary>
		private const string c_classJobType = "classJob";

		/// <summary>
		/// The job type for a method job.
		/// </summary>
		private const string c_methodJobType = "methodJob";

		/// <summary>
		/// Constructor.
		/// </summary>
		public CodeRunnerService(IDockerHost dockerHost, IJsonConverter jsonConverter)
		{
			_dockerHost = dockerHost;
			_jsonConverter = jsonConverter;
		}

		/// <summary>
		/// Executes a class job, and returns the result.
		/// </summary>
		public async Task<ClassJobResult> ExecuteClassJobAsync(ClassJob codeJob)
		{
			return await ExecuteCodeJobAsync<ClassJob, ClassJobResult>(codeJob, c_classJobType);
		}

		/// <summary>
		/// Executes a method job, and returns the result.
		/// </summary>
		public async Task<MethodJobResult> ExecuteMethodJobAsync(MethodJob methodJob)
		{
			return await ExecuteCodeJobAsync<MethodJob, MethodJobResult>(methodJob, c_methodJobType);
		}

		/// <summary>
		/// Executes a code job, and returns the result.
		/// </summary>
		private async Task<TCodeJobResult> ExecuteCodeJobAsync<TCodeJob, TCodeJobResult>(
			TCodeJob codeJob, 
			string jobType) 
				where TCodeJobResult: CodeJobResult, new()
		{
			var serializedCodeJob = _jsonConverter.Serialize(codeJob);

			var dockerResult = await _dockerHost.RunImageInNewContainerAsync(
				serializedCodeJob,
				new Dictionary<string, string>()
				{
					[c_jobTypeVar] = jobType
				});

			if (dockerResult.Response == null)
			{
				return new TCodeJobResult()
				{
					Status = dockerResult.Completed ? CodeJobStatus.Error : CodeJobStatus.Timeout,
					DiagnosticOutput = dockerResult.Output
				};
			}

			var codeJobResult = _jsonConverter.Deserialize<TCodeJobResult>(dockerResult.Response);
			codeJobResult.Status = CodeJobStatus.Completed;

			return codeJobResult;
		}
	}
}
