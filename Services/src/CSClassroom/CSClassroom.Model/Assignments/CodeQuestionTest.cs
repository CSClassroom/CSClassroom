using System.ComponentModel.DataAnnotations;

namespace CSC.CSClassroom.Model.Assignments
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
		/// The name of the test.
		/// </summary>
		[Display(Name = "Test Name")]
		public string Name { get; set; }

		/// <summary>
		/// The order the test is displayed in.
		/// </summary>
		[Required]
		public int Order { get; set; }

		/// <summary>
		/// The expected return value (or null if there is no return value).
		/// </summary>
		[Display(Name = "Return Value")]
		public string ExpectedReturnValue { get; set; }

		/// <summary>
		/// The expected output (or null if no output is expected).
		/// </summary>
		[Display(Name = "Output")]
		public string ExpectedOutput { get; set; }
	}
}
