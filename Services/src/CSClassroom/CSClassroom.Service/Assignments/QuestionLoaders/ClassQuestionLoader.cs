using System.Threading.Tasks;
using CSC.CSClassroom.Model.Questions;
using CSC.CSClassroom.Repository;

namespace CSC.CSClassroom.Service.Questions.QuestionLoaders
{
	/// <summary>
	/// Loads a class question.
	/// </summary>
	public class ClassQuestionLoader : CodeQuestionLoader<ClassQuestion>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public ClassQuestionLoader(DatabaseContext dbContext, ClassQuestion question) 
			: base(dbContext, question)
		{
		}

		/// <summary>
		/// Loads the class question.
		/// </summary>
		protected override async Task LoadCodeQuestionImplAsync()
		{
			await LoadCollectionAsync(q => q.Tests);
			await LoadCollectionAsync(q => q.RequiredMethods);
		}
	}
}
