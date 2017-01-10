using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSC.CSClassroom.Model.Questions;
using CSC.CSClassroom.Model.Users;
using CSC.CSClassroom.Service.Classrooms;
using CSC.CSClassroom.Service.Questions;
using CSC.CSClassroom.WebApp.Filters;
using CSC.CSClassroom.WebApp.Utilities;
using CSC.CSClassroom.WebApp.ViewModels.Question;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CSC.CSClassroom.WebApp.Controllers
{
	/// <summary>
	/// The question controller.
	/// </summary>
	[Route(ClassroomRoutePrefix)]
	public class QuestionController : BaseClassroomController
	{
		/// <summary>
		/// The question service.
		/// </summary>
		private IQuestionService QuestionService { get; }

		/// <summary>
		/// The question category service.
		/// </summary>
		private IQuestionCategoryService QuestionCategoryService { get; }

		/// <summary>
		/// A list of supported question types.
		/// </summary>
		private static readonly IReadOnlyList<QuestionType> s_questionTypes;

		/// <summary>
		/// Caches the list of supported question types.
		/// </summary>
		static QuestionController()
		{
			s_questionTypes = new[]
			{
				typeof(MultipleChoiceQuestion),
				typeof(ShortAnswerQuestion),
				typeof(MethodQuestion),
				typeof(ClassQuestion),
				typeof(ProgramQuestion)
			}.Select
			(
				type => new QuestionType(type)
			).ToList();
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		public QuestionController(
			BaseControllerArgs args,
			IClassroomService classroomService,
			IQuestionService questionService,
			IQuestionCategoryService questionCategoryService)
				: base(args, classroomService)
		{
			QuestionService = questionService;
			QuestionCategoryService = questionCategoryService;
		}

		/// <summary>
		/// Shows all questions.
		/// </summary>
		[Route("Questions")]
		[ClassroomAuthorization(ClassroomRole.General)]
		public async Task<IActionResult> Index()
		{
			var questions = await QuestionService.GetQuestionsAsync(ClassroomName);
			var sortedQuestions = questions.OrderBy(q => q.Name, new NaturalComparer())
				.ToList();

			if (ClassroomRole >= ClassroomRole.Admin)
			{
				return View(sortedQuestions);
			}
			else
			{
				return View(sortedQuestions.Where(q => !q.IsPrivate).ToList());
			}
		}

		/// <summary>
		/// Creates a new question.
		/// </summary>
		[Route("CreateQuestion")]
		[ClassroomAuthorization(ClassroomRole.Admin)]
		public async Task<IActionResult> Create(string questionType)
		{
			var questionTypeObj = s_questionTypes.FirstOrDefault(type => type.Type.Name == questionType);
			if (questionTypeObj == null)
			{
				return View("CreateEdit/SelectType", s_questionTypes);
			}
			else
			{
				await PopulateDropDownsAsync();

				return View("CreateEdit", Activator.CreateInstance(questionTypeObj.Type));
			}
		}

		/// <summary>
		/// Creates a new question.
		/// </summary>
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Route("CreateQuestion")]
		[ClassroomAuthorization(ClassroomRole.Admin)]
		public async Task<IActionResult> Create(Question question)
		{
			if (ModelState.IsValid && 
				await QuestionService.CreateQuestionAsync(ClassroomName, question, ModelErrors))
			{
				return RedirectToAction("Index");
			}
			else
			{
				await PopulateDropDownsAsync();

				return View("CreateEdit", question);
			}
		}

		/// <summary>
		/// Creates a new question.
		/// </summary>
		[Route("DuplicateExistingQuestion")]
		[ClassroomAuthorization(ClassroomRole.Admin)]
		public async Task<IActionResult> DuplicateExisting(int id)
		{
			await PopulateDropDownsAsync();

			var question = await QuestionService
				.DuplicateExistingQuestionAsync(ClassroomName, id);

			ViewBag.ActionName = "Create";
			return View("CreateEdit", question);
		}

		/// <summary>
		/// Edits a question.
		/// </summary>
		[Route("Questions/{id}/Edit")]
		[ClassroomAuthorization(ClassroomRole.Admin)]
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var question = await QuestionService.GetQuestionAsync(ClassroomName, id.Value);
			if (question == null)
			{
				return NotFound();
			}
			
			await PopulateDropDownsAsync();

			return View("CreateEdit", question);
		}

		/// <summary>
		/// Edits a question.
		/// </summary>
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Route("Questions/{id}/Edit")]
		[ClassroomAuthorization(ClassroomRole.Admin)]
		public async Task<IActionResult> Edit(int? id, Question question)
		{
			if (id != question.Id)
			{
				return NotFound();
			}

			if (ModelState.IsValid 
				&& await QuestionService.UpdateQuestionAsync(ClassroomName, question, ModelErrors))
			{
				return RedirectToAction("Index");
			}
			else
			{
				await PopulateDropDownsAsync();

				return View("CreateEdit", question);
			}
		}

		/// <summary>
		/// Returns a new generated question template based off of an existing question. 
		/// The generated question is not saved unless and until it is submitted through 
		/// the Create action.
		/// </summary>
		[Route("GenerateFromExistingQuestion")]
		[ClassroomAuthorization(ClassroomRole.Admin)]
		public async Task<IActionResult> GenerateFromExisting(int id)
		{
			await PopulateDropDownsAsync();

			var question = await QuestionService.GenerateFromExistingQuestionAsync
			(
				ClassroomName,
				id
			);

			ViewBag.ActionName = "Create";
			return View("CreateEdit", question);
		}

		/// <summary>
		/// Shows a page allowing the submission of a solution to a question.
		/// </summary>
		[Route("Questions/{id}/Solve")]
		[ClassroomAuthorization(ClassroomRole.General)]
		public async Task<IActionResult> Solve(int id)
		{
			var questionToSolve = await QuestionService.GetQuestionToSolveAsync
			(
				ClassroomName, 
				User.Id,
				id
			);

			if (questionToSolve == null)
			{
				return NotFound();
			}

			return View(questionToSolve);
		}

		/// <summary>
		/// Shows a page allowing the submission of a solution to a question.
		/// </summary>
		[Route("Questions/{id}/Solve")]
		[HttpPost]
		[ClassroomAuthorization(ClassroomRole.General)]
		public async Task<IActionResult> Solve(int id, [FromBody] QuestionSubmission submission)
		{
			var result = await QuestionService.GradeSubmissionAsync
			(
				ClassroomName, 
				User.Id,
				id, 
				submission
			);

			if (result == null)
			{
				return NotFound();
			}

			return Ok(result);
		}

		/// <summary>
		/// Deletes a question.
		/// </summary>
		[Route("Questions/{id}/Delete")]
		[ClassroomAuthorization(ClassroomRole.Admin)]
		public async Task<IActionResult> Delete(int id)
		{
			var question = await QuestionService.GetQuestionAsync(ClassroomName, id);
			if (question == null)
			{
				return NotFound();
			}

			return View(question);
		}

		/// <summary>
		/// Deletes a question.
		/// </summary>
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		[Route("Questions/{id}/Delete")]
		[ClassroomAuthorization(ClassroomRole.Admin)]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			await QuestionService.DeleteQuestionAsync(ClassroomName, id);

			return RedirectToAction("Index");
		}

		/// <summary>
		/// Populates dropdown lists required by the Create/Edit actions.
		/// </summary>
		private async Task PopulateDropDownsAsync()
		{
			var questionCategories = await QuestionCategoryService
				.GetQuestionCategoriesAsync(ClassroomName);

			ViewBag.QuestionCategoryId = new SelectList(questionCategories, "Id", "Name");
			ViewBag.AvailableQuestions = await QuestionService.GetQuestionsAsync(ClassroomName);
		}
	}
}
