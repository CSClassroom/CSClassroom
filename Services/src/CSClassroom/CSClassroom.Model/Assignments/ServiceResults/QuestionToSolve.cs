using System;
using System.Collections.Generic;
using System.Linq;
using CSC.CSClassroom.Model.Users;

namespace CSC.CSClassroom.Model.Assignments.ServiceResults
{
	/// <summary>
	/// A question to solve.
	/// </summary>
	public class QuestionToSolve
	{
		/// <summary>
		/// The assignment question ID.
		/// </summary>
		public int AssignmentQuestionId { get; }

		/// <summary>
		/// The name of the question for this assignment.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// The question to solve.
		/// </summary>
		public Question Question { get; }

		/// <summary>
		/// The seed used to generate the question (if any).
		/// </summary>
		public int? Seed { get; }

		/// <summary>
		/// The user solving the question (if other than the current user)
		/// </summary>
		public User User { get; }

		/// <summary>
		/// The most recent submission.
		/// </summary>
		public QuestionSubmission LastSubmission { get; }

		/// <summary>
		/// Whether or not the user can continue to resubmit
		/// from the same page after an incorrect answer.
		/// </summary>
		public bool Interactive { get; }

		/// <summary>
		/// The status of the question for the user.
		/// </summary>
		public UserQuestionStatus Status { get; }

		/// <summary>
		/// A list of past submissions for this question.
		/// </summary>
		public IList<DateTime> PastSubmissions { get; }

		/// <summary>
		/// The progress for questions in this assignment.
		/// </summary>
		public AssignmentProgress AssignmentProgress { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public QuestionToSolve(
			int assignmentQuestionId,
			string name,
			Question question, 
			int? seed,
			User user,
			QuestionSubmission questionSubmission,
			bool interactive,
			IList<DateTime> pastSubmissions,
			UserQuestionStatus userQuestionStatus,
			AssignmentProgress assignmentProgress)
		{
			AssignmentQuestionId = assignmentQuestionId;
			Name = name;
			Question = question;
			Seed = seed;
			User = user;
			LastSubmission = questionSubmission;
			Interactive = interactive;
			PastSubmissions = pastSubmissions;
			Status = userQuestionStatus;
			AssignmentProgress = assignmentProgress;
		}
	}
}
