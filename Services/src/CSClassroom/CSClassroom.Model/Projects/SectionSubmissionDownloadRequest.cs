using System;
using System.Collections.Generic;
using System.Text;

namespace CSC.CSClassroom.Model.Projects
{
	public class SectionSubmissionDownloadRequest
	{
		public int SectionId { get; }
		public IList<int> UserIds { get; }

		public SectionSubmissionDownloadRequest(int sectionId, IList<int> userIds)
		{
			SectionId = sectionId;
			UserIds = userIds;
		}
	}
}
