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
	/// Unit tests for the MethodQuestionUpdater class.
	/// </summary>
	public class MethodQuestionUpdater_UnitTests
	{
		/// <summary>
		/// Ensures that class question tests are updated.
		/// </summary>
		[Fact]
		public async Task UpdateQuestionAsync_UpdatesTests_NoError()
		{
			var database = GetDatabase().Build();
			var question = database.Context.MethodQuestions
				.Include(q => q.Tests)
				.First();

			database.Reload();

			question.Tests.Clear();
			question.Tests.Add(new MethodQuestionTest() { Name = "NewTest1" });
			question.Tests.Add(new MethodQuestionTest() { Name = "NewTest2" });

			var errors = new MockErrorCollection();
			var updater = new MethodQuestionUpdater(database.Context, question, errors);
			await updater.UpdateQuestionAsync();
			database.Context.Questions.Update(question);
			database.Context.SaveChanges();

			database.Reload();
			question = database.Context.MethodQuestions
				.Include(q => q.Tests)
				.First();

			Assert.False(errors.HasErrors);
			Assert.Equal(2, question.Tests.Count);
			Assert.Equal("NewTest1", question.Tests[0].Name);
			Assert.Equal("NewTest2", question.Tests[1].Name);
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
					ImportedClasses = Collections.CreateList(new ImportedClass() { ClassName = "Imported"}),
					CodeConstraints = Collections.CreateList(new CodeConstraint() { Regex = "Regex" }),
					Tests = Collections.CreateList(new MethodQuestionTest() { Name = "Test" })
				});
		}
	}
}
