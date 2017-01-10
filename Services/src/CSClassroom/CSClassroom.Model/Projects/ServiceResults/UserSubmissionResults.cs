using System.Collections.Generic;
using System.Linq;
using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Model.Users;

namespace CSC.CSClassroom.Model.Projects.ServiceResults
{
	/// <summary>
	/// Submission results for a given user.
	/// </summary>
	public class UserSubmissionResults
	{
		/// <summary>
		/// The user.
		/// </summary>
		public User User { get; }

		/// <summary>
		/// The submission results.
		/// </summary>
		public IList<UserSubmissionResult> SubmissionResults { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public UserSubmissionResults(
			User user,
			Section section,
			IList<Checkpoint> checkpoints,
			IList<Submission> submissions)
		{
			User = user;
			SubmissionResults = checkpoints
				.Where
				(
					c => c.SectionDates?.Any(sd => sd.Section == section) ?? false
				)
				.OrderBy
				(
					c => c.SectionDates.Single(sd => sd.Section == section).DueDate
				)
				.Select
				(
					checkpoint => new UserSubmissionResult
					(
						section,
						checkpoint,
						submissions
							.Where(s => s.Checkpoint == checkpoint)
							.OrderByDescending(s => s.DateSubmitted)
							.FirstOrDefault()
					)
				).ToList();
		}
	}
}
