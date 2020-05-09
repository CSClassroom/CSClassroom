using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.Email;
using CSC.Common.Infrastructure.System;
using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Model.Projects;
using CSC.CSClassroom.Model.Projects.ServiceResults;
using CSC.CSClassroom.Model.Users;
using CSC.CSClassroom.Repository;
using CSC.CSClassroom.Service.Projects.Submissions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CSC.CSClassroom.Service.Projects
{
	/// <summary>
	/// Performs submission operations.
	/// </summary>
	public class SubmissionService : ISubmissionService
	{
		/// <summary>
		/// The logger.
		/// </summary>
		private readonly ILogger _logger;

		/// <summary>
		/// The database context.
		/// </summary>
		private readonly DatabaseContext _dbContext;

		/// <summary>
		/// Creates student submissions.
		/// </summary>
		private readonly ISubmissionCreator _submissionCreator;

		/// <summary>
		/// Downloads student submissions.
		/// </summary>
		private readonly ISubmissionDownloader _submissionDownloader;

		/// <summary>
		/// Builds an archive with all student submissions.
		/// </summary>
		private readonly ISubmissionArchiveBuilder _submissionArchiveBuilder;

		/// <summary>
		/// The e-mail provider.
		/// </summary>
		private readonly ITimeProvider _timeProvider;

		/// <summary>
		/// The e-mail provider.
		/// </summary>
		private readonly IEmailProvider _emailProvider;

		/// <summary>
		/// Constructor.
		/// </summary>
		public SubmissionService(
			ILogger<SubmissionService> logger,
			DatabaseContext dbContext,
			ISubmissionCreator submissionCreator,
			ISubmissionDownloader submissionDownloader,
			ISubmissionArchiveBuilder submissionArchiveBuilder,
			ITimeProvider timeProvider,
			IEmailProvider emailProvider)
		{
			_logger = logger;
			_dbContext = dbContext;
			_submissionCreator = submissionCreator;
			_submissionDownloader = submissionDownloader;
			_submissionArchiveBuilder = submissionArchiveBuilder;
			_timeProvider = timeProvider;
			_emailProvider = emailProvider;
		}

		/// <summary>
		/// Returns submission candidates for the given user.
		/// </summary>
		public async Task<IList<Commit>> GetSubmissionCandidatesAsync(
			string classroomName,
			string projectName,
			int userId)
		{
			var project = await LoadProjectAsync(classroomName, projectName);

			var user = await _dbContext.Users
				.Where(u => u.Id == userId)
				.Include(u => u.ClassroomMemberships)
				.SingleAsync();

			var commits = await _dbContext.Commits
				.Where(commit => commit.ProjectId == project.Id)
				.Where(commit => commit.UserId == userId)
				.Include(commit => commit.Build)
				.Include(commit => commit.Build.TestResults)
				.Include(commit => commit.Submissions)
				.OrderByDescending(commit => commit.PushDate)
				.ThenByDescending(commit => commit.CommitDate)
				.ToListAsync();

			var validCommits = await _submissionCreator.GetSubmissionCandidatesAsync
			(
				project,
				user
			);

			return commits
				.Where(commit => validCommits.Contains(commit.Sha))
				.Where(commit => commit.Build != null)
				.ToList();
		}

		/// <summary>
		/// Returns the list of submissions a user has made for a given checkpoint
		/// </summary>
		public async Task<IList<Submission>> GetUserSubmissionsAsync(
			string classroomName,
			string projectName,
			int userId)
		{
			return await _dbContext.Submissions
				.Where(submission => submission.Commit.Project.Classroom.Name == classroomName)
				.Where(submission => submission.Commit.Project.Name == projectName)
				.Where(submission => submission.Commit.UserId == userId)
				.OrderByDescending(submission => submission.DateSubmitted)
				.Include(submission => submission.Commit)
				.Include(submission => submission.Checkpoint)
				.ToListAsync();
		}

		/// <summary>
		/// Returns the latest submission for each student in a given section,
		/// for a given checkpoint.
		/// </summary>
		public async Task<IList<SectionSubmissionResult>> GetSectionSubmissionsAsync(
			string classroomName,
			string projectName,
			string checkpointName,
			string sectionName)
		{
			var section = await LoadSectionAsync(classroomName, sectionName);
			var users = await GetStudentsAsync(section);
			var checkpoint = await LoadCheckpointAsync
			(
				classroomName,
				projectName,
				checkpointName
			);

			var submissions = await GetCheckpointSubmissionsQuery(checkpoint, section)
				.ToListAsync();

			var userSubmissions = submissions
				.GroupBy(s => s.Commit.User)
				.ToDictionary
				(
					group => group.Key,
					group => group
						.OrderByDescending(submission => submission.DateSubmitted)
						.First()
				);

			return users
				.Select
				(
					user => new SectionSubmissionResult
					(
						user,
						userSubmissions.ContainsKey(user)
							? userSubmissions[user]
							: null
					)
				)
				.OrderBy(result => result.CommitDate != null)
				.ThenBy(result => result.LastName)
				.ThenBy(result => result.FirstName)
				.ToList();
		}

		/// <summary>
		/// Submits a checkpoint.
		/// </summary>
		public async Task<Submission> SubmitCheckpointAsync(
			string classroomName,
			string projectName,
			string checkpointName,
			int userId,
			int commitId)
		{
			var checkpoint = await LoadCheckpointAsync
			(
				classroomName,
				projectName,
				checkpointName
			);

			var commit = await _dbContext.Commits
				.Where(c => c.Id == commitId)
				.Where(c => c.UserId == userId)
				.Include(c => c.Project.Classroom)
				.Include(c => c.User.ClassroomMemberships)
				.Include(c => c.Build.TestResults)
				.SingleOrDefaultAsync();

			var pullRequestNumber = await _submissionCreator.CreatePullRequestAsync
			(
				commit,
				checkpoint
			);

			var submission = new Submission()
			{
				CheckpointId = checkpoint.Id,
				Checkpoint = checkpoint,
				CommitId = commit.Id,
				Commit = commit,
				DateSubmitted = _timeProvider.UtcNow,
				PullRequestNumber = pullRequestNumber
			};

			_dbContext.Submissions.Add(submission);

			await _dbContext.SaveChangesAsync();

			return submission;
		}

		/// <summary>
		/// Downloads all submissions for a given checkpoint from the sections,
		/// in the form of a zip archive. The archive will also include
		/// the latest state of repositories for which there is no submission. 
		/// </summary>
		public async Task<Stream> DownloadSubmissionsAsync(
			string classroomName,
			string projectName,
			string checkpointName,
            IList<string> sectionNames,
            bool includeEclipseProjects,
	        bool includeFlatFiles)

        {
            var sections = await LoadSectionsAsync(classroomName, sectionNames);
			var checkpoint = await LoadCheckpointAsync
			(
				classroomName,
				projectName,
				checkpointName
			);

			// TODO: Study the query this generates.  Will it bring back unnecessary data?
			var students = await _dbContext.ClassroomMemberships
				.Where
				(
					cm => cm.SectionMemberships.Any
					(
						sm => sections.Any
						(
							sec => sec.Id == sm.SectionId
						)						
						&& sm.Role == SectionRole.Student
					)
				)
				.Include(cm => cm.User)
					.ThenInclude(user => user.ClassroomMemberships)
						.ThenInclude(cm => cm.SectionMemberships)
				.ToListAsync();

			var allCheckpointSubmissions =
				await GetCheckpointSubmissionsQuery(checkpoint, sections)
					.ToListAsync();

			var usersWithSubmissions = new HashSet<User>
			(
				allCheckpointSubmissions
					.GroupBy(submission => submission.Commit.User)
					.Select(group => group.Key)
					.ToList()
			);

			var studentDownloadRequests = students
				.Select
				(
					student => new StudentDownloadRequest
					(
						student,
						usersWithSubmissions.Contains(student.User)
					)
				).ToList();

			var templateContents = await _submissionDownloader.DownloadTemplateContentsAsync
			(
				checkpoint.Project
			);

			var submissionContents = await _submissionDownloader.DownloadSubmissionsAsync
			(
				checkpoint,
				studentDownloadRequests
			);

			return await _submissionArchiveBuilder.BuildSubmissionArchiveAsync
			(
				checkpoint.Project,
				templateContents,
				submissionContents,
				includeEclipseProjects,
				includeFlatFiles
			);
		}

		/// <summary>
		/// Returns submissions for grading.
		/// </summary>
		public async Task<IList<GradeSubmissionResult>> GradeSubmissionsAsync(
			string classroomName,
			string projectName,
			string checkpointName,
			string sectionName)
		{
			var section = await LoadSectionAsync(classroomName, sectionName);
			var checkpoint = await LoadCheckpointAsync
			(
				classroomName,
				projectName,
				checkpointName
			);

			var students = await GetStudentsAsync(section);
			var dueDate = checkpoint
				.SectionDates
				.Single(sd => sd.SectionId == section.Id)
				.DueDate;

			var submissions = await GetSubmissionsForGrading(checkpoint, section, dueDate);

			return students
				.Select
				(
					student => new
					{
						User = student,
						Submissions = GroupSubmissions(section, submissions, student)
					}
				)
				.Where
				(
					studentSubmissions => studentSubmissions
						.Submissions
						.Any(s => s.Key.Id == checkpoint.Id)
				)
				.Select
				(
					studentSubmissions => new GradeSubmissionResult
					(
						studentSubmissions.User,
						section,
						studentSubmissions.Submissions
							.Single(group => group.Key == checkpoint)
							.First(),
						studentSubmissions.Submissions
							.SelectMany
							(
								group => group.Where
								(
									s => (s == group.First() && s.Checkpoint != checkpoint)
									     || (s != group.First() && !string.IsNullOrEmpty(s.Feedback))
								)
							).ToList()
					)
				).ToList();
		}

		/// <summary>
		/// Updates submission feedback.
		/// </summary>
		public async Task SaveFeedbackAsync(
			string classroomName,
			string projectName,
			string checkpointName,
			int submissionId,
			string feedbackText)
		{
			var checkpoint = await LoadCheckpointAsync
			(
				classroomName,
				projectName,
				checkpointName
			);

			var submission = await _dbContext.Submissions
				.Where(s => s.CheckpointId == checkpoint.Id)
				.FirstAsync(s => s.Id == submissionId);

			submission.Feedback = feedbackText;
			submission.DateFeedbackSaved = _timeProvider.UtcNow;

			await _dbContext.SaveChangesAsync();
		}

		/// <summary>
		/// Sends all filled-out feedback for the given checkpoint in the given section.
		/// </summary>
		public async Task SendFeedbackAsync(
			string classroomName,
			string projectName,
			string checkpointName,
			string sectionName,
			Func<Submission, string> viewFeedbackUrlBuilder)
		{
			var section = await LoadSectionAsync(classroomName, sectionName);
			var checkpoint = await LoadCheckpointAsync
			(
				classroomName,
				projectName,
				checkpointName
			);

			var submissions = await _dbContext.Submissions
				.Where
				(
					submission =>
						submission.CheckpointId == checkpoint.Id &&
						submission.Commit.User.ClassroomMemberships.Any
						(
							cm => cm.SectionMemberships.Any
							(
								sm => sm.SectionId == section.Id
								      && sm.Role == SectionRole.Student
							)
						)
				)
				.Include(s => s.Commit.User)
				.Include(s => s.Checkpoint.Project)
				.ToListAsync();

			var submissionsWithFeedback = submissions
				.GroupBy(submission => submission.Commit.User)
				.Select
				(
					group => group
						.OrderByDescending(g => g.DateSubmitted)
						.First()
				)
				.Where
				(
					s => !string.IsNullOrWhiteSpace(s.Feedback) && !s.FeedbackSent
				)
				.ToList();

			var tasks = submissionsWithFeedback.Select
			(
				swf => SendSubmissionFeedbackAsync(swf, viewFeedbackUrlBuilder)
			).ToList();

			await Task.WhenAll(tasks);

			bool updated = false;
			foreach (var task in tasks)
			{
				if (task.Result.Item2)
				{
					task.Result.Item1.FeedbackSent = true;
					updated = true;
				}
			}

			if (updated)
			{
				await _dbContext.SaveChangesAsync();
			}
		}

		/// <summary>
		/// Returns feedback for the given submission.
		/// </summary>
		public async Task<ViewFeedbackResult> GetSubmissionFeedbackAsync(
			string classroomName,
			string projectName,
			string checkpointName,
			int submissionId)
		{
			var checkpoint = await LoadCheckpointAsync
			(
				classroomName,
				projectName,
				checkpointName
			);

			var submission = await _dbContext.Submissions
				.Where(s => s.Id == submissionId)
				.Include(s => s.Commit.User.ClassroomMemberships)
				.FirstOrDefaultAsync();

			if (submission == null
			    || submission.CheckpointId != checkpoint.Id
			    || !submission.FeedbackSent)
			{
				return null;
			}

			var sectionMembership = await _dbContext.SectionMemberships
				.Where
				(
					sm => sm.ClassroomMembership.ClassroomId == checkpoint.Project.ClassroomId
					      && sm.ClassroomMembership.UserId == submission.Commit.UserId
				)
				.Include(sm => sm.Section)
				.FirstOrDefaultAsync();

			var checkpoints = await _dbContext.Checkpoints
				.Where(c => c.Project.Classroom.Name == classroomName)
				.Where(c => c.Project.Name == projectName)
				.Include(c => c.SectionDates)
				.ToListAsync();

			return new ViewFeedbackResult
			(
				sectionMembership?.Section,
				submission,
				checkpoints
			);
		}

		/// <summary>
		/// Marks feedback as read for the given submission.
		/// </summary>
		public async Task MarkFeedbackReadAsync(
			string classroomName,
			string projectName,
			string checkpointName,
			int submissionId,
			int userId)
		{
			var checkpoint = await LoadCheckpointAsync
			(
				classroomName,
				projectName,
				checkpointName
			);

			var submission = await _dbContext.Submissions
				.Where(s => s.CheckpointId == checkpoint.Id)
				.Where(s => s.Commit.UserId == userId)
				.Where(s => s.FeedbackSent)
				.Include(s => s.Commit)
				.SingleAsync(s => s.Id == submissionId);

			if (submission.DateFeedbackRead == null)
			{
				submission.DateFeedbackRead = _timeProvider.UtcNow;

				await _dbContext.SaveChangesAsync();
			}
		}

		/// <summary>
		/// Returns all unread feedback for the given user.
		/// </summary>
		public async Task<IList<UnreadFeedbackResult>> GetUnreadFeedbackAsync(
			string classroomName,
			int userId)
		{
			return await _dbContext.Submissions
				.Where(s => s.Commit.Project.Classroom.Name == classroomName)
				.Where(s => s.Commit.UserId == userId)
				.Where(s => s.FeedbackSent && s.DateFeedbackRead == null)
				.OrderBy(s => s.DateFeedbackSaved)
				.Select
				(
					s => new UnreadFeedbackResult
					(
						s.Commit.Project.Name,
						s.Checkpoint.Name,
						s.Checkpoint.DisplayName,
						s.Id,
						s.Commit.PushDate
					)
				).ToListAsync();
		}

		/// <summary>
		/// Returns all students in a given section.
		/// </summary>
		private async Task<List<User>> GetStudentsAsync(Section section)
		{
			return await _dbContext.Users
				.Where
				(
					user => user.ClassroomMemberships.Any
					(
						cm => cm.SectionMemberships.Any
						(
							sm => sm.SectionId == section.Id
							      && sm.Role == SectionRole.Student
						)
					)
				)
				.OrderBy(user => user.LastName)
				.ThenBy(user => user.FirstName)
				.ToListAsync();
		}

		/// <summary>
		/// Returns all submissions for the current checkpoint, along with
		/// submissions for past checkpoints.
		/// </summary>
		private async Task<List<Submission>> GetSubmissionsForGrading(
			Checkpoint checkpoint,
			Section section,
			DateTime dueDate)
		{
			var submissions = await _dbContext.Submissions
				.Where
				(
					submission =>
						submission.Checkpoint.ProjectId == checkpoint.ProjectId &&
						submission.Commit.User.ClassroomMemberships.Any
						(
							cm => cm.SectionMemberships.Any
							(
								sm => sm.SectionId == section.Id
								      && sm.Role == SectionRole.Student
							)
						)
				)
				.Include(submission => submission.Commit.Build.TestResults)
				.Include(submission => submission.Commit.User.ClassroomMemberships)
				.Include(submission => submission.Checkpoint.SectionDates)
				.Include(submission => submission.Checkpoint.TestClasses)
				.ToListAsync();

			return submissions
				.Where
				(
					s => s.Checkpoint
						.SectionDates
						.First(sd => sd.SectionId == section.Id)
						.DueDate <= dueDate
				).ToList();
		}

		/// <summary>
		/// Returns all checkpoints for a given user, grouped by checkpoint.
		/// </summary>
		private static List<IGrouping<Checkpoint, Submission>> GroupSubmissions(
			Section section,
			List<Submission> submissions,
			User student)
		{
			return submissions
				.Where(s => s.Commit.UserId == student.Id)
				.OrderByDescending(s => s.DateSubmitted)
				.GroupBy(s => s.Checkpoint)
				.OrderByDescending
				(
					group => group.Key
						.SectionDates
						.Single(sd => sd.SectionId == section.Id)
						.DueDate
				).ToList();
		}

		/// <summary>
		/// Sends feedback for a particular submission.
		/// </summary>
		private async Task<Tuple<Submission, bool>> SendSubmissionFeedbackAsync(
			Submission submission,
			Func<Submission, string> markReadUrlBuilder)
		{
			var user = submission.Commit.User;
			try
			{
				await _emailProvider.SendMessageAsync
				(
					new List<EmailRecipient>() {new EmailRecipient($"{user.FirstName} {user.LastName}", user.EmailAddress)},
					$"{submission.Checkpoint.Project.Name} {submission.Checkpoint.DisplayName} Feedback",
					$"{submission.Feedback.Replace("\n", "<br>")}<br><br><a href=\"{markReadUrlBuilder(submission)}\">Click here</a> to mark this feedback as read."
				);

				return new Tuple<Submission, bool>(submission, true);
			}
			catch (Exception ex)
			{
				_logger.LogError
				(
					0,
					ex,
					"Failed to send e-mail to {emailAddress}",
					submission.Commit.User.EmailAddress
				);

				return new Tuple<Submission, bool>(submission, false);
			}
		}

		/// <summary>
		/// Returns a query for all submissions in a given checkpoint
		/// for a single section.
		/// </summary>
		private IQueryable<Submission> GetCheckpointSubmissionsQuery(
			Checkpoint checkpoint,
			Section section)
		{
			return _dbContext.Submissions
				.Where
				(
					submission =>
						submission.CheckpointId == checkpoint.Id &&
						submission.Commit.User.ClassroomMemberships.Any
						(
							cm => cm.SectionMemberships.Any
							(
								sm => sm.SectionId == section.Id
								      && sm.Role == SectionRole.Student
							)
						)
				)
				.Include(submission => submission.Commit.User.ClassroomMemberships)
				.Include(submission => submission.Commit.Build);
		}

		/// <summary>
		/// Returns a query for all submissions in a given checkpoint
		/// for potentially multiple sections.
		/// </summary>
		private IQueryable<Submission> GetCheckpointSubmissionsQuery(
			Checkpoint checkpoint,
			List<Section> sections)
		{
			return _dbContext.Submissions
				.Where
				(
					submission =>
						submission.CheckpointId == checkpoint.Id &&
						submission.Commit.User.ClassroomMemberships.Any
						(
							cm => cm.SectionMemberships.Any
							(
								sm => sections.Any
								(
									sec => sm.SectionId == sec.Id
								)
								&& sm.Role == SectionRole.Student
							)
						)
				)
				.Include(submission => submission.Commit.User.ClassroomMemberships)
				.Include(submission => submission.Commit.Build);
		}

		/// <summary>
		/// Loads a project from the database.
		/// </summary>
		private async Task<Project> LoadProjectAsync(
			string classroomName,
			string projectName)
		{
			return await _dbContext.Projects
				.Where(p => p.Classroom.Name == classroomName)
				.Include(p => p.Classroom)
				.SingleAsync(p => p.Name == projectName);
		}

		/// <summary>
		/// Loads a checkpoint from the database.
		/// </summary>
		private async Task<Checkpoint> LoadCheckpointAsync(
			string classroomName,
			string projectName,
			string checkpointName)
		{
			return await _dbContext.Checkpoints
				.Where(c => c.Project.Classroom.Name == classroomName)
				.Where(c => c.Project.Name == projectName)
				.Include(c => c.Project)
				.Include(c => c.Project.Classroom)
				.Include(c => c.Project.TestClasses)
				.Include(c => c.SectionDates)
				.Include(c => c.TestClasses)
				.SingleAsync(c => c.Name == checkpointName);
		}

		/// <summary>
		/// Loads a single section from the database.
		/// </summary>
		private async Task<Section> LoadSectionAsync(
			string classroomName,
			string sectionName)
		{
			return await _dbContext.Sections
				.Where(s => s.Classroom.Name == classroomName)
				.Include(s => s.Classroom)
				.SingleAsync(s => s.Name == sectionName);
		}

		/// <summary>
		/// Loads multiple sections from the database.
		/// </summary>
		private async Task<List<Section>> LoadSectionsAsync(
			string classroomName,
			IList<string> sectionNames)
		{
			return await _dbContext.Sections
				.Where(s => (s.Classroom.Name == classroomName && sectionNames.Contains(s.Name)))
				.Include(s => s.Classroom)
				.ToListAsync();
		}
	}
}
