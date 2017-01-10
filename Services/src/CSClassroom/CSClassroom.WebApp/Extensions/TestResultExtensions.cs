using CSC.CSClassroom.Model.Projects;

namespace CSC.CSClassroom.WebApp.Extensions
{
	/// <summary>
	/// Extension methods for the test result class.
	/// </summary>
	public static class TestResultExtensions
	{
		/// <summary>
		/// Redirects the user to the sign in page, after which they will be redirected back.
		/// </summary>
		public static string GetTestStatusHtml(this TestResult testResult, bool alwaysBold)
		{
			bool changed = testResult.Succeeded ^ testResult.PreviouslySucceeded;
			string color = testResult.Succeeded ? "green" : "red";
			bool bold = changed || alwaysBold;
			string status = testResult.Succeeded
				? changed
					? "Fixed"
					: "Passed"
				: changed
					? "Regression"
					: "Failed";
			
			return $"<span style=\"{(bold ? "font-weight: bold; " : "")}color: {color}\">{status}</span>";
		}
	}
}
