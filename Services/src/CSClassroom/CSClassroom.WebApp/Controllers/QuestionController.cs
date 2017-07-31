using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.Utilities;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Model.Users;
using CSC.CSClassroom.Service.Classrooms;
using CSC.CSClassroom.Service.Assignments;
using CSC.CSClassroom.WebApp.Filters;
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
		private static readonly IReadOnlyList<QuestionType> s_createQuestionTypes;

		/// <summary>
		/// A list of supported question types for randomly-selected question choices.
		/// </summary>
		private static readonly IReadOnlyList<QuestionType> s_randomlySelectedQuestionTypes;

		/// <summary>
		/// Caches the list of supported question types.
		/// </summary>
		static QuestionController()
		{
			s_createQuestionTypes = CreateQuestionTypes
			(
				typeof(MultipleChoiceQuestion),
				typeof(ShortAnswerQuestion),
				typeof(MethodQuestion),
				typeof(ClassQuestion),
				typeof(ProgramQuestion),
				typeof(RandomlySelectedQuestion)
			);

			s_randomlySelectedQuestionTypes = CreateQuestionTypes
			(
				typeof(MultipleChoiceQuestion),
				typeof(ShortAnswerQuestion)
			);
		}

		/// <summary>
		/// Creates a set of QuestionType objects for the given set of types.
		/// </summary>
		private static IReadOnlyList<QuestionType> CreateQuestionTypes(params Type[] types)
		{
			return types
				.Select(type => new QuestionType(type))
				.ToList();
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
		[ClassroomAuthorization(ClassroomRole.Admin)]
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
				return View(sortedQuestions.ToList());
			}
		}

		/// <summary>
		/// Creates a new question.
		/// </summary>
		[Route("CreateQuestion")]
		[ClassroomAuthorization(ClassroomRole.Admin)]
		public async Task<IActionResult> Create(string questionType, int? questionCategoryId)
		{
			QuestionCategory category = null;
			if (questionCategoryId.HasValue)
			{
				category = await QuestionCategoryService.GetQuestionCategoryAsync
				(
					ClassroomName,
					questionCategoryId.Value
				);
			}

			var questionTypes = category?.RandomlySelectedQuestionId != null
				? s_randomlySelectedQuestionTypes
				: s_createQuestionTypes;
			
			var questionTypeObj = questionTypes.FirstOrDefault(type => type.Type.Name == questionType);
				
			if (questionTypeObj == null)
			{
				return View
				(
					"CreateEdit/SelectType", 
					new SelectTypeViewModel(questionTypes, questionCategoryId)
				);
			}
			else
			{
				await PopulateDropDownsAsync();

				var newQuestion = (Question) Activator.CreateInstance(questionTypeObj.Type);
				newQuestion.QuestionCategory = category;
				newQuestion.QuestionCategoryId = category?.Id ?? 0;

				return View("CreateEdit", newQuestion);
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
			var category = await QuestionCategoryService.GetQuestionCategoryAsync
			(
				ClassroomName,
				question.QuestionCategoryId
			);

			if (ModelState.IsValid && 
				await QuestionService.CreateQuestionAsync(ClassroomName, question, ModelErrors))
			{
				if (question.HasChoices)
				{
					return RedirectToAction("QuestionChoices", new { id = question.Id });
				}
				else if (category.RandomlySelectedQuestionId.HasValue)
				{
					return RedirectToAction("QuestionChoices", new { id = category.RandomlySelectedQuestionId });
				}
				else
				{
					return RedirectToAction("Index");
				}
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
		public async Task<IActionResult> Edit(int? id, int? seed)
		{
			if (id == null)
			{
				return NotFound();
			}

			await PopulateDropDownsAsync();

			if (seed != null)
			{
				var questionInstance = await QuestionService.GetQuestionInstanceAsync
				(
					ClassroomName,
					id.Value,
					seed.Value
				);

				if (questionInstance == null)
				{
					return NotFound();
				}

				if (questionInstance.Question == null)
				{
					return View("CreateEdit/QuestionGenerationError", questionInstance);
				}

				ViewBag.Seed = seed;
				return View("CreateEdit", questionInstance.Question);
			}
			else
			{
				var question = await QuestionService.GetQuestionAsync(ClassroomName, id.Value);
				if (question == null)
				{
					return NotFound();
				}

				return View("CreateEdit", question);
			}
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

			var category = await QuestionCategoryService.GetQuestionCategoryAsync
			(
				ClassroomName,
				question.QuestionCategoryId
			);

			if (ModelState.IsValid 
				&& await QuestionService.UpdateQuestionAsync(ClassroomName, question, ModelErrors))
			{
				if (category.RandomlySelectedQuestionId.HasValue)
				{
					return RedirectToAction("QuestionChoices", new { id = category.RandomlySelectedQuestionId });
				}
				else
				{
					return RedirectToAction("Index");
				}
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
			var category = await QuestionService.DeleteQuestionAsync(ClassroomName, id);
			if (category.RandomlySelectedQuestionId.HasValue)
			{
				return RedirectToAction("QuestionChoices", new { id = category.RandomlySelectedQuestionId.Value });
			}
			else
			{
				return RedirectToAction("Index");
			}
		}

		/// <summary>
		/// Shows all questions.
		/// </summary>
		[Route("Question/{id}/Choices")]
		[ClassroomAuthorization(ClassroomRole.Admin)]
		public async Task<IActionResult> QuestionChoices(int id)
		{
			var choicesCategory = await QuestionService.GetQuestionChoicesAsync(ClassroomName, id);
			if (choicesCategory == null)
			{
				return NotFound();
			}

			return View(choicesCategory);
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
