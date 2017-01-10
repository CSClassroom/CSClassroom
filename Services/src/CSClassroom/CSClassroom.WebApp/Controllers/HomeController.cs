using System.Linq;
using CSC.CSClassroom.Model.Users;
using CSC.CSClassroom.WebApp.Filters;
using Microsoft.AspNetCore.Mvc;

namespace CSC.CSClassroom.WebApp.Controllers
{
	/// <summary>
	/// The home controller.
	/// </summary>
	public class HomeController : BaseController
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public HomeController(BaseControllerArgs args) 
			: base(args)
		{
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
					?.FirstOrDefault(c => c.Role >= ClassroomRole.General);

			if (classroomMembership != null)
			{
				return RedirectToAction
				(
					"Home",
					"Classroom",
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
			return View();
		}
	}
}
