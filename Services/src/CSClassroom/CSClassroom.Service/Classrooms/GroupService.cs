using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Service.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CSC.CSClassroom.Service.Classrooms
{
	/// <summary>
	/// Performs group operations.
	/// </summary>
	public class GroupService : IGroupService
	{
		/// <summary>
		/// The database context.
		/// </summary>
		private DatabaseContext _dbContext;

		/// <summary>
		/// Constructor.
		/// </summary>
		public GroupService(DatabaseContext dbContext)
		{
			_dbContext = dbContext;
		}

		/// <summary>
		/// Returns the list of groups.
		/// </summary>
		public async Task<IList<Group>> GetGroupsAsync()
		{
			return await _dbContext.Groups.ToListAsync();
		}

		/// <summary>
		/// Returns the group with the given name.
		/// </summary>
		public async Task<Group> GetGroupAsync(string groupName)
		{
			return await _dbContext.Groups
				.SingleOrDefaultAsync(group => group.Name == groupName);
		}

		/// <summary>
		/// Creates a group.
		/// </summary>
		public async Task CreateGroupAsync(Group group)
		{
			_dbContext.Add(group);

			await _dbContext.SaveChangesAsync();
		}

		/// <summary>
		/// Updates a group.
		/// </summary>
		public async Task UpdateGroupAsync(Group group)
		{
			_dbContext.Update(group);

			await _dbContext.SaveChangesAsync();
		}

		/// <summary>
		/// Removes a group.
		/// </summary>
		/// <param name="groupName">The name of the group to remove.</param>
		public async Task DeleteGroupAsync(string groupName)
		{
			var group = await GetGroupAsync(groupName);
			_dbContext.Groups.Remove(group);

			await _dbContext.SaveChangesAsync();
		}
	}
}
