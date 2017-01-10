namespace CSC.Common.Infrastructure.System
{
	/// <summary>
	/// An uncompressed memory-backed archive file.
	/// </summary>
	public class UncompressedMemoryArchiveFile : ArchiveFile
	{
		/// <summary>
		/// The full path of the file in the archive.
		/// </summary>
		public override string FullPath { get; }

		/// <summary>
		/// The raw data of the file.
		/// </summary>
		private readonly byte[] _rawData;

		/// <summary>
		/// Constructor.
		/// </summary>
		public UncompressedMemoryArchiveFile(string fullPath, byte[] rawData)
		{
			FullPath = fullPath;
			_rawData = rawData;
		}

		/// <summary>
		/// The raw data.
		/// </summary>
		public override byte[] GetRawData() => _rawData;
	}
}
