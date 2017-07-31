using System.Threading.Tasks;
using CSC.Common.Infrastructure.Utilities;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Repository;

namespace CSC.CSClassroom.Service.Assignments.QuestionUpdaters
{
	/// <summary>
	/// Updates a class question.
	/// </summary>
	public class MethodQuestionUpdater : CodeQuestionUpdater<MethodQuestion>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public MethodQuestionUpdater(DatabaseContext dbContext, MethodQuestion question, IModelErrorCollection errors) 
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
				DbContext.MethodQuestionTests,
				test => test.Id,
				test => test.MethodQuestionId == Question.Id,
				Question.Tests
			);

			return Task.CompletedTask;
		}
	}
}
