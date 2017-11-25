using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.Utilities;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Model.Users;
using CSC.CSClassroom.Service.Classrooms;
using CSC.CSClassroom.Service.Assignments;
using CSC.CSClassroom.Service.Identity;
using CSC.CSClassroom.WebApp.Filters;
using CSC.CSClassroom.WebApp.ViewModels.Assignment;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CSC.CSClassroom.WebApp.Controllers
{
	/// <summary>
	/// The assignment controller.
	/// </summary>
	[Route(ClassroomRoutePrefix)]
	public class AssignmentController : BaseClassroomController
	{
		/// <summary>
		/// The classroom service.
		/// </summary>
		private IAssignmentService AssignmentService { get; }

		/// <summary>
		/// The question service.
		/// </summary>
		private IQuestionService QuestionService { get; }

		/// <summary>
		/// The question category service.
		/// </summary>
		private IQuestionCategoryService QuestionCategoryService { get; }

		/// <summary>
		/// The section service.
		/// </summary>
		private ISectionService SectionService { get; }

		/// <summary>
		/// The user service.
		/// </summary>
		private IUserService UserService { get; }

		/// <summary>
		/// The assignment name representing all recently updated assignments.
		/// </summary>
		public const string AllRecentlyUpdated = "_AllRecentlyUpdated";

		/// <summary>
		/// Constructor.
		/// </summary>
		public AssignmentController(
			BaseControllerArgs args,
			IClassroomService classroomService,
			IAssignmentService assignmentService,
			ISectionService sectionService,
			IQuestionService questionService,
			IQuestionCategoryService questionCategoryService,
			IUserService userService)
				: base(args, classroomService)
		{
			AssignmentService = assignmentService;
			SectionService = sectionService;
			QuestionService = questionService;
			QuestionCategoryService = questionCategoryService;
			UserService = userService;
		}

		/// <summary>
		/// Shows all assignments.
		/// </summary>
		[Route("AssignmentsAdmin")]
		[ClassroomAuthorization(ClassroomRole.Admin)]
		public async Task<IActionResult> Admin()
		{
			var assignments = await AssignmentService.GetAssignmentsAsync(ClassroomName);

			return View(assignments);
		}

		/// <summary>
		/// Creates a new assignment.
		/// </summary>
		[Route("CreateAssignment")]
		[ClassroomAuthorization(ClassroomRole.Admin)]
		public async Task<IActionResult> Create()
		{
			await PopulateDropDownsAsync();

			return View("CreateEdit");
		}

		/// <summary>
		/// Creates a new assignment.
		/// </summary>
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Route("CreateAssignment")]
		[ClassroomAuthorization(ClassroomRole.Admin)]
		public async Task<IActionResult> Create(Assignment assignment)
		{
			if (ModelState.IsValid
				&& await AssignmentService.CreateAssignmentAsync(ClassroomName, assignment, ModelErrors))
			{
				return RedirectToAction("Admin");
			}
			else
			{
				await PopulateDropDownsAsync(assignment);

				return View("CreateEdit", assignment);
			}
		}

		/// <summary>
		/// Edits an assignment.
		/// </summary>
		[Route("Assignments/{id:int}/Edit")]
		[ClassroomAuthorization(ClassroomRole.Admin)]
		public async Task<IActionResult> Edit(int id, int? createdQuestionId = null)
		{
			var assignment = await AssignmentService.GetAssignmentAsync(ClassroomName, id);
			if (assignment == null)
			{
				return NotFound();
			}

			if (createdQuestionId != null)
			{
				var createdQuestion = await QuestionService.GetQuestionAsync
				(
					ClassroomName, 
					createdQuestionId.Value
				);

				assignment.Questions.Add
				(
					new AssignmentQuestion()
					{
						Name = createdQuestion.Name,
						Order = assignment.Questions.Count,
						Question = createdQuestion,
						QuestionId = createdQuestion.Id
					}	
				);
			}

			await PopulateDropDownsAsync(assignment);

			return View("CreateEdit", assignment);
		}

		/// <summary>
		/// Edits an assignment.
		/// </summary>
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Route("Assignments/{id:int}/Edit")]
		[ClassroomAuthorization(ClassroomRole.Admin)]
		public async Task<IActionResult> Edit(string assignmentName, Assignment assignment)
		{
			if (ModelState.IsValid
				&& await AssignmentService.UpdateAssignmentAsync(ClassroomName, assignment, ModelErrors))
			{
				return RedirectToAction("Admin");
			}
			else
			{
				await PopulateDropDownsAsync(assignment);

				return View("CreateEdit", assignment);
			}
		}

		/// <summary>
		/// Deletes a assignment.
		/// </summary>
		[Route("Assignments/{id:int}/Delete")]
		[ClassroomAuthorization(ClassroomRole.Admin)]
		public async Task<IActionResult> Delete(int id)
		{
			var assignment = await AssignmentService.GetAssignmentAsync(ClassroomName, id);
			if (assignment == null)
			{
				return NotFound();
			}

			return View(assignment);
		}

		/// <summary>
		/// Deletes a assignment.
		/// </summary>
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		[Route("Assignments/{id:int}/Delete")]
		[ClassroomAuthorization(ClassroomRole.Admin)]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			await AssignmentService.DeleteAssignmentAsync(ClassroomName, id);

			return RedirectToAction("Admin");
		}

		/// <summary>
		/// Asks the user to select an assignment report to show.
		/// </summary>
		[Route("AssignmentReport")]
		[ClassroomAuthorization(ClassroomRole.Admin)]
		public async Task<IActionResult> SectionReport(string sectionName, string assignment)
		{
			var assignments = await AssignmentService.GetAssignmentsAsync(ClassroomName);

			var assignmentGroups = assignments.GroupBy(a => a.GroupName)
				.OrderBy
				(
					g => g.Max
					(
						a => a.DueDates.Count > 0 ? a.DueDates.Max(d => d.DueDate) : DateTime.MinValue
					)
				)
				.Select(g => g.Key)
				.ToList();

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

			ViewBag.AssignmentGroups = new List<SelectListItem>
			(
				assignmentGroups.Select
				(
					groupName => new SelectListItem()
					{
						Text = groupName,
						Value = groupName,
						Selected = (assignment == groupName)
					}
				)
			)
			.OrderBy(s => s.Text, new NaturalComparer())
			.Union
			(
				new[]
				{
					new SelectListItem()
					{
						Text = "All Recently Updated",
						Value = AllRecentlyUpdated,
						Selected = (assignment == null)
					}
				}
			);

			ViewBag.GradebookNames = new List<SelectListItem>
			(
				Classroom.ClassroomGradebooks.Select
				(
					classroomGradebook => new SelectListItem()
					{
						Text = classroomGradebook.Name,
						Value = classroomGradebook.Name
					}
				)
			);

			return View("SelectSectionReport");
		}

		/// <summary>
		/// Returns an assignment report.
		/// </summary>
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Route("AssignmentReport")]
		[ClassroomAuthorization(ClassroomRole.Admin)]
		public async Task<IActionResult> SectionReport(SelectAssignmentReport selectAssignmentReport)
		{
			if (selectAssignmentReport.LastTransferDate != null)
			{
				await AssignmentService.MarkAssignmentsGradedAsync
				(
					ClassroomName,
					selectAssignmentReport.SectionName,
					selectAssignmentReport.GradebookName,
					selectAssignmentReport.LastTransferDate.Value
				);
			}

			if (selectAssignmentReport.AssignmentGroupName == AllRecentlyUpdated)
			{
				var updatedResults = await AssignmentService.GetUpdatedAssignmentResultsAsync
				(
					ClassroomName,
					selectAssignmentReport.SectionName,
					selectAssignmentReport.GradebookName
				);

				var report = new UpdatedSectionAssignmentsViewModel
				(
					updatedResults,
					new SelectAssignmentReport()
					{
						SectionName = selectAssignmentReport.SectionName,
						AssignmentGroupName = AllRecentlyUpdated,
						GradebookName = selectAssignmentReport.GradebookName,
						LastTransferDate = updatedResults.ResultsRetrievedDate
					},
					TimeZoneProvider,
					GetAssignmentDisplayProviderFactory()
				);

				return View("UpdatedAssignmentsReport", report);
			}
			else
			{
				var sectionResults = await AssignmentService.GetSectionAssignmentResultsAsync
				(
					ClassroomName,
					selectAssignmentReport.SectionName,
					selectAssignmentReport.AssignmentGroupName
				);

				var report = new SectionAssignmentGroupViewModel
				(
					sectionResults,
					GetAssignmentDisplayProviderFactory()
				);

				return View("SingleAssignmentReport", report);
			}
		}

		/// <summary>
		/// Returns an assignment report.
		/// </summary>
		[Route("Assignments")]
		[ClassroomAuthorization(ClassroomRole.General)]
		public async Task<IActionResult> Index(int? userId)
		{
			if (userId == null)
			{
				userId = User.Id;
			}

			if (userId != User.Id && ClassroomRole != ClassroomRole.Admin)
			{
				return Forbid();
			}

			var results = await AssignmentService.GetStudentAssignmentResultsAsync
			(
				ClassroomName, 
				userId.Value,
				ClassroomRole == ClassroomRole.Admin
			);

			var viewModel = new StudentAssignmentsViewModel
			(
				results,
				GetAssignmentDisplayProviderFactory()
			);

			return View(viewModel);
		}

		/// <summary>
		/// Returns an assignment display provider factory.
		/// </summary>
		private IAssignmentDisplayProviderFactory GetAssignmentDisplayProviderFactory()
		{
			return new AssignmentDisplayProviderFactory
			(
				TimeZoneProvider,
				GetAssignmentUrlProvider()
			);
		}

		/// <summary>
		/// Returns a question result display provider.
		/// </summary>
		private IAssignmentUrlProvider GetAssignmentUrlProvider()
		{
			return new AssignmentUrlProvider
			(
				Url,
				ClassroomRole >= ClassroomRole.Admin
			);
		}

		/// <summary>
		/// Populates dropdown lists required by the Create/Edit actions.
		/// </summary>
		private async Task PopulateDropDownsAsync(Assignment assignment = null)
		{
			ViewBag.AvailableQuestions = await QuestionService.GetQuestionsAsync(ClassroomName);

			var availableCategories = await QuestionCategoryService
				.GetQuestionCategoriesAsync(ClassroomName);

			var defaultCategory = assignment?.Questions
				?.LastOrDefault()
				?.Question
				?.QuestionCategory;

			ViewBag.AvailableCategories = availableCategories
				.OrderBy(qc => qc.Name, new NaturalComparer())
				.Select
				(
					category => new SelectListItem()
					{
						Text = category.Name,
						Value = category.Id.ToString(),
						Selected = (category == defaultCategory)
					}
				).ToList();
		}
	}
}
