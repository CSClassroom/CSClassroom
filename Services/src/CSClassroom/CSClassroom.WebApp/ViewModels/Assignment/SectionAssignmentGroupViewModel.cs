using System.Collections.Generic;
using System.Linq;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Model.Assignments.ServiceResults;
using CSC.CSClassroom.WebApp.Providers;
using CSC.CSClassroom.WebApp.ViewHelpers.NestedTables;
using Newtonsoft.Json;

namespace CSC.CSClassroom.WebApp.ViewModels.Assignment
{
	/// <summary>
	/// A report for a single homework assignment group in one section.
	/// </summary>
	public class SectionAssignmentGroupViewModel : TableEntry
	{
		/// <summary>
		/// The section name.
		/// </summary>
		public string SectionName { get; }

		/// <summary>
		/// The name of the homework assignment.
		/// </summary>
		[TableColumn("Assignment Name")]
		public string AssignmentName { get; }

		/// <summary>
		/// The total number of points the assignment is out of.
		/// </summary>
		[TableColumn("Total Points")]
		public double Points { get; }

		/// <summary>
		/// The results for each student
		/// </summary>
		[SubTable(typeof(StudentAssignmentResultViewModel))]
		[JsonProperty(PropertyName = "childTableData")]
		public List<StudentAssignmentResultViewModel> StudentResults { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public SectionAssignmentGroupViewModel(
			SectionAssignmentResults sectionAssignmentResults,
			IAssignmentDisplayProviderFactory displayProviderFactory)
		{
			AssignmentName = sectionAssignmentResults.AssignmentGroupName;
			SectionName = sectionAssignmentResults.SectionName;
			Points = sectionAssignmentResults.Points;
			StudentResults = sectionAssignmentResults.AssignmentGroupResults.Select
			(
				result => new StudentAssignmentResultViewModel
				(
					result,
					displayProviderFactory
				)
			).ToList();
		}
	}
}
