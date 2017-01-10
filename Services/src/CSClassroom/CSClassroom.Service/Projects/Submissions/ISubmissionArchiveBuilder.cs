using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.System;
using CSC.CSClassroom.Model.Projects;

namespace CSC.CSClassroom.Service.Projects.Submissions
{
	/// <summary>
	/// Builds an archive containing all student submissions.
	/// </summary>
	public interface ISubmissionArchiveBuilder
	{
		/// <summary>
		/// Builds a submission archive containing the submissions of
		/// all students.
		/// </summary>
		Task<Stream> BuildSubmissionArchiveAsync(
			Project project,
			IArchive templateContents,
			IList<StudentSubmission> submissions);
	}
}
