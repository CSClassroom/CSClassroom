using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Model.Exercises;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CSC.CSClassroom.Service.Exercises
{
	/// <summary>
	/// Performs question category operations.
	/// </summary>
	public interface IQuestionCategoryService
	{
		/// <summary>
		/// Returns the list of categories.
		/// </summary>
		Task<IList<QuestionCategory>> GetQuestionCategoriesAsync(Group group);

		/// <summary>
		/// Returns the category with the given id.
		/// </summary>
		Task<QuestionCategory> GetQuestionCategoryAsync(Group group, int id);

		/// <summary>
		/// Creates a category.
		/// </summary>
		Task CreateQuestionCategoryAsync(Group group, QuestionCategory questionCategory);

		/// <summary>
		/// Updates a category.
		/// </summary>
		Task UpdateQuestionCategoryAsync(Group group, QuestionCategory questionCategory);

		/// <summary>
		/// Removes a category.
		/// </summary>
		/// <param name="categoryName">The ID of the category to remove.</param>
		Task DeleteQuestionCategoryAsync(Group group, int id);
	}
}
