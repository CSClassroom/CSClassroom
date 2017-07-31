using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSC.CSClassroom.Model.Assignments.ServiceResults;
using CSC.CSClassroom.Repository;
using Microsoft.EntityFrameworkCore;

namespace CSC.CSClassroom.Service.Assignments.QuestionSolvers
{
	/// <summary>
	/// Retrieves the progress of an assignment for a student.
	/// </summary>
	public class AssignmentProgressRetriever : IAssignmentProgressRetriever
	{
		/// <summary>
		/// The database context.
		/// </summary>
		private readonly DatabaseContext _dbContext;

		/// <summary>
		/// Constructor.
		/// </summary>
		public AssignmentProgressRetriever(DatabaseContext dbContext)
		{
			_dbContext = dbContext;
		}

		/// <summary>
		/// Returns the assignment progress for a given student.
		/// </summary>
		public async Task<AssignmentProgress> GetAssignmentProgressAsync(
			int assignmentId,
			int currentAssignmentQuestionId,
			int userId)
		{
			var questionProgress = await _dbContext.AssignmentQuestions
				.Where(aq => aq.AssignmentId == assignmentId)
				.OrderBy(aq => aq.Order)
				.Select
				(
					aq => new
					{
						aq.Id,
						aq.Name,
						MaxScore = _dbContext.UserQuestionSubmissions
							.Where(uqs => uqs.UserQuestionData.AssignmentQuestionId == aq.Id)
							.Where(uqs => uqs.UserQuestionData.UserId == userId)
							.Max(uqs => (double?) uqs.Score)
					}
				).ToListAsync();

			return new AssignmentProgress
			(
				userId,
				currentAssignmentQuestionId,
				questionProgress.Select
				(
					qp => new QuestionProgress
					(
						qp.Id,
						qp.Name,
						GetQuestionCompletion(qp.MaxScore)
					)
				).ToList()
			);
		}

		/// <summary>
		/// Returns the completion status corresponding to the given score.
		/// </summary>
		private QuestionCompletion GetQuestionCompletion(double? score)
		{
			if (score == null || score <= 0)
			{
				return QuestionCompletion.NotCompleted;
			}
			else if (score < 1.0)
			{
				return QuestionCompletion.PartiallyCompleted;
			}
			else
			{
				return QuestionCompletion.Completed;
			}
		}
	}
}
