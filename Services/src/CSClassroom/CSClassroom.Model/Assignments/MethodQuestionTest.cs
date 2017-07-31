using System.ComponentModel.DataAnnotations;

namespace CSC.CSClassroom.Model.Questions
{
	/// <summary>
	/// A test for a method exercise.
	/// </summary>
	public class MethodQuestionTest : CodeQuestionTest
	{
		/// <summary>
		/// The ID of the question.
		/// </summary>
		public int MethodQuestionId { get; set; }

		/// <summary>
		/// The question.
		/// </summary>
		public virtual MethodQuestion MethodQuestion { get; set; }

		/// <summary>
		/// The parameter values.
		/// </summary>
		[Display(Name = "Parameter Values")]
		public string ParameterValues { get; set; }
	}
}
