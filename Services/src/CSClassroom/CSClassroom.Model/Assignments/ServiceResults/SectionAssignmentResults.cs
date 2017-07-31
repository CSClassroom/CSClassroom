using System.Collections.Generic;

namespace CSC.CSClassroom.Model.Assignments.ServiceResults
{
	/// <summary>
	/// Contains the results for a given assignment in a given section.
	/// </summary>
	public class SectionAssignmentResults
	{
		/// <summary>
		/// The name of the homework assignment.
		/// </summary>
		public string AssignmentGroupName { get; }

		/// <summary>
		/// The section name.
		/// </summary>
		public string SectionName { get; }

		/// <summary>
		/// The total number of points the assignment is out of.
		/// </summary>
		public double Points { get; }

		/// <summary>
		/// The results for each student.
		/// </summary>
		public IList<AssignmentGroupResult> AssignmentGroupResults { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public SectionAssignmentResults(
			string assignmentGroupName, 
			string sectionName, 
			double points,
			IList<AssignmentGroupResult> assignmentGroupResults)
		{
			AssignmentGroupName = assignmentGroupName;
			SectionName = sectionName;
			Points = points;
			AssignmentGroupResults = assignmentGroupResults;
		}
	}
}
