using System.Collections.Generic;
using CSC.Common.Infrastructure.GitHub;
using CSC.CSClassroom.Model.Users;

namespace CSC.CSClassroom.Service.Projects.PushEvents
{
	/// <summary>
	/// Push events for a student repository.
	/// </summary>
	public class StudentRepoPushEvents
	{
		/// <summary>
		/// The student.
		/// </summary>
		public ClassroomMembership Student { get; }

		/// <summary>
		/// The push events.
		/// </summary>
		public IList<GitHubPushEvent> PushEvents { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public StudentRepoPushEvents(
			ClassroomMembership student, 
			IList<GitHubPushEvent> pushEvents)
		{
			Student = student;
			PushEvents = pushEvents;
		}
	}
}
