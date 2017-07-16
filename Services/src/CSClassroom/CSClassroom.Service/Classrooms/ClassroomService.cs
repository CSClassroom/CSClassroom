using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Model.Questions;
using CSC.CSClassroom.Model.Users;
using CSC.CSClassroom.Repository;
using Microsoft.EntityFrameworkCore;

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
		private readonly DatabaseContext _dbContext;

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
		public async Task<IList<Classroom>> GetClassroomsAsync()
		{
			return await _dbContext.Classrooms.ToListAsync();
		}

		/// <summary>
		/// Returns the classroom with the given name.
		/// </summary>
		public async Task<Classroom> GetClassroomAsync(string classroomName)
		{
			// Work around EF not supporting eager/lazy loading
			await _dbContext.SectionGradebooks
				.Where(sg => sg.ClassroomGradebook.Classroom.Name == classroomName)
				.ToListAsync();

			return await _dbContext.Classrooms
				.Include(c => c.Sections)
				.Include(c => c.ClassroomGradebooks)
				.SingleOrDefaultAsync(classroom => classroom.Name == classroomName);
		}

		/// <summary>
		/// Returns all administrators of the current classroom.
		/// </summary>
		public async Task<IList<ClassroomMembership>> GetClassroomAdminsAsync(string classroomName)
		{
			return await _dbContext.ClassroomMemberships
				.Where
				(
					   m => m.Classroom.Name == classroomName
					&& m.Role == ClassroomRole.Admin
				)
				.Include(m => m.User)
				.ToListAsync();
		}

		/// <summary>
		/// Returns all classrooms the user has access to.
		/// </summary>
		public async Task<IList<ClassroomMembership>> GetClassroomsWithAccessAsync(int userId)
		{
			var user = await _dbContext.Users
				.Where(u => u.Id == userId)
				.SingleOrDefaultAsync();

			if (user == null)
				return new List<ClassroomMembership>();

			var userMemberships = await _dbContext.ClassroomMemberships.Where
			(
				m => m.UserId == user.Id
			).ToListAsync();

			if (user.SuperUser)
			{
				var classrooms = await GetClassroomsAsync();
				var classroomsWithoutExplicitMembership = classrooms
					.Where
					(
						c => !userMemberships.Any(m => m.ClassroomId == c.Id)
					)
					.Select
					(
						c => new ClassroomMembership()
						{
							ClassroomId = c.Id,
							Classroom = c,
							Role = ClassroomRole.Admin,
							UserId = user.Id,
							User = user
						}
					).ToList();

				return userMemberships.Concat(classroomsWithoutExplicitMembership).ToList();
			}
			else
			{
				return userMemberships;
			}			
		}

		/// <summary>
		/// Creates a classroom.
		/// </summary>
		public async Task CreateClassroomAsync(Classroom classroom)
		{
			UpdateClassroom(classroom);

			_dbContext.Add(classroom);

			await _dbContext.SaveChangesAsync();
		}

		/// <summary>
		/// Updates a classroom.
		/// </summary>
		public async Task UpdateClassroomAsync(Classroom classroom)
		{
			var currentClassroom = await _dbContext.Classrooms
				.Where(c => c.Id == classroom.Id)
				.SingleOrDefaultAsync();

			_dbContext.Entry(currentClassroom).State = EntityState.Detached;

			UpdateClassroom(classroom);
			_dbContext.Update(classroom);

			await _dbContext.SaveChangesAsync();
		}

		/// <summary>
		/// Removes a classroom.
		/// </summary>
		/// <param name="classroomName">The name of the group to remove.</param>
		public async Task DeleteClassroomAsync(string classroomName)
		{
			var classroom = await GetClassroomAsync(classroomName);
			_dbContext.Classrooms.Remove(classroom);

			await _dbContext.SaveChangesAsync();
		}

		/// <summary>
		/// Updates a classroom.
		/// </summary>
		private void UpdateClassroom(Classroom classroom)
		{
			UpdateClassroomGradebookOrder(classroom.ClassroomGradebooks);

			_dbContext.RemoveUnwantedObjects
			(
				_dbContext.ClassroomGradebooks,
				classroomGradebook => classroomGradebook.Id,
				classroomGradebook => classroomGradebook.ClassroomId == classroom.Id,
				classroom.ClassroomGradebooks
			);
		}

		/// <summary>
		/// Updates the order of test classes.
		/// </summary>
		private void UpdateClassroomGradebookOrder(
			IEnumerable<ClassroomGradebook> classroomGradebooks)
		{
			if (classroomGradebooks != null)
			{
				int index = 0;
				foreach (var classroomGradebook in classroomGradebooks)
				{
					classroomGradebook.Order = index;
					index++;
				}
			}
		}
	}
}
