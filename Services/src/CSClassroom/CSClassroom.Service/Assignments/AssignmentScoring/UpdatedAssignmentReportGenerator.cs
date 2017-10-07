using System;
using System.Collections.Generic;
using System.Linq;
using CSC.Common.Infrastructure.Extensions;
using MoreLinq;
using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Model.Assignments.ServiceResults;
using CSC.CSClassroom.Model.Users;
using CSC.Common.Infrastructure.System;

namespace CSC.CSClassroom.Service.Assignments.AssignmentScoring
{
	/// <summary>
	/// Generates an assignment report with updated assignment results.
	/// </summary>
	public class UpdatedAssignmentReportGenerator : IUpdatedAssignmentReportGenerator
	{
		/// <summary>
		/// Generates assignment snapshots.
		/// </summary>
		private readonly ISnapshotAssignmentReportGenerator _snapshotAssignmentReportGenerator;

		/// <summary>
		/// The time provider.
		/// </summary>
		private readonly ITimeProvider _timeProvider;

		/// <summary>
		/// The tolerance for comparing two scores.
		/// </summary>
		private const double c_tolerance = 0.0001;

		/// <summary>
		/// Constructor.
		/// </summary>
		public UpdatedAssignmentReportGenerator(
			ISnapshotAssignmentReportGenerator snapshotAssignmentReportGenerator,
			ITimeProvider timeProvider)
		{
			_snapshotAssignmentReportGenerator = snapshotAssignmentReportGenerator;
			_timeProvider = timeProvider;
		}

		/// <summary>
		/// Calculates the scores for all assignment groups updated since the last time 
		/// assignments were marked as graded, for a given section.
		/// </summary>
		public UpdatedSectionAssignmentResults GetUpdatedAssignmentGroupResults(
			IList<Assignment> assignments,
			IList<User> users,
			Section section,
			string gradebookName,
			DateTime lastTransferDate,
			IList<UserQuestionSubmission> userQuestionSubmissions)
		{
			var dateRetrieved = GetDateRetrieved();

			var oldAssignmentResults = _snapshotAssignmentReportGenerator
				.GetAssignmentGroupResultsSnapshot
				(
					assignments,
					users,
					section,
					userQuestionSubmissions,
					lastTransferDate
				);

			var newAssignmentResults = _snapshotAssignmentReportGenerator
				.GetAssignmentGroupResultsSnapshot
				(
					assignments,
					users,
					section,
					userQuestionSubmissions,
					dateRetrieved
				);

			return new UpdatedSectionAssignmentResults
			(
				section.DisplayName,
				gradebookName,
				lastTransferDate,
				dateRetrieved,
				GetChangedAssignmentResults
				(
					oldAssignmentResults,
					newAssignmentResults
				)
			);
		}

		/// <summary>
		/// Returns the current date, which will be used to limit the
		/// returned results.
		/// </summary>
		private DateTime GetDateRetrieved()
		{
			var dateRetrieved = _timeProvider.UtcNow;
			dateRetrieved = dateRetrieved.AddSeconds(-dateRetrieved.Second);

			return dateRetrieved;
		}

		/// <summary>
		/// Returns the same function passed in. This allows us to use 
		/// lambdas with anonymous types.
		/// </summary>
		private Func<TValue, TKey> GetFunc<TValue, TKey>(Func<TValue, TKey> getKey)
		{
			return getKey;
		}

		/// <summary>
		/// Removes unchanged assignment results.
		/// </summary>
		private IList<SectionAssignmentResults> GetChangedAssignmentResults(
			IList<SectionAssignmentResults> oldAssignmentResults,
			IList<SectionAssignmentResults> newAssignmentResults)
		{
			var getKey = GetFunc
			(
				(AssignmentGroupResult agr) => new
				{
					agr.LastName,
					agr.FirstName,
					agr.AssignmentGroupName
				}
			);

			var oldScores = oldAssignmentResults
				.SelectMany(sar => sar.AssignmentGroupResults)
				.ToDictionary(getKey, agr => agr.Score);

			var includeResult = GetFunc
			(
				(AssignmentGroupResult agr) =>
					!oldScores.ContainsKey(getKey(agr))
					|| Math.Abs(agr.Score - oldScores[getKey(agr)]) > c_tolerance
			);

			return newAssignmentResults
				.Where
				(
					assignmentGroupResults => assignmentGroupResults
						.AssignmentGroupResults
						.Any(includeResult)
				)
				.Select
				(
					assignmentResults => new SectionAssignmentResults
					(
						assignmentResults.AssignmentGroupName,
						assignmentResults.SectionName,
						assignmentResults.Points,
						assignmentResults.AssignmentGroupResults
							.Where(includeResult)
							.ToList()
					)
				).ToList();
		}
	}
}
