namespace CSC.CodeRunner.Model
{
	/// <summary>
	/// The definition for a field on a class.
	/// </summary>
	public class FieldDefinition
	{
		/// <summary>
		/// The name of the field.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// The type of the field.
		/// </summary>
		public string Type { get; set; }

		/// <summary>
		/// Whether or not the field is static.
		/// </summary>
		public bool IsStatic { get; set; }

		/// <summary>
		/// Whether or not the field is public.
		/// </summary>
		public bool IsPublic { get; set; }
	}
}