using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSC.CSClassroom.Model.Exercises;

namespace CSC.CSClassroom.WebApp.ViewModels.Question
{
	using Question = Model.Exercises.Question;

	/// <summary>
	/// A type of question.
	/// </summary>
	public class QuestionType
	{
		/// <summary>
		/// The type of question referred to by this object.
		/// </summary>
		public Type Type { get; private set; }

		/// <summary>
		/// The display name of the type of question.
		/// </summary>
		public string DisplayName { get; private set; }

		/// <summary>
		/// The description of the type of question.
		/// </summary>
		public string Description { get; private set; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public QuestionType(Type type)
		{
			Type = type;

			var question = (Question)Activator.CreateInstance(type);
			DisplayName = question.QuestionTypeDisplay;
			Description = question.QuestionTypeDescription;
		}
    }
}
