using System.IO;
using System.Threading.Tasks;

namespace CSC.Common.Infrastructure.System
{
	/// <summary>
	/// Provides access to the file system.
	/// </summary>
	public interface IFileSystem
	{
		/// <summary>
		/// Creates a new temporary file, and returns the corresponding
		/// stream. The file is deleted when the stream is closed.
		/// </summary>
		Stream CreateNewTempFile();

		/// <summary>
		/// Reads the contents of a file.
		/// </summary>
		Task<string> ReadFileContentsAsync(string path);

		/// <summary>
		/// Writes contents to a file.
		/// </summary>
		Task WriteFileContentsAsync(string path, string contents);

		/// <summary>
		/// Creates the given folder.
		/// </summary>
		void CreateFolder(string path);

		/// <summary>
		/// Deletes the given folder.
		/// </summary>
		void DeleteFolder(string path);
	}
}
