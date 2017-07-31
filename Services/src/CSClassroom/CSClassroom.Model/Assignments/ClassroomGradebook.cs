using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CSC.CSClassroom.Model.Classrooms;

namespace CSC.CSClassroom.Model.Questions
{
	/// <summary>
	/// A gradebook for assignments.
	/// </summary>
	public class ClassroomGradebook
	{
		/// <summary>
		/// The unique ID for the gradebook.
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// The classroom ID.
		/// </summary>
		public int ClassroomId { get; set; }

		/// <summary>
		/// The classroom.
		/// </summary>
		public virtual Classroom Classroom { get; set; }

		/// <summary>
		/// The name of the gradebook.
		/// </summary>
		[Required]
		[Display(Name = "Name")]
		public string Name { get; set; }

		/// <summary>
		/// The order the gradebook appears in the list of gradebooks.
		/// </summary>
		public int Order { get; set; }

		/// <summary>
		/// Section gradebooks.
		/// </summary>
		public virtual IList<SectionGradebook> SectionGradebooks { get; set; }
	}
}
