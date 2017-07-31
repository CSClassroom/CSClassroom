using System;
using System.Linq;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.System;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Service.Assignments.QuestionGeneration;
using CSC.CSClassroom.Service.Assignments.QuestionUpdaters;
using CSC.CSClassroom.Service.UnitTests.TestDoubles;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Assignments.QuestionUpdaters
{
	/// <summary>
	/// Unit tests for the RandomlySelectedQuestionUpdater class.
	/// </summary>
	public class RandomlySelectedQuestionUpdater_UnitTests
	{
		/// <summary>
		/// Ensures that a question category is created to hold the choices
		/// for a newly created random question. 
		/// </summary>
		[Fact]
		public async Task UpdateQuestionAsync_NewQuestion_CreatesChoicesCategory()
		{
			var database = GetDatabase().Build();
			var classroom = database.Context.Classrooms.First();
			var questionCategoryId = database.Context
				.QuestionCategories
				.First()
				.Id;

			var errors = new MockErrorCollection();
			var newQuestion = new RandomlySelectedQuestion()
			{
				Name = "Random Question",
				QuestionCategoryId = questionCategoryId
			};

			var updater = new RandomlySelectedQuestionUpdater
			(
				database.Context,
				newQuestion, 
				errors
			);

			await updater.UpdateQuestionAsync();

			Assert.False(errors.HasErrors);
			Assert.NotNull(newQuestion.ChoicesCategory);
			Assert.Equal(classroom.Id, newQuestion.ChoicesCategory.ClassroomId);
			Assert.Equal("Category1: Random Question", newQuestion.ChoicesCategory.Name);
		}

		/// <summary>
		/// Ensures that a question category is not created when updating
		/// an already-existing question.
		/// </summary>
		[Fact]
		public async Task UpdateQuestionAsync_ExistingQuestion_NoNewCategoryCreated()
		{
			var database = GetDatabase()
				.AddQuestion("Class1", "Category1", new RandomlySelectedQuestion())
				.Build();

			var existingQuestion = database.Context
				.Questions
				.OfType<RandomlySelectedQuestion>()
				.First();

			database.Reload();

			var errors = new MockErrorCollection();
			var updater = new RandomlySelectedQuestionUpdater
			(
				database.Context,
				existingQuestion,
				errors
			);

			await updater.UpdateQuestionAsync();

			Assert.False(errors.HasErrors);
			Assert.Null(existingQuestion.ChoicesCategory);
			Assert.Equal(1, database.Context.QuestionCategories.Count());
		}

		/// <summary>
		/// Builds a database.
		/// </summary>
		private static TestDatabaseBuilder GetDatabase()
		{
			return new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddQuestionCategory("Class1", "Category1");
		}
	}
}
