using CSC.Common.Infrastructure.System;
using CSC.CSClassroom.Model.Projects;
using CSC.CSClassroom.Model.Users;

namespace CSC.CSClassroom.Service.Projects.Submissions
{
	/// <summary>
	/// Applies transformations to submitted files, if applicable.
	/// </summary>
	public interface ISubmissionFileTransformer
	{
		/// <summary>
		/// Returns the file's contents, with any applicable 
		/// transformations applied.
		/// </summary>
		byte[] GetFileContents(
			Project project,
			ClassroomMembership student,
			IArchiveFile entry);
	}
}
