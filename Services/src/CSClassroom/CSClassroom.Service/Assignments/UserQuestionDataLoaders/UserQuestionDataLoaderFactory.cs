using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Repository;
using CSC.CSClassroom.Service.Assignments.QuestionLoaders;
using CSC.CSClassroom.Service.Assignments.UserQuestionDataUpdaters;

namespace CSC.CSClassroom.Service.Assignments.UserQuestionDataLoaders
{
	/// <summary>
	/// Creates user question data loaders.
	/// </summary>
	public class UserQuestionDataLoaderFactory : IUserQuestionDataLoaderFactory
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
		/// Constructor.
		/// </summary>
		public UserQuestionDataLoaderFactory(
			DatabaseContext dbContext, 
			IQuestionLoaderFactory questionLoaderFactory, 
			IUserQuestionDataUpdaterFactory userQuestionDataUpdaterFactory)
		{
			_dbContext = dbContext;
			_questionLoaderFactory = questionLoaderFactory;
			_userQuestionDataUpdaterFactory = userQuestionDataUpdaterFactory;
		}

		/// <summary>
		/// Creates a loader that loads the user question data for 
		/// a single question.
		/// </summary>
		public IUserQuestionDataLoader CreateLoaderForSingleQuestion(
			string classroomName,
			int assignmentId,
			int assignmentQuestionId,
			int userId)
		{
			return CreateLoader
			(
				classroomName,
				assignmentId,
				userId,
				assignmentQuestion => assignmentQuestion.Id == assignmentQuestionId,
				userQuestionData => userQuestionData.AssignmentQuestionId == assignmentQuestionId
			);
		}

		/// <summary>
		/// Creates a loader that loads the user question data for 
		/// all questions in the assignment.
		/// </summary>
		public IUserQuestionDataLoader CreateLoaderForAllAssignmentQuestions(
			string classroomName,
			int assignmentId,
			int userId)
		{
			return CreateLoader
			(
				classroomName,
				assignmentId,
				userId,
				assignmentQuestion => true,
				userQuestionData => true
			);
		}

		/// <summary>
		/// Creates a loader.
		/// </summary>
		private IUserQuestionDataLoader CreateLoader(
			string classroomName, 
			int assignmentId, 
			int userId,
			Expression<Func<AssignmentQuestion, bool>> assignmentQuestionFilter,
			Expression<Func<UserQuestionData, bool>> userQuestionDataFilter)
		{
			return new UserQuestionDataLoader
			(
				_dbContext,
				_questionLoaderFactory,
				_userQuestionDataUpdaterFactory,
				classroomName,
				assignmentId,
				userId,
				assignmentQuestionFilter,
				userQuestionDataFilter
			);
		}
	}
}
