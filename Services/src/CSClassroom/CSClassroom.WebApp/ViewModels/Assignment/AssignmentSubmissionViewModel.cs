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
	/// A report for a single student's submission, for an assignment that supports
	/// combined submissions.
	/// </summary>
	public class AssignmentSubmissionViewModel : TableEntry
	{
		/// <summary>
		/// The due date for the assignment.
		/// </summary>
		[TableColumn("Submission Date")]
		public string SubmissionDate { get; }

		/// <summary>
		/// The student's score for the assignment.
		/// </summary>
		[TableColumn("Score")]
		public string Score { get; }

		/// <summary>
		/// The assignment status.
		/// </summary>
		[TableColumn("Status")]
		public string Status { get; }

		/// <summary>
		/// The results for each question.
		/// </summary>
		[SubTable(typeof(StudentQuestionResultViewModel))]
		[JsonProperty(PropertyName = "childTableData")]
		public List<StudentQuestionResultViewModel> QuestionResults { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public AssignmentSubmissionViewModel(
			AssignmentSubmissionResult result,
			IAssignmentUrlProvider urlProvider,
			ITimeZoneProvider timeZoneProvider)
		{
			var submissionUrl = urlProvider.GetAssignmentSubmissionUrl(
				result.AssignmentId, 
				result.SubmissionDate,
				result.UserId);

			SubmissionDate = GetLink
			(
				submissionUrl, 
				result.SubmissionDate.FormatShortDateTime(timeZoneProvider), 
				preventWrapping: true
			);
			
			Score = GetColoredText
			(
				"black",
				$"{result.Score} / {result.AssignmentPoints}",
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
			
			QuestionResults = result.QuestionResults
				.Select
				(
					qr => new StudentQuestionResultViewModel
					(
						qr,
						urlProvider,
						timeZoneProvider
					)
				).ToList();
		}
	}
}
