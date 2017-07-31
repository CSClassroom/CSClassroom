using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSC.CSClassroom.Model.Assignments.ServiceResults;

namespace CSC.CSClassroom.WebApp.ViewModels.Assignment
{
	/// <summary>
	/// Provides URLs related to an assignment.
	/// </summary>
	public interface IAssignmentUrlProvider
	{
		/// <summary>
		/// Returns the URL for the assignment.
		/// </summary>
		string GetAssignmentUrl(int assignmentId, int userId);

		/// <summary>
		/// Returns the URL for an assignment submission.
		/// </summary>
		string GetAssignmentSubmissionUrl(
			int assignmentId,
			DateTime submissionDate,
			int userId);

		/// <summary>
		/// Returns the URL for the question.
		/// </summary>
		string GetQuestionUrl(
			int assignmentId,
			int questionId,
			int userId);

		/// <summary>
		/// Returns the URL for a question submission.
		/// </summary>
		string GetQuestionSubmissionUrl(
			int assignmentId,
			int questionId,
			DateTime submissionDate,
			int userId);
	}
}
