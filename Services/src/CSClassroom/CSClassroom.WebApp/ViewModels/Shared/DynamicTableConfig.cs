using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace CSC.CSClassroom.WebApp.ViewModels.Shared
{
	/// <summary>
	/// Settings for a dynamic table.
	/// </summary>
	public class DynamicTableConfig
	{
		/// <summary>
		/// The table DOM element ID.
		/// </summary>
		public string TableElementId { get; }

		/// <summary>
		/// The name of the collection.
		/// </summary>
		public string CollectionName { get; }

		/// <summary>
		/// The columns of the table.
		/// </summary>
		public JArray Columns { get; }

		/// <summary>
		/// The initial data of the table.
		/// </summary>
		public JArray InitData { get; }

		/// <summary>
		/// The minimum number of rows to start with.
		/// </summary>
		public int StartMinRows { get; }

		/// <summary>
		/// Whether or not to use text areas.
		/// </summary>
		public bool TextAreas { get; }

		/// <summary>
		/// The drop down lists.
		/// </summary>
		public IList<DropDownList> DropDownLists { get; }

		/// <summary>
		/// The sub panel config.
		/// </summary>
		public SubPanelConfig SubPanelConfig { get; }

		/// <summary>
		/// Constructor
		/// </summary>
		public DynamicTableConfig(
			string tableElementId,
			string collectionName, 
			JArray columns,
			JArray initData, 
			int startMinRows,
			bool textAreas,
			IList<DropDownList> dropDownLists,
			SubPanelConfig subPanelConfig)
		{
			TableElementId = tableElementId;
			CollectionName = collectionName;
			Columns = columns;
			InitData = initData;
			StartMinRows = startMinRows;
			TextAreas = textAreas;
			DropDownLists = dropDownLists;
			SubPanelConfig = subPanelConfig;
		}
	}
}
