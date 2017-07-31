using System;
using System.Collections.Generic;
using System.Text;
using CSC.CSClassroom.Model.Questions;

namespace CSC.CSClassroom.Service.Questions.QuestionLoaders
{
	/// <summary>
	/// Creates question loaders.
	/// </summary>
	public interface IQuestionLoaderFactory
	{
		/// <summary>
		/// Creates a question loader.
		/// </summary>
		IQuestionLoader CreateQuestionLoader(Question question);
	}
}
