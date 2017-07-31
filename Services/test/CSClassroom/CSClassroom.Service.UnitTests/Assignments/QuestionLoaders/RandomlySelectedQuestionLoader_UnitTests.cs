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
	/// Unit tests for the RandomlySelectedQuestionLoader class.
	/// </summary>
	public class RandomlySelectedQuestionLoader_UnitTests
	{
		/// <summary>
		/// Ensures that the chocies category is loaded.
		/// </summary>
		[Fact]
		public async Task LoadQuestionAsync_LoadsChoicesCategory()
		{
			var database = GetDatabase().Build();
			var question = database.Context.RandomlySelectedQuestions.First();
			var loader = new RandomlySelectedQuestionLoader(database.Context, question);

			await loader.LoadQuestionAsync();

			Assert.NotNull(question.ChoicesCategory);
		}

		/// <summary>
		/// Builds a database.
		/// </summary>
		private static TestDatabaseBuilder GetDatabase()
		{
			return new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddQuestionCategory("Class1", "Category1")
				.AddQuestion("Class1", "Category1", new RandomlySelectedQuestion()
				{
					ChoicesCategory = new QuestionCategory()
				});
		}
	}
}
