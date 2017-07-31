using System;
using System.Collections.Generic;
using System.Text;
using CSC.CSClassroom.Model.Questions;

namespace CSC.CSClassroom.Service.Questions.UserQuestionDataUpdaters
{
	/// <summary>
	/// Generates a new seed for a GeneratedQuestionTemplate.
	/// </summary>
	public interface IGeneratedQuestionSeedGenerator
	{
		/// <summary>
		/// Generates a new seed.
		/// </summary>
		int GenerateSeed(UserQuestionData userQuestionData, int numSeeds);
	}
}
