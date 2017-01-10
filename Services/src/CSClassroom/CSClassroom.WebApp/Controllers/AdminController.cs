using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSC.CSClassroom.Model.Users;
using CSC.CSClassroom.Service.Classrooms;
using CSC.CSClassroom.Service.Identity;
using CSC.CSClassroom.WebApp.Filters;
using CSC.CSClassroom.WebApp.ViewModels.Shared;
using Microsoft.AspNetCore.Mvc;

namespace CSC.CSClassroom.WebApp.Controllers
{
	/// <summary>
	/// The admin controller, which manages administrators of a classroom.
	/// </summary>
	[Route(ClassroomRoutePrefix)]
	public class AdminController : BaseClassroomController
	{
		/// <summary>
		/// The user service.
		/// </summary>
		private IUserService UserService { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public AdminController(
			BaseControllerArgs args,
			IClassroomService classroomService,
			IUserService userService)
				: base(args, classroomService)
		{
			UserService = userService;
		}

		/// <summary>
		/// Shows all administrators of the class.
		/// </summary>
		[Route("Admins")]
		[Authorization(RequiredAccess.SuperUser)]
		public async Task<IActionResult> Admins(string className)
		{
			var classroom = await ClassroomService.GetClassroomAsync(className);
			var admins = await ClassroomService.GetClassroomAdminsAsync(className);

			var viewModel = new UserListViewModel
			(
				admins.Select(admin => admin.User).ToList(),
				new List<UserAction>()
				{
						new UserAction
						(
							"Remove",
							user => Url.Action("RemoveAdmin", new {userId = user.Id})
						)
				}
			);

			ViewBag.Classroom = classroom;
			ViewBag.ClassroomRole = ClassroomRole.Admin;

			return View(viewModel);
		}

		/// <summary>
		/// Adds an admin to the class.
		/// </summary>
		[Route("AddAdmin")]
		[Authorization(RequiredAccess.SuperUser)]
		public IActionResult AddAdmin(string className)
		{
			return View(new UserToAdd());
		}

		/// <summary>
		/// Adds an admin to the class.
		/// </summary>
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Route("AddAdmin")]
		[Authorization(RequiredAccess.SuperUser)]
		public async Task<IActionResult> AddAdmin(string className, UserToAdd userToAdd)
		{
			var result = await UserService.AddClassroomAdminAsync
			(
				className,
				userToAdd.UserName
			);

			if (result)
			{
				return RedirectToAction("Admins");
			}
			else
			{
				ModelErrors.AddError("UserName", "User not found. (Did the user register?)");
				return View(userToAdd);
			}
		}

		/// <summary>
		/// Removes an admin from the class.
		/// </summary>
		[Route("RemoveAdmin")]
		[Authorization(RequiredAccess.SuperUser)]
		public async Task<IActionResult> RemoveAdmin(string className, int userId)
		{
			var user = await UserService.GetUserAsync(userId);

			return View(user);
		}

		/// <summary>
		/// Removes an admin from the class.
		/// </summary>
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Route("RemoveAdmin")]
		[ActionName("RemoveAdmin")]
		[Authorization(RequiredAccess.SuperUser)]
		public async Task<IActionResult> RemoveAdminPost(string className, int userId)
		{
			await UserService.RemoveClassroomAdminAsync(className, userId);

			return RedirectToAction("Admins");
		}
	}
}
