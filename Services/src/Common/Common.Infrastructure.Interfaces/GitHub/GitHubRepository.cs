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
		public long Id { get; }

		/// <summary>
		/// The owner.
		/// </summary>
		public string Owner { get; }

		/// <summary>
		/// The repository name.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// The default branch for the repository.
		/// </summary>
		public string DefaultBranch { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public GitHubRepository(long id, string owner, string name, string defaultBranch)
		{
			Id = id;
			Owner = owner;
			Name = name;
			DefaultBranch = defaultBranch;
		}
	}
}
