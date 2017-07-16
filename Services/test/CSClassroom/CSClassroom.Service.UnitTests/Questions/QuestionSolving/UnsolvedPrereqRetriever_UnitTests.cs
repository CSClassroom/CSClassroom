using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSC.CSClassroom.Model.Questions;
using CSC.CSClassroom.Service.Questions.QuestionSolvers;
using CSC.CSClassroom.Service.UnitTests.TestDoubles;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Questions.QuestionSolving
{
	/// <summary>
	/// Unit tests for the UnsolvedPrereqRetriever class.
	/// </summary>
	public class UnsolvedPrereqRetriever_UnitTests
	{
		/// <summary>
		/// Ensures that GetUnsolvedPrereqsAsync returns unsolved prereqs, when the
		/// assignment requires questions to be successfully answered in order.
		/// </summary>
		[Fact]
		public async Task GetUnsolvedPrereqsAsync_AnswerInOrder_ReturnsUnsolvedPrereqs()
		{
			var database = GetDatabase(answerInOrder: true).Build();

			var userId = database.Context
				.Users
				.First()
				.Id;

			var assignmentQuestions = database.Context
				.AssignmentQuestions
				.Include(aq => aq.Assignment.Questions)
				.Include(aq => aq.Question)
				.ToList();

			var assignmentQuestion = assignmentQuestions
				.Single(aq => aq.Question.Name == "Question4");

			var userQuestionData = new UserQuestionData()
			{
				UserId = userId,
				AssignmentQuestion = assignmentQuestion
			};

			var unsolvedPrereqRetriever = new UnsolvedPrereqsRetriever(database.Context);

			var result = await unsolvedPrereqRetriever.GetUnsolvedPrereqsAsync
			(
				userQuestionData
			);

			Assert.Equal(2, result.Count);
			Assert.Equal("Question2", result[0].Question.Name);
			Assert.Equal("Question3", result[1].Question.Name);
		}

		/// <summary>
		/// Ensures that GetUnsolvedPrereqsAsync returns no unsolved prereqs, when the
		/// assignment does not require questions to be successfully answered in order.
		/// </summary>
		[Fact]
		public async Task GetUnsolvedPrereqsAsync_NoAnswerInOrder_ReturnsNoUnsolvedPrereqs()
		{
			var database = GetDatabase(answerInOrder: false).Build();

			var userId = database.Context
				.Users
				.First()
				.Id;

			var assignmentQuestions = database.Context
				.AssignmentQuestions
				.Include(aq => aq.Assignment.Questions)
				.Include(aq => aq.Question)
				.ToList();

			var assignmentQuestion = assignmentQuestions
				.Single(aq => aq.Question.Name == "Question4");

			var userQuestionData = new UserQuestionData()
			{
				UserId = userId,
				AssignmentQuestion = assignmentQuestion
			};

			var unsolvedPrereqRetriever = new UnsolvedPrereqsRetriever(database.Context);

			var result = await unsolvedPrereqRetriever.GetUnsolvedPrereqsAsync
			(
				userQuestionData
			);

			Assert.Equal(0, result.Count);
		}

		/// <summary>
		/// Returns a database builder with pre-added questions.
		/// </summary>
		private TestDatabaseBuilder GetDatabase(bool answerInOrder)
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
							"Question4"
						}
					},
					answerInOrder: answerInOrder
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
