using System;
using System.Collections.Generic;
using System.Text;
using CSC.CSClassroom.Model.Questions;

namespace CSC.CSClassroom.Service.Questions.QuestionGraders
{
	/// <summary>
	/// Creates question graders.
	/// </summary>
	public interface IQuestionGraderFactory
	{
		/// <summary>
		/// Creates a question grader.
		/// </summary>
		IQuestionGrader CreateQuestionGrader(Question question);
	}
}
