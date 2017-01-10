using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using CSC.Common.Infrastructure.Extensions;

namespace CSC.Common.Infrastructure.System
{
	/// <summary>
	/// An archive of files, backed by a ZipArchive.
	/// </summary>
	public class CompressedArchive : IArchive
	{
		/// <summary>
		/// The underlying zip archive.
		/// </summary>
		private readonly ZipArchive _zipArchive;

		/// <summary>
		/// The contents of the archive.
		/// </summary>
		private IList<IArchiveFile> _contents;

		/// <summary>
		/// Constructor.
		/// </summary>
		public CompressedArchive(ZipArchive zipArchive, int stripInitialFolders)
		{
			_zipArchive = zipArchive;
			_contents = zipArchive.Entries
				.Where(entry => entry.IsFile())
				.Select(entry => new CompressedArchiveFile(entry, stripInitialFolders))
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
			_zipArchive.Dispose();
		}
	}
}
