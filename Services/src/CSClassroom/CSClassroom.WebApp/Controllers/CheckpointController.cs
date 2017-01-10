using System.Threading.Tasks;
using CSC.CSClassroom.Model.Projects;
using CSC.CSClassroom.Model.Users;
using CSC.CSClassroom.Service.Classrooms;
using CSC.CSClassroom.Service.Projects;
using CSC.CSClassroom.WebApp.Filters;
using Microsoft.AspNetCore.Mvc;

namespace CSC.CSClassroom.WebApp.Controllers
{
	/// <summary>
	/// The checkpoint controller.
	/// </summary>
	[Route(ProjectRoutePrefix)]
	public class CheckpointController : BaseProjectController
	{
		/// <summary>
		/// The section service.
		/// </summary>
		private ISectionService SectionService { get; }

		/// <summary>
		/// The checkpoint service.
		/// </summary>
		private ICheckpointService CheckpointService { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public CheckpointController(
			BaseControllerArgs args,
			IClassroomService classroomService,
			ISectionService sectionService,
			IProjectService projectService,
			ICheckpointService checkpointService)
				: base(args, classroomService, projectService)
		{
			SectionService = sectionService;
			CheckpointService = checkpointService;
		}

		/// <summary>
		/// Shows all sections.
		/// </summary>
		[Route("Checkpoints")]
		[ClassroomAuthorization(ClassroomRole.Admin)]
		public async Task<IActionResult> Index()
		{
			var checkpoints = await CheckpointService.GetCheckpointsAsync
			(
				ClassroomName, 
				ProjectName
			);

			return View(checkpoints);
		}

		/// <summary>
		/// Creates a new checkpoint.
		/// </summary>
		[Route("CreateCheckpoint")]
		[ClassroomAuthorization(ClassroomRole.Admin)]
		public IActionResult Create()
		{
			return View("CreateEdit");
		}

		/// <summary>
		/// Creates a new checkpoint.
		/// </summary>
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Route("CreateCheckpoint")]
		[ClassroomAuthorization(ClassroomRole.Admin)]
		public async Task<IActionResult> Create(Checkpoint checkpoint)
		{
			if (ModelState.IsValid)
			{
				var succeeded = await CheckpointService.CreateCheckpointAsync
				(
					ClassroomName,
					ProjectName,
					checkpoint,
					ModelErrors
				);

				if (succeeded)
				{
					return RedirectToAction("Index");
				}
			}

			return View("CreateEdit", checkpoint);
		}

		/// <summary>
		/// Edits a checkpoint.
		/// </summary>
		[Route("Checkpoints/{checkpointName}/Edit")]
		[ClassroomAuthorization(ClassroomRole.Admin)]
		public async Task<IActionResult> Edit(string checkpointName)
		{
			if (checkpointName == null)
			{
				return NotFound();
			}

			var checkpoint = await CheckpointService.GetCheckpointAsync
			(
				ClassroomName, 
				ProjectName, 
				checkpointName
			);

			if (checkpoint == null)
			{
				return NotFound();
			}

			return View("CreateEdit", checkpoint);
		}

		/// <summary>
		/// Edits a checkpoint.
		/// </summary>
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Route("Checkpoints/{checkpointName}/Edit")]
		[ClassroomAuthorization(ClassroomRole.Admin)]
		public async Task<IActionResult> Edit(Checkpoint checkpoint)
		{
			if (ModelState.IsValid)
			{
				var succeeded = await CheckpointService.UpdateCheckpointAsync
				(
					ClassroomName,
					ProjectName,
					checkpoint,
					ModelErrors
				);

				if (succeeded)
				{
					return RedirectToAction("Index");
				}
			}

			return View("CreateEdit", checkpoint);
		}

		/// <summary>
		/// Deletes a checkpoint.
		/// </summary>
		[Route("Checkpoints/{checkpointName}/Delete")]
		[ClassroomAuthorization(ClassroomRole.Admin)]
		public async Task<IActionResult> Delete(string checkpointName)
		{
			if (checkpointName == null)
			{
				return NotFound();
			}

			var checkpoint = await CheckpointService.GetCheckpointAsync
			(
				ClassroomName,
				ProjectName,
				checkpointName
			);

			if (checkpoint == null)
			{
				return NotFound();
			}

			return View(checkpoint);
		}

		/// <summary>
		/// Deletes a checkpoint.
		/// </summary>
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		[Route("Checkpoints/{checkpointName}/Delete")]
		[ClassroomAuthorization(ClassroomRole.Admin)]
		public async Task<IActionResult> DeleteConfirmed(string checkpointName)
		{
			await CheckpointService.DeleteCheckpointAsync
			(
				ClassroomName, 
				ProjectName, 
				checkpointName
			);

			return RedirectToAction("Index");
		}
	}
}
