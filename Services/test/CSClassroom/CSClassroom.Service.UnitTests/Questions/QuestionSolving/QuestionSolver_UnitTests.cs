using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.Serialization;
using CSC.CSClassroom.Model.Questions;
using CSC.CSClassroom.Model.Questions.ServiceResults;
using CSC.CSClassroom.Model.Users;
using CSC.CSClassroom.Service.Questions.AssignmentScoring;
using CSC.CSClassroom.Service.Questions.QuestionGraders;
using CSC.CSClassroom.Service.Questions.QuestionResolvers;
using CSC.CSClassroom.Service.Questions.QuestionSolvers;
using CSC.CSClassroom.Service.Questions.UserQuestionDataLoaders;
using CSC.CSClassroom.Service.UnitTests.Utilities;
using Moq;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Questions.QuestionSolving
{
	/// <summary>
	/// Unit tests for the QuestionSolver class.
	/// </summary>
	public class QuestionSolver_UnitTests
	{
		/// <summary>
		/// An example submission date.
		/// </summary>
		private readonly DateTime SubmissionDate = new DateTime(2017, 1, 1, 0, 0, 0);

		/// <summary>
		/// An example due date.
		/// </summary>
		private readonly DateTime DueDate = new DateTime(2017, 1, 2, 0, 0, 0);

		/// <summary>
		/// Ensures that the simple properties on the object returned by
		/// GetQuestionToSolveAsync are correct.
		/// </summary>
		[Fact]
		public async Task GetQuestionToSolveAsync_SimplePropertiesCorrect()
		{
			var user = new User();
			var store = GetUserQuestionDataStore
			(
				assignmentQuestionId: 1,
				assignmentQuestionName: "Question 1",
				user: user,
				numAttempts: 1,
				numAttemptsRemaining: 1
			);

			var resolvedQuestion = new MethodQuestion();
			var assignmentProgressRetriever = GetMockAssignmentProgressRetriever(store);
			var resolverFactory = GetMockQuestionResolverFactory
			(
				store, 
				resolvedQuestion
			);

			var questionSolver = GetQuestionSolver
			(
				questionResolverFactory: resolverFactory,
				assignmentProgressRetriever: assignmentProgressRetriever
			);

			var result = await questionSolver.GetQuestionToSolveAsync
			(
				store,
				assignmentQuestionId: 1
			);

			Assert.Equal(1, result.AssignmentQuestionId);
			Assert.Equal("Question 1", result.Name);
			Assert.Equal(resolvedQuestion, result.Question);
			Assert.Equal(user, result.User);
			Assert.Equal(1, result.NumAttempts);
			Assert.Equal(1, result.NumAttemptsRemaining);
		}

		/// <summary>
		/// Ensures that the Interactive property on the object returned by
		/// GetQuestionToSolveAsync is correct.
		/// </summary>
		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task GetQuestionToSolveAsync_ReturnsIsInteractive(
			bool isInteractive)
		{
			var store = GetUserQuestionDataStore(interactive: isInteractive);
			var assignmentProgressRetriever = GetMockAssignmentProgressRetriever(store);
			var resolverFactory = GetMockQuestionResolverFactory(store);

			var questionSolver = GetQuestionSolver
			(
				questionResolverFactory: resolverFactory,
				assignmentProgressRetriever: assignmentProgressRetriever
			);

			var result = await questionSolver.GetQuestionToSolveAsync
			(
				store,
				assignmentQuestionId: 1
			);

			Assert.Equal(isInteractive, result.Interactive);
		}

		/// <summary>
		/// Ensures that the seed property on the object returned by
		/// GetQuestionToSolveAsync is only shown if the underlying 
		/// question is a generated question template.
		/// </summary>
		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task GetQuestionToSolveAsync_ReturnsSeedIfAppropriate(
			bool generated)
		{
			var expectedSeed = generated ? 1 : (int?)null;
			var store = GetUserQuestionDataStore(generated: generated, seed: 1);
			var assignmentProgressRetriever = GetMockAssignmentProgressRetriever(store);
			var resolverFactory = GetMockQuestionResolverFactory(store);

			var questionSolver = GetQuestionSolver
			(
				questionResolverFactory: resolverFactory,
				assignmentProgressRetriever: assignmentProgressRetriever
			);

			var result = await questionSolver.GetQuestionToSolveAsync
			(
				store,
				assignmentQuestionId: 1
			);

			Assert.Equal(expectedSeed, result.Seed);
		}

		/// <summary>
		/// Ensures that the assignment progress is returned when
		/// GetQuestionToSolveAsync is called for an assignment that
		/// does not have combined submissions.
		/// </summary>
		[Fact]
		public async Task GetQuestionToSolveAsync_SeparateSubmissions_ReturnsAssignmentProgress()
		{
			var store = GetUserQuestionDataStore(interactive: true);
			var resolverFactory = GetMockQuestionResolverFactory(store);
			var assignmentProgress = new AssignmentProgress
			(
				userId: 0,
				currentAssignmentQuestionId: 0,
				questions: null
			);

			var assignmentProgressRetriever = GetMockAssignmentProgressRetriever
			(
				store, 
				assignmentProgress
			);

			var questionSolver = GetQuestionSolver
			(
				questionResolverFactory: resolverFactory,
				assignmentProgressRetriever: assignmentProgressRetriever
			);

			var result = await questionSolver.GetQuestionToSolveAsync
			(
				store,
				assignmentQuestionId: 1
			);

			Assert.Equal(assignmentProgress, result.AssignmentProgress);
		}

		/// <summary>
		/// Ensures that the assignment progress is not returned when
		/// GetQuestionToSolveAsync is called for an assignment that
		/// has combined submissions.
		/// </summary>
		[Fact]
		public async Task GetQuestionToSolveAsync_CombinedSubmissions_NoAssignmentProgress()
		{
			var store = GetUserQuestionDataStore(interactive: false);
			var resolverFactory = GetMockQuestionResolverFactory(store);
			var questionSolver = GetQuestionSolver
			(
				questionResolverFactory: resolverFactory
			);

			var result = await questionSolver.GetQuestionToSolveAsync
			(
				store,
				assignmentQuestionId: 1
			);

			Assert.Null(result.AssignmentProgress);
		}

		/// <summary>
		/// Ensures that the last submission on the object returned by
		/// GetQuestionToSolveAsync is included if it can deserialize
		/// successfully.
		/// </summary>
		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task GetQuestionToSolveAsync_ReturnsLastSubmissionIfValid(
			bool validLastSubmission)
		{
			var lastSubmission = validLastSubmission
				? CreateQuestionSubmission(assignmentQuestionId: 1)
				: null;

			var store = GetUserQuestionDataStore(lastSubmission: "LastSubmission");
			var serializer = GetMockJsonSerializer("LastSubmission", lastSubmission);
			var assignmentProgressRetriever = GetMockAssignmentProgressRetriever(store);
			var resolverFactory = GetMockQuestionResolverFactory(store);

			var questionSolver = GetQuestionSolver
			(
				questionResolverFactory: resolverFactory,
				assignmentProgressRetriever: assignmentProgressRetriever,
				jsonSerializer: serializer
			);

			var result = await questionSolver.GetQuestionToSolveAsync
			(
				store,
				assignmentQuestionId: 1
			);

			Assert.Equal(lastSubmission, result.LastSubmission);
		}

		/// <summary>
		/// Ensures that any past submissions are only returned if the question is
		/// non-interactive.
		/// </summary>
		[Theory]
		[InlineData(true, true)]
		[InlineData(false, true)]
		[InlineData(true, false)]
		[InlineData(false, false)]
		public async Task GetQuestionToSolveAsync_ReturnsPastSubmissionsIfNonInteractive(
			bool pastSubmissionsExist,
			bool isInteractive)
		{
			var expectedPastSubmissions = pastSubmissionsExist && !isInteractive
				? Collections.CreateList(DateTime.MinValue, DateTime.MaxValue)
				: new List<DateTime>();

			var actualPastSubmissions = pastSubmissionsExist
				? Collections.CreateList
					(
						DateTime.MinValue,
						DateTime.MaxValue,
						DateTime.MaxValue,
						DateTime.MinValue
					)
				: null;

			var store = GetUserQuestionDataStore
			(
				interactive: isInteractive,
				pastSubmissions: GetPastSubmissions(actualPastSubmissions)
			);
			
			var assignmentProgressRetriever = GetMockAssignmentProgressRetriever(store);
			var resolverFactory = GetMockQuestionResolverFactory(store);

			var questionSolver = GetQuestionSolver
			(
				questionResolverFactory: resolverFactory,
				assignmentProgressRetriever: assignmentProgressRetriever
			);

			var result = await questionSolver.GetQuestionToSolveAsync
			(
				store,
				assignmentQuestionId: 1
			);

			Assert.True(result.PastSubmissions.SequenceEqual(expectedPastSubmissions));
		}

		/// <summary>
		/// Ensures that GradeSubmissionAsync throws if no attempts are remaining.
		/// </summary>
		[Fact]
		public async Task GradeSubmissionAsync_NoAttemptsRemaining_Throws()
		{
			var store = GetUserQuestionDataStore
			(
				numAttempts: 1,
				numAttemptsRemaining: 0
			);

			var submission = CreateQuestionSubmission(assignmentQuestionId: 1);
			var resolverFactory = GetMockQuestionResolverFactory(store);
			var questionSolver = GetQuestionSolver(resolverFactory);

			await Assert.ThrowsAsync<InvalidOperationException>
			(
				async () => await questionSolver.GradeSubmissionAsync
				(
					store,
					submission,
					SubmissionDate
				)
			);
		}

		/// <summary>
		/// Ensures that GradeSubmissionAsync throws if no attempts are remaining.
		/// </summary>
		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task GradeSubmissionAsync_AttemptsRemaining_UserQuestionDataUpdated(
			bool isInteractive)
		{
			var store = GetUserQuestionDataStore
			(
				numAttempts: 1,
				numAttemptsRemaining: 2,
				interactive: isInteractive,
				cachedQuestionData: "Cached",
				seed: 12345
			);

			var submission = CreateQuestionSubmission(assignmentQuestionId: 1);
			var resolvedQuestion = new MethodQuestion();
			var resolverFactory = GetMockQuestionResolverFactory(store, resolvedQuestion);
			var serializer = GetMockJsonSerializer("Contents", submission);
			var graderFactory = GetMockQuestionGraderFactory
			(
				resolvedQuestion,
				submission,
				new ScoredQuestionResult(result: null, score: 0.5)
			);

			var questionSolver = GetQuestionSolver
			(
				questionResolverFactory: resolverFactory, 
				questionGraderFactory: graderFactory,
				jsonSerializer: serializer
			);

			await questionSolver.GradeSubmissionAsync
			(
				store,
				submission,
				SubmissionDate
			);

			var userQuestionData = store.GetUserQuestionData
			(
				store.GetLoadedAssignmentQuestionIds()[0]
			);

			var expectedLastSubmission = isInteractive ? "Contents" : null;

			Assert.Equal(1, userQuestionData.Submissions.Count);
			Assert.Equal(0.5, userQuestionData.Submissions[0].Score);
			Assert.Equal(SubmissionDate, userQuestionData.Submissions[0].DateSubmitted);
			Assert.Equal(12345, userQuestionData.Submissions[0].Seed);
			Assert.Equal("Cached", userQuestionData.Submissions[0].CachedQuestionData);
			Assert.Equal("Contents", userQuestionData.Submissions[0].SubmissionContents);
			Assert.Equal(2, userQuestionData.NumAttempts);
			Assert.Null(userQuestionData.Seed);
			Assert.Equal(expectedLastSubmission, userQuestionData.LastQuestionSubmission);
		}

		/// <summary>
		/// Ensures that GradeSubmissionAsync returns the scored result back to the user
		/// when the question is being solved interactively.
		/// </summary>
		[Fact]
		public async Task GradeSubmissionAsync_InteractiveAttemptsRemaining_ReturnsResultWithScore()
		{
			var store = GetUserQuestionDataStore
			(
				numAttempts: 1,
				numAttemptsRemaining: 2,
				interactive: true
			);

			var submission = CreateQuestionSubmission(assignmentQuestionId: 1);
			var resolvedQuestion = new MethodQuestion();
			var scoredQuestionResult = new ScoredQuestionResult(result: null, score: 0.5);
			var resolverFactory = GetMockQuestionResolverFactory(store, resolvedQuestion);
			var serializer = GetMockJsonSerializer("Contents", submission);
			var graderFactory = GetMockQuestionGraderFactory
			(
				resolvedQuestion,
				submission,
				scoredQuestionResult
			);

			var questionSolver = GetQuestionSolver
			(
				questionResolverFactory: resolverFactory,
				questionGraderFactory: graderFactory,
				jsonSerializer: serializer
			);

			var result = await questionSolver.GradeSubmissionAsync
			(
				store,
				submission,
				SubmissionDate
			);

			Assert.Equal(scoredQuestionResult, result.ScoredQuestionResult);
			Assert.Null(result.SubmissionDate);
		}

		/// <summary>
		/// Ensures that GradeSubmissionAsync returns the submission date back to the
		/// user (omitting the result). This allows the client to navigate to the page
		/// with the submission results.
		/// </summary>
		[Fact]
		public async Task GradeSubmissionAsync_NonInteractiveAttemptsRemaining_ReturnsResultWithDate()
		{
			var store = GetUserQuestionDataStore
			(
				numAttempts: 1,
				numAttemptsRemaining: 2,
				interactive: false
			);

			var submission = CreateQuestionSubmission(assignmentQuestionId: 1);
			var resolvedQuestion = new MethodQuestion();
			var resolverFactory = GetMockQuestionResolverFactory(store, resolvedQuestion);
			var serializer = GetMockJsonSerializer("Contents", submission);
			var graderFactory = GetMockQuestionGraderFactory
			(
				resolvedQuestion,
				submission,
				new ScoredQuestionResult(result: null, score: 0.5)
			);

			var questionSolver = GetQuestionSolver
			(
				questionResolverFactory: resolverFactory,
				questionGraderFactory: graderFactory,
				jsonSerializer: serializer
			);

			var result = await questionSolver.GradeSubmissionAsync
			(
				store,
				submission,
				SubmissionDate
			);

			Assert.Null(result.ScoredQuestionResult);
			Assert.Equal(SubmissionDate, result.SubmissionDate);
		}

		/// <summary>
		/// Ensures that GetSubmissionResultAsync returns null if the given
		/// submission time is not found.
		/// </summary>
		[Fact]
		public async Task GetSubmissionResultAsync_SubmissionNotFound_ReturnsNull()
		{
			var store = GetUserQuestionDataStore();

			var questionSolver = GetQuestionSolver();

			var result = await questionSolver.GetSubmissionResultAsync
			(
				store,
				assignmentQuestionId: 1,
				submissionDate: SubmissionDate,
				dueDate: DateTime.MaxValue
			);

			Assert.Null(result);
		}

		/// <summary>
		/// Ensures that the simple properties on the object returned by
		/// GetQuestionToSolveAsync are correct.
		/// </summary>
		[Fact]
		public async Task GetSubmissionResultAsync_SubmissionFound_ReturnsCorrectResult()
		{
			var user = new User();
			var userQuestionSubmission = new UserQuestionSubmission()
			{
				DateSubmitted = SubmissionDate,
				SubmissionContents = "Contents"
			};

			var store = GetUserQuestionDataStore
			(
				assignmentQuestionId: 1,
				assignmentQuestionName: "Question 1",
				user: user,
				numAttempts: 1,
				numAttemptsRemaining: 1,
				generated: true,
				seed: 12345,
				questionPoints: 1.0,
				pastSubmissions: Collections.CreateList
				(
					userQuestionSubmission
				)
			);

			var submission = CreateQuestionSubmission(assignmentQuestionId: 1);
			var serializer = GetMockJsonSerializer("Contents", submission);
			var resolvedQuestion = new MultipleChoiceQuestion();
			var resolverFactory = GetMockQuestionResolverFactory
			(
				store, 
				resolvedQuestion, 
				submission: userQuestionSubmission
			);

			var questionResult = new MultipleChoiceQuestionResult(correct: true);
			var graderFactory = GetMockQuestionGraderFactory
			(
				resolvedQuestion,
				submission,
				new ScoredQuestionResult(questionResult, score: 0.5)
			);

			var scoreCalculator = GetMockQuestionScoreCalculator
			(
				userQuestionSubmission,
				DueDate,
				questionPoints: 1.0,
				scoreWithoutLateness: 1.0,
				scoreWithLateness: 0.9
			);

			var questionSolver = GetQuestionSolver
			(
				questionResolverFactory: resolverFactory,
				questionGraderFactory: graderFactory,
				questionScoreCalculator: scoreCalculator,
				jsonSerializer: serializer
			);

			var result = await questionSolver.GetSubmissionResultAsync
			(
				store,
				assignmentQuestionId: 1,
				submissionDate: SubmissionDate,
				dueDate: DueDate
			);

			Assert.Equal(1, result.QuestionSubmitted.AssignmentQuestionId);
			Assert.Equal("Question 1", result.QuestionSubmitted.Name);
			Assert.Equal(resolvedQuestion, result.QuestionSubmitted.Question);
			Assert.Equal(12345, result.QuestionSubmitted.Seed);
			Assert.Equal(user, result.QuestionSubmitted.User);
			Assert.Equal(submission, result.QuestionSubmitted.LastSubmission);
			Assert.Equal(false, result.QuestionSubmitted.Interactive);
			Assert.Equal(1, result.QuestionSubmitted.NumAttempts);
			Assert.Equal(1, result.QuestionSubmitted.NumAttemptsRemaining);
			Assert.Equal(1, result.QuestionSubmitted.PastSubmissions.Count);
			Assert.Equal(SubmissionDate, result.QuestionSubmitted.PastSubmissions[0]);
			Assert.Equal(null, result.QuestionSubmitted.AssignmentProgress);
			Assert.Equal(questionResult, result.QuestionResult);
			Assert.Equal(1.0, result.QuestionPoints);
			Assert.Equal(SubmissionDate, result.SubmissionDate);
			Assert.Equal(1.0, result.ScoreWithoutLateness);
			Assert.Equal(0.9, result.ScoreWithLateness);
		}

		/// <summary>
		/// Returns a list of past submissions, given a list of submission times.
		/// </summary>
		public IList<UserQuestionSubmission> GetPastSubmissions(
			IList<DateTime> submissionTimes)
		{
			return submissionTimes
				?.Select
				(
					dateTime => new UserQuestionSubmission()
					{
						DateSubmitted = dateTime
					}
				)?.ToList();
		}

		/// <summary>
		/// Returns a UserQuestionDataStore with a single question.
		/// </summary>
		private UserQuestionDataStore GetUserQuestionDataStore(
			int assignmentQuestionId = 1,
			string assignmentQuestionName = "Question",
			User user = null,
			string lastSubmission = null,
			bool interactive = false,
			bool generated = false,
			int numAttempts = 0,
			int numAttemptsRemaining = 0,
			string cachedQuestionData = null,
			int seed = 0,
			double questionPoints = 1.0,
			IList<UserQuestionSubmission> pastSubmissions = null)
		{
			var userQuestionData = new UserQuestionData()
			{
				User = user,
				AssignmentQuestionId = assignmentQuestionId,
				AssignmentQuestion = new AssignmentQuestion()
				{
					Id = assignmentQuestionId,
					Name = assignmentQuestionName,
					Assignment = new Assignment()
					{
						CombinedSubmissions = !interactive,
						MaxAttempts = numAttempts + numAttemptsRemaining
					},
					Points = questionPoints,
					Question = generated
						? (Question) new GeneratedQuestionTemplate()
						: interactive
							? (Question) new MethodQuestion()
							: (Question) new MultipleChoiceQuestion()
				},
				LastQuestionSubmission = lastSubmission,
				NumAttempts = numAttempts,
				CachedQuestionData = cachedQuestionData,
				Seed = seed,
				Submissions = pastSubmissions
			};

			if (pastSubmissions != null)
			{
				foreach (var submission in pastSubmissions)
				{
					submission.UserQuestionData = userQuestionData;
				}
			}

			return new UserQuestionDataStore
			(
				new Dictionary<int, UserQuestionData>()
				{
					[assignmentQuestionId] = userQuestionData
				}
			);
		}

		/// <summary>
		/// Creates a new question submission.
		/// </summary>
		private QuestionSubmission CreateQuestionSubmission(
			int assignmentQuestionId)
		{
			var submission = new Mock<QuestionSubmission>();

			submission.Object.AssignmentQuestionId = assignmentQuestionId;

			return submission.Object;
		}

		/// <summary>
		/// Returns a mock QuestionResolver.
		/// </summary>
		private IQuestionResolverFactory GetMockQuestionResolverFactory(
			UserQuestionDataStore store,
			Question resolvedQuestion = null,
			UserQuestionSubmission submission = null)
		{
			var questionResolverFactory = new Mock<IQuestionResolverFactory>();

			var userQuestionData = store.GetUserQuestionData
			(
				store.GetLoadedAssignmentQuestionIds()[0]
			);

			if (submission != null)
			{
				questionResolverFactory
					.Setup
					(
						m => m
							.CreateQuestionResolver(userQuestionData)
							.ResolveSolvedQuestionAsync(submission)
					).ReturnsAsync(resolvedQuestion);
			}
			else
			{

				questionResolverFactory
					.Setup
					(
						m => m
							.CreateQuestionResolver(userQuestionData)
							.ResolveUnsolvedQuestionAsync()
					).ReturnsAsync(resolvedQuestion);
			}

			return questionResolverFactory.Object;
		}

		/// <summary>
		/// Returns a mock QuestionResolver.
		/// </summary>
		private IQuestionGraderFactory GetMockQuestionGraderFactory(
			Question resolvedQuestion,
			QuestionSubmission submission,
			ScoredQuestionResult gradeResult)
		{
			var questionGraderFactory = new Mock<IQuestionGraderFactory>();

			questionGraderFactory
				.Setup
				(
					m => m
						.CreateQuestionGrader(resolvedQuestion)
						.GradeSubmissionAsync(submission)
				).ReturnsAsync(gradeResult);

			return questionGraderFactory.Object;
		}

		/// <summary>
		/// Returns a mock assignment progress retriever.
		/// </summary>
		private IAssignmentProgressRetriever GetMockAssignmentProgressRetriever(
			UserQuestionDataStore store,
			AssignmentProgress assignmentProgress = null)
		{
			var assignmentProgressRetriever = new Mock<IAssignmentProgressRetriever>();

			var uqd = store.GetUserQuestionData
			(
				store.GetLoadedAssignmentQuestionIds()[0]
			);

			assignmentProgressRetriever
				.Setup
				(
					m => m.GetAssignmentProgressAsync
					(
						uqd.AssignmentQuestion.AssignmentId,
						uqd.AssignmentQuestionId,
						uqd.UserId
					)
				).ReturnsAsync(assignmentProgress);

			return assignmentProgressRetriever.Object;
		}

		/// <summary>
		/// Returns a mock JSON serializer.
		/// </summary>
		private IJsonSerializer GetMockJsonSerializer(
			string serializedSubmission,
			QuestionSubmission submission)
		{
			var serializer = new Mock<IJsonSerializer>();

			if (submission != null)
			{
				serializer
					.Setup(s => s.Deserialize<QuestionSubmission>(serializedSubmission))
					.Returns(submission);

				serializer
					.Setup(s => s.Serialize(submission))
					.Returns(serializedSubmission);
			}
			else
			{
				serializer
					.Setup(s => s.Deserialize<QuestionSubmission>(serializedSubmission))
					.Throws(new InvalidOperationException());
			}

			return serializer.Object;
		}

		/// <summary>
		/// Returns a mock QuestionScoreCalculator.
		/// </summary>
		private IQuestionScoreCalculator GetMockQuestionScoreCalculator(
			UserQuestionSubmission submission,
			DateTime dueDate,
			double questionPoints,
			double scoreWithoutLateness,
			double scoreWithLateness)
		{
			var questionScoreCalculator = new Mock<IQuestionScoreCalculator>();

			questionScoreCalculator
				.Setup
				(
					m => m.GetSubmissionScore
					(
						submission, 
						dueDate, 
						questionPoints,
						false /*withLateness*/
					)
				).Returns(scoreWithoutLateness);

			questionScoreCalculator
				.Setup
				(
					m => m.GetSubmissionScore
					(
						submission,
						dueDate,
						questionPoints,
						true /*withLateness*/
					)
				).Returns(scoreWithLateness);


			return questionScoreCalculator.Object;
		}

		/// <summary>
		/// Creates a question solver.
		/// </summary>
		private QuestionSolver GetQuestionSolver(
			IQuestionResolverFactory questionResolverFactory = null,
			IQuestionGraderFactory questionGraderFactory = null,
			IQuestionScoreCalculator questionScoreCalculator = null,
			IAssignmentProgressRetriever assignmentProgressRetriever = null,
			IJsonSerializer jsonSerializer = null)
		{
			return new QuestionSolver
			(
				questionResolverFactory,
				questionGraderFactory,
				questionScoreCalculator,
				assignmentProgressRetriever,
				jsonSerializer
			);
		}
	}
}
