using System.Linq;
using CSC.CSClassroom.Model.Projects;
using CSC.CSClassroom.WebApp.Extensions;

namespace CSC.CSClassroom.WebApp.ViewModels.Build
{
	/// <summary>
	/// The view model for a test result.
	/// </summary>
	public class TestResultViewModel
	{
		/// <summary>
		/// The class name.
		/// </summary>
		public string ClassName { get; }

		/// <summary>
		/// The name of the test method.
		/// </summary>
		public string TestName { get; }

		/// <summary>
		/// Whether or not the test succeeed.
		/// </summary>
		public bool Succeeded { get; }

		/// <summary>
		/// The test status string.
		/// </summary>
		public string TestStatusHtml { get; }

		/// <summary>
		/// The failure message (if any).
		/// </summary>
		public string FailureMessage { get; }

		/// <summary>
		/// The stack trace of the failure (if any).
		/// </summary>
		public string FailureTrace { get; }

		/// <summary>
		/// The contents of stdout/stderr when the failure occured (if any).
		/// </summary>
		public string FailureOutput { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public TestResultViewModel(TestResult testResult)
		{
			ClassName = testResult.Build
				.Commit
				.Project
				.TestClasses
				.SingleOrDefault(tc => tc.ClassName == testResult.ClassName)
				?.DisplayName ?? testResult.ClassName;

			TestName = testResult.TestName;
			Succeeded = testResult.Succeeded;
			TestStatusHtml = testResult.GetTestStatusHtml(alwaysBold: true);
			FailureMessage = testResult.FailureMessage;
			FailureTrace = testResult.FailureTrace;
			FailureOutput = testResult.FailureOutput;
		}
	}
}
