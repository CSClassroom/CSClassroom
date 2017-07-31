using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Model.Assignments.ServiceResults;
using CSC.CSClassroom.Service.Assignments.UserQuestionDataLoaders;

namespace CSC.CSClassroom.Service.Assignments.QuestionSolvers
{
	/// <summary>
	/// Functionality relating to solving questions.
	/// </summary>
	public interface IQuestionSolver
	{
		/// <summary>
		/// Returns the question with the given ID.
		/// </summary>
		Task<QuestionToSolve> GetQuestionToSolveAsync(
			UserQuestionDataStore userQuestionDataStore,
			int assignmentQuestionId);

		/// <summary>
		/// Grades a question submission.
		/// </summary>
		Task<GradeSubmissionResult> GradeSubmissionAsync(
			UserQuestionDataStore store,
			QuestionSubmission submission,
			DateTime dateSubmitted);

		/// <summary>
		/// Returns the submission result for the given submission.
		/// </summary>
		Task<SubmissionResult> GetSubmissionResultAsync(
			UserQuestionDataStore store,
			int assignmentQuestionId,
			DateTime submissionDate,
			DateTime? dueDate);
	}
}
