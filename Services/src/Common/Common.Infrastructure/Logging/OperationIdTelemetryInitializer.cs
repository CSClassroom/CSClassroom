using System.Linq;
using Microsoft.ApplicationInsights.AspNetCore.TelemetryInitializers;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace CSC.Common.Infrastructure.Logging
{
	/// <summary>
	/// Initializes the operation ID from the request header, if any.
	/// </summary>
	public class OperationIdTelemetryInitializer : ITelemetryInitializer
	{
		/// <summary>
		/// The HTTP context accessor.
		/// </summary>
		private readonly IHttpContextAccessor _httpContextAccessor;

		/// <summary>
		/// Constructor.
		/// </summary>
		public OperationIdTelemetryInitializer(IHttpContextAccessor httpContextAccessor)
		{
			_httpContextAccessor = httpContextAccessor;
		}

		/// <summary>
		/// Initializes the initializer.
		/// </summary>
		public void Initialize(ITelemetry telemetry)
		{
			HttpContext httpContext = _httpContextAccessor.HttpContext;
			RequestTelemetry requestTelemetry = httpContext?.Features.Get<RequestTelemetry>();
			if (requestTelemetry != null)
			{
				StringValues value;

				httpContext
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
}
