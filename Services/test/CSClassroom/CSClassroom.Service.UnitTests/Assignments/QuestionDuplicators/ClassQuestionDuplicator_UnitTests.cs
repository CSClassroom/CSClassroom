using System.Linq;
using CSC.CSClassroom.Model.Questions;
using CSC.CSClassroom.Service.Questions.QuestionDuplicators;
using CSC.CSClassroom.Service.UnitTests.TestDoubles;
using CSC.CSClassroom.Service.UnitTests.Utilities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Questions.QuestionDuplicators
{
	/// <summary>
	/// Unit tests for the ClassQuestionDuplicator class.
	/// </summary>
	public class ClassQuestionDuplicator_UnitTests
	{
		/// <summary>
		/// Ensures that the question tests are duplicated.
		/// </summary>
		[Fact]
		public void DuplicateQuestionAsync_DuplicatesTests()
		{
			var database = GetDatabase().Build();
			var question = database.Context.ClassQuestions
				.Include(q => q.Tests)
				.First();

			var duplicator = new ClassQuestionDuplicator(database.Context, question);
			var result = (ClassQuestion)duplicator.DuplicateQuestion();

			Assert.Equal(1, result.Tests.Count);
			Assert.True(question.Tests[0] != result.Tests[0]);
			Assert.Equal("Test", result.Tests[0].Name);
			Assert.Equal(0, result.Tests[0].Id);
		}

		/// <summary>
		/// Ensures that the required methods are duplicated.
		/// </summary>
		[Fact]
		public void DuplicateQuestionAsync_DuplicatesRequiredMethods()
		{
			var database = GetDatabase().Build();
			var question = database.Context.ClassQuestions
				.Include(q => q.RequiredMethods)
				.First();

			var duplicator = new ClassQuestionDuplicator(database.Context, question);
			var result = (ClassQuestion)duplicator.DuplicateQuestion();

			Assert.Equal(1, result.RequiredMethods.Count);
			Assert.True(question.RequiredMethods[0] != result.RequiredMethods[0]);
			Assert.Equal("Required", result.RequiredMethods[0].Name);
			Assert.Equal(0, result.RequiredMethods[0].Id);
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
					RequiredMethods = Collections.CreateList(new RequiredMethod() { Name = "Required" }),
					Tests = Collections.CreateList(new ClassQuestionTest() { Name = "Test" })
				});
		}
	}
}
