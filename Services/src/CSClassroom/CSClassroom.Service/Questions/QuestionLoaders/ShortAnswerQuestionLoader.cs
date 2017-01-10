using System.Threading.Tasks;
using CSC.CSClassroom.Model.Questions;
using CSC.CSClassroom.Repository;

namespace CSC.CSClassroom.Service.Questions.QuestionLoaders
{
	/// <summary>
	/// Loads a short answer question.
	/// </summary>
	public class ShortAnswerQuestionLoader : QuestionLoader<ShortAnswerQuestion>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public ShortAnswerQuestionLoader(DatabaseContext dbContext, ShortAnswerQuestion question) 
			: base(dbContext, question)
		{
		}

		/// <summary>
		/// Loads the multiple choice question.
		/// </summary>
		protected override async Task LoadQuestionImplAsync()
		{
			await LoadCollectionAsync(q => q.Blanks);
		}
	}
}
