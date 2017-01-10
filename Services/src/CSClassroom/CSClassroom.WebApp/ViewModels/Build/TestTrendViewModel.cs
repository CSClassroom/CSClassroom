using System;
using System.Collections.Generic;
using System.Linq;
using CSC.CSClassroom.Model.Projects;
using CSC.CSClassroom.Model.Projects.ServiceResults;
using Microsoft.AspNetCore.Mvc;

namespace CSC.CSClassroom.WebApp.ViewModels.Build
{
	/// <summary>
	/// The view model for the test trend chart.
	/// </summary>
	public class TestTrendViewModel
	{
		/// <summary>
		/// All passed/failed test counts.
		/// </summary>
		public IList<BuildTestCount> AllBuildTestCounts { get; }

		/// <summary>
		/// The current build index.
		/// </summary>
		public int? CurrentBuildIndex { get; }

		/// <summary>
		/// The name of the project.
		/// </summary>
		public string ProjectName { get; }

		/// <summary>
		/// Whether or not to show a thumbnail version of the chart.
		/// </summary>
		public bool Thumbnail { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public TestTrendViewModel(
			IList<BuildTestCount> allBuildTestCounts,
			string projectName,
			Model.Projects.Build currentBuild,
			bool thumbnail)
		{
			AllBuildTestCounts = GetTestCounts(allBuildTestCounts);

			ProjectName = projectName;

			CurrentBuildIndex = currentBuild != null
				? GetCurrentBuildIndex(AllBuildTestCounts, currentBuild)
				: (int?)null;

			Thumbnail = thumbnail;
		}

		/// <summary>
		/// Returns the index of the current build.
		/// </summary>
		private static int GetCurrentBuildIndex(
			IList<BuildTestCount> allBuildTestCounts, 
			Model.Projects.Build currentBuild)
		{
			var lastBuildTestCounts = allBuildTestCounts.Last
			(
				buildTestCount => buildTestCount.BuildId == currentBuild.Id
			);

			return allBuildTestCounts.IndexOf(lastBuildTestCounts);
		}

		/// <summary>
		/// Returns a serializable array for the given series.
		/// </summary>
		public object GetDataSeriesArray(IUrlHelper urlHelper, bool passed)
		{
			var dayBoundaries = GetDayBoundaries();
			var lastBuild = AllBuildTestCounts.Last();
			var totalTests = lastBuild.PassedCount + lastBuild.FailedCount;

			return AllBuildTestCounts.Select
			(
				(value, index) => new
				{
					Index = index,
					TestCount = passed
						? value.PassedCount
						: totalTests - value.PassedCount,
					ShortCommitDate = dayBoundaries[index]
						? value.PushDate.ToLocalTime().ToString("MM/dd")
						: "--",
					LongCommitDate = value.PushDate.ToLocalTime().ToString("MM/dd/yyyy h:mm tt"),
					BuildUrl = urlHelper.Action
					(
						"BuildResult",
						"Build",
						new { buildId = value.BuildId }
					)
				}
			);
		}

		/// <summary>
		/// Returns the test counts.
		/// </summary>
		private static IList<BuildTestCount> GetTestCounts(
			IList<BuildTestCount> allBuildTestCounts)
		{
			if (allBuildTestCounts.Count == 1)
			{
				// The charting library does not work well with only one point.
				// So we'll give it two identical points.

				allBuildTestCounts = allBuildTestCounts
					.Concat(allBuildTestCounts)
					.ToList();
			}

			return allBuildTestCounts;
		}

		/// <summary>
		/// Returns an array of booleans, indicating whether each index
		/// is the first index corresponding to the given value's day.
		/// </summary>
		private bool[] GetDayBoundaries()
		{
			bool[] days = new bool[AllBuildTestCounts.Count];

			DateTime prevValue = DateTime.MinValue;
			for (int index = 0; index < AllBuildTestCounts.Count; index++)
			{
				var value = AllBuildTestCounts[index].PushDate.ToLocalTime().Date;
				if (value != prevValue)
				{
					days[index] = true;
					prevValue = value;
				}
			}

			return days;
		}
	}
}
