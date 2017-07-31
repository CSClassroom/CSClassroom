using System.Threading.Tasks;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Model.Assignments.ServiceResults;

namespace CSC.CSClassroom.Service.Assignments.QuestionGraders
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
