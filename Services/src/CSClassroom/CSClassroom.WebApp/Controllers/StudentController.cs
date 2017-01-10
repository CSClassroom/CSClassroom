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
	/// The student controller, which manages students of a section.
	/// </summary>
	[Route(SectionRoutePrefix)]
	public class StudentController : BaseSectionController
	{
		/// <summary>
		/// The user service.
		/// </summary>
		private IUserService UserService { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public StudentController(
			BaseControllerArgs args,
			IClassroomService classroomService,
			ISectionService sectionService,
			IUserService userService)
			: base(args, classroomService, sectionService)
		{
			UserService = userService;
		}

		/// <summary>
		/// Shows all users in the section, if the user is an admin.
		/// </summary>
		[Route("Students")]
		[ClassroomAuthorization(ClassroomRole.Admin)]
		public async Task<IActionResult> Students(string sectionName)
		{
			var section = Classroom.Sections
				.SingleOrDefault(s => s.Name == sectionName);

			if (section == null)
			{
				return NotFound();
			}

			var students = await SectionService.GetSectionStudentsAsync
			(
				ClassroomName,
				sectionName
			);

			var studentsViewModel = new UserListViewModel
			(
				students
					.Select(m => m.ClassroomMembership.User)
					.OrderBy(u => u.LastName)
					.ThenBy(u => u.FirstName)
					.ToList(),

				new List<UserAction>()
				{
					new UserAction
					(
						"Assignments",
						user => Url.Action("StudentReport", "Assignment", new {userId = user.Id})
					),
					new UserAction
					(
						"Projects",
						user => Url.Action("Status", "Project", new {userId = user.Id})
					),
					new UserAction
					(
						"View",
						user => Url.Action("View", "User", new {userId = user.Id})
					),
					new UserAction
					(
						"Edit",
						user => Url.Action("Edit", "User", new {userId = user.Id})
					),
					new UserAction
					(
						"Remove",
						user => Url.Action("RemoveStudent", new {userId = user.Id})
					)
				}
			);

			ViewBag.Section = section;

			return View(studentsViewModel);
		}

		/// <summary>
		/// Adds a user to the section.
		/// </summary>
		[Route("AddStudent")]
		[ClassroomAuthorization(ClassroomRole.Admin)]
		public IActionResult AddStudent(string sectionName)
		{
			return View(new UserToAdd());
		}

		/// <summary>
		/// Adds a user to the section.
		/// </summary>
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Route("AddStudent")]
		[ClassroomAuthorization(ClassroomRole.Admin)]
		public async Task<IActionResult> AddStudent(string sectionName, UserToAdd userToAdd)
		{
			var result = await UserService.AddSectionStudentAsync
			(
				ClassroomName,
				sectionName,
				userToAdd.UserName
			);

			if (result)
			{
				return RedirectToAction("Students");
			}
			else
			{
				ModelErrors.AddError("UserName", "User not found. (Did the user register?)");
				return View(userToAdd);
			}
		}

		/// <summary>
		/// Removes a user from the section.
		/// </summary>
		[Route("RemoveStudent")]
		[ClassroomAuthorization(ClassroomRole.Admin)]
		public async Task<IActionResult> RemoveStudent(string sectionName, int userId)
		{
			var user = await UserService.GetUserAsync(userId);

			return View(user);
		}

		/// <summary>
		/// Removes a user from the section.
		/// </summary>
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Route("RemoveStudent")]
		[ActionName("RemoveStudent")]
		[SectionAuthorization(SectionRole.Admin)]
		public async Task<IActionResult> RemoveStudentPost(string sectionName, int userId)
		{
			await UserService.RemoveSectionStudentAsync
			(
				ClassroomName,
				sectionName,
				userId
			);

			return RedirectToAction("Students");
		}
	}
}
