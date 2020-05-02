using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSC.CSClassroom.Model.Users;
using CSC.CSClassroom.Service.Classrooms;
using CSC.CSClassroom.Service.Projects;
using CSC.CSClassroom.Service.Identity;
using CSC.CSClassroom.WebApp.Extensions;
using CSC.CSClassroom.WebApp.Filters;
using CSC.CSClassroom.WebApp.Settings;
using CSC.CSClassroom.WebApp.ViewModels.Build;
using CSC.CSClassroom.WebApp.ViewModels.Submission;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CSC.CSClassroom.WebApp.Controllers
{
	/// <summary>
	/// The submission controller.
	/// </summary>
	[Route(CheckpointRoutePrefix)]
	public class SubmissionController : BaseCheckpointController
	{
		/// <summary>
		/// The submission service.
		/// </summary>
		private ISubmissionService SubmissionService { get; }

		/// <summary>
		/// The user service.
		/// </summary>
		private IUserService UserService { get; }

		/// <summary>
		/// The domain name of the service.
		/// </summary>
		private readonly WebAppHost _webAppHost;

		/// <summary>
		/// Constructor.
		/// </summary>
		public SubmissionController(
			BaseControllerArgs args,
			IClassroomService classroomService,
			IProjectService projectService,
			ICheckpointService checkpointService,
			ISubmissionService submissionService,
			IUserService userService,
			WebAppHost webAppHost)
			: base(args, classroomService, projectService, checkpointService)
		{
			SubmissionService = submissionService;
			UserService = userService;
			_webAppHost = webAppHost;
		}

		/// <summary>
		/// Asks the user to show a list of submissions.
		/// </summary>
		[Route("Submissions")]
		[ClassroomAuthorization(ClassroomRole.Admin)]
		public IActionResult Index()
		{
			ViewBag.SectionNames = new List<SelectListItem>
			(
				Classroom.Sections.Select
				(
					section => new SelectListItem()
					{
						Text = section.DisplayName,
						Value = section.Name
					}
				)
			);

			return View();
		}

		/// <summary>
		/// Asks the user to show a list of submissions.
		/// </summary>
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Route("Submissions")]
		[ClassroomAuthorization(ClassroomRole.Admin)]
		public IActionResult Index(string sectionName)
		{
			return RedirectToAction("List", new { sectionName });
		}

		/// <summary>
		/// Shows all submissions for the current checkpoint.
		/// </summary>
		[Route("Submissions/{sectionName}")]
		[ClassroomAuthorization(ClassroomRole.Admin)]
		public async Task<IActionResult> List(string sectionName)
		{
			var section = Classroom.Sections
				.SingleOrDefault(s => s.Name == sectionName);

			if (section == null)
			{
				return NotFound();
			}

			var submissionResults = await SubmissionService.GetSectionSubmissionsAsync
			(
				ClassroomName,
				ProjectName,
				CheckpointName,
				sectionName
			);

			return View(submissionResults);
		}

		/// <summary>
		/// Submits a checkpoint. 
		/// </summary>
		[Route("Submit")]
		[ClassroomAuthorization(ClassroomRole.General)]
		public async Task<IActionResult> Submit(int? userId)
		{
			if (userId == null)
			{
				userId = User.Id;
			}

			if (userId != User.Id && ClassroomRole < ClassroomRole.Admin)
			{
				return Forbid();
			}

			if (ClassroomRole < ClassroomRole.Admin)
			{
				var unreadFeedback = await SubmissionService.GetUnreadFeedbackAsync
				(
					ClassroomName,
					userId.Value
				);

				if (unreadFeedback.Any())
				{
					var viewModel = unreadFeedback.Select
					(
						uf => new UnreadFeedbackViewModel
						(
							uf,
							(projectName, checkpointName, submissionId) => Url.Action
							(
								"ViewFeedback",
								"Submission",
								new
								{
									projectName,
									checkpointName,
									submissionId
								}
							),
							TimeZoneProvider
						)
					).ToList();

					return View("UnreadFeedback", viewModel);
				}
			}

			var candidates = await SubmissionService.GetSubmissionCandidatesAsync
			(
				ClassroomName,
				ProjectName,
				userId.Value
			);

			var submissions = await SubmissionService.GetUserSubmissionsAsync
			(
				ClassroomName,
				ProjectName,
				userId.Value
			);

			var user = await UserService.GetUserAsync(userId.Value);

			var candidatesViewModel = new SubmissionCandidatesViewModel
			(
				user,
				candidates,
				commit => commit.GetCommitUrl(Url),
				Checkpoint,
				submissions
					.OrderByDescending(s => s.DateSubmitted)
					.FirstOrDefault(s => s.Checkpoint == Checkpoint),
				TimeZoneProvider
			);

			return View(candidatesViewModel);
		}

		/// <summary>
		/// Submits a checkpoint.
		/// </summary>
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Route("Submit")]
		[ClassroomAuthorization(ClassroomRole.General)]
		public async Task<IActionResult> Submit(int userId, int commitId)
		{
			if (userId != User.Id && ClassroomRole < ClassroomRole.Admin)
			{
				return Forbid();
			}

			var submission = await SubmissionService.SubmitCheckpointAsync
			(
				ClassroomName,
				ProjectName,
				CheckpointName,
				userId,
				commitId
			);

			var viewModel = new SubmittedViewModel
			(
				submission.Commit,
				submission.Commit.GetCommitUrl(Url),
				submission.Checkpoint,
				TimeZoneProvider
			);

			return View("Submitted", viewModel);
		}

		/// <summary>
		/// Downloads all submissions for the given section.
		/// </summary>
		[Route("Submissions/{sectionName}/Download")]
		[ClassroomAuthorization(ClassroomRole.Admin)]
		public async Task<IActionResult> Download(string sectionName)
		{
			var section = Classroom.Sections
				.SingleOrDefault(s => s.Name == sectionName);

			if (section == null)
			{
				return NotFound();
			}

			var archiveContents = await SubmissionService.DownloadSubmissionsAsync
			(
				ClassroomName,
				ProjectName,
				CheckpointName,
				sectionName
			);

			var timestamp = TimeZoneProvider.ToUserLocalTime(DateTime.UtcNow)
				.ToString("yyyyMMdd-HHmmss");

			var filename = $"{Classroom.Name}-{Project.Name}-{Checkpoint.Name}-{timestamp}.zip";

			return File(archiveContents, "application/zip", filename);
		}

		/// <summary>
		/// Submits a checkpoint.
		/// </summary>
		[Route("Submissions/{sectionName}/Grade")]
		[ClassroomAuthorization(ClassroomRole.Admin)]
		public async Task<IActionResult> Grade(string sectionName)
		{
			var section = Classroom.Sections
				.SingleOrDefault(s => s.Name == sectionName);

			if (section == null)
			{
				return NotFound();
			}

			var submissions = await SubmissionService.GradeSubmissionsAsync
			(
				ClassroomName,
				ProjectName,
				CheckpointName,
				sectionName
			);

			var viewModels = submissions.Select
			(
				submissionResult => new GradeSubmissionViewModel
				(
					Checkpoint,
					submissionResult,
					testResult => Url.Action
					(
						"TestResult",
						"Build",
						new
						{
							buildId = testResult.BuildId,
							testResultId = testResult.Id
						}
					),
					buildId => Url.Action
					(
						"BuildResult",
						"Build",
						new { buildId }
					),
					(commit, pullRequestNumber) => Url.GitHub().PullRequest
					(
						Classroom.GitHubOrganization,
						commit.GetRepoName(),
						pullRequestNumber
					),
					TimeZoneProvider
				)
			).ToList();

			return View(viewModels);
		}

		/// <summary>
		/// Updates submission feedback.
		/// </summary>
		[HttpPost]
		[Route("SaveSubmissionFeedback")]
		[ClassroomAuthorization(ClassroomRole.Admin)]
		public async Task<IActionResult> SaveFeedback(
			int submissionId, 
			string feedbackText)
		{
			await SubmissionService.SaveFeedbackAsync
			(
				ClassroomName,
				ProjectName,
				CheckpointName,
				submissionId, 
				feedbackText
			);

			return Ok();
		}

		/// <summary>
		/// Updates submission feedback.
		/// </summary>
		[HttpPost]
		[Route("Submissions/{sectionName}/SendFeedback")]
		[ClassroomAuthorization(ClassroomRole.Admin)]
		public async Task<IActionResult> SendFeedback(string sectionName)
		{
			var section = Classroom.Sections
				.SingleOrDefault(s => s.Name == sectionName);

			if (section == null)
			{
				return NotFound();
			}

			await SubmissionService.SendFeedbackAsync
			(
				ClassroomName,
				ProjectName,
				CheckpointName,
				sectionName,
				submission => $"{_webAppHost.HostName}" +
					Url.Action
					(
						"ViewFeedback", 
						new { submissionId = submission.Id }
					)
			);

			return RedirectToAction("Grade");
		}

		/// <summary>
		/// Submits a checkpoint.
		/// </summary>
		[Route("ViewSubmissionFeedback")]
		[ClassroomAuthorization(ClassroomRole.General)]
		public async Task<IActionResult> ViewFeedback(int submissionId)
		{
			var feedback = await SubmissionService.GetSubmissionFeedbackAsync
			(
				ClassroomName,
				ProjectName,
				CheckpointName,
				submissionId
			);

			if (feedback == null)
			{
				return NotFound();
			}

			if (ClassroomRole < ClassroomRole.Admin 
				&& feedback.UserId != User.Id)
			{
				return Forbid();
			}

			return View(feedback);
		}

		/// <summary>
		/// Marks submission feedback as read.
		/// </summary>
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Route("MarkSubmissionFeedbackRead")]
		[ClassroomAuthorization(ClassroomRole.General)]
		public async Task<IActionResult> MarkFeedbackRead(int submissionId)
		{
			await SubmissionService.MarkFeedbackReadAsync
			(
				ClassroomName,
				ProjectName,
				CheckpointName,
				submissionId,
				User.Id
			);

			return RedirectToAction("LatestBuildResult", "Build", new { userId = User.Id });
		}
	}
}
