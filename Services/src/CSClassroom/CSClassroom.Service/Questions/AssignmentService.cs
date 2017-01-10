using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.Utilities;
using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Model.Questions;
using CSC.CSClassroom.Model.Questions.ServiceResults;
using CSC.CSClassroom.Model.Users;
using CSC.CSClassroom.Repository;
using CSC.CSClassroom.Service.Questions.AssignmentScoring;
using Microsoft.EntityFrameworkCore;

namespace CSC.CSClassroom.Service.Questions
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
		/// The assignment result generator.
		/// </summary>
		private readonly IAssignmentScoreCalculator _assignmentScoreCalculator;

		/// <summary>
		/// Constructor.
		/// </summary>
		public AssignmentService(
			DatabaseContext dbContext, 
			IAssignmentScoreCalculator assignmentScoreCalculator)
		{
			_dbContext = dbContext;
			_assignmentScoreCalculator = assignmentScoreCalculator;
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
		public async Task<IList<Assignment>> GetAssignmentsAsync(
			string classroomName,
			string sectionName,
			string groupName)
		{
			var classroom = await LoadClassroomAsync(classroomName);
			var section = classroom.Sections.SingleOrDefault(s => s.Name == sectionName);
			if (section == null)
			{
				return null;
			}

			IQueryable<AssignmentQuestion> assignmentQuestionsQuery = 
				_dbContext.AssignmentQuestions.Where
				(
					aq =>  aq.Assignment.ClassroomId == section.ClassroomId
						&& aq.Assignment.DueDates.Any(d => d.SectionId == section.Id)
				)
				.Include(aq => aq.Question)
				.Include(aq => aq.Assignment)
				.Include(aq => aq.Assignment.Classroom)
				.Include(aq => aq.Assignment.Questions)
				.Include(aq => aq.Assignment.DueDates);

			if (groupName != null)
			{
				assignmentQuestionsQuery = assignmentQuestionsQuery
					.Where(aq => aq.Assignment.GroupName == groupName);
			}

			var assignmentQuestions = await assignmentQuestionsQuery.ToListAsync();

			return assignmentQuestions.Select(aq => aq.Assignment)
				.Distinct()
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

			if (!UpdateAssignment(assignment, modelErrors))
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

			if (!UpdateAssignment(assignment, modelErrors))
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
		/// Returns the submissions for a given section. Optionally filters
		/// by assignment group name, user ID, and/or date.
		/// </summary>
		private async Task<IList<UserQuestionSubmission>> GetSubmissionsAsync(
			int sectionId,
			string assignmentGroupName,
			int? userId)
		{
			var questionDataQuery = _dbContext.UserQuestionData.Where
			(
				uqd => uqd.User.ClassroomMemberships.Any
				(
					cm => cm.SectionMemberships.Any
					(
						sm =>  sm.SectionId == sectionId
							&& sm.Role == SectionRole.Student
					)
				)
			);

			if (assignmentGroupName != null)
			{
				questionDataQuery = questionDataQuery.Where
				(
					uqd => uqd.Question.AssignmentQuestions.Any
					(
						aq => aq.Assignment.GroupName == assignmentGroupName
					)
				);
			}

			if (userId != null)
			{
				questionDataQuery = questionDataQuery.Where
				(
					uqd => uqd.UserId == userId.Value
				);
			}

			var questionData = await questionDataQuery
				.Include(uqd => uqd.Submissions)
				.ToListAsync();

			var submissions = questionData
				.SelectMany(uqd => uqd.Submissions)
				.ToList();

			return submissions;
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

			var assignments = await GetAssignmentsAsync(classroomName, sectionName, assignmentGroupName);

			var students = await _dbContext.SectionMemberships
				.Where
				(
					sm =>  sm.SectionId == section.Id
						&& sm.Role == SectionRole.Student
				)
				.Select(sm => sm.ClassroomMembership.User)
				.ToListAsync();

			var submissions = await GetSubmissionsAsync
			(
				section.Id,
				assignmentGroupName,
				userId: null
			);

			return _assignmentScoreCalculator.GetSectionAssignmentResults
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

			var assignments = await GetAssignmentsAsync(classroomName, sectionName, groupName: null);

			var students = await _dbContext.SectionMemberships
				.Where
				(
					sm => sm.SectionId == section.Id
						&& sm.Role == SectionRole.Student
				)
				.Select(sm => sm.ClassroomMembership.User)
				.ToListAsync();

			var submissions = await GetSubmissionsAsync
			(
				section.Id,
				assignmentGroupName: null,
				userId: null
			);

			return _assignmentScoreCalculator.GetUpdatedAssignmentResults
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
			int userId)
		{
			var classroom = await LoadClassroomAsync(classroomName);
			
			var user = await _dbContext.Users
				.Where(u => u.Id == userId)
				.SingleOrDefaultAsync();

			var section = await _dbContext.SectionMemberships.Where
				(
					sm => sm.ClassroomMembership.UserId == userId
						&& sm.ClassroomMembership.ClassroomId == classroom.Id
						&& sm.Role == SectionRole.Student
				)
				.Select(sm => sm.Section)
				.FirstOrDefaultAsync();

			if (section == null)
			{
				return null;
			}

			var assignments = await GetAssignmentsAsync(classroomName, section.Name, groupName: null);

			var submissions = await GetSubmissionsAsync
			(
				section.Id,
				null /*assignmentGroupName*/,
				userId
			);

			return _assignmentScoreCalculator.GetStudentAssignmentResults
			(
				user,
				section,
				assignments,
				submissions
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
		private bool UpdateAssignment(Assignment assignment, IModelErrorCollection modelErrors)
		{
			if (assignment.DueDates != null)
			{
				var sections = assignment.DueDates.Select(d => d.SectionId).ToList();
				if (sections.Distinct().Count() != sections.Count)
				{
					modelErrors.AddError("DueDates", "You may only have one due date per section.");

					return false;
				}
			}

			UpdateQuestionOrder(assignment.Questions);

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
