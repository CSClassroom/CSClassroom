namespace CSC.Common.Infrastructure.System
{
	/// <summary>
	/// An file in a repository.
	/// </summary>
	public interface IArchiveFile
	{
		/// <summary>
		/// Whether or not this is an ASCII file.
		/// </summary>
		bool Ascii { get; }

		/// <summary>
		/// The full path of the file in the archive.
		/// </summary>
		string FullPath { get; }

		/// <summary>
		/// Returns the raw data.
		/// </summary>
		byte[] GetRawData();

		/// <summary>
		/// Returns the data in string form. For non-ascii file,
		/// the contents are base64-encoded.
		/// </summary>
		string GetEncodedData();
	}
}
