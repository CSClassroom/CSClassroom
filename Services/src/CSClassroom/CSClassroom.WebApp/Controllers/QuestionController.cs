using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CSC.CSClassroom.Service.Exercises;
using CSC.CSClassroom.Model.Exercises;
using Microsoft.AspNetCore.Mvc.Rendering;
using CSC.CSClassroom.Service.Classrooms;
using CSC.CSClassroom.WebApp.ViewModels.Question;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System;

namespace CSC.CSClassroom.WebApp.Controllers
{
	/// <summary>
	/// The question controller.
	/// </summary>
	[Route(GroupRoutePrefix)]
	public class QuestionController : BaseGroupController
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
			s_questionTypes = typeof(Question).GetTypeInfo()
				.Assembly
				.GetTypes()
				.Where(type => typeof(Question).IsAssignableFrom(type))
				.Where(type => !type.GetTypeInfo().IsAbstract)
				.Select(type => new QuestionType(type))
				.ToList();
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		public QuestionController(
			IGroupService groupService, 
			IQuestionService questionService, 
			IQuestionCategoryService questionCategoryService)
				: base(groupService)
		{
			QuestionService = questionService;
			QuestionCategoryService = questionCategoryService;
		}

		/// <summary>
		/// Shows all questions.
		/// </summary>
		[Route("Questions")]
		public async Task<IActionResult> Index()
		{
			var questions = await QuestionService.GetQuestionsAsync(Group);

			return View(questions);
		}

		/// <summary>
		/// Shows the details of a question.
		/// </summary>
		[Route("Questions/{id}/Details")]
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var question = await QuestionService.GetQuestionAsync(Group, id.Value);
			if (question == null)
			{
				return NotFound();
			}

			return View(question);
		}

		/// <summary>
		/// Creates a new question.
		/// </summary>
		[Route("CreateQuestion")]
		public async Task<IActionResult> Create(string questionType)
		{
			var questionTypeObj = s_questionTypes.FirstOrDefault(type => type.Type.Name == questionType);
			if (questionTypeObj == null)
			{
				return View("Create/SelectType", s_questionTypes);
			}
			else
			{
				await PopulateQuestionCategoryListAsync();

				return View(Activator.CreateInstance(questionTypeObj.Type));
			}
		}

		/// <summary>
		/// Creates a new question.
		/// </summary>
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Route("CreateQuestion")]
		public async Task<IActionResult> Create(string questionType, Question question)
		{
			var questionTypeObj = s_questionTypes.FirstOrDefault(type => type.Type.Name == questionType);
			if (questionTypeObj == null)
			{
				return NotFound();
			}

			await PopulateQuestionCategoryListAsync();

			if (ModelState.IsValid)
			{
				await QuestionService.CreateQuestionAsync(Group, question);

				return RedirectToAction("Index");
			}
			else
			{
				return View(question);
			}
		}

		/// <summary>
		/// Edits a question.
		/// </summary>
		[Route("Questions/{id}/Edit")]
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			await PopulateQuestionCategoryListAsync();

			var question = await QuestionService.GetQuestionAsync(Group, id.Value);
			if (question == null)
			{
				return NotFound();
			}

			return View(question);
		}

		/// <summary>
		/// Edits a question.
		/// </summary>
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Route("Questions/{id}/Edit")]
		public async Task<IActionResult> Edit(int? id, Question question)
		{
			if (id != question.Id)
			{
				return NotFound();
			}

			await PopulateQuestionCategoryListAsync();

			if (ModelState.IsValid)
			{
				await QuestionService.UpdateQuestionAsync(Group, question);

				return RedirectToAction("Index");
			}
			else
			{
				return View(question);
			}
		}

		/// <summary>
		/// Deletes a question.
		/// </summary>
		[Route("Questions/{id}/Delete")]
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var question = await QuestionService.GetQuestionAsync(Group, id.Value);
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
		public async Task<IActionResult> DeleteConfirmed(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			await QuestionService.DeleteQuestionAsync(Group, id.Value);

			return RedirectToAction("Index");
		}

		/// <summary>
		/// Adds the the question category list to the view data.
		/// </summary>
		private async Task PopulateQuestionCategoryListAsync()
		{
			var questionCategories = await QuestionCategoryService.GetQuestionCategoriesAsync(Group);

			ViewData["QuestionCategoryId"] = new SelectList(questionCategories, "Id", "Name");
		}
	}
}
