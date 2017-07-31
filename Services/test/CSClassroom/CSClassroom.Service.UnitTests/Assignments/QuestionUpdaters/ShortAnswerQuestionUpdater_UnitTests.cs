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
	/// Unit tests for the ShortAnswerQuestionUpdater class.
	/// </summary>
	public class ShortAnswerQuestionUpdater_UnitTests
	{
		/// <summary>
		/// Ensures that blanks are updated.
		/// </summary>
		[Fact]
		public async Task UpdateQuestionAsync_UpdatesBlanks()
		{
			var database = GetDatabase().Build();
			var question = database.Context.ShortAnswerQuestions
				.Include(q => q.Blanks)
				.First();

			database.Reload();

			question.Blanks.Clear();
			question.Blanks.Add(new ShortAnswerQuestionBlank() { Name = "NewBlank1\r\nLine2" });
			question.Blanks.Add(new ShortAnswerQuestionBlank() { Name = "NewBlank2\r\nLine2" });

			var errors = new MockErrorCollection();
			var updater = new ShortAnswerQuestionUpdater(database.Context, question, errors);
			await updater.UpdateQuestionAsync();

			Assert.False(errors.HasErrors);
			Assert.Equal(2, question.Blanks.Count);
			Assert.Equal("NewBlank1\nLine2", question.Blanks[0].Name);
			Assert.Equal(0, question.Blanks[0].Order);
			Assert.Equal("NewBlank2\nLine2", question.Blanks[1].Name);
			Assert.Equal(1, question.Blanks[1].Order);
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
					Blanks = Collections.CreateList(new ShortAnswerQuestionBlank() { Name = "Choice" })
				});
		}
	}
}
