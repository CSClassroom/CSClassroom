using System.Collections.Generic;
using System.Linq;
using CSC.CSClassroom.WebApp.ViewHelpers.NestedTables;
using Microsoft.AspNetCore.Mvc;

namespace CSC.CSClassroom.WebApp.ViewModels.Question
{
	/// <summary>
	/// A type of question.
	/// </summary>
	public class QuestionListItem
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public QuestionListItem(Model.Assignments.Question question, IEnumerable<string> actions, IUrlHelper urlHelper)
		{
			Name = question.Name;
			ActionLinks = string.Join
			(
				"|", 
				actions.Select
				(
					action => $"<a href=\"{urlHelper.Action(action, new { id = question.Id })}\" target=\"_blank\"></a>"
				)
			);
		}

		/// <summary>
		/// The name of the question.
		/// </summary>
		[TableColumn("Question")]
		public string Name { get; }

		/// <summary>
		/// The actions one can take with this question.
		/// </summary>
		[TableColumn("Actions")]
		public string ActionLinks { get; }

	}
}
