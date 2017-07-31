using System.Linq;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Service.Assignments.QuestionDuplicators;
using CSC.CSClassroom.Service.UnitTests.TestDoubles;
using CSC.CSClassroom.Service.UnitTests.Utilities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Assignments.QuestionDuplicators
{
	/// <summary>
	/// Unit tests for the MethodQuestionDuplicator class.
	/// </summary>
	public class MethodQuestionDuplicator_UnitTests
	{
		/// <summary>
		/// Ensures that the question tests are duplicated.
		/// </summary>
		[Fact]
		public void DuplicateQuestionAsync_DuplicatesTests()
		{
			var database = GetDatabase().Build();
			var question = database.Context.MethodQuestions
				.Include(q => q.Tests)
				.First();

			var duplicator = new MethodQuestionDuplicator(database.Context, question);
			var result = (MethodQuestion)duplicator.DuplicateQuestion();

			Assert.Equal(1, result.Tests.Count);
			Assert.True(question.Tests[0] != result.Tests[0]);
			Assert.Equal("Test", result.Tests[0].Name);
			Assert.Equal(0, result.Tests[0].Id);
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
					Tests = Collections.CreateList(new MethodQuestionTest() { Name = "Test" })
				});
		}
	}
}
