using System.Threading.Tasks;
using CSC.CSClassroom.Model.Questions;
using CSC.CSClassroom.Repository;

namespace CSC.CSClassroom.Service.Questions.QuestionLoaders
{
	/// <summary>
	/// Loads a method question.
	/// </summary>
	public class MethodQuestionLoader : CodeQuestionLoader<MethodQuestion>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public MethodQuestionLoader(DatabaseContext dbContext, MethodQuestion question) 
			: base(dbContext, question)
		{
		}

		/// <summary>
		/// Loads the method question.
		/// </summary>
		protected override async Task LoadCodeQuestionImplAsync()
		{
			await LoadCollectionAsync(q => q.Tests);
		}
	}
}
