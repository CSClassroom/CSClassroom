using CSC.CSClassroom.Model.Users;

namespace CSC.CSClassroom.Model.Assignments
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
		public Assignment Assignment { get; }

		/// <summary>
		/// The old score for the assignment.
		/// </summary>
		public double OldScore { get; }

		/// <summary>
		/// The new score for the assignment.
		/// </summary>
		public double NewScore { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public AssignmentGrade(
			User user, 
			Assignment assignment, 
			double oldScore, 
			double newScore)
		{
			User = user;
			Assignment = assignment;
			OldScore = oldScore;
			NewScore = newScore;
		}
	}
}
