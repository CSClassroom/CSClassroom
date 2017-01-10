namespace CSC.Common.Infrastructure.System
{
	/// <summary>
	/// The backing store of an archive.
	/// </summary>
	public enum ArchiveStore
	{
		/// <summary>
		/// The contents are backed by memory.
		/// </summary>
		Memory,

		/// <summary>
		/// The contents are backed by a temp file on the file system.
		/// The file is deleted when the archive is disposed.
		/// </summary>
		FileSystem
	}
}
