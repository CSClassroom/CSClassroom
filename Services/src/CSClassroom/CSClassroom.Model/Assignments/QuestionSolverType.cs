using System;
using System.Collections.Generic;
using System.Text;

namespace CSC.CSClassroom.Model.Questions
{
	/// <summary>
	/// The type of solver supported for this question.
	/// </summary>
	[Flags]
	public enum QuestionSolverType
	{
		None = 0,
		Interactive = 1,
		NonInteractive = 2
	}
}
