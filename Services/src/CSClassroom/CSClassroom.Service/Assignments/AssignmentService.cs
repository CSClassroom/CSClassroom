using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.Utilities;
using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Model.Assignments.ServiceResults;
using CSC.CSClassroom.Model.Users;
using CSC.CSClassroom.Repository;
using CSC.CSClassroom.Service.Assignments.AssignmentScoring;
using CSC.CSClassroom.Service.Assignments.Validators;
using Microsoft.EntityFrameworkCore;

namespace CSC.CSClassroom.Service.Assignments
{
	/// <summary>
	/// Performs assignment operations.
	/// </summary>
	public class AssignmentService : IAssignmentService
	{
		/// <summary>
		/// The database context.
		/// </summary>
		private readonly DatabaseContext _dbContext;

		/// <summary>
		/// The assignment validator.
		/// </summary>
		private readonly IAssignmentValidator _assignmentValidator; 

		/// <summary>
		/// The section assignment report generator.
		/// </summary>
		private readonly ISectionAssignmentReportGenerator _sectionAssignmentReportGenerator;

		/// <summary>
		/// The student assignment report generator.
		/// </summary>
		private readonly IStudentAssignmentReportGenerator _studentAssignmentReportGenerator;

		/// <summary>
		/// The updated assignment report generator.
		/// </summary>
		private readonly IUpdatedAssignmentReportGenerator _updatedAssignmentReportGenerator;

		/// <summary>
		/// Constructor.
		/// </summary>
		public AssignmentService(
			DatabaseContext dbContext,
			IAssignmentValidator assignmentValidator,
			ISectionAssignmentReportGenerator sectionAssignmentReportGenerator,
			IStudentAssignmentReportGenerator studentAssignmentReportGenerator,
			IUpdatedAssignmentReportGenerator updatedAssignmentReportGenerator)
		{
			_dbContext = dbContext;
			_assignmentValidator = assignmentValidator;
			_sectionAssignmentReportGenerator = sectionAssignmentReportGenerator;
			_studentAssignmentReportGenerator = studentAssignmentReportGenerator;
			_updatedAssignmentReportGenerator = updatedAssignmentReportGenerator;
		}

		/// <summary>
		/// Returns the list of assignments.
		/// </summary>
		public async Task<IList<Assignment>> GetAssignmentsAsync(string classroomName)
		{
			var classroom = await LoadClassroomAsync(classroomName);

			return await GetAssignmentsQuery(classroom)
				.ToListAsync();
		}

		/// <summary>
		/// Returns the list of assignments.
		/// </summary>
		private async Task<IList<Assignment>> GetAssignmentsAsync(
			string classroomName,
			string sectionName,
			string groupName,
			bool admin)
		{
			var classroom = await LoadClassroomAsync(classroomName);
			var section = classroom.Sections.SingleOrDefault(s => s.Name == sectionName);

			IQueryable<AssignmentQuestion> assignmentQuestionsQuery = _dbContext.AssignmentQuestions
				.Include(aq => aq.Question)
				.Include(aq => aq.Assignment)
				.Include(aq => aq.Assignment.Classroom)
				.Include(aq => aq.Assignment.Questions)
				.Include(aq => aq.Assignment.DueDates)
				.Where(aq => aq.Assignment.ClassroomId == classroom.Id);

			if (section != null)
			{
				assignmentQuestionsQuery = assignmentQuestionsQuery.Where
				(
					aq => aq.Assignment.DueDates.Any(d => d.SectionId == section.Id)
				);
			}

			if (groupName != null)
			{
				assignmentQuestionsQuery = assignmentQuestionsQuery
					.Where(aq => aq.Assignment.GroupName == groupName);
			}

			var assignmentQuestions = await assignmentQuestionsQuery.ToListAsync();

			return assignmentQuestions.Select(aq => aq.Assignment)
				.Distinct()
				.Where(a => admin || !a.IsPrivate)
				.ToList();
		}

		/// <summary>
		/// Returns the assignment with the given name.
		/// </summary>
		public async Task<Assignment> GetAssignmentAsync(string classroomName, int id)
		{
			var classroom = await LoadClassroomAsync(classroomName);

			return await GetAssignmentsQuery(classroom)
				.SingleOrDefaultAsync(assignment => assignment.Id == id);
		}

		/// <summary>
		/// Returns the assignments query.
		/// </summary>
		private IQueryable<Assignment> GetAssignmentsQuery(Classroom classroom)
		{
			return _dbContext.Assignments
				.Where(assignment => assignment.ClassroomId == classroom.Id)
				.Include(assignment => assignment.Classroom)
				.Include(assignment => assignment.Questions)
					.ThenInclude(assignmentQuestion => assignmentQuestion.Question)
				.Include(assignment => assignment.DueDates);
		}

