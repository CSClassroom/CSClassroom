using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.Async;
using CSC.Common.Infrastructure.Extensions;
using CSC.Common.Infrastructure.System;
using Octokit;
using System.Net;

namespace CSC.Common.Infrastructure.GitHub
{
	/// <summary>
	/// Performs repository-related operations.
	/// </summary>
	public class GitHubRepositoryClient : IGitHubRepositoryClient
	{
		/// <summary>
		/// The GitHub client.
		/// </summary>
		private readonly GitHubClient _client;

		/// <summary>
		/// The secret used to sign GitHub webhook payloads.
		/// </summary>
		private readonly GitHubWebhookSecret _webhookSecret;

		/// <summary>
		/// The file system.
		/// </summary>
		private readonly IFileSystem _fileSystem;

		/// <summary>
		/// An operation runner.
		/// </summary>
		private readonly IOperationRunner _operationRunner;

		/// <summary>
		/// The archive factory.
		/// </summary>
		private readonly IArchiveFactory _archiveFactory;

		/// <summary>
		/// The number of attempts to make for retryable GitHub operations.
		/// </summary>
		private const int c_numAttempts = 5;

		/// <summary>
		/// The delay between attempts of retryable GitHub operations.
		/// </summary>
		private readonly TimeSpan c_delayBetweenAttempts 
			= TimeSpan.FromSeconds(2);

		/// <summary>
		/// Constructor.
		/// </summary>
		public GitHubRepositoryClient(
			GitHubClient client, 
			GitHubWebhookSecret webhookSecret,
			IFileSystem fileSystem,
			IOperationRunner operationRunner,
			IArchiveFactory archiveFactory)
		{
			_client = client;
			_webhookSecret = webhookSecret;
			_fileSystem = fileSystem;
			_operationRunner = operationRunner;
			_archiveFactory = archiveFactory;
		}

		/// <summary>
		/// Returns all repositories in the GitHub organization.
		/// </summary>
		public async Task<ICollection<GitHubRepository>> GetAllRepositoriesAsync(
			string organizationName)
		{
			var repositories = await _client.Repository.GetAllForOrg(organizationName);

			return repositories
				.Select(ToModelRepository)
				.ToList();
		}

		/// <summary>
		/// Returns the given repository in the GitHub organization.
		/// </summary>
		public async Task<GitHubRepository> GetRepositoryAsync(
			string organizationName, 
			string repositoryName)
		{
			var repository = await _client.Repository.Get(organizationName, repositoryName);

			return ToModelRepository(repository);
		}

		/// <summary>
		/// Returns the given repository in the GitHub organization.
		/// </summary>
		public async Task<IArchive> GetRepositoryContentsAsync(
			string organizationName,
			string repositoryName,
			string branchName,
			ArchiveStore backingStore)
		{
			var reference = branchName != null
				? $"heads/{branchName}"
				: string.Empty;

			byte[] sourceRepoBytes = await _client.Repository.Content.GetArchive
			(
				organizationName,
				repositoryName,
				ArchiveFormat.Zipball,
				reference
			);

			return GetArchive(sourceRepoBytes, backingStore);
		}

		/// <summary>
		/// Creates a new repository.
		/// </summary>
		public async Task<GitHubRepository> CreateRepositoryAsync(
			string organizationName,
			string repositoryName,
			GitHubTeam team,
			bool overwrite)
		{
			var repository = await RetryGitHubOperationIfNeededAsync
			(
				() => _client.Repository.Create
				(
					organizationName,
					new NewRepository(repositoryName)
					{
						TeamId = team.Id,
						AutoInit = true,
						Private = true
					}
				)
			);

			return ToModelRepository(repository);
		}

		/// <summary>
		/// Converts an OctoKit repo to a model repo.
		/// </summary>
		private GitHubRepository ToModelRepository(Repository repo)
		{
			return new GitHubRepository
			(
				repo.Id,
				repo.Owner.Login,
				repo.Name,
				repo.DefaultBranch
			);
		}

