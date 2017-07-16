using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Http;

namespace CSC.Common.Infrastructure.Logging
{
	/// <summary>
	/// Provides the operation ID of the current request.
	/// </summary>
	public class OperationIdProvider : IOperationIdProvider
	{
		/// <summary>
		/// The HTTP context accessor.
		/// </summary>
		private readonly IHttpContextAccessor _httpContextAccessor;

		/// <summary>
		/// The operation ID of the current request.
		/// </summary>
		public string OperationId => _httpContextAccessor
			.HttpContext
			.Features
			.Get<RequestTelemetry>()
			.Id;

		/// <summary>
		/// Constructor.
		/// </summary>
		public OperationIdProvider(IHttpContextAccessor httpContextAccessor)
		{
			_httpContextAccessor = httpContextAccessor;
		}
	}
}
