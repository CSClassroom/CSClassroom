using System;
using System.Collections.Generic;
using System.Linq;
using CSC.CSClassroom.Model.Questions;
using CSC.CSClassroom.Model.Questions.ServiceResults;
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
		[TableColumn("Assignment Name")]
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
		[JsonProperty(PropertyName = "childTableData")]
		public List<StudentQuestionResultViewModel> QuestionResults { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public AssignmentResultViewModel(
			StudentAssignmentResult result,
			Func<int, string> getQuestionUrl,
			ITimeZoneProvider timeZoneProvider)
		{
			AssignmentName = result.AssignmentName;
			DueDate = result.AssignmentDueDate.FormatLongDateTime(timeZoneProvider);
			Score = $"{result.Score} / {result.QuestionResults.Sum(qr => qr.QuestionPoints)}";
			Status = GetColoredText
			(
				result.Status.GetColor(), 
				result.Status.GetText(),
				result.Status.GetBold(),
				preventWrapping: true
			);

			QuestionResults = result.QuestionResults
				.Select
				(
					qr => new StudentQuestionResultViewModel
					(
						qr,
						getQuestionUrl
					)
				).ToList();
		}
	}
}
