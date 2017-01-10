using CSC.CSClassroom.Model.Users;

namespace CSC.CSClassroom.WebApp.ViewModels.Assignment
{
	/// <summary>
	/// A grade for an assignment.
	/// </summary>
	public class AssignmentGrade
	{
		/// <summary>
		/// The user that received the grade.
		/// </summary>
		public User User { get; }

		/// <summary>
		/// The graded assignment.
		/// </summary>
		public Model.Questions.Assignment Assignment { get; }

		/// <summary>
		/// The score for the assignment.
		/// </summary>
		public double Score { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public AssignmentGrade(
			User user, 
			Model.Questions.Assignment assignment, 
			double score)
		{
			User = user;
			Assignment = assignment;
			Score = score;
		}
	}
}
