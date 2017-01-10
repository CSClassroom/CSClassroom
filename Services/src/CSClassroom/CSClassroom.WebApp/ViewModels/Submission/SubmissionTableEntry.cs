using CSC.CSClassroom.WebApp.ViewHelpers.NestedTables;

namespace CSC.CSClassroom.WebApp.ViewModels.Submission
{
	public class SubmissionTableEntry : TableEntry
	{
		/// <summary>
		/// Returns the submission status HTML for a given submission.
		/// </summary>
		public string GetSubmissionStatus(int? daysLate)
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
				: "No submission";

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
