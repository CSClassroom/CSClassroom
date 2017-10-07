using System;
using System.Collections.Generic;
using System.Linq;
using CSC.Common.Infrastructure.Extensions;
using MoreLinq;
using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Model.Assignments.ServiceResults;
using CSC.CSClassroom.Model.Users;
using CSC.Common.Infrastructure.System;

namespace CSC.CSClassroom.Service.Assignments.AssignmentScoring
{
	/// <summary>
	/// Generates an assignment report for a single student.
	/// </summary>
	public class StudentAssignmentReportGenerator : IStudentAssignmentReportGenerator
	{
		/// <summary>
		/// Generates assignment group results.
		/// </summary>
		private readonly IAssignmentGroupResultGenerator _assignmentGroupResultGenerator;

		/// <summary>
		/// The assignment filter.
		/// </summary>
		private readonly IAssignmentFilter _assignmentFilter;

		/// <summary>
		/// Constructor.
		/// </summary>
		public StudentAssignmentReportGenerator(
			IAssignmentGroupResultGenerator assignmentGroupResultGenerator,
			IAssignmentFilter assignmentFilter)
		{
			_assignmentGroupResultGenerator = assignmentGroupResultGenerator;
			_assignmentFilter = assignmentFilter;
		}

		/// <summary>
		/// Calculates the scores of all assignment groups for a given student.
		/// </summary>
		public StudentAssignmentResults GetStudentAssignmentGroupResults(
			User user,
			Section section,
			IList<Assignment> assignments,
			IList<UserQuestionSubmission> submissions,
			bool admin)
		{
			var sectionAssignments = _assignmentFilter.FilterAssignments
			(
				assignments,
				section
			);

			var userSubmissions = _assignmentFilter.FilterSubmissions
			(
				sectionAssignments,
				submissions,
				user
			);

			var assignmentGroups = _assignmentFilter.GetAssignmentGroups
			(
				section,
				sectionAssignments
			);

			var assignmentGroupResults = assignmentGroups.Select
			(
				assignmentGroup => _assignmentGroupResultGenerator.GetAssignmentGroupResult
				(
					assignmentGroup.Key,
					assignmentGroup.ToList(),
					section,
					user,
					userSubmissions,
					admin
				)
			).ToList();

			return new StudentAssignmentResults
			(
				user.LastName,
				user.FirstName,
				section?.DisplayName,
				assignmentGroupResults
			);
		}
	}
}
