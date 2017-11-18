using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.Serialization;
using CSC.Common.Infrastructure.System;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Model.Assignments.ServiceResults;
using CSC.CSClassroom.Model.Users;
using CSC.CSClassroom.Repository;
using CSC.CSClassroom.Service.Assignments.AssignmentScoring;
using CSC.CSClassroom.Service.Assignments.QuestionGraders;
using CSC.CSClassroom.Service.Assignments.QuestionResolvers;
using CSC.CSClassroom.Service.Assignments.UserQuestionDataLoaders;
using Microsoft.EntityFrameworkCore;

namespace CSC.CSClassroom.Service.Assignments.QuestionSolvers
{
	/// <summary>
	/// Functionality relating to solving questions.
	/// </summary>
	public class QuestionSolver : IQuestionSolver
	{
		/// <summary>
		/// Resolves the actual questions presented to users.
		/// </summary>
		private readonly IQuestionResolverFactory _questionResolverFactory;

		/// <summary>
		/// Creates question graders.
		/// </summary>
		private readonly IQuestionGraderFactory _questionGraderFactory;

		/// <summary>
		/// Determines the status of a question for the current user.
		/// </summary>
		private readonly IQuestionStatusCalculator _questionStatusCalculator;

		/// <summary>
		/// The question score calculator.
		/// </summary>
		private readonly IQuestionScoreCalculator _questionScoreCalculator;

		/// <summary>
		/// Retrieves the progress for a questions in an assignment.
		/// </summary>
		private readonly IAssignmentProgressRetriever _assignmentProgressRetriever;

		/// <summary>
		/// The json serializer.
		/// </summary>
		private readonly IJsonSerializer _jsonSerializer;

		/// <summary>
		/// Constructor.
		/// </summary>
		public QuestionSolver(
			IQuestionResolverFactory questionResolverFactory,
			IQuestionGraderFactory questionGraderFactory,
			IQuestionStatusCalculator questionStatusCalculator,
			IQuestionScoreCalculator questionScoreCalculator,
			IAssignmentProgressRetriever assignmentProgressRetriever,
			IJsonSerializer jsonSerializer)
		{
			_questionResolverFactory = questionResolverFactory;
			_questionGraderFactory = questionGraderFactory;
			_questionStatusCalculator = questionStatusCalculator;
			_questionScoreCalculator = questionScoreCalculator;
			_assignmentProgressRetriever = assignmentProgressRetriever;
			_jsonSerializer = jsonSerializer;
		}

		/// <summary>
		/// Returns the question with the given ID.
		/// </summary>
		public async Task<QuestionToSolve> GetQuestionToSolveAsync(
			UserQuestionDataStore userQuestionDataStore,
			int assignmentQuestionId)
		{
			var userQuestionData = userQuestionDataStore
				.GetUserQuestionData(assignmentQuestionId);

			var lastSubmission = GetLastQuestionAttempt(userQuestionData);
			var resolvedQuestion = await ResolveUnsolvedQuestionAsync(userQuestionData);

			return await GetQuestionToSolveAsync
			(
				userQuestionData, 
				resolvedQuestion, 
				lastSubmission
			);
		}

		/// <summary>
		/// Grades a question submission.
		/// </summary>
		public async Task<GradeSubmissionResult> GradeSubmissionAsync(
			UserQuestionDataStore store,
			QuestionSubmission submission,
			DateTime dateSubmitted)
		{
			var userQuestionData = store.GetUserQuestionData
			(
				submission.AssignmentQuestionId
			);
			
			var userQuestionStatus = GetUserQuestionStatus(userQuestionData);

			var resolvedQuestion = await ResolveUnsolvedQuestionAsync
			(
				userQuestionData
			);

			var scoredQuestionResult = await GradeSubmission
			(
				submission,
				resolvedQuestion,
				userQuestionData,
				userQuestionStatus,
				dateSubmitted
			);

			if (userQuestionData.AssignmentQuestion.IsInteractive())
			{
				return new GradeSubmissionResult(scoredQuestionResult);
			}
			else
			{
				return new GradeSubmissionResult(dateSubmitted);
			}
		}

