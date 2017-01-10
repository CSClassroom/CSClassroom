using System.Reflection;
using CSC.Common.Infrastructure.Extensions;

namespace CSC.CSClassroom.WebApp.ViewHelpers.NestedTables
{
	/// <summary>
	/// Table column information.
	/// </summary>
	public class TableColumnInfo
	{
		/// <summary>
		/// The column name.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// The column text.
		/// </summary>
		public string Text { get; set; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public TableColumnInfo(string name, string text)
		{
			Name = name;
			Text = text;
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		public TableColumnInfo(PropertyInfo propInfo)
		{
			Name = propInfo.Name.ToCamelCase();
			Text = propInfo.GetCustomAttribute<TableColumnAttribute>().ColumnText;
		}
	}
}
