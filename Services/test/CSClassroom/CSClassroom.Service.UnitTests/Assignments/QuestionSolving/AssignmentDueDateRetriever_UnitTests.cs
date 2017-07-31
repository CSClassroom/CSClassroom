using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Service.Assignments.QuestionSolvers;
using CSC.CSClassroom.Service.UnitTests.TestDoubles;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Assignments.QuestionSolving
{
	/// <summary>
	/// Unit tests for the AssignmentDueDateRetriever class.
	/// </summary>
	public class AssignmentDueDateRetriever_UnitTests
	{
		/// <summary>
		/// An example due date.
		/// </summary>
		private readonly DateTime DueDate = new DateTime(2017, 1, 2, 0, 0, 0);

		/// <summary>
		/// Ensures that GetUserAssignmentDueDateAsync returns the due date
		/// of the section the user is in.
		/// </summary>
		[Fact]
		public async Task GetUserAssignmentDueDateAsync_UserInSection_ReturnsDueDate()
		{
			var database = GetDatabase().Build();
			var assignmentId = database.Context.Assignments.First().Id;
			var userId = database.Context.Users.Single(u => u.UserName == "User1").Id;
			var dueDateRetriever = new AssignmentDueDateRetriever(database.Context);

			var result = await dueDateRetriever.GetUserAssignmentDueDateAsync
			(
				"Class1", 
				assignmentId, 
				userId
			);

			Assert.Equal(DueDate, result);
		}

		/// <summary>
		/// Ensures that GetUserAssignmentDueDateAsync returns null for a user
		/// that does not belong to a section with a due date.
		/// </summary>
		[Fact]
		public async Task GetUserAssignmentDueDateAsync_UserNotInSection_ReturnsNull()
		{
			var database = GetDatabase().Build();
			var assignmentId = database.Context.Assignments.First().Id;
			var userId = database.Context.Users.Single(u => u.UserName == "User2").Id;
			var dueDateRetriever = new AssignmentDueDateRetriever(database.Context);

			var result = await dueDateRetriever.GetUserAssignmentDueDateAsync
			(
				"Class1",
				assignmentId,
				userId
			);

			Assert.Null(result);
		}

		/// <summary>
		/// Returns a database builder with pre-added questions.
		/// </summary>
		private TestDatabaseBuilder GetDatabase()
		{
			return new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddSection("Class1", "Period1")
				.AddStudent("User1", "Last1", "First1", "Class1", "Period1")
				.AddStudent("User2", "Last2", "First2", "Class1", sectionName: null)
				.AddQuestionCategory("Class1", "Category1")
				.AddQuestion("Class1", "Category1", new MethodQuestion() { Name = "Question1" })
				.AddAssignment
				(
					"Class1",
					"Unit 1",
					"Unit 1a",
					sectionDueDates: new Dictionary<string, DateTime>()
					{
						["Period1"] = DueDate
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
