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
	/// Unit tests for the assignment validator class.
	/// </summary>
	public class AssignmentValidator_UnitTests
	{
		/// <summary>
		/// Ensures that ValidateAssignmentAsync returns true when an assignment is valid.
		/// </summary>
		[Fact]
		public async Task ValidateAssignmentAsync_ValidAssignment_ReturnsTrueWithNoErrors()
		{
			var database = GetDatabase().Build();
			var modelErrors = new MockErrorCollection();
			var assignment = database.Context.Assignments.First();
			
			var assignmentValidator = new AssignmentValidator(database.Context);
			var result = await assignmentValidator.ValidateAssignmentAsync
			(
				assignment, 
				modelErrors
			);

			Assert.True(result);
			Assert.False(modelErrors.HasErrors);
		}

		/// <summary>
		/// Ensures that ValidateAssignmentAsync returns false when there are duplicate
		/// due dates for a single section.
		/// </summary>
		[Fact]
		public async Task ValidateAssignmentAsync_DuplicateDueDates_ReturnsFalseWithCorrectError()
		{
			var database = GetDatabase().Build();
			var modelErrors = new MockErrorCollection();
			var sectionId = database.Context.Sections.First().Id;
			var assignment = database.Context
				.Assignments
				.Include(a => a.DueDates)
				.Include(a => a.Questions)
				.First();

			assignment.DueDates.Add
			(
				new AssignmentDueDate()
				{
					SectionId = sectionId,
					DueDate = DateTime.MaxValue
				}
			);

			database.Reload();

			var assignmentValidator = new AssignmentValidator(database.Context);
			var result = await assignmentValidator.ValidateAssignmentAsync
			(
				assignment,
				modelErrors
			);

			Assert.False(result);
			Assert.True(modelErrors.VerifyErrors("DueDates"));
		}

		/// <summary>
		/// Ensures that ValidateAssignmentAsync returns false when the question ID for
		/// an existing assignment question was changed. This is prohibited, since user 
		/// submissions are associated with the assignment question. (Changing a question 
		/// can be accomplished by removing and adding.)
		/// </summary>
		[Fact]
		public async Task ValidateAssignmentAsync_ChangedExistingQuestionId_ReturnsFalseWithCorrectError()
		{
			var database = GetDatabase().Build();
			var modelErrors = new MockErrorCollection();
			var assignment = database.Context
				.Assignments
				.Include(a => a.DueDates)
				.Include(a => a.Questions)
				.First();

			int newQuestionId = database.Context
				.Questions
				.First(q => q.Id != assignment.Questions[0].QuestionId)
				.Id;

			assignment.Questions[0].QuestionId = newQuestionId;

			database.Reload();

			var assignmentValidator = new AssignmentValidator(database.Context);
			var result = await assignmentValidator.ValidateAssignmentAsync
			(
				assignment,
				modelErrors
			);

			Assert.False(result);
			Assert.True(modelErrors.VerifyErrors("Questions"));
		}

		/// <summary>
		/// Ensures that ValidateAssignmentAsync returns false when two questions have
		/// the same name.
		/// </summary>
		[Fact]
		public async Task ValidateAssignmentAsync_DuplicateQuestionNames_ReturnsFalseWithCorrectError()
		{
			var database = GetDatabase().Build();
			var modelErrors = new MockErrorCollection();
			var assignment = database.Context
				.Assignments
				.Include(a => a.DueDates)
				.Include(a => a.Questions)
				.First();

			int newQuestionId = database.Context
				.Questions
				.First(q => q.Id != assignment.Questions[0].QuestionId)
				.Id;

			assignment.Questions.Add
			(
				new AssignmentQuestion()
				{
					QuestionId = newQuestionId,
					Name = assignment.Questions[0].Name
				}
			);

			database.Reload();

			var assignmentValidator = new AssignmentValidator(database.Context);
			var result = await assignmentValidator.ValidateAssignmentAsync
			(
				assignment,
				modelErrors
			);

			Assert.False(result);
			Assert.True(modelErrors.VerifyErrors("Questions"));
		}

		/// <summary>
		/// Ensures that ValidateAssignmentAsync returns false when an assignment
		/// with combined submissions contains a question that only supports interactive
		/// submissions.
		/// </summary>
		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task ValidateAssignmentAsync_ContainsInteractiveOnlyQuestion_ReturnsFalseIfCombinedSubmissions(
			bool combinedSubmissions)
		{
			var database = GetDatabase().Build();
			var modelErrors = new MockErrorCollection();
			var assignment = database.Context
				.Assignments
				.Include(a => a.DueDates)
				.Include(a => a.Questions)
				.First();

			var codeQuestionId = database.Context
				.Questions
				.First(q => q is CodeQuestion)
				.Id;

			assignment.CombinedSubmissions = combinedSubmissions;
			assignment.Questions.Add
			(
				new AssignmentQuestion()
				{
					QuestionId = codeQuestionId,
					Name = "Code Question 1"
				}
			);
			
			database.Reload();

			var assignmentValidator = new AssignmentValidator(database.Context);
			var result = await assignmentValidator.ValidateAssignmentAsync
			(
				assignment,
				modelErrors
			);

			Assert.Equal(!combinedSubmissions, result);
			if (combinedSubmissions)
			{
				Assert.True(modelErrors.VerifyErrors("Questions"));
			}
		}

		/// <summary>
		/// Ensures that ValidateAssignmentAsync returns false when an assignment
		/// with combined submissions also has the AnswerInOrder option set.
		/// </summary>
		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task ValidateAssignmentAsync_AnswerInOrder_ReturnsFalseIfCombinedSubmissions(
			bool combinedSubmissions)
		{
			var database = GetDatabase().Build();
			var modelErrors = new MockErrorCollection();
			var assignment = database.Context
				.Assignments
				.Include(a => a.DueDates)
				.Include(a => a.Questions)
				.First();

			assignment.AnswerInOrder = true;
			assignment.CombinedSubmissions = combinedSubmissions;

			database.Reload();

			var assignmentValidator = new AssignmentValidator(database.Context);
			var result = await assignmentValidator.ValidateAssignmentAsync
			(
				assignment,
				modelErrors
			);

			Assert.Equal(!combinedSubmissions, result);
			if (combinedSubmissions)
			{
				Assert.True(modelErrors.VerifyErrors("AnswerInOrder"));
			}
		}

		/// <summary>
		/// Ensures that ValidateAssignmentAsync returns false when an assignment
		/// with separate submissions also has the OnlyShowCombinedScore option set.
		/// </summary>
		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task ValidateAssignmentAsync_OnlyShowCombinedScore_ReturnsFalseIfSeparateSubmissions(
			bool combinedSubmissions)
		{
			var database = GetDatabase().Build();
			var modelErrors = new MockErrorCollection();
			var assignment = database.Context
				.Assignments
				.Include(a => a.DueDates)
				.Include(a => a.Questions)
				.First();

			assignment.OnlyShowCombinedScore = true;
			assignment.CombinedSubmissions = combinedSubmissions;

			database.Reload();

			var assignmentValidator = new AssignmentValidator(database.Context);
			var result = await assignmentValidator.ValidateAssignmentAsync
			(
				assignment,
				modelErrors
			);

			Assert.Equal(combinedSubmissions, result);
			if (!combinedSubmissions)
			{
				Assert.True(modelErrors.VerifyErrors("OnlyShowCombinedScore"));
			}
		}

		/// <summary>
		/// Returns a database builder with pre-added questions and assignments.
		/// </summary>
		private static TestDatabaseBuilder GetDatabase()
		{
			return new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddSection("Class1", "Period1")
				.AddQuestionCategory("Class1", "Category1")
				.AddQuestion("Class1", "Category1", new MultipleChoiceQuestion() { Name = "Question1" })
				.AddQuestion("Class1", "Category1", new MultipleChoiceQuestion() { Name = "Question2" })
				.AddQuestion("Class1", "Category1", new MethodQuestion() { Name = "Question3" })
				.AddAssignment
				(
					"Class1",
					"Unit 1",
					"Unit 1a",
					sectionDueDates: new Dictionary<string, DateTime>()
					{
						["Period1"] = DateTime.MinValue
					},
					questionsByCategory: new Dictionary<string, string[]>()
					{
						["Category1"] = new[]
						{
							"Question1"
						}
					}
				);
		}
	}
}
