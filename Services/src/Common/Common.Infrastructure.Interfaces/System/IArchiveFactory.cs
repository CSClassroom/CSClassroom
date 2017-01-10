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
	public interface IArchiveFactory
	{
		/// <summary>
		/// Creates an uncompressed archive.
		/// </summary>
		/// <param name="files">The set of files to include.</param>
		IArchive CreateUncompressedArchive(
			IDictionary<string, byte[]> files);

		/// <summary>
		/// Creates a compressed archive.
		/// </summary>
		/// <param name="zipArchive">The underlying zip archive.</param>
		/// <param name="stripInitialFolders">The number of folders
		/// to strip from the path of all entries in the archive.</param>
		IArchive CreateCompressedArchive(
			ZipArchive zipArchive,
			int stripInitialFolders);
	}
}
