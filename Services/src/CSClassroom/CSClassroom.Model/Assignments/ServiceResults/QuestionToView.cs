using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSC.CSClassroom.Model.Assignments.ServiceResults
{
	/// <summary>
	/// A generated question instance.
	/// </summary>
	public class GeneratedQuestionInstance
	{
		/// <summary>
		/// The generated question instance.
		/// </summary>
		public Question Question { get; }

		/// <summary>
		/// The seed used to generate the question.
		/// </summary>
		public int Seed { get; }

		/// <summary>
		/// The error encountered when attempting to generate the question.
		/// </summary>
		public string Error { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public GeneratedQuestionInstance(Question question, int seed, string error)
		{
			Question = question;
			Seed = seed;
			Error = error;
		}
	}
}
