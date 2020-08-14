using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.Projects.Submissions;
using CSC.Common.Infrastructure.System;
using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Model.Projects;
using CSC.CSClassroom.Model.Users;
using CSC.CSClassroom.Service.Projects.Submissions;
using CSC.CSClassroom.Service.UnitTests.TestDoubles;
using CSC.CSClassroom.Service.UnitTests.Utilities;
using Moq;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Projects.Submissions
{
	/// <summary>
	/// Unit tests for the SubmissionArchiveBuilder class.
	/// </summary>
	public class SubmissionArchiveBuilder_UnitTests
	{
		/// <summary>
		/// Ensures that public files are taken from the submission
		/// (instead of the template).
		/// </summary>
		[Fact]
		public async Task BuildSubmissionArchiveAsync_PublicFile_TakenFromSubmission()
		{
			var classroom = GetClassroom();
			var project = GetProject(classroom);
			var student = GetStudent(classroom, "Student1", "Period1");

			var templateContents = GetArchive
			(
				new RepositoryFile("Public/PublicFile1.java", "Original")
			);

			var submissionContents = GetArchive
			(
				new RepositoryFile("Public/PublicFile1.java", "ModifiedByStudent")
			);

			var archiveBuilder = GetSubmissionArchiveBuilder();

			var archive = await archiveBuilder.BuildSubmissionArchiveAsync
			(
				project,
				templateContents,
				GetSubmissions(GetSubmission(student, submissionContents)),
				ProjectSubmissionDownloadFormat.All
			);

			var archiveContents = GetArchiveContents(archive);

			Assert.Equal(2, archiveContents.Count);

			Assert.Equal
			(
				"TransformedModifiedByStudent", 
				archiveContents["EclipseProjects\\Period1\\Student1\\Public\\PublicFile1.java"]
			);

			Assert.Equal
			(
				"TransformedModifiedByStudent",
				archiveContents["AllFiles\\PublicFile1\\Period1\\Student1-PublicFile1.java"]
			);
		}

		/// <summary>
		/// Ensures that immutable files are taken from the template
		/// (instead of the submission).
		/// </summary>
		[Fact]
		public async Task BuildSubmissionArchiveAsync_ImmutableFile_TakenFromTemplate()
		{
			var classroom = GetClassroom();
			var project = GetProject(classroom);
			var student = GetStudent(classroom, "Student1", "Period1");

			var templateContents = GetArchive
			(
				new RepositoryFile("Immutable/ImmutableFile1.java", "Original")
			);

			var submissionContents = GetArchive
			(
				new RepositoryFile("Immutable/ImmutableFile1.java", "ModifiedByStudent")
			);

			var archiveBuilder = GetSubmissionArchiveBuilder();

			var archive = await archiveBuilder.BuildSubmissionArchiveAsync
			(
				project,
				templateContents,
				GetSubmissions(GetSubmission(student, submissionContents)),
				ProjectSubmissionDownloadFormat.All
			);

			var archiveContents = GetArchiveContents(archive);

			Assert.Equal(1, archiveContents.Count);

			Assert.Equal
			(
				"TransformedOriginal",
				archiveContents["EclipseProjects\\Period1\\Student1\\Immutable\\ImmutableFile1.java"]
			);
		}

		/// <summary>
		/// Ensures that private files are taken from the template
		/// (instead of the submission).
		/// </summary>
		[Fact]
		public async Task BuildSubmissionArchiveAsync_PrivateFile_TakenFromTemplate()
		{
			var classroom = GetClassroom();
			var project = GetProject(classroom);
			var student = GetStudent(classroom, "Student1", "Period1");

			var templateContents = GetArchive
			(
				new RepositoryFile("Private/PrivateFile1.java", "Original")
			);

			var submissionContents = GetArchive
			(
				new RepositoryFile("Private/PrivateFile1.java", "ModifiedByStudent")
			);

			var archiveBuilder = GetSubmissionArchiveBuilder();

			var archive = await archiveBuilder.BuildSubmissionArchiveAsync
			(
				project,
				templateContents,
				GetSubmissions(GetSubmission(student, submissionContents)),
				ProjectSubmissionDownloadFormat.All
			);

			var archiveContents = GetArchiveContents(archive);

			Assert.Equal(1, archiveContents.Count);

			Assert.Equal
			(
				"TransformedOriginal",
				archiveContents["EclipseProjects\\Period1\\Student1\\Private\\PrivateFile1.java"]
			);
		}
		
		/// <summary>
		/// Ensures that only public java files will be placed in the AllFiles directory
		/// of the archive.
		/// </summary>
		[Fact]
		public async Task BuildSubmissionArchiveAsync_OnlyPublicJavaFilesInAllFiles()
		{
			var classroom = GetClassroom();
			var project = GetProject(classroom);
			var student = GetStudent(classroom, "Student1", "Period1");

			var templateContents = GetArchive
			(
				new RepositoryFile("Private/PrivateFile1.java", "Original"),
				new RepositoryFile("Immutable/ImmutableFile1.java", "Original"),
				new RepositoryFile("Public/PublicFile1.java", "Original"),
				new RepositoryFile("Public/PublicFile2.txt", "Original")
			);

			var submissionContents = GetArchive
			(
				new RepositoryFile("Private/PrivateFile1.java", "ModifiedByStudent"),
				new RepositoryFile("Immutable/ImmutableFile1.java", "ModifiedByStudent"),
				new RepositoryFile("Public/PublicFile1.java", "ModifiedByStudent"),
				new RepositoryFile("Public/PublicFile2.txt", "ModifiedByStudent")
			);

			var archiveBuilder = GetSubmissionArchiveBuilder();

			var archive = await archiveBuilder.BuildSubmissionArchiveAsync
			(
				project,
				templateContents,
				GetSubmissions(GetSubmission(student, submissionContents)),
				ProjectSubmissionDownloadFormat.All
			);

			var archiveContents = GetArchiveContents(archive);

			Assert.Equal(1, archiveContents.Count(f => f.Key.StartsWith("AllFiles")));

			Assert.Equal
			(
				"TransformedModifiedByStudent",
				archiveContents["AllFiles\\PublicFile1\\Period1\\Student1-PublicFile1.java"]
			);
		}

		/// <summary>
		/// Ensures the download format parameter is respected
		/// </summary>
		[Fact]
		public async Task BuildSubmissionArchiveAsync_DownloadFormat()
		{
			var classroom = GetClassroom();
			var project = GetProject(classroom);
			var student = GetStudent(classroom, "Student1", "Period1");

			// Call BuildSubmissionArchiveAsync with each valid format
			foreach (ProjectSubmissionDownloadFormat format in 
					 Enum.GetValues(typeof(ProjectSubmissionDownloadFormat)))
			{
				var templateContents = GetArchive
				(
					new RepositoryFile("Private/PrivateFile1.java", "Original"),
					new RepositoryFile("Immutable/ImmutableFile1.java", "Original"),
					new RepositoryFile("Public/PublicFile1.java", "Original"),
					new RepositoryFile("Public/PublicFile2.txt", "Original")
				);

				var submissionContents = GetArchive
				(
					new RepositoryFile("Private/PrivateFile1.java", "ModifiedByStudent"),
					new RepositoryFile("Immutable/ImmutableFile1.java", "ModifiedByStudent"),
					new RepositoryFile("Public/PublicFile1.java", "ModifiedByStudent"),
					new RepositoryFile("Public/PublicFile2.txt", "ModifiedByStudent")
				);

				var archiveBuilder = GetSubmissionArchiveBuilder();

				var archive = await archiveBuilder.BuildSubmissionArchiveAsync
				(
					project,
					templateContents,
					GetSubmissions(GetSubmission(student, submissionContents)),
					format
				);

				var archiveContents = GetArchiveContents(archive);

				Assert.Equal(
					(
							format == ProjectSubmissionDownloadFormat.All 
						 || format == ProjectSubmissionDownloadFormat.Eclipse
					) ? 4 : 0,
					archiveContents.Count(f => f.Key.StartsWith("EclipseProjects")));
				Assert.Equal(
					(
							format == ProjectSubmissionDownloadFormat.All
						 || format == ProjectSubmissionDownloadFormat.Flat
					) ? 1 : 0,
					archiveContents.Count(f => f.Key.StartsWith("AllFiles")));
			}
		}

		/// <summary>
		/// Returns a classroom.
		/// </summary>
		private Classroom GetClassroom()
		{
			return new Classroom()
			{
				Name = "Class1",
				GitHubOrganization = "GitHubOrg"
			};
		}

		/// <summary>
		/// Returns a project.
		/// </summary>
		private Project GetProject(Classroom classroom)
		{
			return new Project()
			{
				Name = "Project1",
				Classroom = classroom,
				ClassroomId = classroom.Id,
				ImmutableFilePaths = Collections.CreateList
				(
					new ImmutableFilePath() { Path = "Immutable" }
				),
				PrivateFilePaths = Collections.CreateList
				(
					new PrivateFilePath() { Path = "Private" }
				)
			};
		}

		/// <summary>
		/// Returns a user.
		/// </summary>
		private ClassroomMembership GetStudent(Classroom classroom, string team, string sectionName)
		{
			return new ClassroomMembership()
			{
				Classroom = classroom,
				ClassroomId = classroom.Id,
				GitHubTeam = team,
				SectionMemberships = Collections.CreateList
				(
					new SectionMembership()
					{
						Section = new Section()
						{
							Name = sectionName
						}
					}
				)
			};
		}

		/// <summary>
		/// Returns a new student submission.
		/// </summary>
		private StudentSubmission GetSubmission(
			ClassroomMembership student,
			IArchive contents)
		{
			return new StudentSubmission(student, contents);
		}

		/// <summary>
		/// Returns a list of student submissions.
		/// </summary>
		private StudentSubmissions GetSubmissions(
			params StudentSubmission[] submissions)
		{
			return new StudentSubmissions(submissions);
		}

		/// <summary>
		/// Returns a mock submission file transformer.
		/// </summary>
		private ISubmissionFileTransformer GetSubmissionFileTransformer()
		{
			var transformer = new Mock<ISubmissionFileTransformer>();

			transformer
				.Setup
				(
					t => t.GetFileContents
					(
						It.IsAny<Project>(),
						It.IsAny<ClassroomMembership>(),
						It.IsAny<IArchiveFile>()
					)
				).Returns<Project, ClassroomMembership, IArchiveFile>
				(
					(project, student, file) => Encoding.ASCII.GetBytes
					(
						$"Transformed{file.GetEncodedData()}"
					) 
				);

			return transformer.Object;
		}

		/// <summary>
		/// Returns a new submission archive builder.
		/// </summary>
		private ISubmissionArchiveBuilder GetSubmissionArchiveBuilder()
		{
			return new SubmissionArchiveBuilder
			(
				GetSubmissionFileTransformer(), 
				GetMockFileSystem()
			);
		}

		/// <summary>
		/// Returns a mock file system.
		/// </summary>
		private IFileSystem GetMockFileSystem()
		{
			var fileSystem = new Mock<IFileSystem>();

			fileSystem
				.Setup(fs => fs.CreateNewTempFile())
				.Returns(() => new MemoryStream());

			return fileSystem.Object;
		}

		/// <summary>
		/// Returns an archive with the given files.
		/// </summary>
		private IArchive GetArchive(params RepositoryFile[] files)
		{
			return new UncompressedMemoryArchive
			(
				files.ToDictionary
				(
					file => file.Path,
					file => file.GetRawData()
				)
			);
		}

		/// <summary>
		/// Returns the contents of an archive.
		/// </summary>
		private IDictionary<string, string> GetArchiveContents(Stream stream)
		{
			var archive = new ZipArchive(stream);
			using (var result = new CompressedArchive(archive, stripInitialFolders: 0))
			{
				return result.Files.ToDictionary
				(
					file => file.FullPath,
					file => Encoding.ASCII.GetString(file.GetRawData())
				);
			}
		}

		/// <summary>
		/// A file in a repository.
		/// </summary>
		public class RepositoryFile
		{
			/// <summary>
			/// The path to the file.
			/// </summary>
			public string Path { get; }

			/// <summary>
			/// The contents of the file.
			/// </summary>
			public string Contents { get; }

			/// <summary>
			/// Constructor.
			/// </summary>
			public RepositoryFile(string path, string contents)
			{
				Path = path;
				Contents = contents;
			}

			/// <summary>
			/// Returns the raw data of the file.
			/// </summary>
			public byte[] GetRawData()
			{
				return Encoding.ASCII.GetBytes(Contents);
			}
		}
	}
}
