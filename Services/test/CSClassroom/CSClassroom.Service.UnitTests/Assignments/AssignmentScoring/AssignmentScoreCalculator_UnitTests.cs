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
	/// Unit tests for the AssignmentScoreCalculator class.
	/// </summary>
	public class AssignmentScoreCalculator_UnitTests
	{
		/// <summary>
		/// Ensures GetAssignmentScore correctly sums up the scores
		/// for each question in the assignment.
		/// </summary>
		[Fact]
		public void GetAssignmentScore_GivenQuestionResults_ReturnsSum()
		{
			var questionResults = Collections.CreateList
			(
				CreateQuestionResult(score: 1.0),
				CreateQuestionResult(score: 2.0),
				CreateQuestionResult(score: 2.954)
			);

			var assignmentScoreCalculator = CreateAssignmentScoreCalculator();
			var result = assignmentScoreCalculator.GetAssignmentScore
			(
				questionResults,
				roundDigits: 2
			);

			Assert.Equal(5.95, result);
		}

		/// <summary>
		/// Ensures GetAssignmentScore correctly returns the maximum submission score
		/// when given a list of submission results.
		/// </summary>
		[Fact]
		public void GetAssignmentScore_GivenAssignmentSubmissionResults_ReturnsMaxScore()
		{
			var assignmentSubmissionResults = Collections.CreateList
			(
				CreateAssignmentSubmissionResult(score: 1.0),
				CreateAssignmentSubmissionResult(score: 2.0),
				CreateAssignmentSubmissionResult(score: 2.954)
			);

			var assignmentScoreCalculator = CreateAssignmentScoreCalculator();
			var result = assignmentScoreCalculator.GetAssignmentScore
			(
				assignmentSubmissionResults,
				roundDigits: 2
			);

			Assert.Equal(2.95, result);
		}

		/// <summary>
		/// Ensures GetAssignmentScore correctly returns zero when given a list
		/// of zero submission results.
		/// </summary>
		[Fact]
		public void GetAssignmentScore_GivenZeroAssignmentSubmissionResults_ReturnsZero()
		{
			var assignmentSubmissionResults = new List<AssignmentSubmissionResult>();

			var assignmentScoreCalculator = CreateAssignmentScoreCalculator();
			var result = assignmentScoreCalculator.GetAssignmentScore
			(
				assignmentSubmissionResults,
				roundDigits: 2
			);

			Assert.Equal(0.0, result);
		}

		/// <summary>
		/// Ensures GetAssignmentStatus correctly returns the assignment status
		/// given a set of question results.
		/// </summary>
		[Fact]
		public void GetAssignmentStatus_GivenQuestionResults_ReturnsAssignmentStatus()
		{
			var questionResults = Collections.CreateList
			(
				CreateQuestionResult(status: new SubmissionStatus(Completion.Completed, late: false)),
				CreateQuestionResult(status: new SubmissionStatus(Completion.InProgress, late: true)),
				CreateQuestionResult(status: new SubmissionStatus(Completion.NotStarted, late: true))
			);

			var expectedResult = new SubmissionStatus(Completion.Completed, late: false);
			var submissionStatusCalculator = CreateSubmissionStatusCalculator
			(
				questionResults.Select(qr => qr.Status),
				expectedResult
			);

			var assignmentScoreCalculator = CreateAssignmentScoreCalculator
			(
				submissionStatusCalculator
			);

			var result = assignmentScoreCalculator.GetAssignmentStatus(questionResults);

			Assert.Equal(expectedResult, result);
		}

		/// <summary>
		/// Ensures GetAssignmentStatus correctly returns the assignment status
		/// given a set of question results.
		/// </summary>
		[Fact]
		public void GetAssignmentStatus_GivenAssignmentSubmissionResults_ReturnsHighestScoringStatus()
		{
			var assignmentSubmissionResults = Collections.CreateList
			(
				CreateAssignmentSubmissionResult
				(
					score: 1.0, 
					status: new SubmissionStatus(Completion.Completed, late: false)
				),
				CreateAssignmentSubmissionResult
				(
					score: 0.0,
					status: new SubmissionStatus(Completion.InProgress, late: true)
				),
				CreateAssignmentSubmissionResult
				(
					score: 0.0,
					status: new SubmissionStatus(Completion.NotStarted, late: true)
				)
			);

			var assignmentScoreCalculator = CreateAssignmentScoreCalculator();
			var result = assignmentScoreCalculator.GetAssignmentStatus
			(
				assignmentSubmissionResults,
				dueDate: null
			);

			Assert.Equal(assignmentSubmissionResults[0].Status, result);
		}

		/// <summary>
		/// Ensures GetAssignmentStatus correctly returns a status of NotStarted
		/// that is not yet late, when the due date has not yet been reached.
		/// </summary>
		[Fact]
		public void GetAssignmentStatus_GivenZeroAssignmentSubmissionResultsBeforeDeadline_ReturnNotStartedOnTime()
		{
			var assignmentSubmissionResults = new List<AssignmentSubmissionResult>();

			var timeProvider = new Mock<ITimeProvider>();
			timeProvider.Setup(m => m.UtcNow).Returns(DateTime.MinValue);

			var assignmentScoreCalculator = CreateAssignmentScoreCalculator
			(
				timeProvider: timeProvider.Object
			);

			var result = assignmentScoreCalculator.GetAssignmentStatus
			(
				assignmentSubmissionResults, DateTime.MaxValue
			);

			Assert.Equal(Completion.NotStarted, result.Completion);
			Assert.False(result.Late);
		}

		/// <summary>
		/// Ensures GetAssignmentStatus correctly returns a status of NotStarted
		/// that is late, when the due date has been passed.
		/// </summary>
		[Fact]
		public void GetAssignmentStatus_GivenZeroAssignmentSubmissionResultsAfterDeadline_ReturnNotStartedLate()
		{
			var assignmentSubmissionResults = new List<AssignmentSubmissionResult>();

			var timeProvider = new Mock<ITimeProvider>();
			timeProvider.Setup(m => m.UtcNow).Returns(DateTime.MaxValue);

			var assignmentScoreCalculator = CreateAssignmentScoreCalculator
			(
				timeProvider: timeProvider.Object
			);

			var result = assignmentScoreCalculator.GetAssignmentStatus
			(
				assignmentSubmissionResults, DateTime.MinValue
			);

			Assert.Equal(Completion.NotStarted, result.Completion);
			Assert.True(result.Late);
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
		/// Creates an assignment score calculator.
		/// </summary>
		private AssignmentScoreCalculator CreateAssignmentScoreCalculator(
			ISubmissionStatusCalculator submissionStatusCalculator = null,
			ITimeProvider timeProvider = null)
		{
			return new AssignmentScoreCalculator
			(
				submissionStatusCalculator, 
				timeProvider
			);
		}

		/// <summary>
		/// Creates a question result with the given score and status.
		/// </summary>
		private StudentQuestionResult CreateQuestionResult(
			double score = 0.0, 
			SubmissionStatus status = null)
		{
			return new StudentQuestionResult
			(
				questionId: 1,
				assignmentId: 1,
				userId: 1,
				combinedSubmissions: false,
				questionName: "",
				questionPoints: 0.0,
				score: score,
				status: status,
				submissionResults: null
			);
		}

		/// <summary>
		/// Creates an assignment submission result with the given score and status.
		/// </summary>
		private AssignmentSubmissionResult CreateAssignmentSubmissionResult(
			double score = 0.0,
			SubmissionStatus status = null)
		{
			return new AssignmentSubmissionResult
			(
				assignmentId: 1,
				userId: 1,
				submissionDate: DateTime.MinValue,
				status: status,
				score: score,
				assignmentPoints: 0.0,
				questionResults: null
			);
		}
	}
}