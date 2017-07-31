using System;
using System.Collections.Generic;
using System.Linq;
using CSC.Common.Infrastructure.System;
using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Model.Assignments.ServiceResults;
using CSC.CSClassroom.Model.Users;
using CSC.CSClassroom.Service.Assignments.AssignmentScoring;
using CSC.CSClassroom.Service.UnitTests.TestDoubles;
using CSC.CSClassroom.Service.UnitTests.Utilities;
using Moq;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Assignments.AssignmentScoring
{
	/// <summary>
	/// Unit tests for the AssignmentGroupScoreCalculator class.
	/// </summary>
	public class AssignmentGroupScoreCalculator_UnitTests
	{
		/// <summary>
		/// Ensures GetAssignmentGroupScore correctly sums up the scores
		/// for each assignment in the group.
		/// </summary>
		[Fact]
		public void GetAssignmentGroupScore_ReturnsSum()
		{
			var questionResults = Collections.CreateList
			(
				CreateAssignmentResult(score: 1.0),
				CreateAssignmentResult(score: 2.0),
				CreateAssignmentResult(score: 2.954)
			);

			var assignmentGroupScoreCalculator = CreateAssignmentGroupScoreCalculator();
			var result = assignmentGroupScoreCalculator.GetAssignmentGroupScore
			(
				questionResults,
				roundDigits: 2
			);

			Assert.Equal(5.95, result);
		}

		/// <summary>
		/// Ensures GetAssignmentGroupTotalPoints correctly returns the total number
		/// of points in the assignment group.
		/// </summary>
		[Fact]
		public void GetAssignmentGroupTotalPoints_ReturnsTotalPoints()
		{
			var assignments = Collections.CreateList
			(
				new Assignment()
				{
					Questions = Collections.CreateList
					(
						new AssignmentQuestion() { Points = 1.0 },
						new AssignmentQuestion() { Points = 2.0 }
					)
				},
				new Assignment()
				{
					Questions = Collections.CreateList
					(
						new AssignmentQuestion() { Points = 3.0 },
						new AssignmentQuestion() { Points = 4.0 }
					)
				}
			);

			var assignmentGroupScoreCalculator = CreateAssignmentGroupScoreCalculator();
			var result = assignmentGroupScoreCalculator.GetAssignmentGroupTotalPoints
			(
				assignments,
				roundDigits: 2
			);

			Assert.Equal(10.0, result);
		}

		/// <summary>
		/// Ensures GetAssignmentGroupStatus correctly returns the status of 
		/// the assignment group.
		/// </summary>
		[Fact]
		public void GetAssignmentStatus_GivenQuestionResults_ReturnsAssignmentStatus()
		{
			var assignmentResults = Collections.CreateList
			(
				CreateAssignmentResult(status: new SubmissionStatus(Completion.Completed, late: false)),
				CreateAssignmentResult(status: new SubmissionStatus(Completion.InProgress, late: true)),
				CreateAssignmentResult(status: new SubmissionStatus(Completion.NotStarted, late: true))
			);

			var expectedResult = new SubmissionStatus(Completion.Completed, late: false);
			var submissionStatusCalculator = CreateSubmissionStatusCalculator
			(
				assignmentResults.Select(qr => qr.Status),
				expectedResult
			);

			var assignmentGroupScoreCalculator = CreateAssignmentGroupScoreCalculator
			(
				submissionStatusCalculator
			);

			var result = assignmentGroupScoreCalculator.GetAssignmentGroupStatus
			(
				assignmentResults
			);

			Assert.Equal(expectedResult, result);
		}

		/// <summary>
		/// Creates a mock submission status calculator.
		/// </summary>
		private ISubmissionStatusCalculator CreateSubmissionStatusCalculator(
			IEnumerable<SubmissionStatus> expectedQuestionStatus,
			SubmissionStatus expectedResult)
		{
			var submissionStatusCalculator = new Mock<ISubmissionStatusCalculator>();
			submissionStatusCalculator
				.Setup
				(
					m => m.GetStatusForAssignment
					(
						It.Is<IList<SubmissionStatus>>
						(
							list => list.SequenceEqual(expectedQuestionStatus)
						)
					)
				).Returns(expectedResult);

			return submissionStatusCalculator.Object;
		}

		/// <summary>
		/// Creates an assignment group score calculator.
		/// </summary>
		private AssignmentGroupScoreCalculator CreateAssignmentGroupScoreCalculator(
			ISubmissionStatusCalculator submissionStatusCalculator = null)
		{
			return new AssignmentGroupScoreCalculator
			(
				submissionStatusCalculator
			);
		}

		/// <summary>
		/// Creates an assignment submission result with the given score and status.
		/// </summary>
		private AssignmentResult CreateAssignmentResult(
			double score = 0.0,
			SubmissionStatus status = null)
		{
			return new SeparateSubmissionsAssignmentResult
			(
				assignmentName: "",
				assignmentId: 1,
				userId: 1,
				assignmentDueDate: null,
				score: score,
				totalPoints: 0,
				status: status,
				questionResults: null
			);
		}
	}
}