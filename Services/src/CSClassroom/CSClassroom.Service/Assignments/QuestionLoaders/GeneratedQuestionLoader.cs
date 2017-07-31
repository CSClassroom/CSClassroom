using System.Threading.Tasks;
using CSC.CSClassroom.Model.Questions;
using CSC.CSClassroom.Repository;

namespace CSC.CSClassroom.Service.Questions.QuestionLoaders
{
	/// <summary>
	/// Loads a generated question.
	/// </summary>
	public class GeneratedQuestionLoader : CodeQuestionLoader<GeneratedQuestionTemplate>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public GeneratedQuestionLoader(DatabaseContext dbContext, GeneratedQuestionTemplate question) 
			: base(dbContext, question)
		{
		}

		/// <summary>
		/// Loads the generated question.
		/// </summary>
		protected override Task LoadCodeQuestionImplAsync()
		{
			return Task.CompletedTask;
		}
	}
}
