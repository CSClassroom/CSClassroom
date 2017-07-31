using System.Linq;
using CSC.CSClassroom.Model.Questions;
using CSC.CSClassroom.Service.Questions.QuestionDuplicators;
using CSC.CSClassroom.Service.UnitTests.TestDoubles;
using CSC.CSClassroom.Service.UnitTests.Utilities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Questions.QuestionDuplicators
{
	/// <summary>
	/// Unit tests for the MultipleChoiceQuestionDuplicator class.
	/// </summary>
	public class MultipleChoiceQuestionDuplicator_UnitTests
	{
		/// <summary>
		/// Ensures that the choices are duplicated.
		/// </summary>
		[Fact]
		public void DuplicateQuestionAsync_DuplicatesChoices()
		{
			var database = GetDatabase().Build();
			var question = database.Context.MultipleChoiceQuestions
				.Include(q => q.Choices)
				.First();

			var duplicator = new MultipleChoiceQuestionDuplicator(database.Context, question);
			var result = (MultipleChoiceQuestion)duplicator.DuplicateQuestion();

			Assert.Equal(1, question.Choices.Count);
			Assert.True(question.Choices[0] != result.Choices[0]);
			Assert.Equal("Choice", result.Choices[0].Value);
			Assert.Equal(0, result.Choices[0].Id);
		}

		/// <summary>
		/// Builds a database.
		/// </summary>
		private static TestDatabaseBuilder GetDatabase()
		{
			return new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddQuestionCategory("Class1", "Category1")
				.AddQuestion("Class1", "Category1", new MultipleChoiceQuestion()
				{
					Choices = Collections.CreateList(new MultipleChoiceQuestionChoice() { Value = "Choice" })
				});
		}
	}
}
