using System.Threading.Tasks;
using CSC.CSClassroom.Model.Questions;
using CSC.CSClassroom.Repository;

namespace CSC.CSClassroom.Service.Questions.QuestionLoaders
{
	/// <summary>
	/// Loads a randomly selected question.
	/// </summary>
	public class RandomlySelectedQuestionLoader : QuestionLoader<RandomlySelectedQuestion>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public RandomlySelectedQuestionLoader(DatabaseContext dbContext, RandomlySelectedQuestion question) 
			: base(dbContext, question)
		{
		}

		/// <summary>
		/// Performs any question-type-specific loading operations.
		/// </summary>
		protected override async Task LoadQuestionImplAsync()
		{
			await LoadReferenceAsync(q => q.ChoicesCategory);
		}
	}
}
