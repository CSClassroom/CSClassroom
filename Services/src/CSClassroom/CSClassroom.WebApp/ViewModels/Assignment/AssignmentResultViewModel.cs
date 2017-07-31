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
	/// A report for a single student.
	/// </summary>
	public class AssignmentResultViewModel : TableEntry
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
		[SubTable(typeof(StudentQuestionResultViewModel), typeof(AssignmentSubmissionViewModel))]
		[JsonProperty(PropertyName = "childTableData")]
		public List<object> ChildTableData { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public AssignmentResultViewModel(
			AssignmentResult result,
			IAssignmentDisplayProvider displayProvider)
		{
			var assignmentUrl = displayProvider.GetAssignmentUrl();

			AssignmentName = assignmentUrl != null
				? GetLink(assignmentUrl, result.AssignmentName, preventWrapping: true)
				: GetColoredText("black", result.AssignmentName, bold: false, preventWrapping: true);

			DueDate = GetColoredText
			(
				"black",
				displayProvider.GetAssignmentDueDate(),
				bold: false,
				preventWrapping: true
			);

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

			ChildTableData = displayProvider.GetChildTableData();
		}
	}
}
