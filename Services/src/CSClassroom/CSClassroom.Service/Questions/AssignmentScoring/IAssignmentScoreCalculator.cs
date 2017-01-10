using System;
using System.Collections.Generic;
using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Model.Questions;
using CSC.CSClassroom.Model.Questions.ServiceResults;
using CSC.CSClassroom.Model.Users;

namespace CSC.CSClassroom.Service.Questions.AssignmentScoring
{
	/// <summary>
	/// Calculates student assignment scores.
	/// </summary>
	public interface IAssignmentScoreCalculator
	{
		/// <summary>
		/// Calculates the scores for a single assignment in a single section,
		/// for all students.
		/// </summary>
		SectionAssignmentResults GetSectionAssignmentResults(
			string assignmentGroupName,
			IList<Assignment> assignments,
			Section section,
			IList<User> users,
			IList<UserQuestionSubmission> submissions);

		/// <summary>
		/// Calculates the scores of all assignments for a given student.
		/// </summary>
		StudentAssignmentResults GetStudentAssignmentResults(
			User user,
			Section section,
			IList<Assignment> assignments,
			IList<UserQuestionSubmission> submissions);

		/// <summary>
		/// Calculates the scores for all assignments updated since the last time 
		/// assignments were marked as graded, for a given section.
		/// </summary>
		UpdatedSectionAssignmentResults GetUpdatedAssignmentResults(
			IList<Assignment> assignments,
			IList<User> users,
			Section section,
			string gradebookName,
			DateTime lastTransferDate,
			IList<UserQuestionSubmission> submissions);
	}
}
