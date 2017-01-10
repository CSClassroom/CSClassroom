using System;
using System.Collections.Generic;
using System.Linq;

namespace CSC.Common.Infrastructure.System
{
	/// <summary>
	/// An uncompressed archive stored in memory.
	/// </summary>
	public class UncompressedMemoryArchive : IArchive
	{
		/// <summary>
		/// The files in the archive.
		/// </summary>
		private IList<IArchiveFile> _contents;

		/// <summary>
		/// Constructor.
		/// </summary>
		public UncompressedMemoryArchive(IDictionary<string, byte[]> files)
		{
			_contents = files
				.Select(kvp => new UncompressedMemoryArchiveFile(kvp.Key, kvp.Value))
				.Cast<IArchiveFile>()
				.ToList();
		}

		/// <summary>
		/// The files in the archive.
		/// </summary>
		public IList<IArchiveFile> Files
		{
			get
			{
				if (_contents == null)
					throw new ObjectDisposedException("ZipFile");

				return _contents;
			}
		}

		/// <summary>
		/// Disposes of the archive.
		/// </summary>
		public void Dispose()
		{
			_contents = null;
		}
	}
}
