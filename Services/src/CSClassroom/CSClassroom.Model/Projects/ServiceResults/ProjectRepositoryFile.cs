namespace CSC.CSClassroom.Model.Projects.ServiceResults
{
	/// <summary>
	/// The type of file in the repository.
	/// </summary>
	public enum FileType
	{
		// A file that will appear in student repositories.
		Public,

		// A file that will not appear in student repositories.
		Private,

		// A file that appears in student repositories, for which
		// student changes are ignored during building and testing.
		Immutable
	}

	/// <summary>
	/// A file in a project repository.
	/// </summary>
	public class ProjectRepositoryFile
	{
		/// <summary>
		/// The type of file.
		/// </summary>
		public FileType FileType { get; }

		/// <summary>
		/// The path to the file.
		/// </summary>
		public string Path { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public ProjectRepositoryFile(FileType fileType, string path)
		{
			FileType = fileType;
			Path = path;
		}
	}
}
