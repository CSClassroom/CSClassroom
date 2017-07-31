using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSC.CSClassroom.Model.Questions.ServiceResults
{
	/// <summary>
	/// The progress of an assignment for a student.
	/// </summary>
	public class AssignmentProgress
	{
		/// <summary>
		/// The user ID.
		/// </summary>
		public int UserId { get; }

		/// <summary>
		/// The current question ID.
		/// </summary>
		public int? CurrentAssignmentQuestionId { get; }

		/// <summary>
		/// The progress for each question in the assignment.
		/// </summary>
		public List<QuestionProgress> Questions { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public AssignmentProgress(
			int userId,
			int currentAssignmentQuestionId,
			List<QuestionProgress> questions)
		{
			UserId = userId;
			CurrentAssignmentQuestionId = currentAssignmentQuestionId;
			Questions = questions;
		}

		/// <summary>
		/// Returns any questions prior to the current question
		/// that remain unsolved.
		/// </summary>
		public IList<QuestionProgress> GetUnsolvedPriorQuestions()
		{
			int curQuestionIndex = Questions
				.FindIndex(qp => qp.AssignmentQuestionId == CurrentAssignmentQuestionId);

			return Questions
				.Where
				(
					(qp, index) => index < curQuestionIndex
								   && qp.Completion != QuestionCompletion.Completed
				).ToList();
		}

		/// <summary>
		/// The previous question in the assignment, if any.
		/// </summary>
		public int? PreviousAssignmentQuestionId
		{
			get
			{
				var currentIndex = CurrentQuestionIndex;
				return currentIndex >= 1
					? Questions[currentIndex - 1].AssignmentQuestionId
					: (int?)null;
			}
		}

		/// <summary>
		/// The next question in the assignment, if any.
		/// </summary>
		public int? NextAssignmentQuestionId
		{
			get
			{
				var currentIndex = CurrentQuestionIndex;
				return currentIndex < Questions.Count - 1
					? Questions[currentIndex + 1].AssignmentQuestionId
					: (int?)null;
			}
		}

		/// <summary>
		/// The index of the current question in the list of questions.
		/// </summary>
		private int CurrentQuestionIndex =>
			Questions.FindIndex
			(
				qp => qp.AssignmentQuestionId == CurrentAssignmentQuestionId
			);

	}
}
