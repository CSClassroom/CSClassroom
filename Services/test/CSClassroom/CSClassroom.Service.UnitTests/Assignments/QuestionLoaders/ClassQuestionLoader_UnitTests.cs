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
	/// Unit tests for the ClassQuestionLoader class.
	/// </summary>
	public class ClassQuestionLoader_UnitTests
	{
		/// <summary>
		/// Ensures that required methods are loaded.
		/// </summary>
		[Fact]
		public async Task LoadQuestionAsync_LoadsRequiredMethods()
		{
			var database = GetDatabase().Build();
			var question = database.Context.ClassQuestions.First();
			var loader = new ClassQuestionLoader(database.Context, question);

			await loader.LoadQuestionAsync();

			Assert.Equal(1, question.RequiredMethods.Count);
			Assert.Equal("Required", question.RequiredMethods[0].Name);
		}

		/// <summary>
		/// Ensures that question tests are loaded.
		/// </summary>
		[Fact]
		public async Task LoadQuestionAsync_LoadsTests()
		{
			var database = GetDatabase().Build();
			var question = database.Context.ClassQuestions.First();
			var loader = new ClassQuestionLoader(database.Context, question);

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
				.AddQuestion("Class1", "Category1", new ClassQuestion()
				{
					ImportedClasses = Collections.CreateList(new ImportedClass() { ClassName = "Imported"}),
					CodeConstraints = Collections.CreateList(new CodeConstraint() { Regex = "Regex" }),
					RequiredMethods = Collections.CreateList(new RequiredMethod() { Name = "Required" }),
					Tests = Collections.CreateList(new ClassQuestionTest() { Name = "Test" })
				});
		}
	}
}
