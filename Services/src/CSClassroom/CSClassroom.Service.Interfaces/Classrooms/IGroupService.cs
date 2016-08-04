using CSC.CSClassroom.Model.Classrooms;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CSC.CSClassroom.Service.Classrooms
{
	/// <summary>
	/// Performs group operations.
	/// </summary>
	public interface IGroupService
	{
		/// <summary>
		/// Returns the list of groups.
		/// </summary>
		Task<IList<Group>> GetGroupsAsync();

		/// <summary>
		/// Returns the group with the given name.
		/// </summary>
		Task<Group> GetGroupAsync(string groupName);

		/// <summary>
		/// Creates a group.
		/// </summary>
		Task CreateGroupAsync(Group group);

		/// <summary>
		/// Updates a group.
		/// </summary>
		Task UpdateGroupAsync(Group group);

		/// <summary>
		/// Removes a group.
		/// </summary>
		/// <param name="groupName">The name of the group to remove.</param>
		Task DeleteGroupAsync(string groupName);
	}
}
