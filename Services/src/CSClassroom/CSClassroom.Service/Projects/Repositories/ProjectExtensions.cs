using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CSC.Common.Infrastructure.System;
using CSC.CSClassroom.Model.Projects;
using CSC.CSClassroom.Model.Projects.ServiceResults;

namespace CSC.CSClassroom.Service.Projects.Repositories
{
	/// <summary>
	/// Extension methods for the Project class.
	/// </summary>
	public static class ProjectExtensions
	{
		/// <summary>
		/// Returns the file type of the given file.
		/// </summary>
		public static FileType GetFileType(this Project project, IArchiveFile entry)
		{
			if (DoesFileMatch(entry, project.PrivateFilePaths, path => path.Path))
			{
				return FileType.Private;
			}
			else if (DoesFileMatch(entry, project.ImmutableFilePaths, path => path.Path)
				|| entry.FullPath.Contains(".gitignore"))
			{
				return FileType.Immutable;
			}
			else
			{
				return FileType.Public;
			}
		}

		/// <summary>
		/// Returns the regular expression for the given path.
		/// </summary>
		private static bool DoesFileMatch<TPath>(
			IArchiveFile entry,
			IList<TPath> pathExpressions,
			Func<TPath, string> getPathExpression)
		{
			return pathExpressions.Select
			(
				path => getPathExpression(path)
					.Replace("*", ".*")
					.Replace("/", "\\/")
			).Any
			(
				exp => Regex.Match(entry.FullPath, exp).Success
			);
		}
	}
}
