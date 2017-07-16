using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSC.CSClassroom.WebApp.ViewModels.Question
{
	/// <summary>
	/// Contains a list of question types.
	/// </summary>
	public class SelectTypeViewModel
	{
		/// <summary>
		/// The question types available to create.
		/// </summary>
		public IReadOnlyList<QuestionType> QuestionTypes { get; }

		/// <summary>
		/// The question category ID for the new question, if specified.
		/// </summary>
		public int? QuestionCategoryId { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public SelectTypeViewModel(IReadOnlyList<QuestionType> questionTypes, int? questionCategoryId)
		{
			QuestionTypes = questionTypes;
			QuestionCategoryId = questionCategoryId;
		}
	}
}
