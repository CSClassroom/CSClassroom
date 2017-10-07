using System.Linq;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Service.Assignments.QuestionDuplicators;
using CSC.CSClassroom.Service.UnitTests.TestDoubles;
using CSC.CSClassroom.Service.UnitTests.Utilities;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Assignments.QuestionDuplicators
{
	/// <summary>
	/// Unit tests for the CodeQuestionDuplicator class.
	/// </summary>
	public class CodeQuestionDuplicator_UnitTests
	{
		/// <summary>
		/// Ensures that imported classes are duplicated.
		/// </summary>
		[Fact]
		public void DuplicateQuestionAsync_DuplicatesImportedClasses()
		{
			var database = GetDatabase().Build();
			var question = database.Context.ClassQuestions
				.Include(q => q.ImportedClasses)
				.First();

			var duplicator = GetCodeQuestionDuplicator(database, question);
			var result = (CodeQuestion)duplicator.DuplicateQuestion();

			Assert.Single(result.ImportedClasses);
			Assert.True(question.ImportedClasses[0] != result.ImportedClasses[0]);
			Assert.Equal("Imported", result.ImportedClasses[0].ClassName);
			Assert.Equal(0, result.ImportedClasses[0].Id);
		}

		/// <summary>
		/// Ensures that code constraints are duplicated.
		/// </summary>
		[Fact]
		public void DuplicateQuestionAsync_DuplicatesCodeConstraints()
		{
			var database = GetDatabase().Build();
			var question = database.Context.ClassQuestions
				.Include(q => q.CodeConstraints)
				.First();

			var duplicator = GetCodeQuestionDuplicator(database, question);
			var result = (CodeQuestion)duplicator.DuplicateQuestion();

			Assert.Single(result.CodeConstraints);
			Assert.True(question.CodeConstraints[0] != result.CodeConstraints[0]);
			Assert.Equal("Regex", result.CodeConstraints[0].Regex);
			Assert.Equal(0, result.CodeConstraints[0].Id);
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
					CodeConstraints = Collections.CreateList(new CodeConstraint() { Regex = "Regex" })
				});
		}

		/// <summary>
		/// Creates a new mock question duplicator.
		/// </summary>
		private static CodeQuestionDuplicator<CodeQuestion> GetCodeQuestionDuplicator(
			TestDatabase database, 
			CodeQuestion question)
		{
			return new Mock<CodeQuestionDuplicator<CodeQuestion>>(database.Context, question).Object;
		}
	}
}
