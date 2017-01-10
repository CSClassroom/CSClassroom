using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CSC.CSClassroom.Model.Classrooms;

namespace CSC.CSClassroom.Model.Questions
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
		[MaxLength(50)]
		[Display
		(
			Name = "Name",
			Description = "Enter the name of the category."
		)]
		public string Name { get; set; }

		/// <summary>
		/// Whether or not the question is public.
		/// </summary>
		[Required]
		[Display
		(
			Name = "Private",
			Description = "Select whether or not the category should be hidden from view."
		)]
		public bool IsPrivate { get; set; }

		/// <summary>
		/// The classroom that contains this category.
		/// </summary>
		public virtual Classroom Classroom { get; set; }

		/// <summary>
		/// The exercises in this category.
		/// </summary>
		public virtual ICollection<Question> Questions { get; set; }
	}
}
