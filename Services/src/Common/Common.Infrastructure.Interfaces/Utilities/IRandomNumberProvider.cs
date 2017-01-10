namespace CSC.Common.Infrastructure.Utilities
{
	/// <summary>
	/// A random number generator.
	/// </summary>
	public interface IRandomNumberProvider
	{
		/// <summary>
		/// Returns a random integer.
		/// </summary>
		int NextInt();
	}
}
