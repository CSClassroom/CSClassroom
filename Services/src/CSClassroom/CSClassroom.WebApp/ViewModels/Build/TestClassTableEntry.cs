using System;
using System.Collections.Generic;
using System.Linq;
using CSC.CSClassroom.Model.Projects;
using CSC.CSClassroom.WebApp.ViewHelpers.NestedTables;
using Newtonsoft.Json;

namespace CSC.CSClassroom.WebApp.ViewModels.Build
{
	/// <summary>
	/// A test class table entry.
	/// </summary>
	public class TestClassTableEntry : TableEntry
	{
		/// <summary>
		/// The name of the test class.
		/// </summary>
		[TableColumn("Category")]
		public string Name { get; set; }

		/// <summary>
		/// The number of tests that passed.
		/// </summary>
		[TableColumn("Pass count")]
		public int PassCount { get; set; }

		/// <summary>
		/// The number of tests that failed.
		/// </summary>
		[TableColumn("Fail count")]
		public int FailCount { get; set; }

		/// <summary>
		/// The total number of tests.
		/// </summary>
		[TableColumn("Total count")]
		public int TotalCount { get; set; }

		/// <summary>
		/// The test results.
		/// </summary>
		[TableOptions(ShowHeader = true)]
		[SubTable(typeof(TestResultTableEntry))]
		[JsonProperty(PropertyName = "childTableData")]
		public List<TestResultTableEntry> TestResults { get; set; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public TestClassTableEntry(
			TestClass testClass,
			bool emphasized,
			IList<TestResult> testResults, 
			Func<TestResult, string> testUrlBuilder)
		{
			Name = GetColoredText("black", testClass.DisplayName, emphasized, preventWrapping: true);
			PassCount = testResults.Count(result => result.Succeeded);
			FailCount = testResults.Count(result => !result.Succeeded);
			TotalCount = testResults.Count;
			TestResults = testResults.Select
			(
				result => new TestResultTableEntry(result, testUrlBuilder)
			).ToList();
		}

		/// <summary>
		/// Returns table entries for each test class.
		/// </summary>
		public static IList<TestClassTableEntry> GetTestClassResults(
			Checkpoint checkpoint,
			Model.Projects.Build build,
			Func<TestResult, string> testUrlBuilder)
		{
			var testClasses = build.Commit.Project.TestClasses;

			return build.TestResults.GroupBy(result => result.ClassName)
				.OrderBy
				(
					result => testClasses.FirstOrDefault
					(
						testClass => testClass.ClassName == result.Key
					)?.Order ?? 0
				)
				.Where
				(
					group => testClasses.Any
					(
						testClass => testClass.ClassName == group.Key
					)

					&& 
					
					(
						checkpoint == null ||
						(
							checkpoint.TestClasses?.Any
							(
								testClass => testClass.TestClass.ClassName == group.Key	
							) ?? false
						)
					)
				)
				.Select
				(
					group => new TestClassTableEntry
					(
						testClasses.Single(testClass => testClass.ClassName == group.Key),
						checkpoint?.TestClasses
							?.FirstOrDefault(tc => tc.TestClass.ClassName == group.Key)
							?.Required ?? false,
						group.OrderBy(t => t.TestName).ToList(),
						testUrlBuilder
					)
				).ToList();
		}
	}
}
