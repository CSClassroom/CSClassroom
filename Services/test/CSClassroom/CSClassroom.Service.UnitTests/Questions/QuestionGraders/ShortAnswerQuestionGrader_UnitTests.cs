using System.Threading.Tasks;
using CSC.CSClassroom.Model.Questions;
using CSC.CSClassroom.Model.Questions.ServiceResults;
using CSC.CSClassroom.Service.Questions.QuestionGraders;
using CSC.CSClassroom.Service.UnitTests.TestDoubles;
using CSC.CSClassroom.Service.UnitTests.Utilities;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Questions.QuestionGraders
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
	}
}
