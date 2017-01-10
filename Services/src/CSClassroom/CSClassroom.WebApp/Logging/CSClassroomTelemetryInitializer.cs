using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace CSC.CSClassroom.WebApp.Logging
{
	/// <summary>
	/// Constructor.
	/// </summary>
	public class CSClassroomTelemetryInitializer : ITelemetryInitializer
	{
		/// <summary>
		/// The action context accessor.
		/// </summary>
		private readonly IHttpContextAccessor _httpContextAccessor;

		/// <summary>
		/// A factory that creates an action context accessor.
		/// </summary>
		private readonly IActionContextAccessor _actionContextAccessor;

		/// <summary>
		/// Constructor.
		/// </summary>
		public CSClassroomTelemetryInitializer(
			IHttpContextAccessor httpContextAccessor,
			IActionContextAccessor actionContextAccessor)
		{
			_httpContextAccessor = httpContextAccessor;
			_actionContextAccessor = actionContextAccessor;
		}

		/// <summary>
		/// Initializes the telemetry.
		/// </summary>
		public void Initialize(ITelemetry telemetry)
		{
			telemetry.Context.User.AuthenticatedUserId = _httpContextAccessor
				?.HttpContext
				?.User
				?.Identity
				?.Name;

			var actionContext = _actionContextAccessor.ActionContext;

			var classroomName = (string)actionContext?.RouteData?.Values["className"];

			if (classroomName != null)
				telemetry.Context.Properties["Classroom"] = classroomName;

			var sectionName = (string)actionContext?.RouteData?.Values["sectionName"];

			if (sectionName != null)
				telemetry.Context.Properties["Section"] = sectionName;
		}
	}
}

