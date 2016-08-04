using CSC.CSClassroom.Model.Exercises;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CSC.CSClassroom.Model.Classrooms
{
	/// <summary>
	/// A classroom.
	/// </summary>
    public class Classroom
	{
		/// <summary>
		/// The unique ID for the classroom.
		/// </summary>
		[Required]
		public int Id { get; set; }

		/// <summary>
		/// The ID of the group that contains this classroom.
		/// </summary>
		[Required]
		[Display
		(
			Name = "Group",
			Description = "Select the category for the question."
		)]
		public int GroupId { get; set; }

		/// <summary>
		/// The name of the group.
		/// </summary>
		[Required]
		[MaxLength(50)]
		[RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = ModelStrings.OnlyAlphanumeric)]
		[Display
		(
			Name = "Name",
			Description = "Enter the name of the classroom that will appear in URLs. This must be unique within the group."
		)]
		public string Name { get; set; }

		/// <summary>
		/// The display name of the group.
		/// </summary>
		[Required]
		[Display
		(
			Name = "Display Name",
			Description = "Enter the name of the classroom to be displayed on the website."
		)]
		public string DisplayName { get; set; }

		/// <summary>
		/// The group that contains this classroom.
		/// </summary>
		public virtual Group Group { get; set; }

		/// <summary>
		/// The categories of exercises available in this classroom.
		/// </summary>
		public ICollection<QuestionCategory> Categories { get; set; }
	}
}
