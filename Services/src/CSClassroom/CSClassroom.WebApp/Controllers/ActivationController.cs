using System.Threading.Tasks;
using CSC.CSClassroom.Model.Users;
using CSC.CSClassroom.Service.Identity;
using CSC.CSClassroom.WebApp.Extensions;
using CSC.CSClassroom.WebApp.ModelBinders;
using Microsoft.AspNetCore.Mvc;

namespace CSC.CSClassroom.WebApp.Controllers
{
	/// <summary>
	/// The controller for activating the service.
	/// </summary>
	public class ActivationController : Controller
	{
		/// <summary>
		/// The identity provider.
		/// </summary>
		private readonly IIdentityProvider _identityProvider;

		/// <summary>
		/// The user service.
		/// </summary>
		private readonly IUserService _userService;

		/// <summary>
		/// Constructor.
		/// </summary>
		public ActivationController(IIdentityProvider identityProvider, IUserService userService)
		{
			_identityProvider = identityProvider;
			_userService = userService;
		}

		/// <summary>
		/// Activate the service, allowing a superuser to sign up
		/// (requiring the correct activation code).
		/// </summary>
		[Route("ActivateService")]
		public async Task<IActionResult> ActivateService()
		{
			if (_identityProvider.CurrentIdentity == null)
			{
				return this.RedirectToSignIn();
			}

			if (await _userService.AnyRegisteredUsersAsync())
			{
				return View("AlreadyActivated");
			}

			ViewBag.UserName = _identityProvider.CurrentIdentity.UserName;

			return View
			(
				new SuperUserRegistration()
				{
					FirstName = _identityProvider.CurrentIdentity.FirstName,
					LastName = _identityProvider.CurrentIdentity.LastName
				}
			);
		}

		/// <summary>
		/// Activate the service by signing up a superuser, if the provided
		/// activation code is correct.
		/// </summary>
		[Route("ActivateService")]
		[HttpPost]
		public async Task<IActionResult> ActivateService(SuperUserRegistration registration)
		{
			if (_identityProvider.CurrentIdentity == null)
			{
				return this.RedirectToSignIn();
			}

			if (await _userService.AnyRegisteredUsersAsync())
			{
				return View("AlreadyActivated");
			}

			var result = await _userService.RegisterFirstSuperUserAsync
			(
				registration, 
				new ModelErrorCollection(ModelState)
			);

			if (result == RegisterNewUserResult.AlreadyRegistered)
			{
				return View("AlreadyActivated");
			}
			else if (result == RegisterNewUserResult.Failed)
			{
				ViewBag.UserName = _identityProvider.CurrentIdentity.UserName;

				return View(registration);
			}
			else
			{
				return View("ActivationSuccessful");
			}
		}
	}
}
