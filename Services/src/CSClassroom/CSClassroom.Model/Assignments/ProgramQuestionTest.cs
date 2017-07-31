using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;

namespace CSC.CSClassroom.Model.Questions
{
	/// <summary>
	/// A test for a program exercise.
	/// </summary>
	public class ProgramQuestionTest : CodeQuestionTest
	{
		/// <summary>
		/// The ID of the question.
		/// </summary>
		public int ProgramQuestionId { get; set; }

		/// <summary>
		/// The question.
		/// </summary>
		public virtual ProgramQuestion ProgramQuestion { get; set; }

		/// <summary>
		/// A description of what the test does.
		/// </summary>
		[Required]
		[Display(Name = "Description")]
		public string TestDescription { get; set; }

		/// <summary>
		/// The command line arguments, separated by spaces.
		/// </summary>
		[Display(Name = "Command Line Arguments")]
		public string CommandLineArguments { get; set; }

		/// <summary>
		/// Returns a list of arguments
		/// </summary>
		public string[] GetArguments()
		{
			return Regex.Matches(CommandLineArguments ?? string.Empty, @"[\""].+?[\""]|[^ ]+")
				.Cast<Match>()
				.Select(m => m.Value)
				.ToArray();
		}
	}
}
