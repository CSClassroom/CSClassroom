using System.Collections.Generic;
using System.Threading.Tasks;
using CSC.CSClassroom.Model.Assignments;

namespace CSC.CSClassroom.Service.Assignments
{
	/// <summary>
	/// Performs question category operations.
	/// </summary>
	public interface IQuestionCategoryService
	{
		/// <summary>
		/// Returns the list of categories.
		/// </summary>
		Task<IList<QuestionCategory>> GetQuestionCategoriesAsync(string classroomName);

		/// <summary>
		/// Returns the category with the given id.
		/// </summary>
		Task<QuestionCategory> GetQuestionCategoryAsync(string classroomName, int id);

		/// <summary>
		/// Creates a category.
		/// </summary>
		Task CreateQuestionCategoryAsync(string classroomName, QuestionCategory questionCategory);

		/// <summary>
		/// Updates a category.
		/// </summary>
		Task UpdateQuestionCategoryAsync(string classroomName, QuestionCategory questionCategory);

		/// <summary>
		/// Removes a category.
		/// </summary>
		Task DeleteQuestionCategoryAsync(string classroomName, int id);
	}
}
