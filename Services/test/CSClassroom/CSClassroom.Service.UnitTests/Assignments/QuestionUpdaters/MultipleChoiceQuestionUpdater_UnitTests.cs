using System.Linq;
using System.Threading.Tasks;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Service.Assignments.QuestionUpdaters;
using CSC.CSClassroom.Service.UnitTests.TestDoubles;
using CSC.CSClassroom.Service.UnitTests.Utilities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Assignments.QuestionUpdaters
{
	/// <summary>
	/// Unit tests for the MultipleChoiceQuestionUpdater class.
	/// </summary>
	public class MultipleChoiceQuestionUpdater_UnitTests
	{
		/// <summary>
		/// Ensures that answer choices are updated.
		/// </summary>
		[Fact]
		public async Task UpdateQuestionAsync_UpdatesChoices()
		{
			var database = GetDatabase().Build();
			var question = database.Context.MultipleChoiceQuestions
				.Include(q => q.Choices)
				.First();

			database.Reload();

			question.Choices.Clear();
			question.Choices.Add(new MultipleChoiceQuestionChoice() { Value = "NewChoice1\r\nLine2" });
			question.Choices.Add(new MultipleChoiceQuestionChoice() { Value = "NewChoice2\r\nLine2" });

			var errors = new MockErrorCollection();
			var updater = new MultipleChoiceQuestionUpdater(database.Context, question, errors);
			await updater.UpdateQuestionAsync();

			Assert.False(errors.HasErrors);
			Assert.Equal(2, question.Choices.Count);
			Assert.Equal("NewChoice1\nLine2", question.Choices[0].Value);
			Assert.Equal(0, question.Choices[0].Order);
			Assert.Equal("NewChoice2\nLine2", question.Choices[1].Value);
			Assert.Equal(1, question.Choices[1].Order);
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
