using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.Extensions;
using CSC.Common.Infrastructure.Serialization;
using CSC.CSClassroom.Model.Projects;
using CSC.CSClassroom.Model.Projects.ServiceResults;
using CSC.CSClassroom.Model.Users;
using CSC.CSClassroom.Service.Classrooms;
using CSC.CSClassroom.Service.Projects;
using CSC.CSClassroom.WebApp.Extensions;
using CSC.CSClassroom.WebApp.Filters;
using CSC.CSClassroom.WebApp.ViewHelpers.NestedTables;
using CSC.CSClassroom.WebApp.ViewModels.Build;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace CSC.CSClassroom.WebApp.Controllers
{
	/// <summary>
	/// The section controller.
	/// </summary>
	[Route(ProjectRoutePrefix)]
	public class BuildController : BaseProjectController
	{
		/// <summary>
		/// The section service.
		/// </summary>
		private ISectionService SectionService { get; }

		/// <summary>
		/// The build service.
		/// </summary>
		private IBuildService BuildService { get; }

		/// <summary>
		/// The checkpoint service.
		/// </summary>
		private ICheckpointService CheckpointService { get; }

		/// <summary>
		/// The submission service.
		/// </summary>
		private ISubmissionService SubmissionService { get; }

		/// <summary>
		/// The time zone provider.
		/// </summary>
		private readonly IJsonSerializer _jsonSerializer;

		/// <summary>
		/// Constructor.
		/// </summary>
		public BuildController(
			BaseControllerArgs args,
			IClassroomService classroomService,
			ISectionService sectionService,
			IProjectService projectService,
			IBuildService buildService,
			ICheckpointService checkpointService,
			ISubmissionService submissionService,
			IJsonSerializer jsonSerializer)
				: base(args, classroomService, projectService)
		{
			SectionService = sectionService;
			BuildService = buildService;
			CheckpointService = checkpointService;
			SubmissionService = submissionService;
			_jsonSerializer = jsonSerializer;
		}

		/// <summary>
		/// Returns the user's latest build.
		/// </summary>
		[Route("Builds")]
		[ClassroomAuthorization(ClassroomRole.General)]
		public IActionResult Index()
		{
			return RedirectToAction("LatestBuildResult");
		}

		/// <summary>
		/// Returns the results of a build with the given ID.
		/// </summary>
		[Route("Builds/latestBuild")]
		[ClassroomAuthorization(ClassroomRole.General)]
		public async Task<IActionResult> LatestBuildResult(int? userId)
		{
			if (!Project.ExplicitSubmissionRequired)
			{
				return NotFound();
			}

			if (userId == null)
			{
				userId = User.Id;
			}

			if (userId != User.Id && ClassroomRole < ClassroomRole.Admin)
			{
				return Forbid();
			}

			var latestBuildResult = await BuildService.GetLatestBuildResultAsync
			(
				ClassroomName,
				ProjectName, 
				userId.Value
			);

			if (latestBuildResult == null)
			{
				return NotFound();
			}

			var unreadFeedback = await SubmissionService.GetUnreadFeedbackAsync
			(
				userId.Value
			);

			if (latestBuildResult.BuildResult != null)
			{
				var buildViewModel = GetBuildViewModelAsync
				(
					latestBuildResult.BuildResult, 
					unreadFeedback
				);

				return View("BuildResult", buildViewModel);
			}
			else
			{
				var buildInProgressViewModel = new BuildInProgressViewModel
				(
					latestBuildResult.Commit, 
					latestBuildResult.EstimatedBuildDuration.Value
				);

				return View("BuildInProgress", buildInProgressViewModel);
			}
		}

		/// <summary>
		/// Returns the results of a build with the given ID.
		/// </summary>
		[Route("Builds/{buildId:int}")]
		[ClassroomAuthorization(ClassroomRole.General)]
		public async Task<IActionResult> BuildResult(int buildId)
		{
			if (!Project.ExplicitSubmissionRequired)
			{
				return NotFound();
			}

			var buildResult = await BuildService.GetBuildResultAsync
			(
				ClassroomName, 
				ProjectName, 
				buildId
			);

			if (buildResult == null)
			{
				return NotFound();
			}

			if (buildResult.Build.Commit.UserId != User.Id 
				&& ClassroomRole < ClassroomRole.Admin)
			{
				return Forbid();
			}

			var unreadFeedback = await SubmissionService.GetUnreadFeedbackAsync
			(
				buildResult.Build.Commit.UserId
			);

			var viewModel = GetBuildViewModelAsync(buildResult, unreadFeedback);

			return View(viewModel);
		}

		/// <summary>
		/// Returns the results of all builds in the given section.
		/// </summary>
		[Route("Builds/{sectionName}")]
		[ClassroomAuthorization(ClassroomRole.Admin)]
		public async Task<IActionResult> SectionBuildResults(string sectionName)
		{
			if (!Project.ExplicitSubmissionRequired)
			{
				return NotFound();
			}

			var section = Classroom.Sections.SingleOrDefault(s => s.Name == sectionName);
			if (section == null)
			{
				return NotFound();
			}

			var builds = await BuildService.GetSectionBuildsAsync
			(
				ClassroomName, 
				ProjectName, 
				sectionName
			);

			var testCounts = GetTestCounts(builds);
			var tableInfo = GetSectionBuildsTableInfo(testCounts, builds);
			var tableData = GetSectionBuildsTableData(builds, testCounts.Keys);

			var viewModel = new SectionBuildResultsViewModel
			(
				section.DisplayName, 
				tableInfo, 
				tableData
			);

			return View(viewModel);
		}

		/// <summary>
		/// Returns the progress of the latest build.
		/// </summary>
		[Route("BuildProgress")]
		[ClassroomAuthorization(ClassroomRole.General)]
		public async Task<IActionResult> MonitorProgress(int userId)
		{
			if (!Project.ExplicitSubmissionRequired)
			{
				return NotFound();
			}

			if (userId != User.Id && ClassroomRole < ClassroomRole.Admin)
			{
				return Forbid();
			}

			var buildProgress = await BuildService.MonitorProgressAsync
			(
				ClassroomName,
				ProjectName,
				userId
			);

			if (buildProgress == null)
			{
				return NotFound();
			}

			return Json(buildProgress);
		}

		/// <summary>
		/// Returns a user's build history.
		/// </summary>
		[Route("BuildHistory")]
		[ClassroomAuthorization(ClassroomRole.General)]
		public async Task<IActionResult> History(int? userId)
		{
			if (!Project.ExplicitSubmissionRequired)
			{
				return NotFound();
			}

			if (userId == null)
			{
				userId = User.Id;
			}

			if (userId != User.Id && ClassroomRole < ClassroomRole.Admin)
			{
				return Forbid();
			}

			var builds = await BuildService.GetUserBuildsAsync
			(
				ClassroomName, 
				ProjectName, 
				userId.Value
			);

			if (builds.Count == 0)
			{
				return NotFound();
			}

			var viewModel = new HistoryViewModel(builds.First().Commit.User, builds);

			return View(viewModel);
		}

		/// <summary>
		/// Shows a single test result.
		/// </summary>
		[Route("Builds/{buildId:int}/TestResult/{testResultId:int}")]
		[ClassroomAuthorization(ClassroomRole.General)]
		public async Task<IActionResult> TestResult(int buildId, int testResultId)
		{
			if (!Project.ExplicitSubmissionRequired)
			{
				return NotFound();
			}

			var testResult = await BuildService.GetTestResultAsync
			(
				ClassroomName,
				ProjectName,
				testResultId
			);

			if (testResult == null || testResult.BuildId != buildId)
			{
				return NotFound();
			}

			if (testResult.Build.Commit.UserId != User.Id && ClassroomRole < ClassroomRole.Admin)
			{
				return Forbid();
			}

			var viewModel = new TestResultViewModel(testResult);

			return View(viewModel);
		}

		/// <summary>
		/// Returns the view model for the given build.
		/// </summary>
		private BuildViewModel GetBuildViewModelAsync(
			BuildResult buildResult,
			IList<UnreadFeedbackResult> unreadFeedback)
		{
			return new BuildViewModel
			(
				buildResult,
				unreadFeedback,
				b => b.Commit.GetCommitUrl(Url),
				testResult => Url.Action
				(
					"TestResult",
					new
					{
						buildId = testResult.BuildId,
						testResultId = testResult.Id
					}
				),
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
			);
		}

		/// <summary>
		/// Returns a list of test counts, by class name.
		/// </summary>
		private IDictionary<TestClass, int> GetTestCounts(IEnumerable<Build> builds)
		{
			var testCounts = new Dictionary<TestClass, int>();
			var buildLastSeen = new Dictionary<string, DateTime>();

			// We go through all builds, since some builds might have had tests
			// fail before running some test classes.
			foreach (var build in builds)
			{
				if (build.TestResults != null)
				{
					foreach (var testClass in build.Commit.Project.TestClasses)
					{
						if (!buildLastSeen.ContainsKey(testClass.ClassName) 
							|| build.Commit.PushDate > buildLastSeen[testClass.ClassName])
						{
							if (buildLastSeen.ContainsKey(testClass.ClassName))
							{
								testCounts.Remove(testClass);
							}

							testCounts[testClass] = build.TestResults
								.Count(tr => tr.ClassName == testClass.ClassName);

							buildLastSeen[testClass.ClassName] = build.Commit.PushDate;
						}
					}
				}
			}

			return testCounts;
		}

		/// <summary>
		/// Returns table metadata containing column info.
		/// </summary>
		private static TableInfo GetSectionBuildsTableInfo(
			IDictionary<TestClass, int> testCounts,
			IList<Build> builds)
		{
			var buildIdentifierColumns = new List<TableColumnInfo>()
			{
				new TableColumnInfo("LastName", "Last Name"),
				new TableColumnInfo("FirstName", "First Name"),
				new TableColumnInfo("BuildSucceeded", "Built?"),
				new TableColumnInfo("BuildTime", "Build time")
			};

			var allTestsColumn = new TableColumnInfo("AllTests", $"All tests ({testCounts.Values.Sum()})");

			var testResultColumns = testCounts
				.Keys
				.OrderBy(testClass => testClass.Order)
				.Select
				(
					testClass => new TableColumnInfo
					(
						testClass.ClassName.ToAlphaNumeric(),
						$"{testClass.DisplayName} ({testCounts[testClass]})"
					)
				).ToList();

			return new TableInfo
			(
				buildIdentifierColumns.Concat(new[] { allTestsColumn }).Concat(testResultColumns).ToList(),
				new List<TableInfo>() { new TableInfo(typeof(TestClassTableEntry), showHeader: true) },
				showHeader: true
			);
		}

		/// <summary>
		/// Returns the table data containing all build information.
		/// </summary>
		private JArray GetSectionBuildsTableData(
			IList<Build> builds, 
			ICollection<TestClass> testClasses)
		{
			return new JArray
			(
				builds.Select
				(
					build => new JObject
					(
						GetBuildInfo(build).Concat
						(
							new[] { GetAllTestCount(build) }
						).Concat
						(
							GetTestClassCounts(build, testClasses)
						).Concat
						(
							GetAllTestResults(build)
						).ToArray()
					)
				)
			);
		}

		/// <summary>
		/// Returns student build information.
		/// </summary>
		private JProperty[] GetBuildInfo(Build build)
		{
			var user = build.Commit.User;
			var buildUrl = Url.Action("BuildResult", new { buildId = build.Id });
			var buildLink = $"<a href=\"{buildUrl}\" target=\"_blank\">(Link)</a>";
			var buildTime = build.Commit.PushDate.FormatShortDateTime(TimeZoneProvider);

			return new[]
			{
				new JProperty("LastName", user.LastName),
				new JProperty("FirstName", user.FirstName),
				new JProperty("BuildSucceeded", $"{(build.Status == BuildStatus.Completed ? "Yes" : "No")} {buildLink}"),
				new JProperty("BuildTime", $"<span style=\"white-space:nowrap\">{buildTime}</span>")
			};
		}

		/// <summary>
		/// Returns the total number of tests the student passed.
		/// </summary>
		private static JProperty GetAllTestCount(Build build)
		{
			return new JProperty
			(
				"AllTests",
				build.TestResults?.Count(tr => tr.Succeeded) ?? 0
			);
		}

		/// <summary>
		/// Returns the number of tests the student passed in each class.
		/// </summary>
		private static JProperty[] GetTestClassCounts(Build build, ICollection<TestClass> testClasses)
		{
			return testClasses.Select
			(
				testClass => new JProperty
				(
					testClass.ClassName.ToAlphaNumeric(),
					build.TestResults
						?.Count
						(
							testResult => 
							   testResult.ClassName == testClass.ClassName &&
							   testResult.Succeeded
						) ?? 0
				)
			).ToArray();
		}

		/// <summary>
		/// Returns all test results.
		/// </summary>
		private object[] GetAllTestResults(Build build)
		{
			return build.TestResults != null
				? new[]
				{
					new JProperty
					(
						"childTableData",
						build.Commit.Project.TestClasses.Select
						(
							testClass => _jsonSerializer.SerializeToJObject
							(
								new TestClassTableEntry
								(
									testClass,

									false /*emphasized*/,

									build.TestResults
										.Where(tr => tr.ClassName == testClass.ClassName)
										.ToList(),

									tr => Url.Action
									(
										"TestResult", 
										new
										{
											buildId = build.Id,
											testResultId = tr.Id
										}
									)
								)
							)
						)
					)
				}
				: new object[0];
		}
	}
}
