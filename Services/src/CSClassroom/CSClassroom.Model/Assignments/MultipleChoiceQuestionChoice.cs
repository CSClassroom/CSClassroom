using System.ComponentModel.DataAnnotations;

namespace CSC.CSClassroom.Model.Assignments
{
	/// <summary>
	/// An option for a multiple choice question.
	/// </summary>
	public class MultipleChoiceQuestionChoice
	{
		/// <summary>
		/// The ID of this multiple choice question option.
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// The ID of the multiple choice question this option belongs to.
		/// </summary>
		public int MultipleChoiceQuestionId { get; set; }

		/// <summary>
		/// The multiple choice question this option belongs to.
		/// </summary>
		public MultipleChoiceQuestion MultipleChoiceQuestion { get; set; }

		/// <summary>
		/// The value of the choice.
		/// </summary>
		[Display(Name = "Value")]
		public string Value { get; set; }

		/// <summary>
		/// Whether or not this choice is correct.
		/// </summary>
		[Display(Name = "Correct")]
		public bool Correct { get; set; }

		/// <summary>
		/// An explanation for the choice.
		/// </summary>
		[Display(Name = "Explanation")]
		public string Explanation { get; set; }

		/// <summary>
		/// The order.
		/// </summary>
		public int Order { get; set; }
	}
}
