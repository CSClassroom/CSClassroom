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
	/// Unit tests for the AssignmentGroupResultGenerator class.
	/// </summary>
	public class AssignmentGroupResultGenerator_UnitTests
	{
		/// <summary>
		/// Ensures that GetAssignmentGroupResult returns a correct result.
		/// </summary>
		[Fact]
		void GetAssignmentGroupResult_ReturnsCorrectResult()
		{
			var user = new User() { Id = 1, LastName = "Last", FirstName = "First" };
			var section = new Section();
			var assignmentGroupName = "GroupName";
			var assignments = Collections.CreateList
			(
				new Assignment() { Id = 10 },
				new Assignment() { Id = 20 },
				new Assignment() { Id = 30 }
			);
			var filteredAssignments = assignments.SkipLast(1).ToList();
			var submissions = Collections.CreateList
			(
				new UserQuestionSubmission() { Id = 100 },
				new UserQuestionSubmission() { Id = 200 },
				new UserQuestionSubmission() { Id = 300 }
			);
			var filteredSubmissions = submissions.SkipLast(1).ToList();
			var assignmentResults = Collections.CreateList
			(
				CreateAssignmentResult(assignments[0]),
				CreateAssignmentResult(assignments[1])
			);
			
			var assignmentFilter = CreateMockAssignmentFilter
			(
				user, 
				section, 
				assignmentGroupName,
				assignments, 
				filteredAssignments,
				submissions, 
				filteredSubmissions
			);

			var assignmentResultGenerator = CreateMockAssignmentResultGenerator
			(
				user, 
				section, 
				filteredAssignments, 
				filteredSubmissions,
				assignmentResults
			);

			var assignmentGroupScoreCalculator = CreateMockAssignmentGroupScoreCalculator
			(
				filteredAssignments,
				assignmentResults
			);

			var assignmentGroupResultGenerator = new AssignmentGroupResultGenerator
			(
				assignmentResultGenerator,
				assignmentFilter,
				assignmentGroupScoreCalculator
			);

			var result = assignmentGroupResultGenerator.GetAssignmentGroupResult
			(
				assignmentGroupName,
				assignments,
				section,
				user,
				submissions,
				admin: false
			);

			Assert.Equal(user.FirstName, result.FirstName);
			Assert.Equal(user.LastName, result.LastName);
			Assert.Equal(assignmentGroupName, result.AssignmentGroupName);
			Assert.Equal(5.0, result.Score);
			Assert.Equal(6.0, result.TotalPoints);
			Assert.Equal(Completion.Completed, result.Status.Completion);
			Assert.False(result.Status.Late);
			Assert.True(result.AssignmentResults.SequenceEqual(assignmentResults));
		}

		/// <summary>
		/// Returns a mock assignment filter.
		/// </summary>
		private static IAssignmentFilter CreateMockAssignmentFilter(
			User user,
			Section section,
			string assignmentGroupName,
			IList<Assignment> assignments,
			IList<Assignment> filteredAssignments, 
			IList<UserQuestionSubmission> submissions,
			IList<UserQuestionSubmission> filteredSubmissions)
		{
			var assignmentFilter = new Mock<IAssignmentFilter>();

			assignmentFilter
				.Setup(m => m.FilterAssignments(assignments, section, assignmentGroupName))
				.Returns(filteredAssignments);

			assignmentFilter
				.Setup(m => m.FilterSubmissions(filteredAssignments, submissions, user, null /*snapshotDate*/))
				.Returns(filteredSubmissions);

			return assignmentFilter.Object;
		}

		/// <summary>
		/// Creates a mock assignment result generator.
		/// </summary>
		private static IAssignmentResultGenerator CreateMockAssignmentResultGenerator(
			User user,
			Section section,
			IList<Assignment> assignments,
			IList<UserQuestionSubmission> submissions,
			IList<AssignmentResult> assignmentResults)
		{
			var assignmentResultGenerator = new Mock<IAssignmentResultGenerator>();

			assignmentResultGenerator
				.Setup(m => m.CreateAssignmentResult(section, assignments[0], user, false /*admin*/, submissions))
				.Returns(assignmentResults[0]);

			assignmentResultGenerator
				.Setup(m => m.CreateAssignmentResult(section, assignments[1], user, false /*admin*/, submissions))
				.Returns(assignmentResults[1]);

			return assignmentResultGenerator.Object;
		}

		/// <summary>
		/// Creates a mock assignment group score calculator.
		/// </summary>
		private IAssignmentGroupScoreCalculator CreateMockAssignmentGroupScoreCalculator(
			IList<Assignment> filteredAssignments,
			IList<AssignmentResult> assignmentResults)
		{
			var assignmentGroupScoreCalculator = new Mock<IAssignmentGroupScoreCalculator>();

			assignmentGroupScoreCalculator
				.Setup
				(
					m => m.GetAssignmentGroupScore
					(
						It.Is<IList<AssignmentResult>>
						(
							seq => seq.SequenceEqual(assignmentResults)
						),
						1 /*roundDigits*/
					)
				).Returns(5.0);

			assignmentGroupScoreCalculator
				.Setup
				(
					m => m.GetAssignmentGroupTotalPoints
					(
						It.Is<IList<Assignment>>
						(
							seq => seq.SequenceEqual(filteredAssignments)
						),
						1 /*roundDigits*/
					)
				).Returns(6.0);

			assignmentGroupScoreCalculator
				.Setup
				(
					m => m.GetAssignmentGroupStatus
					(
						It.Is<IList<AssignmentResult>>
						(
							seq => seq.SequenceEqual(assignmentResults)
						)
					)
				).Returns(new SubmissionStatus(Completion.Completed, late: false));

			return assignmentGroupScoreCalculator.Object;
		}

		/// <summary>
		/// Creates an assignment result for the given assignment.
		/// </summary>
		private AssignmentResult CreateAssignmentResult(Assignment assignment)
		{
			return new SeparateSubmissionsAssignmentResult
			(
				assignmentName: "",
				assignmentId: assignment.Id,
				userId: 0,
				assignmentDueDate: null,
				score: 0.0,
				totalPoints: 0.0,
				status: null,
				questionResults: null
			);
		}
	}
}
