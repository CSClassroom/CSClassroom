namespace CSC.CSClassroom.Service.Assignments.QuestionGeneration
{
	/// <summary>
	/// The result of attempting to generate a question.
	/// </summary>
	public class QuestionGenerationResult
	{
		/// <summary>
		/// The generated question, if successful.
		/// </summary>
		public string SerializedQuestion { get; }

		/// <summary>
		/// The full file used to generate the question.
		/// </summary>
		public string FullGeneratorFileContents { get; }

		/// <summary>
		/// The line offset in the file used to generate the question.
		/// </summary>
		public int FullGeneratorFileLineOffset { get; }

		/// <summary>
		/// The error text, if the question could not be generated.
		/// </summary>
		public string Error { get; }

		/// <summary>
		/// The seed used to generate the question.
		/// </summary>
		public int? Seed { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public QuestionGenerationResult(
			string serializedQuestion,
			string fullGeneratorFileContents,
			int fullGeneratorFileLineOffset,
			int seed)
		{
			SerializedQuestion = serializedQuestion;
			FullGeneratorFileContents = fullGeneratorFileContents;
			FullGeneratorFileLineOffset = fullGeneratorFileLineOffset;
			Seed = seed;
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		public QuestionGenerationResult(string error)
		{
			Error = error;
		}
	}
}
