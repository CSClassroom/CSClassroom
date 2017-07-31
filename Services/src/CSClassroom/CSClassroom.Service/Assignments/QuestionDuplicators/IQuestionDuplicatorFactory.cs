using System;
using System.Collections.Generic;
using System.Text;
using CSC.CSClassroom.Model.Assignments;

namespace CSC.CSClassroom.Service.Assignments.QuestionDuplicators
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
