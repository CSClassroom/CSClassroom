using System;
using CSC.Common.Infrastructure.Utilities;

namespace CSC.CSClassroom.WebApp.Providers
{
	/// <summary>
	/// A random number provider.
	/// </summary>
	public class RandomNumberProvider : IRandomNumberProvider
	{
		/// <summary>
		/// The random number generator.
		/// </summary>
		private readonly Random _random = new Random();

		/// <summary>
		/// Generates a new random number.
		/// </summary>
		public int NextInt() => _random.Next();
	}
}
