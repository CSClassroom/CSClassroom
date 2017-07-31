using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CSC.CSClassroom.Model.Classrooms;

namespace CSC.CSClassroom.Model.Assignments
{
	/// <summary>
	/// A category of exercises.
	/// </summary>
	public class QuestionCategory
	{
		/// <summary>
		/// The unique ID for the category.
		/// </summary>
		[Required]
		public int Id { get; set; }

		/// <summary>
		/// The ID of the classroom that contains this category.
		/// </summary>
		[Required]
		public int ClassroomId { get; set; }

		/// <summary>
		/// The name of the category.
		/// </summary>
		[Required]
		[MaxLength(100)]
		[Display
		(
			Name = "Name",
			Description = "Enter the name of the category."
		)]
		public string Name { get; set; }

		/// <summary>
		/// The classroom that contains this category.
		/// </summary>
		public virtual Classroom Classroom { get; set; }

		/// <summary>
		/// The exercises in this category.
		/// </summary>
		public virtual ICollection<Question> Questions { get; set; }

		/// <summary>
		/// The randomly generated question that this category contains
		/// choices for (if any).
		/// </summary>
		public int? RandomlySelectedQuestionId { get; set; }

		/// <summary>
		/// The randomly generated question that this category contains
		/// choices for (if any).
		/// </summary>
		public RandomlySelectedQuestion RandomlySelectedQuestion { get; set; }
	}
}
