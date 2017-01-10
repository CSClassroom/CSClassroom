using System.ComponentModel.DataAnnotations;

namespace CSC.CSClassroom.Model.Questions
{
	/// <summary>
	/// A single blank for a short answer question.
	/// </summary>
	public class ShortAnswerQuestionBlank
	{
		/// <summary>
		/// The ID of this short answer question blank.
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// The ID of the short answer question this blank belongs to.
		/// </summary>
		public int ShortAnswerQuestionId { get; set; }

		/// <summary>
		/// The short answer question this blank belongs to.
		/// </summary>
		public ShortAnswerQuestion ShortAnswerQuestion { get; set; }

		/// <summary>
		/// The name of the blank.
		/// </summary>
		[Display(Name = "Name")]
		public string Name { get; set; }

		/// <summary>
		/// The answer.
		/// </summary>
		[Display(Name = "Answer")]
		public string Answer { get; set; }

		/// <summary>
		/// The order of the blanks.
		/// </summary>
		public int Order { get; set; }
	}
}
