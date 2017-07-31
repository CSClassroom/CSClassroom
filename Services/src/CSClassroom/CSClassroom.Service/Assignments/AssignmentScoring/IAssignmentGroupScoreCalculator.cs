using System;
using System.Collections.Generic;
using System.Text;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Model.Assignments.ServiceResults;

namespace CSC.CSClassroom.Service.Assignments.AssignmentScoring
{
	/// <summary>
	/// Calculates the score and status of an assignment group.
	/// </summary>
	public interface IAssignmentGroupScoreCalculator
	{
		/// <summary>
		/// Returns the score for an assignment.
		/// </summary>
		double GetAssignmentGroupScore(
			IList<AssignmentResult> assignmentResults,
			int roundDigits);

		/// <summary>
		/// Returns the total points available for an assignment.
		/// </summary>
		double GetAssignmentGroupTotalPoints(
			IList<Assignment> assignments,
			int roundDigits);

		/// <summary>
		/// Returns the status for an assignment group.
		/// </summary>
		SubmissionStatus GetAssignmentGroupStatus(
			IList<AssignmentResult> assignmentResults);
	}
}
