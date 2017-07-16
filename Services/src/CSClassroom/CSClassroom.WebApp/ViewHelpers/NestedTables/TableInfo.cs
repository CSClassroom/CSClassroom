using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace CSC.CSClassroom.WebApp.ViewHelpers.NestedTables
{
	/// <summary>
	/// Table information.
	/// </summary>
	public class TableInfo
	{
		/// <summary>
		/// The type of the content in the table.
		/// </summary>
		public string Type { get; set; }

		/// <summary>
		/// This table's column info.
		/// </summary>
		public List<TableColumnInfo> Columns { get; set; }

		/// <summary>
		/// The child table's info (if more than one potential type).
		/// </summary>
		public List<TableInfo> ChildTableInfos { get; set; }

		/// <summary>
		/// Whether or not to show the header, with the column names.
		/// </summary>
		public bool ShowHeader { get; set; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public TableInfo(List<TableColumnInfo> columns, List<TableInfo> childTableInfos, bool showHeader)
		{
			Columns = columns;
			ChildTableInfos = childTableInfos;
			ShowHeader = showHeader;
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		public TableInfo(Type type, bool showHeader)
		{
			Type = type.FullName;
			Columns = GetColumns(type);
			ChildTableInfos = GetChildTableInfos(type);
			ShowHeader = showHeader;
		}

		/// <summary>
		/// Returns a list of table columns for the given type.
		/// </summary>
		private List<TableColumnInfo> GetColumns(Type type)
		{
			return type
				.GetTypeInfo()
				.DeclaredProperties.Where(IncludeColumn)
				.Select(propInfo => new TableColumnInfo(propInfo))
				.ToList();
		}

		/// <summary>
		/// Returns whether or not to include the column in the table.
		/// </summary>
		private bool IncludeColumn(PropertyInfo propInfo)
		{
			return propInfo.GetCustomAttribute<TableColumnAttribute>() != null;
		}

		/// <summary>
		/// Returns the child table info, if any.
		/// </summary>
		private List<TableInfo> GetChildTableInfos(Type type)
		{
			var childTableProperties = type
				.GetTypeInfo()
				.DeclaredProperties.Where
				(
					prop => prop.GetCustomAttribute<SubTableAttribute>() != null
				).ToList();

			if (!childTableProperties.Any())
				return null;

			if (childTableProperties.Count > 1)
				throw new InvalidOperationException("There can only be one child table for any given table.");
			
			var childTableProperty = childTableProperties[0];
			var jsonPropertyAttribute = childTableProperty.GetCustomAttribute<JsonPropertyAttribute>();
			if (jsonPropertyAttribute == null || jsonPropertyAttribute.PropertyName != "childTableData")
				throw new InvalidOperationException("The child table data must have a JsonProperty attribute with name childTableData.");
			var subTableAttribute = childTableProperty.GetCustomAttribute<SubTableAttribute>();
			var tableOptionsAttribute = childTableProperty.GetCustomAttribute<TableOptionsAttribute>();

			return subTableAttribute.SubTableTypes
				.Select
				(
					subTableType => new TableInfo
					(
						subTableType,
						tableOptionsAttribute?.ShowHeader ?? true
					)
				).ToList();
		}
	}
}
