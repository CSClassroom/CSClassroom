using System;
using System.IO;
using System.IO.Compression;
using CSC.Common.Infrastructure.Extensions;

namespace CSC.Common.Infrastructure.System
{
	/// <summary>
	/// Implementation of IRepositoryFile, backed by a ZipArchiveEntry.
	/// </summary>
	public class CompressedArchiveFile : ArchiveFile
	{
		/// <summary>
		/// The entry.
		/// </summary>
		private readonly ZipArchiveEntry _entry;

		/// <summary>
		/// The number of initial folders to strip when
		/// returning the file path.
		/// </summary>
		private readonly int _stripInitialFolders;

		/// <summary>
		/// The full path of the entry in the zip file.
		/// </summary>
		public override string FullPath => StripInitialFolders
		(
			_entry.FullName, 
			_stripInitialFolders
		);

		/// <summary>
		/// Constructor.
		/// </summary>
		public CompressedArchiveFile(ZipArchiveEntry entry, int stripInitialFolders)
		{
			if (!entry.IsFile())
				throw new InvalidOperationException("Not a file.");

			_entry = entry;
			_stripInitialFolders = stripInitialFolders;
		}

		/// <summary>
		/// The raw data.
		/// </summary>
		public override byte[] GetRawData()
		{
			using (MemoryStream rawDataStream = new MemoryStream())
			using (Stream stream = _entry.Open())
			{
				stream.CopyTo(rawDataStream);

				return rawDataStream.ToArray();
			}
		}
		
		/// <summary>
		/// Returns the path of an entry in the repository archive.
		/// </summary>
		public static string StripInitialFolders(string path, int numFoldersToStrip)
		{
			for (int i = 0; i < numFoldersToStrip; i++)
			{
				path = path.Substring(path.IndexOf("/") + 1);
			}

			return path;
		}
	}
}
