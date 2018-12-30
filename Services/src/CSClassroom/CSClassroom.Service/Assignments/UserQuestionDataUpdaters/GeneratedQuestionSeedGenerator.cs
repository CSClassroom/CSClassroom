using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSC.Common.Infrastructure.Utilities;
using CSC.CSClassroom.Model.Assignments;
using MoreLinq;

namespace CSC.CSClassroom.Service.Assignments.UserQuestionDataUpdaters
{
	/// <summary>
	/// Generates a new seed for a GeneratedQuestionTemplate.
	/// </summary>
	public class GeneratedQuestionSeedGenerator : IGeneratedQuestionSeedGenerator
	{
		/// <summary>
		/// A random number generator.
		/// </summary>
		private readonly IRandomNumberProvider _randomNumberProvider;

		/// <summary>
		/// Constructor.
		/// </summary>
		public GeneratedQuestionSeedGenerator(IRandomNumberProvider randomNumberProvider)
		{
			_randomNumberProvider = randomNumberProvider;
		}

		/// <summary>
		/// Generates a new seed.
		/// </summary>
		public int GenerateSeed(UserQuestionData userQuestionData, int numSeeds)
		{
			var existingSeeds = userQuestionData.Submissions
				?.Where(uqs => uqs.Seed.HasValue && uqs.Seed < numSeeds)
				?.GroupBy(uqs => uqs.Seed.Value)
				?.ToDictionary(g => g.Key, g => g.Count())
					?? new Dictionary<int, int>();

			int newSeed;

			if (existingSeeds.Count >= numSeeds)
			{
				newSeed = existingSeeds
					.MinBy(kvp => kvp.Value)
					.First()
					.Key;
			}
			else
			{
				do
				{
					newSeed = _randomNumberProvider.NextInt() % numSeeds;                 
				} while (existingSeeds.ContainsKey(newSeed));
			}

			return newSeed;
		}
	}
}
