using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CSC.BuildService.Model.ProjectRunner;
using CSC.Common.Infrastructure.Serialization;

namespace CSC.BuildService.Service.ProjectRunner
{
	/// <summary>
	/// Notifies the source of the project job that the job is complete.
	/// </summary>
	public class ProjectJobResultNotifier : IProjectJobResultNotifier
	{
		/// <summary>
		/// The JSON serializer.
		/// </summary>
		private readonly IJsonSerializer _jsonSerializer;

		/// <summary>
		/// Constructor.
		/// </summary>
		public ProjectJobResultNotifier(IJsonSerializer jsonSerializer)
		{
			_jsonSerializer = jsonSerializer;
		}

		/// <summary>
		/// Notifies the source of the project job that the job is complete.
		/// </summary>
		/// <param name="callbackHost">The callback host.</param>
		/// <param name="callbackPath">The callback path.</param>
		/// <param name="operationId">The operation ID (for logging purposes).</param>
		/// <param name="result">The result to post.</param>
		public async Task NotifyAsync(
			string callbackHost, 
			string callbackPath, 
			string operationId, 
			ProjectJobResult result)
		{
			var serializedResult = _jsonSerializer.Serialize(result);
			var httpClient = new HttpClient();
			var requestContent = new StringContent
			(
				serializedResult,
				Encoding.UTF8,
				"application/json"
			);

			requestContent.Headers.Add("X-Operation-Id", operationId);

			var response = await httpClient.PostAsync
			(
				$"{callbackHost}{callbackPath}",
				requestContent
			);
		}
	}
}
