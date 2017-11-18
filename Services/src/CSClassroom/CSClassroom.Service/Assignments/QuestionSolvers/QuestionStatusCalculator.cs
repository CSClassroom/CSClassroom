using System.Linq;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Model.Assignments.ServiceResults;
using CSC.CSClassroom.Model.Users;

namespace CSC.CSClassroom.Service.Assignments.QuestionSolvers
{
	/// <summary>
	/// Calculates the number of attempts remaining for a user answering a question.
	/// </summary>
	public class QuestionStatusCalculator : IQuestionStatusCalculator
	{
		/// <summary>
		/// Returns information about the user's attempts for a given question,
		/// and whether future attempts are permitted. 
		/// </summary>
		public UserQuestionStatus GetQuestionStatus(UserQuestionData uqd)
		{
			return new UserQuestionStatus
			(
				GetNumAttempts(uqd),
				AnyCorrectSubmissions(uqd),
				GetAttemptsRemaining(uqd)
			);
		}

		/// <summary>
		/// Returns whether or not the user is an admin for the class.
		/// </summary>
		private bool IsClassAdmin(UserQuestionData uqd)
		{
			var classroom = uqd.AssignmentQuestion.Assignment.Classroom;
			
			return uqd.User
				.ClassroomMemberships
				.SingleOrDefault(cm => cm.Classroom == classroom)
				?.Role >= ClassroomRole.Admin;
		}

		/// <summary>
		/// Returns whether or not to allow attempts after a correct submission.
		/// </summary>
		private bool AllowAttemptsAfterCorrectSubmission(UserQuestionData uqd)
		{
			return uqd.AssignmentQuestion.IsInteractive() 
				|| uqd.AssignmentQuestion.Assignment.CombinedSubmissions;
		}

		/// <summary>
		/// Returns whether the user has any correct submissions for this question.
		/// </summary>
		private bool AnyCorrectSubmissions(UserQuestionData uqd)
		{
			return uqd.Submissions?.Any(s => s.Score == 1.0) ?? false;
		}

		/// <summary>
		/// Returns the number of attempts the user has made so far.
		/// </summary>
		private int GetNumAttempts(UserQuestionData uqd)
		{
			return uqd.NumAttempts;
		}

		/// <summary>
		/// Returns the maximum number of attempts for the given question.
		/// </summary>
		private int? GetMaxAttempts(UserQuestionData uqd)
		{
			return uqd.AssignmentQuestion.Assignment.MaxAttempts;
		}

		/// <summary>
		/// Returns the number of attempts remaining for the given question,
		/// or null if the user has an unlimited number of attempts remaining.
		/// </summary>
		private int? GetAttemptsRemaining(UserQuestionData uqd)
		{
			if (IsClassAdmin(uqd))
			{
				return null;
			}

			if (!AllowAttemptsAfterCorrectSubmission(uqd) && AnyCorrectSubmissions(uqd))
			{
				return 0;
			}

			return uqd.AssignmentQuestion
				.LimitedAttempts()
					? GetMaxAttempts(uqd) - GetNumAttempts(uqd)
					: null;
		}
	}
}