using System.Collections.Generic;

namespace CSC.CSClassroom.Model.Assignments.ServiceResults
{
	/// <summary>
	/// The result for a single student's assignment.
	/// </summary>
	public class AssignmentGroupResult
	{
		/// <summary>
		/// The student's last name.
		/// </summary>
		public string LastName { get; }

		/// <summary>
		/// The student's first name.
		/// </summary>
		public string FirstName { get; }

		/// <summary>
		/// The name of the assignment group.
		/// </summary>
		public string AssignmentGroupName { get; }

		/// <summary>
		/// The student's score for the assignment group.
		/// </summary>
		public double Score { get; }

		/// <summary>
		/// The total points the assignment group is out of.
		/// </summary>
		public double TotalPoints { get; }

		/// <summary>
		/// The results for each assignment in the group.
		/// </summary>
		public IList<AssignmentResult> AssignmentResults { get; }

		/// <summary>
		/// The status for the overall assignment.
		/// </summary>
		public SubmissionStatus Status { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public AssignmentGroupResult(
			string assignmentGroupName,
			string lastName, 
			string firstName, 
			double score, 
			double totalPoints,
			IList<AssignmentResult> assignmentResults,
			SubmissionStatus status)
		{
			AssignmentGroupName = assignmentGroupName;
			LastName = lastName;
			FirstName = firstName;
			Score = score;
			TotalPoints = totalPoints;
			AssignmentResults = assignmentResults;
			Status = status;
		}
	}
}
