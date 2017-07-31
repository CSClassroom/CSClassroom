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
	/// A report for a single student's submission, for a single non-interactive question.
	/// </summary>
	public class QuestionSubmissionViewModel : TableEntry
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
		/// Constructor.
		/// </summary>
		public QuestionSubmissionViewModel(
			QuestionSubmissionResult result,
			IAssignmentUrlProvider urlProvider,
			ITimeZoneProvider timeZoneProvider)
		{
			var submissionUrl = urlProvider.GetQuestionSubmissionUrl(
				result.AssignmentId, 
				result.QuestionId,
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
				$"{result.Score} / {result.QuestionPoints}",
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
		}
	}
}
