using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CSC.BuildService.Model.CodeRunner;
using CSC.BuildService.Service.CodeRunner;
using CSC.Common.Infrastructure.Logging;
using CSC.Common.Infrastructure.Serialization;
using CSC.CSClassroom.WebApp.Settings;

namespace CSC.CSClassroom.WebApp.ServiceClients
{
	/// <summary>
	/// A proxy for the code runner service.
	/// </summary>
	public class CodeRunnerClient : ICodeRunnerService
	{
		/// <summary>
		/// The base URI.
		/// </summary>
		private readonly BuildServiceSettings _settings;

		/// <summary>
		/// The JSON serializer.
		/// </summary>
		private readonly IJsonSerializer _jsonSerializer;
		
		/// <summary>
		/// The operation ID provider.
		/// </summary>
		private readonly IOperationIdProvider _operationIdProvider;

		/// <summary>
		/// Constructor.
		/// </summary>
		public CodeRunnerClient(
			BuildServiceSettings buildServiceSettings, 
			IJsonSerializer jsonSerializer, 
			IOperationIdProvider operationIdProvider)
		{
			_settings = buildServiceSettings;
			_jsonSerializer = jsonSerializer;
			_operationIdProvider = operationIdProvider;
		}

		/// <summary>
		/// Executes a class job.
		/// </summary>
		public async Task<ClassJobResult> ExecuteClassJobAsync(ClassJob classJob)
		{
			return await ExecuteCodeJobAsync<ClassJob, ClassJobResult>(classJob, "classjob");
		}

		/// <summary>
		/// Executes a method job.
		/// </summary>
		public async Task<MethodJobResult> ExecuteMethodJobAsync(MethodJob methodJob)
		{
			return await ExecuteCodeJobAsync<MethodJob, MethodJobResult>(methodJob, "methodjob");
		}

		/// <summary>
		/// Executes a code job.
		/// </summary>
		private async Task<TCodeJobResult> ExecuteCodeJobAsync<TCodeJob, TCodeJobResult>(
			TCodeJob codeJob, 
			string jobType)
		{
			var httpClient = new HttpClient();
			var content = new StringContent
			(
				_jsonSerializer.Serialize(codeJob),
				Encoding.UTF8,
				"application/json"
			);

			content.Headers.Add("X-Operation-Id", _operationIdProvider.OperationId);

			var response = await httpClient.PostAsync
			(
				$"{_settings.Host}/api/{jobType}/execute",
				content
			);

			var responseStr = await response.Content.ReadAsStringAsync();

			return _jsonSerializer.Deserialize<TCodeJobResult>(responseStr);
		}

	}
}
