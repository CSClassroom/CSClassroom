using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace CSC.CSClassroom.Model.Questions
{
	/// <summary>
	/// A required method for a class question.
	/// </summary>
	public class RequiredMethod
	{
		/// <summary>
		/// The primary key.
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// The ID of the class question for this required method.
		/// </summary>
		public int ClassQuestionId { get; set; }

		/// <summary>
		/// The class question for this required method.
		/// </summary>
		public virtual ClassQuestion ClassQuestion { get; set; }

		/// <summary>
		/// The name of the required method.
		/// </summary>
		[Display(Name = "Method Name")]
		public string Name { get; set; }

		/// <summary>
		/// The required parameter types, separated by commas.
		/// </summary>
		[Display(Name = "Parameter Types")]
		public string ParamTypes { get; set; }

		/// <summary>
		/// The return type (null for a constructor).
		/// </summary>
		[Display(Name = "Return Type")]
		public string ReturnType { get; set; }

		/// <summary>
		/// Whether or not the method must be public.
		/// </summary>
		[Display(Name = "Public")]
		public bool IsPublic { get; set; }

		/// <summary>
		/// Whether or not the method must be static.
		/// </summary>
		[Display(Name = "Static")]
		public bool IsStatic { get; set; }

		/// <summary>
		/// A list of parameter types.
		/// </summary>
		public IList<string> ParamTypeList => 
			ParamTypes
				?.Split(',')
				?.Select(paramType => paramType.Trim())
				?.ToList() ?? new List<string>();
	}
}
