using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace CSC.Common.Infrastructure.System
{
	/// <summary>
	/// Creates archive files.
	/// </summary>
	public class ArchiveFactory : IArchiveFactory
	{
		/// <summary>
		/// Creates an uncompressed archive.
		/// </summary>
		/// <param name="files">The set of files to include.</param>
		public IArchive CreateUncompressedArchive(
			IDictionary<string, byte[]> files)
		{
			return new UncompressedMemoryArchive(files);
		}

		/// <summary>
		/// Creates a compressed archive.
		/// </summary>
		/// <param name="zipArchive">The underlying zip archive.</param>
		/// <param name="stripInitialFolders">The number of folders
		/// to strip from the path of all entries in the archive.</param>
		public IArchive CreateCompressedArchive(
			ZipArchive zipArchive,
			int stripInitialFolders)
		{
			return new CompressedArchive(zipArchive, stripInitialFolders);
		}
	}
}
