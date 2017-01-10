using System.Collections.Generic;
using System.Threading.Tasks;
using CSC.CSClassroom.Model.Projects;
using CSC.CSClassroom.Model.Users;

namespace CSC.CSClassroom.Service.Projects.PushEvents
{
	/// <summary>
	/// Validates push events.
	/// </summary>
	public interface IPushEventRetriever
	{
		/// <summary>
		/// Returns a list of push events for the given project.
		/// </summary>
		Task<IList<StudentRepoPushEvents>> GetAllPushEventsAsync(
			Project project,
			IList<ClassroomMembership> students);
	}
}