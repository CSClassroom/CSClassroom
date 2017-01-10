using CSC.CSClassroom.WebApp.ViewHelpers.NestedTables;
using Newtonsoft.Json.Linq;

namespace CSC.CSClassroom.WebApp.ViewModels.Build
{
	/// <summary>
	/// Section build results
	/// </summary>
	public class SectionBuildResultsViewModel
	{
		/// <summary>
		/// The section being shown.
		/// </summary>
		public string SectionName { get; }

		/// <summary>
		/// Build table info
		/// </summary>
		public TableInfo BuildTableInfo { get; }

		/// <summary>
		/// The builds in the table.
		/// </summary>
		public JArray BuildTableData { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public SectionBuildResultsViewModel(
			string sectionName, 
			TableInfo buildTableInfo, 
			JArray buildTableData)
		{
			SectionName = sectionName;
			BuildTableInfo = buildTableInfo;
			BuildTableData = buildTableData;
		}
	}
}
