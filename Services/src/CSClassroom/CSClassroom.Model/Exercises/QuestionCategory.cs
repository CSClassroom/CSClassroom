using CSC.CSClassroom.Model.Classrooms;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CSC.CSClassroom.Model.Exercises
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
		/// The ID of the group that contains this category.
		/// </summary>
		[Required]
		public int GroupId { get; set; }

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
		/// The group that contains this category.
		/// </summary>
		public virtual Group Group { get; set; }

		/// <summary>
		/// The exercises in this category.
		/// </summary>
		public virtual ICollection<Question> Questions { get; set; }
	}
}
