using System.Linq;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Service.Assignments.QuestionDuplicators;
using CSC.CSClassroom.Service.UnitTests.TestDoubles;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Assignments.QuestionDuplicators
{
	/// <summary>
	/// Unit tests for the QuestionDuplicator base class.
	/// </summary>
	public class QuestionDuplicator_UnitTests
	{
		/// <summary>
		/// Ensures that imported classes are duplicated.
		/// </summary>
		[Fact]
		public void DuplicateQuestionAsync_DuplicatesQuestion()
		{
			var database = GetDatabase().Build();
			var question = database.Context
				.ClassQuestions
				.Include(q => q.QuestionCategory)
				.Single(q => q.Name == "Question2");

			var duplicator = GetCodeQuestionDuplicator(database, question);
			var result = (ClassQuestion)duplicator.DuplicateQuestion();
			
			Assert.True(question != result);
			Assert.Equal("Question2", result.Name);
			Assert.Equal("ClassName", result.ClassName);
			Assert.Equal(question.QuestionCategory, result.QuestionCategory);
		}

		/// <summary>
		/// Builds a database.
		/// </summary>
		private static TestDatabaseBuilder GetDatabase()
		{
			return new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddQuestionCategory("Class1", "Category1")
				.AddQuestion("Class1", "Category1", 
					new ClassQuestion() { Name = "Question1" })
				.AddQuestion("Class1", "Category1", 
					new ClassQuestion() { Name = "Question2", ClassName = "ClassName" })
				.AddPrerequisiteQuestion("Class1", "Category1", "Question1",
					"Category1", "Question2");
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
