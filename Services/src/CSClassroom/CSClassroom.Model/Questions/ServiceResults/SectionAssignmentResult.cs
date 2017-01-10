using System.Collections.Generic;

namespace CSC.CSClassroom.Model.Questions.ServiceResults
{
	/// <summary>
	/// The result for a single student's assignment.
	/// </summary>
	public class SectionAssignmentResult
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
		/// The name of the assignment.
		/// </summary>
		public string AssignmentGroupName { get; }

		/// <summary>
		/// The student's score for the assignment.
		/// </summary>
		public double Score { get; }

		/// <summary>
		/// The results for each question.
		/// </summary>
		public IList<StudentQuestionResult> QuestionResults { get; }

		/// <summary>
		/// The status for the overall assignment.
		/// </summary>
		public SubmissionStatus Status { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public SectionAssignmentResult(
			string assignmentGroupName,
			string lastName, 
			string firstName, 
			double score, 
			IList<StudentQuestionResult> questionResults,
			SubmissionStatus status)
		{
			AssignmentGroupName = assignmentGroupName;
			LastName = lastName;
			FirstName = firstName;
			Score = score;
			QuestionResults = questionResults;
			Status = status;
		}
	}
}
