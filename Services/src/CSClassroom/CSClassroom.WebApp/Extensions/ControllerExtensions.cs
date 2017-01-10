using Microsoft.AspNetCore.Mvc;

namespace CSC.CSClassroom.WebApp.Extensions
{
	/// <summary>
	/// Extension methods for the controller class.
	/// </summary>
	public static class ControllerExtensions
	{
		/// <summary>
		/// Redirects the user to the sign in page, after which they will be redirected back.
		/// </summary>
		public static IActionResult RedirectToSignIn(this ControllerBase controller)
		{
			var request = controller.HttpContext.Request;

			return controller.RedirectToAction
			(
				"SignIn", 
				"Account", 
				new { returnUrl = $"{request.Path}{request.QueryString}" }
			);
		}
	}
}