		/// <summary>
		/// Converts an OctoKit repo to a model repo.
		/// </summary>
		private GitHubCommit ToModelCommit(
			Octokit.GitHubCommit commit,
			string organizationName,
			string repositoryName)
		{
			return new GitHubCommit
			(
				commit.Sha,
				organizationName,
				repositoryName,
				commit.Author?.Login,
				commit.Commit.Message,
				commit.Commit.Committer.Date,
				Enumerable.ToList<string>(commit.Parents
						.Select(parent => parent.Sha))
			);
		}

		/// <summary>
		/// Returns all commits for the given repository/
		/// </summary>
		public async Task<ICollection<GitHubCommit>> GetAllCommitsAsync(
			string organizationName,
			string repositoryName)
		{
			var commits = await _client.Repository.Commit.GetAll
			(
				organizationName, 
				repositoryName
			);

			return commits
				.Select(c => ToModelCommit(c, organizationName, repositoryName))
				.ToList();
		}

		/// <summary>
		/// Overwrites an existing repository with an existing archive.
		/// All existing commits will be erased, and the repository will
		/// be populated with two commits.
		/// </summary>
		public async Task OverwriteRepositoryAsync(
			GitHubRepository repository,
			string commitMessage,
			IArchive contents,
			Func<IArchiveFile, bool> includeFile,
			Func<IArchiveFile, bool> includeInFirstCommit)
		{
			var firstBatch = contents.Files
				.Where(file => includeFile(file) && includeInFirstCommit(file))
				.ToList();

			Commit firstCommit = null;

			if (firstBatch.Count > 0)
			{
				firstCommit = await RetryGitHubOperationIfNeededAsync
				(
					() => CreateCommitAsync
					(
						repository,
						$"{commitMessage}: Part 1",
						firstBatch,
						parentCommit: null
					)
				);
			}

			var secondBatch = contents.Files
				.Where(file => includeFile(file) && !includeInFirstCommit(file))
				.ToList();

			var starterCommit = await RetryGitHubOperationIfNeededAsync
			(
				() => CreateCommitAsync
				(
					repository,
					firstCommit != null
						? $"{commitMessage}: Part 2"
						: commitMessage,
					secondBatch,
					firstCommit
				)
			);

			await RetryGitHubOperationIfNeededAsync
			(
				() => _client.Git.Reference.Update
				(
					repository.Owner,
					repository.Name,
					$"heads/{repository.DefaultBranch}",
					new ReferenceUpdate(starterCommit.Sha, force: true)
				)
			);
		}

		/// <summary>
		/// Adds a push webhook for the repository.
		/// </summary>
		public Task EnsurePushWebhookAsync(GitHubRepository repository, string url)
		{
			return RetryGitHubOperationIfNeededAsync
			(
				async () =>
				{
					var webhooks = await _client.Repository.Hooks.GetAll
					(
						repository.Owner,
						repository.Name
					);

					if (!webhooks.Any(webhook => webhook.Config["url"] == url))
					{
						await RetryGitHubOperationIfNeededAsync
						(
							() => _client.Repository.Hooks.Create
							(
								repository.Owner,
								repository.Name,
								new NewRepositoryHook
								(
									"web",
									new Dictionary<string, string>()
									{
										{"url", url},
										{"content_type", "json"},
										{"secret", _webhookSecret.Value},
										{"insecure_ssl", "0"}
									}
								)
								{
									Active = true,
									Events = new[] { "push" }
								}
							)
						);

						webhooks = await _client.Repository.Hooks.GetAll
						(
							repository.Owner,
							repository.Name
						);

						if (!webhooks.Any(webhook => webhook.Config["url"] == url))
						{
							throw new ApiException
							(
								$"Webhook not found for repository {repository.Name}",
								HttpStatusCode.NotFound
							);
						}
					}

					return true;
				}
			);
		}

