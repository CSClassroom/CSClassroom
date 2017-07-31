using System;
using System.Collections.Generic;
using System.Text;
using CSC.Common.Infrastructure.Utilities;
using CSC.CSClassroom.Model.Questions;

namespace CSC.CSClassroom.Service.Questions.QuestionUpdaters
{
	/// <summary>
	/// Creates question updaters.
	/// </summary>
	public interface IQuestionUpdaterFactory
	{
		/// <summary>
		/// Creates a question updater.
		/// </summary>
		IQuestionUpdater CreateQuestionUpdater(
			Question question, 
			IModelErrorCollection errors);
	}
}
