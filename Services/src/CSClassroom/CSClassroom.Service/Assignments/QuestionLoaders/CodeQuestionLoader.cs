using System.Threading.Tasks;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Repository;

namespace CSC.CSClassroom.Service.Assignments.QuestionLoaders
{
	/// <summary>
	/// Loads a code question.
	/// </summary>
	public abstract class CodeQuestionLoader<TQuestion> : QuestionLoader<TQuestion> where TQuestion : CodeQuestion
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		protected CodeQuestionLoader(DatabaseContext dbContext, TQuestion question) 
			: base(dbContext, question)
		{
		}

		/// <summary>
		/// Loads the class question.
		/// </summary>
		protected sealed override async Task LoadQuestionImplAsync()
		{
			await LoadCollectionAsync(q => q.ImportedClasses);
			await LoadCollectionAsync(q => q.CodeConstraints);

			await LoadCodeQuestionImplAsync();
		}

		/// <summary>
		/// Loads any properties specific to the question type.
		/// </summary>
		protected abstract Task LoadCodeQuestionImplAsync();
	}
}
