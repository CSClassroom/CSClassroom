using System.IO;
using System.Linq;
using System.Xml.Linq;
using CSC.Common.Infrastructure.System;
using CSC.CSClassroom.Model.Projects;
using CSC.CSClassroom.Model.Users;

namespace CSC.CSClassroom.Service.Projects.Submissions
{
	/// <summary>
	/// Applies transformations to submitted files, if applicable.
	/// </summary>
	public class SubmissionFileTransformer : ISubmissionFileTransformer
	{
		/// <summary>
		/// Returns the file's contents, with any applicable 
		/// transformations applied.
		/// </summary>
		public byte[] GetFileContents(
			Project project,
			ClassroomMembership student,
			IArchiveFile entry)
		{
			if (entry.FullPath.EndsWith(".project"))
			{
				var newContents = entry.GetEncodedData().Replace
				(
					$"<name>{project.Name}</name>",
					$"<name>{project.Name}_{student.GitHubTeam}</name>"
				);

				using (var memoryStream = new MemoryStream())
				using (var streamWriter = new StreamWriter(memoryStream))
				{
					streamWriter.Write(newContents);
					streamWriter.Flush();

					return memoryStream.ToArray();
				}
			}
			else if (entry.FullPath.EndsWith(".classpath"))
			{
				var jUnitPath = "org.eclipse.jdt.junit.JUNIT_CONTAINER/4";
				using (var stream = new MemoryStream(entry.GetRawData()))
				{
					var projectNode = XElement.Load(stream);
					var hasJUnit = projectNode
						.Elements(XName.Get("classpathentry"))
						.Any
						(
							   elt => elt.Attribute(XName.Get("path")) != null
							&& elt.Attribute(XName.Get("path")).Value == jUnitPath
						);

					if (!hasJUnit)
						projectNode.Add(XElement.Parse($"<classpathentry kind=\"con\" path=\"{jUnitPath}\"/>"));

					using (var newStream = new MemoryStream())
					{
						projectNode.Save(newStream);
						newStream.Flush();

						return newStream.ToArray();
					}
				}
			}
			else
			{
				return entry.GetRawData();
			}
		}
	}
}
