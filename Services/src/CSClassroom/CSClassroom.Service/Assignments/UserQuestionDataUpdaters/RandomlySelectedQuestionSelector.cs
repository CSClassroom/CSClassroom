using System.Collections.Generic;
using System.Linq;
using CSC.Common.Infrastructure.Utilities;
using CSC.CSClassroom.Model.Assignments;
using MoreLinq;

namespace CSC.CSClassroom.Service.Assignments.UserQuestionDataUpdaters
{
	/// <summary>
	/// Generates the next question to show for a randomly selected question.
	/// </summary>
	public class RandomlySelectedQuestionSelector : IRandomlySelectedQuestionSelector
	{
		/// <summary>
		/// A random number generator.
		/// </summary>
		private readonly IRandomNumberProvider _randomNumberProvider;

		/// <summary>
		/// Constructor.
		/// </summary>
		public RandomlySelectedQuestionSelector(
			IRandomNumberProvider randomNumberProvider)
		{
			_randomNumberProvider = randomNumberProvider;
		}

		/// <summary>
		/// Returns the next question ID to use.
		/// </summary>
		public int GetNextQuestionId(
			UserQuestionData userQuestionData,
			IList<int> availableQuestionIds)
		{
			var seenQuestionIds = userQuestionData
				?.Submissions
				?.Where(uqs => uqs.Seed.HasValue)
				?.GroupBy(uqs => uqs.Seed.Value)
				?.ToDictionary(g => g.Key, g => g.Count()) 
					?? new Dictionary<int, int>();

			var validQuestionIds = new HashSet<int>(availableQuestionIds);

			var unseenQuestionIds = validQuestionIds
				.Except(seenQuestionIds.Keys)
				.ToList();

			if (unseenQuestionIds.Any())
			{
				var randomIndex = _randomNumberProvider.NextInt() % unseenQuestionIds.Count;

				return unseenQuestionIds[randomIndex];
			}
			else
			{
				return seenQuestionIds
					.Where(kvp => validQuestionIds.Contains(kvp.Key))
					.MinBy(kvp => kvp.Value)
					.First()
					.Key;
			}
		}
	}
}