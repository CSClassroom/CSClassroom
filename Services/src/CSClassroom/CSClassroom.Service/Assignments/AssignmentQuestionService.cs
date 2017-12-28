using System;
using CSC.Common.Infrastructure.System;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Model.Assignments.ServiceResults;
using CSC.CSClassroom.Repository;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSC.CSClassroom.Model.Users;
using CSC.CSClassroom.Service.Assignments.QuestionSolvers;
using CSC.CSClassroom.Service.Assignments.UserQuestionDataLoaders;

namespace CSC.CSClassroom.Service.Assignments
{
	/// <summary>
	/// Performs assignment operations.
	/// </summary>
	public class AssignmentQuestionService : IAssignmentQuestionService
	{
		/// <summary>
		/// The database context.
		/// </summary>
		private readonly DatabaseContext _dbContext;

		/// <summary>
		/// Loads user question data.
		/// </summary>
		private readonly IUserQuestionDataLoaderFactory _userQuestionDataLoaderFactory;

		/// <summary>
		/// Retrieves an assignment's due date.
		/// </summary>
		private readonly IAssignmentDueDateRetriever _assignmentDueDateRetriever;

		/// <summary>
		/// The question solver.
		/// </summary>
		private readonly IQuestionSolver _questionSolver;

		/// <summary>
		/// Provides the current time.
		/// </summary>
		private readonly ITimeProvider _timeProvider;
		
		/// <summary>
		/// Constructor.
		/// </summary>
		public AssignmentQuestionService(
			DatabaseContext dbContext,
			IUserQuestionDataLoaderFactory userQuestionDataLoaderFactory,
			IAssignmentDueDateRetriever assignmentDueDateRetriever,
			IQuestionSolver questionSolver,
			ITimeProvider timeProvider)
		{
			_dbContext = dbContext;
			_userQuestionDataLoaderFactory = userQuestionDataLoaderFactory;
			_assignmentDueDateRetriever = assignmentDueDateRetriever;
			_questionSolver = questionSolver;
			_timeProvider = timeProvider;
		}

		/// <summary>
		/// Returns a list of questions for the given assignment.
		/// </summary>
		public async Task<IList<AssignmentQuestion>> GetAssignmentQuestionsAsync(
			string classroomName,
			int assignmentId)
		{
			return await _dbContext.AssignmentQuestions
				.Where(aq => aq.Assignment.Classroom.Name == classroomName)
				.Where(aq => aq.AssignmentId == assignmentId)
				.ToListAsync();
		}

		/// <summary>
		/// Returns the question with the given ID.
		/// </summary>
		public async Task<QuestionToSolve> GetQuestionToSolveAsync(
			string classroomName,
			int assignmentId,
			int assignmentQuestionId,
			int userId)
		{
			var store = await LoadDataForSingleQuestionAsync
			(
				classroomName,
				assignmentId,
				assignmentQuestionId,
				userId
			);

			var result = await _questionSolver.GetQuestionToSolveAsync
			(
				store,
				assignmentQuestionId
			);

			await _dbContext.SaveChangesAsync();

			return result;
		}

		/// <summary>
		/// Returns the question with the given ID.
		/// </summary>
		public async Task<IList<QuestionToSolve>> GetQuestionsToSolveAsync(
			string classroomName,
			int assignmentId,
			int userId)
		{
			var store = await LoadDataForAllAssignmentQuestionsAsync
			(
				classroomName,
				assignmentId,
				userId
			);

			var questionsToSolve = new List<QuestionToSolve>();
			foreach (int assignmentQuestionId in store.GetLoadedAssignmentQuestionIds())
			{
				var questionToSolve = await _questionSolver.GetQuestionToSolveAsync
				(
					store,
					assignmentQuestionId
				);

				if (questionToSolve.Status.AllowNewAttempt)
				{
					questionsToSolve.Add(questionToSolve);
				}
			}

			await _dbContext.SaveChangesAsync();

			return questionsToSolve;
		}

		/// <summary>
		/// Grades a question submission (returning and storing the result).
		/// </summary>
		public async Task<GradeSubmissionResult> GradeSubmissionAsync(
			string classroomName,
			int assignmentId,
			int userId,
			QuestionSubmission submission)
		{
			var store = await LoadDataForSingleQuestionAsync
			(
				classroomName,
				assignmentId,
				submission.AssignmentQuestionId,
				userId
			);

			var dateSubmitted = _timeProvider.UtcNow;

			var result = await _questionSolver.GradeSubmissionAsync
			(
				store,
				submission,
				dateSubmitted
			);

			await _dbContext.SaveChangesAsync();

			return result;
		}

