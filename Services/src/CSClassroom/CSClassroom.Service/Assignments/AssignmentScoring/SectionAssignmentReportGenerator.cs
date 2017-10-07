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
	/// Generates an assignment report for all students in a section.
	/// </summary>
	public class SectionAssignmentReportGenerator : ISectionAssignmentReportGenerator
	{
		/// <summary>
		/// Generates assignment group results.
		/// </summary>
		private readonly IAssignmentGroupResultGenerator _assignmentGroupResultGenerator;

		/// <summary>
		/// Calculates the score for an assignment group.
		/// </summary>
		private readonly IAssignmentGroupScoreCalculator _assignmentGroupScoreCalculator;

		/// <summary>
		/// The assignment filter.
		/// </summary>
		private readonly IAssignmentFilter _assignmentFilter;

		/// <summary>
		/// Constructor.
		/// </summary>
		public SectionAssignmentReportGenerator(
			IAssignmentGroupResultGenerator assignmentGroupResultGenerator,
			IAssignmentGroupScoreCalculator assignmentGroupScoreCalculator,
			IAssignmentFilter assignmentFilter)
		{
			_assignmentGroupResultGenerator = assignmentGroupResultGenerator;
			_assignmentFilter = assignmentFilter;
			_assignmentGroupScoreCalculator = assignmentGroupScoreCalculator;
		}

		/// <summary>
		/// Calculates the score for a single assignment group in a single section,
		/// for all students.
		/// </summary>
		public SectionAssignmentResults GetSectionAssignmentGroupResults(
			string assignmentGroupName,
			IList<Assignment> assignments,
			Section section,
			IList<User> users,
			IList<UserQuestionSubmission> submissions)
		{
			var sectionAssignmentsInGroup = _assignmentFilter.FilterAssignments
			(
				assignments,
				section,
				assignmentGroupName
			);

			var assignmentGroupSubmissions = _assignmentFilter.FilterSubmissions
			(
				sectionAssignmentsInGroup,
				submissions
			);

			var assignmentGroupSubmissionsByUser = assignmentGroupSubmissions
				.GroupBy(s => s.UserQuestionData.UserId)
				.ToDictionary(g => g.Key, g => g.ToList());

			var studentResults = users
				.OrderBy(u => u.LastName)
				.ThenBy(u => u.FirstName)
				.Select
				(
					user => _assignmentGroupResultGenerator.GetAssignmentGroupResult
					(
						assignmentGroupName,
						sectionAssignmentsInGroup,
						section,
						user,
						assignmentGroupSubmissionsByUser.GetValueOrNew(user.Id),
						admin: true
					)
				).ToList();

			return new SectionAssignmentResults
			(
				assignmentGroupName,
				section.DisplayName,
				_assignmentGroupScoreCalculator.GetAssignmentGroupTotalPoints
				(
					sectionAssignmentsInGroup,
					roundDigits: 1
				),
				studentResults
			);
		}
	}
}
