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
		/// This table's column info.
		/// </summary>
		public List<TableColumnInfo> Columns { get; set; }

		/// <summary>
		/// The child table's info (if any).
		/// </summary>
		public TableInfo ChildTableInfo { get; set; }

		/// <summary>
		/// Whether or not to show the header, with the column names.
		/// </summary>
		public bool ShowHeader { get; set; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public TableInfo(List<TableColumnInfo> columns, TableInfo childTableInfo, bool showHeader)
		{
			Columns = columns;
			ChildTableInfo = childTableInfo;
			ShowHeader = showHeader;
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		public TableInfo(Type type, bool showHeader)
		{
			Columns = GetColumns(type);
			ChildTableInfo = GetChildTable(type);
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
		private TableInfo GetChildTable(Type type)
		{
			var childTableProperties = type
				.GetTypeInfo()
				.DeclaredProperties.Where
				(
					prop => prop.PropertyType.GetTypeInfo().IsGenericType 
						&& prop.PropertyType.GetGenericTypeDefinition() == typeof(List<>)
				).ToList();

			if (!childTableProperties.Any())
				return null;

			if (childTableProperties.Count > 1)
				throw new InvalidOperationException("There can only be one child table for any given table.");

			var childTableProperty = childTableProperties[0];
			var jsonPropertyAttribute = childTableProperty.GetCustomAttribute<JsonPropertyAttribute>();
			if (jsonPropertyAttribute == null || jsonPropertyAttribute.PropertyName != "childTableData")
				throw new InvalidOperationException("The child table data must have a JsonProperty attribute with name ChildTableData.");
			var tableOptionsAttribute = childTableProperty.GetCustomAttribute<TableOptionsAttribute>();

			return new TableInfo
			(
				childTableProperty.PropertyType.GetGenericArguments()[0],
				tableOptionsAttribute?.ShowHeader ?? true
			);
		}
	}
}
