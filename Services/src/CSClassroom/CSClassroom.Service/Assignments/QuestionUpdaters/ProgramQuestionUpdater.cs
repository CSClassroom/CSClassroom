using System.Threading.Tasks;
using CSC.Common.Infrastructure.Utilities;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Repository;

namespace CSC.CSClassroom.Service.Assignments.QuestionUpdaters
{
	/// <summary>
	/// Updates a program question.
	/// </summary>
	public class ProgramQuestionUpdater : CodeQuestionUpdater<ProgramQuestion>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public ProgramQuestionUpdater(DatabaseContext dbContext, ProgramQuestion question, IModelErrorCollection errors) 
			: base(dbContext, question, errors)
		{
		}

		/// <summary>
		/// Performs code-question-type-specific update operations.
		/// </summary>
		protected override Task UpdateCodeQuestionImplAsync()
		{
			DbContext.RemoveUnwantedObjects
			(
				DbContext.ProgramQuestionTests,
				test => test.Id,
				test => test.ProgramQuestionId == Question.Id,
				Question.Tests
			);

			return Task.CompletedTask;
		}
	}
}
