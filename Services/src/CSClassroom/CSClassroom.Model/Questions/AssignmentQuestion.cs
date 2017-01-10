using System.ComponentModel.DataAnnotations;

namespace CSC.CSClassroom.Model.Questions
{
	/// <summary>
	/// A question on an assignment.
	/// </summary>
	public class AssignmentQuestion
	{
		/// <summary>
		/// The ID of the assignment.
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// The assignment for this question.
		/// </summary>
		public int AssignmentId { get; set; }

		/// <summary>
		/// The assignment for this question.
		/// </summary>
		public virtual Assignment Assignment { get; set; }

		/// <summary>
		/// The actual question.
		/// </summary>
		[Display(Name = "Question")]
		public int QuestionId { get; set; }

		/// <summary>
		/// The actual question.
		/// </summary>
		public virtual Question Question { get; set; }

		/// <summary>
		/// The number of points this question is worth.
		/// </summary>
		[Display(Name = "Points")]
		public double Points { get; set; }

		/// <summary>
		/// The order the question appears in the assignment.
		/// </summary>
		public int Order { get; set; }
	}
}
