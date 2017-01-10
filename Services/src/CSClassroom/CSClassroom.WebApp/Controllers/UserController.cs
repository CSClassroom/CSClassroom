using System;
using System.Linq;
using System.Threading.Tasks;
using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Model.Users;
using CSC.CSClassroom.Service.Classrooms;
using CSC.CSClassroom.Service.Identity;
using CSC.CSClassroom.WebApp.Filters;
using CSC.CSClassroom.WebApp.Settings;
using Microsoft.AspNetCore.Mvc;

namespace CSC.CSClassroom.WebApp.Controllers
{
	/// <summary>
	/// The controller for account operations.
	/// </summary>
	public class UserController : BaseController
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
		/// The classroom service.
		/// </summary>
		private readonly IClassroomService _classroomService;

		/// <summary>
		/// The section service.
		/// </summary>
		private readonly ISectionService _sectionService;

		/// <summary>
		/// The domain name of the service.
		/// </summary>
		private readonly WebAppHost _webAppHost;

		/// <summary>
		/// Constructor.
		/// </summary>
		public UserController(
			BaseControllerArgs args,
			IIdentityProvider identityProvider,
			IUserService userService, 
			IClassroomService classroomService, 
			ISectionService sectionService,
			WebAppHost webAppHost) 
				: base(args)
		{
			_userService = userService;
			_identityProvider = identityProvider;
			_classroomService = classroomService;
			_sectionService = sectionService;
			_webAppHost = webAppHost;
		}

		/// <summary>
		/// Register a student
		/// </summary>
		[Route("Register/{classroomName}/{sectionName}")]
		[Authorization(RequiredAccess.Authenticated)]
		public async Task<IActionResult> Register(
			string classroomName, 
			string sectionName)
		{
			var section = await GetSectionAsync(classroomName, sectionName);
			if (section == null)
			{
				return NotFound();
			}

			PopulateRegistrationViewBag(section);

			var currentMembership = User?.ClassroomMemberships
				?.SingleOrDefault(m => m.ClassroomId == section.ClassroomId)
				?.SectionMemberships
				?.SingleOrDefault(m => m.SectionId == section.Id);

			if (currentMembership != null)
			{
				return RedirectToAction("View", new { userId = User.Id });
			}
			else if (!section.AllowNewRegistrations)
			{
				return View("RegistrationNotAvailable");
			}
			else if (User != null)
			{
				return View("RegisterExistingStudent");
			}
			else
			{
				return View
				(
					"RegisterNewStudent",
					new StudentRegistration()
					{
						FirstName = _identityProvider.CurrentIdentity.FirstName
					}
				);
			}
		}

		/// <summary>
		/// Adds a new student to a class.
		/// </summary>
		[Route("AddNewStudent/{classroomName}/{sectionName}")]
		[Authorization(RequiredAccess.Authenticated)]
		[HttpPost]
		public async Task<IActionResult> AddNewStudent(
			string classroomName, 
			string sectionName, 
			StudentRegistration registration)
		{
			var section = await GetSectionAsync(classroomName, sectionName);
			if (section == null)
			{
				return NotFound();
			}

			if (!section.AllowNewRegistrations)
			{
				return RedirectToAction("Register");
			}

			var emailConfirmationUrl = GetEmailConfirmationUrlBuilder();

			var result = await _userService.RegisterNewStudentAsync
			(
				classroomName,
				sectionName,
				registration, 
				emailConfirmationUrl, 
				ModelErrors
			);

			switch (result)
			{
				case RegisterNewUserResult.SectionNotFound:
					return NotFound();

				case RegisterNewUserResult.SectionNotOpen:
				case RegisterNewUserResult.AlreadyRegistered:
					return RedirectToAction("Register");

				case RegisterNewUserResult.Failed:
					PopulateRegistrationViewBag(section);
					return View("RegisterNewStudent", registration);

				case RegisterNewUserResult.Success:
					var currentUser = await _userService.GetAndUpdateCurrentUserAsync();
					return RedirectToAction("View", new { userId = currentUser.Id });

				default:
					throw new InvalidOperationException("Invalid result from RegisterNewUserAsync.");
			}
		}

		/// <summary>
		/// Returns a template for the e-mail confirmation URL.
		/// </summary>
		private string GetEmailConfirmationUrlBuilder()
		{
			return $"{_webAppHost.HostName}" + Url.Action
			(
				"ConfirmEmailAddress",
				new {emailConfirmationCode = "REPLACE"}
			);
		}

