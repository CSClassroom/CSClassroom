using System.Linq;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.Utilities;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Service.Assignments.QuestionUpdaters;
using CSC.CSClassroom.Service.UnitTests.TestDoubles;
using CSC.CSClassroom.Service.UnitTests.Utilities;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Assignments.QuestionUpdaters
{
	/// <summary>
	/// Unit tests for the CodeQuestionUpdater base class.
	/// </summary>
	public class CodeQuestionUpdater_UnitTests
	{
		/// <summary>
		/// Ensures that imported classes are updated.
		/// </summary>
		[Fact]
		public async Task UpdateQuestionAsync_UpdatesImportedClasses()
		{
			var database = GetDatabase().Build();
			var question = database.Context.ClassQuestions
				.Include(q => q.ImportedClasses)
				.Include(q => q.Tests)
				.First();

			database.Reload();

			question.ImportedClasses.Clear();
			question.ImportedClasses.Add(new ImportedClass() { ClassName = "NewImported" });

			var errors = new MockErrorCollection();
			var updater = GetCodeQuestionUpdater(database, question, errors);
			await updater.UpdateQuestionAsync();
			database.Context.Questions.Update(question);
			database.Context.SaveChanges();

			database.Reload();
			question = database.Context.ClassQuestions
				.Include(q => q.ImportedClasses)
				.Include(q => q.Tests)
				.First();

			Assert.False(errors.HasErrors);
			Assert.Single(question.ImportedClasses);
			Assert.Equal("NewImported", question.ImportedClasses[0].ClassName);
		}

		/// <summary>
		/// Ensures that code constraints are updated.
		/// </summary>
		[Fact]
		public async Task UpdateQuestionAsync_UpdatesCodeConstraints()
		{
			var database = GetDatabase().Build();
			var question = database.Context.ClassQuestions
				.Include(q => q.CodeConstraints)
				.Include(q => q.Tests)
				.First();

			database.Reload();

			question.CodeConstraints.Clear();
			question.CodeConstraints.Add(new CodeConstraint() { Regex = "NewRegex1" });
			question.CodeConstraints.Add(new CodeConstraint() { Regex = "NewRegex2" });

			var errors = new MockErrorCollection();
			var updater = GetCodeQuestionUpdater(database, question, errors);
			await updater.UpdateQuestionAsync();
			database.Context.Questions.Update(question);
			database.Context.SaveChanges();

			database.Reload();
			question = database.Context.ClassQuestions
				.Include(q => q.CodeConstraints)
				.Include(q => q.Tests)
				.First();

			Assert.False(errors.HasErrors);
			Assert.Equal(2, question.CodeConstraints.Count);
			Assert.Equal("NewRegex1", question.CodeConstraints[0].Regex);
			Assert.Equal(0, question.CodeConstraints[0].Order);
			Assert.Equal("NewRegex2", question.CodeConstraints[1].Regex);
			Assert.Equal(1, question.CodeConstraints[1].Order);
		}

		/// <summary>
		/// Ensures that class question tests are updated.
		/// </summary>
		[Fact]
		public async Task UpdateQuestionAsync_UpdatesTests_NoError()
		{
			var database = GetDatabase().Build();
			var question = database.Context.ClassQuestions
				.Include(q => q.Tests)
				.First();

			database.Reload();
			
			question.Tests.Add(new ClassQuestionTest() { Name = "NewTest", ExpectedOutput = "Line1\r\nLine2" });

			var errors = new MockErrorCollection();
			var updater = GetCodeQuestionUpdater(database, question, errors);
			await updater.UpdateQuestionAsync();
			database.Context.Questions.Update(question);
			database.Context.SaveChanges();

			database.Reload();
			question = database.Context.ClassQuestions
				.Include(q => q.ImportedClasses)
				.Include(q => q.Tests)
				.First();

			Assert.False(errors.HasErrors);
			Assert.Equal(2, question.Tests.Count);
			Assert.Equal("Test", question.Tests[0].Name);
			Assert.Equal(0, question.Tests[0].Order);
			Assert.Equal("NewTest", question.Tests[1].Name);
			Assert.Equal(1, question.Tests[1].Order);
			Assert.Equal("Line1\nLine2", question.Tests[1].ExpectedOutput);
		}

		/// <summary>
		/// Ensures that an error is returned when an attempt is made
		/// to remove all tests.
		/// </summary>
		[Fact]
		public async Task UpdateQuestionAsync_RemovesAllTests_Error()
		{
			var database = GetDatabase().Build();
			var question = database.Context.ClassQuestions
				.Include(q => q.Tests)
				.First();

			database.Reload();

			question.Tests.Clear();

			var errors = new MockErrorCollection();
			var updater = GetCodeQuestionUpdater(database, question, errors);
			await updater.UpdateQuestionAsync();
			database.Context.Questions.Update(question);
			database.Context.SaveChanges();

			Assert.True(errors.HasErrors);
			Assert.True(errors.VerifyErrors("Tests"));

			database.Reload();
			question = database.Context.ClassQuestions
				.Include(q => q.Tests)
				.First();

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
					Tests = Collections.CreateList(new ClassQuestionTest() { Name = "Test" })
				});
		}

		/// <summary>
		/// Creates a new code question updater.
		/// </summary>
		private static CodeQuestionUpdater<CodeQuestion> GetCodeQuestionUpdater(
			TestDatabase database,
			CodeQuestion question,
			IModelErrorCollection errors)
		{
			return new Mock<CodeQuestionUpdater<CodeQuestion>>(database.Context, question, errors).Object;
		}
	}
}
