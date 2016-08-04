using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Service.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSC.CSClassroom.Service.Classrooms
{
	/// <summary>
	/// Performs classroom operations.
	/// </summary>
	public class ClassroomService : IClassroomService
	{
		/// <summary>
		/// The database context.
		/// </summary>
		private DatabaseContext _dbContext;

		/// <summary>
		/// Constructor.
		/// </summary>
		public ClassroomService(DatabaseContext dbContext)
		{
			_dbContext = dbContext;
		}

		/// <summary>
		/// Returns the list of classrooms.
		/// </summary>
		public async Task<IList<Classroom>> GetClassroomsAsync(Group group)
		{
			return await _dbContext.Classrooms
				.Where(classroom => classroom.GroupId == group.Id)
				.ToListAsync();
		}

		/// <summary>
		/// Returns the classroom with the given name.
		/// </summary>
		public async Task<Classroom> GetClassroomAsync(Group group, string classroomName)
		{
			return await _dbContext.Classrooms
				.Where(classroom => classroom.GroupId == group.Id)
				.SingleOrDefaultAsync(classroom => classroom.Name == classroomName);
		}

		/// <summary>
		/// Creates a classroom.
		/// </summary>
		public async Task CreateClassroomAsync(Group group, Classroom classroom)
		{
			classroom.GroupId = group.Id;
			_dbContext.Add(classroom);

			await _dbContext.SaveChangesAsync();
		}

		/// <summary>
		/// Updates a classroom.
		/// </summary>
		public async Task UpdateClassroomAsync(Group group, Classroom classroom)
		{
			classroom.GroupId = group.Id;
			_dbContext.Update(classroom);

			await _dbContext.SaveChangesAsync();
		}

		/// <summary>
		/// Removes a classroom.
		/// </summary>
		/// <param name="classroomName">The name of the classroom to remove.</param>
		public async Task DeleteClassroomAsync(Group group, string classroomName)
		{
			var classroom = await GetClassroomAsync(group, classroomName);
			_dbContext.Classrooms.Remove(classroom);

			await _dbContext.SaveChangesAsync();
		}
	}
}
