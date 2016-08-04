using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CSC.CSClassroom.Model.Classrooms
{
	/// <summary>
	/// A group of classrooms.
	/// </summary>
    public class Group
	{
		/// <summary>
		/// The unique ID for the group.
		/// </summary>
		[Required]
		public int Id { get; set; }

		/// <summary>
		/// The name of the group.
		/// </summary>
		[Required]
		[MaxLength(50)]
		[RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = ModelStrings.OnlyAlphanumeric)]
		[Display
		(
			Name = "Name",
			Description = "Enter the name of the group that will appear in URLs. This must be unique."
		)]
		public string Name { get; set; }

		/// <summary>
		/// The display name of the group.
		/// </summary>
		[Required]
		[Display
		(
			Name = "Display Name",
			Description = "Enter the name of the group to be displayed on the website."
		)]
		public string DisplayName { get; set; }

		/// <summary>
		/// The classrooms in this group.
		/// </summary>
		public ICollection<Classroom> Classrooms { get; set; }
	}
}
