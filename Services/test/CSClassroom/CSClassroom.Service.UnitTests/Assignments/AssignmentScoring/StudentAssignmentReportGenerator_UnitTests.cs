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
	/// Unit tests for the StudentAssignmentReportGenerator class.
	/// </summary>
	public class StudentAssignmentReportGenerator_UnitTests : AssignmentReportGeneratorUnitTestBase
	{
		/// <summary>
		/// Ensures that GetStudentAssignmentGroupResults returns the correct results.
		/// </summary>
		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		void GetStudentAssignmentGroupResults_ReturnsCorrectResults(bool admin)
		{
			var section = new Section() { DisplayName = "Section Name" };

			var users = Collections.CreateList
			(
				new User() { Id = 1, LastName = "Last2", FirstName = "First2" },
				new User() { Id = 2, LastName = "Last1", FirstName = "First1" }
			);

			var assignments = Collections.CreateList
			(
				new Assignment() { Id = 10, GroupName = "Group1" },
				new Assignment() { Id = 20, GroupName = "Group1" },
				new Assignment() { Id = 30, GroupName = "Group2" },
				new Assignment() { Id = 30, GroupName = "Group2" }
			);

			var filteredAssignments = assignments.SkipLast(1).ToList();

			var submissions = Collections.CreateList
			(
				CreateSubmission(id: 100, user: users[0]),
				CreateSubmission(id: 200, user: users[0]),
				CreateSubmission(id: 300, user: users[1]),
				CreateSubmission(id: 400, user: users[1])
			);

			var filteredSubmissions = submissions.SkipLast(2).ToList();

			var getAssignmentGroupResultCalls = filteredAssignments
				.GroupBy(a => a.GroupName)
				.Select
				(
					assignmentGroup => new GetAssignmentGroupResultCall
					(
						assignmentGroup,
						users[0],
						CreateAssignmentGroupResult(assignmentGroup, users[0])
					)
				).ToList();

			var assignmentReportGenerator = CreateStudentAssignmentReportGenerator
			(
				section,
				new FilterAssignmentsCall(assignments, filteredAssignments), 
				new FilterSubmissionsCall(submissions, filteredSubmissions, users[0]),
				getAssignmentGroupResultCalls,
				admin
			);

			var result = assignmentReportGenerator.GetStudentAssignmentGroupResults
			(
				users[0],
				section,
				assignments,
				submissions,
				admin
			);

			var expectedAssignmentGroupResults = getAssignmentGroupResultCalls
				.Select(call => call.Result)
				.ToList();

			Assert.Equal(users[0].LastName, result.LastName);
			Assert.Equal(users[0].FirstName, result.FirstName);
			Assert.Equal(section.DisplayName, result.SectionName);
			Assert.True(expectedAssignmentGroupResults.SequenceEqual(result.AssignmentGroupResults));
		}

		/// <summary>
		/// Creates a new assignment report generator.
		/// </summary>
		private StudentAssignmentReportGenerator CreateStudentAssignmentReportGenerator(
			Section section,
			FilterAssignmentsCall filterAssignmentsCall,
			FilterSubmissionsCall filterSubmissionsCall,
			IList<GetAssignmentGroupResultCall> getAssignmentGroupResultCalls,
			bool admin)
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
				admin
			);

			return new StudentAssignmentReportGenerator
			(
				assignmentGroupResultGenerator,
				assignmentFilter
			);
		}
	}
}
