using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.Utilities;
using CSC.CSClassroom.Model.Questions;
using CSC.CSClassroom.Repository;
using Microsoft.EntityFrameworkCore;

namespace CSC.CSClassroom.Service.Questions.Validators
{
	/// <summary>
	/// Ensures that a new or existing assignment is valid.
	/// </summary>
	public class AssignmentValidator : IAssignmentValidator
	{
		/// <summary>
		/// The database context.
		/// </summary>
		private readonly DatabaseContext _dbContext;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="dbContext"></param>
		public AssignmentValidator(DatabaseContext dbContext)
		{
			_dbContext = dbContext;
		}

		/// <summary>
		/// Validates that an assignment is correctly configured.
		/// </summary>
		public async Task<bool> ValidateAssignmentAsync(
			Assignment assignment,
			IModelErrorCollection modelErrors)
		{
			if (assignment.DueDates != null)
			{
				var sections = assignment.DueDates.Select(d => d.SectionId).ToList();
				if (sections.Distinct().Count() != sections.Count)
				{
					modelErrors.AddError("DueDates", "You may only have one due date per section.");

					return false;
				}
			}

			var existingAssignmentQuestions = await _dbContext.AssignmentQuestions
				.Where(aq => aq.AssignmentId == assignment.Id)
				.ToListAsync();

			foreach (var oldQuestion in assignment.Questions)
			{
				var conflicts = existingAssignmentQuestions
					.Any
					(
						newQuestion => newQuestion.Id == oldQuestion.Id
									   && newQuestion.QuestionId != oldQuestion.QuestionId
					);

				if (conflicts)
				{
					modelErrors.AddError("Questions", "You may not modify an existing question.");
					return false;
				}

				_dbContext.Entry(oldQuestion).State = EntityState.Detached;
			}

			var questionNames = assignment.Questions
				.Select(aq => aq.Name)
				.ToList();

			if (questionNames.Distinct().Count() != questionNames.Count)
			{
				modelErrors.AddError("Questions", "No two questions may have the same name.");
				return false;
			}

			if (assignment.CombinedSubmissions)
			{
				var newQuestionIds = new HashSet<int>
				(
					assignment.Questions
						.Select(aq => aq.QuestionId)
				);

				bool anyUnsupportedQuestions = await _dbContext.Questions
					.Where(q => newQuestionIds.Contains(q.Id))
					.AnyAsync(q => q.UnsupportedSolver(QuestionSolverType.NonInteractive));

				if (anyUnsupportedQuestions)
				{
					modelErrors.AddError
					(
						"CombinedSubmissions",
						"Submissions may not be combined if the assignment contains any questions " 
							+ "that do not support non-interactive submissions (such as code questions)."
					);

					return false;
				}
			}

			if (assignment.CombinedSubmissions && assignment.AnswerInOrder)
			{
				modelErrors.AddError
				(
					"AnswerInOrder",
					"The 'Answer In Order' option may not be selected when submissions are combined."
				);

				return false;
			}

			if (!assignment.CombinedSubmissions && assignment.OnlyShowCombinedScore)
			{
				modelErrors.AddError
				(
					"OnlyShowCombinedScore",
					"The 'Only Show Combined Score' option may only be selected when submissions are combined."
				);

				return false;
			}

			return true;
		}
	}
}
