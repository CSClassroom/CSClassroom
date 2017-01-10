using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace CSC.Common.Infrastructure.System
{
	/// <summary>
	/// Provides access to the file system.
	/// </summary>
	public class FileSystem : IFileSystem
	{
		/// <summary>
		/// The buffer size for a file stream.
		/// </summary>
		private const int c_bufferSize = 4000;

		/// <summary>
		/// Creates a new temporary file, and returns the corresponding
		/// stream. The file is deleted when the stream is closed.
		/// </summary>
		public Stream CreateNewTempFile()
		{
			return new FileStream
			(
				Path.GetTempFileName(),
				FileMode.OpenOrCreate,
				FileAccess.ReadWrite,
				FileShare.ReadWrite,
				c_bufferSize,
				FileOptions.DeleteOnClose
			);
		}

		/// <summary>
		/// Reads the contents of a file.
		/// </summary>
		public async Task<string> ReadFileContentsAsync(string path)
		{
			var stream = new FileStream
			(
				path,
				FileMode.Open,
				FileAccess.Read,
				FileShare.Read
			);

			using (var streamReader = new StreamReader(stream))
			{
				return await streamReader.ReadToEndAsync();
			}
		}

		/// <summary>
		/// Writes contents to a file.
		/// </summary>
		public async Task WriteFileContentsAsync(string path, string contents)
		{
			var stream = new FileStream
			(
				path,
				FileMode.Create,
				FileAccess.Write,
				FileShare.Read
			);

			using (var streamWriter = new StreamWriter(stream))
			{
				await streamWriter.WriteAsync(contents);
			}
		}

		/// <summary>
		/// Creates the given folder.
		/// </summary>
		public void CreateFolder(string path)
		{
			Directory.CreateDirectory(path);
		}

		/// <summary>
		/// Deletes the given folder.
		/// </summary>
		public void DeleteFolder(string path)
		{
			Directory.Delete(path, recursive: true);
		}
	}
}