		/// <summary>
		/// Returns a list of push events for the given repository.
		/// </summary>
		public async Task<IList<GitHubPushEvent>> GetPushEventsAsync(string orgName, string repoName)
		{
			var allEvents = await _client.Activity
				.Events
				.GetAllForRepository(orgName, repoName);

			var allCommits = await _client.Repository.Commit.GetAll(orgName, repoName);

			var commitOrder = allCommits.Reverse()
				.Select((c, i) => new {c.Sha, Order = i})
				.ToDictionary(c => c.Sha, c => c.Order);

			return Enumerable.
				Select
				(
					allEvents.Where
					(
						   activity => activity.Type == "PushEvent"
						&& activity.Payload is PushEventPayload
					), 
					activity => new
					{
						activity.CreatedAt,
						Payload = (PushEventPayload)activity.Payload
					}
				).Select
				(
					activity => new GitHubPushEvent()
					{
						CreatedAt = activity.CreatedAt,
						Repository = new PushEventRepository()
						{
							Owner = new PushEventRepositoryOwner()
							{
								Name = orgName
							},
							Name = repoName
						},
						Ref = activity.Payload.Ref,
						After = activity.Payload.Head,
						Commits = activity.Payload
							.Commits
							.OrderBy(c => commitOrder.GetValueOrDefault(c.Sha))
							.Select
							(
								(commit, index) => new GitHubPushEventCommit()
								{
									Id = commit.Sha,
									Message = commit.Message,
									Timestamp = activity.CreatedAt.AddSeconds(index)
								}
							)
							.ToList()
					}
				).ToList();
		}

		/// <summary>
		/// Ensures that a branch is created with the given commit.
		/// If the branch already exists, its reference is updated.
		/// </summary>
		public async Task CreateBranchAsync(
			string orgName,
			string repoName,
			string branchName,
			string commitSha)
		{
			string refName = $"heads/{branchName}";

			try
			{
				if (await _client.Git.Reference.Get(orgName, repoName, refName) != null)
				{
					await _client.Git.Reference.Delete(orgName, repoName, refName);
				}
			}
			catch (NotFoundException)
			{
				// Doesn't exist.
			}

			await _client.Git.Reference.Create
			(
				orgName,
				repoName,
				new NewReference(refName, commitSha)
			);
		}

		/// <summary>
		/// Creates a pull request, and returns the pull request number.
		/// </summary>
		public async Task<int> CreatePullRequestAsync(
			string orgName,
			string repoName,
			string title,
			string sourceBranchName,
			string destBranchName)
		{
			var pullRequest = await RetryGitHubOperationIfNeededAsync
			(
				() => _client.Repository.PullRequest.Create
				(
					orgName,
					repoName,
					new NewPullRequest
					(
						title,
						sourceBranchName,
						destBranchName
					)
				)
			);

			await RetryGitHubOperationIfNeededAsync
			(
				() => _client.PullRequest.Merge
				(
					orgName,
					repoName,
					pullRequest.Number,
					new MergePullRequest()
				)
			);

			return pullRequest.Number;
		}

		/// <summary>
		/// Deletes the given branch.
		/// </summary>
		public async Task DeleteBranchAsync(
			string orgName, 
			string repoName, 
			string branchName)
		{
			var refName = $"heads/{branchName}";

			await _client.Git.Reference.Delete(orgName, repoName, refName);
		}

		/// <summary>
		/// Creates a new commit.
		/// </summary>
		private async Task<Octokit.Commit> CreateCommitAsync(
			GitHubRepository repository,
			string commitMessage,
			IList<IArchiveFile> fileContents,
			Octokit.Commit parentCommit)
		{
			var newTreeResponse = await RetryGitHubOperationIfNeededAsync
			(
				async () => await _client.Git.Tree.Create
				(
					repository.Owner,
					repository.Name,
					await GetTreeToPushAsync
					(
						repository,
						fileContents,
						parentCommit?.Tree?.Sha
					)
				)
			);

			return await RetryGitHubOperationIfNeededAsync
			(
				() => _client.Git.Commit.Create
				(
					repository.Owner,
					repository.Name,
					parentCommit != null
						? new NewCommit(commitMessage, newTreeResponse.Sha, parentCommit.Sha)
						: new NewCommit(commitMessage, newTreeResponse.Sha)
				)
			);
		}

