using System.Collections.Generic;
using System.Linq;
using CSC.CSClassroom.Model.Questions;
using CSC.CSClassroom.Model.Questions.ServiceResults;
using CSC.CSClassroom.WebApp.Providers;
using CSC.CSClassroom.WebApp.ViewHelpers.NestedTables;
using Newtonsoft.Json;

namespace CSC.CSClassroom.WebApp.ViewModels.Assignment
{
	/// <summary>
	/// A report including the updates for a single homework assignment.
	/// </summary>
	public class AssignmentUpdatesViewModel : TableEntry
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
		/// The number of students with updated assignment scores.
		/// </summary>
		[TableColumn("Students Updated")]
		public double NumStudentsUpdated { get; }

		/// <summary>
		/// The results for each student
		/// </summary>
		[SubTable(typeof(StudentAssignmentResultViewModel))]
		[JsonProperty(PropertyName = "childTableData")]
		public List<StudentAssignmentResultViewModel> StudentResults { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public AssignmentUpdatesViewModel(
			SectionAssignmentResults sectionAssignmentResults,
			IAssignmentDisplayProviderFactory displayProviderFactory)
		{
			AssignmentName = $"{sectionAssignmentResults.AssignmentGroupName} ({sectionAssignmentResults.Points} points)";
			SectionName = sectionAssignmentResults.SectionName;
			NumStudentsUpdated = sectionAssignmentResults.AssignmentGroupResults.Count;
			StudentResults = sectionAssignmentResults.AssignmentGroupResults.Select
			(
				result => new StudentAssignmentResultViewModel(result, displayProviderFactory)
			).ToList();
		}
	}
}
