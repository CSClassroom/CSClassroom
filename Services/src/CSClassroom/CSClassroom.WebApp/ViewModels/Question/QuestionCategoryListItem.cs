using System.Collections.Generic;
using CSC.CSClassroom.WebApp.ViewHelpers.NestedTables;
using Newtonsoft.Json;

namespace CSC.CSClassroom.WebApp.ViewModels.Question
{
	/// <summary>
	/// A type of question.
	/// </summary>
	public class QuestionCategoryListItem
	{
		/// <summary>
		/// The name of the category.
		/// </summary>
		[TableColumn("Category")]
		public string Name { get; }

		/// <summary>
		/// The questions in the category.
		/// </summary>
		[SubTable(typeof(QuestionListItem))]
		[JsonProperty(PropertyName = "ChildTableData")]
		public List<QuestionListItem> Questions { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public QuestionCategoryListItem(string name, List<QuestionListItem> questions)
		{
			Name = name;
			Questions = questions;
		}
	}
}
