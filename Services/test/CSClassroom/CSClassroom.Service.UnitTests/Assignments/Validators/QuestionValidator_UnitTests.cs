using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Service.Assignments.Validators;
using CSC.CSClassroom.Service.UnitTests.TestDoubles;
using CSC.CSClassroom.Service.UnitTests.Utilities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Assignments.Validators
{
	/// <summary>
	/// Unit tests for the QuestionValidator class.
	/// </summary>
	public class QuestionValidator_UnitTests
	{
		/// <summary>
		/// Ensures that ValidateQuestionAsync throws if the new question category
		/// of the question is in a different classroom.
		/// </summary>
		[Fact]
		public async Task ValidateQuestionAsync_QuestionCategoryInDifferentClassroom_Throws()
		{
			var database = GetDatabase().Build();

			var question = database.Context
				.Questions
				.Include(q => q.QuestionCategory.Classroom)
				.Single(q => q.Name == "Question1");

			var newCategoryId = database.Context
				.QuestionCategories
				.Single(qc => qc.Name == "Category2")
				.Id;

			database.Reload();

			question.QuestionCategoryId = newCategoryId;
			question.QuestionCategory = null;

			var modelErrors = new MockErrorCollection();
			var validator = new QuestionValidator(database.Context);

			await Assert.ThrowsAsync<InvalidOperationException>
			(
				async () => await validator.ValidateQuestionAsync
				(
					question,
					modelErrors,
					"Class1"
				)
			);
		}

		/// <summary>
		/// Ensures that ValidateQuestionAsync throws if the category was changed
		/// on a choice for a randomly selected question.
		/// </summary>
		[Fact]
		public async Task ValidateQuestionAsync_CategoryChangedForRandomQuestionChoice_Throws()
		{
			var database = GetDatabase().Build();

			var question = database.Context
				.Questions
				.Include(q => q.QuestionCategory.Classroom)
				.Single(q => q.Name == "Choice1");

			var newCategoryId = database.Context
				.QuestionCategories
				.Single(qc => qc.Name == "Category1")
				.Id;

			database.Reload();

			question.QuestionCategoryId = newCategoryId;
			question.QuestionCategory = null;

			var modelErrors = new MockErrorCollection();
			var validator = new QuestionValidator(database.Context);

			await Assert.ThrowsAsync<InvalidOperationException>
			(
				async () => await validator.ValidateQuestionAsync
				(
					question,
					modelErrors,
					"Class1"
				)
			);
		}

		/// <summary>
		/// Ensures that ValidateQuestionAsync throws if the category of any non-choice
		/// question is changed to a randomzied question choice category.
		/// </summary>
		[Fact]
		public async Task ValidateQuestionAsync_ChoiceCategoryForNonChoiceQuestion_Throws()
		{
			var database = GetDatabase().Build();

			var question = database.Context
				.Questions
				.Include(q => q.QuestionCategory.Classroom)
				.Single(q => q.Name == "Question1");

			var newCategoryId = database.Context
				.QuestionCategories
				.Single(qc => qc.Name == "ChoiceCategory")
				.Id;

			database.Reload();

			question.QuestionCategoryId = newCategoryId;
			question.QuestionCategory = null;

			var modelErrors = new MockErrorCollection();
			var validator = new QuestionValidator(database.Context);

			await Assert.ThrowsAsync<InvalidOperationException>
			(
				async () => await validator.ValidateQuestionAsync
				(
					question,
					modelErrors,
					"Class1"
				)
			);
		}

		/// <summary>
		/// Ensures that ValidateQuestionAsync returns false if the question's name
		/// is changed to the name of an existing question.
		/// </summary>
		[Fact]
		public async Task ValidateQuestionAsync_NameConflict_ReturnsFalseWithError()
		{
			var database = GetDatabase().Build();

			var question = new MultipleChoiceQuestion()
			{
				Name = "Question1",
				QuestionCategoryId = database.Context
					.QuestionCategories
					.Single(qc => qc.Name == "Category1")
					.Id
			};

			database.Reload();

			var modelErrors = new MockErrorCollection();
			var validator = new QuestionValidator(database.Context);
			var result = await validator.ValidateQuestionAsync
			(
				question,
				modelErrors,
				"Class1"
			);

			Assert.False(result);
			Assert.True(modelErrors.VerifyErrors("Name"));
		}

		/// <summary>
		/// Ensures that ValidateQuestionAsync returns true if a newly-created
		/// question is valid.
		/// </summary>
		[Fact]
		public async Task ValidateQuestionAsync_NewValidQuestion_ReturnsTrueNoErrors()
		{
			var database = GetDatabase().Build();

			var question = new MultipleChoiceQuestion()
			{
				Name = "NewQuestion",
				QuestionCategoryId = database.Context
					.QuestionCategories
					.Single(qc => qc.Name == "Category1")
					.Id
			};

			database.Reload();

			var modelErrors = new MockErrorCollection();
			var validator = new QuestionValidator(database.Context);

			var result = await validator.ValidateQuestionAsync
			(
				question,
				modelErrors,
				"Class1"
			);

			Assert.True(result);
			Assert.False(modelErrors.HasErrors);
		}
		
		/// <summary>
		/// Ensures that ValidateQuestionAsync returns true if a modified question
		/// is valid.
		/// </summary>
		[Fact]
		public async Task ValidateQuestionAsync_ModifiedValidQuestion_ReturnsTrueNoErrors()
		{
			var database = GetDatabase().Build();

			var question = database.Context
				.Questions
				.Include(q => q.QuestionCategory.Classroom)
				.Single(q => q.Name == "Question1");

			database.Reload();

			question.Name = "ModifiedQuestion";

			var modelErrors = new MockErrorCollection();
			var validator = new QuestionValidator(database.Context);

			var result = await validator.ValidateQuestionAsync
			(
				question,
				modelErrors,
				"Class1"
			);

			Assert.True(result);
			Assert.False(modelErrors.HasErrors);
		}

		/// <summary>
		/// Builds a database.
		/// </summary>
		private static TestDatabaseBuilder GetDatabase()
		{
			return new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddClassroom("Class2")
				.AddQuestionCategory("Class1", "Category1")
				.AddQuestionCategory("Class2", "Category2")
				.AddQuestion("Class1", "Category1", new MultipleChoiceQuestion()
				{
					Name = "Question1"
				})
				.AddQuestion("Class1", "Category1", new MultipleChoiceQuestion()
				{
					Name = "Question2"
				})
				.AddQuestion("Class1", "Category1", new RandomlySelectedQuestion()
				{
					Name = "Question3",
					ChoicesCategory = new QuestionCategory()
					{
						Name = "ChoiceCategory",
						Questions = Collections.CreateList
						(
							new MultipleChoiceQuestion() { Name = "Choice1" }	
						).Cast<Question>().ToList()
					}
				});
		}
	}
}
