namespace CSC.CSClassroom.Model.Projects
{
	/// <summary>
	/// A path to a private file (or set of files). Private files are not
	/// distributed to student project repositories, but are used for testing.
	/// </summary>
	public class PrivateFilePath
	{
		/// <summary>
		/// The primary key.
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// The ID of the project that this private file path belongs to.
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
