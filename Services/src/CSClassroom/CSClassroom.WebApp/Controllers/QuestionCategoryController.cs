using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CSC.CSClassroom.Service.Exercises;
using CSC.CSClassroom.Model.Exercises;
using CSC.CSClassroom.Service.Classrooms;

namespace CSC.CSClassroom.WebApp.Controllers
{
	/// <summary>
	/// The question category controller.
	/// </summary>
	[Route(GroupRoutePrefix)]
	public class QuestionCategoryController : BaseGroupController
	{
		/// <summary>
		/// The category service.
		/// </summary>
		private IQuestionCategoryService QuestionCategoryService { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public QuestionCategoryController(
			IGroupService groupService, 
			IQuestionCategoryService questionCategoryService) 
				: base(groupService)
		{
			QuestionCategoryService = questionCategoryService;
		}

		/// <summary>
		/// Shows all categories.
		/// </summary>
		[Route("QuestionCategories")]
		public async Task<IActionResult> Index()
		{
			var categories = await QuestionCategoryService.GetQuestionCategoriesAsync(Group);

			return View(categories);
		}

		/// <summary>
		/// Shows the details of a category.
		/// </summary>
		[Route("QuestionCategories/{id}/Details")]
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var questionCategory = await QuestionCategoryService.GetQuestionCategoryAsync(Group, id.Value);
			if (questionCategory == null)
			{
				return NotFound();
			}

			return View(questionCategory);
		}

		/// <summary>
		/// Creates a new category.
		/// </summary>
		[Route("CreateQuestionCategory")]
		public IActionResult Create()
		{
			return View();
		}

		/// <summary>
		/// Creates a new category.
		/// </summary>
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Route("CreateQuestionCategory")]
		public async Task<IActionResult> Create(QuestionCategory questionCategory)
		{
			if (ModelState.IsValid)
			{
				await QuestionCategoryService.CreateQuestionCategoryAsync(Group, questionCategory);

				return RedirectToAction("Index");
			}
			else
			{
				return View(questionCategory);
			}
		}

		/// <summary>
		/// Edits a category.
		/// </summary>
		[Route("QuestionCategories/{id}/Edit")]
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var questionCategory = await QuestionCategoryService.GetQuestionCategoryAsync(Group, id.Value);
			if (questionCategory == null)
			{
				return NotFound();
			}

			return View(questionCategory);
		}

		/// <summary>
		/// Edits a category.
		/// </summary>
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Route("QuestionCategories/{id}/Edit")]
		public async Task<IActionResult> Edit(int? id, QuestionCategory questionCategory)
		{
			if (id != questionCategory.Id)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
			{
				await QuestionCategoryService.UpdateQuestionCategoryAsync(Group, questionCategory);

				return RedirectToAction("Index");
			}
			else
			{
				return View(questionCategory);
			}
		}

		/// <summary>
		/// Deletes a category.
		/// </summary>
		[Route("QuestionCategories/{id}/Delete")]
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var questionCategory = await QuestionCategoryService.GetQuestionCategoryAsync(Group, id.Value);
			if (questionCategory == null)
			{
				return NotFound();
			}

			return View(questionCategory);
		}

		/// <summary>
		/// Deletes a category.
		/// </summary>
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		[Route("QuestionCategories/{id}/Delete")]
		public async Task<IActionResult> DeleteConfirmed(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			await QuestionCategoryService.DeleteQuestionCategoryAsync(Group, id.Value);

			return RedirectToAction("Index");
		}
	}
}
