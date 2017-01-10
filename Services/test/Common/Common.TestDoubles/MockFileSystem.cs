using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.System;

namespace CSC.Common.TestDoubles
{
	/// <summary>
	/// A mock file system that stores files and 
	/// directories in memory.
	/// </summary>
	public class MockFileSystem : IFileSystem
	{
		/// <summary>
		/// The folders in the file system. Each folder
		/// contains a dictionary of the names and contents
		/// of each file.
		/// </summary>
		public IDictionary<string, IDictionary<string, string>> Folders { get; }
			= new Dictionary<string, IDictionary<string, string>>();

		/// <summary>
		/// Creates a new temporary file, and returns the corresponding
		/// stream. The file is deleted when the stream is closed.
		/// </summary>
		public Stream CreateNewTempFile()
		{
			return new MemoryStream();
		}

		/// <summary>
		/// Reads the contents of a file.
		/// </summary>
		public Task<string> ReadFileContentsAsync(string path)
		{
			return Task.FromResult
			(
				Folders[GetFolderName(path)][GetFileName(path)]
			);
		}

		/// <summary>
		/// Writes contents to a file.
		/// </summary>
		public Task WriteFileContentsAsync(string path, string contents)
		{
			Folders[GetFolderName(path)][GetFileName(path)] = contents;

			return Task.FromResult(true);
		}

		/// <summary>
		/// Creates the given folder.
		/// </summary>
		public void CreateFolder(string folderName)
		{
			Folders[folderName] = new Dictionary<string, string>();
		}

		/// <summary>
		/// Deletes the given folder.
		/// </summary>
		public void DeleteFolder(string folderName)
		{
			Folders.Remove(folderName);
		}

		/// <summary>
		/// Returns whether or not the given folder exists.
		/// </summary>
		public bool FolderExists(string folderName)
		{
			return Folders.ContainsKey(folderName);
		}

		/// <summary>
		/// Returns the folder name in the path.
		/// </summary>
		private string GetFolderName(string path)
		{
			var lastSlash = path.LastIndexOf("/");

			return path.Substring(0, lastSlash);
		}

		/// <summary>
		/// Returns the file name in the path.
		/// </summary>
		private string GetFileName(string path)
		{
			var lastSlash = path.LastIndexOf("/");

			return path.Substring(lastSlash + 1);
		}
	}
}
