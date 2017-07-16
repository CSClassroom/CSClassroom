using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSC.CSClassroom.Model.Questions;
using CSC.CSClassroom.Service.Classrooms;
using CSC.CSClassroom.Service.Questions;
using CSC.CSClassroom.WebApp.Filters;
using CSC.CSClassroom.Model.Users;
using CSC.CSClassroom.WebApp.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace CSC.CSClassroom.WebApp.Controllers
{
	/// <summary>
	/// The assignment controller.
	/// </summary>
	[Route(AssignmentRoutePrefix)]
	public class AssignmentQuestionController : BaseAssignmentController
	{
		/// <summary>
		/// The assignment question service.
		/// </summary>
		protected IAssignmentQuestionService AssignmentQuestionService { get; private set; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public AssignmentQuestionController(
			BaseControllerArgs args,
			IClassroomService classroomService,
			IAssignmentService assignmentService,
			IAssignmentQuestionService assignmentQuestionService)
			: base(args, classroomService, assignmentService)
		{
			AssignmentQuestionService = assignmentQuestionService;
		}

		[Route("Questions")]
		[ClassroomAuthorization(ClassroomRole.Admin)]
		public async Task<IActionResult> Index()
		{
			var assignmentQuestions = await AssignmentQuestionService.GetAssignmentQuestionsAsync
			(
				ClassroomName,
				AssignmentId
			);

			return View(assignmentQuestions);
		}

		/// <summary>
		/// Shows a page allowing the submission of a solution to a question.
		/// </summary>
		[Route("Questions/{id:int}/Solve")]
		[ClassroomAuthorization(ClassroomRole.General)]
		public async Task<IActionResult> Solve(int id, int? userId)
		{
			if (Assignment.CombinedSubmissions)
			{
				return NotFound();
			}

			if (!CanSeeAssignment())
			{
				return Forbid();
			}

			if (ClassroomRole < ClassroomRole.Admin)
			{
				userId = null;
			}

			var questionToSolve = await AssignmentQuestionService.GetQuestionToSolveAsync
			(
				ClassroomName,
				AssignmentId,
				id,
				userId ?? User.Id
			);

			if (questionToSolve == null)
			{
				return NotFound();
			}

			return View(questionToSolve);
		}

		/// <summary>
		/// Grades the submission to a question, and returns the result. 
		/// </summary>
		[Route("SolveQuestion")]
		[HttpPost]
		[ClassroomAuthorization(ClassroomRole.General)]
		public async Task<IActionResult> SolveQuestion([FromBody] QuestionSubmission submission)
		{
			if (Assignment.CombinedSubmissions)
			{
				return NotFound();
			}

			if (!CanSeeAssignment())
			{
				return Forbid();
			}

			var result = await AssignmentQuestionService.GradeSubmissionAsync
			(
				ClassroomName,
				AssignmentId,
				User.Id,
				submission
			);

			if (result == null)
			{
				return NotFound();
			}

			if (result.SubmissionDate.HasValue)
			{
				return RedirectToAction
				(
					"ViewSubmission",
					new
					{
						id = submission.AssignmentQuestionId,
						submissionDate = result.SubmissionDate.Value.ToEpoch()
					}
				);
			}
			else
			{
				return Ok(result.ScoredQuestionResult);
			}
		}

		/// <summary>
		/// Shows a page allowing the submission of solutions to all questions
		/// in an assignment with combined submissions.
		/// </summary>
		[Route("SolveAllQuestions")]
		[ClassroomAuthorization(ClassroomRole.General)]
		public async Task<IActionResult> SolveAllQuestions(int? userId)
		{
			if (!Assignment.CombinedSubmissions)
			{
				return NotFound();
			}

			if (!CanSeeAssignment())
			{
				return Forbid();
			}

			if (ClassroomRole < ClassroomRole.Admin)
			{
				userId = null;
			}

			var questionsToSolve = await AssignmentQuestionService.GetQuestionsToSolveAsync
			(
				ClassroomName,
				AssignmentId,
				userId ?? User.Id
			);

			return View("SolveAll", questionsToSolve);
		}

		/// <summary>
		/// Grades the solutions to all questions in an assignment with 
		/// combined submissions, and returns the results.
		/// </summary>
		[Route("SolveAllQuestions")]
		[HttpPost]
		[ClassroomAuthorization(ClassroomRole.General)]
		public async Task<IActionResult> SolveAll(IList<QuestionSubmission> submissions)
		{
			if (!Assignment.CombinedSubmissions)
			{
				return NotFound();
			}

			if (!CanSeeAssignment())
			{
				return Forbid();
			}

			var result = await AssignmentQuestionService.GradeSubmissionsAsync
			(
				ClassroomName,
				AssignmentId,
				User.Id,
				submissions
			);

			return RedirectToAction
			(
				"ViewAllSubmissions",
				new { submissionDate = result.SubmissionDate.ToEpoch() }
			);
		}

		/// <summary>
		/// Shows a submission to a question.
		/// </summary>
		[Route("Questions/{id}/ViewSubmission/{submissionDate}")]
		[ClassroomAuthorization(ClassroomRole.General)]
		public async Task<IActionResult> ViewSubmission(int id, int? userId, long submissionDate)
		{
			if (Assignment.CombinedSubmissions)
			{
				return NotFound();
			}

			if (!CanSeeAssignment())
			{
				return Forbid();
			}

			if (ClassroomRole < ClassroomRole.Admin)
			{
				userId = null;
			}

			var questionSubmission = await AssignmentQuestionService.GetSubmissionAsync
			(
				ClassroomName,
				AssignmentId,
				id,
				userId ?? User.Id,
				submissionDate.FromEpoch()
			);

			if (questionSubmission == null)
			{
				return NotFound();
			}

			return View(questionSubmission);
		}

		/// <summary>
		/// Shows all submissions for a given question.
		/// </summary>
		[Route("ViewSubmissions")]
		[ClassroomAuthorization(ClassroomRole.General)]
		public async Task<IActionResult> ViewAllSubmissions(int? userId, long submissionDate)
		{
			if (!Assignment.CombinedSubmissions)
			{
				return NotFound();
			}

			if (!CanSeeAssignment())
			{
				return Forbid();
			}

			if (ClassroomRole < ClassroomRole.Admin)
			{
				userId = null;
			}

			var questionSubmissions = await AssignmentQuestionService.GetSubmissionsAsync
			(
				ClassroomName,
				AssignmentId,
				userId ?? User.Id,
				submissionDate.FromEpoch()
			);

			if (!questionSubmissions.Any())
			{
				return NotFound();
			}

			return View(questionSubmissions);
		}

		/// <summary>
		/// Returns whether or not the user has permission to see the assignment.
		/// </summary>
		private bool CanSeeAssignment()
		{
			return !Assignment.IsPrivate || ClassroomRole >= ClassroomRole.Admin;
		}
	}
}