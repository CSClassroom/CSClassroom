using System.Threading.Tasks;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Model.Assignments.ServiceResults;

namespace CSC.CSClassroom.Service.Assignments.QuestionGraders
{
	/// <summary>
	/// Grades a question submission.
	/// </summary>
	public abstract class QuestionGrader<TQuestion> : IQuestionGrader where TQuestion : Question
	{
		/// <summary>
		/// The question to grade.
		/// </summary>
		protected TQuestion Question { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		protected QuestionGrader(TQuestion question)
		{
			Question = question;
		}

		/// <summary>
		/// Grades the question submission.
		/// </summary>
		public abstract Task<ScoredQuestionResult> GradeSubmissionAsync(
			QuestionSubmission submission);
	}
}
