using System;
using System.IO;
using System.Threading;

namespace CSC.Common.Infrastructure.System
{
	/// <summary>
	/// An archive file.
	/// </summary>
	public abstract class ArchiveFile : IArchiveFile
	{
		/// <summary>
		/// Whether or not the file is ASCII.
		/// </summary>
		private readonly Lazy<bool> _ascii;

		/// <summary>
		/// Whether or not the file is ASCII.
		/// </summary>
		public bool Ascii => _ascii.Value;

		/// <summary>
		/// The full path of the entry in the zip file.
		/// </summary>
		public abstract string FullPath { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		protected ArchiveFile()
		{
			_ascii = new Lazy<bool>
			(
				IsAscii, 
				LazyThreadSafetyMode.ExecutionAndPublication
			);
		}

		/// <summary>
		/// The raw data.
		/// </summary>
		public abstract byte[] GetRawData();

		/// <summary>
		/// Returns whether or not the file is ascii.
		/// </summary>
		private bool IsAscii()
		{
			var rawData = GetRawData();
			for (int i = 0; i < rawData.Length; i++)
			{
				if (rawData[i] == 0 || rawData[i] > 127)
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// The contents (base64 encoded if not ascii)
		/// </summary>
		public string GetEncodedData()
		{
			if (Ascii)
			{
				using (var stream = new MemoryStream(GetRawData()))
				{
					using (var textReader = new StreamReader(stream))
					{
						return textReader.ReadToEnd();
					}
				}
			}
			else
			{
				return Convert.ToBase64String(GetRawData());
			}
		}
	}
}
