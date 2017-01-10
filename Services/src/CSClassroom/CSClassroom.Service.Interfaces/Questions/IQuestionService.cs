using System.Collections.Generic;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.Utilities;
using CSC.CSClassroom.Model.Questions;
using CSC.CSClassroom.Model.Questions.ServiceResults;

namespace CSC.CSClassroom.Service.Questions
{
	/// <summary>
	/// Performs question operations.
	/// </summary>
	public interface IQuestionService
	{
		/// <summary>
		/// Returns the list of questions.
		/// </summary>
		Task<IList<Question>> GetQuestionsAsync(string classroomName);

		/// <summary>
		/// Returns the question with the given id.
		/// </summary>
		Task<Question> GetQuestionAsync(string classroomName, int id);

		/// <summary>
		/// Creates a question.
		/// </summary>
		Task<bool> CreateQuestionAsync(
			string classroomName,
			Question question,
			IModelErrorCollection errors);

		/// <summary>
		/// Updates a question.
		/// </summary>
		Task<bool> UpdateQuestionAsync(
			string classroomName,
			Question question,
			IModelErrorCollection errors);

		/// <summary>
		/// Removes a question.
		/// </summary>
		Task DeleteQuestionAsync(string classroomName, int id);

		/// <summary>
		/// Returns the question with the given ID, along with any unsolved prerequisites
		/// for this question (if any), and the student's previous submission contents 
		/// (if any).
		/// </summary>
		Task<QuestionToSolve> GetQuestionToSolveAsync(
			string classroomName,
			int userId,
			int questionId);

		/// <summary>
		/// Returns a copy of an existing quesrtion. The copy is not saved unless and until
		/// it is submitted through CreateQuestionAsync.
		/// </summary>
		Task<Question> DuplicateExistingQuestionAsync(
			string classroomName,
			int existingQuestionId);

		/// <summary>
		/// Returns a new generated question template based off of an existing question. 
		/// The generated question is not saved unless and until it is submitted through 
		/// CreateQuestionAsync.
		/// </summary>
		Task<Question> GenerateFromExistingQuestionAsync(
			string classroomName, 
			int existingQuestionId);

		/// <summary>
		/// Grades a question submission (returning and storing the result).
		/// </summary>
		Task<ScoredQuestionResult> GradeSubmissionAsync(
			string classroomName,
			int userId,
			int questionId,
			QuestionSubmission submission);
	}
}
