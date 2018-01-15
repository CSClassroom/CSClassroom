using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.Utilities;
using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Model.Users;
using CSC.CSClassroom.Service.Assignments;
using CSC.CSClassroom.Service.Classrooms;
using CSC.CSClassroom.Service.Identity;
using CSC.CSClassroom.WebApp.Filters;
using CSC.CSClassroom.WebApp.ViewModels.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CSC.CSClassroom.WebApp.Controllers
{
	/// <summary>
	/// The section controller.
	/// </summary>
	[Route(ClassroomRoutePrefix)]
	public class SectionController : BaseClassroomController
	{
		/// <summary>
		/// The section service.
		/// </summary>
		private ISectionService SectionService { get; }

		/// <summary>
		/// The user service.
		/// </summary>
		private IUserService UserService { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public SectionController(
			BaseControllerArgs args,
			IClassroomService classroomService,
			ISectionService sectionService,
			IUserService userService)
				: base(args, classroomService)
		{
			SectionService = sectionService;
			UserService = userService;
		}

		/// <summary>
		/// Shows all sections.
		/// </summary>
		[Route("Sections")]
		[ClassroomAuthorization(ClassroomRole.Admin)]
		public IActionResult Index()
		{
			return View(Classroom.Sections);
		}

		/// <summary>
		/// Shows all sections.
		/// </summary>
		[Route("Sections/{sectionName}")]
		[ClassroomAuthorization(ClassroomRole.General)]
		public IActionResult SectionHome()
		{
			return RedirectToAction
			(
				"Index", 
				"ClassroomHome", 
				new { classroomName = ClassroomName }
			);
		}

		/// <summary>
		/// Creates a new section.
		/// </summary>
		[Route("CreateSection")]
		[ClassroomAuthorization(ClassroomRole.Admin)]
		public async Task<IActionResult> Create()
		{
			await PopulateDropDownsAsync();
			
			return View("CreateEdit");
		}

		/// <summary>
		/// Creates a new section.
		/// </summary>
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Route("CreateSection")]
		[ClassroomAuthorization(ClassroomRole.Admin)]
		public async Task<IActionResult> Create(Section section)
		{
			if (ModelState.IsValid 
			    && await SectionService.CreateSectionAsync(ClassroomName, section, ModelErrors)) 
			{
				return RedirectToAction("Index");
			}
			else
			{
				await PopulateDropDownsAsync();
				
				return View("CreateEdit", section);
			}
		}

		/// <summary>
		/// Edits a classroom.
		/// </summary>
		[Route("Sections/{sectionName}/Edit")]
		[ClassroomAuthorization(ClassroomRole.Admin)]
		public async Task<IActionResult> Edit(string sectionName)
		{
			if (sectionName == null)
			{
				return NotFound();
			}

			var section = await SectionService.GetSectionAsync(ClassroomName, sectionName);
			if (section == null)
			{
				return NotFound();
			}
			
			await PopulateDropDownsAsync();

			return View("CreateEdit", section);
		}

		/// <summary>
		/// Edits a section.
		/// </summary>
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Route("Sections/{sectionName}/Edit")]
		[ClassroomAuthorization(ClassroomRole.Admin)]
		public async Task<IActionResult> Edit(string sectionName, Section section)
		{
			if (ModelState.IsValid 
			    && await SectionService.UpdateSectionAsync(ClassroomName, section, ModelErrors))
			{
				return RedirectToAction("Index");
			}
			else
			{
				await PopulateDropDownsAsync();
				
				return View("CreateEdit", section);
			}
		}

		/// <summary>
		/// Deletes a section.
		/// </summary>
		[Route("Sections/{sectionName}/Delete")]
		[ClassroomAuthorization(ClassroomRole.Admin)]
		public async Task<IActionResult> Delete(string sectionName)
		{
			if (sectionName == null)
			{
				return NotFound();
			}

			var section = await SectionService.GetSectionAsync(ClassroomName, sectionName);
			if (section == null)
			{
				return NotFound();
			}

			return View(section);
		}

		/// <summary>
		/// Deletes a section.
		/// </summary>
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		[Route("Sections/{sectionName}/Delete")]
		[ClassroomAuthorization(ClassroomRole.Admin)]
		public async Task<IActionResult> DeleteConfirmed(string sectionName)
		{
			await SectionService.DeleteSectionAsync(ClassroomName, sectionName);

			return RedirectToAction("Index");
		}

		/// <summary>
		/// Populates dropdown lists required by the Create/Edit actions.
		/// </summary>
		private async Task PopulateDropDownsAsync()
		{
			var admins = await ClassroomService.GetClassroomAdminsAsync(ClassroomName);
			ViewBag.ClassroomAdmins = admins
				.OrderBy(a => a.User.LastName)
				.ThenBy(a => a.User.FirstName)
				.ToList();
		}
	}
}
