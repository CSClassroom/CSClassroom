using System.Collections.Generic;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.System;
using CSC.CSClassroom.Model.Projects;

namespace CSC.CSClassroom.Service.Projects.Submissions
{
	/// <summary>
	/// Downloads student submissions.
	/// </summary>
	public interface ISubmissionDownloader
	{
		/// <summary>
		/// Downloads the contents of the project template.
		/// </summary>
		Task<IArchive> DownloadTemplateContentsAsync(Project project);

		/// <summary>
		/// Downloads submissions for a set of students.
		/// </summary>
		Task<StudentSubmissions> DownloadSubmissionsAsync(
			Checkpoint checkpoint,
			IList<StudentDownloadRequest> studentDownloadRequests);
	}
}
