using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Model.Assignments.ServiceResults;
using CSC.CSClassroom.Model.Users;
using CSC.CSClassroom.Service.Assignments.AssignmentScoring;
using CSC.CSClassroom.Service.UnitTests.Utilities;
using Moq;
using MoreLinq;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Assignments.AssignmentScoring
{
	/// <summary>
	/// Unit tests for the SnapshotAssignmentReportGenerator class.
	/// </summary>
	public class SnapshotAssignmentReportGenerator_UnitTests : AssignmentReportGeneratorUnitTestBase
	{
		/// <summary>
		/// Ensures that GetUpdatedAssignmentGroupResults returns the correct result.
		/// </summary>
		[Fact]
		public void GetUpdatedAssignmentGroupResults_ReturnsCorrectResult()
		{
			var section = new Section() { DisplayName = "Section Name" };

			var users = Collections.CreateList
			(
				new User() { Id = 1, LastName = "Last1", FirstName = "First1" },
				new User() { Id = 2, LastName = "Last2", FirstName = "First2" }
			);

			var snapshotDate = DateTime.MaxValue;

			var assignments = Collections.CreateList
			(
				new Assignment() { Id = 10, GroupName = "Group1" },
				new Assignment() { Id = 20, GroupName = "Group2" },
				new Assignment() { Id = 30, GroupName = "Group3" }
			);

			var filteredAssignments = assignments.SkipLast(1).ToList();

			var submissions = users
				.SelectMany
				(
					user => assignments,
					(user, assignment) => new {user, assignment}
				)
				.Select
				(
					(pair, index) => CreateSubmission
					(
						(index + 1) * 100,
						pair.user,
						pair.assignment
					)
				).ToList();

			var filteredSubmissions = submissions.SkipLast(1).ToList();

			var getSectionAssignmentGroupResultsCalls = filteredAssignments
				.GroupBy(a => a.GroupName)
				.Select
				(
					assignmentGroup => new GetSectionAssignmentGroupResultsCall
					(
						assignmentGroup,
						CreateSectionAssignmentResults(assignmentGroup)
					)
				).ToList();

			var snapshotAssignmentReportGenerator = CreateSnapshotAssignmentReportGenerator
			(
				section,
				users,
				new FilterAssignmentsCall(assignments, filteredAssignments),
				new FilterSubmissionsCall
				(
					submissions,
					filteredSubmissions,
					null /*user*/,
					snapshotDate
				),
				getSectionAssignmentGroupResultsCalls
			);

			var results = snapshotAssignmentReportGenerator.GetAssignmentGroupResultsSnapshot
			(
				assignments,
				users,
				section,
				submissions,
				snapshotDate
			);

			Assert.True
			(
				results.SequenceEqual
				(
					getSectionAssignmentGroupResultsCalls.Select
					(
						call => call.Results
					)
				)
			);
		}

		/// <summary>
		/// Creates a mock section assignment report generator.
		/// </summary>
		protected ISectionAssignmentReportGenerator CreateMockSectionAssignmentReportGenerator(
			Section section,
			IList<User> users,
			IList<UserQuestionSubmission> submissions,
			IList<GetSectionAssignmentGroupResultsCall> getSectionAssignmentGroupResultsCalls)
		{
			var sectionAssignmentReportGenerator = new Mock<ISectionAssignmentReportGenerator>();

			foreach (var call in getSectionAssignmentGroupResultsCalls)
			{
				sectionAssignmentReportGenerator
					.Setup
					(
						m => m.GetSectionAssignmentGroupResults
						(
							call.AssignmentGroup.Key,
							It.Is<IList<Assignment>>
							(
								seq => seq.SequenceEqual(call.AssignmentGroup)
							),
							section,
							users,
							It.Is<IList<UserQuestionSubmission>>
							(
								seq => seq.SequenceEqual
								(
									submissions.Where
									(
										s => s.UserQuestionData
												 .AssignmentQuestion
												 .Assignment
												 .GroupName == call.AssignmentGroup.Key
									)
								)
							)
						)
					).Returns(call.Results);
			}

			return sectionAssignmentReportGenerator.Object;
		}

		/// <summary>
		/// Creates an updated assignment report generator.
		/// </summary>
		private SnapshotAssignmentReportGenerator CreateSnapshotAssignmentReportGenerator(
			Section section,
			IList<User> users,
			FilterAssignmentsCall filterAssignmentsCall,
			FilterSubmissionsCall filterSubmissionsCall,
			IList<GetSectionAssignmentGroupResultsCall> getSectionAssignmentGroupResultsCalls)
		{
			var assignmentFilter = CreateMockAssignmentFilter
			(
				section,
				filterAssignmentsCall,
				filterSubmissionsCall,
				getSectionAssignmentGroupResultsCalls.Select(c => c.AssignmentGroup).ToList()
			);

			var sectionAssignmentReportGenerator = CreateMockSectionAssignmentReportGenerator
			(
				section,
				users,
				filterSubmissionsCall.FilteredSubmissions,
				getSectionAssignmentGroupResultsCalls
			);

			return new SnapshotAssignmentReportGenerator
			(
				assignmentFilter, 
				sectionAssignmentReportGenerator
			);
		}
	}
}
