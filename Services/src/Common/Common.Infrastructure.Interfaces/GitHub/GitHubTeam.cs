namespace CSC.Common.Infrastructure.GitHub
{
	/// <summary>
	/// A GitHub team.
	/// </summary>
	public class GitHubTeam
	{
		/// <summary>
		/// The team ID.
		/// </summary>
		public int Id { get; }

		/// <summary>
		/// The team name.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public GitHubTeam(int id, string name)
		{
			Id = id;
			Name = name;
		}
	}
}
