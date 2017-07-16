using System;
using System.Collections.Generic;
using System.Text;
using CSC.CSClassroom.Model.Questions;

namespace CSC.CSClassroom.Service.Questions.QuestionDuplicators
{
	/// <summary>
	/// Creates question duplicators.
	/// </summary>
	public interface IQuestionDuplicatorFactory
	{
		/// <summary>
		/// Creates a question duplicator.
		/// </summary>
		IQuestionDuplicator CreateQuestionDuplicator(Question question);
	}
}
