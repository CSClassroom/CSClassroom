using System.Collections.Generic;

namespace CSC.CSClassroom.Model.Assignments.ServiceResults
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
		public IList<AssignmentGroupResult> AssignmentGroupResults { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public StudentAssignmentResults(
			string lastName, 
			string firstName, 
			string sectionName,
			IList<AssignmentGroupResult> assignmentGroupResultses)
		{
			LastName = lastName;
			FirstName = firstName;
			SectionName = sectionName;
			AssignmentGroupResults = assignmentGroupResultses;
		}
	}
}
