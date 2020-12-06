using System.Threading.Tasks;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Model.Assignments.ServiceResults;
using CSC.CSClassroom.Service.Assignments.QuestionGraders;
using CSC.CSClassroom.Service.UnitTests.TestDoubles;
using CSC.CSClassroom.Service.UnitTests.Utilities;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Assignments.QuestionGraders
{
	/// <summary>
	/// Unit tests for the ShortAnswerQuestionGrader class.
	/// </summary>
	public class ShortAnswerQuestionGrader_UnitTests
	{
		/// <summary>
		/// Ensures that full credit is received for having all correct answers.
		/// </summary>
		[Fact]
		public async Task GradeSubmissionAsync_AllBlanksCorrect_FullCredit()
		{
			var question = new ShortAnswerQuestion()
			{
				Blanks = Collections.CreateList
				(
					new ShortAnswerQuestionBlank() { Name = "A", Answer = "One" },
					new ShortAnswerQuestionBlank() { Name = "B", Answer = "Two" }
				)
			};

			var submission = new ShortAnswerQuestionSubmission()
			{
				Blanks = Collections.CreateList
				(
					new SubmissionBlank() { Name = "A", Answer = "One" },
					new SubmissionBlank() { Name = "B", Answer = "Two" }
				)
			};

			var grader = new ShortAnswerQuestionGrader(question);
			var result = await grader.GradeSubmissionAsync(submission);

			Assert.Equal(new[] { true, true }, ((ShortAnswerQuestionResult)result.Result).Correct);
			Assert.Equal(1.0, result.Score);
		}

		/// <summary>
		/// Ensures that no credit is received for having some incorrect answers,
		/// when the question does not offer partial credit.
		/// </summary>
		[Fact]
		public async Task GradeSubmissionAsync_SomeBlanksIncorrect_NoCredit()
		{
			var question = new ShortAnswerQuestion()
			{
				AllowPartialCredit = false,
				Blanks = Collections.CreateList
				(
					new ShortAnswerQuestionBlank() { Name = "A", Answer = "One" },
					new ShortAnswerQuestionBlank() { Name = "B", Answer = "Two" }
				)
			};

			var submission = new ShortAnswerQuestionSubmission()
			{
				Blanks = Collections.CreateList
				(
					new SubmissionBlank() { Name = "A", Answer = "One" },
					new SubmissionBlank() { Name = "B", Answer = "Wrong" }
				)
			};

			var grader = new ShortAnswerQuestionGrader(question);
			var result = await grader.GradeSubmissionAsync(submission);

			Assert.Equal(new[] { true, false }, ((ShortAnswerQuestionResult)result.Result).Correct);
			Assert.Equal(0.0, result.Score);
		}

		/// <summary>
		/// Ensures that some credit is received for having a mix of correct answers
		/// and incorrect answers, when the question offers partial credit.
		/// </summary>
		[Fact]
		public async Task GradeSubmissionAsync_SomeBlanksIncorrect_PartialCredit()
		{
			var question = new ShortAnswerQuestion()
			{
				AllowPartialCredit = true,
				Blanks = Collections.CreateList
				(
					new ShortAnswerQuestionBlank() { Name = "A", Answer = "One" },
					new ShortAnswerQuestionBlank() { Name = "B", Answer = "Two" }
				)
			};

			var submission = new ShortAnswerQuestionSubmission()
			{
				Blanks = Collections.CreateList
				(
					new SubmissionBlank() { Name = "A", Answer = "One" },
					new SubmissionBlank() { Name = "B", Answer = "Wrong" }
				)
			};

			var grader = new ShortAnswerQuestionGrader(question);
			var result = await grader.GradeSubmissionAsync(submission);

			Assert.Equal(new[] { true, false }, ((ShortAnswerQuestionResult)result.Result).Correct);
			Assert.Equal(0.5, result.Score);
		}

		/// <summary>
		/// Non-ajax submissions will submit carriage returns instead of newlines. This test
		/// ensures that we correctly handle submissions with blank names containing carriage
		/// returns.
		/// </summary>
		[Fact]
		public async Task GradeSubmissionAsync_BlankNamesContainCarriageReturns_ParsedCorrectly()
		{
			var question = new ShortAnswerQuestion()
			{
				Blanks = Collections.CreateList
				(
					new ShortAnswerQuestionBlank() { Name = "A\n", Answer = "One" }
				)
			};

			var submission = new ShortAnswerQuestionSubmission()
			{
				Blanks = Collections.CreateList
				(
					new SubmissionBlank() { Name = "A\r\n", Answer = "One" }
				)
			};

			var grader = new ShortAnswerQuestionGrader(question);
			var result = await grader.GradeSubmissionAsync(submission);

			Assert.Equal(new[] { true }, ((ShortAnswerQuestionResult)result.Result).Correct);
			Assert.Equal(1.0, result.Score);
		}
	}
}
