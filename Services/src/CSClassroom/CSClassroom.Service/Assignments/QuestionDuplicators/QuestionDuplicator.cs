using System;
using System.Collections.Generic;
using System.Linq;
using CSC.CSClassroom.Model.Questions;
using CSC.CSClassroom.Repository;
using Microsoft.EntityFrameworkCore;

namespace CSC.CSClassroom.Service.Questions.QuestionDuplicators
{
	/// <summary>
	/// Duplicates question objects.
	/// </summary>
	public abstract class QuestionDuplicator<TQuestion> : IQuestionDuplicator where TQuestion : Question
	{
		/// <summary>
		/// The database context.
		/// </summary>
		protected DatabaseContext DbContext { get; }

		/// <summary>
		/// The question to duplicate.
		/// </summary>
		protected TQuestion Question { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		protected QuestionDuplicator(DatabaseContext dbContext, TQuestion question)
		{
			DbContext = dbContext;
			Question = question;
		}

		/// <summary>
		/// Returns a duplicate question object.
		/// </summary>
		public Question DuplicateQuestion()
		{
			var duplicate = (TQuestion) DbContext.Entry(Question)
				.CurrentValues
				.ToObject();

			duplicate.Id = 0;

			PopulateQuestion(duplicate);

			return duplicate;
		}

		/// <summary>
		/// Populates navigation properties.
		/// </summary>
		private void PopulateQuestion(TQuestion duplicate)
		{
			duplicate.QuestionCategory = Question.QuestionCategory;

			PopulateQuestionImpl(duplicate);
		}

		/// <summary>
		/// Returns a duplicate list of objects, whose
		/// IDs have been reset to 0.
		/// </summary>
		protected List<TObject> DuplicateList<TObject>(
			IList<TObject> originalList,
			Action<TObject> resetId) where TObject : class
		{
			if (originalList == null)
			{
				return null;
			}

			var newList = new List<TObject>();

			foreach (var obj in originalList)
			{
				var newObj = (TObject)DbContext.Entry(obj)
					.CurrentValues
					.ToObject();

				resetId(newObj);

				newList.Add(newObj);
			}

			return newList;
		}

		/// <summary>
		/// Populates navigation properties specific to the question type.
		/// </summary>
		protected abstract void PopulateQuestionImpl(TQuestion duplicate);
	}
}
