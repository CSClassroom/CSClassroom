using System;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.System;
using CSC.Common.Infrastructure.Utilities;
using CSC.CSClassroom.Model.Questions;
using CSC.CSClassroom.Repository;
using CSC.CSClassroom.Service.Questions.QuestionGeneration;

namespace CSC.CSClassroom.Service.Questions.QuestionUpdaters
{
	/// <summary>
	/// Updates a generated question.
	/// </summary>
	public class GeneratedQuestionUpdater : QuestionUpdater<GeneratedQuestionTemplate>
	{
		/// <summary>
		/// The question generator.
		/// </summary>
		private readonly IQuestionGenerator _questionGenerator;

		/// <summary>
		/// The time provider.
		/// </summary>
		private readonly ITimeProvider _timeProvider;

		/// <summary>
		/// Constructor.
		/// </summary>
		public GeneratedQuestionUpdater(
			DatabaseContext dbContext, 
			GeneratedQuestionTemplate question, 
			IModelErrorCollection errors, 
			IQuestionGenerator questionGenerator,
			ITimeProvider timeProvider) 
				: base(dbContext, question, errors)
		{
			_questionGenerator = questionGenerator;
			_timeProvider = timeProvider;
		}

		/// <summary>
		/// Performs code-question-type-specific update operations.
		/// </summary>
		protected override async Task UpdateQuestionImplAsync()
		{
			DbContext.RemoveUnwantedObjects
			(
				DbContext.ImportedClasses,
				importedClass => importedClass.Id,
				importedClass => importedClass.CodeQuestionId == Question.Id,
				Question.ImportedClasses
			);

			var generateResult = await _questionGenerator.GenerateQuestionAsync(Question, 0);
			if (generateResult.Error != null)
			{
				Errors.AddError("GeneratorContents", generateResult.Error);
			}
			else
			{
				Question.DateModified = _timeProvider.UtcNow;
				Question.FullGeneratorFileContents = generateResult.FullGeneratorFileContents;
				Question.FullGeneratorFileLineOffset = generateResult.FullGeneratorFileLineOffset;
			}
		}
	}
}
