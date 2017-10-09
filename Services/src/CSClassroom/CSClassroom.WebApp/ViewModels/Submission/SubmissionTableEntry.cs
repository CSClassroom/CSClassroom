using System;
using CSC.CSClassroom.WebApp.Extensions;
using CSC.CSClassroom.WebApp.Providers;
using CSC.CSClassroom.WebApp.ViewHelpers.NestedTables;

namespace CSC.CSClassroom.WebApp.ViewModels.Submission
{
	public class SubmissionTableEntry : TableEntry
	{
		/// <summary>
		/// Returns the submission status HTML for a given submission.
		/// </summary>
		public string GetStatus(
			DateTime? date, 
			int? daysLate, 
			string buildUrl, 
			ITimeZoneProvider timeZoneProvider)
		{
			if (date == null || daysLate == null)
			{
				return "No Submission";
			}

			var dateStr = GetDate(date, buildUrl, timeZoneProvider);
			var lateStr = GetLate(daysLate);
			return $"<div style=\"white-space: nowrap\">{dateStr}: {lateStr}</div>";
		}

		/// <summary>
		/// Returns a string with the given date. If a build URL is included,
		/// the string will be a link to the build result.
		/// </summary>
		private string GetDate(
			DateTime? date,
			string buildUrl,
			ITimeZoneProvider timeZoneProvider)
		{
			return buildUrl != null
				? GetLink
					(
						buildUrl,
						date.Value.FormatShortDateTime(timeZoneProvider),
						preventWrapping: true
					)
				: GetColoredText
					(
						"black",
						date.Value.FormatShortDateTime(timeZoneProvider),
						bold: false,
						preventWrapping: true
					);
		}

		/// <summary>
		/// Returns a string with the lateness of the submission.
		/// </summary>
		private string GetLate(int? daysLate)
		{
			bool submissionExists = (daysLate != null);
			bool late = submissionExists && daysLate.Value > 0;

			string fontColor = submissionExists && !late
				? "green"
				: "red";

			string text = submissionExists
				? late
					? $"{daysLate.Value} {(daysLate.Value > 1 ? "days" : "day")} late"
					: "On time"
				: string.Empty;

			return GetColoredText
			(
				fontColor, 
				text, 
				bold: !submissionExists || late, 
				preventWrapping: true
			);
		}
	}
}
