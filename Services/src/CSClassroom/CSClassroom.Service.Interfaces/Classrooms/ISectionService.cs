using System.Collections.Generic;
using System.Threading.Tasks;
using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Model.Users;

namespace CSC.CSClassroom.Service.Classrooms
{
	/// <summary>
	/// Performs section operations.
	/// </summary>
	public interface ISectionService
	{
		/// <summary>
		/// Returns the section with the given name.
		/// </summary>
		Task<Section> GetSectionAsync(string classroom, string sectionName);

		/// <summary>
		/// Returns all students in the given section.
		/// </summary>
		Task<IList<SectionMembership>> GetSectionStudentsAsync(
			string classroomName, 
			string sectionName);

		/// <summary>
		/// Creates a section.
		/// </summary>
		Task CreateSectionAsync(string classroomName, Section section);

		/// <summary>
		/// Updates a section.
		/// </summary>
		Task UpdateSectionAsync(string classroomName, Section section);

		/// <summary>
		/// Removes a section.
		/// </summary>
		Task DeleteSectionAsync(string classroomName, string sectionName);
	}
}
