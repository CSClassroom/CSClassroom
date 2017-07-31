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
	/// Unit tests for the ProgramQuestionLoader class.
	/// </summary>
	public class ProgramQuestionLoader_UnitTests
	{
		/// <summary>
		/// Ensures that question tests are loaded.
		/// </summary>
		[Fact]
		public async Task LoadQuestionAsync_LoadsTests()
		{
			var database = GetDatabase().Build();
			var question = database.Context.ProgramQuestions.First();
			var loader = new ProgramQuestionLoader(database.Context, question);

			await loader.LoadQuestionAsync();

			Assert.Equal(1, question.Tests.Count);
			Assert.Equal("Test", question.Tests[0].Name);
		}

		/// <summary>
		/// Builds a database.
		/// </summary>
		private static TestDatabaseBuilder GetDatabase()
		{
			return new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddQuestionCategory("Class1", "Category1")
				.AddQuestion("Class1", "Category1", new ProgramQuestion()
				{
					Tests = Collections.CreateList(new ProgramQuestionTest() { Name = "Test" })
				});
		}
	}
}
