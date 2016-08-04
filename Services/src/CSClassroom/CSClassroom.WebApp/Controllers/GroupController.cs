using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Service.Classrooms;

namespace CSC.CSClassroom.WebApp.Controllers
{
	/// <summary>
	/// The group controller.
	/// </summary>
	public class GroupController : BaseController
	{
		/// <summary>
		/// The group service.
		/// </summary>
		private IGroupService GroupService { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public GroupController(IGroupService groupService)
		{
			GroupService = groupService;
		}

		/// <summary>
		/// Shows all groups.
		/// </summary>
		[Route("Groups")]
		public async Task<IActionResult> Index()
		{
			var groups = await GroupService.GetGroupsAsync();

			return View(groups);
		}

		/// <summary>
		/// Shows the details of a group.
		/// </summary>
		[Route("Groups/{groupName}/Details")]
		public async Task<IActionResult> Details(string groupName)
		{
			if (groupName == null)
			{
				return NotFound();
			}

			var group = await GroupService.GetGroupAsync(groupName);
			if (group == null)
			{
				return NotFound();
			}

			return View(group);
		}

		/// <summary>
		/// Creates a new group.
		/// </summary>
		[Route("CreateGroup")]
		public IActionResult Create()
		{
			return View();
		}

		/// <summary>
		/// Creates a new group.
		/// </summary>
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Route("CreateGroup")]
		public async Task<IActionResult> Create(Group group)
		{
			if (ModelState.IsValid)
			{
				await GroupService.CreateGroupAsync(group);

				return RedirectToAction("Index");
			}
			else
			{
				return View(group);
			}
		}

		/// <summary>
		/// Edits a group.
		/// </summary>
		[Route("Groups/{groupName}/Edit")]
		public async Task<IActionResult> Edit(string groupName)
		{
			if (groupName == null)
			{
				return NotFound();
			}

			var group = await GroupService.GetGroupAsync(groupName);
			if (group == null)
			{
				return NotFound();
			}

			return View(group);
		}

		/// <summary>
		/// Edits a group.
		/// </summary>
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Route("Groups/{groupName}/Edit")]
		public async Task<IActionResult> Edit(string groupName, Group group)
		{
			if (ModelState.IsValid)
			{
				await GroupService.UpdateGroupAsync(group);

				return RedirectToAction("Index");
			}
			else
			{
				return View(group);
			}
		}

		/// <summary>
		/// Deletes a group.
		/// </summary>
		[Route("Groups/{groupName}/Delete")]
		public async Task<IActionResult> Delete(string groupName)
		{
			if (groupName == null)
			{
				return NotFound();
			}

			var group = await GroupService.GetGroupAsync(groupName);
			if (group == null)
			{
				return NotFound();
			}

			return View(group);
		}

		/// <summary>
		/// Deletes a group.
		/// </summary>
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		[Route("Groups/{groupName}/Delete")]
		public async Task<IActionResult> DeleteConfirmed(string groupName)
		{
			await GroupService.DeleteGroupAsync(groupName);

			return RedirectToAction("Index");
		}
	}
}
