using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSC.BuildService.Model.ProjectRunner;
using CSC.BuildService.Service.ProjectRunner;
using CSC.Common.Infrastructure.Logging;
using CSC.Common.Infrastructure.Queue;
using CSC.CSClassroom.Model.Projects;
using CSC.CSClassroom.Service.Projects.Repositories;

namespace CSC.CSClassroom.Service.Projects.PushEvents
{
	/// <summary>
	/// Creates build jobs for a set of commits.
	/// </summary>
	public class PushEventProcessor : IPushEventProcessor
	{
		/// <summary>
		/// The background job client.
		/// </summary>
		private readonly IJobQueueClient _jobQueueClient;

		/// <summary>
		/// The operation ID provider.
		/// </summary>
		private readonly IOperationIdProvider _operationIdProvider;

		/// <summary>
		/// Constructor.
		/// </summary>
		public PushEventProcessor(
			IJobQueueClient jobQueueClient,
			IOperationIdProvider operationIdProvider)
		{
			_jobQueueClient = jobQueueClient;
			_operationIdProvider = operationIdProvider;
		}

		/// <summary>
		/// Given a list of push events, along with a list of 
		/// existing recorded commits, returns a list of commits
		/// that need to be processed.
		/// <param name="project">The project whose commits we are processing.</param>
		/// <param name="repoEventLists">The push events to process if needed.</param>
		/// <param name="existingCommits">The commits that have already been processed.</param>
		/// </summary>
		public IList<PushEventCommit> GetNewCommitsToProcess(
			Project project,
			ICollection<CommitDescriptor> existingCommits,
			IList<StudentRepoPushEvents> repoEventLists)
		{
			var allPushEvents = repoEventLists
				.SelectMany
				(
					repoEventList => repoEventList.PushEvents,
					(repoEventList, pushEvent) => new
					{
						repoEventList.Student,
						PushEvent = pushEvent
					}
				).ToList();

			var allPushEventCommits = allPushEvents
				.SelectMany
				(
					studentPushEvent => studentPushEvent.PushEvent.Commits,
					(studentPushEvent, commit) => new
					{
						studentPushEvent.Student,
						studentPushEvent.PushEvent,
						Commit = commit
					}
				).ToList();

			var allPushEventCommitDescriptors = allPushEventCommits
				.Select
				(
					studentPushEventCommit => new
					{
						studentPushEventCommit.Student,
						studentPushEventCommit.PushEvent,
						studentPushEventCommit.Commit,
						CommitDescriptor = new CommitDescriptor
						(
							studentPushEventCommit.Commit.Id,
							project.Id,
							studentPushEventCommit.Student.UserId
						)
					}
				).ToList();

			var allNewPushEventCommitDescriptors = allPushEventCommitDescriptors
				.Where
				(
					newStudentCommit => !existingCommits.Contains
					(
						newStudentCommit.CommitDescriptor
					)
				).ToList();

			var commitsToAdd = allNewPushEventCommitDescriptors
				.Select
				(
					newStudentCommit => new PushEventCommit
					(
						newStudentCommit.PushEvent,
						new Commit()
						{
							Sha = newStudentCommit.Commit.Id,
							ProjectId = project.Id,
							UserId = newStudentCommit.Student.UserId,
							PushDate = newStudentCommit.PushEvent.CreatedAt.UtcDateTime,
							CommitDate = newStudentCommit.Commit.Timestamp.UtcDateTime,
							Message = newStudentCommit.Commit.Message,
							BuildRequestToken = project.ExplicitSubmissionRequired
								? Guid.NewGuid().ToString()
								: null
						}
					)
				).ToList();

			var uniqueCommitsToAdd = commitsToAdd
				.GroupBy
				(
					commitToAdd => new
					{
						commitToAdd.Commit.UserId,
						commitToAdd.Commit.Sha
					}
				)
				.Select
				(
					group => group.OrderBy(c => c.Commit.PushDate).Last()
				).ToList();

			return uniqueCommitsToAdd;
		}

		/// <summary>
		/// Creates a build job for a new commit received by a push event.
		/// Returns the job ID for the build job.
		/// </summary>
		public async Task<string> CreateBuildJobAsync(
			Project project,
			PushEventCommit newCommit,
			string buildResultCallbackUrl)
		{
			var projectJob = new ProjectJob
			(
				newCommit.Commit.BuildRequestToken,
				newCommit.PushEvent.Repository.Owner.Name,
				project.Name,
				newCommit.PushEvent.Repository.Name,
				$"{project.Name}_Template",
				newCommit.Commit.Sha,
				project.PrivateFilePaths
					.Select(p => p.Path)
					.Concat(project.ImmutableFilePaths.Select(p => p.Path))
					.ToList(),
				project.TestClasses
					.Select(tc => tc.ClassName)
					.ToList(),
				buildResultCallbackUrl
			);

			var jobId = await _jobQueueClient.EnqueueAsync<IProjectRunnerService>
			(
				service => service.ExecuteProjectJobAsync
				(
					projectJob,
					_operationIdProvider.OperationId
				)
			);

			return jobId;
		}
	}
}
