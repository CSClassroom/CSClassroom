using System;
using System.Linq;
using System.Threading.Tasks;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Service.Assignments;
using CSC.CSClassroom.Service.UnitTests.TestDoubles;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Assignments
{
	/// <summary>
	/// Unit tests for the question category service.
	/// </summary>
	public class QuestionCategoryService_UnitTests
	{
		/// <summary>
		/// Ensures that GetQuestionCategoriesAsync returns only categories
		/// for a given classroom.
		/// </summary>
		[Fact]
		public async Task GetQuestionCategoriesAsync_OnlyForClassroom()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddClassroom("Class2")
				.AddQuestionCategory("Class1", "Category1")
				.AddQuestionCategory("Class1", "Category2")
				.AddQuestionCategory("Class2", "Category1")
				.Build();

			var questionCategoryService = new QuestionCategoryService(database.Context);
			var categories = await questionCategoryService.GetQuestionCategoriesAsync("Class1");

			Assert.Equal(2, categories.Count);
			Assert.Equal("Class1", categories[0].Classroom.Name);
			Assert.Equal("Category1", categories[0].Name);
			Assert.Equal("Class1", categories[0].Classroom.Name);
			Assert.Equal("Category2", categories[1].Name);
		}

		/// <summary>
		/// Ensures that GetQuestionCategoryAsync returns the desired
		/// category, if it exists.
		/// </summary>
		[Fact]
		public async Task GetQuestionCategoryAsync_Exists_ReturnCategory()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddQuestionCategory("Class1", "Category1")
				.Build();

			var questionCategoryId = database.Context.QuestionCategories.First().Id;

			database.Reload();

			var questionCategoryService = new QuestionCategoryService(database.Context);
			var category = await questionCategoryService.GetQuestionCategoryAsync
			(
				"Class1", 
				questionCategoryId
			);

			Assert.Equal("Class1", category.Classroom.Name);
			Assert.Equal("Category1", category.Name);
		}

		/// <summary>
		/// Ensures that GetQuestionCategoryAsync returns null, if the desired
		/// category doesn't exist.
		/// </summary>
		[Fact]
		public async Task GetQuestionCategoryAsync_DoesntExist_ReturnNull()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.Build();
			
			var questionCategoryService = new QuestionCategoryService(database.Context);
			var category = await questionCategoryService.GetQuestionCategoryAsync
			(
				"Class1",
				id: 1
			);

			Assert.Null(category);
		}

		/// <summary>
		/// Ensures that CreateQuestionCategoryAsync actually creates the category.
		/// </summary>
		[Fact]
		public async Task CreateQuestionCategoryAsync_CategoryCreated()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.Build();
			
			var questionCategoryService = new QuestionCategoryService(database.Context);
			await questionCategoryService.CreateQuestionCategoryAsync
			(
				"Class1",
				new QuestionCategory()
				{
					Name = "Category1"
				}
			);

			database.Reload();

			var questionCategory = database.Context.QuestionCategories
				.Include(qc => qc.Classroom)
				.Single();

			Assert.Equal("Class1", questionCategory.Classroom.Name);
			Assert.Equal("Category1", questionCategory.Name);
		}

		/// <summary>
		/// Ensures that UpdateQuestionCategoryAsync throws if the choices
		/// category is changed.
		/// </summary>
		[Fact]
		public async Task UpdateQuestionCategoryAsync_IsChoicesCategory_Throws()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddQuestionCategory("Class1", "Category1")
				.AddQuestion
				(
					"Class1",
					"Category1", 
					new RandomlySelectedQuestion()
					{
						ChoicesCategory = new QuestionCategory()
						{
							Name = "Randomly Selected Question Category"
						}
					}
				).Build();

			var questionCategory = database.Context.QuestionCategories
				.Include(qc => qc.Classroom)
				.Single(qc => qc.RandomlySelectedQuestionId.HasValue);

			questionCategory.Classroom = database.Context.Classrooms.First();
			database.Context.SaveChanges();

			// Update the category
			database.Context.Entry(questionCategory).State = EntityState.Detached;
			questionCategory.Name = "New Category Name";

			// Apply the update
			var questionCategoryService = new QuestionCategoryService(database.Context);

			await Assert.ThrowsAsync<InvalidOperationException>
			(			
				async () => await questionCategoryService.UpdateQuestionCategoryAsync
				(
					"Class1",
					questionCategory
				)
			);
		}

		/// <summary>
		/// Ensures that UpdateQuestionCategoryAsync actually updates the category.
		/// </summary>
		[Fact]
		public async Task UpdateQuestionCategoryAsync_CategoryUpdated()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddQuestionCategory("Class1", "Category1")
				.Build();
			
			var questionCategory = database.Context.QuestionCategories
				.Include(qc => qc.Classroom)
				.First();

			// Update the category
			database.Context.Entry(questionCategory).State = EntityState.Detached;
			questionCategory.Name = "Category1Updated";

			// Apply the update
			var questionCategoryService = new QuestionCategoryService(database.Context);
			await questionCategoryService.UpdateQuestionCategoryAsync
			(
				"Class1",
				questionCategory
			);

			database.Reload();

			questionCategory = database.Context.QuestionCategories
				.Include(qc => qc.Classroom)
				.Single();

			Assert.Equal("Class1", questionCategory.Classroom.Name);
			Assert.Equal("Category1Updated", questionCategory.Name);
		}

		/// <summary>
		/// Ensures that DeleteQuestionCategoryAsync actually deletes a category.
		/// </summary>
		[Fact]
		public async Task DeleteQuestionCategoryAsync_CategoryDeleted()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddQuestionCategory("Class1", "Category1")
				.Build();
			
			var questionCategoryId = database.Context.QuestionCategories.First().Id;

			database.Reload();

			var questionCategoryService = new QuestionCategoryService(database.Context);
			await questionCategoryService.DeleteQuestionCategoryAsync
			(
				"Class1",
				questionCategoryId
			);

			database.Reload();
			
			Assert.Equal(0, database.Context.QuestionCategories.Count());
		}
	}
}
