using System;
using System.Collections.Generic;
using System.Linq;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Model.Assignments.ServiceResults;
using CSC.CSClassroom.WebApp.Extensions;
using CSC.CSClassroom.WebApp.Providers;
using CSC.CSClassroom.WebApp.ViewHelpers.NestedTables;
using Newtonsoft.Json;

namespace CSC.CSClassroom.WebApp.ViewModels.Assignment
{
	/// <summary>
	/// A report for a single student's results for an assignment group.
	/// </summary>
	public class AssignmentGroupResultViewModel : TableEntry
	{
		/// <summary>
		/// The name of the homework assignment.
		/// </summary>
		[TableColumn("Assignment")]
		public string AssignmentName { get; }

		/// <summary>
		/// The student's score for the assignment.
		/// </summary>
		[TableColumn("Score")]
		public string Score { get; }

		/// <summary>
		/// The due date for the assignment.
		/// </summary>
		[TableColumn("Due Date")]
		public string DueDate { get; }

		/// <summary>
		/// The assignment status.
		/// </summary>
		[TableColumn("Status")]
		public string Status { get; }

		/// <summary>
		/// The results for each question.
		/// </summary>
		[SubTable(typeof(StudentQuestionResultViewModel), typeof(AssignmentSubmissionViewModel), typeof(AssignmentResultViewModel))]
		[JsonProperty(PropertyName = "childTableData")]
		public List<object> ChildTableData { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public AssignmentGroupResultViewModel(
			AssignmentGroupResult result,
			IAssignmentDisplayProviderFactory displayProviderFactory)
		{
			Score = GetColoredText
			(
				"black",
				$"{result.Score} / {result.TotalPoints}",
				bold: false,
				preventWrapping: true
			);

			Status = GetColoredText
			(
				result.Status.GetColor(),
				result.Status.GetText(),
				result.Status.GetBold(),
				preventWrapping: true
			);

			if (result.AssignmentResults.Count == 1)
			{
				var assignmentResult = result.AssignmentResults[0];
				var displayProvider = displayProviderFactory.CreateDisplayProvider
				(
					assignmentResult
				);

				var assignmentUrl = displayProvider.GetAssignmentUrl();

				AssignmentName = assignmentUrl != null
					? GetLink
						(
							assignmentUrl, 
							result.AssignmentGroupName, 
							preventWrapping: true
						)
					: GetColoredText
						(
							"black", 
							result.AssignmentGroupName, 
							bold: false, 
							preventWrapping: true
						);

				DueDate = GetColoredText
				(
					"black",
					displayProvider.GetAssignmentDueDate(),
					bold: false,
					preventWrapping: true
				);

				ChildTableData = displayProvider.GetChildTableData();
			}
			else
			{
				AssignmentName = GetColoredText
				(
					"black",
					result.AssignmentGroupName,
					bold: false,
					preventWrapping: true
				);

				DueDate = result.AssignmentResults.Any(a => a.AssignmentDueDate.HasValue)
					? "Multiple"
					: "Unassigned";

				ChildTableData = result.AssignmentResults
					.Select
					(
						assignmentResult => new AssignmentResultViewModel
						(
							assignmentResult,
							displayProviderFactory.CreateDisplayProvider(assignmentResult)
						)
					).Cast<object>().ToList();
			}
		}
	}
}
