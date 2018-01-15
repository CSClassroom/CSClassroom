using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.Utilities;
using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Model.Users;
using CSC.CSClassroom.Repository;
using Microsoft.EntityFrameworkCore;
using MoreLinq;

namespace CSC.CSClassroom.Service.Classrooms
{
	/// <summary>
	/// Performs section operations.
	/// </summary>
	public class SectionService : ISectionService
	{
		/// <summary>
		/// The database context.
		/// </summary>
		private readonly DatabaseContext _dbContext;

		/// <summary>
		/// Ensures that sections being created/modified are valid.
		/// </summary>
		private readonly ISectionValidator _sectionValidator;

		/// <summary>
		/// Constructor.
		/// </summary>
		public SectionService(
			DatabaseContext dbContext, 
			ISectionValidator sectionValidator)
		{
			_dbContext = dbContext;
			_sectionValidator = sectionValidator;
		}

		/// <summary>
		/// Returns the section with the given name.
		/// </summary>
		public async Task<Section> GetSectionAsync(string classroomName, string sectionName)
		{
			var classroom = await LoadClassroomAsync(classroomName);
			if (classroom == null)
			{
				return null;
			}

			return await _dbContext.Sections
				.Where(section => section.ClassroomId == classroom.Id)
				.Include(section => section.SectionGradebooks)
				.Include(section => section.SectionRecipients)
				.SingleOrDefaultAsync(section => section.Name == sectionName);
		}

		/// <summary>
		/// Returns all section memberships for the given section.
		/// </summary>
		public async Task<IList<SectionMembership>> GetSectionStudentsAsync(
			string classroomName,
			string sectionName)
		{
			var classroom = await LoadClassroomAsync(classroomName);
			var section = classroom.Sections
				.Single(s => s.Name == sectionName);

			return await _dbContext.SectionMemberships
				.Where(sm => sm.ClassroomMembership.ClassroomId == section.ClassroomId)
				.Where(sm => sm.SectionId == section.Id)
				.Include(sm => sm.ClassroomMembership)
					.ThenInclude(cm => cm.User)
				.ToListAsync();
		}

		/// <summary>
		/// Creates a section.
		/// </summary>
		public async Task<bool> CreateSectionAsync(
			string classroomName, 
			Section section,
			IModelErrorCollection errors)
		{
			var classroom = await LoadClassroomAsync(classroomName);
			section.ClassroomId = classroom.Id;

			if (!await _sectionValidator.ValidateSectionAsync(section, errors))
			{
				return false;
			}
			
			_dbContext.Add(section);

			await _dbContext.SaveChangesAsync();

			return true;
		}

		/// <summary>
		/// Updates a section.
		/// </summary>
		public async Task<bool> UpdateSectionAsync(
			string classroomName, 
			Section section,
			IModelErrorCollection errors)
		{
			var classroom = await LoadClassroomAsync(classroomName);

			section.ClassroomId = classroom.Id;

			var currentSection = await _dbContext.Sections
				.Where(s => s.Id == section.Id)
				.SingleOrDefaultAsync();

			_dbContext.Entry(currentSection).State = EntityState.Detached;

			if (!await _sectionValidator.ValidateSectionAsync(section, errors))
			{
				return false;
			}
			
			UpdateSection(section);
			_dbContext.Update(section);

			await _dbContext.SaveChangesAsync();

			return true;
		}

		/// <summary>
		/// Removes a section.
		/// </summary>
		public async Task DeleteSectionAsync(string classroomName, string sectionName)
		{
			var section = await GetSectionAsync(classroomName, sectionName);
			_dbContext.Sections.Remove(section);

			await _dbContext.SaveChangesAsync();
		}

		/// <summary>
		/// Updates a classroom.
		/// </summary>
		private void UpdateSection(Section section)
		{
			_dbContext.RemoveUnwantedObjects
			(
				_dbContext.SectionGradebooks,
				sectionGradebook => sectionGradebook.Id,
				sectionGradebook => sectionGradebook.SectionId == section.Id,
				section.SectionGradebooks
			);
			
			_dbContext.RemoveUnwantedObjects
			(
				_dbContext.SectionRecipients,
				sectionRecipient => sectionRecipient.Id,
				sectionRecipient => sectionRecipient.SectionId == section.Id,
				section.SectionRecipients
			);
		}

		/// <summary>
		/// Returns the classroom with the given name.
		/// </summary>
		private async Task<Classroom> LoadClassroomAsync(string classroomName)
		{
			return await _dbContext.Classrooms
				.Where(c => c.Name == classroomName)
				.Include(c => c.Sections)
				.SingleOrDefaultAsync();
		}
	}
}
