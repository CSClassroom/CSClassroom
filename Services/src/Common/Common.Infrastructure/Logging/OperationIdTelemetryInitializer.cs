using System.Linq;
using Microsoft.ApplicationInsights.AspNetCore.TelemetryInitializers;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace CSC.Common.Infrastructure.Logging
{
	/// <summary>
	/// Initializes the operation ID from the request header, if any.
	/// </summary>
	public class OperationIdTelemetryInitializer : TelemetryInitializerBase
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public OperationIdTelemetryInitializer(IHttpContextAccessor httpContextAccessor)
			: base(httpContextAccessor)
		{
		}

		/// <summary>
		/// Initializes the telemetry.
		/// </summary>
		protected override void OnInitializeTelemetry(
			HttpContext platformContext, 
			RequestTelemetry requestTelemetry, 
			ITelemetry telemetry)
		{
			StringValues value;

			platformContext
				?.Request
				?.Headers
				?.TryGetValue("X-Operation-Id", out value);

			if (value.Count == 1)
			{
				requestTelemetry.Id = value.First();
				telemetry.Context.Operation.Id = value.First();
			}
		}
	}
}
