using System.Linq;
using System.Threading.Tasks;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Service.Assignments.QuestionLoaders;
using CSC.CSClassroom.Service.UnitTests.TestDoubles;
using CSC.CSClassroom.Service.UnitTests.Utilities;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Assignments.QuestionLoaders
{
	/// <summary>
	/// Unit tests for the MultipleChoiceQuestionLoader class.
	/// </summary>
	public class MultipleChoiceQuestionLoader_UnitTests
	{
		/// <summary>
		/// Ensures that question choices are loaded.
		/// </summary>
		[Fact]
		public async Task LoadQuestionAsync_LoadsChoices()
		{
			var database = GetDatabase().Build();
			var question = database.Context.MultipleChoiceQuestions.First();
			var loader = new MultipleChoiceQuestionLoader(database.Context, question);

			await loader.LoadQuestionAsync();

			Assert.Single(question.Choices);
			Assert.Equal("Choice1", question.Choices[0].Value);
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
					Choices = Collections.CreateList(new MultipleChoiceQuestionChoice() { Value = "Choice1" })
				});
		}
	}
}
