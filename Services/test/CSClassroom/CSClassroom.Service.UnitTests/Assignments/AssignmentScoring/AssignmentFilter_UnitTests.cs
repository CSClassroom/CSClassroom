using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Model.Users;
using CSC.CSClassroom.Service.Assignments.AssignmentScoring;
using CSC.CSClassroom.Service.UnitTests.Utilities;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Assignments.AssignmentScoring
{
	/// <summary>
	/// Unit tests for the assignment filter class.
	/// </summary>
	public class AssignmentFilter_UnitTests
	{
		/// <summary>
		/// Ensures that FilterAssignments filters the given list of assignments
		/// to those in the given group assigned to the given section.
		/// </summary>
		[Fact]
		public void FilterAssignments_GroupNameAndSectionGiven_ReturnsAssignedInGroupToSection()
		{
			var section1 = new Section() { Id = 1 };
			var section2 = new Section() { Id = 2 };
			var assignments = Collections.CreateList
			(
				CreateAssignment("Group1", section1, GetDate(5)),
				CreateAssignment("Group1", section1, GetDate(4)),
				CreateAssignment("Group2", section1, GetDate(3)),
				CreateAssignment("Group1", section2, GetDate(2)),
				CreateAssignment("Group2", section2, GetDate(1)),
				CreateAssignment("Group1", section: null, dueDate: null)
			);

			var assignmentFilter = new AssignmentFilter();
			var results = assignmentFilter.FilterAssignments
			(
				assignments, 
				section1, 
				"Group1"
			);

			Assert.Equal(2, results.Count);
			Assert.Equal(assignments[1], results[0]);
			Assert.Equal(assignments[0], results[1]);
		}

		/// <summary>
		/// Ensures that FilterAssignments filters the given list of assignments
		/// to all assignments assigned to the given section, when no group name
		/// is specified.
		/// </summary>
		[Fact]
		public void FilterAssignments_OnlySectionGiven_FiltersToAssignedInSection()
		{
			var section1 = new Section() { Id = 1 };
			var section2 = new Section() { Id = 2 };
			var assignments = Collections.CreateList
			(
				CreateAssignment("Group2", section1, GetDate(5)),
				CreateAssignment("Group1", section1, GetDate(5)),
				CreateAssignment("Group1", section1, GetDate(4)),
				CreateAssignment("Group1", section2, GetDate(2)),
				CreateAssignment("Group2", section2, GetDate(1)),
				CreateAssignment("Group1", section: null, dueDate: null)
			);

			var assignmentFilter = new AssignmentFilter();
			var results = assignmentFilter.FilterAssignments
			(
				assignments,
				section1,
				assignmentGroupName: null
			);

			Assert.Equal(3, results.Count);
			Assert.Equal(assignments[2], results[0]);
			Assert.Equal(assignments[1], results[1]);
			Assert.Equal(assignments[0], results[2]);
		}

		/// <summary>
		/// Ensures that FilterAssignments filters the given list of assignments
		/// to all those with the given group name, when no section is given.
		/// </summary>
		[Fact]
		public void FilterAssignments_OnlyGroupNameGiven_ReturnsAllInGroup()
		{
			var section1 = new Section() { Id = 1 };
			var section2 = new Section() { Id = 2 };
			var assignments = Collections.CreateList
			(
				CreateAssignment("Group1", section1, GetDate(5)),
				CreateAssignment("Group1", section1, GetDate(4)),
				CreateAssignment("Group2", section1, GetDate(3)),
				CreateAssignment("Group1", section2, GetDate(2)),
				CreateAssignment("Group2", section2, GetDate(1)),
				CreateAssignment("Group1", section: null, dueDate: null)
			);

			var assignmentFilter = new AssignmentFilter();
			var results = assignmentFilter.FilterAssignments
			(
				assignments,
				null /*section*/,
				"Group1"
			);

			Assert.Equal(4, results.Count);
			Assert.Equal(assignments[3], results[0]);
			Assert.Equal(assignments[1], results[1]);
			Assert.Equal(assignments[0], results[2]);
			Assert.Equal(assignments[5], results[3]);
		}

		/// <summary>
		/// Ensures that FilterAssignments returns the same list of assignments
		/// when no group name or section is given.
		/// </summary>
		[Fact]
		public void FilterAssignments_NeitherGroupNameNorSectionGiven_ReturnsAllAssignments()
		{
			var section1 = new Section() { Id = 1 };
			var section2 = new Section() { Id = 2 };
			var assignments = Collections.CreateList
			(
				CreateAssignment("Group1", section1, GetDate(5)),
				CreateAssignment("Group1", section1, GetDate(4)),
				CreateAssignment("Group2", section1, GetDate(3)),
				CreateAssignment("Group1", section2, GetDate(2)),
				CreateAssignment("Group2", section2, GetDate(1)),
				CreateAssignment("Group1", section: null, dueDate: null)
			);

			var assignmentFilter = new AssignmentFilter();
			var results = assignmentFilter.FilterAssignments
			(
				assignments,
				section: null,
				assignmentGroupName: null
			);

			Assert.Equal(6, results.Count);
			Assert.Equal(assignments[4], results[0]);
			Assert.Equal(assignments[3], results[1]);
			Assert.Equal(assignments[2], results[2]);
			Assert.Equal(assignments[1], results[3]);
			Assert.Equal(assignments[0], results[4]);
			Assert.Equal(assignments[5], results[5]);
		}

		/// <summary>
		/// Ensures that GetAssignmentGroups returns a list of assignment groups 
		/// for the given assignments, ordered by the due date for the given 
		/// section (with ties broken by group name).
		/// </summary>
		[Fact]
		public void GetAssignmentGroups_ReturnsGroupsOrderedByLastDueDateThenName()
		{
			var section = new Section() { Id = 1 };
			var assignments = Collections.CreateList
			(
				CreateAssignment("Group1", section: null, dueDate: null),
				CreateAssignment("Group1", section, GetDate(1)),
				CreateAssignment("Group2", section, GetDate(5)),
				CreateAssignment("Group2", section, GetDate(4)),
				CreateAssignment("Group3", section, GetDate(5)),
				CreateAssignment("Group2", section, GetDate(2))
			);

			var assignmentFilter = new AssignmentFilter();
			var results = assignmentFilter.GetAssignmentGroups
			(
				section,
				assignments
			);

			Assert.Equal(3, results.Count);

			Assert.Equal("Group2", results[0].Key);
			Assert.Equal(3, results[0].Count());
			Assert.Equal(assignments[2], results[0].ElementAt(0));
			Assert.Equal(assignments[3], results[0].ElementAt(1));
			Assert.Equal(assignments[5], results[0].ElementAt(2));

			Assert.Equal("Group3", results[1].Key);
			Assert.Single(results[1]);
			Assert.Equal(assignments[4], results[1].ElementAt(0));

			Assert.Equal("Group1", results[2].Key);
			Assert.Equal(2, results[2].Count());
			Assert.Equal(assignments[0], results[2].ElementAt(0));
			Assert.Equal(assignments[1], results[2].ElementAt(1));
		}

		/// <summary>
		/// Ensures that FilterSubmissions returns all submissions of answers
		/// to questions for the given set of assignments.
		/// </summary>
		[Fact]
		public void FilterSubmissions_NoUserOrDateSpecified_ReturnsSubmissionsForAssignment()
		{
			var assignments = CreateAssignments(1);
			var submissions = Collections.CreateList
			(
				CreateSubmission(assignmentId: 1),
				CreateSubmission(assignmentId: 1),
				CreateSubmission(assignmentId: 2)
			);

			var assignmentFilter = new AssignmentFilter();
			var results = assignmentFilter.FilterSubmissions
			(
				assignments,
				submissions
			);

			Assert.Equal(2, results.Count);
			Assert.Equal(submissions[0], results[0]);
			Assert.Equal(submissions[1], results[1]);
		}

		/// <summary>
		/// Ensures that FilterSubmissions called with a user returns all submissions 
		/// of answers to questions for the given set of assignments, submitted by the
		/// given user.
		/// </summary>
		[Fact]
		public void FilterSubmissions_UserSpecified_ReturnsUserSubmissionsForAssignment()
		{
			var assignments = CreateAssignments(1);
			var user = new User() { Id = 10 };
			var submissions = Collections.CreateList
			(
				CreateSubmission(assignmentId: 1, userId: 10),
				CreateSubmission(assignmentId: 1, userId: 20),
				CreateSubmission(assignmentId: 2, userId: 10)
			);

			var assignmentFilter = new AssignmentFilter();
			var results = assignmentFilter.FilterSubmissions
			(
				assignments,
				submissions,
				user
			);

			Assert.Equal(1, results.Count);
			Assert.Equal(submissions[0], results[0]);
		}

		/// <summary>
		/// Ensures that FilterSubmissions called with a snapshot date returns all 
		/// submissions of answers to questions for the given set of assignments, 
		/// submitted up through the given date.
		/// </summary>
		[Fact]
		public void FilterSubmissions_DateSpecified_ReturnsSubmissionsForAssignmentThroughDate()
		{
			var assignments = CreateAssignments(1);
			var submissions = Collections.CreateList
			(
				CreateSubmission(assignmentId: 1, userId: 10, dateSubmitted: GetDate(1)),
				CreateSubmission(assignmentId: 1, userId: 20, dateSubmitted: GetDate(1)),
				CreateSubmission(assignmentId: 2, userId: 10, dateSubmitted: GetDate(2))
			);

			var assignmentFilter = new AssignmentFilter();
			var results = assignmentFilter.FilterSubmissions
			(
				assignments,
				submissions,
				user: null,
				snapshotDate: GetDate(1)
			);

			Assert.Equal(2, results.Count);
			Assert.Equal(submissions[0], results[0]);
			Assert.Equal(submissions[1], results[1]);
		}

		/// <summary>
		/// Ensures that FilterSubmissions called with a user and snapshot date returns 
		/// all submissions of answers to questions for the given set of assignments,
		/// submitted by the given user up through the given date.
		/// </summary>
		[Fact]
		public void FilterSubmissions_UserAndDateSpecified_ReturnsUserSubmissionsForAssignmentThroughDate()
		{
			var assignments = CreateAssignments(1);
			var user = new User() { Id = 10 };
			var submissions = Collections.CreateList
			(
				CreateSubmission(assignmentId: 1, userId: 10, dateSubmitted: GetDate(1)),
				CreateSubmission(assignmentId: 1, userId: 20, dateSubmitted: GetDate(1)),
				CreateSubmission(assignmentId: 2, userId: 10, dateSubmitted: GetDate(2))
			);

			var assignmentFilter = new AssignmentFilter();
			var results = assignmentFilter.FilterSubmissions
			(
				assignments,
				submissions,
				user,
				snapshotDate: GetDate(1)
			);

			Assert.Equal(1, results.Count);
			Assert.Equal(submissions[0], results[0]);
		}

		/// <summary>
		/// Creates a submission for a question with the given assignment.
		/// </summary>
		private UserQuestionSubmission CreateSubmission(
			int assignmentId,
			int? userId = null,
			DateTime? dateSubmitted = null)
		{
			return new UserQuestionSubmission()
			{
				UserQuestionData = new UserQuestionData()
				{
					UserId = userId ?? -1,
					AssignmentQuestion = new AssignmentQuestion()
					{
						AssignmentId = assignmentId
					}
				},
				DateSubmitted = dateSubmitted ?? DateTime.MaxValue
			};
		}

		/// <summary>
		/// Creates an assignment with the given ID.
		/// </summary>
		private IList<Assignment> CreateAssignments(params int[] assignmentIds)
		{
			return assignmentIds
				.Select(id => new Assignment() {Id = id})
				.ToList();
		}

		/// <summary>
		/// Creates an assignment with the given group name, 
		/// section, and due date.
		/// </summary>
		private Assignment CreateAssignment(
			string groupName,
			Section section = null, 
			DateTime? dueDate = null)
		{
			return new Assignment()
			{
				GroupName = groupName,
				Name = groupName,
				DueDates = section != null
					? Collections.CreateList
						(
							new AssignmentDueDate()
							{
								Section = section,
								SectionId = section.Id,
								DueDate = dueDate.Value
							}
						)
					: null
			};
		}

		/// <summary>
		/// Returns a due date with the given day.
		/// </summary>
		private DateTime GetDate(int day)
		{
			return new DateTime(2017, 1, day, 12, 0, 0);
		}
	}
}
