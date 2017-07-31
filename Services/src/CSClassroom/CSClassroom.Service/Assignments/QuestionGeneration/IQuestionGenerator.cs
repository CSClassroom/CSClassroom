using System.Threading.Tasks;
using CSC.CSClassroom.Model.Questions;

namespace CSC.CSClassroom.Service.Questions.QuestionGeneration
{
	/// <summary>
	/// The result of attempting to generate a question.
	/// </summary>
	public interface IQuestionGenerator
	{
		/// <summary>
		/// Generates an invocation of the question constructor that
		/// populates the resulting question with all of the question's data.
		/// </summary>
		void GenerateConstructorInvocation(
			Question existingQuestion, 
			JavaFileBuilder fileBuilder);

		/// <summary>
		/// Generates a specific question given a generated question and a seed.
		/// </summary>
		Task<QuestionGenerationResult> GenerateQuestionAsync(
			GeneratedQuestionTemplate question, 
			int seed);
	}
}