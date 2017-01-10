using System.Collections.Generic;

namespace CSC.BuildService.Model.CodeRunner
{
	/// <summary>
	/// The definition of a method on a class.
	/// </summary>
	public class MethodDefinition
	{
		/// <summary>
		/// The name of the method.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// The parameter types for the method.
		/// </summary>
		public List<string> ParameterTypes { get; set; }

		/// <summary>
		/// The return type of the method.
		/// </summary>
		public string ReturnType { get; set; }

		/// <summary>
		/// Whether or not the method is static.
		/// </summary>
		public bool IsStatic { get; set; }

		/// <summary>
		/// Whether or not the method is public.
		/// </summary>
		public bool IsPublic { get; set; }
	}
}