		/// <summary>
		/// Creates a assignment.
		/// </summary>
		public async Task<bool> CreateAssignmentAsync(
			string classroomName, 
			Assignment assignment,
			IModelErrorCollection modelErrors)
		{
			var classroom = await LoadClassroomAsync(classroomName);

			if (!await UpdateAssignmentAsync(assignment, modelErrors))
				return false;

			assignment.ClassroomId = classroom.Id;
			_dbContext.Add(assignment);

			await _dbContext.SaveChangesAsync();

			return true;
		}

		/// <summary>
		/// Updates an assignment.
		/// </summary>
		public async Task<bool> UpdateAssignmentAsync(
			string classroomName, 
			Assignment assignment,
			IModelErrorCollection modelErrors)
		{
			var classroom = await LoadClassroomAsync(classroomName);

			if (!await UpdateAssignmentAsync(assignment, modelErrors))
				return false;

			assignment.ClassroomId = classroom.Id;
			_dbContext.Update(assignment);

			await _dbContext.SaveChangesAsync();

			return true;
		}

		/// <summary>
		/// Removes an assignment.
		/// </summary>
		public async Task DeleteAssignmentAsync(string classroomName, int id)
		{
			var assignment = await GetAssignmentAsync(classroomName, id);
			_dbContext.Assignments.Remove(assignment);

			await _dbContext.SaveChangesAsync();
		}

		/// <summary>
		/// Returns the results for a single assignment group in a single section.
		/// </summary>
		public async Task<SectionAssignmentResults> GetSectionAssignmentResultsAsync(
			string classroomName,
			string sectionName,
			string assignmentGroupName)
		{
			var classroom = await LoadClassroomAsync(classroomName);
			var section = classroom.Sections.SingleOrDefault(s => s.Name == sectionName);
			if (section == null)
			{
				return null;
			}

			var assignments = await GetAssignmentsAsync
			(
				classroomName, 
				sectionName, 
				assignmentGroupName,
				admin: true
			);

			var students = await _dbContext.SectionMemberships
				.Where
				(
					sm =>  sm.SectionId == section.Id
						&& sm.Role == SectionRole.Student
				)
				.Select(sm => sm.ClassroomMembership.User)
				.ToListAsync();

			var submissions = await GetUserQuestionSubmissionsAsync
			(
				classroom.Id,
				section.Id,
				assignmentGroupName,
				userId: null
			);

			return _sectionAssignmentReportGenerator.GetSectionAssignmentGroupResults
			(
				assignmentGroupName,
				assignments,
				section,
				students,
				submissions
			);
		}

		/// <summary>
		/// Returns all results updated since the last time assignments
		/// were marked as graded, for a given section.
		/// </summary>
		public async Task<UpdatedSectionAssignmentResults> GetUpdatedAssignmentResultsAsync(
			string classroomName,
			string sectionName,
			string gradebookName)
		{
			var classroom = await LoadClassroomAsync(classroomName);
			var section = classroom.Sections.SingleOrDefault(s => s.Name == sectionName);
			if (section == null)
			{
				return null;
			}
			
			var lastTransferDate = section.SectionGradebooks?.SingleOrDefault
			(
				sg => sg.ClassroomGradebook.Name == gradebookName
			)?.LastTransferDate ?? DateTime.MinValue;

			var assignments = await GetAssignmentsAsync
			(
				classroomName, 
				sectionName, 
				groupName: null,
				admin: true
			);

			var students = await _dbContext.SectionMemberships
				.Where
				(
					sm => sm.SectionId == section.Id
						&& sm.Role == SectionRole.Student
				)
				.Select(sm => sm.ClassroomMembership.User)
				.ToListAsync();

			var submissions = await GetUserQuestionSubmissionsAsync
			(
				classroom.Id,
				section.Id,
				assignmentGroupName: null,
				userId: null
			);

			return _updatedAssignmentReportGenerator.GetUpdatedAssignmentGroupResults
			(
				assignments,
				students,
				section,
				gradebookName,
				lastTransferDate,
				submissions
			);
		}

		/// <summary>
		/// Returns the results for all assignments, for a given student.
		/// </summary>
		public async Task<StudentAssignmentResults> GetStudentAssignmentResultsAsync(
			string classroomName,
			int userId,
			bool admin)
		{
			var classroom = await LoadClassroomAsync(classroomName);
			
			var user = await _dbContext.Users
				.Where(u => u.Id == userId)
				.SingleOrDefaultAsync();

			var section = await _dbContext
				.SectionMemberships
				.Where
				(
					sm =>    sm.ClassroomMembership.UserId == userId
					      && sm.ClassroomMembership.Role != ClassroomRole.Admin
					      && sm.ClassroomMembership.ClassroomId == classroom.Id
					      && sm.Role == SectionRole.Student
				)
				.Select(sm => sm.Section)
				.FirstOrDefaultAsync();

			if (section == null && !admin)
			{
				return null;
			}

			var assignments = await GetAssignmentsAsync
			(
				classroomName, 
				section?.Name, 
				groupName: null,
				admin: admin
			);

			var submissions = await GetUserQuestionSubmissionsAsync
			(
				classroom.Id,
				section?.Id,
				null /*assignmentGroupName*/,
				userId
			);

			return _studentAssignmentReportGenerator.GetStudentAssignmentGroupResults
			(
				user,
				section,
				assignments,
				submissions,
				admin
			);
		}

