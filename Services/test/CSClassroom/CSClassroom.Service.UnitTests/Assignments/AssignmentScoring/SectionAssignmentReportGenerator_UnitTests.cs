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
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Assignments.AssignmentScoring
{
	/// <summary>
	/// Unit tests for the SectionAssignmentReportGenerator class.
	/// </summary>
	public class SectionAssignmentReportGenerator_UnitTests : AssignmentReportGeneratorUnitTestBase
	{
		/// <summary>
		/// Ensures that GetAssignmentGroupResult returns a correct result.
		/// </summary>
		[Fact]
		void GetSectionAssignmentGroupResults_ReturnsCorrectResult()
		{
			var section = new Section() { DisplayName = "Section Name" };

			var assignmentGroupName = "Group1";

			var users = Collections.CreateList
			(
				new User() { Id = 1, LastName = "Last2", FirstName = "First2"},
				new User() { Id = 2, LastName = "Last1", FirstName = "First1" }
			);

			var assignments = Collections.CreateList
			(
				new Assignment() { Id = 10, GroupName = assignmentGroupName },
				new Assignment() { Id = 20, GroupName = assignmentGroupName },
				new Assignment() { Id = 30, GroupName = "OtherGroupName" }
			);

			var filteredAssignments = assignments.SkipLast(1).ToList();

			var submissions = Collections.CreateList
			(
				CreateSubmission(id: 100, user: users[0]),
				CreateSubmission(id: 200, user: users[0]),
				CreateSubmission(id: 300, user: users[1]),
				CreateSubmission(id: 400, user: users[1])
			);

			var filteredSubmissions = submissions.SkipLast(1).ToList();

			var getAssignmentGroupResultCalls = filteredAssignments
				.GroupBy(a => a.GroupName)
				.SelectMany
				(
					assignmentGroup => users, 
					(assignmentGroup, user) => new GetAssignmentGroupResultCall
					(
						assignmentGroup,
						user, 
						CreateAssignmentGroupResult(assignmentGroup, user)
					)
				).ToList();

			var assignmentReportGenerator = CreateSectionAssignmentReportGenerator
			(
				section,
				new FilterAssignmentsCall(assignments, filteredAssignments, assignmentGroupName), 
				new FilterSubmissionsCall(submissions, filteredSubmissions),
				getAssignmentGroupResultCalls,
				totalPoints: 10.0
			);

			var result = assignmentReportGenerator.GetSectionAssignmentGroupResults
			(
				assignmentGroupName,
				assignments,
				section,
				users,
				submissions
			);

			var expectedAssignmentGroupResults = getAssignmentGroupResultCalls
				.OrderBy(call => call.User.LastName)
				.ThenBy(call => call.User.FirstName)
				.Select(call => call.Result)
				.ToList();

			Assert.Equal(assignmentGroupName, result.AssignmentGroupName);
			Assert.Equal(section.DisplayName, result.SectionName);
			Assert.Equal(10.0, result.Points);
			Assert.True(expectedAssignmentGroupResults.SequenceEqual(result.AssignmentGroupResults));
		}

		/// <summary>
		/// Creates a mock assignment group score calculator.
		/// </summary>
		protected IAssignmentGroupScoreCalculator CreateMockAssignmentGroupScoreCalculator(
			IList<Assignment> assignments,
			double totalPoints)
		{
			var assignmentGroupScoreCalculator = new Mock<IAssignmentGroupScoreCalculator>();

			assignmentGroupScoreCalculator
				.Setup
				(
					m => m.GetAssignmentGroupTotalPoints
					(
						It.Is<IList<Assignment>>
						(
							seq => seq.SequenceEqual(assignments)
						),
						1 /*roundDigits*/
					)
				).Returns(totalPoints);

			return assignmentGroupScoreCalculator.Object;
		}

		/// <summary>
		/// Creates a new assignment report generator.
		/// </summary>
		private SectionAssignmentReportGenerator CreateSectionAssignmentReportGenerator(
			Section section,
			FilterAssignmentsCall filterAssignmentsCall,
			FilterSubmissionsCall filterSubmissionsCall,
			IList<GetAssignmentGroupResultCall> getAssignmentGroupResultCalls,
			double totalPoints)
		{
			var assignmentFilter = CreateMockAssignmentFilter
			(
				section,
				filterAssignmentsCall,
				filterSubmissionsCall,
				getAssignmentGroupResultCalls.Select(t => t.AssignmentGroup).ToList()
			);

			var assignmentGroupResultGenerator = CreateMockAssignmentGroupResultGenerator
			(
				section,
				getAssignmentGroupResultCalls,
				filterSubmissionsCall.FilteredSubmissions,
				admin: true
			);

			var assignmentGroupScoreCalculator = CreateMockAssignmentGroupScoreCalculator
			(
				filterAssignmentsCall.FilteredAssignments,
				totalPoints
			);

			return new SectionAssignmentReportGenerator
			(
				assignmentGroupResultGenerator,
				assignmentGroupScoreCalculator,
				assignmentFilter
			);
		}
	}
}
