using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Model.Assignments.ServiceResults;

namespace CSC.CSClassroom.Service.Assignments
{
	/// <summary>
	/// Performs assignment operations.
	/// </summary>
	public interface IAssignmentQuestionService
	{
		/// <summary>
		/// Returns a list of questions for the given assignment.
		/// </summary>
		Task<IList<AssignmentQuestion>> GetAssignmentQuestionsAsync(
			string classroomName,
			int assignmentId);

		/// <summary>
		/// Returns the question with the given ID.
		/// </summary>
		Task<QuestionToSolve> GetQuestionToSolveAsync(
			string classroomName,
			int assignmentId,
			int assignmentQuestionId,
			int userId);

		/// <summary>
		/// Returns the question with the given ID.
		/// </summary>
		Task<IList<QuestionToSolve>> GetQuestionsToSolveAsync(
			string classroomName,
			int assignmentId,
			int userId);

		/// <summary>
		/// Grades a question submission (returning and storing the result).
		/// </summary>
		Task<GradeSubmissionResult> GradeSubmissionAsync(
			string classroomName,
			int assignmentId,
			int userId,
			QuestionSubmission submission);

		/// <summary>
		/// Grades question submissions.
		/// </summary>
		Task<GradeSubmissionsResult> GradeSubmissionsAsync(
			string classroomName,
			int assignmentId,
			int userId,
			IList<QuestionSubmission> submissions);

		/// <summary>
		/// Returns a user's question submission.
		/// </summary>
		Task<SubmissionResult> GetSubmissionAsync(
			string classroomName,
			int assignmentId,
			int assignmentQuestionId,
			int userId,
			DateTime submissionDate);

		/// <summary>
		/// Returns a user's question submissions.
		/// </summary>
		Task<IList<SubmissionResult>> GetSubmissionsAsync(
			string classroomName,
			int assignmentId,
			int userId,
			DateTime submissionDate);
	}
}
