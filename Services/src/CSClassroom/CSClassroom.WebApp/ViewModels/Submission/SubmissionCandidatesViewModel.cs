using System;
using System.Collections.Generic;
using System.Linq;
using CSC.CSClassroom.Model.Projects;
using CSC.CSClassroom.Model.Users;
using CSC.CSClassroom.WebApp.Providers;

namespace CSC.CSClassroom.WebApp.ViewModels.Submission
{
	/// <summary>
	/// A view model for a submission candidate.
	/// </summary>
	public class SubmissionCandidatesViewModel
	{
		/// <summary>
		/// The user.
		/// </summary>
		public User User { get; }

		/// <summary>
		/// The checkpoint.
		/// </summary>
		public Checkpoint Checkpoint { get; }

		/// <summary>
		/// The submission candidates.
		/// </summary>
		public IList<SubmissionCandidateViewModel> Candidates { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public SubmissionCandidatesViewModel(
			User user,
			IList<Commit> commits,
			Func<Commit, string> commitUrlBuilder,
			Checkpoint checkpoint,
			Model.Projects.Submission latestSubmission,
			ITimeZoneProvider timeZoneProvider)
		{
			User = user;
			Checkpoint = checkpoint;
			Candidates = commits
				.OrderByDescending(commit => commit.PushDate)
				.ThenByDescending(commit => commit.CommitDate)
				.Select
				(
					commit => new SubmissionCandidateViewModel
					(
						commit,
						commitUrlBuilder(commit),
						latestSubmission?.CommitId == commit.Id,
						commit == commits.First(),
						timeZoneProvider
					)
				).ToList();
		}
	}
}
