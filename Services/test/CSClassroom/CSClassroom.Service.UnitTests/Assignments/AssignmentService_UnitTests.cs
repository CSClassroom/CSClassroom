using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.Utilities;
using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Model.Assignments.ServiceResults;
using CSC.CSClassroom.Model.Users;
using CSC.CSClassroom.Repository;
using CSC.CSClassroom.Service.Assignments;
using CSC.CSClassroom.Service.Assignments.AssignmentScoring;
using CSC.CSClassroom.Service.Assignments.Validators;
using CSC.CSClassroom.Service.UnitTests.TestDoubles;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Assignments
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

			var assignmentService = GetAssignmentService(database.Context);
			var assignments = await assignmentService.GetAssignmentsAsync("Class1");

			Assert.Equal(3, assignments.Count);
			Assert.Equal("Class1", assignments[0].Classroom.Name);
			Assert.Equal("Unit 1a", assignments[0].Name);
			Assert.Equal("Class1", assignments[1].Classroom.Name);
			Assert.Equal("Unit 1b", assignments[1].Name);
			Assert.Equal("Class1", assignments[2].Classroom.Name);
			Assert.Equal("Unit 1c", assignments[2].Name);
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
			var questionIds = database.Context
				.Questions
				.Take(2)
				.Select(q => q.Id)
				.ToList();

			database.Reload();

			var newAssignment = CreateNewAssignment(sectionId, questionIds);
			var modelErrors = new MockErrorCollection();
			var assignmentValidator = CreateMockAssignmentValidator
			(
				newAssignment,
				modelErrors,
				validAssignment: true
			);

			var assignmentService = GetAssignmentService
			(
				database.Context,
				assignmentValidator
			);

			var result = await assignmentService.CreateAssignmentAsync
			(
				"Class1",
				newAssignment,
				modelErrors
			);

			Assert.True(result);
			Assert.False(modelErrors.HasErrors);

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
			Assert.Equal(2, assignment.Questions.Count);
			Assert.Equal(questionIds[0], assignment.Questions[0].QuestionId);
			Assert.Equal("Question1", assignment.Questions[0].Name);
			Assert.Equal(0, assignment.Questions[0].Order);
			Assert.Equal(questionIds[1], assignment.Questions[1].QuestionId);
			Assert.Equal("Question2", assignment.Questions[1].Name);
			Assert.Equal(1, assignment.Questions[1].Order);
			Assert.Equal(1.0, assignment.Questions.First().Points);
		}

		/// <summary>
		/// Ensures that CreateAssignmentAsync updates the group name to be
		/// the assignment name if the group name is blank.
		/// </summary>
		[Fact]
		public async Task CreateAssignmentAsync_EmptyGroupName_GroupNameIsAssignmentName()
		{
			var database = GetDatabase().Build();
			var sectionId = database.Context.Sections.First().Id;
			var questionIds = database.Context
				.Questions
				.Take(2)
				.Select(q => q.Id)
				.ToList();

			database.Reload();

			var newAssignment = CreateNewAssignment(sectionId, questionIds);
			newAssignment.GroupName = null;

			var modelErrors = new MockErrorCollection();
			var assignmentValidator = CreateMockAssignmentValidator
			(
				newAssignment,
				modelErrors,
				validAssignment: true
			);

			var assignmentService = GetAssignmentService
			(
				database.Context,
				assignmentValidator
			);

			var result = await assignmentService.CreateAssignmentAsync
			(
				"Class1",
				newAssignment,
				modelErrors
			);

			Assert.True(result);
			Assert.False(modelErrors.HasErrors);

			database.Reload();

			var assignment = database.Context.Assignments.Single();
			
			Assert.Equal("Unit 1a", assignment.GroupName);
		}

		/// <summary>
		/// Ensures that CreateAssignmentAsync does not create an assignment
		/// when there are two due dates for the same section.
		/// </summary>
		[Fact]
		public async Task CreateAssignmentAsync_HasErrors_AssignmentNotCreated()
		{
			var database = GetDatabase().Build();

			var sectionId = database.Context.Sections.First().Id;
			var questionIds = database.Context
				.Questions
				.Take(2)
				.Select(q => q.Id)
				.ToList();

			database.Reload();

			var newAssignment = CreateNewAssignment(sectionId, questionIds);
			var modelErrors = new MockErrorCollection();
			var assignmentValidator = CreateMockAssignmentValidator
			(
				newAssignment,
				modelErrors,
				validAssignment: false
			);

			var assignmentService = GetAssignmentService
			(
				database.Context,
				assignmentValidator
			);

			var result = await assignmentService.CreateAssignmentAsync
			(
				"Class1",
				newAssignment,
				modelErrors
			);

			Assert.False(result);
			Assert.True(modelErrors.VerifyErrors("Error"));

			database.Reload();

			var assignment = database.Context.Assignments.SingleOrDefault();

			Assert.Null(assignment);
		}

		/// <summary>
		/// Ensures that UpdateAssignmentAsync actually updates the assignment,
		/// when there are no errors with the changes to the assignment.
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
			var newQuestion = database.Context.Questions.Single(q => q.Name == "Question4");

			database.Reload();

			assignment.GroupName = "Updated Group Name";
			assignment.DueDates.Remove(assignment.DueDates.First());
			assignment.Questions.Remove(assignment.Questions.First());
			assignment.Questions = assignment.Questions.Reverse().ToList();
			assignment.Questions.Add
			(
				new AssignmentQuestion()
				{
					QuestionId = newQuestion.Id, 
					Points = 1.0
				}
			);

			var modelErrors = new MockErrorCollection();
			var assignmentValidator = CreateMockAssignmentValidator
			(
				assignment,
				modelErrors,
				validAssignment: true
			);

			var assignmentService = GetAssignmentService
			(
				database.Context,
				assignmentValidator
			);

			var result = await assignmentService.UpdateAssignmentAsync
			(
				"Class1",
				assignment,
				modelErrors
			);

			Assert.True(result);
			Assert.False(modelErrors.HasErrors);
			
			database.Reload();

			assignment = database.Context.Assignments
				.Include(a => a.Classroom)
				.Include(a => a.Questions)
				.Include(a => a.DueDates)
				.Single(a => a.Id == assignment.Id);

			Assert.Equal("Class1", assignment.Classroom.Name);
			Assert.Equal("Unit 1a", assignment.Name);
			Assert.Equal("Updated Group Name", assignment.GroupName);
			Assert.Equal(1, assignment.DueDates.Count);
			Assert.Equal(sectionIds[1], assignment.DueDates.Single().SectionId);
			Assert.Equal(AssignmentDueDate, assignment.DueDates.Single().DueDate);
			Assert.Equal(3, assignment.Questions.Count);
			Assert.Equal
			(
				questionIds[2], 
				assignment.Questions.OrderBy(q => q.Order).ElementAt(0).QuestionId
			);
			Assert.Equal
			(
				questionIds[1], 
				assignment.Questions.OrderBy(q => q.Order).ElementAt(1).QuestionId
			);
			Assert.Equal
			(
				newQuestion.Id, 
				assignment.Questions.OrderBy(q => q.Order).ElementAt(2).QuestionId
			);
			Assert.Equal
			(
				"Question4",
				assignment.Questions.OrderBy(q => q.Order).ElementAt(2).Name
			);
		}

		/// <summary>
		/// Ensures that UpdateAssignmentAsync updates the group name to be
		/// the assignment name if the group name is blank.
		/// </summary>
		[Fact]
		public async Task UpdateAssignmentAsync_EmptyGroupName_GroupNameIsAssignmentName()
		{
			var database = GetDatabaseWithAssignments().Build();
			var assignment = database.Context.Assignments
				.Include(a => a.Questions)
				.First();

			database.Reload();
			assignment.GroupName = null;

			var modelErrors = new MockErrorCollection();
			var assignmentValidator = CreateMockAssignmentValidator
			(
				assignment,
				modelErrors,
				validAssignment: true
			);

			var assignmentService = GetAssignmentService
			(
				database.Context,
				assignmentValidator
			);

			var result = await assignmentService.UpdateAssignmentAsync
			(
				"Class1",
				assignment,
				modelErrors
			);

			Assert.True(result);
			Assert.False(modelErrors.HasErrors);

			database.Reload();

			assignment = database.Context.Assignments
				.Single(a => a.Id == assignment.Id);

			Assert.Equal("Unit 1a", assignment.GroupName);
		}

		/// <summary>
		/// Ensures that UpdateAssignmentAsync does not update the assignment,
		/// when there are errors with the changes to the assignment.
		/// </summary>
		[Fact]
		public async Task UpdateAssignmentAsync_HasErrors_AssignmentNotUpdated()
		{
			var database = GetDatabaseWithAssignments().Build();

			var assignment = database.Context.Assignments
				.Include(a => a.Classroom)
				.Include(a => a.Questions)
				.Include(a => a.DueDates)
				.First();

			database.Reload();

			assignment.GroupName = "Updated Group Name";

			var modelErrors = new MockErrorCollection();
			var assignmentValidator = CreateMockAssignmentValidator
			(
				assignment,
				modelErrors,
				validAssignment: false
			);

			var assignmentService = GetAssignmentService
			(
				database.Context,
				assignmentValidator
			);

			var result = await assignmentService.UpdateAssignmentAsync
			(
				"Class1",
				assignment,
				modelErrors
			);

			Assert.False(result);
			Assert.True(modelErrors.VerifyErrors("Error"));

			database.Reload();

			assignment = database.Context.Assignments
				.Include(a => a.Classroom)
				.Include(a => a.Questions)
				.Include(a => a.DueDates)
				.Single(a => a.Id == assignment.Id);
			
			Assert.Equal("Unit 1", assignment.GroupName);
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

			Assert.Equal(2, database.Context.Assignments.Count());
		}

		/// <summary>
		/// Ensures that GetSectionAssignmentResultsAsync returns null when the section
		/// is not found.
		/// </summary>
		[Fact]
		public async Task GetSectionAssignmentResultsAsync_SectionNotFound_ReturnsNull()
		{
			var database = GetDatabaseWithSubmissions().Build();

			database.Reload();

			var assignmentService = GetAssignmentService(database.Context);

			var actualResults = await assignmentService.GetSectionAssignmentResultsAsync
			(
				"Class1",
				"Period10",
				"Unit 1"
			);

			Assert.Null(actualResults);
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
			var sectionAssignmentReportGenerator = new Mock<ISectionAssignmentReportGenerator>();
			sectionAssignmentReportGenerator
				.Setup
				(
					asc => asc.GetSectionAssignmentGroupResults
					(
						"Unit 1",
						It.Is<IList<Assignment>>
						(
							assignments => ValidateAssignments
							(
								assignments, 
								true /*includePrivate*/
							)
						),
						It.Is<Section>(section => section.Name == "Period1"),
						It.Is<IList<User>>(users => ValidateSectionUsers(users)),
						It.Is<IList<UserQuestionSubmission>>
						(
							submissions => ValidateSectionSubmissions(submissions)
						)
					)
				).Returns(expectedResults);

			var assignmentService = GetAssignmentService
			(
				database.Context, 
				sectionAssignmentReportGenerator: sectionAssignmentReportGenerator.Object
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
		/// Ensures that GetStudentAssignmentResultsAsync returns null for a student user
		/// that does not belong to a section.
		/// </summary>
		[Fact]
		public async Task GetStudentAssignmentResultsAsync_StudentNotInSection_ReturnsNull()
		{
			var database = GetDatabaseWithSubmissions().Build();

			var userId = database.Context
				.Users
				.SingleOrDefault(u => u.UserName == "User4")
				.Id;

			database.Reload();

			var assignmentService = GetAssignmentService(database.Context);

			var results = await assignmentService.GetStudentAssignmentResultsAsync
			(
				"Class1",
				userId,
				admin: false
			);

			Assert.Null(results);
		}

		/// <summary>
		/// Ensures that GetStudentAssignmentResultsAsync returns the correct results.
		/// </summary>
		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task GetStudentAssignmentResultsAsync_ReturnsResults(
			bool admin)
		{
			var database = GetDatabaseWithSubmissions().Build();

			var userId = database.Context.Users.First().Id;

			database.Reload();

			var expectedResults = new StudentAssignmentResults(null, null, null, null);
			var studentAssignmentReportGenerator = new Mock<IStudentAssignmentReportGenerator>();
			studentAssignmentReportGenerator
				.Setup
				(
					asc => asc.GetStudentAssignmentGroupResults
					(
						It.Is<User>(user => user.UserName == "User1"),
						It.Is<Section>
						(
							section => section.Name == "Period1"
						),
						It.Is<IList<Assignment>>
						(
							assignments => ValidateAssignments
							(
								assignments,
								admin /*includePrivate*/
							)
						),
						It.Is<IList<UserQuestionSubmission>>
						(
							submissions => ValidateStudentSubmissions(submissions)
						),
						admin
					)
				).Returns(expectedResults);

			var assignmentService = GetAssignmentService
			(
				database.Context,
				studentAssignmentReportGenerator: studentAssignmentReportGenerator.Object
			);

			var actualResults = await assignmentService.GetStudentAssignmentResultsAsync
			(
				"Class1",
				userId,
				admin
			);

			Assert.Equal(expectedResults, actualResults);
		}

		/// <summary>
		/// Ensures that GetUpdatedAssignmentResultsAsync returns null if the section
		/// is not found.
		/// </summary>
		[Fact]
		public async Task GetUpdatedAssignmentResultsAsync_SectionNotFound_ReturnsNull()
		{
			var database = GetDatabaseWithSubmissions().Build();

			database.Reload();

			var assignmentService = GetAssignmentService(database.Context);

			var results = await assignmentService.GetUpdatedAssignmentResultsAsync
			(
				"Class1",
				"Period10",
				"Gradebook1"
			);

			Assert.Null(results);
		}

		/// <summary>
		/// Ensures that GetUpdatedAssignmentResultsAsync returns the correct results.
		/// </summary>
		[Fact]
		public async Task GetUpdatedAssignmentResultsAsync_ReturnsResults()
		{
			var database = GetDatabaseWithSubmissions()
				.AddSectionGradebook("Class1", "Gradebook1", "Period1", AssignmentDueDate)
				.Build();

			database.Reload();

			var expectedResults = new UpdatedSectionAssignmentResults
			(
				null, 
				null, 
				DateTime.MinValue, 
				DateTime.MinValue, 
				null
			);

			var updatedAssignmentReportGenerator = new Mock<IUpdatedAssignmentReportGenerator>();
			updatedAssignmentReportGenerator
				.Setup
				(
					asc => asc.GetUpdatedAssignmentGroupResults
					(
						It.Is<IList<Assignment>>
						(
							assignments => ValidateAssignments
							(
								assignments, 
								true /*includePrivate*/
							)
						),
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
				updatedAssignmentReportGenerator: updatedAssignmentReportGenerator.Object
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
		/// Ensures that MarkAssignmentsAsGradedAsync throws when given an 
		/// invalid section.
		/// </summary>
		[Fact]
		public async Task MarkAssignmentsAsGradedAsync_InvalidSection_Throws()
		{
			var databaseBuilder = GetDatabaseWithSubmissions();
			var database = databaseBuilder.Build();
			var assignmentService = GetAssignmentService(database.Context);

			await Assert.ThrowsAsync<InvalidOperationException>
			(
				async () => await assignmentService.MarkAssignmentsGradedAsync
				(
					"Class1",
					"InvalidSection",
					"Gradebook1",
					AssignmentDueDate + TimeSpan.FromDays(1)
				)
			);
		}

		/// <summary>
		/// Ensures that MarkAssignmentsAsGradedAsync updates the last transfer date
		/// for the given section.
		/// </summary>
		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async Task MarkAssignmentsAsGradedAsync_ValidSection_UpdatesLastTransferDate(
			bool gradebooksAlreadyExists)
		{
			var databaseBuilder = GetDatabaseWithSubmissions();

			if (gradebooksAlreadyExists)
			{
				databaseBuilder.AddSectionGradebook
				(
					"Class1", 
					"Gradebook1",
					"Period1",
					AssignmentDueDate
				);

				databaseBuilder.AddSectionGradebook
				(
					"Class2", 
					"Gradebook1",
					"Period1",
					AssignmentDueDate
				);
			}

			var database = databaseBuilder.Build();

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
		/// Returns a database builder with pre-added questions.
		/// </summary>
		private static TestDatabaseBuilder GetDatabase()
		{
			return new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddClassroom("Class2")
				.AddSection("Class1", "Period1")
				.AddSection("Class1", "Period2")
				.AddSection("Class2", "Period1")
				.AddSection("Class2", "Period2")
				.AddClassroomGradebook("Class1", "Gradebook1")
				.AddClassroomGradebook("Class2", "Gradebook1")
				.AddStudent("User1", "Last1", "First1", "Class1", "Period1")
				.AddStudent("User2", "Last2", "First2", "Class1", "Period1")
				.AddStudent("User3", "Last3", "First3", "Class1", "Period2")
				.AddStudent("User4", "Last4", "First4", "Class1", sectionName: null)
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
				.AddAssignment
				(
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
					}
				)
				.AddAssignment
				(
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
					}
				)
				.AddAssignment
				(
					"Class1",
					"Unit 1",
					"Unit 1c",
					new Dictionary<string, DateTime>()
					{
						["Period1"] = AssignmentDueDate,
						["Period2"] = AssignmentDueDate
					},
					new Dictionary<string, string[]>()
					{
						["Category1"] = new[] { "Question4" }
					},
					isPrivate: true
				);
		}

		/// <summary>
		/// Returns a database builder with pre-added assignments with submissions.
		/// </summary>
		private static TestDatabaseBuilder GetDatabaseWithSubmissions()
		{
			return GetDatabaseWithAssignments()
				.AddQuestionSubmission("Class1", "Category1", "Question1", "User1", "Unit 1a", "PS", 0.0, AssignmentDueDate - TimeSpan.FromDays(1))
				.AddQuestionSubmission("Class1", "Category1", "Question1", "User1", "Unit 1a", "PS", 1.0, AssignmentDueDate - TimeSpan.FromDays(1))
				.AddQuestionSubmission("Class1", "Category1", "Question1", "User1", "Unit 1a", "PS", 1.0, AssignmentDueDate + TimeSpan.FromHours(1))
				.AddQuestionSubmission("Class1", "Category1", "Question2", "User1", "Unit 1a", "PS", 1.0, AssignmentDueDate + TimeSpan.FromHours(50))
				.AddQuestionSubmission("Class1", "Category1", "Question3", "User1", "Unit 1a", "PS", 1.0, AssignmentDueDate - TimeSpan.FromHours(50))
				.AddQuestionSubmission("Class1", "Category1", "Question4", "User1", "Unit 1b", "PS", 1.0, AssignmentDueDate + TimeSpan.FromHours(50))
				.AddQuestionSubmission("Class1", "Category1", "Question1", "User2", "Unit 1a", "PS", 0.0, AssignmentDueDate - TimeSpan.FromDays(1))
				.AddQuestionSubmission("Class1", "Category1", "Question1", "User2", "Unit 1a", "PS", 1.0, AssignmentDueDate - TimeSpan.FromDays(1))
				.AddQuestionSubmission("Class1", "Category1", "Question3", "User2", "Unit 1a", "PS", 0.0, AssignmentDueDate + TimeSpan.FromDays(1))
				.AddQuestionSubmission("Class1", "Category1", "Question1", "User3", "Unit 1a", "PS", 0.0, AssignmentDueDate - TimeSpan.FromDays(1))
				.AddQuestionSubmission("Class1", "Category1", "Question1", "User3", "Unit 1a", "PS", 1.0, AssignmentDueDate - TimeSpan.FromDays(1));
		}

		/// <summary>
		/// Returns a new assignment validator.
		/// </summary>
		private static IAssignmentValidator CreateMockAssignmentValidator(
			Assignment assignmentToValidate,
			IModelErrorCollection modelErrors,
			bool validAssignment)
		{
			var assignmentValidator = new Mock<IAssignmentValidator>();
			assignmentValidator
				.Setup
				(
					m => m.ValidateAssignmentAsync
					(
						assignmentToValidate, 
						modelErrors
					)
				)
				.Callback
				(
					(Assignment assignment, IModelErrorCollection errors) =>
					{
						if (!validAssignment)
						{
							errors.AddError("Error", "Error Description");
						}
					} 
				).ReturnsAsync(validAssignment);

			return assignmentValidator.Object;
		}

		/// <summary>
		/// Returns a new assignments service.
		/// </summary>
		private static AssignmentService GetAssignmentService(
			DatabaseContext context,
			IAssignmentValidator assignmentValidator = null,
			ISectionAssignmentReportGenerator sectionAssignmentReportGenerator = null,
			IStudentAssignmentReportGenerator studentAssignmentReportGenerator = null,
			IUpdatedAssignmentReportGenerator updatedAssignmentReportGenerator = null)
		{
			return new AssignmentService
			(
				context,
				assignmentValidator,
				sectionAssignmentReportGenerator,
				studentAssignmentReportGenerator,
				updatedAssignmentReportGenerator
			);
		}

		/// <summary>
		/// Creates a new assignment.
		/// </summary>
		private static Assignment CreateNewAssignment(
			int sectionId, 
			IList<int> questionIds)
		{
			return new Assignment()
			{
				GroupName = "Unit 1",
				Name = "Unit 1a",
				Questions = questionIds
					.Select
					(
						(questionId, index) => new AssignmentQuestion()
						{
							QuestionId = questionId,
							Points = 1.0,
						}
					).ToList(),
				DueDates = new List<AssignmentDueDate>()
				{
					new AssignmentDueDate()
					{
						SectionId = sectionId,
						DueDate = DateTime.MaxValue
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
				&& submission.UserQuestionData.AssignmentQuestion != null
				&& submission.UserQuestionData.User != null
				&& submission.UserQuestionData.AssignmentQuestion.Name == questionName
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
		private bool ValidateAssignments(IList<Assignment> assignments, bool includePrivate)
		{
			return assignments.Count == (includePrivate ? 3 : 2)
				&& ValidateAssignment(assignments[0], "Unit 1a", "Unit 1")
				&& ValidateAssignment(assignments[1], "Unit 1b", "Unit 1")
				&& (!includePrivate || ValidateAssignment(assignments[2], "Unit 1c", "Unit 1"));
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