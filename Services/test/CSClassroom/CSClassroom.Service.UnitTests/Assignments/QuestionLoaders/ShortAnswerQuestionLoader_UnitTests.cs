using System.Linq;
using System.Threading.Tasks;
using CSC.CSClassroom.Model.Questions;
using CSC.CSClassroom.Service.Questions.QuestionLoaders;
using CSC.CSClassroom.Service.UnitTests.TestDoubles;
using CSC.CSClassroom.Service.UnitTests.Utilities;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Questions.QuestionLoaders
{
	/// <summary>
	/// Unit tests for the ShortAnswerQuestionLoader class.
	/// </summary>
	public class ShortAnswerQuestionLoader_UnitTests
	{
		/// <summary>
		/// Ensures that question blanks are loaded.
		/// </summary>
		[Fact]
		public async Task LoadQuestionAsync_LoadsBlanks()
		{
			var database = GetDatabase().Build();
			var question = database.Context.ShortAnswerQuestions.First();
			var loader = new ShortAnswerQuestionLoader(database.Context, question);

			await loader.LoadQuestionAsync();

			Assert.Equal(1, question.Blanks.Count);
			Assert.Equal("Blank1", question.Blanks[0].Name);
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
					Blanks = Collections.CreateList(new ShortAnswerQuestionBlank() { Name = "Blank1" })
				});
		}
	}
}
