using Microsoft.ApplicationInsights.DataContracts;

namespace CSC.Common.Infrastructure.Logging
{
	/// <summary>
	/// Provides the operation ID of the current request.
	/// </summary>
	public class OperationIdProvider : IOperationIdProvider
	{
		/// <summary>
		/// The telemetry of the current request.
		/// </summary>
		private readonly RequestTelemetry _requestTelemetry;

		/// <summary>
		/// The operation ID of the current request.
		/// </summary>
		public string OperationId => _requestTelemetry.Id;

		/// <summary>
		/// Constructor.
		/// </summary>
		public OperationIdProvider(RequestTelemetry requestTelemetry)
		{
			_requestTelemetry = requestTelemetry;
		}
	}
}