		/// <summary>
		/// Returns the submission result for the given submission.
		/// </summary>
		public async Task<SubmissionResult> GetSubmissionResultAsync(
			UserQuestionDataStore store,
			int assignmentQuestionId,
			DateTime submissionDate,
			DateTime? dueDate)
		{
			var userQuestionData = store.GetUserQuestionData(assignmentQuestionId);
			var submission = userQuestionData.Submissions
				?.FirstOrDefault
				(
					s => (s.DateSubmitted - submissionDate).Duration() <
						 TimeSpan.FromMilliseconds(1)
				);

			if (submission == null)
			{
				return null;
			}

			var submissionContents = _jsonSerializer
				.Deserialize<QuestionSubmission>(submission.SubmissionContents);
			
			var resolvedQuestion = await ResolveSolvedQuestionAsync
			(
				userQuestionData,
				submission
			);

			var questionToSolve = await GetQuestionToSolveAsync
			(
				userQuestionData, 
				resolvedQuestion, 
				submissionContents
			);

			var scoredQuestionResult = await GradeQuestionAsync
			(
				resolvedQuestion,
				submissionContents
			);

			return new SubmissionResult
			(
				questionToSolve,
				scoredQuestionResult.Result,
				GetSubmissionScore(submission, dueDate, withLateness: false),
				GetSubmissionScore(submission, dueDate, withLateness: true),
				submission.UserQuestionData.AssignmentQuestion.Points,
				submission.DateSubmitted
			);
		}

		/// <summary>
		/// Returns the contents of the submission for the most recent
		/// attempt at solving the question.
		/// </summary>
		public QuestionSubmission GetLastQuestionAttempt(
			UserQuestionData userQuestionData)
		{
			if (userQuestionData.LastQuestionSubmission == null)
				return null;

			try
			{
				return _jsonSerializer
					.Deserialize<QuestionSubmission>(userQuestionData.LastQuestionSubmission);
			}
			catch
			{
				return null;
			}
		}

		/// <summary>
		/// Returns the question to solve.
		/// </summary>
		private async Task<QuestionToSolve> GetQuestionToSolveAsync(
			UserQuestionData userQuestionData,
			Question resolvedQuestion,
			QuestionSubmission lastQuestionSubmission)
		{
			var userQuestionStatus = GetUserQuestionStatus(userQuestionData);
			var assignmentProgress = await GetAssignmentProgressAsync(userQuestionData);
			var seed = GetSeedToDisplay(userQuestionData);
			var pastSubmissions = GetPastSubmissions(userQuestionData);
			
			return new QuestionToSolve
			(
				userQuestionData.AssignmentQuestionId,
				userQuestionData.AssignmentQuestion.Name,
				resolvedQuestion,
				seed,
				userQuestionData.User,
				lastQuestionSubmission,
				userQuestionData.AssignmentQuestion.IsInteractive(),
				pastSubmissions,
				userQuestionStatus,
				assignmentProgress
			);
		}

		/// <summary>
		/// Returns the seed to display.
		/// </summary>
		private static int? GetSeedToDisplay(UserQuestionData userQuestionData)
		{
			return userQuestionData.AssignmentQuestion.Question.IsQuestionTemplate
				? userQuestionData.Seed
				: null;
		}

		/// <summary>
		/// Returns a list of past submissions for a given question.
		/// </summary>
		private IList<DateTime> GetPastSubmissions(UserQuestionData userQuestionData)
		{
			if (userQuestionData.AssignmentQuestion.IsInteractive())
				return new List<DateTime>();

			return userQuestionData.Submissions
					   ?.GroupBy(s => s.DateSubmitted)
					   ?.Select(g => g.Key)
					   ?.OrderBy(d => d)
					   ?.ToList() ?? new List<DateTime>();
		}

