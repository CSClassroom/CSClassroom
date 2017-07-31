using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.Utilities;
using CSC.CSClassroom.Model.Questions;
using CSC.CSClassroom.Repository;
using CSC.CSClassroom.Service.Questions.QuestionLoaders;
using Microsoft.EntityFrameworkCore;
using MoreLinq;

namespace CSC.CSClassroom.Service.Questions.UserQuestionDataUpdaters
{
	/// <summary>
	/// A UserQuestionDataUpdater for randomly selected questions.
	/// </summary>
	public class RandomlySelectedUserQuestionDataUpdater : IUserQuestionDataUpdater
	{
		/// <summary>
		/// The database context.
		/// </summary>
		private readonly DatabaseContext _dbContext;

		/// <summary>
		/// The UserQuestionData objects to update.
		/// </summary>
		private readonly IList<UserQuestionData> _userQuestionDatas;

		/// <summary>
		/// The random number provider.
		/// </summary>
		private readonly IRandomlySelectedQuestionSelector _randomlySelectedQuestionSelector;

		/// <summary>
		/// Constructor.
		/// </summary>
		public RandomlySelectedUserQuestionDataUpdater(
			DatabaseContext dbContext,
			IRandomlySelectedQuestionSelector randomlySelectedQuestionSelector)
		{
			_dbContext = dbContext;
			_randomlySelectedQuestionSelector = randomlySelectedQuestionSelector;
			_userQuestionDatas = new List<UserQuestionData>();
		}

		/// <summary>
		/// Adds a job to the batch that updates the given UserQuestionData object.
		/// </summary>
		public void AddToBatch(UserQuestionData userQuestionData)
		{
			if (!(userQuestionData.AssignmentQuestion.Question is RandomlySelectedQuestion))
			{
				throw new InvalidOperationException("Invalid question type.");
			}

			_userQuestionDatas.Add(userQuestionData);
		}

		/// <summary>
		/// Updates all UserQuestionData objects. 
		/// </summary>
		public async Task UpdateAllAsync()
		{
			foreach (var userQuestionData in _userQuestionDatas)
			{
				await UpdateUserQuestionDataAsync(userQuestionData);
			}
		}

		/// <summary>
		/// Updates the given UserQuestionData object.
		/// </summary>
		private async Task UpdateUserQuestionDataAsync(UserQuestionData userQuestionData)
		{
			var randomlySelectedQuestion = (RandomlySelectedQuestion)userQuestionData
				.AssignmentQuestion
				.Question;

			var availableQuestionIds = await _dbContext.Questions
				.Where(q => q.QuestionCategoryId == randomlySelectedQuestion.ChoicesCategory.Id)
				.Select(q => q.Id)
				.ToListAsync();

			if (NewQuestionNeededForNextAttempt(userQuestionData)
				|| ExistingQuestionNoLongerValid(userQuestionData, availableQuestionIds))
			{
				userQuestionData.Seed = _randomlySelectedQuestionSelector
					.GetNextQuestionId(userQuestionData, availableQuestionIds);

				userQuestionData.LastQuestionSubmission = null;
			}
		}

		/// <summary>
		/// Returns whether or not a new question is needed for the user's next attempt.
		/// </summary>
		private bool NewQuestionNeededForNextAttempt(UserQuestionData userQuestionData)
		{
			return userQuestionData.Seed == null && userQuestionData.AnyAttemptsRemaining;
		}

		/// <summary>
		/// Returns whether or not a new question is needed because the existing question
		/// is no longer valid. This can happen if a choice is removed.
		/// </summary>
		private bool ExistingQuestionNoLongerValid(
			UserQuestionData userQuestionData,
			IList<int> availableQuestionIds)
		{
			return userQuestionData.Seed.HasValue
				&& !availableQuestionIds.Contains(userQuestionData.Seed.Value);
		}
	}
}
