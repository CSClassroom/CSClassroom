using System.Linq;
using CSC.CSClassroom.Model.Users;
using CSC.CSClassroom.Service.Identity;
using CSC.CSClassroom.WebApp.Filters;
using CSC.CSClassroom.WebApp.Settings;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CSC.CSClassroom.WebApp.Controllers
{
	/// <summary>
	/// The home controller.
	/// </summary>
	public class HomeController : BaseController
	{
		/// <summary>
		/// Whether or not to show full error information.
		/// </summary>
		private readonly ErrorSettings _errorSettings;

		/// <summary>
		/// Constructor.
		/// </summary>
		public HomeController(BaseControllerArgs args, ErrorSettings errorSettings) 
			: base(args)
		{
			_errorSettings = errorSettings;
		}

		/// <summary>
		/// The home page.
		/// </summary>
		[Route("")]
		[Authorization(RequiredAccess.Anonymous)]
		public IActionResult Index()
		{
			if (User == null)
			{
				return View();
			}

			if (User.SuperUser)
			{
				return RedirectToAction("Index", "Classroom");
			}

			var classroomMembership = User.ClassroomMemberships
				?.OrderBy(cm => !cm.Classroom.IsActive)
				?.FirstOrDefault(c => c.Role >= ClassroomRole.General);

			if (classroomMembership != null)
			{
				return RedirectToAction
				(
					"Index",
					"ClassroomHome",
					new { className = classroomMembership.Classroom.Name }
				);
			}

			return View();
		}

		/// <summary>
		/// An error page.
		/// </summary>
		[Route("Error")]
		[Authorization(RequiredAccess.Anonymous)]
		public IActionResult Error()
		{
			if (_errorSettings.ShowExceptions && IdentityState >= IdentityState.Registered)
			{
				var exceptionHandlerFeature = HttpContext.Features
					.Get<IExceptionHandlerFeature>();

				return View(exceptionHandlerFeature?.Error);
			}
			else
			{
				return View();
			}
		}
	}
}
