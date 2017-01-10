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
	/// Unit tests for the MultipleChoiceQuestionGrader class.
	/// </summary>
	public class MultipleChoiceQuestionGrader_UnitTests
	{
		/// <summary>
		/// Ensures that full credit is received for having the correct answer.
		/// </summary>
		[Fact]
		public async Task GradeSubmissionAsync_OneCorrectAnswer_CorrectSubmission()
		{
			var question = new MultipleChoiceQuestion()
			{
				AllowMultipleCorrectAnswers = true,
				Choices = Collections.CreateList
				(
					new MultipleChoiceQuestionChoice() { Value = "A", Correct = true },
					new MultipleChoiceQuestionChoice() { Value = "B", Correct = false },
					new MultipleChoiceQuestionChoice() { Value = "C", Correct = false },
					new MultipleChoiceQuestionChoice() { Value = "D", Correct = false }
				)
			};

			var submission = new MultipleChoiceQuestionSubmission()
			{
				SelectedChoices = Collections.CreateList("A")
			};

			var grader = new MultipleChoiceQuestionGrader(question);
			var result = await grader.GradeSubmissionAsync(submission);

			Assert.True(((MultipleChoiceQuestionResult)result.Result).Correct);
			Assert.Equal(1.0, result.Score);
		}

		/// <summary>
		/// Ensures that no credit is received for having an incorrect answer.
		/// </summary>
		[Fact]
		public async Task GradeSubmissionAsync_OneCorrectAnswer_IncorrectSubmission()
		{
			var question = new MultipleChoiceQuestion()
			{
				Choices = Collections.CreateList
				(
					new MultipleChoiceQuestionChoice() { Value = "A", Correct = true },
					new MultipleChoiceQuestionChoice() { Value = "B", Correct = false },
					new MultipleChoiceQuestionChoice() { Value = "C", Correct = false },
					new MultipleChoiceQuestionChoice() { Value = "D", Correct = false }
				)
			};

			var submission = new MultipleChoiceQuestionSubmission()
			{
				SelectedChoices = Collections.CreateList("B")
			};

			var grader = new MultipleChoiceQuestionGrader(question);
			var result = await grader.GradeSubmissionAsync(submission);

			Assert.False(((MultipleChoiceQuestionResult)result.Result).Correct);
			Assert.Equal(0.0, result.Score);
		}

		/// <summary>
		/// Ensures that full credit is received for having all correct answers
		/// for a question that requires multiple answers to be selected.
		/// </summary>
		[Fact]
		public async Task GradeSubmissionAsync_MultipleCorrectAnswers_CorrectSubmission()
		{
			var question = new MultipleChoiceQuestion()
			{
				AllowMultipleCorrectAnswers = true,
				Choices = Collections.CreateList
				(
					new MultipleChoiceQuestionChoice() { Value = "A", Correct = true },
					new MultipleChoiceQuestionChoice() { Value = "B", Correct = false },
					new MultipleChoiceQuestionChoice() { Value = "C", Correct = false },
					new MultipleChoiceQuestionChoice() { Value = "D", Correct = true }
				)
			};

			var submission = new MultipleChoiceQuestionSubmission()
			{
				SelectedChoices = Collections.CreateList("A", "D")
			};

			var grader = new MultipleChoiceQuestionGrader(question);
			var result = await grader.GradeSubmissionAsync(submission);

			Assert.True(((MultipleChoiceQuestionResult)result.Result).Correct);
			Assert.Equal(1.0, result.Score);
		}

		/// <summary>
		/// Ensures that no credit is received for having some incorrect answers
		/// for a question that requires multiple answers to be selected.
		/// </summary>
		[Fact]
		public async Task GradeSubmissionAsync_MultipleCorrectAnswers_IncorrectSubmission()
		{
			var question = new MultipleChoiceQuestion()
			{
				AllowMultipleCorrectAnswers = true,
				Choices = Collections.CreateList
				(
					new MultipleChoiceQuestionChoice() { Value = "A", Correct = true },
					new MultipleChoiceQuestionChoice() { Value = "B", Correct = false },
					new MultipleChoiceQuestionChoice() { Value = "C", Correct = false },
					new MultipleChoiceQuestionChoice() { Value = "D", Correct = true }
				)
			};

			var submission = new MultipleChoiceQuestionSubmission()
			{
				SelectedChoices = Collections.CreateList("A")
			};

			var grader = new MultipleChoiceQuestionGrader(question);
			var result = await grader.GradeSubmissionAsync(submission);

			Assert.False(((MultipleChoiceQuestionResult)result.Result).Correct);
			Assert.Equal(0.0, result.Score);
		}
	}
}
