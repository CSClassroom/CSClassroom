using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.Email;
using CSC.Common.Infrastructure.Projects.Submissions;
using CSC.Common.Infrastructure.System;
using CSC.CSClassroom.Model.Projects;
using CSC.CSClassroom.Model.Users;
using CSC.CSClassroom.Repository;
using CSC.CSClassroom.Service.Projects;
using CSC.CSClassroom.Service.Projects.Submissions;
using CSC.CSClassroom.Service.UnitTests.TestDoubles;
using CSC.CSClassroom.Service.UnitTests.Utilities;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Projects
{
	/// <summary>
	/// Unit tests for the submission service.
	/// </summary>
	public class SubmissionService_UnitTests
	{
		/// <summary>
		/// Ensures that GetSubmissionCandidatesAsync returns only valid
		/// candidates for submission. Commits with pending builds will be
		/// excluded, as will commits that have been overwritten.
		/// </summary>
		[Fact]
		public async Task GetSubmissionCandidatesAsync_ReturnsOnlyValidCandidates()
		{
			var database = GetDatabaseBuilder().Build();

			var userId = database.Context.Users
				.Single(u => u.UserName == "Student1")
				.Id;

			var validCommits = Collections.CreateList("Commit2", "Commit3", "Commit4");
			var submissionCreator = GetMockSubmissionCreator(validCommits);

			var submissionService = GetSubmissionService
			(
				database.Context,
				submissionCreator: submissionCreator.Object
			);

			var results = await submissionService.GetSubmissionCandidatesAsync
			(
				"Class1",
				"Project1",
				userId
			);

			Assert.Equal(2, results.Count);

			Assert.Equal("Class1", results[0].Project.Classroom.Name);
			Assert.Equal("Project1", results[0].Project.Name);
			Assert.Equal("Commit3", results[0].Sha);
			Assert.Equal(2, results[0].Build.TestResults.Count);
			Assert.Equal(1, results[0].Submissions.Count);

			Assert.Equal("Class1", results[1].Project.Classroom.Name);
			Assert.Equal("Project1", results[1].Project.Name);
			Assert.Equal("Commit2", results[1].Sha);
			Assert.Equal(0, results[1].Build.TestResults.Count);
			Assert.Equal(0, results[1].Submissions.Count);
		}

		/// <summary>
		/// Ensures that GetUserSubmissionsAsync returns all user submissions
		/// for the given project.
		/// </summary>
		[Fact]
		public async Task GetUserSubmissionsAsync_ReturnsAllUserSubmissions()
		{
			var database = GetDatabaseBuilder().Build();

			var userId = database.Context.Users
				.Single(u => u.UserName == "Student2")
				.Id;

			var submissionService = GetSubmissionService(database.Context);
			var results = await submissionService.GetUserSubmissionsAsync
			(
				"Class1",
				"Project1",
				userId
			);

			Assert.Equal(3, results.Count);
			Assert.Equal("Checkpoint2", results[0].Checkpoint.Name);
			Assert.Equal("Commit4", results[0].Commit.Sha);
			Assert.Equal("Checkpoint1", results[1].Checkpoint.Name);
			Assert.Equal("Commit3", results[1].Commit.Sha);
			Assert.Equal("Checkpoint1", results[2].Checkpoint.Name);
			Assert.Equal("Commit1", results[2].Commit.Sha);
		}

		/// <summary>
		/// Ensures that GetSectionSubmissionsAsync returns each student's latest 
		/// submission for the given checkpoint.
		/// </summary>
		[Fact]
		public async Task GetSectionSubmissionsAsync_ReturnsSubmissions()
		{
			var database = GetDatabaseBuilder().Build();

			var users = database.Context.Users
				.OrderBy(u => u.UserName)
				.ToList();

			database.Reload();

			var submissionService = GetSubmissionService(database.Context);
			var results = await submissionService.GetSectionSubmissionsAsync
			(
				"Class1",
				"Project1",
				"Checkpoint2",
				"Period1"
			);

			Assert.Equal(2, results.Count);

			Assert.Equal("Last1", results[0].LastName);
			Assert.Equal("First1", results[0].FirstName);
			Assert.Equal(users[0].Id, results[0].UserId);
			Assert.Null(results[0].Commit);
			Assert.Null(results[0].CommitDate);
			Assert.Null(results[0].SubmissionDate);
			Assert.Null(results[0].PullRequestNumber);

			Assert.Equal("Last2", results[1].LastName);
			Assert.Equal("First2", results[1].FirstName);
			Assert.Equal(users[1].Id, results[1].UserId);
			Assert.Equal("Commit4", results[1].Commit.Sha);
			Assert.Equal(CommitDates[3], results[1].CommitDate);
			Assert.Equal(SubmissionDates[3], results[1].SubmissionDate);
			Assert.Equal(3, results[1].PullRequestNumber);
			Assert.NotNull(results[1].Commit.Build);
		}

		/// <summary>
		/// Ensures that SubmitCheckpointAsync actually submits the checkpoint.
		/// </summary>
		[Fact]
		public async Task SubmitCheckpointAsync_CheckpointSubmitted()
		{
			var database = GetDatabaseBuilder().Build();

			var userId = database.Context.Users
				.Single(u => u.UserName == "Student1")
				.Id;

			var commitId = database.Context.Commits
				.Where(c => c.UserId == userId)
				.Single(c => c.Sha == "Commit3")
				.Id;

			database.Reload();

			var timeProvider = GetMockTimeProvider(CommitDates[2]);
			var submissionCreator = GetMockSubmissionCreator(pullRequestId: 3);

			var submissionService = GetSubmissionService
			(
				database.Context,
				timeProvider: timeProvider.Object,
				submissionCreator: submissionCreator.Object
			);

			var result = await submissionService.SubmitCheckpointAsync
			(
				"Class1",
				"Project1",
				"Checkpoint2",
				userId,
				commitId
			);

			database.Reload();

			var submission = database.Context.Submissions
				.Where(s => s.Commit.UserId == userId)
				.Include(s => s.Commit)
				.Single(s => s.Checkpoint.Name == "Checkpoint2");

			Assert.Equal("Commit3", submission.Commit.Sha);
			Assert.Equal(3, submission.PullRequestNumber);
			Assert.Equal(CommitDates[2], submission.DateSubmitted);
		}

		/// <summary>
		/// Ensures GetCheckpointDownloadCandidateListAsync returns the correct list of candidates 
		/// </summary>
		[Fact]
		public async Task GetCheckpointDownloadCandidateListAsync_ReturnsCandidates()
		{
			var database = GetDatabaseBuilder().Build();

			var submissionService = GetSubmissionService(database.Context);

			var candidates = await submissionService.GetCheckpointDownloadCandidateListAsync
			(
				"Class1",
				"Project1",
				"Checkpoint2"
			);

			// To insulate this test from data used for other tests, focus on
			// Period2 and Period3
			var candidateUsersP2 = candidates.Single
			(
				c => c.Section.Name == "Period2"
			).Users;
			Assert.Equal(3, candidateUsersP2.Count);
			Assert.Equal("Student3", candidateUsersP2[0].User.UserName);
			Assert.False(candidateUsersP2[0].Submitted);
			Assert.Equal("Student4", candidateUsersP2[1].User.UserName);
			Assert.True(candidateUsersP2[1].Submitted);
			Assert.Equal("Student5", candidateUsersP2[2].User.UserName);
			Assert.True(candidateUsersP2[2].Submitted);
			var candidateUsersP3 = candidates.Single
			(
				c => c.Section.Name == "Period3"
			).Users;
			Assert.Equal(3, candidateUsersP3.Count);
			Assert.Equal("Student6", candidateUsersP3[0].User.UserName);
			Assert.False(candidateUsersP3[0].Submitted);
			Assert.Equal("Student7", candidateUsersP3[1].User.UserName);
			Assert.True(candidateUsersP3[1].Submitted);
			Assert.Equal("Student8", candidateUsersP3[2].User.UserName);
			Assert.False(candidateUsersP3[2].Submitted);
		}

		/// <summary>
		/// Ensures that DownloadSubmissionsAsync downloads the submissions
		/// for a given checkpoint.
		/// </summary>
		[Fact]
		public async Task DownloadSubmissionsAsync_DownloadsSubmissions()
		{
			var database = GetDatabaseBuilder().Build();

			var templateContents = new Mock<IArchive>().Object;
			var studentSubmissions = new StudentSubmissions(new List<StudentSubmission>());
			var expectedResult = new MemoryStream();

			var submissionDownloader = GetMockSubmissionDownloader
			(
				templateContents, 
				studentSubmissions
			);

			// Downloading these students allows testing the following combinations:
			//		Period 2: Student3 submission: NO, Student4 submission: YES
			//				  EXCLUDE this student from downloading: Student5 submission: YES
			//		Period 3: Student6 submission: NO, Student7 submission: YES
			//				  EXCLUDE this student from downloading: Student8 submission: NO
			//
			string[] studentNamesForDownloadingFromPeriod2 = new string[] { "Student3", "Student4" };
			string[] studentNamesForDownloadingFromPeriod3 = new string[] { "Student6", "Student7" };

			var dlRequests = database.Context.Sections.Where
			(
				sec => sec.Classroom.Name == "Class1" &&
					   (sec.Name == "Period2" || sec.Name == "Period3")
			).SelectMany
			(
				sec => database.Context.Users
					.Where
					(
						user =>
							(
								   sec.Name == "Period2" 
								&& studentNamesForDownloadingFromPeriod2.Contains(user.UserName)
							) 
						|| (
								   sec.Name == "Period3"
								&& studentNamesForDownloadingFromPeriod3.Contains(user.UserName)
							)
					).Select
					(
						user => user.Id
					)
			).ToList();

			// Call DownloadSubmissionsAsync with each valid format
			foreach (ProjectSubmissionDownloadFormat format in Enum.GetValues(typeof(ProjectSubmissionDownloadFormat)))
			{
				var submissionArchiveBuilder = GetMockSubmissionArchiveBuilder
				(
					templateContents,
					studentSubmissions,
					expectedResult,
					format
				);

				var submissionService = GetSubmissionService
				(
					database.Context,
					submissionDownloader: submissionDownloader.Object,
					submissionArchiveBuilder: submissionArchiveBuilder.Object
				);

				var result = await submissionService.DownloadSubmissionsAsync
				(
					"Class1",
					"Project1",
					"Checkpoint2",
					dlRequests,
					format
				);

				Assert.Equal(result, expectedResult);
			}
		}

		/// <summary>
		/// Ensures that GradeSubmissionsAsync returns submissions to grade
		/// when all students have submitted the checkpoint.
		/// </summary>
		[Fact]
		public async Task GradeSubmissionsAsync_AllSubmitted_ReturnsSubmissions()
		{
			var database = GetDatabaseBuilder().Build();

			var submissionService = GetSubmissionService(database.Context);

			var results = await submissionService.GradeSubmissionsAsync
			(
				"Class1",
				"Project1",
				"Checkpoint1",
				"Period1"
			);

			Assert.Equal(2, results.Count);

			Assert.Equal("Last1", results[0].LastName);
			Assert.Equal("First1", results[0].FirstName);
			Assert.NotNull(results[0].Build.TestResults);
			Assert.Equal(CommitDates[2], results[0].CommitDate);
			Assert.Equal(SubmissionDates[2], results[0].SubmissionDate);
			Assert.True(results[0].RequiredTestsPassed);
			Assert.Equal(2, results[0].CommitDaysLate);
			Assert.Equal(6, results[0].SubmissionDaysLate);
			Assert.Null(results[0].Feedback);
			Assert.False(results[0].FeedbackSent);
			Assert.Equal(1, results[0].PastSubmissions.Count);
			Assert.Equal("Checkpoint1 DisplayName", results[0].PastSubmissions[0].CheckpointDisplayName);
			Assert.Equal(CommitDates[0], results[0].PastSubmissions[0].CommitDate);
			Assert.Equal(SubmissionDates[0], results[0].PastSubmissions[0].SubmitDate);
			Assert.Equal(0, results[0].PastSubmissions[0].CommitDaysLate);
			Assert.Equal(4, results[0].PastSubmissions[0].SubmitDaysLate);
			Assert.True(results[0].PastSubmissions[0].RequiredTestsPassed);
			Assert.Equal("Feedback1", results[0].PastSubmissions[0].Feedback);
			Assert.NotNull(results[0].PastSubmissions[0].Build.TestResults);

			Assert.Equal("Last2", results[1].LastName);
			Assert.Equal("First2", results[1].FirstName);
			Assert.NotNull(results[1].Build.TestResults);
			Assert.Equal(CommitDates[2], results[1].CommitDate);
			Assert.Equal(SubmissionDates[2], results[1].SubmissionDate);
			Assert.True(results[1].RequiredTestsPassed);
			Assert.Equal(2, results[1].CommitDaysLate);
			Assert.Equal(6, results[1].SubmissionDaysLate);
			Assert.Equal("Feedback3", results[1].Feedback);
			Assert.True(results[1].FeedbackSent);
			Assert.Equal(1, results[1].PastSubmissions.Count);
			Assert.Equal("Checkpoint1 DisplayName", results[1].PastSubmissions[0].CheckpointDisplayName);
			Assert.Equal(CommitDates[0], results[1].PastSubmissions[0].CommitDate);
			Assert.Equal(SubmissionDates[0], results[1].PastSubmissions[0].SubmitDate);
			Assert.Equal(0, results[1].PastSubmissions[0].CommitDaysLate);
			Assert.Equal(4, results[1].PastSubmissions[0].SubmitDaysLate);
			Assert.True(results[1].PastSubmissions[0].RequiredTestsPassed);
			Assert.Equal("Feedback1", results[1].PastSubmissions[0].Feedback);
			Assert.NotNull(results[1].PastSubmissions[0].Build.TestResults);
		}

		/// <summary>
		/// Ensures that GradeSubmissionsAsync returns submissions to grade
		/// when not all students have submitted the checkpoint.
		/// </summary>
		[Fact]
		public async Task GradeSubmissionsAsync_NotAllSubmitted_ReturnsSubmissions()
		{
			var database = GetDatabaseBuilder().Build();

			var submissionService = GetSubmissionService(database.Context);

			var results = await submissionService.GradeSubmissionsAsync
			(
				"Class1",
				"Project1",
				"Checkpoint2",
				"Period1"
			);

			Assert.Equal(1, results.Count);

			Assert.Equal("Last2", results[0].LastName);
			Assert.Equal("First2", results[0].FirstName);
			Assert.NotNull(results[0].Build.TestResults);
			Assert.Equal(CommitDates[3], results[0].CommitDate);
			Assert.Equal(SubmissionDates[3], results[0].SubmissionDate);
			Assert.False(results[0].RequiredTestsPassed);
			Assert.Equal(0, results[0].CommitDaysLate);
			Assert.Equal(4, results[0].SubmissionDaysLate);
			Assert.Equal("Feedback4", results[0].Feedback);
			Assert.False(results[0].FeedbackSent);

			Assert.Equal(2, results[0].PastSubmissions.Count);

			Assert.Equal("Checkpoint1 DisplayName", results[0].PastSubmissions[0].CheckpointDisplayName);
			Assert.Equal(CommitDates[2], results[0].PastSubmissions[0].CommitDate);
			Assert.Equal(SubmissionDates[2], results[0].PastSubmissions[0].SubmitDate);
			Assert.Equal(2, results[0].PastSubmissions[0].CommitDaysLate);
			Assert.Equal(6, results[0].PastSubmissions[0].SubmitDaysLate);
			Assert.True(results[0].PastSubmissions[0].RequiredTestsPassed);
			Assert.Equal("Feedback3", results[0].PastSubmissions[0].Feedback);
			Assert.NotNull(results[0].PastSubmissions[0].Build.TestResults);

			Assert.Equal("Checkpoint1 DisplayName", results[0].PastSubmissions[1].CheckpointDisplayName);
			Assert.Equal(CommitDates[0], results[0].PastSubmissions[1].CommitDate);
			Assert.Equal(SubmissionDates[0], results[0].PastSubmissions[1].SubmitDate);
			Assert.Equal(0, results[0].PastSubmissions[1].CommitDaysLate);
			Assert.Equal(4, results[0].PastSubmissions[1].SubmitDaysLate);
			Assert.True(results[0].PastSubmissions[1].RequiredTestsPassed);
			Assert.Equal("Feedback1", results[0].PastSubmissions[1].Feedback);
			Assert.NotNull(results[0].PastSubmissions[1].Build.TestResults);
		}

		/// <summary>
		/// Ensures that SaveFeedbackAsync saves the new feedback
		/// for a given submission.
		/// </summary>
		[Fact]
		public async Task SaveFeedbackAsync_SavesFeedback()
		{
			var database = GetDatabaseBuilder().Build();

			var submissionId = database.Context.Submissions
				.Where(s => s.Commit.User.UserName == "Student1")
				.Where(s => s.Commit.Sha == "Commit3")
				.Single(s => s.Checkpoint.Name == "Checkpoint1")
				.Id;

			database.Reload();

			var timeProvider = GetMockTimeProvider(CommitDates[2]);

			var submissionService = GetSubmissionService
			(
				database.Context,
				timeProvider: timeProvider.Object
			);

			await submissionService.SaveFeedbackAsync
			(
				"Class1",
				"Project1",
				"Checkpoint1",
				submissionId,
				"Feedback3"
			);

			database.Reload();

			var submission = database.Context.Submissions
				.Where(s => s.Commit.User.UserName == "Student1")
				.Where(s => s.Commit.Sha == "Commit3")
				.Single(s => s.Checkpoint.Name == "Checkpoint1");

			Assert.Equal("Feedback3", submission.Feedback);
			Assert.Equal(CommitDates[2], submission.DateFeedbackSaved);
		}

		/// <summary>
		/// Ensures that SendFeedbackAsync sends all unsent feedback for
		/// a given submission.
		/// </summary>
		[Fact]
		public async Task SendFeedbackAsync_FeedbackToSend_FeedbackSent()
		{
			var database = GetDatabaseBuilder().Build();

			var emailProvider = GetMockEmailProvider(feedbackSent: true);

			var submissionService = GetSubmissionService
			(
				database.Context,
				emailProvider: emailProvider.Object
			);

			await submissionService.SendFeedbackAsync
			(
				"Class1",
				"Project1",
				"Checkpoint2",
				"Period1",
				submission => "UrlMarkAsRead"
			);

			emailProvider.Verify(GetSendMessageExpression(feedbackSent: true), Times.Once);
		}

		/// <summary>
		/// Ensures that SendFeedbackAsync does not send any feedback when there
		/// is no feedback to send.
		/// </summary>
		[Fact]
		public async Task SendFeedbackAsync_NoFeedbackToSend_NoFeedbackSent()
		{
			var database = GetDatabaseBuilder().Build();

			var emailProvider = GetMockEmailProvider(feedbackSent: false);

			var submissionService = GetSubmissionService
			(
				database.Context,
				emailProvider: emailProvider.Object
			);

			await submissionService.SendFeedbackAsync
			(
				"Class1",
				"Project1",
				"Checkpoint1",
				"Period1",
				submission => "UrlMarkAsRead"
			);

			emailProvider.Verify(GetSendMessageExpression(feedbackSent: false), Times.Never);
		}

		/// <summary>
		/// Ensures that SendFeedbackAsync marks sent feedback as sent.
		/// </summary>
		[Fact]
		public async Task SendFeedbackAsync_FeedbackToSend_FeedbackMarkedAsSent()
		{
			var database = GetDatabaseBuilder().Build();

			var emailProvider = GetMockEmailProvider(feedbackSent: true);

			var submissionService = GetSubmissionService
			(
				database.Context,
				emailProvider: emailProvider.Object
			);

			await submissionService.SendFeedbackAsync
			(
				"Class1",
				"Project1",
				"Checkpoint2",
				"Period1",
				submissionWithFeedback => "UrlMarkAsRead"
			);

			database.Reload();
			
			var submission = database.Context.Submissions
				.Where(s => s.Commit.User.UserName == "Student2")
				.Where(s => s.Commit.Sha == "Commit4")
				.Single(s => s.Checkpoint.Name == "Checkpoint2");

			Assert.True(submission.FeedbackSent);
		}

		/// <summary>
		/// Ensures that SendFeedbackAsync does not mark unsent feedback as sent.
		/// </summary>
		[Fact]
		public async Task SendFeedbackAsync_NoFeedbackToSend_FeedbackNotMarkedAsSent()
		{
			var database = GetDatabaseBuilder().Build();

			var emailProvider = GetMockEmailProvider(feedbackSent: true);

			var submissionService = GetSubmissionService
			(
				database.Context,
				emailProvider: emailProvider.Object
			);

			await submissionService.SendFeedbackAsync
			(
				"Class1",
				"Project1",
				"Checkpoint1",
				"Period1",
				submissionWithFeedback => "UrlMarkAsRead"
			);

			database.Reload();

			var submission = database.Context.Submissions
				.Where(s => s.Commit.User.UserName == "Student1")
				.Where(s => s.Commit.Sha == "Commit3")
				.Single(s => s.Checkpoint.Name == "Checkpoint1");

			Assert.False(submission.FeedbackSent);
		}

		/// <summary>
		/// Ensures that GetSubmissionFeedbackAsync returns null if feedback
		/// has not been sent.
		/// </summary>
		[Fact]
		public async Task GetSubmissionFeedbackAsync_FeedbackNotSent_ReturnsNull()
		{
			var database = GetDatabaseBuilder().Build();
			
			var submissionId = database.Context.Submissions
				.Where(s => s.Commit.User.UserName == "Student2")
				.Where(s => s.Commit.Sha == "Commit4")
				.Single(s => s.Checkpoint.Name == "Checkpoint2")
				.Id;

			database.Reload();

			var submissionService = GetSubmissionService(database.Context);

			var result = await submissionService.GetSubmissionFeedbackAsync
			(
				"Class1",
				"Project1",
				"Checkpoint2",
				submissionId
			);

			Assert.Null(result);
		}

		/// <summary>
		/// Ensures that GetSubmissionFeedbackAsync returns feedback if it has
		/// been sent.
		/// </summary>
		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task GetSubmissionFeedbackAsync_FeedbackSentFutureCheckpoints_ReturnsResult(
			bool unread)
		{
			var database = GetDatabaseBuilder().Build();

			var submission = database.Context.Submissions
				.Where(s => s.Commit.User.UserName == "Student2")
				.Where(s => s.Commit.Sha == "Commit3")
				.Include(s => s.Commit)
				.Single(s => s.Checkpoint.Name == "Checkpoint1");

			submission.FeedbackSent = true;
			submission.DateFeedbackRead = unread ? null : (DateTime?)DateTime.MaxValue;
			database.Context.SaveChanges();
			database.Reload();

			var submissionService = GetSubmissionService(database.Context);

			var result = await submissionService.GetSubmissionFeedbackAsync
			(
				"Class1",
				"Project1",
				"Checkpoint1",
				submission.Id
			);

			Assert.Equal("Last2", result.LastName);
			Assert.Equal("First2", result.FirstName);
			Assert.Equal(submission.Commit.UserId, result.UserId);
			Assert.Equal("Project1_Last2First2", result.RepoName);
			Assert.Equal(CommitDates[2], result.PushDate);
			Assert.Equal("Feedback3", result.Feedback);
			Assert.Equal(2, result.PullRequestNumber);
			Assert.Equal(unread, result.Unread);
			Assert.True(result.FutureCheckpoints);
		}

		/// <summary>
		/// Ensures that GetSubmissionFeedbackAsync returns feedback if it has
		/// been sent.
		/// </summary>
		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task GetSubmissionFeedbackAsync_FeedbackSentNoFutureCheckpoints_ReturnsResult(
			bool unread)
		{
			var database = GetDatabaseBuilder().Build();

			var submission = database.Context.Submissions
				.Where(s => s.Commit.User.UserName == "Student2")
				.Where(s => s.Commit.Sha == "Commit4")
				.Include(s => s.Commit)
				.Single(s => s.Checkpoint.Name == "Checkpoint2");

			submission.FeedbackSent = true;
			submission.DateFeedbackRead = unread ? null : (DateTime?)DateTime.MaxValue;
			database.Context.SaveChanges();
			database.Reload();

			var submissionService = GetSubmissionService(database.Context);

			var result = await submissionService.GetSubmissionFeedbackAsync
			(
				"Class1",
				"Project1", 
				"Checkpoint2",
				submission.Id
			);

			Assert.Equal("Last2", result.LastName);
			Assert.Equal("First2", result.FirstName);
			Assert.Equal(submission.Commit.UserId, result.UserId);
			Assert.Equal("Project1_Last2First2", result.RepoName);
			Assert.Equal(CommitDates[3], result.PushDate);
			Assert.Equal("Feedback4", result.Feedback);
			Assert.Equal(3, result.PullRequestNumber);
			Assert.Equal(unread, result.Unread);
			Assert.False(result.FutureCheckpoints);
		}

		/// <summary>
		/// Ensures that MarkFeedbackReadAsync marks the given feedback
		/// as read.
		/// </summary>
		[Fact]
		public async Task MarkFeedbackReadAsync_MarksFeedbackAsRead()
		{
			var database = GetDatabaseBuilder().Build();

			var submissionId = database.Context.Submissions
				.Where(s => s.Commit.User.UserName == "Student2")
				.Single(s => s.Commit.Sha == "Commit3")
				.Id;

			var userId = database.Context.Users
				.Single(u => u.UserName == "Student2")
				.Id;

			database.Reload();

			var timeProvider = GetMockTimeProvider(DateTime.MaxValue);
			var submissionService = GetSubmissionService
			(
				database.Context,
				timeProvider: timeProvider.Object
			);

			await submissionService.MarkFeedbackReadAsync
			(
				"Class1",
				"Project1",
				"Checkpoint1",
				submissionId,
				userId
			);

			var submission = database.Context.Submissions
				.Single(s => s.Id == submissionId);

			Assert.Equal(DateTime.MaxValue, submission.DateFeedbackRead);
		}

		/// <summary>
		/// Ensures that GetUnreadFeedbackAsync returns information about 
		/// unread feedback, if any exists.
		/// </summary>
		[Fact]
		public async Task GetUnreadFeedbackAsync_UnreadFeedbackExists_ReturnsResult()
		{
			var database = GetDatabaseBuilder().Build();

			var submissionId = database.Context.Submissions
				.Where(s => s.Commit.User.UserName == "Student2")
				.Single(s => s.Commit.Sha == "Commit3")
				.Id;

			var userId = database.Context.Users
				.Single(u => u.UserName == "Student2")
				.Id;

			database.Reload();

			var submissionService = GetSubmissionService(database.Context);

			var results = await submissionService.GetUnreadFeedbackAsync
			(
				"Class1", 
				userId
			);

			Assert.Equal(1, results.Count);
			Assert.Equal("Project1", results[0].ProjectName);
			Assert.Equal("Checkpoint1", results[0].CheckpointName);
			Assert.Equal("Checkpoint1 DisplayName", results[0].CheckpointDisplayName);
			Assert.Equal(CommitDates[2], results[0].CommitDate);
			Assert.Equal(submissionId, results[0].SubmissionId);
		}

		/// <summary>
		/// Ensures that GetUnreadFeedbackAsync returns no unread feedback
		/// if none exists.
		/// </summary>
		[Fact]
		public async Task GetUnreadFeedbackAsync_NoUnreadFeedbackExists_ReturnsResult()
		{
			var database = GetDatabaseBuilder().Build();

			var userId = database.Context.Users
				.Single(u => u.UserName == "Student1")
				.Id;

			database.Reload();

			var submissionService = GetSubmissionService(database.Context);

			var results = await submissionService.GetUnreadFeedbackAsync
			(
				"Class1",
				userId
			);

			Assert.Equal(0, results.Count);
		}

		/// <summary>
		/// Returns build 2 for use during testing.
		/// </summary>
		private Build GetFailedBuild()
		{
			return new Build() { Status = BuildStatus.Error };
		}

		/// <summary>
		/// Returns build 1 for use during testing.
		/// </summary>
		private Build GetSuccessfulBuild()
		{
			return new Build()
			{
				Status = BuildStatus.Completed,
				TestResults = Collections.CreateList
				(
					new TestResult()
					{
						ClassName = "TestClass1",
						TestName = "Test1",
						Succeeded = true
					},
					new TestResult()
					{
						ClassName = "TestClass2",
						TestName = "Test2",
						Succeeded = false
					}
				)
			};
		}

		/// <summary>
		/// Returns a database with a project.
		/// </summary>
		private TestDatabaseBuilder GetDatabaseBuilder()
		{
			return new TestDatabaseBuilder()
				// Test data shared by many tests.  Includes Class1 Period1, and Class2 Period2
				.AddClassroom("Class1")
				.AddClassroom("Class2")
				.AddSection("Class1", "Period1")
				.AddSection("Class2", "Period2")
				.AddStudent("Student1", "Last1", "First1", "Class1", "Period1", "GitHubUser")
				.AddStudent("Student2", "Last2", "First2", "Class1", "Period1", "GitHubUser")
				.AddProject("Class1", "Project1")
				.AddProject("Class2", "Project2")
				.AddProjectTestClass("Class1", "Project1", "TestClass1")
				.AddProjectTestClass("Class1", "Project1", "TestClass2")
				.AddCheckpoint("Class1", "Project1", "Checkpoint1")
				.AddCheckpoint("Class1", "Project1", "Checkpoint2")
				.AddCheckpoint("Class2", "Project2", "Checkpoint1")
				.AddCheckpointDueDate("Class1", "Project1", "Checkpoint1", "Period1", CommitDates[0])
				.AddCheckpointDueDate("Class1", "Project1", "Checkpoint2", "Period1", CommitDates[3])
				.AddCheckpointDueDate("Class2", "Project2", "Checkpoint1", "Period2", CommitDates[0])
				.AddCheckpointTestClass("Class1", "Project1", "Checkpoint1", "TestClass1", required: true)
				.AddCheckpointTestClass("Class1", "Project1", "Checkpoint1", "TestClass2", required: false)
				.AddCheckpointTestClass("Class1", "Project1", "Checkpoint2", "TestClass1", required: true)
				.AddCheckpointTestClass("Class1", "Project1", "Checkpoint2", "TestClass2", required: true)
				.AddCommit("Class1", "Project1", "Student1", "Commit1", CommitDates[0], GetSuccessfulBuild())
				.AddCommit("Class1", "Project1", "Student1", "Commit2", CommitDates[1], GetFailedBuild())
				.AddCommit("Class1", "Project1", "Student1", "Commit3", CommitDates[2], GetSuccessfulBuild())
				.AddCommit("Class1", "Project1", "Student1", "Commit4", CommitDates[3], null /*inProgress*/)
				.AddCommit("Class1", "Project1", "Student2", "Commit1", CommitDates[0], GetSuccessfulBuild())
				.AddCommit("Class1", "Project1", "Student2", "Commit2", CommitDates[1], GetFailedBuild())
				.AddCommit("Class1", "Project1", "Student2", "Commit3", CommitDates[2], GetSuccessfulBuild())
				.AddCommit("Class1", "Project1", "Student2", "Commit4", CommitDates[3], GetSuccessfulBuild())
				.AddCommit("Class2", "Project2", "Student1", "Commit1", CommitDates[0], GetSuccessfulBuild())
				.AddSubmission("Class1", "Project1", "Checkpoint1", "Student1", "Commit1", SubmissionDates[0],
					1 /*pullRequest*/, "Feedback1", sentFeedback: true, readFeedback: true)
				.AddSubmission("Class1", "Project1", "Checkpoint1", "Student1", "Commit3", SubmissionDates[2],
					2 /*pullRequest*/, feedback: null, sentFeedback: false, readFeedback: false)
				.AddSubmission("Class1", "Project1", "Checkpoint1", "Student2", "Commit1", SubmissionDates[0],
					1 /*pullRequest*/, "Feedback1", sentFeedback: true, readFeedback: true)
				.AddSubmission("Class1", "Project1", "Checkpoint1", "Student2", "Commit3", SubmissionDates[2],
					2 /*pullRequest*/, "Feedback3", sentFeedback: true, readFeedback: false)
				.AddSubmission("Class1", "Project1", "Checkpoint2", "Student2", "Commit4", SubmissionDates[3],
					3 /*pullRequest*/, "Feedback4", sentFeedback: false, readFeedback: false)
				.AddSubmission("Class2", "Project2", "Checkpoint1", "Student1", "Commit1", SubmissionDates[0],
					1 /*pullRequest*/, "Feedback1", sentFeedback: true, readFeedback: false)

				// For the submission download tests, some additional periods, students, checkpoints,
				// commits, and submissions

				// Submission download tests: Class1 Period2, Student3-5
				.AddSection("Class1", "Period2")
				.AddStudent("Student3", "Last3", "First3", "Class1", "Period2", "GitHubUser")
				.AddStudent("Student4", "Last4", "First4", "Class1", "Period2", "GitHubUser")
				.AddStudent("Student5", "Last5", "First5", "Class1", "Period2", "GitHubUser")
				.AddCheckpointDueDate("Class1", "Project1", "Checkpoint1", "Period2", CommitDates[0])
				.AddCheckpointDueDate("Class1", "Project1", "Checkpoint2", "Period2", CommitDates[3])
				.AddCommit("Class1", "Project1", "Student3", "Commit1", CommitDates[0], GetSuccessfulBuild())
				.AddCommit("Class1", "Project1", "Student3", "Commit2", CommitDates[1], GetFailedBuild())
				.AddCommit("Class1", "Project1", "Student3", "Commit3", CommitDates[2], GetSuccessfulBuild())
				.AddCommit("Class1", "Project1", "Student3", "Commit4", CommitDates[3], null /*inProgress*/)
				.AddCommit("Class1", "Project1", "Student4", "Commit1", CommitDates[0], GetSuccessfulBuild())
				.AddCommit("Class1", "Project1", "Student4", "Commit2", CommitDates[1], GetFailedBuild())
				.AddCommit("Class1", "Project1", "Student4", "Commit3", CommitDates[2], GetSuccessfulBuild())
				.AddCommit("Class1", "Project1", "Student4", "Commit4", CommitDates[3], GetSuccessfulBuild())
				.AddCommit("Class1", "Project1", "Student5", "Commit1", CommitDates[0], GetSuccessfulBuild())
				.AddCommit("Class1", "Project1", "Student5", "Commit2", CommitDates[1], GetFailedBuild())
				.AddCommit("Class1", "Project1", "Student5", "Commit3", CommitDates[2], GetSuccessfulBuild())
				.AddCommit("Class1", "Project1", "Student5", "Commit4", CommitDates[3], GetSuccessfulBuild())
				.AddSubmission("Class1", "Project1", "Checkpoint1", "Student3", "Commit1", SubmissionDates[0],
					1 /*pullRequest*/, "Feedback1", sentFeedback: true, readFeedback: true)
				.AddSubmission("Class1", "Project1", "Checkpoint1", "Student3", "Commit3", SubmissionDates[2],
					2 /*pullRequest*/, "Feedback3", sentFeedback: true, readFeedback: false)
				.AddSubmission("Class1", "Project1", "Checkpoint1", "Student4", "Commit1", SubmissionDates[0],
					1 /*pullRequest*/, "Feedback1", sentFeedback: true, readFeedback: true)
				.AddSubmission("Class1", "Project1", "Checkpoint1", "Student4", "Commit3", SubmissionDates[2],
					2 /*pullRequest*/, feedback: null, sentFeedback: false, readFeedback: false)
				.AddSubmission("Class1", "Project1", "Checkpoint2", "Student4", "Commit4", SubmissionDates[3],
					3 /*pullRequest*/, "Feedback4", sentFeedback: false, readFeedback: false)
				.AddSubmission("Class1", "Project1", "Checkpoint1", "Student5", "Commit1", SubmissionDates[0],
					1 /*pullRequest*/, "Feedback1", sentFeedback: true, readFeedback: true)
				.AddSubmission("Class1", "Project1", "Checkpoint1", "Student5", "Commit3", SubmissionDates[2],
					2 /*pullRequest*/, "Feedback3", sentFeedback: true, readFeedback: false)
				.AddSubmission("Class1", "Project1", "Checkpoint2", "Student5", "Commit4", SubmissionDates[3],
					3 /*pullRequest*/, "Feedback4", sentFeedback: false, readFeedback: false)

				// Submission download tests: Class1 Period3, Student6-8
				.AddSection("Class1", "Period3")
				.AddStudent("Student6", "Last6", "First6", "Class1", "Period3", "GitHubUser")
				.AddStudent("Student7", "Last7", "First7", "Class1", "Period3", "GitHubUser")
				.AddStudent("Student8", "Last8", "First8", "Class1", "Period3", "GitHubUser")
				.AddCheckpointDueDate("Class1", "Project1", "Checkpoint1", "Period3", CommitDates[0])
				.AddCheckpointDueDate("Class1", "Project1", "Checkpoint2", "Period3", CommitDates[3])
				.AddCommit("Class1", "Project1", "Student6", "Commit1", CommitDates[0], GetSuccessfulBuild())
				.AddCommit("Class1", "Project1", "Student6", "Commit2", CommitDates[1], GetFailedBuild())
				.AddCommit("Class1", "Project1", "Student6", "Commit3", CommitDates[2], GetSuccessfulBuild())
				.AddCommit("Class1", "Project1", "Student6", "Commit4", CommitDates[3], null /*inProgress*/)
				.AddCommit("Class1", "Project1", "Student7", "Commit1", CommitDates[0], GetSuccessfulBuild())
				.AddCommit("Class1", "Project1", "Student7", "Commit2", CommitDates[1], GetFailedBuild())
				.AddCommit("Class1", "Project1", "Student7", "Commit3", CommitDates[2], GetSuccessfulBuild())
				.AddCommit("Class1", "Project1", "Student7", "Commit4", CommitDates[3], GetSuccessfulBuild())
				.AddCommit("Class1", "Project1", "Student8", "Commit1", CommitDates[0], GetSuccessfulBuild())
				.AddCommit("Class1", "Project1", "Student8", "Commit2", CommitDates[1], GetFailedBuild())
				.AddCommit("Class1", "Project1", "Student8", "Commit3", CommitDates[2], GetSuccessfulBuild())
				.AddCommit("Class1", "Project1", "Student8", "Commit4", CommitDates[3], null /*inProgress*/)
				.AddSubmission("Class1", "Project1", "Checkpoint1", "Student6", "Commit1", SubmissionDates[0],
					1 /*pullRequest*/, "Feedback1", sentFeedback: true, readFeedback: true)
				.AddSubmission("Class1", "Project1", "Checkpoint1", "Student6", "Commit3", SubmissionDates[2],
					2 /*pullRequest*/, feedback: null, sentFeedback: false, readFeedback: false)
				.AddSubmission("Class1", "Project1", "Checkpoint1", "Student7", "Commit1", SubmissionDates[0],
					1 /*pullRequest*/, "Feedback1", sentFeedback: true, readFeedback: true)
				.AddSubmission("Class1", "Project1", "Checkpoint1", "Student7", "Commit3", SubmissionDates[2],
					2 /*pullRequest*/, "Feedback3", sentFeedback: true, readFeedback: false)
				.AddSubmission("Class1", "Project1", "Checkpoint2", "Student7", "Commit4", SubmissionDates[3],
					3 /*pullRequest*/, "Feedback4", sentFeedback: false, readFeedback: false)
				.AddSubmission("Class1", "Project1", "Checkpoint1", "Student8", "Commit1", SubmissionDates[0],
					1 /*pullRequest*/, "Feedback1", sentFeedback: true, readFeedback: true)
				.AddSubmission("Class1", "Project1", "Checkpoint1", "Student8", "Commit3", SubmissionDates[2],
					2 /*pullRequest*/, feedback: null, sentFeedback: false, readFeedback: false);
		}

		/// <summary>
		/// Returns a mock submission creator that returns submission candidates.
		/// </summary>
		private Mock<ISubmissionCreator> GetMockSubmissionCreator(
			IList<string> submissionCandidates = null)
		{
			var submissionCreator = new Mock<ISubmissionCreator>();

			submissionCreator
				.Setup
				(
					sc => sc.GetSubmissionCandidatesAsync
					(
						It.Is<Project>(p => p.Name == "Project1"),
						It.Is<User>(u => u.UserName == "Student1")
					)
				).ReturnsAsync(submissionCandidates);

			return submissionCreator;
		}

		/// <summary>
		/// Returns a mock submission creator that creates a pull request.
		/// </summary>
		private Mock<ISubmissionCreator> GetMockSubmissionCreator(
			int pullRequestId)
		{
			var submissionCreator = new Mock<ISubmissionCreator>();

			submissionCreator
				.Setup
				(
					sc => sc.CreatePullRequestAsync
					(
						It.Is<Commit>(c => c.Sha == "Commit3"),
						It.Is<Checkpoint>(c => c.Name == "Checkpoint2")
					)
				)
				.ReturnsAsync(pullRequestId);

			return submissionCreator;
		}

		/// <summary>
		/// Returns a mock submission downloader.
		/// </summary>
		private Mock<ISubmissionDownloader> GetMockSubmissionDownloader(
			IArchive templateContents,
			StudentSubmissions studentSubmissions)
		{
			var submissionDownloader = new Mock<ISubmissionDownloader>();

			submissionDownloader
				.Setup
				(
					sd => sd.DownloadTemplateContentsAsync
					(
						It.Is<Project>(p => p.Name == "Project1")
					)
				).ReturnsAsync(templateContents);

			// See comment in DownloadSubmissionsAsync_DownloadsSubmissions for summary of
			// student/period/submission/selection combos being tested
			submissionDownloader
				.Setup
				(
					sd => sd.DownloadSubmissionsAsync
					(
						It.Is<Checkpoint>(c => c.Name == "Checkpoint2"),
						It.Is<IList<StudentDownloadRequest>>
						(
							requests => requests.Count == 4
									&& requests[0].Student.User.UserName == "Student3"
									&& requests[0].Submitted == false
									&& requests[1].Student.User.UserName == "Student4"
									&& requests[1].Submitted == true
									&& requests[2].Student.User.UserName == "Student6"
									&& requests[2].Submitted == false
									&& requests[3].Student.User.UserName == "Student7"
									&& requests[3].Submitted == true
						)
					)
				).ReturnsAsync(studentSubmissions);

			return submissionDownloader;
		}

		/// <summary>
		/// Returns a mock submission archive builder.
		/// </summary>
		private Mock<ISubmissionArchiveBuilder> GetMockSubmissionArchiveBuilder(
			IArchive templateContents,
			StudentSubmissions studentSubmissions,
			Stream expectedArchive,
			ProjectSubmissionDownloadFormat format)
		{
			var archiveBuilder = new Mock<ISubmissionArchiveBuilder>();

			archiveBuilder
				.Setup
				(
					builder => builder.BuildSubmissionArchiveAsync
					(
						It.Is<Project>(p => p.Name == "Project1"),
						templateContents,
						studentSubmissions,
						format
					)
				).ReturnsAsync(expectedArchive);

			return archiveBuilder;
		}

		/// <summary>
		/// Returns a mock time provider.
		/// </summary>
		private Mock<ITimeProvider> GetMockTimeProvider(DateTime currentTime)
		{
			var timeProvider = new Mock<ITimeProvider>();

			timeProvider
				.Setup(tp => tp.UtcNow)
				.Returns(currentTime);

			return timeProvider;
		}

		/// <summary>
		/// Returns a mock e-mail provider.
		/// </summary>
		private Mock<IEmailProvider> GetMockEmailProvider(bool feedbackSent)
		{
			var emailProvider = new Mock<IEmailProvider>(MockBehavior.Strict);

			emailProvider
				.Setup(GetSendMessageExpression(feedbackSent))
				.Returns(Task.CompletedTask);

			return emailProvider;
		}

		/// <summary>
		/// Returns the expression for sending an e-mail message with submission feedback.
		/// </summary>
		private static Expression<Func<IEmailProvider, Task>> GetSendMessageExpression(
			bool feedbackSent)
		{
			if (feedbackSent)
			{
				return ep => ep.SendMessageAsync
				(
					It.Is<IList<EmailRecipient>>
					(
						to => to.Count == 1 && to[0].EmailAddress == "Student2Email"
					),
					$"Project1 Checkpoint2 DisplayName Feedback",
					It.Is<string>
					(
						body =>
							body.Contains("Feedback4")
							&& body.Contains("UrlMarkAsRead")
					),
					null /* customSender */,
					null /* threadInfo */
				);
			}
			else
			{
				return ep => ep.SendMessageAsync
				(
					It.IsAny<IList<EmailRecipient>>(),
					It.IsAny<string>(),
					It.IsAny<string>(),
					null /* customSender */,
					null /* threadInfo */
				);
			}
		}

		/// <summary>
		/// Returns a new submission service.
		/// </summary>
		private ISubmissionService GetSubmissionService(
			DatabaseContext dbContext,
			ISubmissionCreator submissionCreator = null,
			ISubmissionDownloader submissionDownloader = null,
			ISubmissionArchiveBuilder submissionArchiveBuilder = null,
			ITimeProvider timeProvider = null,
			IEmailProvider emailProvider = null)
		{
			return new SubmissionService
			(
				new Mock<ILogger<SubmissionService>>().Object,
				dbContext,
				submissionCreator,
				submissionDownloader,
				submissionArchiveBuilder,
				timeProvider,
				emailProvider
			);
		}

		/// <summary>
		/// The date of commit 1.
		/// </summary>
		private static readonly IList<DateTime> CommitDates = Collections.CreateList
		(
			new DateTime(2016, 1, 1),
			new DateTime(2016, 1, 2),
			new DateTime(2016, 1, 3),
			new DateTime(2016, 1, 4)
		);

		/// <summary>
		/// The date of commit 1.
		/// </summary>
		private static readonly IList<DateTime> SubmissionDates = Collections.CreateList
		(
			new DateTime(2016, 1, 5),
			new DateTime(2016, 1, 6),
			new DateTime(2016, 1, 7),
			new DateTime(2016, 1, 8)
		);
	}
}
