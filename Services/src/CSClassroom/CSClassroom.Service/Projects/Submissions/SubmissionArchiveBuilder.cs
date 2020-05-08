using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.System;
using CSC.CSClassroom.Model.Projects;
using CSC.CSClassroom.Model.Projects.ServiceResults;
using CSC.CSClassroom.Model.Users;
using CSC.CSClassroom.Service.Projects.Repositories;

namespace CSC.CSClassroom.Service.Projects.Submissions
{
	/// <summary>
	/// Builds an archive containing all student submissions.
	/// </summary>
	public class SubmissionArchiveBuilder : ISubmissionArchiveBuilder
	{
		/// <summary>
		/// Transforms student files, if applicable.
		/// </summary>
		private readonly ISubmissionFileTransformer _transformer;

		/// <summary>
		/// The file system.
		/// </summary>
		private readonly IFileSystem _fileSystem;

		/// <summary>
		/// Constructor.
		/// </summary>
		public SubmissionArchiveBuilder(
			ISubmissionFileTransformer transformer,
			IFileSystem fileSystem)
		{
			_transformer = transformer;
			_fileSystem = fileSystem;
		}

		/// <summary>
		/// Builds a submission archive containing the submissions of
		/// all students.
		/// </summary>
		public async Task<Stream> BuildSubmissionArchiveAsync(
			Project project,
			IArchive templateContents,
			IList<StudentSubmission> submissions,
			bool includeEclipseProjects,
			bool includeFlatFiles)
		{
			var stream = _fileSystem.CreateNewTempFile();

			using (ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
			{
				foreach (var result in submissions)
				{
					await WriteSubmissionToArchiveAsync
					(
						archive,
						project,
						result.Student,
						templateContents,
						result.Contents,
						includeEclipseProjects,
						true
					);

					result.Contents.Dispose();
				}
			}

			stream.Position = 0;
			return stream;
		}

		/// <summary>
		/// Writes the contents of a submission to an archive.
		/// </summary>
		private async Task WriteSubmissionToArchiveAsync(
			ZipArchive archive,
			Project project,
			ClassroomMembership student,
			IArchive templateContents,
			IArchive submissionContents,
			bool includeEclipseProjects,
			bool includeFlatFiles)
		{
			// Submissions are grouped by section.  If a student is in multiple
			// sections, just grab the first one
			string sectionName = student.SectionMemberships.First().Section.Name;

			var studentFolder = $"EclipseProjects\\{sectionName}\\{student.GitHubTeam}";

			// The project will contain all non-immutable submission files, 
			// plus all immutable and private files from the template project.

			var projectContents = submissionContents.Files
				.Where
				(
					entry => project.GetFileType(entry) == FileType.Public
				)
				.Concat
				(
					templateContents.Files.Where
					(
						entry => project.GetFileType(entry) != FileType.Public
					)
				)
				.ToList();

			foreach (var entry in projectContents)
			{
				if (ExcludeEntry(project, entry))
					continue;

				var contents = _transformer.GetFileContents(project, student, entry);
				var archiveFilePath = entry.FullPath;
				var fileName = archiveFilePath.Substring(archiveFilePath.LastIndexOf("/") + 1);

				if (includeEclipseProjects)
				{
					var archiveFileFolder = archiveFilePath.Contains("/")
						? archiveFilePath.Substring(0, archiveFilePath.LastIndexOf("/"))
						: archiveFilePath;

					var localFileFolder = $"{studentFolder}\\{archiveFileFolder}";
					var localFilePath = $"{localFileFolder}\\{fileName}";

					// Add the file to the student project folder.
					var projectFolderEntry = archive.CreateEntry(localFilePath);
					using (Stream stream = projectFolderEntry.Open())
					{
						await stream.WriteAsync(contents, offset: 0, count: contents.Length);
					}
				}

				// Add the file to the folder containing all files, if applicable.
				if (includeFlatFiles && fileName.EndsWith(".java") && project.GetFileType(entry) == FileType.Public)
				{
					// Files are grouped by base name, and then by section
					var baseFileName = fileName.Substring(0, fileName.LastIndexOf("."));
					var allFilesEntry = archive.CreateEntry($"AllFiles\\{baseFileName}\\{sectionName}\\{student.GitHubTeam}-{fileName}");
					using (Stream stream = allFilesEntry.Open())
					{
						await stream.WriteAsync(contents, offset: 0, count: contents.Length);
					}
				}
			}
		}

		/// <summary>
		/// Should we exclude this entry?
		/// </summary>
		private bool ExcludeEntry(Project project, IArchiveFile entry)
		{
			if (entry.FullPath.EndsWith(".project")
				&& !entry.FullPath.EndsWith($"{project.Name}/.project"))
			{
				return true;
			}

			return false;
		}
	}
}
