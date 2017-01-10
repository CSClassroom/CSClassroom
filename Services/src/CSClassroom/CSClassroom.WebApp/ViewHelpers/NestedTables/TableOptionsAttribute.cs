using System;

namespace CSC.CSClassroom.WebApp.ViewHelpers.NestedTables
{
	/// <summary>
	/// Options for displaying a table.
	/// </summary>
	[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
	public class TableOptionsAttribute : Attribute
	{
		/// <summary>
		/// Whether or not to show the header with the column names.
		/// </summary>
		public bool ShowHeader { get; set; }
	}
}
