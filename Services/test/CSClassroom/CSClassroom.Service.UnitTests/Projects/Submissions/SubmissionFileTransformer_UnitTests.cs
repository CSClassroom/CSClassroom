using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using CSC.Common.Infrastructure.System;
using CSC.CSClassroom.Model.Projects;
using CSC.CSClassroom.Model.Users;
using CSC.CSClassroom.Service.Projects.Submissions;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Projects.Submissions
{
	/// <summary>
	/// Unit tests for the SubmissionFileTransformer class.
	/// </summary>
	public class SubmissionFileTransformer_UnitTests
	{
		/// <summary>
		/// Ensures that a normal file is not transformed.
		/// </summary>
		[Fact]
		public void GetFileContents_NormalFile_NotTransformed()
		{
			var project = GetProject();
			var student = GetStudent();

			var file = new UncompressedMemoryArchiveFile
			(
				"Project1\\SomeNormalFile.txt",
				Encoding.UTF8.GetBytes
				(
					"SomeNormalFile"
				)
			);

			var transformer = new SubmissionFileTransformer();

			var result = transformer.GetFileContents(project, student, file);
			var decodedResult = Encoding.UTF8.GetString(result);

			Assert.Equal("SomeNormalFile", decodedResult);
		}

		/// <summary>
		/// Ensures that the .project file is modified to have the
		/// student's name pre-pended to the project name.
		/// </summary>
		[Fact]
		public void GetFileContents_ProjectFile_ProjectRenamed()
		{
			var project = GetProject();
			var student = GetStudent();

			var file = new UncompressedMemoryArchiveFile
			(
				"Project1\\.project",
				Encoding.UTF8.GetBytes
				(
					"Before<name>Project1</name>After"
				)
			);

			var transformer = new SubmissionFileTransformer();

			var result = transformer.GetFileContents(project, student, file);
			var decodedResult = Encoding.UTF8.GetString(result);

			Assert.Equal("Before<name>Project1_LastNameFirstName</name>After", decodedResult);
		}

		/// <summary>
		/// Ensures that JUnit is added to the project's classpath.
		/// </summary>
		[Fact]
		public void GetFileContents_ClassPathWithoutJUnit_JUnitAdded()
		{
			var project = GetProject();
			var student = GetStudent();

			var file = new UncompressedMemoryArchiveFile
			(
				"Project1\\.classpath",
				GetClasspathFile(includeJUnit: false)
			);

			var transformer = new SubmissionFileTransformer();

			var result = transformer.GetFileContents(project, student, file);
			var decodedResult = Encoding.UTF8.GetString(result);
			var parsedResult = XElement.Parse(decodedResult.Substring(decodedResult.IndexOf("<")));
			var junitNodes = parsedResult.Elements()
				.Where(node => node.Name == "classpathentry")
				.Where
				(
					node => node.Attribute("path")
								?.Value == "org.eclipse.jdt.junit.JUNIT_CONTAINER/4"
				).ToList();

			Assert.Single(junitNodes);
		}
		
		/// <summary>
		/// Ensures that JUnit is not added to the project's classpath
		/// when it is already present.
		/// </summary>
		[Fact]
		public void GetFileContents_ClassPathWithJUnit_JUnitNotAdded()
		{
			var project = GetProject();
			var student = GetStudent();

			var file = new UncompressedMemoryArchiveFile
			(
				"Project1\\.classpath",
				GetClasspathFile(includeJUnit: true)
			);

			var transformer = new SubmissionFileTransformer();

			var result = transformer.GetFileContents(project, student, file);
			var decodedResult = Encoding.UTF8.GetString(result);
			var parsedResult = XElement.Parse(decodedResult.Substring(decodedResult.IndexOf("<")));
			var junitNodes = parsedResult.Elements()
				.Where(node => node.Name == "classpathentry")
				.Where
				(
					node => node.Attribute("path")
								?.Value == "org.eclipse.jdt.junit.JUNIT_CONTAINER/4"
				).ToList();

			Assert.Single(junitNodes);
		}

		/// <summary>
		/// Returns a student.
		/// </summary>
		private ClassroomMembership GetStudent()
		{
			return new ClassroomMembership()
			{
				GitHubTeam = "LastNameFirstName"
			};
		}

		/// <summary>
		/// Returns a project.
		/// </summary>
		private Project GetProject()
		{
			return new Project()
			{
				Name = "Project1"
			};
		}

		/// <summary>
		/// Returns a classpath file.
		/// </summary>
		private byte[] GetClasspathFile(bool includeJUnit)
		{
			using (MemoryStream stream = new MemoryStream())
			{
				var root = new XElement
				(
					"classpath",
					new XElement
					(
						"classpathentry",
						new XAttribute("kind", "con"),
						new XAttribute("path", "some.path")
					),

					includeJUnit
						? new XElement
						(
							"classpathentry",
							new XAttribute("kind", "con"),
							new XAttribute("path", "org.eclipse.jdt.junit.JUNIT_CONTAINER/4")
						)
						: new XElement
						(
							"classpathentry",
							new XAttribute("kind", "con"),
							new XAttribute("path", "some.other.path")
						)
				);

				root.Save(stream, new SaveOptions() { });

				return stream.ToArray();
			}
		}
	}
}