		/// <summary>
		/// Marks assignments in the given section as graded.
		/// </summary>
		public async Task MarkAssignmentsGradedAsync(
			string classroomName,
			string sectionName,
			string gradebookName,
			DateTime dateTime)
		{
			var classroom = await LoadClassroomAsync(classroomName);
			var section = classroom.Sections.SingleOrDefault(s => s.Name == sectionName);
			if (section == null)
			{
				throw new InvalidOperationException("Invalid section.");
			}

			var classroomGradebook = await _dbContext.ClassroomGradebooks
				.Where(cg => cg.ClassroomId == classroom.Id)
				.Where(cg => cg.Name == gradebookName)
				.SingleOrDefaultAsync();

			var sectionGradebook = await _dbContext.SectionGradebooks
				.Where(sg => sg.Section.Name == sectionName)
				.Where(sg => sg.ClassroomGradebook.Name == gradebookName)
				.SingleOrDefaultAsync();

			if (sectionGradebook == null)
			{
				sectionGradebook = new SectionGradebook()
				{
					SectionId = section.Id,
					ClassroomGradebookId = classroomGradebook.Id,
					LastTransferDate = dateTime
				};

				_dbContext.Add(sectionGradebook);
			}
			else
			{
				sectionGradebook.LastTransferDate = dateTime;
				_dbContext.Update(sectionGradebook);
			}

			await _dbContext.SaveChangesAsync();
		}

		/// <summary>
		/// Updates a assignment.
		/// </summary>
		private async Task<bool> UpdateAssignmentAsync(Assignment assignment, IModelErrorCollection modelErrors)
		{
			if (!await _assignmentValidator.ValidateAssignmentAsync(assignment, modelErrors))
			{
				return false;
			}

			UpdateQuestionOrder(assignment.Questions);

			if (string.IsNullOrWhiteSpace(assignment.GroupName))
			{
				assignment.GroupName = assignment.Name;
			}

			_dbContext.RemoveUnwantedObjects
			(
				_dbContext.AssignmentQuestions,
				question => question.Id,
				question => question.AssignmentId == assignment.Id,
				assignment.Questions
			);

			_dbContext.RemoveUnwantedObjects
			(
				_dbContext.AssignmentDueDates,
				dueDate => dueDate.Id,
				dueDate => dueDate.AssignmentId == assignment.Id,
				assignment.DueDates
			);

			return true;
		}

		/// <summary>
		/// Updates the order of test classes.
		/// </summary>
		private void UpdateQuestionOrder(IEnumerable<AssignmentQuestion> assignmentQuestions)
		{
			int index = 0;
			foreach (var question in assignmentQuestions)
			{
				question.Order = index;
				index++;
			}
		}
		
		/// <summary>
		/// Returns the submissions for a given section. Optionally filters
		/// by assignment group name, user ID, and/or date.
		/// </summary>
		private async Task<IList<UserQuestionSubmission>> GetUserQuestionSubmissionsAsync(
			int classroomId,
			int? sectionId,
			string assignmentGroupName,
			int? userId)
		{
			var submissionsQuery = 
				sectionId.HasValue
				? _dbContext.UserQuestionSubmissions.Where
					(
						submission => submission
							.UserQuestionData
							.User
							.ClassroomMemberships.Any
							(
								cm => cm.SectionMemberships.Any
								(
									sm => sm.SectionId == sectionId
										  && sm.Role == SectionRole.Student
								)
							)
					)
				: _dbContext.UserQuestionSubmissions.Where
					(
						submission => submission
							.UserQuestionData
							.AssignmentQuestion
							.Assignment
							.ClassroomId == classroomId
					);

			if (assignmentGroupName != null)
			{
				submissionsQuery = submissionsQuery.Where
				(
					submission => submission
						.UserQuestionData
						.AssignmentQuestion
						.Assignment
						.GroupName == assignmentGroupName
				);
			}

			if (userId != null)
			{
				submissionsQuery = submissionsQuery.Where
				(
					submission => submission
						.UserQuestionData
						.UserId == userId.Value
				);
			}

			var submissions = await submissionsQuery
				.Include(submission => submission.UserQuestionData.AssignmentQuestion.Question)
				.ToListAsync();

			return submissions;
		}

		/// <summary>
		/// Returns the classroom with the given name.
		/// </summary>
		private async Task<Classroom> LoadClassroomAsync(string classroomName)
		{
			return await _dbContext.Classrooms
				.Where(c => c.Name == classroomName)
				.Include(c => c.Sections)
				.Include(c => c.ClassroomGradebooks)
					.ThenInclude(cg => cg.SectionGradebooks)
				.SingleOrDefaultAsync();
		}
	}
}
