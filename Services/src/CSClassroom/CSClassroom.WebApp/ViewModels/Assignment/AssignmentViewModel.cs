using System.Collections.Generic;
using System.Linq;
using CSC.CSClassroom.Model.Questions;
using CSC.CSClassroom.Model.Questions.ServiceResults;
using CSC.CSClassroom.WebApp.ViewHelpers.NestedTables;
using Newtonsoft.Json;

namespace CSC.CSClassroom.WebApp.ViewModels.Assignment
{
	/// <summary>
	/// A report for a single homework assignment.
	/// </summary>
	public class AssignmentViewModel : TableEntry
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
		[JsonProperty(PropertyName = "childTableData")]
		public List<StudentAssignmentResultViewModel> StudentResults { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public AssignmentViewModel(
			SectionAssignmentResults sectionAssignmentResults)
		{
			AssignmentName = sectionAssignmentResults.AssignmentName;
			SectionName = sectionAssignmentResults.SectionName;
			Points = sectionAssignmentResults.Points;
			StudentResults = sectionAssignmentResults.AssignmentResults.Select
			(
				result => new StudentAssignmentResultViewModel(result)
			).ToList();
		}
	}
}
