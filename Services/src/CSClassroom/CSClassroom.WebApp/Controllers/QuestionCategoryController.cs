using System.Threading.Tasks;
using CSC.CSClassroom.Model.Questions;
using CSC.CSClassroom.Model.Users;
using CSC.CSClassroom.Service.Classrooms;
using CSC.CSClassroom.Service.Questions;
using CSC.CSClassroom.WebApp.Filters;
using Microsoft.AspNetCore.Mvc;

namespace CSC.CSClassroom.WebApp.Controllers
{
	/// <summary>
	/// The question category controller.
	/// </summary>
	[Route(ClassroomRoutePrefix)]
	[ClassroomAuthorization(ClassroomRole.Admin)]
	public class QuestionCategoryController : BaseClassroomController
	{
		/// <summary>
		/// The category service.
		/// </summary>
		private IQuestionCategoryService QuestionCategoryService { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public QuestionCategoryController(
			BaseControllerArgs args,
			IClassroomService classroomService, 
			IQuestionCategoryService questionCategoryService) 
				: base(args, classroomService)
		{
			QuestionCategoryService = questionCategoryService;
		}

		/// <summary>
		/// Shows all categories.
		/// </summary>
		[Route("QuestionCategories")]
		public async Task<IActionResult> Index()
		{
			var categories = await QuestionCategoryService
				.GetQuestionCategoriesAsync(ClassroomName);

			return View(categories);
		}

		/// <summary>
		/// Creates a new category.
		/// </summary>
		[Route("CreateQuestionCategory")]
		public IActionResult Create()
		{
			return View("CreateEdit");
		}

		/// <summary>
		/// Creates a new category.
		/// </summary>
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Route("CreateQuestionCategory")]
		public async Task<IActionResult> Create(QuestionCategory questionCategory)
		{
			questionCategory.RandomlySelectedQuestionId = null;

			if (ModelState.IsValid)
			{
				await QuestionCategoryService.CreateQuestionCategoryAsync
				(
					ClassroomName, 
					questionCategory
				);

				return RedirectToAction("Index");
			}
			else
			{
				return View("CreateEdit", questionCategory);
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

			var questionCategory = await QuestionCategoryService.GetQuestionCategoryAsync
			(
				ClassroomName, 
				id.Value
			);

			if (questionCategory == null || questionCategory.RandomlySelectedQuestionId != null)
			{
				return NotFound();
			}

			return View("CreateEdit", questionCategory);
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
				await QuestionCategoryService.UpdateQuestionCategoryAsync
				(
					ClassroomName, 
					questionCategory
				);

				return RedirectToAction("Index");
			}
			else
			{
				return View("CreateEdit", questionCategory);
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

			var questionCategory = await QuestionCategoryService.GetQuestionCategoryAsync
			(
				ClassroomName, 
				id.Value
			);

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

			await QuestionCategoryService.DeleteQuestionCategoryAsync(ClassroomName, id.Value);

			return RedirectToAction("Index");
		}
	}
}
