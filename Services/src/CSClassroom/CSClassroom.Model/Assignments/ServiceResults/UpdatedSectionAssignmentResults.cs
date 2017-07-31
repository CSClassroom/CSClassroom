using System;
using System.Collections.Generic;

namespace CSC.CSClassroom.Model.Questions.ServiceResults
{
	/// <summary>
	/// Contains all results updated since the last time assignments
	/// were marked as graded, for a given section.
	/// </summary>
	public class UpdatedSectionAssignmentResults
	{
		/// <summary>
		/// The section name.
		/// </summary>
		public string SectionName { get; }

		/// <summary>
		/// The gradebook name.
		/// </summary>
		public string GradebookName { get; }

		/// <summary>
		/// The date assignments were last transferred to the gradebook.
		/// </summary>
		public DateTime AssignmentsLastGradedDate { get; }

		/// <summary>
		/// The date results were retrieved.
		/// </summary>
		public DateTime ResultsRetrievedDate { get; }

		/// <summary>
		/// The results for each student.
		/// </summary>
		public IList<SectionAssignmentResults> AssignmentResults { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public UpdatedSectionAssignmentResults(
			string sectionName, 
			string gradebookName,
			DateTime assignmentsLastGradedDate,
			DateTime resultsRetrievedDate,
			IList<SectionAssignmentResults> assignmentResults)
		{
			SectionName = sectionName;
			GradebookName = gradebookName;
			AssignmentsLastGradedDate = assignmentsLastGradedDate;
			ResultsRetrievedDate = resultsRetrievedDate;
			AssignmentResults = assignmentResults;
		}
	}
}
