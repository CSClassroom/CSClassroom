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
	/// Unit tests for the ClassQuestionUpdater class.
	/// </summary>
	public class ClassQuestionUpdater_UnitTests
	{
		/// <summary>
		/// Ensures that required methods are updated.
		/// </summary>
		[Fact]
		public async Task UpdateQuestionAsync_UpdatesRequiredMethods()
		{
			var database = GetDatabase().Build();
			var question = database.Context.ClassQuestions
				.Include(q => q.RequiredMethods)
				.Include(q => q.Tests)
				.First();

			database.Reload();

			question.RequiredMethods.Clear();
			question.RequiredMethods.Add(new RequiredMethod() { Name = "NewRequired" });

			var errors = new MockErrorCollection();
			var updater = new ClassQuestionUpdater(database.Context, question, errors);
			await updater.UpdateQuestionAsync();

			Assert.False(errors.HasErrors);
			Assert.Equal(1, question.RequiredMethods.Count);
			Assert.Equal("NewRequired", question.RequiredMethods[0].Name);
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

			question.Tests.Clear();
			question.Tests.Add(new ClassQuestionTest() { Name = "NewTest1", ExpectedOutput = "Line1\r\nLine2" });
			question.Tests.Add(new ClassQuestionTest() { Name = "NewTest2", ExpectedOutput = "Line1\r\nLine2" });

			var errors = new MockErrorCollection();
			var loader = new ClassQuestionUpdater(database.Context, question, errors);
			await loader.UpdateQuestionAsync();

			Assert.False(errors.HasErrors);
			Assert.Equal(2, question.Tests.Count);
			Assert.Equal("NewTest1", question.Tests[0].Name);
			Assert.Equal(0, question.Tests[0].Order);
			Assert.Equal("Line1\nLine2", question.Tests[0].ExpectedOutput);
			Assert.Equal("NewTest2", question.Tests[1].Name);
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
			var loader = new ClassQuestionUpdater(database.Context, question, errors);
			await loader.UpdateQuestionAsync();

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
		/// Ensures that an error is returned when an attempt is made
		/// to remove all tests.
		/// </summary>
		[Fact]
		public async Task UpdateQuestionAsync_IncorrectTemplate_Error()
		{
			var database = GetDatabase().Build();
			var question = database.Context.ClassQuestions
				.Include(q => q.Tests)
				.First();

			database.Reload();
			question.FileTemplate = "Missing Submission Expression";

			var errors = new MockErrorCollection();
			var loader = new ClassQuestionUpdater(database.Context, question, errors);
			await loader.UpdateQuestionAsync();

			Assert.True(errors.HasErrors);
			Assert.True(errors.VerifyErrors("FileTemplate"));

			database.Reload();
			question = database.Context.ClassQuestions.First();

			Assert.Equal("%SUBMISSION%", question.FileTemplate);
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
					FileTemplate = "%SUBMISSION%",
					ImportedClasses = Collections.CreateList(new ImportedClass() { ClassName = "Imported"}),
					CodeConstraints = Collections.CreateList(new CodeConstraint() { Regex = "Regex" }),
					RequiredMethods = Collections.CreateList(new RequiredMethod() { Name = "Required" }),
					Tests = Collections.CreateList(new ClassQuestionTest() { Name = "Test" })
				});
		}
	}
}
