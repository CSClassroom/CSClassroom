using System.ComponentModel.DataAnnotations;

namespace CSC.CSClassroom.Model.Questions
{
	/// <summary>
	/// A test for a class exercise.
	/// </summary>
	public class ClassQuestionTest : CodeQuestionTest
	{
		/// <summary>
		/// The ID of the question.
		/// </summary>
		public int ClassQuestionId { get; set; }

		/// <summary>
		/// The question.
		/// </summary>
		public virtual ClassQuestion ClassQuestion { get; set; }

		/// <summary>
		/// A description of what the test does.
		/// </summary>
		[Required]
		[Display(Name = "Description")]
		public string Description { get; set; }

		/// <summary>
		/// The method body to execute for the test.
		/// </summary>
		[Display(Name = "Method Body")]
		public string MethodBody { get; set; }

		/// <summary>
		/// The return type of the test method.
		/// </summary>
		[Required]
		[Display(Name = "Return Type")]
		public string ReturnType { get; set; }
	}
}