		/// <summary>
		/// Adds an existing student to a class.
		/// </summary>
		[Route("AddExistingStudent/{classroomName}/{sectionName}")]
		[Authorization(RequiredAccess.Registered)]
		[HttpPost]
		public async Task<IActionResult> AddExistingStudent(
			string classroomName,
			string sectionName)
		{
			var section = await GetSectionAsync(classroomName, sectionName);
			if (section == null)
			{
				return NotFound();
			}

			if (!section.AllowNewRegistrations)
			{
				return RedirectToAction("Register");
			}

			var result = await _userService.RegisterExistingStudentAsync
			(
				classroomName, 
				sectionName
			);

			switch (result)
			{
				case RegisterExistingUserResult.SectionNotFound:
					return NotFound();

				case RegisterExistingUserResult.SectionNotOpen:
				case RegisterExistingUserResult.AlreadyRegistered:
					return RedirectToAction("Register");

				case RegisterExistingUserResult.Success:
					return RedirectToAction("View", new { userId = User.Id });

				default:
					throw new InvalidOperationException("Invalid result from RegisterNewUserAsync.");

			}
		}

		/// <summary>
		/// Views a user.
		/// </summary>
		[Route("Users/{userId}")]
		[Authorization(RequiredAccess.Registered)]
		public async Task<IActionResult> View(int? userId)
		{
			if (userId == null)
			{
				userId = User.Id;
			}

			if (!await _userService.CanViewAndEditUserAsync(userId.Value))
			{
				return Forbid();
			}

			var user = await _userService.GetUserAsync(userId.Value);

			return View(user);
		}

		/// <summary>
		/// Views a user.
		/// </summary>
		[Route("Users/{userId}/Edit")]
		[Authorization(RequiredAccess.Registered)]
		public async Task<IActionResult> Edit(int? userId)
		{
			if (userId == null)
			{
				userId = User.Id;
			}

			if (!await _userService.CanViewAndEditUserAsync(userId.Value))
			{
				return Forbid();
			}

			var user = await _userService.GetUserAsync(userId.Value);

			return View(user);
		}

		/// <summary>
		/// Views a user.
		/// </summary>
		[Route("Users/{userId}/Edit")]
		[Authorization(RequiredAccess.Registered)]
		[HttpPost]
		public async Task<IActionResult> Edit(User user)
		{
			if (!await _userService.CanViewAndEditUserAsync(user.Id))
			{
				return Forbid();
			}

			if (ModelState.IsValid 
				&& await _userService.UpdateUserAsync
				(
					user, 
					GetEmailConfirmationUrlBuilder(), 
					ModelErrors
				))
			{
				return RedirectToAction("View");
			}

			return View(user);
		}

		/// <summary>
		/// Sends a confirmation e-mail.
		/// </summary>
		[HttpPost]
		[Route("ResendEmailConfirmation")]
		[Authorization(RequiredAccess.Registered)]
		public async Task<IActionResult> ResendEmailConfirmation(int? userId)
		{
			if (userId == null)
			{
				userId = User.Id;
			}

			if (!await _userService.CanViewAndEditUserAsync(userId.Value))
			{
				return Forbid();
			}

			await _userService.ResendEmailConfirmationAsync
			(
				userId.Value,
				GetEmailConfirmationUrlBuilder()
			);

			ViewBag.HideActivationNotice = (bool?)true;
			return View("EmailConfirmationSent");
		}

		/// <summary>
		/// Resends the GitHub invitation.
		/// </summary>
		[Route("ConfirmEmailAddress")]
		[Authorization(RequiredAccess.Registered)]
		public async Task<IActionResult> ConfirmEmailAddress(string emailConfirmationCode)
		{
			await _userService.SubmitEmailConfirmationCodeAsync(emailConfirmationCode);

			return RedirectToAction("View", new { userId = User.Id });
		}

		/// <summary>
		/// Used to ensure we are authenticated.
		/// </summary>
		[Route("EnsureAuthenticated")]
		[Authorization(RequiredAccess.Authenticated)]
		public ActionResult EnsureAuthenticated()
		{
			return Ok();
		}

		/// <summary>
		/// Returns the section for the given classroom/section names.
		/// </summary>
		private async Task<Section> GetSectionAsync(string classroomName, string sectionName)
		{
			return await _sectionService.GetSectionAsync(classroomName, sectionName);
		}

		/// <summary>
		/// Populates the view bag with information needed for registration.
		/// </summary>
		private void PopulateRegistrationViewBag(Section section)
		{
			ViewBag.ClassroomDisplayName = section.Classroom.DisplayName;
			ViewBag.SectionDisplayName = section.DisplayName;
			ViewBag.UserName = _identityProvider.CurrentIdentity.UserName;
			ViewBag.LastName = _identityProvider.CurrentIdentity.LastName;
		}
	}
}
