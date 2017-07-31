using CSC.CSClassroom.Model.Questions;
using CSC.CSClassroom.Repository;

namespace CSC.CSClassroom.Service.Questions.QuestionDuplicators
{
	/// <summary>
	/// Loads a generated question.
	/// </summary>
	public class GeneratedQuestionDuplicator : CodeQuestionDuplicator<GeneratedQuestionTemplate>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public GeneratedQuestionDuplicator(DatabaseContext dbContext, GeneratedQuestionTemplate question) 
			: base(dbContext, question)
		{
		}

		/// <summary>
		/// Populates navigation properties specific to the question type.
		/// </summary>
		protected override void PopulateCodeQuestionImpl(GeneratedQuestionTemplate duplicate)
		{
		}
	}
}
