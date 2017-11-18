namespace CSC.CSClassroom.Model.Assignments.ServiceResults
{
	/// <summary>
	/// The status of a question for a given user.
	/// </summary>
	public class UserQuestionStatus
	{
		/// <summary>
		/// The number of attempts the user has made so far for the given question.
		/// </summary>
		public int NumAttempts { get; }
		
		/// <summary>
		/// Whether or not the question was previously answered correctly.
		/// </summary>
		public bool AnsweredCorrectly { get; }
		
		/// <summary>
		/// The number of attempts remaining, or null if unlimited.
		/// </summary>
		public int? NumAttemptsRemaining { get; }

		/// <summary>
		/// Whether or not a new attempt is permitted.
		/// </summary>
		public bool AllowNewAttempt => 
			!NumAttemptsRemaining.HasValue 
			|| NumAttemptsRemaining > 0;
		
		/// <summary>
		/// Constructor.
		/// </summary>
		public UserQuestionStatus(
			int numAttempts, 
			bool answeredCorrectly,
			int? numAttemptsRemaining)
		{
			NumAttempts = numAttempts;
			AnsweredCorrectly = answeredCorrectly;
			NumAttemptsRemaining = numAttemptsRemaining;
		}
	}
}