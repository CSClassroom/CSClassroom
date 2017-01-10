using System;

namespace CSC.CSClassroom.WebApp.ViewHelpers.NestedTables
{
	/// <summary>
	/// A table column attribute.
	/// </summary>
	[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
	public class TableColumnAttribute : Attribute
	{
		/// <summary>
		/// The column text.
		/// </summary>
		public string ColumnText { get; private set; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public TableColumnAttribute(string columnText)
		{
			ColumnText = columnText;
		}
	}
}
