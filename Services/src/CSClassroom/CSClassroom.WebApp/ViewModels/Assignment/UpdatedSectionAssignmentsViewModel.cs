using System;
using System.Collections.Generic;
using System.Linq;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Model.Assignments.ServiceResults;
using CSC.CSClassroom.WebApp.Extensions;
using CSC.CSClassroom.WebApp.Providers;
using CSC.CSClassroom.WebApp.ViewHelpers.NestedTables;

namespace CSC.CSClassroom.WebApp.ViewModels.Assignment
{
	/// <summary>
	/// A report containing all recently updated assignments.
	/// </summary>
	public class UpdatedSectionAssignmentsViewModel : TableEntry
	{
		/// <summary>
		/// The section name.
		/// </summary>
		public string SectionName { get; }

		/// <summary>
		/// The gradebookName name.
		/// </summary>
		public string GradebookName { get; }

		/// <summary>
		/// The date assignments were last transferred to the gradebook.
		/// </summary>
		public string StartDate { get; set; }

		/// <summary>
		/// The date assignments on this report were retrieved.
		/// </summary>
		public string EndDate { get; set; }

		/// <summary>
		/// The results for updated each assignment
		/// </summary>
		public List<AssignmentUpdatesViewModel> AssignmentResults { get; }

		/// <summary>
		/// The form body that can be used to mark the assignment as graded.
		/// </summary>
		public SelectAssignmentReport MarkAssignmentsGraded { get; set; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public UpdatedSectionAssignmentsViewModel(
			UpdatedSectionAssignmentResults results,
			SelectAssignmentReport markAssignmentsGraded,
			ITimeZoneProvider timeZoneProvider,
			IAssignmentDisplayProviderFactory displayProviderFactory)
		{
			SectionName = results.SectionName;

			GradebookName = results.GradebookName;

			StartDate =
				results.AssignmentsLastGradedDate != DateTime.MinValue
					? results.AssignmentsLastGradedDate
						.FormatLongDateTime(timeZoneProvider)
					: "The beginning";
			
			EndDate = results.ResultsRetrievedDate.FormatLongDateTime(timeZoneProvider);

			AssignmentResults = results.AssignmentResults.Select
			(
				result => new AssignmentUpdatesViewModel
				(
					result,
					displayProviderFactory
				)
			).ToList();

			MarkAssignmentsGraded = markAssignmentsGraded;
		}
	}
}
