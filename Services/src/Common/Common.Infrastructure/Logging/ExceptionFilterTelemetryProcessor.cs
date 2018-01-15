using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace CSC.Common.Infrastructure.Logging
{
	/// <summary>
	/// Filters out all exception logging handled at the platform level.
	/// This avoids duplicate exception traces being sent to Application
	/// Insights (as Serilog already sends enriched exception information).
	/// </summary>
	public class ExceptionFilterTelemetryProcessor : ITelemetryProcessor
	{
		/// <summary>
		/// The next telemetry processor in the chain.
		/// </summary>
		private ITelemetryProcessor Next { get; set; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public ExceptionFilterTelemetryProcessor(ITelemetryProcessor next)
		{
			this.Next = next;
		}
		
		/// <summary>
		/// Processes the telemetry item.
		/// </summary>
		public void Process(ITelemetry item)
		{
			var excTelem = item as ExceptionTelemetry;
			if (excTelem != null && 
			    excTelem.Properties.TryGetValue("handledAt", out var handledAt) && 
			    handledAt == "Platform")
			{
				return;
			}

			this.Next.Process(item);
		}
	}
}