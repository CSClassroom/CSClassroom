namespace CSC.Common.Infrastructure.GitHub
{
	/// <summary>
	/// A GitHub repository.
	/// </summary>
	public class GitHubRepository
	{
		/// <summary>
		/// The repository ID.
		/// </summary>
		public int Id { get; }

		/// <summary>
		/// The owner.
		/// </summary>
		public string Owner { get; }

		/// <summary>
		/// The repository name.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public GitHubRepository(int id, string owner, string name)
		{
			Id = id;
			Owner = owner;
			Name = name;
		}
	}
}
