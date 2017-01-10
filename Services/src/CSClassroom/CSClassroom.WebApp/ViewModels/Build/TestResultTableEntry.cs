using System;
using CSC.CSClassroom.Model.Projects;
using CSC.CSClassroom.WebApp.Extensions;
using CSC.CSClassroom.WebApp.ViewHelpers.NestedTables;

namespace CSC.CSClassroom.WebApp.ViewModels.Build
{
	/// <summary>
	/// A table entry for a test result.
	/// </summary>
	public class TestResultTableEntry : TableEntry
	{
		/// <summary>
		/// The name of the test class.
		/// </summary>
		[TableColumn("Test name")]
		public string Name { get; }

		/// <summary>
		/// The test result.
		/// </summary>
		[TableColumn("Test result")]
		public string Result { get; }

		/// <summary>
		/// The test reuslt link.
		/// </summary>
		[TableColumn("Result link")]
		public string ResultLink { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public TestResultTableEntry(TestResult testResult, Func<TestResult, string> testUrlBuilder)
		{
			Name = testResult.TestName;
			Result = testResult.GetTestStatusHtml(alwaysBold: false);
			ResultLink = GetLink(testUrlBuilder(testResult), "Result", preventWrapping: true);
		}
	}
}
