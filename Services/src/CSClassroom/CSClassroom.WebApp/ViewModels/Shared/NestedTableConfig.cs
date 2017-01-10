using System;
using System.Collections.Generic;
using CSC.CSClassroom.WebApp.ViewHelpers.NestedTables;

namespace CSC.CSClassroom.WebApp.ViewModels.Shared
{
	/// <summary>
	/// Settings for a nested table.
	/// </summary>
	public class NestedTableConfig
	{
		/// <summary>
		/// The table DOM element ID.
		/// </summary>
		public string TableElementId { get; }

		/// <summary>
		/// The outer table entry type. (This cannot be derived from
		/// OuterTableEntries, since the list may be empty.)
		/// </summary>
		public Type OuterTableEntryType { get; }

		/// <summary>
		/// The table info.
		/// </summary>
		public TableInfo TableInfo { get; }

		/// <summary>
		/// The serializable data for this table.
		/// </summary>
		public IEnumerable<object> OuterTableEntries { get; }

		/// <summary>
		/// The text to show when there is an empty table.
		/// </summary>
		public string EmptyTableText { get; }

		/// <summary>
		/// Constructor
		/// </summary>
		public NestedTableConfig(
			string tableElementId,
			TableInfo tableInfo,
			IEnumerable<object> outerTableEntries,
			string emptyTableText)
		{
			TableElementId = tableElementId;
			TableInfo = tableInfo;
			OuterTableEntries = outerTableEntries;
			EmptyTableText = emptyTableText;
		}

		/// <summary>
		/// Constructor
		/// </summary>
		public NestedTableConfig(
			string tableElementId,
			Type tableEntityType,
			IEnumerable<object> outerTableEntries,
			string emptyTableText)
		{
			TableElementId = tableElementId;
			TableInfo = new TableInfo(tableEntityType, showHeader: true);
			OuterTableEntries = outerTableEntries;
			EmptyTableText = emptyTableText;
		}
	}
}
