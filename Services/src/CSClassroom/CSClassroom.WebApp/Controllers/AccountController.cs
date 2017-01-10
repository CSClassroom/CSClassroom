using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace CSC.CSClassroom.WebApp.Controllers
{
	/// <summary>
	/// The controller for signing in and out.
	/// </summary>
	public class AccountController : Controller
	{
		/// <summary>
		/// Signs in the current user.
		/// </summary>
		[Route("SignIn")]
		public IActionResult SignIn(string returnUrl)
		{
			var validatedUrl = returnUrl != null && Url.IsLocalUrl(returnUrl)
				? returnUrl
				: "/";

			return Challenge
			(
				new AuthenticationProperties { RedirectUri = validatedUrl }, 
				OpenIdConnectDefaults.AuthenticationScheme
			);
		}

		/// <summary>
		/// Signs out the current user.
		/// </summary>
		[Route("SignOut")]
		public IActionResult SignOut()
		{
			var callbackUrl = Url.Action("SignedOut", "Account", values: null, protocol: Request.Scheme);
			return SignOut
			(
				new AuthenticationProperties { RedirectUri = callbackUrl },
				CookieAuthenticationDefaults.AuthenticationScheme, 
				OpenIdConnectDefaults.AuthenticationScheme
			);
		}

		/// <summary>
		/// Called after the user signs out.
		/// </summary>
		[Route("SignedOut")]
		public IActionResult SignedOut()
		{
			if (HttpContext.User.Identity.IsAuthenticated)
			{
				// Redirect to home page if the user is authenticated.
				return RedirectToAction(nameof(HomeController.Index), "Home");
			}

			return View();
		}

		/// <summary>
		/// Called when the user does not have permissions for the requested resource.
		/// </summary>
		[Route("Account/AccessDenied")]
		public IActionResult AccessDenied()
		{
			return View();
		}
	}
}
