using System.Collections.Generic;

namespace CSC.BuildService.Model.CodeRunner
{
	/// <summary>
	/// The definition of the class that was compiled.
	/// </summary>
	public class ClassDefinition
	{
		/// <summary>
		/// The name of the class.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// The fields of the class.
		/// </summary>
		public List<FieldDefinition> Fields { get; set; }

		/// <summary>
		/// The methods of the class.
		/// </summary>
		public List<MethodDefinition> Methods { get; set; }
	}
}