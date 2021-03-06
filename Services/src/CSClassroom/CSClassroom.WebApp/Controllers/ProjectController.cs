using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSC.BuildService.Model.ProjectRunner;
using CSC.Common.Infrastructure.Serialization;
using CSC.Common.Infrastructure.Utilities;
using CSC.CSClassroom.Model.Projects;
using CSC.CSClassroom.Model.Users;
using CSC.CSClassroom.Service.Classrooms;
using CSC.CSClassroom.Service.Projects;
using CSC.CSClassroom.WebApp.Filters;
using CSC.CSClassroom.WebApp.Settings;
using CSC.CSClassroom.WebApp.ViewModels.Project;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CSC.CSClassroom.WebApp.Controllers
{
	/// <summary>
	/// The project controller.
	/// </summary>
	[Route(ClassroomRoutePrefix)]
	public class ProjectController : BaseClassroomController
	{
		/// <summary>
		/// The section service.
		/// </summary>
		private ISectionService SectionService { get; }

		/// <summary>
		/// The classroom service.
		/// </summary>
		private IProjectService ProjectService { get; }

		/// <summary>
		/// The build service.
		/// </summary>
		private IBuildService BuildService { get; }

		/// <summary>
		/// The JSON serializer.
		/// </summary>
		private readonly IJsonSerializer _jsonSerializer;

		/// <summary>
		/// The domain name.
		/// </summary>
		private readonly WebAppHost _webAppHost;

		/// <summary>
		/// Constructor.
		/// </summary>
		public ProjectController(
			BaseControllerArgs args,
			IClassroomService classroomService,
			ISectionService sectionService,
			IProjectService projectService,
			IBuildService buildService,
			IJsonSerializer jsonSerializer,
			WebAppHost webAppHost)
				: base(args, classroomService)
		{
			SectionService = sectionService;
			ProjectService = projectService;
			BuildService = buildService;
			_jsonSerializer = jsonSerializer;
			_webAppHost = webAppHost;
		}

		/// <summary>
		/// Shows all projects.
		/// </summary>
		[Route("ProjectsAdmin")]
		[ClassroomAuthorization(ClassroomRole.Admin)]
		public async Task<IActionResult> Admin()
		{
			var projects = await ProjectService.GetProjectsAsync(ClassroomName);

			return View(projects);
		}

		/// <summary>
		/// Shows the status of all proejcts.
		/// </summary>
		[Route("Projects")]
		[ClassroomAuthorization(ClassroomRole.General)]
		public async Task<IActionResult> Index(int? userId)
		{
			if (userId == null)
			{
				userId = User.Id;
			}

			if (userId != User.Id && ClassroomRole < ClassroomRole.Admin)
			{
				return Forbid();
			}

			var projectStatusResults = await ProjectService.GetProjectStatusAsync
			(
				ClassroomName,
				userId.Value
			);

			var viewModel = new ProjectStatusResultsViewModel
			(
				projectStatusResults,
				TimeZoneProvider
			);

			return View(viewModel);
		}

		/// <summary>
		/// Asks the user to select a project report to show.
		/// </summary>
		[ClassroomAuthorization(ClassroomRole.Admin)]
		[Route("ProjectReport")]
		public async Task<IActionResult> SectionReport(
			string sectionName, 
			string projectName)
		{
			var projects = await ProjectService.GetProjectsAsync(ClassroomName);

			ViewBag.SectionNames = new List<SelectListItem>
			(
				Classroom.Sections.OrderBy(s => s.Name).Select
				(
					section => new SelectListItem()
					{
						Text = section.DisplayName,
						Value = section.Name,
						Selected = (sectionName == section.Name)
					}
				)
			);

			ViewBag.ProjectNames = new List<SelectListItem>
			(
				projects.OrderBy(p => p.Name, new NaturalComparer()).Select
				(
					project => new SelectListItem()
					{
						Text = project.Name,
						Value = project.Name,
						Selected = (project.Name == projectName)
					}
				)
			);
			
			return View("SelectSectionReport");
		}

		/// <summary>
		/// Returns a project report.
		/// </summary>
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Route("ProjectReport")]
		[ClassroomAuthorization(ClassroomRole.Admin)]
		public IActionResult SectionReport(
			SelectProjectReport selectProjectReport)
		{
			return RedirectToAction
			(
				"SectionBuildResults",
				"Build",
				new
				{
					projectName = selectProjectReport.ProjectName,
					sectionName = selectProjectReport.SectionName
				}
			);
		}

		/// <summary>
		/// Creates a new project.
		/// </summary>
		[Route("CreateProject")]
		[ClassroomAuthorization(ClassroomRole.Admin)]
		public IActionResult Create()
		{
			return View("CreateEdit");
		}

		/// <summary>
		/// Creates a new project.
		/// </summary>
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Route("CreateProject")]
		[ClassroomAuthorization(ClassroomRole.Admin)]
		public async Task<IActionResult> Create(Project project)
		{
			if (ModelState.IsValid)
			{
				await ProjectService.CreateProjectAsync(ClassroomName, project);

				return RedirectToAction("Admin");
			}
			else
			{
				return View("CreateEdit", project);
			}
		}

		/// <summary>
		/// Edits a project.
		/// </summary>
		[Route("Projects/{projectName}/Edit")]
		[ClassroomAuthorization(ClassroomRole.Admin)]
		public async Task<IActionResult> Edit(string projectName)
		{
			if (projectName == null)
			{
				return NotFound();
			}

			var project = await ProjectService.GetProjectAsync(ClassroomName, projectName);
			if (project == null)
			{
				return NotFound();
			}

			return View("CreateEdit", project);
		}

		/// <summary>
		/// Edits a project.
		/// </summary>
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Route("Projects/{projectName}/Edit")]
		[ClassroomAuthorization(ClassroomRole.Admin)]
		public async Task<IActionResult> Edit(string projectName, Project project)
		{
			if (ModelState.IsValid)
			{
				await ProjectService.UpdateProjectAsync(ClassroomName, project);

				return RedirectToAction("Admin");
			}
			else
			{
				return View("CreateEdit", project);
			}
		}

		/// <summary>
		/// Deletes a project.
		/// </summary>
		[Route("Projects/{projectName}/Delete")]
		[ClassroomAuthorization(ClassroomRole.Admin)]
		public async Task<IActionResult> Delete(string projectName)
		{
			if (projectName == null)
			{
				return NotFound();
			}

			var project = await ProjectService.GetProjectAsync(ClassroomName, projectName);
			if (project == null)
			{
				return NotFound();
			}

			return View(project);
		}

		/// <summary>
		/// Deletes a project.
		/// </summary>
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		[Route("Projects/{projectName}/Delete")]
		[ClassroomAuthorization(ClassroomRole.Admin)]
		public async Task<IActionResult> DeleteConfirmed(string projectName)
		{
			await ProjectService.DeleteProjectAsync(ClassroomName, projectName);

			return RedirectToAction("Admin");
		}

		/// <summary>
		/// Creates student repositories.
		/// </summary>
		[Route("Projects/{projectName}/CreateStudentRepositories")]
		[ClassroomAuthorization(ClassroomRole.Admin)]
		public async Task<IActionResult> CreateStudentRepositories(string projectName)
		{
			var project = await ProjectService.GetProjectAsync(ClassroomName, projectName);
			if (project == null)
			{
				return NotFound();
			}

			var fileList = await ProjectService.GetTemplateFileListAsync(ClassroomName, projectName);

			return View
			(
				new CreateStudentReposViewModel(project, fileList)
			);
		}

		/// <summary>
		/// Creates student repositories.
		/// </summary>
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Route("Projects/{projectName}/CreateStudentRepositories")]
		[ClassroomAuthorization(ClassroomRole.Admin)]
		public async Task<IActionResult> CreateStudentRepositories(
			string projectName, 
			string sectionName, 
			bool overwrite)
		{
			var webhookUrl = GetWebhookUrl();

			var results = await ProjectService.CreateStudentRepositoriesAsync
			(
				ClassroomName,
				projectName, 
				sectionName,
				webhookUrl,
				overwrite
			);

			return View("CreateStudentRepositoryResults", results);
		}

		/// <summary>
		/// Called when a commit is pushed to a GitHub repository
		/// </summary>
		[HttpPost]
		[Route("Projects/OnRepositoryPush")]
		[Authorization(RequiredAccess.Anonymous)]
		public async Task<IActionResult> OnRepositoryPush()
		{
			string gitHubEventHeader = Request.Headers["X-GitHub-Event"];
			if (gitHubEventHeader != "push" && gitHubEventHeader != "ping")
				return BadRequest();

			byte[] requestBody;
			using (var contentStream = new MemoryStream())
			{
				await Request.Body.CopyToAsync(contentStream);

				requestBody = contentStream.ToArray();
				string signature = Request.Headers["X-Hub-Signature"];
				
				if (!ProjectService.VerifyGitHubWebhookPayloadSigned(requestBody, signature))
					return BadRequest();
			}

			if (gitHubEventHeader == "ping")
				return NoContent();
			
			await ProjectService.OnRepositoryPushAsync
			(
				ClassroomName,
				Encoding.UTF8.GetString(requestBody),
				Url.Action("OnBuildCompleted")
			);

			return NoContent();
		}

		/// <summary>
		/// Called to check GitHub for any commits that did not trigger a
		/// webhook notification, for a single student.
		/// </summary>
		[Route("Projects/{projectName}/CheckForCommits")]
		[ClassroomAuthorization(ClassroomRole.General)]
		public async Task<IActionResult> CheckForCommits(string projectName, int? userId)
		{
			if (userId == null)
			{
				userId = User.Id;
			}

			if (ClassroomRole < ClassroomRole.Admin && userId != User.Id)
			{
				return Forbid();
			}

			var project = await ProjectService.GetProjectAsync(ClassroomName, projectName);
			if (project == null)
			{
				return NotFound();
			}
			
			return View(new CheckForCommitsViewModel(project, userId.Value));
		}

		/// <summary>
		/// Called to check GitHub for any commits that did not trigger a
		/// webhook notification, for a single student.
		/// </summary>
		[HttpPost]
		[Route("Projects/{projectName}/CheckForCommits")]
		[ClassroomAuthorization(ClassroomRole.General)]
		public async Task<IActionResult> CheckForCommitsConfirmed(string projectName, int? userId)
		{
			if (userId == null)
			{
				userId = User.Id;
			}

			if (ClassroomRole < ClassroomRole.Admin && userId != User.Id)
			{
				return Forbid();
			}

			var result = await ProjectService.ProcessMissedCommitsForStudentAsync
			(
				ClassroomName,
				projectName,
				userId.Value,
				Url.Action("OnBuildCompleted")
			);

			if (!result)
			{
				return NotFound();
			}

			return RedirectToAction
			(
				"LatestBuildResult", 
				"Build", 
				userId != User.Id ? new { userId } : null
			);
		}

		/// <summary>
		/// Called to check GitHub for any commits that did not trigger a
		/// webhook notification.
		/// </summary>
		[Route("Projects/{projectName}/CheckForCommitsAllStudents")]
		[ClassroomAuthorization(ClassroomRole.Admin)]
		public async Task<IActionResult> CheckForCommitsAllStudents(string projectName)
		{
			var project = await ProjectService.GetProjectAsync(ClassroomName, projectName);
			if (project == null)
			{
				return NotFound();
			}

			return View(project);
		}

		/// <summary>
		/// Called to check GitHub for any commits that did not trigger a
		/// webhook notification.
		/// </summary>
		[HttpPost]
		[Route("Projects/{projectName}/CheckForCommitsAllStudents")]
		[ClassroomAuthorization(ClassroomRole.Admin)]
		public async Task<IActionResult> CheckForCommitsAllStudentsConfirmed(string projectName)
		{
			var result = await ProjectService.ProcessMissedCommitsForAllStudentsAsync
			(
				ClassroomName,
				projectName,
				Url.Action("OnBuildCompleted")
			);

			if (!result)
			{
				return NotFound();
			}

			return RedirectToAction("Admin");
		}

		/// <summary>
		/// Called when a commit is pushed to a GitHub repository
		/// </summary>
		[HttpPost]
		[Route("Projects/OnBuildCompleted")]
		[Authorization(RequiredAccess.Anonymous)]
		public async Task<IActionResult> OnBuildCompleted([FromBody] ProjectJobResult projectJobResult)
		{
			await BuildService.OnBuildCompletedAsync(projectJobResult);

			return NoContent();
		}

		/// <summary>
		/// Returns the webhook URL.
		/// </summary>
		private string GetWebhookUrl()
		{
			return $"{_webAppHost.HostName}{Url.Action("OnRepositoryPush")}";
		}
	}
}
