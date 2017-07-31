using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSC.CSClassroom.Repository;
using Microsoft.EntityFrameworkCore;

namespace CSC.CSClassroom.Service.Assignments.QuestionSolvers
{
	/// <summary>
	/// Retrieves an assignment due date from the database.
	/// </summary>
	public class AssignmentDueDateRetriever : IAssignmentDueDateRetriever
	{
		/// <summary>
		/// The database context.
		/// </summary>
		private readonly DatabaseContext _dbContext;

		/// <summary>
		/// Constructor.
		/// </summary>
		public AssignmentDueDateRetriever(DatabaseContext dbContext)
		{
			_dbContext = dbContext;
		}

		/// <summary>
		/// Returns the due date for the assignment, for the given user.
		/// </summary>
		public async Task<DateTime?> GetUserAssignmentDueDateAsync(
			string classroomName,
			int assignmentId,
			int userId)
		{
			var sectionMembership = await _dbContext.SectionMemberships
				.Where(sm => sm.ClassroomMembership.Classroom.Name == classroomName)
				.Where(sm => sm.ClassroomMembership.UserId == userId)
				.FirstOrDefaultAsync();

			if (sectionMembership != null)
			{
				var dueDate = await _dbContext.AssignmentDueDates
					.Where(dd => dd.AssignmentId == assignmentId)
					.Where(dd => dd.SectionId == sectionMembership.SectionId)
					.FirstOrDefaultAsync();

				return dueDate?.DueDate;
			}
			else
			{
				return null;
			}
		}
	}
}
