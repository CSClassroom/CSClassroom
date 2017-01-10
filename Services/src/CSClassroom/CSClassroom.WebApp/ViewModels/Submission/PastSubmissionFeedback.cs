using CSC.CSClassroom.WebApp.ViewHelpers.NestedTables;

namespace CSC.CSClassroom.WebApp.ViewModels.Submission
{
	/// <summary>
	/// Feedback for a past submission.
	/// </summary>
	public class PastSubmissionFeedback : TableEntry
	{
		/// <summary>
		/// The feedback for the past submission.
		/// </summary>
		[TableColumn("Feedback")]
		public string Feedback { get; set; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public PastSubmissionFeedback(string feedback)
		{
			Feedback = !string.IsNullOrWhiteSpace(feedback)
				? feedback.Replace("\n", "<br>")
				: "<em>No feedback entered.</em>";
		}
	}
}
