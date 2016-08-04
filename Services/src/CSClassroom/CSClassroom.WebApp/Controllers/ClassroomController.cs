using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Service.Classrooms;

namespace CSC.CSClassroom.WebApp.Controllers
{
	/// <summary>
	/// The classroom controller.
	/// </summary>
	[Route(GroupRoutePrefix)]
	public class ClassroomController : BaseGroupController
	{
		/// <summary>
		/// The classroom service.
		/// </summary>
		private IClassroomService ClassroomService;

		/// <summary>
		/// Constructor.
		/// </summary>
		public ClassroomController(
			IGroupService groupService, 
			IClassroomService classroomService) 
				: base(groupService)
		{
			ClassroomService = classroomService;
		}

		/// <summary>
		/// Shows all classrooms.
		/// </summary>
		[Route("Classrooms")]
		public async Task<IActionResult> Index()
		{
			var classrooms = await ClassroomService.GetClassroomsAsync(Group);

			return View(classrooms);
		}

		/// <summary>
		/// Shows the details of a classroom.
		/// </summary>
		[Route("Classrooms/{classroomName}/Details")]
		public async Task<IActionResult> Details(string classroomName)
		{
			if (classroomName == null)
			{
				return NotFound();
			}

			var classroom = await ClassroomService.GetClassroomAsync(Group, classroomName);
			if (classroom == null)
			{
				return NotFound();
			}

			return View(classroom);
		}

		/// <summary>
		/// Creates a new classroom.
		/// </summary>
		[Route("CreateClassroom")]
		public IActionResult Create()
		{
			return View();
		}

		/// <summary>
		/// Creates a new classroom.
		/// </summary>
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Route("CreateClassroom")]
		public async Task<IActionResult> Create(Classroom classroom)
		{
			if (ModelState.IsValid)
			{
				await ClassroomService.CreateClassroomAsync(Group, classroom);

				return RedirectToAction("Index");
			}
			else
			{
				return View(classroom);
			}
		}

		/// <summary>
		/// Edits a classroom.
		/// </summary>
		[Route("Classrooms/{classroomName}/Edit")]
		public async Task<IActionResult> Edit(string classroomName)
		{
			if (classroomName == null)
			{
				return NotFound();
			}

			var classroom = await ClassroomService.GetClassroomAsync(Group, classroomName);
			if (classroom == null)
			{
				return NotFound();
			}

			return View(classroom);
		}

		/// <summary>
		/// Edits a classroom.
		/// </summary>
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Route("Classrooms/{classroomName}/Edit")]
		public async Task<IActionResult> Edit(string classroomName, Classroom classroom)
		{
			if (ModelState.IsValid)
			{
				await ClassroomService.UpdateClassroomAsync(Group, classroom);

				return RedirectToAction("Index");
			}
			else
			{
				return View(classroom);
			}
		}

		/// <summary>
		/// Deletes a classroom.
		/// </summary>
		[Route("Classrooms/{classroomName}/Delete")]
		public async Task<IActionResult> Delete(string classroomName)
		{
			if (classroomName == null)
			{
				return NotFound();
			}

			var classroom = await ClassroomService.GetClassroomAsync(Group, classroomName);
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
		[Route("Classrooms/{classroomName}/Delete")]
		public async Task<IActionResult> DeleteConfirmed(string classroomName)
		{
			await ClassroomService.DeleteClassroomAsync(Group, classroomName);

			return RedirectToAction("Index");
		}
	}
}
