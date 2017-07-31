using System;
using System.Collections.Generic;
using System.Text;

namespace CSC.CSClassroom.Model.Questions.ServiceResults
{
	/// <summary>
	/// The completion status of a question for a student.
	/// </summary>
	public enum QuestionCompletion
	{
		Completed,
		PartiallyCompleted,
		NotCompleted
	}

	/// <summary>
	/// The progress of a question for a student.
	/// </summary>
	public class QuestionProgress
	{
		/// <summary>
		/// The ID of the assignment question.
		/// </summary>
		public int AssignmentQuestionId { get; }

		/// <summary>
		/// The name of the assignment question.
		/// </summary>
		public string AssignmentQuestionName { get; }

		/// <summary>
		/// The completion status of the question.
		/// </summary>
		public QuestionCompletion Completion { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public QuestionProgress(
			int assignmentQuestionId,
			string assignmentQuestionName,
			QuestionCompletion completion)
		{
			AssignmentQuestionId = assignmentQuestionId;
			AssignmentQuestionName = assignmentQuestionName;
			Completion = completion;
		}
	}
}
