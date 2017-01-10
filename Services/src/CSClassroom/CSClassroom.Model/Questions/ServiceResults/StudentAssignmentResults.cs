using System.Collections.Generic;

namespace CSC.CSClassroom.Model.Questions.ServiceResults
{
	/// <summary>
	/// The result for a single student's assignment.
	/// </summary>
	public class StudentAssignmentResults
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
		/// The student's section name.
		/// </summary>
		public string SectionName { get; }

		/// <summary>
		/// The results for each question.
		/// </summary>
		public IList<StudentAssignmentResult> AssignmentResults { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public StudentAssignmentResults(
			string lastName, 
			string firstName, 
			string sectionName,
			IList<StudentAssignmentResult> assignmentResults)
		{
			LastName = lastName;
			FirstName = firstName;
			SectionName = sectionName;
			AssignmentResults = assignmentResults;
		}
	}
}
