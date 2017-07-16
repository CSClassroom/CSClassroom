using System;
using System.Collections.Generic;
using System.Text;
using CSC.CSClassroom.Model.Questions.ServiceResults;

namespace CSC.CSClassroom.Service.Questions.AssignmentScoring
{
	/// <summary>
	/// Calculates the status of a submission for a question 
	/// or an assignment.
	/// </summary>
	public interface ISubmissionStatusCalculator
	{
		/// <summary>
		/// Returns the submission status for a question.
		/// </summary>
		SubmissionStatus GetStatusForQuestion(
			DateTime? dateSubmitted,
			DateTime? dateDue,
			bool interactive,
			double score);

		/// <summary>
		/// Returns the submission status for an entire assignment.
		/// </summary>
		SubmissionStatus GetStatusForAssignment(
			IList<SubmissionStatus> questionStatus);
	}

}
