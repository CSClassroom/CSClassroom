using System;
using System.Net;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace CSC.Common.Infrastructure.Logging
{
	/// <summary>
	/// Initializes the operation ID from the request header, if any.
	/// </summary>
	public class HostnameTelemetryInitializer : ITelemetryInitializer
	{
		/// <summary>
		/// The hostname.
		/// </summary>
		private readonly Lazy<string> _hostName = new Lazy<string>(Dns.GetHostName);

		/// <summary>
		/// Initializes the telemetry.
		/// </summary>
		public void Initialize(ITelemetry telemetry)
		{
			if (telemetry.Context.Cloud.RoleInstance == null)
			{
				telemetry.Context.Cloud.RoleInstance = _hostName.Value;
			}
		}
	}
}
