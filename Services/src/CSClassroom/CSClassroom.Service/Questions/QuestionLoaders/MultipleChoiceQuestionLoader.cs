using System.Threading.Tasks;
using CSC.CSClassroom.Model.Questions;
using CSC.CSClassroom.Repository;

namespace CSC.CSClassroom.Service.Questions.QuestionLoaders
{
	/// <summary>
	/// Loads a multiple choice question.
	/// </summary>
	public class MultipleChoiceQuestionLoader : QuestionLoader<MultipleChoiceQuestion>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public MultipleChoiceQuestionLoader(DatabaseContext dbContext, MultipleChoiceQuestion question) 
			: base(dbContext, question)
		{
		}

		/// <summary>
		/// Loads the multiple choice question.
		/// </summary>
		protected override async Task LoadQuestionImplAsync()
		{
			await LoadCollectionAsync(q => q.Choices);
		}
	}
}
