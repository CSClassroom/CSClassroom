using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSC.CSClassroom.Model.Questions;
using CSC.CSClassroom.Model.Questions.ServiceResults;
using CSC.CSClassroom.Service.Questions.QuestionSolvers;
using CSC.CSClassroom.Service.UnitTests.TestDoubles;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Questions.QuestionSolving
{
	/// <summary>
	/// Unit tests for the UnsolvedPrereqRetriever class.
	/// </summary>
	public class AssignmentProgressRetriever_UnitTests
	{
		/// <summary>
		/// Ensures that GetAssignmentProgressAsync correctly returns the previous 
		/// question ID when the current question is not the first question.
		/// </summary>
		[Fact]
		public async Task GetAssignmentProgressAsync_NotFirstQuestion_ReturnsPreviousQuestion()
		{
			var database = GetDatabase().Build();
			var userId = database.Context.Users.First().Id;

			var curQuestion = database.Context
				.AssignmentQuestions
				.Single(aq => aq.Question.Name == "Question2");

			var previousQuestion = database.Context
				.AssignmentQuestions
				.Single(aq => aq.Question.Name == "Question1");

			database.Reload();

			var assignmentProgressRetriever = new AssignmentProgressRetriever
			(
				database.Context
			);

			var result = await assignmentProgressRetriever.GetAssignmentProgressAsync
			(
				curQuestion.AssignmentId,
				curQuestion.Id,
				userId
			);

			Assert.Equal(previousQuestion.Id, result.PreviousAssignmentQuestionId);
		}

		/// <summary>
		/// Ensures that GetAssignmentProgressAsync correctly returns no
		/// previous question ID when the current question is the first question.
		/// </summary>
		[Fact]
		public async Task GetAssignmentProgressAsync_FirstQuestion_ReturnsNoPreviousQuestion()
		{
			var database = GetDatabase().Build();
			var userId = database.Context.Users.First().Id;
			var curQuestion = database.Context
				.AssignmentQuestions
				.Single(aq => aq.Question.Name == "Question1");

			database.Reload();

			var assignmentProgressRetriever = new AssignmentProgressRetriever
			(
				database.Context
			);

			var result = await assignmentProgressRetriever.GetAssignmentProgressAsync
			(
				curQuestion.AssignmentId,
				curQuestion.Id,
				userId
			);

			Assert.Null(result.PreviousAssignmentQuestionId);
		}

		/// <summary>
		/// Ensures that GetAssignmentProgressAsync correctly returns the next 
		/// question ID when the current question is not the last question.
		/// </summary>
		[Fact]
		public async Task GetAssignmentProgressAsync_NotLastQuestion_ReturnsNextQuestion()
		{
			var database = GetDatabase().Build();
			var userId = database.Context.Users.First().Id;

			var curQuestion = database.Context
				.AssignmentQuestions
				.Single(aq => aq.Question.Name == "Question4");

			var nextQuestion = database.Context
				.AssignmentQuestions
				.Single(aq => aq.Question.Name == "Question5");

			database.Reload();

			var assignmentProgressRetriever = new AssignmentProgressRetriever
			(
				database.Context
			);

			var result = await assignmentProgressRetriever.GetAssignmentProgressAsync
			(
				curQuestion.AssignmentId,
				curQuestion.Id,
				userId
			);

			Assert.Equal(nextQuestion.Id, result.NextAssignmentQuestionId);
		}

		/// <summary>
		/// Ensures that GetAssignmentProgressAsync correctly returns no
		/// next question ID when the current question is the last question.
		/// </summary>
		[Fact]
		public async Task GetAssignmentProgressAsync_LastQuestion_ReturnsNoNextQuestion()
		{
			var database = GetDatabase().Build();
			var userId = database.Context.Users.First().Id;
			var curQuestion = database.Context
				.AssignmentQuestions
				.Single(aq => aq.Question.Name == "Question5");

			database.Reload();

			var assignmentProgressRetriever = new AssignmentProgressRetriever
			(
				database.Context
			);

			var result = await assignmentProgressRetriever.GetAssignmentProgressAsync
			(
				curQuestion.AssignmentId,
				curQuestion.Id,
				userId
			);

			Assert.Null(result.NextAssignmentQuestionId);
		}

		/// <summary>
		/// Ensures that GetAssignmentProgressAsync correctly returns the question
		/// progress for all questions in the assignment.
		/// </summary>
		[Fact]
		public async Task GetAssignmentProgressAsync_ReturnsQuestionProgress()
		{
			var database = GetDatabase().Build();
			var userId = database.Context.Users.First().Id;
			var curQuestion = database.Context
				.AssignmentQuestions
				.Single(aq => aq.Question.Name == "Question2");

			var allQuestions = database.Context
				.AssignmentQuestions
				.OrderBy(aq => aq.Name)
				.ToList();

			database.Reload();

			var assignmentProgressRetriever = new AssignmentProgressRetriever
			(
				database.Context
			);

			var result = await assignmentProgressRetriever.GetAssignmentProgressAsync
			(
				curQuestion.AssignmentId,
				curQuestion.Id,
				userId
			);
			
			Assert.Equal(curQuestion.Id, result.CurrentAssignmentQuestionId);
			Assert.Equal(5, result.Questions.Count);

			var progress = result.Questions;

			Assert.Equal(allQuestions[0].Id, progress[0].AssignmentQuestionId);
			Assert.Equal("Question1", progress[0].AssignmentQuestionName);
			Assert.Equal(QuestionCompletion.Completed, progress[0].Completion);

			Assert.Equal(allQuestions[1].Id, progress[1].AssignmentQuestionId);
			Assert.Equal("Question2", progress[1].AssignmentQuestionName);
			Assert.Equal(QuestionCompletion.PartiallyCompleted, progress[1].Completion);

			Assert.Equal(allQuestions[2].Id, progress[2].AssignmentQuestionId);
			Assert.Equal("Question3", progress[2].AssignmentQuestionName);
			Assert.Equal(QuestionCompletion.NotCompleted, progress[2].Completion);

			Assert.Equal(allQuestions[3].Id, progress[3].AssignmentQuestionId);
			Assert.Equal("Question4", progress[3].AssignmentQuestionName);
			Assert.Equal(QuestionCompletion.NotCompleted, progress[3].Completion);

			Assert.Equal(allQuestions[4].Id, progress[4].AssignmentQuestionId);
			Assert.Equal("Question5", progress[4].AssignmentQuestionName);
			Assert.Equal(QuestionCompletion.NotCompleted, progress[4].Completion);
		}

		/// <summary>
		/// Ensures that the result of GetAssignmentProgressAsync has the
		/// correct set of unsolved prior questions.
		/// </summary>
		[Fact]
		public async Task GetAssignmentProgressAsync_ReturnsUnsolvedPriorQuestions()
		{
			var database = GetDatabase().Build();
			var userId = database.Context.Users.First().Id;
			var assignmentQuestion = database.Context
				.AssignmentQuestions
				.Single(aq => aq.Question.Name == "Question4");

			database.Reload();

			var assignmentProgressRetriever = new AssignmentProgressRetriever
			(
				database.Context
			);

			var result = await assignmentProgressRetriever.GetAssignmentProgressAsync
			(
				assignmentQuestion.AssignmentId,
				assignmentQuestion.Id,
				userId
			);

			var unsolvedPriorQuestions = result.GetUnsolvedPriorQuestions();

			Assert.Equal(2, unsolvedPriorQuestions.Count);
			Assert.Equal("Question2", unsolvedPriorQuestions[0].AssignmentQuestionName);
			Assert.Equal("Question3", unsolvedPriorQuestions[1].AssignmentQuestionName);
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
				.AddQuestionCategory("Class1", "Category1")
				.AddQuestion("Class1", "Category1", new MethodQuestion() { Name = "Question1" })
				.AddQuestion("Class1", "Category1", new MethodQuestion() { Name = "Question2" })
				.AddQuestion("Class1", "Category1", new MethodQuestion() { Name = "Question3" })
				.AddQuestion("Class1", "Category1", new MethodQuestion() { Name = "Question4" })
				.AddQuestion("Class1", "Category1", new MethodQuestion() { Name = "Question5" })
				.AddAssignment
				(
					"Class1",
					"Unit 1",
					"Unit 1a",
					sectionDueDates: new Dictionary<string, DateTime>()
					{
						["Period1"] = DateTime.MaxValue
					},
					questionsByCategory: new Dictionary<string, string[]>()
					{
						["Category1"] = new[]
						{
							"Question1",
							"Question2",
							"Question3",
							"Question4",
							"Question5"
						}
					}
				)
				.AddQuestionSubmission
				(
					"Class1", 
					"Category1", 
					"Question1", 
					"User1", 
					"Unit 1a", 
					"Contents", 
					score: 1.0
				)
				.AddQuestionSubmission
				(
					"Class1",
					"Category1",
					"Question2",
					"User1",
					"Unit 1a",
					"Contents",
					score: 0.5
				);
		}
	}
}
