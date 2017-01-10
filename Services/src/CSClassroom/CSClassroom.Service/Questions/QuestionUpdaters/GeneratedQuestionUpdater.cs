using System;
using System.Threading.Tasks;
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
		/// Constructor.
		/// </summary>
		public GeneratedQuestionUpdater(
			DatabaseContext dbContext, 
			GeneratedQuestionTemplate question, 
			IModelErrorCollection errors, 
			IQuestionGenerator questionGenerator) 
				: base(dbContext, question, errors)
		{
			_questionGenerator = questionGenerator;
		}

		/// <summary>
		/// Performs code-question-type-specific update operations.
		/// </summary>
		protected override async Task UpdateQuestionImplAsync()
		{
			var generateResult = await _questionGenerator.GenerateQuestionAsync(Question, 0);
			if (generateResult.Error != null)
			{
				Errors.AddError("GeneratorContents", generateResult.Error);
			}
			else
			{
				Question.DateModified = DateTime.Now;
				Question.FullGeneratorFileContents = generateResult.FullGeneratorFileContents;
				Question.FullGeneratorFileLineOffset = generateResult.FullGeneratorFileLineOffset;
			}
		}
	}
}
