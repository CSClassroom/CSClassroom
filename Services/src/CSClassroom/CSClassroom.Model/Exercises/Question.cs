using System.ComponentModel.DataAnnotations;

namespace CSC.CSClassroom.Model.Exercises
{
	/// <summary>
	/// An exercise.
	/// </summary>
	public abstract class Question
	{
		/// <summary>
		/// The unique ID for the exercise.
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// The ID of the category that contains this exercise.
		/// </summary>
		[Required]
		[Display
		(
			Name = "Category",
			Description = "Select the category for the question."
		)]
		public int QuestionCategoryId { get; set; }

		/// <summary>
		/// The name of the exercise.
		/// </summary>
		[Required]
		[MaxLength(50)]
		[Display
		(
			Name = "Name",
			Description = "Enter the name of the question."
		)]
		public string Name { get; set; }

		/// <summary>
		/// The description of the exericse.
		/// </summary>
		[Required]
		[Display
		(
			Name = "Description",
			Description = "Enter the description for the question."
		)]
		public string Description { get; set; }

		/// <summary>
		/// The category that contains this exercise.
		/// </summary>
		public QuestionCategory QuestionCategory { get; set; }

		/// <summary>
		/// The string displayed for the type of question
		/// </summary>
		public abstract string QuestionTypeDisplay { get; }

		/// <summary>
		/// The string displayed for the description of the question type
		/// </summary>
		public abstract string QuestionTypeDescription { get; }
	}
}
