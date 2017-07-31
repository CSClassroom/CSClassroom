using System;
using System.Collections.Generic;
using System.Text;
using CSC.CSClassroom.Model.Assignments;

namespace CSC.CSClassroom.Service.Assignments.AssignmentScoring
{
	/// <summary>
	/// Returns the total score received for a question.
	/// </summary>
	public interface IQuestionScoreCalculator
	{
		/// <summary>
		/// Returns the score of the submission.
		/// </summary>
		double GetSubmissionScore(
			UserQuestionSubmission submission,
			DateTime? dueDate,
			double questionPoints,
			bool withLateness);
	}
}
