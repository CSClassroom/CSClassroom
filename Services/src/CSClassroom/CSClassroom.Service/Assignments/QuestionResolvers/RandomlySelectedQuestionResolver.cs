using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.Serialization;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Repository;
using CSC.CSClassroom.Service.Assignments.QuestionLoaders;
using Microsoft.EntityFrameworkCore;
using MoreLinq;

namespace CSC.CSClassroom.Service.Assignments.QuestionResolvers
{
	/// <summary>
	/// Resolves randomly selected questions.
	/// </summary>
	public class RandomlySelectedQuestionResolver : IQuestionResolver
	{
		/// <summary>
		/// The question to resolve.
		/// </summary>
		private readonly UserQuestionData _userQuestionData;
		
		/// <summary>
		/// The database context.
		/// </summary>
		private readonly DatabaseContext _dbContext;

		/// <summary>
		/// The question loader factory.
		/// </summary>
		private readonly IQuestionLoaderFactory _questionLoaderFactory;

		/// <summary>
		/// Constructor.
		/// </summary>
		public RandomlySelectedQuestionResolver(
			UserQuestionData userQuestionData,
			DatabaseContext dbContext, 
			IQuestionLoaderFactory questionLoaderFactory)
		{
			_userQuestionData = userQuestionData;
			_dbContext = dbContext;
			_questionLoaderFactory = questionLoaderFactory;
		}

		/// <summary>
		/// Returns the next question to be solved in the next submission.
		/// </summary>
		public async Task<Question> ResolveUnsolvedQuestionAsync()
		{
			return await ResolveQuestionAsync(_userQuestionData.Seed.Value);
		}

		/// <summary>
		/// Returns the actual question that was solved on a given submission.
		/// </summary>
		public async Task<Question> ResolveSolvedQuestionAsync(
			UserQuestionSubmission submission)
		{
			return await ResolveQuestionAsync(submission.Seed.Value);
		}

		/// <summary>
		/// Resolves the given question.
		/// </summary>
		private async Task<Question> ResolveQuestionAsync(int questionId)
		{
			var actualQuestion = await _dbContext.Questions
				.Where(q => q.Id == questionId)
				.Include(q => q.QuestionCategory)
				.SingleAsync();

			await _questionLoaderFactory
				.CreateQuestionLoader(actualQuestion)
				.LoadQuestionAsync();

			return actualQuestion;
		}
	}
}