		/// <summary>
		/// Grades question submissions.
		/// </summary>
		public async Task<GradeSubmissionsResult> GradeSubmissionsAsync(
			string classroomName,
			int assignmentId,
			int userId,
			IList<QuestionSubmission> submissions)
		{
			var store = await LoadDataForAllAssignmentQuestionsAsync
			(
				classroomName,
				assignmentId,
				userId
			);

			var dateSubmitted = _timeProvider.UtcNow;

			foreach (var submission in submissions)
			{
				await _questionSolver.GradeSubmissionAsync
				(
					store,
					submission,
					dateSubmitted
				);
			}

			await _dbContext.SaveChangesAsync();

			return new GradeSubmissionsResult(dateSubmitted);
		}

		/// <summary>
		/// Returns a user's question submission.
		/// </summary>
		public async Task<SubmissionResult> GetSubmissionAsync(
			string classroomName,
			int assignmentId,
			int assignmentQuestionId,
			int userId,
			DateTime submissionDate)
		{
			var store = await LoadDataForSingleQuestionAsync
			(
				classroomName,
				assignmentId,
				assignmentQuestionId,
				userId
			);

			var assignmentDueDate = await _assignmentDueDateRetriever
				.GetUserAssignmentDueDateAsync
				(
					classroomName,
					assignmentId,
					userId
				);

			var result = await _questionSolver
				.GetSubmissionResultAsync
				(
					store,
					assignmentQuestionId,
					submissionDate,
					assignmentDueDate
				);

			await _dbContext.SaveChangesAsync();

			return result;
		}

		/// <summary>
		/// Returns a user's question submissions.
		/// </summary>
		public async Task<IList<SubmissionResult>> GetSubmissionsAsync(
			string classroomName,
			int assignmentId,
			int userId,
			DateTime submissionDate)
		{
			var store = await LoadDataForAllAssignmentQuestionsAsync
			(
				classroomName,
				assignmentId,
				userId
			);

			var assignmentDueDate = await _assignmentDueDateRetriever
				.GetUserAssignmentDueDateAsync
				(
					classroomName,
					assignmentId,
					userId
				);

			var submissionResults = new List<SubmissionResult>();
			foreach (int assignmentQuestionId in store.GetLoadedAssignmentQuestionIds())
			{
				var submissionResult = await _questionSolver
					.GetSubmissionResultAsync
					(
						store,
						assignmentQuestionId,
						submissionDate,
						assignmentDueDate
					);

				if (submissionResult != null)
				{
					submissionResults.Add(submissionResult);
				}
			}

			await _dbContext.SaveChangesAsync();

			return submissionResults;
		}

		/// <summary>
		/// Removes a submission for one or more questions.
		/// </summary>
		public async Task DeleteSubmissionAsync(
			string classroomName,
			int assignmentId,
			int? assignmentQuestionId,
			int userId,
			DateTime submissionDate)
		{
			var submissionQuery = _dbContext.UserQuestionSubmissions
				.Include(uqs => uqs.UserQuestionData)
				.Where
				(
					uqs => uqs.UserQuestionData
						.AssignmentQuestion
						.Assignment
						.Classroom
						.Name == classroomName
				)
				.Where
				(
					uqs => uqs.UserQuestionData
					   .AssignmentQuestion
					   .AssignmentId == assignmentId	
				)
				.Where
				(
					uqs => uqs.UserQuestionData.UserId == userId	
				)
				.Where
				(
					uqs => (uqs.DateSubmitted - submissionDate).Duration() 
						< TimeSpan.FromMilliseconds(1)
				);

			if (assignmentQuestionId.HasValue)
			{
				submissionQuery = submissionQuery.Where
				(
					uqs => uqs.UserQuestionData
						       .AssignmentQuestionId == assignmentQuestionId
				);
			}

			var submissions = await submissionQuery.ToListAsync();
			foreach (var submission in submissions)
			{
				submission.UserQuestionData.NumAttempts--;
				_dbContext.UserQuestionSubmissions.Remove(submission);
			}

			await _dbContext.SaveChangesAsync();
		}

		/// <summary>
		/// Loads user question data for a single question.
		/// </summary>
		private async Task<UserQuestionDataStore> LoadDataForSingleQuestionAsync(
			string classroomName,
			int assignmentId,
			int assignmentQuestionId,
			int userId)
		{
			return await _userQuestionDataLoaderFactory
				.CreateLoaderForSingleQuestion
				(
					classroomName,
					assignmentId,
					assignmentQuestionId,
					userId
				).LoadUserQuestionDataAsync();
		}

		/// <summary>
		/// Loads user question data for a single question.
		/// </summary>
		private async Task<UserQuestionDataStore> LoadDataForAllAssignmentQuestionsAsync(
			string classroomName,
			int assignmentId,
			int userId)
		{
			return await _userQuestionDataLoaderFactory
				.CreateLoaderForAllAssignmentQuestions
				(
					classroomName,
					assignmentId,
					userId
				).LoadUserQuestionDataAsync();
		}
	}
}