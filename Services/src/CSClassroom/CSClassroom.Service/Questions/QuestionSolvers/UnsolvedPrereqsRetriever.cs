using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSC.CSClassroom.Model.Questions;
using CSC.CSClassroom.Repository;
using Microsoft.EntityFrameworkCore;

namespace CSC.CSClassroom.Service.Questions.QuestionSolvers
{
	/// <summary>
	/// Retrieves unsolved prerequisite questions from the database
	/// for a given question in an assignment.
	/// </summary>
	public class UnsolvedPrereqsRetriever 
		: IUnsolvedPrereqsRetriever
	{
		/// <summary>
		/// The database context.
		/// </summary>
		private readonly DatabaseContext _dbContext;

		/// <summary>
		/// Constructor.
		/// </summary>
		public UnsolvedPrereqsRetriever(DatabaseContext dbContext)
		{
			_dbContext = dbContext;
		}

		/// <summary>
		/// Returns a list of unsolved prerequisite question IDs.
		/// </summary>
		public async Task<IList<AssignmentQuestion>> GetUnsolvedPrereqsAsync(
			UserQuestionData userQuestionData)
		{
			if (!userQuestionData.AssignmentQuestion.Assignment.AnswerInOrder
				|| userQuestionData.AssignmentQuestion.Assignment.CombinedSubmissions)
			{
				return new List<AssignmentQuestion>();
			}

			var solvedPrereqs = await _dbContext.UserQuestionData
				.Where(uqd => uqd.UserId == userQuestionData.UserId)
				.Where
				(
					uqd => uqd.AssignmentQuestion.Order <
						   userQuestionData.AssignmentQuestion.Order
				)
				.Where(uqd => uqd.Submissions.Max(s => s.Score) == 1.0)
				.Select(uqd => uqd.AssignmentQuestionId)
				.ToListAsync();

			var allPrereqs = userQuestionData.AssignmentQuestion
				.Assignment
				.Questions
				.Where(aq => aq.Order < userQuestionData.AssignmentQuestion.Order)
				.Select(aq => aq.Id)
				.ToList();

			return allPrereqs
				.Except(solvedPrereqs)
				.Select
				(
					aqId => userQuestionData
						.AssignmentQuestion
						.Assignment
						.Questions
						.Single(aq => aq.Id == aqId)
				)
				.OrderBy(aq => aq.Order)
				.ToList();
		}
	}
}
