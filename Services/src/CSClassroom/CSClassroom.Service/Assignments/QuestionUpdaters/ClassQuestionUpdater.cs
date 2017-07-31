using System.Threading.Tasks;
using CSC.Common.Infrastructure.Utilities;
using CSC.CSClassroom.Model.Questions;
using CSC.CSClassroom.Repository;

namespace CSC.CSClassroom.Service.Questions.QuestionUpdaters
{
	/// <summary>
	/// Updates a class question.
	/// </summary>
	public class ClassQuestionUpdater : CodeQuestionUpdater<ClassQuestion>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public ClassQuestionUpdater(DatabaseContext dbContext, ClassQuestion question, IModelErrorCollection errors) 
			: base(dbContext, question, errors)
		{
		}

		/// <summary>
		/// Performs code-question-type-specific update operations.
		/// </summary>
		protected override Task UpdateCodeQuestionImplAsync()
		{
			if (!Question.FileTemplate.Contains(ClassQuestion.SubmissionPlaceholder))
			{
				Errors.AddError
				(
					"FileTemplate",
					$"The file template must contain the string '{ClassQuestion.SubmissionPlaceholder}'."
				);
			}

			DbContext.RemoveUnwantedObjects
			(
				DbContext.ClassQuestionTests,
				test => test.Id,
				test => test.ClassQuestionId == Question.Id,
				Question.Tests
			);

			DbContext.RemoveUnwantedObjects
			(
				DbContext.RequiredMethods,
				method => method.Id,
				method => method.ClassQuestionId == Question.Id,
				Question.RequiredMethods
			);

			return Task.CompletedTask;
		}
	}
}
