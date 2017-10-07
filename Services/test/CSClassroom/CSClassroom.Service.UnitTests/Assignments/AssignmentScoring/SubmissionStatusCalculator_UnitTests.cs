using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSC.Common.Infrastructure.System;
using CSC.CSClassroom.Model.Assignments.ServiceResults;
using CSC.CSClassroom.Service.Assignments.AssignmentScoring;
using Moq;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Assignments.AssignmentScoring
{
	/// <summary>
	/// Unit tests for the SubmissionStatusCalculator class.
	/// </summary>
	public class SubmissionStatusCalculator_UnitTests
	{
		private DateTime ExampleDate = new DateTime(2017, 1, 1);

		/// <summary>
		/// Verifies that GetStatusForQuestion returns the correct result.
		/// </summary>
		[Theory]
		[InlineData
		(
			/*interactive*/ true,
			/*dateSubmitted*/ 0,
			/*dateDue*/ 0,
			/*dateNow*/ 0,
			/*correctSubmission*/ true,
			/*expectedCompletion*/ Completion.Completed,
			/*expectedIsLate*/ false
		)]
		[InlineData
		(
			/*interactive*/ true,
			/*dateSubmitted*/ 1,
			/*dateDue*/ 0,
			/*dateNow*/ 1,
			/*correctSubmission*/ true,
			/*expectedCompletion*/ Completion.Completed,
			/*expectedIsLate*/ true
		)]
		[InlineData
		(
			/*interactive*/ true,
			/*dateSubmitted*/ -1,
			/*dateDue*/ 0,
			/*dateNow*/ 0,
			/*correctSubmission*/ false,
			/*expectedCompletion*/ Completion.InProgress,
			/*expectedIsLate*/ false
		)]
		[InlineData
		(
			/*interactive*/ true,
			/*dateSubmitted*/ -1,
			/*dateDue*/ 0,
			/*dateNow*/ 1,
			/*correctSubmission*/ false,
			/*expectedCompletion*/ Completion.InProgress,
			/*expectedIsLate*/ true
		)]
		[InlineData
		(
			/*interactive*/ true,
			/*dateSubmitted*/ null,
			/*dateDue*/ 0,
			/*dateNow*/ 0,
			/*correctSubmission*/ null,
			/*expectedCompletion*/ Completion.NotStarted,
			/*expectedIsLate*/ false
		)]
		[InlineData
		(
			/*interactive*/ true,
			/*dateSubmitted*/ null,
			/*dateDue*/ 0,
			/*dateNow*/ 1,
			/*correctSubmission*/ null,
			/*expectedCompletion*/ Completion.NotStarted,
			/*expectedIsLate*/ true
		)]
		[InlineData
		(
			/*interactive*/ false,
			/*dateSubmitted*/ 0,
			/*dateDue*/ 0,
			/*dateNow*/ 0,
			/*correctSubmission*/ true,
			/*expectedCompletion*/ Completion.Completed,
			/*expectedIsLate*/ false
		)]
		[InlineData
		(
			/*interactive*/ false,
			/*dateSubmitted*/ -1,
			/*dateDue*/ 0,
			/*dateNow*/ 0,
			/*correctSubmission*/ false,
			/*expectedCompletion*/ Completion.Completed,
			/*expectedIsLate*/ false
		)]
		[InlineData
		(
			/*interactive*/ false,
			/*dateSubmitted*/ 11,
			/*dateDue*/ 0,
			/*dateNow*/ 1,
			/*correctSubmission*/ true,
			/*expectedCompletion*/ Completion.Completed,
			/*expectedIsLate*/ true
		)]
		[InlineData
		(
			/*interactive*/ false,
			/*dateSubmitted*/ -1,
			/*dateDue*/ 0,
			/*dateNow*/ 1,
			/*correctSubmission*/ false,
			/*expectedCompletion*/ Completion.Completed,
			/*expectedIsLate*/ false
		)]
		[InlineData
		(
			/*interactive*/ false,
			/*dateSubmitted*/ null,
			/*dateDue*/ 0,
			/*dateNow*/ 0,
			/*correctSubmission*/ null,
			/*expectedCompletion*/ Completion.NotStarted,
			/*expectedIsLate*/ false
		)]
		[InlineData
		(
			/*interactive*/ false,
			/*dateSubmitted*/ null,
			/*dateDue*/ 0,
			/*dateNow*/ 1,
			/*correctSubmission*/ null,
			/*expectedCompletion*/ Completion.NotStarted,
			/*expectedIsLate*/ true
		)]
		[InlineData
		(
			/*interactive*/ false,
			/*dateSubmitted*/ null,
			/*dateDue*/ null,
			/*dateNow*/ 1,
			/*correctSubmission*/ null,
			/*expectedCompletion*/ Completion.NotStarted,
			/*expectedIsLate*/ false
		)]
		public void GetStatusForQuestion_VerifyCorrectResult(
			bool interactive,
			int? dateSubmitted,
			int? dateDue,
			int dateNow,
			bool? correctSubmission,
			Completion expectedCompletion,
			bool expectedIsLate)
		{
			var submissionStatusCalculator = CreateSubmissionStatusCalculator
			(
				ExampleDate + TimeSpan.FromDays(dateNow)
			);

			var result = submissionStatusCalculator.GetStatusForQuestion
			(
				GetDate(dateSubmitted),
				GetDate(dateDue),
				interactive,
				correctSubmission.HasValue && correctSubmission.Value ? 1.0 : 0.0
			);

			Assert.Equal(expectedCompletion, result.Completion);
			Assert.Equal(expectedIsLate, result.Late);
		}

		/// <summary>
		/// Verifies that GetStatusForAssignment returns the correct result.
		/// </summary>
		[Theory]
		[InlineData
		(
			/*anyNotStarted*/ false,
			/*anyInProgress*/ false,
			/*anyCompleted*/ true,
			/*anyLate*/ false,
			/*expectedCompletion*/ Completion.Completed,
			/*expectedIsLate*/ false
		)]
		[InlineData
		(
			/*anyNotStarted*/ false,
			/*anyInProgress*/ false,
			/*anyCompleted*/ true,
			/*anyLate*/ true,
			/*expectedCompletion*/ Completion.Completed,
			/*expectedIsLate*/ true
		)]
		[InlineData
		(
			/*anyNotStarted*/ true,
			/*anyInProgress*/ true,
			/*anyCompleted*/ false,
			/*anyLate*/ false,
			/*expectedCompletion*/ Completion.InProgress,
			/*expectedIsLate*/ false
		)]
		[InlineData
		(
			/*anyNotStarted*/ true,
			/*anyInProgress*/ true,
			/*anyCompleted*/ false,
			/*anyLate*/ true,
			/*expectedCompletion*/ Completion.InProgress,
			/*expectedIsLate*/ true
		)]
		[InlineData
		(
			/*anyNotStarted*/ true,
			/*anyInProgress*/ false,
			/*anyCompleted*/ false,
			/*anyLate*/ false,
			/*expectedCompletion*/ Completion.NotStarted,
			/*expectedIsLate*/ false
		)]
		[InlineData
		(
			/*anyNotStarted*/ true,
			/*anyInProgress*/ false,
			/*anyCompleted*/ false,
			/*anyLate*/ true,
			/*expectedCompletion*/ Completion.NotStarted,
			/*expectedIsLate*/ true
		)]
		public void GetStatusForAssignment(
			bool anyNotStarted,
			bool anyInProgress,
			bool anyCompleted,
			bool anyLate,
			Completion expectedCompletion,
			bool expectedIsLate)
		{
			var submissionStatusCalculator = CreateSubmissionStatusCalculator();

			bool mustAddLate = anyLate;

			var inputs = new List<SubmissionStatus>();
			if (anyNotStarted)
				AddSubmissionStatus(inputs, Completion.NotStarted, ref mustAddLate);
			if (anyInProgress)
				AddSubmissionStatus(inputs, Completion.InProgress, ref mustAddLate);
			if (anyCompleted)
				AddSubmissionStatus(inputs, Completion.Completed, ref mustAddLate);

			var result = submissionStatusCalculator.GetStatusForAssignment(inputs);

			Assert.Equal(expectedCompletion, result.Completion);
			Assert.Equal(expectedIsLate, result.Late);
		}

		/// <summary>
		/// Adds a SubmissionStatus object to the given list.
		/// </summary>
		private void AddSubmissionStatus(
			IList<SubmissionStatus> inputs, 
			Completion completion, 
			ref bool mustAddLate)
		{
			inputs.Add(new SubmissionStatus(completion, mustAddLate));
			if (mustAddLate)
			{
				mustAddLate = false;
			}
		}

		/// <summary>
		/// Returns a date for the given days offset.
		/// </summary>
		private DateTime? GetDate(int? daysOffset)
		{
			return daysOffset.HasValue
				? ExampleDate + TimeSpan.FromDays(daysOffset.Value)
				: (DateTime?)null;
		}

		/// <summary>
		/// Creates a new submission status calculator.
		/// </summary>
		private SubmissionStatusCalculator CreateSubmissionStatusCalculator(
			DateTime? dateTimeNow = null)
		{
			var mockTimeProvider = new Mock<ITimeProvider>();
			if (dateTimeNow.HasValue)
			{
				mockTimeProvider
					.Setup(m => m.UtcNow)
					.Returns(dateTimeNow.Value);
			}

			return new SubmissionStatusCalculator(mockTimeProvider.Object);
		}
	}
}
