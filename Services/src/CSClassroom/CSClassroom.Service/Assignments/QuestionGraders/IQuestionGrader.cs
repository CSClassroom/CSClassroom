using System.Threading.Tasks;
using CSC.CSClassroom.Model.Questions;
using CSC.CSClassroom.Model.Questions.ServiceResults;

namespace CSC.CSClassroom.Service.Questions.QuestionGraders
{
	/// <summary>
	/// Grades a question submission.
	/// </summary>
	public interface IQuestionGrader
	{
		/// <summary>
		/// Grades the question submission.
		/// </summary>
		Task<ScoredQuestionResult> GradeSubmissionAsync(QuestionSubmission submission);
	}
}
