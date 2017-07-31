using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Model.Assignments.ServiceResults;

namespace CSC.CSClassroom.Service.Assignments.AssignmentScoring
{
	/// <summary>
	/// Calculates the score and status of an assignment group.
	/// </summary>
	public class AssignmentGroupScoreCalculator : IAssignmentGroupScoreCalculator
	{
		/// <summary>
		/// The submission status calculator.
		/// </summary>
		private readonly ISubmissionStatusCalculator _submissionStatusCalculator;

		/// <summary>
		/// Constructor.
		/// </summary>
		public AssignmentGroupScoreCalculator(
			ISubmissionStatusCalculator submissionStatusCalculator)
		{
			_submissionStatusCalculator = submissionStatusCalculator;
		}

		/// <summary>
		/// Returns the score for an assignment.
		/// </summary>
		public double GetAssignmentGroupScore(
			IList<AssignmentResult> assignmentResults,
			int roundDigits)
		{
			return Math.Round(assignmentResults.Sum(ar => ar.Score), roundDigits);
		}

		/// <summary>
		/// Returns the total points available for an assignment.
		/// </summary>
		public double GetAssignmentGroupTotalPoints(
			IList<Assignment> assignments,
			int roundDigits)
		{
			return Math.Round
			(
				assignments
					.SelectMany(a => a.Questions)
					.Sum(q => q.Points),
				roundDigits
			);
		}

		/// <summary>
		/// Returns the status for an assignment group.
		/// </summary>
		public SubmissionStatus GetAssignmentGroupStatus(
			IList<AssignmentResult> assignmentResults)
		{
			return _submissionStatusCalculator.GetStatusForAssignment
			(
				assignmentResults.Select(ar => ar.Status).ToList()
			);
		}
	}
}
