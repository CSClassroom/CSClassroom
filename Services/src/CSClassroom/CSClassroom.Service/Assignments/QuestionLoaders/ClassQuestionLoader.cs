using System.Threading.Tasks;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Repository;

namespace CSC.CSClassroom.Service.Assignments.QuestionLoaders
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
