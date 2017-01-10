using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.Utilities;
using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Model.Questions;
using CSC.CSClassroom.Model.Questions.ServiceResults;
using CSC.CSClassroom.Model.Users;
using CSC.CSClassroom.Repository;
using CSC.CSClassroom.Service.Questions;
using CSC.CSClassroom.Service.Questions.AssignmentScoring;
using CSC.CSClassroom.Service.UnitTests.TestDoubles;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Questions
{
	/// <summary>
	/// Unit tests for the assignment service.
	/// </summary>
	public class AssignmentService_UnitTests
	{
		/// <summary>
		/// Ensures that GetAssignmentsAsync returns only assignments
		/// for a given classroom.
		/// </summary>
		[Fact]
		public async Task GetAssignmentsAsync_ReturnAssignments()
		{
			var database = GetDatabaseWithAssignments().Build();

			var assignmentService = new AssignmentService(database.Context, null /*resultGenerator*/);
			var assignments = await assignmentService.GetAssignmentsAsync("Class1");

			Assert.Equal(2, assignments.Count);
			Assert.Equal("Class1", assignments[0].Classroom.Name);
			Assert.Equal("Unit 1a", assignments[0].Name);
			Assert.Equal("Class1", assignments[0].Classroom.Name);
			Assert.Equal("Unit 1b", assignments[1].Name);
		}

		/// <summary>
		/// Ensures that GetAssignmentAsync returns the desired
		/// assignment, if it exists.
		/// </summary>
		[Fact]
		public async Task GetAssignmentAsync_Exists_ReturnAssignment()
		{
			var database = GetDatabaseWithAssignments().Build();

			var assignmentId = database.Context.Assignments.First().Id;

			database.Reload();

			var assignmentService = GetAssignmentService(database.Context);
			var assignment = await assignmentService.GetAssignmentAsync
			(
				"Class1",
				assignmentId
			);

			Assert.Equal("Class1", assignment.Classroom.Name);
			Assert.Equal("Unit 1a", assignment.Name);
		}

		/// <summary>
		/// Ensures that GetAssignmentAsync returns null, if the desired
		/// assignment doesn't exist.
		/// </summary>
		[Fact]
		public async Task GetAssignmentAsync_DoesntExist_ReturnNull()
		{
			var database = GetDatabase().Build();

			var assignmentService = GetAssignmentService(database.Context);
			var assignment = await assignmentService.GetAssignmentAsync
			(
				"Class1",
				id: 1
			);

			Assert.Null(assignment);
		}

		/// <summary>
		/// Ensures that CreateAssignmentAsync actually creates an assignment.
		/// </summary>
		[Fact]
		public async Task CreateAssignmentAsync_NoErrors_AssignmentCreated()
		{
			var database = GetDatabase().Build();

			var sectionId = database.Context.Sections.First().Id;
			var questionId = database.Context.Questions.First().Id;

			database.Reload();
			
			var assignmentService = GetAssignmentService(database.Context);
			var modelErrors = new Mock<IModelErrorCollection>();
			var result = await assignmentService.CreateAssignmentAsync
			(
				"Class1",
				CreateNewAssignment(sectionId, questionId, duplicateDueDates: false),
				modelErrors.Object
			);

			Assert.True(result);
			modelErrors.Verify(e => e.AddError(It.IsAny<string>(), It.IsAny<string>()), Times.Never);

			database.Reload();

			var assignment = database.Context.Assignments
				.Include(a => a.Classroom)
				.Include(a => a.Questions)
				.Include(a => a.DueDates)
				.Single();

			Assert.Equal("Class1", assignment.Classroom.Name);
			Assert.Equal("Unit 1a", assignment.Name);
			Assert.Equal("Unit 1", assignment.GroupName);
			Assert.Equal(1, assignment.DueDates.Count);
			Assert.Equal(sectionId, assignment.DueDates.First().SectionId);
			Assert.Equal(DateTime.MaxValue, assignment.DueDates.First().DueDate);
			Assert.Equal(1, assignment.Questions.Count);
			Assert.Equal(questionId, assignment.Questions.First().QuestionId);
			Assert.Equal(1.0, assignment.Questions.First().Points);
		}

		/// <summary>
		/// Ensures that CreateAssignmentAsync does not create an assignment
		/// when there are errors.
		/// </summary>
		[Fact]
		public async Task CreateAssignmentAsync_DuplicateDueDates_AssignmentNotCreated()
		{
			var database = GetDatabase().Build();

			var sectionId = database.Context.Sections.First().Id;
			var questionId = database.Context.Questions.First().Id;

			database.Reload();

			var assignmentService = GetAssignmentService(database.Context);
			var modelErrors = new Mock<IModelErrorCollection>();
			var result = await assignmentService.CreateAssignmentAsync
			(
				"Class1",
				CreateNewAssignment(sectionId, questionId, duplicateDueDates: true),
				modelErrors.Object
			);

			Assert.False(result);
			modelErrors.Verify(e => e.AddError("DueDates", It.IsAny<string>()), Times.Once);

			database.Reload();

			var assignment = database.Context.Assignments.SingleOrDefault();

			Assert.Null(assignment);
		}

		/// <summary>
		/// Ensures that UpdateAssignmentAsync actually updates the assignment.
		/// </summary>
		[Fact]
		public async Task UpdateAssignmentAsync_NoErrors_AssignmentUpdated()
		{
			var database = GetDatabaseWithAssignments().Build();

			var assignment = database.Context.Assignments
				.Include(a => a.Classroom)
				.Include(a => a.Questions)
				.Include(a => a.DueDates)
				.First();

			var sectionIds = assignment.DueDates.Select(d => d.SectionId).ToList();
			var questionIds = assignment.Questions.Select(aq => aq.QuestionId).ToList();

			database.Reload();

			assignment.GroupName = "Updated Group Name";
			assignment.DueDates.Remove(assignment.DueDates.First());
			assignment.Questions.Remove(assignment.Questions.First());
			assignment.Questions = assignment.Questions.Reverse().ToList();

			var assignmentService = GetAssignmentService(database.Context);
			var modelErrors = new Mock<IModelErrorCollection>();
			var result = await assignmentService.UpdateAssignmentAsync
			(
				"Class1",
				assignment,
				modelErrors.Object
			);

			Assert.True(result);
			modelErrors.Verify(e => e.AddError(It.IsAny<string>(), It.IsAny<string>()), Times.Never);

			database.Reload();

			assignment = database.Context.Assignments
				.Include(a => a.Classroom)
				.Include(a => a.Questions)
				.Include(a => a.DueDates)
				.Where(a => a.Id == assignment.Id)
				.Single();

			Assert.Equal("Class1", assignment.Classroom.Name);
			Assert.Equal("Unit 1a", assignment.Name);
			Assert.Equal("Updated Group Name", assignment.GroupName);
			Assert.Equal(1, assignment.DueDates.Count);
			Assert.Equal(sectionIds[1], assignment.DueDates.Single().SectionId);
			Assert.Equal(AssignmentDueDate, assignment.DueDates.Single().DueDate);
			Assert.Equal(2, assignment.Questions.Count);
			Assert.Equal(questionIds[2], assignment.Questions.OrderBy(q => q.Order).First().QuestionId);
			Assert.Equal(questionIds[1], assignment.Questions.OrderBy(q => q.Order).Last().QuestionId);
		}

		/// <summary>
		/// Ensures that UpdateAssignmentAsync does not update the assignment,
		/// when there are duplicate due dates for a single section.
		/// </summary>
		[Fact]
		public async Task UpdateAssignmentAsync_DuplicateDueDates_AssignmentNotUpdated()
		{
			var database = GetDatabaseWithAssignments().Build();

			var assignment = database.Context.Assignments
				.Include(a => a.Classroom)
				.Include(a => a.Questions)
				.Include(a => a.DueDates)
				.First();

			var sectionId = assignment.DueDates.First().SectionId;

			database.Reload();

			assignment.GroupName = "Updated Group Name";
			assignment.DueDates.Add
			(
				new AssignmentDueDate()
				{
					SectionId = sectionId,
					DueDate = DateTime.MinValue
				}
			);

			var assignmentService = GetAssignmentService(database.Context);
			var modelErrors = new Mock<IModelErrorCollection>();
			var result = await assignmentService.UpdateAssignmentAsync
			(
				"Class1",
				assignment,
				modelErrors.Object
			);

			Assert.False(result);
			modelErrors.Verify(e => e.AddError("DueDates", It.IsAny<string>()), Times.Once);

			database.Reload();

			assignment = database.Context.Assignments
				.Include(a => a.Classroom)
				.Include(a => a.Questions)
				.Include(a => a.DueDates)
				.Where(a => a.Id == assignment.Id)
				.Single();

			Assert.Equal("Class1", assignment.Classroom.Name);
			Assert.Equal("Unit 1a", assignment.Name);
			Assert.Equal("Unit 1", assignment.GroupName);
			Assert.Equal(2, assignment.DueDates.Count);
			Assert.Equal(3, assignment.Questions.Count);
		}

		/// <summary>
		/// Ensures that DeleteAssignmentAsync actually deletes an assignment.
		/// </summary>
		[Fact]
		public async Task DeleteAssignmentAsync_AssignmentDeleted()
		{
			var database = GetDatabaseWithAssignments().Build();

			var assignmentId = database.Context.Assignments.First().Id;

			database.Reload();

			var assignmentService = GetAssignmentService(database.Context);
			await assignmentService.DeleteAssignmentAsync
			(
				"Class1",
				assignmentId
			);

			database.Reload();

			Assert.Equal(1, database.Context.Assignments.Count());
		}

		/// <summary>
		/// Ensures that GetSectionAssignmentResultsAsync returns the correct results.
		/// </summary>
		[Fact]
		public async Task GetSectionAssignmentResultsAsync_ReturnsResults()
		{
			var database = GetDatabaseWithSubmissions().Build();

			database.Reload();

			var expectedResults = new SectionAssignmentResults(null, null, 0.0, null);
			var assignmentScoreCalculator = new Mock<IAssignmentScoreCalculator>();
			assignmentScoreCalculator
				.Setup
				(
					asc => asc.GetSectionAssignmentResults
					(
						"Unit 1",
						It.Is<IList<Assignment>>(assignments => ValidateAssignments(assignments)),
						It.Is<Section>(section => section.Name == "Period1"),
						It.Is<IList<User>>(users => ValidateSectionUsers(users)),
						It.Is<IList<UserQuestionSubmission>>(submissions => ValidateSectionSubmissions(submissions))
					)
				).Returns(expectedResults);

			var assignmentService = GetAssignmentService
			(
				database.Context, 
				assignmentScoreCalculator.Object
			);

			var actualResults = await assignmentService.GetSectionAssignmentResultsAsync
			(
				"Class1",
				"Period1",
				"Unit 1"
			);

			Assert.Equal(expectedResults, actualResults);
		}
		
		/// <summary>
		/// Ensures that GetStudentAssignmentResultsAsync returns the correct results.
		/// </summary>
		[Fact]
		public async Task GetStudentAssignmentResultsAsync_ReturnsResults()
		{
			var database = GetDatabaseWithSubmissions().Build();

			var userId = database.Context.Users.First().Id;

			database.Reload();

			var expectedResults = new StudentAssignmentResults(null, null, null, null);
			var assignmentScoreCalculator = new Mock<IAssignmentScoreCalculator>();
			assignmentScoreCalculator
				.Setup
				(
					asc => asc.GetStudentAssignmentResults
					(
						It.Is<User>(user => user.UserName == "User1"),
						It.Is<Section>(section => section.Name == "Period1"),
						It.Is<IList<Assignment>>(assignments => ValidateAssignments(assignments)),
						It.Is<IList<UserQuestionSubmission>>(submissions => ValidateStudentSubmissions(submissions))
					)
				).Returns(expectedResults);

			var assignmentService = GetAssignmentService
			(
				database.Context,
				assignmentScoreCalculator.Object
			);
			
			var actualResults = await assignmentService.GetStudentAssignmentResultsAsync
			(
				"Class1",
				userId
			);

			Assert.Equal(expectedResults, actualResults);
		}

		/// <summary>
		/// Ensures that GetUpdatedAssignmentResultsAsync returns the correct results.
		/// </summary>
		[Fact]
		public async Task GetUpdatedAssignmentResultsAsync_ReturnsResults()
		{
			var database = GetDatabaseWithSubmissions().Build();

			database.Reload();

			var expectedResults = new UpdatedSectionAssignmentResults(null, null, DateTime.MinValue, null);
			var assignmentScoreCalculator = new Mock<IAssignmentScoreCalculator>();
			assignmentScoreCalculator
				.Setup
				(
					asc => asc.GetUpdatedAssignmentResults
					(
						It.Is<IList<Assignment>>(assignments => ValidateAssignments(assignments)),
						It.Is<IList<User>>(users => ValidateSectionUsers(users)),
						It.Is<Section>(section => section.Name == "Period1"),
						"Gradebook1",
						AssignmentDueDate,
						It.Is<IList<UserQuestionSubmission>>(submissions => ValidateSectionSubmissions(submissions))
					)
				).Returns(expectedResults);

			var assignmentService = GetAssignmentService
			(
				database.Context,
				assignmentScoreCalculator.Object
			);

			var actualResults = await assignmentService.GetUpdatedAssignmentResultsAsync
			(
				"Class1",
				"Period1",
				"Gradebook1"
			);

			Assert.Equal(expectedResults, actualResults);
		}

		/// <summary>
		/// Ensures that MarkAssignmentsAsGradedAsync updates the last transfer date
		/// for the given section.
		/// </summary>
		[Fact]
		public async Task MarkAssignmentsAsGradedAsync_UpdatesLastTransferDate()
		{
			var database = GetDatabaseWithSubmissions().Build();

			database.Reload();

			var assignmentService = GetAssignmentService(database.Context);

			await assignmentService.MarkAssignmentsGradedAsync
			(
				"Class1",
				"Period1",
				"Gradebook1",
				AssignmentDueDate + TimeSpan.FromDays(1)
			);

			database.Reload();

			var gradebook = database.Context.SectionGradebooks.First();

			Assert.Equal(AssignmentDueDate + TimeSpan.FromDays(1), gradebook.LastTransferDate);
		}

		/// <summary>
		/// Builds a database with assignments.
		/// </summary>
		/// <returns></returns>
		private static TestDatabaseBuilder GetDatabase()
		{
			return new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddSection("Class1", "Period1")
				.AddSection("Class1", "Period2")
				.AddGradebook("Class1", "Gradebook1", "Period1", AssignmentDueDate)
				.AddStudent("User1", "Last1", "First1", "Class1", "Period1")
				.AddStudent("User2", "Last2", "First2", "Class1", "Period1")
				.AddStudent("User3", "Last3", "First3", "Class1", "Period2")
				.AddQuestionCategory("Class1", "Category1")
				.AddQuestion("Class1", "Category1", new MethodQuestion() { Name = "Question1" })
				.AddQuestion("Class1", "Category1", new MethodQuestion() { Name = "Question2" })
				.AddQuestion("Class1", "Category1", new MethodQuestion() { Name = "Question3" })
				.AddQuestion("Class1", "Category1", new MethodQuestion() { Name = "Question4" });
		}

		/// <summary>
		/// Returns a database builder with pre-added assignments.
		/// </summary>
		private static TestDatabaseBuilder GetDatabaseWithAssignments()
		{
			return GetDatabase()
				.AddAssignment(
					"Class1",
					"Unit 1",
					"Unit 1a",
					new Dictionary<string, DateTime>()
					{
						["Period1"] = AssignmentDueDate,
						["Period2"] = AssignmentDueDate
					},
					new Dictionary<string, string[]>()
					{
						["Category1"] = new[] 
						{
							"Question1",
							"Question2",
							"Question3"
						}
					})
				.AddAssignment(
					"Class1",
					"Unit 1",
					"Unit 1b",
					new Dictionary<string, DateTime>()
					{
						["Period1"] = AssignmentDueDate,
						["Period2"] = AssignmentDueDate
					},
					new Dictionary<string, string[]>()
					{
						["Category1"] = new[] { "Question4" }
					});
		}

		/// <summary>
		/// Returns a database builder with pre-added assignments with submissions.
		/// </summary>
		private static TestDatabaseBuilder GetDatabaseWithSubmissions()
		{
			return GetDatabaseWithAssignments()
				.AddQuestionSubmission("Class1", "Category1", "Question1", "User1", "PS", 0.0, AssignmentDueDate - TimeSpan.FromDays(1))
				.AddQuestionSubmission("Class1", "Category1", "Question1", "User1", "PS", 1.0, AssignmentDueDate - TimeSpan.FromDays(1))
				.AddQuestionSubmission("Class1", "Category1", "Question1", "User1", "PS", 1.0, AssignmentDueDate + TimeSpan.FromHours(1))
				.AddQuestionSubmission("Class1", "Category1", "Question2", "User1", "PS", 1.0, AssignmentDueDate + TimeSpan.FromHours(50))
				.AddQuestionSubmission("Class1", "Category1", "Question3", "User1", "PS", 1.0, AssignmentDueDate - TimeSpan.FromHours(50))
				.AddQuestionSubmission("Class1", "Category1", "Question4", "User1", "PS", 1.0, AssignmentDueDate + TimeSpan.FromHours(50))
				.AddQuestionSubmission("Class1", "Category1", "Question1", "User2", "PS", 0.0, AssignmentDueDate - TimeSpan.FromDays(1))
				.AddQuestionSubmission("Class1", "Category1", "Question1", "User2", "PS", 1.0, AssignmentDueDate - TimeSpan.FromDays(1))
				.AddQuestionSubmission("Class1", "Category1", "Question3", "User2", "PS", 0.0, AssignmentDueDate + TimeSpan.FromDays(1))
				.AddQuestionSubmission("Class1", "Category1", "Question1", "User3", "PS", 0.0, AssignmentDueDate - TimeSpan.FromDays(1))
				.AddQuestionSubmission("Class1", "Category1", "Question1", "User3", "PS", 1.0, AssignmentDueDate - TimeSpan.FromDays(1));
		}

		/// <summary>
		/// Returns a new assignments service.
		/// </summary>
		private static AssignmentService GetAssignmentService(
			DatabaseContext context,
			IAssignmentScoreCalculator assignmentScoreCalculator = null)
		{
			return new AssignmentService(context, assignmentScoreCalculator);
		}

		/// <summary>
		/// Creates a new assignment.
		/// </summary>
		private static Assignment CreateNewAssignment(
			int sectionId, 
			int questionId, 
			bool duplicateDueDates)
		{
			var dueDates = new List<AssignmentDueDate>();
			dueDates.Add
			(
				new AssignmentDueDate()
				{
					SectionId = sectionId,
					DueDate = DateTime.MaxValue
				}
			);

			if (duplicateDueDates)
			{
				dueDates.Add
				(
					new AssignmentDueDate()
					{
						SectionId = sectionId,
						DueDate = DateTime.MinValue
					}
				);
			}

			return new Assignment()
			{
				GroupName = "Unit 1",
				Name = "Unit 1a",
				DueDates = dueDates,
				Questions = new List<AssignmentQuestion>()
				{
					new AssignmentQuestion()
					{
						QuestionId = questionId,
						Points = 1.0,
					}
				}
			};
		}

		/// <summary>
		/// Validates that the retrieved submissions match our expectations.
		/// </summary>
		private bool ValidateSectionSubmissions(IList<UserQuestionSubmission> submissions)
		{
			return ValidateSubmission(submissions[0], "Question1", "User1", 0.0, AssignmentDueDate - TimeSpan.FromDays(1))
				&& ValidateSubmission(submissions[1], "Question1", "User1", 1.0, AssignmentDueDate - TimeSpan.FromDays(1))
				&& ValidateSubmission(submissions[2], "Question1", "User1", 1.0, AssignmentDueDate + TimeSpan.FromHours(1))
				&& ValidateSubmission(submissions[3], "Question2", "User1", 1.0, AssignmentDueDate + TimeSpan.FromHours(50))
				&& ValidateSubmission(submissions[4], "Question3", "User1", 1.0, AssignmentDueDate - TimeSpan.FromHours(50))
				&& ValidateSubmission(submissions[5], "Question4", "User1", 1.0, AssignmentDueDate + TimeSpan.FromHours(50))
				&& ValidateSubmission(submissions[6], "Question1", "User2", 0.0, AssignmentDueDate - TimeSpan.FromDays(1))
				&& ValidateSubmission(submissions[7], "Question1", "User2", 1.0, AssignmentDueDate - TimeSpan.FromDays(1))
				&& ValidateSubmission(submissions[8], "Question3", "User2", 0.0, AssignmentDueDate + TimeSpan.FromDays(1));
		}

		/// <summary>
		/// Validates that the retrieved submissions match our expectations.
		/// </summary>
		private bool ValidateStudentSubmissions(IList<UserQuestionSubmission> submissions)
		{
			return ValidateSubmission(submissions[0], "Question1", "User1", 0.0, AssignmentDueDate - TimeSpan.FromDays(1))
				&& ValidateSubmission(submissions[1], "Question1", "User1", 1.0, AssignmentDueDate - TimeSpan.FromDays(1))
				&& ValidateSubmission(submissions[2], "Question1", "User1", 1.0, AssignmentDueDate + TimeSpan.FromHours(1))
				&& ValidateSubmission(submissions[3], "Question2", "User1", 1.0, AssignmentDueDate + TimeSpan.FromHours(50))
				&& ValidateSubmission(submissions[4], "Question3", "User1", 1.0, AssignmentDueDate - TimeSpan.FromHours(50))
				&& ValidateSubmission(submissions[5], "Question4", "User1", 1.0, AssignmentDueDate + TimeSpan.FromHours(50));
		}

		/// <summary>
		/// Validates that a complete submission was retrieved from the database.
		/// </summary>
		private bool ValidateSubmission(
			UserQuestionSubmission submission, 
			string questionName, 
			string userName,
			double score, 
			DateTime dateSubmitted)
		{
			return submission.UserQuestionData != null
				&& submission.UserQuestionData.Question != null
				&& submission.UserQuestionData.User != null
				&& submission.UserQuestionData.Question.Name == questionName
				&& submission.UserQuestionData.User.UserName == userName
				&& submission.Score == score
				&& submission.DateSubmitted == dateSubmitted;
		}

		/// <summary>
		/// Validates that the retrieved users match our expectations.
		/// </summary>
		private bool ValidateSectionUsers(IList<User> users)
		{
			return ValidateUser(users[0], "User1") 
				&& ValidateUser(users[1], "User2");
		}

		/// <summary>
		/// Validates that a user is completely retrieved from the database.
		/// </summary>
		private bool ValidateUser(User user, string userName)
		{
			return user.UserName == userName;
		}

		/// <summary>
		/// Validates that the retrieved assignments match our expectations.
		/// </summary>
		private bool ValidateAssignments(IList<Assignment> assignments)
		{
			return ValidateAssignment(assignments[0], "Unit 1a", "Unit 1")
				&& ValidateAssignment(assignments[1], "Unit 1b", "Unit 1");
		}

		/// <summary>
		/// Validates that an assignment is completely retrieved from the database.
		/// </summary>
		private bool ValidateAssignment(
			Assignment assignment, 
			string assignmentName, 
			string assignmentGroupName)
		{
			return assignment.Name == assignmentName
				&& assignment.GroupName == assignmentGroupName
				&& assignment.DueDates.Count > 0
				&& assignment.Questions.Count > 0
				&& assignment.Questions.All(q => q.Question != null);
		}

		/// <summary>
		/// An example question due date.
		/// </summary>
		private static DateTime AssignmentDueDate = new DateTime(2016, 1, 1, 12, 0, 0);
	}
}