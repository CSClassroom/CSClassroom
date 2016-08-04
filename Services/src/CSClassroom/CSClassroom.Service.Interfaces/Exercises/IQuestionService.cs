using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Model.Exercises;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CSC.CSClassroom.Service.Exercises
{
	/// <summary>
	/// Performs question operations.
	/// </summary>
	public interface IQuestionService
	{
		/// <summary>
		/// Returns the list of questions.
		/// </summary>
		Task<IList<Question>> GetQuestionsAsync(Group group);

		/// <summary>
		/// Returns the question with the given id.
		/// </summary>
		Task<Question> GetQuestionAsync(Group group, int id);

		/// <summary>
		/// Creates a question.
		/// </summary>
		Task CreateQuestionAsync(Group group, Question question);

		/// <summary>
		/// Updates a question.
		/// </summary>
		Task UpdateQuestionAsync(Group group, Question question);

		/// <summary>
		/// Removes a question.
		/// </summary>
		/// <param name="categoryName">The ID of the question to remove.</param>
		Task DeleteQuestionAsync(Group group, int id);
	}
}