		/// <summary>
		/// Resolves the question corresponding to the UserQuestionData object.
		/// </summary>
		private async Task<Question> ResolveUnsolvedQuestionAsync(
			UserQuestionData userQuestionData)
		{
			return await _questionResolverFactory
				.CreateQuestionResolver(userQuestionData)
				.ResolveUnsolvedQuestionAsync();
		}

		/// <summary>
		/// Resolves the question corresponding to the given UserQuestionSubmission.
		/// </summary>
		private async Task<Question> ResolveSolvedQuestionAsync(
			UserQuestionData userQuestionData,
			UserQuestionSubmission userQuestionSubmission)
		{
			return await _questionResolverFactory
				.CreateQuestionResolver(userQuestionData)
				.ResolveSolvedQuestionAsync(userQuestionSubmission);
		}

		/// <summary>
		/// Returns the status of the question for the current user.
		/// </summary>
		private UserQuestionStatus GetUserQuestionStatus(UserQuestionData uqd)
		{
			return _questionStatusCalculator.GetQuestionStatus(uqd);
		}
		
		/// <summary>
		/// Returns the progress for the assignment, for assignments that do
		/// not support combined submissions. (For assignments with combined
		/// submissions, progress information is redundant.)
		/// </summary>
		private async Task<AssignmentProgress> GetAssignmentProgressAsync(
			UserQuestionData userQuestionData)
		{
			if (userQuestionData.AssignmentQuestion.Assignment.CombinedSubmissions)
			{
				return null;
			}

			return await _assignmentProgressRetriever.GetAssignmentProgressAsync
			(
				userQuestionData.AssignmentQuestion.AssignmentId,
				userQuestionData.AssignmentQuestionId,
				userQuestionData.UserId
			);
		}

		/// <summary>
		/// Grades the submission.
		/// </summary>
		private async Task<ScoredQuestionResult> GradeSubmission(
			QuestionSubmission submission,
			Question resolvedQuestion,
			UserQuestionData userQuestionData,
			UserQuestionStatus userQuestionStatus,
			DateTime submissionDate)
		{
			if (!userQuestionStatus.AllowNewAttempt)
			{
				throw new InvalidOperationException("No attempts remaining.");
			}

			var scoredQuestionResult = await GradeQuestionAsync
			(
				resolvedQuestion, 
				submission
			);

			if (userQuestionData.Submissions == null)
			{
				userQuestionData.Submissions = new List<UserQuestionSubmission>();
			}

			var submissionContents = _jsonSerializer.Serialize(submission);
			var savedSubmission = new UserQuestionSubmission()
			{
				Score = scoredQuestionResult.Score,
				DateSubmitted = submissionDate,
				Seed = userQuestionData.Seed,
				CachedQuestionData = userQuestionData.CachedQuestionData,
				SubmissionContents = submissionContents
			};

			userQuestionData.Submissions.Add(savedSubmission);
			userQuestionData.NumAttempts++;
			userQuestionData.Seed = null;

			if (userQuestionData.AssignmentQuestion.IsInteractive())
			{
				userQuestionData.LastQuestionSubmission = submissionContents;
			}

			return scoredQuestionResult;
		}

		/// <summary>
		/// Grades the given question.
		/// </summary>
		private async Task<ScoredQuestionResult> GradeQuestionAsync(
			Question resolvedQuestion,
			QuestionSubmission submissionContents)
		{
			return await _questionGraderFactory
				.CreateQuestionGrader(resolvedQuestion)
				.GradeSubmissionAsync(submissionContents);
		}

		/// <summary>
		/// Returns the score for a submission.
		/// </summary>
		private double GetSubmissionScore(
			UserQuestionSubmission submission,
			DateTime? assignmentDueDate,
			bool withLateness)
		{
			return _questionScoreCalculator.GetSubmissionScore
			(
				submission,
				assignmentDueDate ?? DateTime.MaxValue,
				submission.UserQuestionData.AssignmentQuestion.Points,
				withLateness
			);
		}
	}
}
