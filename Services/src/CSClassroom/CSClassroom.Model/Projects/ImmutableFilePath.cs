namespace CSC.CSClassroom.Model.Projects
{
	/// <summary>
	/// A path to an immutable file (or set of files). Immutable files are 
	/// taken from the project template for purposes of testing. Any student
	/// changes to such files are ignored.
	/// </summary>
	public class ImmutableFilePath
	{
		/// <summary>
		/// The primary key.
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// The ID of the project that this immutable file path belongs to.
		/// </summary>
		public int ProjectId { get; set; }

		/// <summary>
		/// The project that this private file path belongs to.
		/// </summary>
		public Project Project { get; set; }

		/// <summary>
		/// The path.
		/// </summary>
		public string Path { get; set; }
	}
}
