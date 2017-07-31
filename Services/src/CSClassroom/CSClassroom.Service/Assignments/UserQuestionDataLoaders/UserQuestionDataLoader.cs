using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.Interfaces.Extensions;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Repository;
using CSC.CSClassroom.Service.Assignments.QuestionLoaders;
using CSC.CSClassroom.Service.Assignments.UserQuestionDataUpdaters;
using Microsoft.EntityFrameworkCore;

namespace CSC.CSClassroom.Service.Assignments.UserQuestionDataLoaders
{
	/// <summary>
	/// Loads user question data.
	/// </summary>
	public class UserQuestionDataLoader : IUserQuestionDataLoader
	{
		/// <summary>
		/// The database context.
		/// </summary>
		private readonly DatabaseContext _dbContext;

		/// <summary>
		/// The question loader factory.
		/// </summary>
		private readonly IQuestionLoaderFactory _questionLoaderFactory;

		/// <summary>
		/// The question updater factory.
		/// </summary>
		private readonly IUserQuestionDataUpdaterFactory _userQuestionDataUpdaterFactory;

		/// <summary>
		/// The classroom name.
		/// </summary>
		private readonly string _classroomName;

		/// <summary>
		/// The assignment ID.
		/// </summary>
		private readonly int _assignmentId;

		/// <summary>
		/// The user ID.
		/// </summary>
		private readonly int _userId;

		/// <summary>
		/// A filter for the assignment questions to load.
		/// </summary>
		private Expression<Func<AssignmentQuestion, bool>> _assignmentQuestionsFilter;

		/// <summary>
		/// A filter for the UserQuestionData objects to load.
		/// </summary>
		private Expression<Func<UserQuestionData, bool>> _userQuestionDataFilter;

		/// <summary>
		/// Constructor.
		/// </summary>
		public UserQuestionDataLoader(
			DatabaseContext dbContext,
			IQuestionLoaderFactory questionLoaderFactory,
			IUserQuestionDataUpdaterFactory userQuestionDataUpdaterFactory,
			string classroomName,
			int assignmentId,
			int userId,
			Expression<Func<AssignmentQuestion, bool>> assignmentQuestionsFilter,
			Expression<Func<UserQuestionData, bool>> userQuestionDataFilter)
		{
			_dbContext = dbContext;
			_questionLoaderFactory = questionLoaderFactory;
			_userQuestionDataUpdaterFactory = userQuestionDataUpdaterFactory;
			_classroomName = classroomName;
			_assignmentId = assignmentId;
			_userId = userId;
			_assignmentQuestionsFilter = assignmentQuestionsFilter;
			_userQuestionDataFilter = userQuestionDataFilter;
		}

		/// <summary>
		/// Loads data for the questions speciied by the given query.
		/// </summary>
		public async Task<UserQuestionDataStore> LoadUserQuestionDataAsync()
		{
			var assignmentQuestions = await GetAssignmentQuestionsQuery()
				.ToDictionaryAsync(aq => aq.Id, aq => aq);

			foreach (var assignmentQuestion in assignmentQuestions.Values)
			{
				await _questionLoaderFactory
					.CreateQuestionLoader(assignmentQuestion.Question)
					.LoadQuestionAsync();
			}

			var includeSubmissions = assignmentQuestions.Values.Any
			(
				aq => !aq.IsInteractive()
			);

			var userQuestionDatas = await GetUserQuestionDatasQuery(includeSubmissions)
				.Where(_userQuestionDataFilter)
				.ToDictionaryAsync(uqd => uqd.AssignmentQuestionId, uqd => uqd);

			await EnsureUserQuestionDataUpdatedAsync
			(
				assignmentQuestions,
				userQuestionDatas
			);

			return new UserQuestionDataStore(userQuestionDatas);
		}

		/// <summary>
		/// Returns a query for assignment questions of a given assignment.
		/// </summary>
		private IQueryable<AssignmentQuestion> GetAssignmentQuestionsQuery()
		{
			return _dbContext.AssignmentQuestions
				.Where(aq => aq.Question.QuestionCategory.Classroom.Name == _classroomName)
				.Where(aq => aq.AssignmentId == _assignmentId)
				.Where(_assignmentQuestionsFilter)
				.Include(aq => aq.Assignment)
				.Include(aq => aq.Question)
				.OrderBy(aq => aq.Order);
		}

		/// <summary>
		/// Returns a list of all user question data objects for the given
		/// assignment and user.
		/// </summary>
		private IQueryable<UserQuestionData> GetUserQuestionDatasQuery(
			bool includeSubmissions)
		{
			var queryable = _dbContext.UserQuestionData
				.Where(uqd => uqd.AssignmentQuestion.Assignment.Classroom.Name == _classroomName)
				.Where(uqd => uqd.AssignmentQuestion.AssignmentId == _assignmentId)
				.Where(uqd => uqd.UserId == _userId)
				.Include(uqd => uqd.User);

			if (includeSubmissions)
			{
				return queryable.Include(uqd => uqd.Submissions);
			}
			else
			{
				return queryable;
			}
		}

		/// <summary>
		/// Regenerates all loaded questions if applicable.
		/// </summary>
		private async Task EnsureUserQuestionDataUpdatedAsync(
			IDictionary<int, AssignmentQuestion> assignmentQuestions,
			IDictionary<int, UserQuestionData> userQuestionDatas)
		{
			var userQuestionDataUpdater = _userQuestionDataUpdaterFactory
				.CreateUserQuestionDataUpdater();

			foreach (var assignmentQuestion in assignmentQuestions.Values)
			{
				var userQuestionData = userQuestionDatas
					.GetValueOrDefault(assignmentQuestion.Id);

				if (userQuestionData == null)
				{
					userQuestionData = new UserQuestionData()
					{
						UserId = _userId,
						AssignmentQuestionId = assignmentQuestion.Id,
						AssignmentQuestion = assignmentQuestion
					};

					_dbContext.UserQuestionData.Add(userQuestionData);
					userQuestionDatas[assignmentQuestion.Id] = userQuestionData;
				}

				userQuestionDataUpdater.AddToBatch(userQuestionData);
			}

			await userQuestionDataUpdater.UpdateAllAsync();
		}
	}
}
