using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Model.Users;
using CSC.CSClassroom.Service.Classrooms;
using CSC.CSClassroom.Service.Identity;
using CSC.CSClassroom.WebApp.Filters;
using CSC.CSClassroom.WebApp.ViewModels.Shared;
using Microsoft.AspNetCore.Mvc;

namespace CSC.CSClassroom.WebApp.Controllers
{
	/// <summary>
	/// The classroom controller.
	/// </summary>
	public class ClassroomController : BaseController
	{
		/// <summary>
		/// The classroom service.
		/// </summary>
		private IClassroomService ClassroomService { get; }

		/// <summary>
		/// The user service.
		/// </summary>
		private IUserService UserService { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public ClassroomController(
			BaseControllerArgs args, 
			IClassroomService classroomService,
			IUserService userService) 
				: base(args)
		{
			ClassroomService = classroomService;
			UserService = userService;
		}

		/// <summary>
		/// Shows all classrooms.
		/// </summary>
		[Route("Classes")]
		[Authorization(RequiredAccess.SuperUser)]
		public async Task<IActionResult> Index()
		{
			var classrooms = await ClassroomService.GetClassroomsAsync();

			return View(classrooms);
		}

		/// <summary>
		/// The home page for a given classroom.
		/// </summary>
		[Route("Classes/{className}")]
		[Authorization(RequiredAccess.Registered)]
		public async Task<IActionResult> Home(string className)
		{
			var classroom = await ClassroomService.GetClassroomAsync(className);

			if (classroom == null)
			{
				return NotFound();
			}

			var membership = User.ClassroomMemberships
				.SingleOrDefault(cm => cm.Classroom == classroom);

			if (membership == null && !User.SuperUser)
			{
				return Forbid();
			}

			ViewBag.Classroom = classroom;
			ViewBag.ClassroomRole = User.SuperUser
				? ClassroomRole.Admin
				: membership?.Role ?? ClassroomRole.Admin;

			return View(classroom);
		}

		/// <summary>
		/// Shows the details of a classroom.
		/// </summary>
		[Route("Classes/{className}/Details")]
		[Authorization(RequiredAccess.SuperUser)]
		public async Task<IActionResult> Details(string className)
		{
			if (className == null)
			{
				return NotFound();
			}

			var classroom = await ClassroomService.GetClassroomAsync(className);
			if (classroom == null)
			{
				return NotFound();
			}

			return View(classroom);
		}

		/// <summary>
		/// Creates a new classroom.
		/// </summary>
		[Route("CreateClass")]
		[Authorization(RequiredAccess.SuperUser)]
		public IActionResult Create()
		{
			return View("CreateEdit");
		}

		/// <summary>
		/// Creates a new classroom.
		/// </summary>
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Route("CreateClass")]
		[Authorization(RequiredAccess.SuperUser)]
		public async Task<IActionResult> Create(Classroom classroom)
		{
			if (ModelState.IsValid)
			{
				await ClassroomService.CreateClassroomAsync(classroom);

				return RedirectToAction("Index");
			}
			else
			{
				return View("CreateEdit", classroom);
			}
		}

		/// <summary>
		/// Edits a classroom.
		/// </summary>
		[Route("Classes/{className}/Edit")]
		[Authorization(RequiredAccess.SuperUser)]
		public async Task<IActionResult> Edit(string className)
		{
			if (className == null)
			{
				return NotFound();
			}

			var classroom = await ClassroomService.GetClassroomAsync(className);
			if (classroom == null)
			{
				return NotFound();
			}

			return View("CreateEdit", classroom);
		}

		/// <summary>
		/// Edits a classroom.
		/// </summary>
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Route("Classes/{className}/Edit")]
		[Authorization(RequiredAccess.SuperUser)]
		public async Task<IActionResult> Edit(string className, Classroom classroom)
		{
			if (ModelState.IsValid)
			{
				await ClassroomService.UpdateClassroomAsync(classroom);

				return RedirectToAction("Index");
			}
			else
			{
				return View("CreateEdit", classroom);
			}
		}

		/// <summary>
		/// Deletes a classroom.
		/// </summary>
		[Route("Classes/{className}/Delete")]
		[Authorization(RequiredAccess.SuperUser)]
		public async Task<IActionResult> Delete(string className)
		{
			if (className == null)
			{
				return NotFound();
			}

			var classroom = await ClassroomService.GetClassroomAsync(className);
			if (classroom == null)
			{
				return NotFound();
			}

			return View(classroom);
		}

		/// <summary>
		/// Deletes a classroom.
		/// </summary>
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		[Route("Classes/{className}/Delete")]
		[Authorization(RequiredAccess.SuperUser)]
		public async Task<IActionResult> DeleteConfirmed(string className)
		{
			await ClassroomService.DeleteClassroomAsync(className);

			return RedirectToAction("Index");
		}
	}
}
