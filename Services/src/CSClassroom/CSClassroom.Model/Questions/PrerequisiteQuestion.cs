using System.ComponentModel.DataAnnotations;

namespace CSC.CSClassroom.Model.Questions
{
	/// <summary>
	/// A question that is a prerequisite for another question.
	/// </summary>
	public class PrerequisiteQuestion
	{
		/// <summary>
		/// The ID of the prerequisite question.
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// The question that must be answered first.
		/// </summary>
		[Display(Name = "Question")]
		public int FirstQuestionId { get; set; }

		/// <summary>
		/// The question that must be answered first.
		/// </summary>
		public virtual Question FirstQuestion { get; set; }

		/// <summary>
		/// The question that may only be answered after the first question.
		/// </summary>
		public int SecondQuestionId { get; set; }

		/// <summary>
		/// The question that may only be answered after the first question.
		/// </summary>
		public virtual Question SecondQuestion { get; set; }

		/// <summary>
		/// The order the question appears in the assignment.
		/// </summary>
		public int Order { get; set; }
	}
}
