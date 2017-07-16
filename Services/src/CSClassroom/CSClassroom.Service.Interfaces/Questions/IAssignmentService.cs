using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.Utilities;
using CSC.CSClassroom.Model.Questions;
using CSC.CSClassroom.Model.Questions.ServiceResults;

namespace CSC.CSClassroom.Service.Questions
{
	/// <summary>
	/// Performs assignment operations.
	/// </summary>
	public interface IAssignmentService
	{
		/// <summary>
		/// Returns the list of assignments.
		/// </summary>
		Task<IList<Assignment>> GetAssignmentsAsync(string classroomName);

		/// <summary>
		/// Returns the assignment with the given name.
		/// </summary>
		Task<Assignment> GetAssignmentAsync(string classroomName, int id);

		/// <summary>
		/// Creates a assignment.
		/// </summary>
		Task<bool> CreateAssignmentAsync(
			string classroomName,
			Assignment assignment, 
			IModelErrorCollection modelErrors);

		/// <summary>
		/// Updates an assignment.
		/// </summary>
		Task<bool> UpdateAssignmentAsync(
			string classroomName,
			Assignment assignment,
			IModelErrorCollection modelErrors);

		/// <summary>
		/// Removes an assignment.
		/// </summary>
		Task DeleteAssignmentAsync(string classroomName, int id);

		/// <summary>
		/// Returns the results for a single assignment group in a single section.
		/// </summary>
		Task<SectionAssignmentResults> GetSectionAssignmentResultsAsync(
			string classroomName,
			string sectionName,
			string assignmentGroupName);

		/// <summary>
		/// Returns all results updated since the last time assignments
		/// were marked as graded, for a given section.
		/// </summary>
		Task<UpdatedSectionAssignmentResults> GetUpdatedAssignmentResultsAsync(
			string classroomName,
			string sectionName,
			string gradebookName);

		/// <summary>
		/// Returns the results for all assignments, for a given student.
		/// </summary>
		Task<StudentAssignmentResults> GetStudentAssignmentResultsAsync(
			string classroomName,
			int userId,
			bool admin);

		/// <summary>
		/// Marks assignments in the given section as graded.
		/// </summary>
		Task MarkAssignmentsGradedAsync(
			string classroomName,
			string sectionName,
			string gradebookName,
			DateTime dateTime);
	}
}
