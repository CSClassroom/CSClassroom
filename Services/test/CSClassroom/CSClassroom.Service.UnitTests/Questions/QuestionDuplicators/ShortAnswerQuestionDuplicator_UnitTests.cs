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
	/// Unit tests for the ShortAnswerQuestionDuplicator class.
	/// </summary>
	public class ShortAnswerQuestionDuplicator_UnitTests
	{
		/// <summary>
		/// Ensures that the blanks are duplicated.
		/// </summary>
		[Fact]
		public void DuplicateQuestionAsync_DuplicatesBlanks()
		{
			var database = GetDatabase().Build();
			var question = database.Context.ShortAnswerQuestions
				.Include(q => q.Blanks)
				.First();

			var duplicator = new ShortAnswerQuestionDuplicator(database.Context, question);
			var result = (ShortAnswerQuestion)duplicator.DuplicateQuestion();

			Assert.Equal(1, result.Blanks.Count);
			Assert.True(question.Blanks[0] != result.Blanks[0]);
			Assert.Equal(0, result.Blanks[0].Id);
		}

		/// <summary>
		/// Builds a database.
		/// </summary>
		private static TestDatabaseBuilder GetDatabase()
		{
			return new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddQuestionCategory("Class1", "Category1")
				.AddQuestion("Class1", "Category1", new ShortAnswerQuestion()
				{
					Blanks = Collections.CreateList(new ShortAnswerQuestionBlank() { Name = "Blank" })
				});
		}
	}
}
