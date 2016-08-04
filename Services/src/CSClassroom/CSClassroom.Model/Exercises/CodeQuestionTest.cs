namespace CSC.CSClassroom.Model.Exercises
{
	/// <summary>
	/// A test for a code exercise.
	/// </summary>
	public abstract class CodeQuestionTest
	{
		/// <summary>
		///  The unique ID for the test.
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// The ID of the question.
		/// </summary>
		public int CodeQuestionId { get; set; }

		/// <summary>
		/// The question.
		/// </summary>
		public virtual CodeQuestion CodeQuestion { get; set; }

		/// <summary>
		/// The name of the test.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// The expected return value (or null if there is no return value).
		/// </summary>
		public string ExpectedReturnValue { get; set; }

		/// <summary>
		/// The expected output (or null if no output is expected).
		/// </summary>
		public string ExpectedOuptut { get; set; }
	}
}
