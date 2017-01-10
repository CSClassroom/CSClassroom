using System.Collections.Generic;

namespace CSC.CSClassroom.Model.Questions.ServiceResults
{
	/// <summary>
	/// Contains the results for a given assignment in a given section.
	/// </summary>
	public class SectionAssignmentResults
	{
		/// <summary>
		/// The name of the homework assignment.
		/// </summary>
		public string AssignmentName { get; }

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
		public IList<SectionAssignmentResult> AssignmentResults { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public SectionAssignmentResults(
			string assignmentName, 
			string sectionName, 
			double points,
			IList<SectionAssignmentResult> assignmentResults)
		{
			AssignmentName = assignmentName;
			SectionName = sectionName;
			Points = points;
			AssignmentResults = assignmentResults;
		}
	}
}
