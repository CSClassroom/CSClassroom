using System;
using System.Collections.Generic;
using System.Linq;

namespace CSC.CSClassroom.WebApp.ViewHelpers.NestedTables
{
	/// <summary>
	/// A sub table attribute.
	/// </summary>
	[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
	public class SubTableAttribute : Attribute
	{
		/// <summary>
		/// Types of subtables for this property.
		/// </summary>
		public IList<Type> SubTableTypes { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public SubTableAttribute(params Type[] subTableTypes)
		{
			SubTableTypes = subTableTypes.ToList();
		}
	}
}
