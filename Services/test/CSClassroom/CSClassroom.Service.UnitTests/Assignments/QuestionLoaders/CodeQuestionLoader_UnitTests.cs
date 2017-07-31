using System.Linq;
using System.Threading.Tasks;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Service.Assignments.QuestionLoaders;
using CSC.CSClassroom.Service.UnitTests.TestDoubles;
using CSC.CSClassroom.Service.UnitTests.Utilities;
using Moq;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Assignments.QuestionLoaders
{
	/// <summary>
	/// Unit tests for the CodeQuestionLoader base class.
	/// </summary>
	public class CodeQuestionLoader_UnitTests
	{
		/// <summary>
		/// Ensures that imported classes are loaded.
		/// </summary>
		[Fact]
		public async Task LoadQuestionAsync_LoadsImportedClasses()
		{
			var database = GetDatabase().Build();
			var question = database.Context.MethodQuestions.First();
			var loader = GetCodeQuestionLoader(database, question);

			await loader.LoadQuestionAsync();

			Assert.Equal(1, question.ImportedClasses.Count);
			Assert.Equal("Imported", question.ImportedClasses[0].ClassName);
		}

		/// <summary>
		/// Ensures that code constraints are loaded.
		/// </summary>
		[Fact]
		public async Task LoadQuestionAsync_LoadsCodeConstraints()
		{
			var database = GetDatabase().Build();
			var question = database.Context.MethodQuestions.First();
			var loader = GetCodeQuestionLoader(database, question);

			await loader.LoadQuestionAsync();

			Assert.Equal(1, question.CodeConstraints.Count);
			Assert.Equal("Regex", question.CodeConstraints[0].Regex);
		}

		/// <summary>
		/// Builds a database.
		/// </summary>
		private static TestDatabaseBuilder GetDatabase()
		{
			return new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddQuestionCategory("Class1", "Category1")
				.AddQuestion("Class1", "Category1", new MethodQuestion()
				{
					ImportedClasses = Collections.CreateList(new ImportedClass() { ClassName = "Imported" }),
					CodeConstraints = Collections.CreateList(new CodeConstraint() { Regex = "Regex" }),
				});
		}

		/// <summary>
		/// Creates a new code question loader.
		/// </summary>
		private static CodeQuestionLoader<CodeQuestion> GetCodeQuestionLoader(
			TestDatabase database,
			CodeQuestion question)
		{
			return new Mock<CodeQuestionLoader<CodeQuestion>>(database.Context, question).Object;
		}
	}
}
