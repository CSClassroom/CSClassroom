using System.ComponentModel.DataAnnotations;

namespace CSC.CSClassroom.Model.Questions
{
	public class ImportedClass
	{
		/// <summary>
		/// The unique ID for the imported class.
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// The ID of the code question containing this import.
		/// </summary>
		public int CodeQuestionId { get; set; }

		/// <summary>
		/// The code question containing this import.
		/// </summary>
		public CodeQuestion CodeQuestion { get; set; }

		/// <summary>
		/// The class name (or wildcard).
		/// </summary>
		[Required]
		[Display
		(
			Name = "Class Name (or wildcard)"
		)]
		public string ClassName { get; set; }
	}
}
