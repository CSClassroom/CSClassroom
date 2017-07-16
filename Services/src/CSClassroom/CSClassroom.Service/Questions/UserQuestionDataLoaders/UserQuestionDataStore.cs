using System;
using System.Collections.Generic;
using System.Linq;
using CSC.Common.Infrastructure.Serialization;
using CSC.CSClassroom.Model.Questions;

namespace CSC.CSClassroom.Service.Questions.UserQuestionDataLoaders
{
	/// <summary>
	/// Returns and updates user question data.
	/// </summary>
	public class UserQuestionDataStore
	{
		/// <summary>
		/// A list of user question data objects in the store.
		/// </summary>
		private readonly IDictionary<int, UserQuestionData> _userQuestionDatas;

		/// <summary>
		/// Constructor.
		/// </summary>
		public UserQuestionDataStore(
			IDictionary<int, UserQuestionData> userQuestionDatas)
		{
			_userQuestionDatas = userQuestionDatas;
		}

		/// <summary>
		/// Returns a list of loaded question IDs.
		/// </summary>
		public IList<int> GetLoadedAssignmentQuestionIds()
		{
			return _userQuestionDatas.Keys.ToList();
		}

		/// <summary>
		/// Returns user question data for the given question.
		/// </summary>
		public UserQuestionData GetUserQuestionData(
			int assignmentQuestionId)
		{
			return _userQuestionDatas[assignmentQuestionId];
		}
	}
}