		/// <summary>
		/// Creates a new project tree.
		/// </summary>
		private async Task<NewTree> GetTreeToPushAsync(
			GitHubRepository repository,
			IList<IArchiveFile> fileContents,
			string baseTreeSha)
		{
			NewTree newTree = new NewTree()
			{
				BaseTree = baseTreeSha
			};

			foreach (var entry in fileContents)
			{
				var newTreeItem = new NewTreeItem()
				{
					Path = entry.FullPath,
					Mode = "100644" /*blob*/,
					Type = TreeType.Blob
				};

				if (entry.Ascii)
				{
					newTreeItem.Content = entry.GetEncodedData();
				}
				else
				{
					var blobRef = await RetryGitHubOperationIfNeededAsync
					(
						() => _client.Git.Blob.Create
						(
							repository.Owner,
							repository.Name,
							new NewBlob()
							{
								Encoding = EncodingType.Base64,
								Content = entry.GetEncodedData()
							}
						)
					);

					newTreeItem.Sha = blobRef.Sha;
				}

				newTree.Tree.Add(newTreeItem);
			}

			return newTree;
		}

		/// <summary>
		/// Executes a GitHub operation, retrying the operation if it 
		/// fails with an ApiException.
		/// </summary>
		private async Task<TResult> RetryGitHubOperationIfNeededAsync<TResult>(
			Func<Task<TResult>> operation)
		{
			return await _operationRunner.RetryOperationIfNeededAsync
			(
				operation,
				ex => ex is ApiException,
				c_numAttempts,
				c_delayBetweenAttempts,
				defaultResultIfFailed: false
			);
		}

		/// <summary>
		/// Returns an archive for the downloaded repository.
		/// </summary>
		private IArchive GetArchive(byte[] sourceRepoBytes, ArchiveStore archiveStore)
		{
			switch (archiveStore)
			{
				case ArchiveStore.FileSystem:
					return GetFileSystemBackedArchive(sourceRepoBytes);

				case ArchiveStore.Memory:
					return GetMemoryBackedArchive(sourceRepoBytes);

				default:
					throw new ArgumentOutOfRangeException(nameof(archiveStore));
			}
		}

		/// <summary>
		/// Returns a compressed archive backed by a temporary file.
		/// The file will be deleted when the archive is disposed.
		/// </summary>
		private IArchive GetFileSystemBackedArchive(byte[] sourceRepoBytes)
		{
			var fileStream = _fileSystem.CreateNewTempFile();
			fileStream.Write(sourceRepoBytes, 0, sourceRepoBytes.Length);
			fileStream.Position = 0;

			var archive = new ZipArchive(fileStream, ZipArchiveMode.Read);

			return _archiveFactory.CreateCompressedArchive
			(
				archive, 
				stripInitialFolders: 1
			);
		}

		/// <summary>
		/// Returns an uncompressed archived backed by memory.
		/// </summary>
		private IArchive GetMemoryBackedArchive(byte[] sourceRepoBytes)
		{
			using (var zipArchive = new ZipArchive(new MemoryStream(sourceRepoBytes)))
			{
				return _archiveFactory.CreateUncompressedArchive
				(
					zipArchive.Entries
						.Where(entry => entry.IsFile())
						.ToDictionary
						(
							entry => entry.FullName.Substring
							(
								entry.FullName.IndexOf("/") + 1
							),
							GetRawFileData
						)
				);
			}
		}

		/// <summary>
		/// Returns the raw file data of a zip entry.
		/// </summary>
		private static byte[] GetRawFileData(ZipArchiveEntry entry)
		{
			using (var stream = entry.Open())
			using (var memoryStream = new MemoryStream())
			{
				stream.CopyTo(memoryStream);

				return memoryStream.ToArray();
			}
		}
	}
}
