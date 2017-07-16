using System;
using System.Collections.Generic;
using System.Linq;
using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Model.Questions;
using CSC.CSClassroom.Model.Projects;
using CSC.CSClassroom.Model.Projects.ServiceResults;
using CSC.CSClassroom.Model.Users;
using CSC.CSClassroom.Repository;
using CSC.CSClassroom.Service.UnitTests.Utilities;
using Microsoft.EntityFrameworkCore;

namespace CSC.CSClassroom.Service.UnitTests.TestDoubles
{
	/// <summary>
	/// Creates a new database context.
	/// </summary>
	public class TestDatabaseBuilder
	{
		/// <summary>
		/// The database store.
		/// </summary>
		private readonly TestDatabaseStore _store;

		/// <summary>
		/// The context used to initialize the database.
		/// </summary>
		private readonly DatabaseContext _buildContext;

		/// <summary>
		/// Creates a builder for an in-memory database.
		/// </summary>
		public TestDatabaseBuilder()
		{
			_store = new TestDatabaseStore();
			_buildContext = new DatabaseContext(_store.Options);
		}

		/// <summary>
		/// Adds a classroom to the database.
		/// </summary>
		public TestDatabaseBuilder AddClassroom(string classroomName)
		{
			var classroom = new Classroom()
			{
				Name = classroomName,
				DisplayName = $"{classroomName} DisplayName",
				GitHubOrganization = $"{classroomName}GitHubOrg"
			};

			_buildContext.Classrooms.Add(classroom);
			_buildContext.SaveChanges();

			return this;
		}

		/// <summary>
		/// Adds a section to the database.
		/// </summary>
		public TestDatabaseBuilder AddSection(
			string classroomName,
			string sectionName,
			bool allowRegistrations = true)
		{
			var classroom = _buildContext.Classrooms
				.Single(c => c.Name == classroomName);

			var section = new Section()
			{
				Name = sectionName,
				DisplayName = $"{sectionName} DisplayName",
				ClassroomId = classroom.Id,
				AllowNewRegistrations = allowRegistrations
			};

			_buildContext.Sections.Add(section);
			_buildContext.SaveChanges();

			return this;
		}

		/// <summary>
		/// Adds a question category to the database.
		/// </summary>
		public TestDatabaseBuilder AddQuestionCategory(
			string classroomName,
			string categoryName)
		{
			var classroom = _buildContext.Classrooms
				.Single(c => c.Name == classroomName);

			var questionCategory = new QuestionCategory()
			{
				Name = categoryName,
				ClassroomId = classroom.Id
			};

			_buildContext.QuestionCategories.Add(questionCategory);
			_buildContext.SaveChanges();

			return this;
		}

		/// <summary>
		/// Adds a question category to the database.
		/// </summary>
		public TestDatabaseBuilder AddQuestion(
			string classroomName,
			string questionCategoryName,
			Question question)
		{
			var questionCategory = _buildContext.QuestionCategories
				.Where(qc => qc.Classroom.Name == classroomName)
				.Single(qc => qc.Name == questionCategoryName);

			question.QuestionCategoryId = questionCategory.Id;

			var randomlySelectedQuestion = question as RandomlySelectedQuestion;
			if (randomlySelectedQuestion?.ChoicesCategory != null)
			{
				var classroom = _buildContext.Classrooms
					.Single(c => c.Name == classroomName);

				randomlySelectedQuestion.ChoicesCategory.Classroom = classroom;
			}

			_buildContext.Questions.Add(question);
			_buildContext.SaveChanges();

			return this;
		}

		/// <summary>
		/// Adds a prerequisite question category to the database.
		/// </summary>
		public TestDatabaseBuilder AddPrerequisiteQuestion(
			string classroomName,
			string firstQuestionCategoryName,
			string firstQuestionName,
			string secondQuestionCategoryName,
			string secondQuestionName)
		{
			var firstQuestionCategory = _buildContext.QuestionCategories
				.Where(qc => qc.Classroom.Name == classroomName)
				.Single(qc => qc.Name == firstQuestionCategoryName);

			var firstQuestion = _buildContext.Questions
				.Where(q => q.QuestionCategoryId == firstQuestionCategory.Id)
				.Single(q => q.Name == firstQuestionName);

			var secondQuestionCategory = _buildContext.QuestionCategories
				.Where(qc => qc.Classroom.Name == classroomName)
				.Single(qc => qc.Name == secondQuestionCategoryName);

			var secondQuestion = _buildContext.Questions
				.Where(q => q.QuestionCategoryId == secondQuestionCategory.Id)
				.Single(q => q.Name == secondQuestionName);

			var prerequisiteQuestion = new PrerequisiteQuestion()
			{
				FirstQuestionId = firstQuestion.Id,
				SecondQuestionId = secondQuestion.Id
			};

			_buildContext.PrerequisiteQuestions.Add(prerequisiteQuestion);
			_buildContext.SaveChanges();

			return this;
		}

		/// <summary>
		/// Adds a classroom admin.
		/// </summary>
		public TestDatabaseBuilder AddAdmin(
			string userName,
			string lastName,
			string firstName,
			string classroomName,
			bool superUser,
			string gitHubLogin = null,
			bool inGitHubOrg = false)
		{
			var classroom = _buildContext.Classrooms
				.Single(c => c.Name == classroomName);

			var user = new User()
			{
				UniqueId = $"{userName}Id",
				UserName = userName,
				FirstName = firstName,
				LastName = lastName,
				SuperUser = superUser,
				GitHubLogin = gitHubLogin,
				ClassroomMemberships = new List<ClassroomMembership>()
				{
					new ClassroomMembership()
					{
						ClassroomId = classroom.Id,
						Role = ClassroomRole.Admin,
						InGitHubOrganization = inGitHubOrg
					}
				}
			};

			_buildContext.Users.Add(user);
			_buildContext.SaveChanges();

			return this;
		}

		/// <summary>
		/// Adds a question category to the database.
		/// </summary>
		public TestDatabaseBuilder AddStudent(
			string userName,
			string lastName,
			string firstName,
			string classroomName,
			string sectionName,
			string gitHubLogin = null,
			bool inGitHubOrg = false)
		{
			var classroom = _buildContext.Classrooms
				.Include(c => c.Sections)
				.Single(c => c.Name == classroomName);

			var section = classroom.Sections
				.SingleOrDefault(s => s.Name == sectionName);

			var user = new User()
			{
				UniqueId = $"{userName}Id",
				UserName = userName,
				FirstName = firstName,
				LastName = lastName,
				EmailAddress = $"{userName}Email",
				EmailConfirmationCode = $"{userName}EmailConfirmationCode",
				GitHubLogin = gitHubLogin,
				ClassroomMemberships = new List<ClassroomMembership>()
				{
					new ClassroomMembership()
					{
						ClassroomId = classroom.Id,
						GitHubTeam = $"{lastName}{firstName}",
						InGitHubOrganization = inGitHubOrg,
						Role = ClassroomRole.General,
						SectionMemberships = 
							section != null
								? new List<SectionMembership>()
									{
										new SectionMembership()
										{
											SectionId = section.Id,
											Role = SectionRole.Student
										}
									}
								: null
					}
				}
			};

			_buildContext.Users.Add(user);
			_buildContext.SaveChanges();

			return this;
		}

		/// <summary>
		/// Adds a question submission to the database.
		/// </summary>
		public TestDatabaseBuilder AddQuestionSubmission(
			string classroomName,
			string questionCategoryName,
			string questionName,
			string userName,
			string assignmentName,
			string submissionContents,
			double score = 0.0,
			DateTime? dateSubmitted = null,
			int? seed = null,
			string cachedQuestionData = null)
		{
			var assignmentQuestion = _buildContext.AssignmentQuestions
				.Where(aq => aq.Assignment.Name == assignmentName)
				.Where(aq => aq.Question.QuestionCategory.Classroom.Name == classroomName)
				.Where(aq => aq.Question.QuestionCategory.Name == questionCategoryName)
				.Single(aq => aq.Question.Name == questionName);

			var user = _buildContext.Users
				.Single(u => u.UserName == userName);

			var userQuestionData = _buildContext.UserQuestionData
				.Where(uqd => uqd.User == user)
				.SingleOrDefault(uqd => uqd.AssignmentQuestion == assignmentQuestion);

			if (userQuestionData == null)
			{
				userQuestionData = new UserQuestionData()
				{
					UserId = user.Id,
					AssignmentQuestionId = assignmentQuestion.Id,
					LastQuestionSubmission = submissionContents,
					CachedQuestionData = cachedQuestionData,
					CachedQuestionDataTime = dateSubmitted,
					Seed = seed
				};

				_buildContext.UserQuestionData.Add(userQuestionData);
			}
			else
			{
				userQuestionData.LastQuestionSubmission = submissionContents;
				userQuestionData.CachedQuestionData = cachedQuestionData;
				userQuestionData.CachedQuestionDataTime = dateSubmitted;
				userQuestionData.Seed = seed;
				
				_buildContext.UserQuestionData.Update(userQuestionData);
			}

			var userQuestionSubmission = new UserQuestionSubmission()
			{
				DateSubmitted = dateSubmitted ?? DateTime.MinValue,
				UserQuestionData = userQuestionData,
				Score = score
			};

			_buildContext.UserQuestionSubmissions.Add(userQuestionSubmission);
			_buildContext.SaveChanges();

			return this;
		}

		/// <summary>
		/// Creates a new assignment with the given data.
		/// </summary>
		public TestDatabaseBuilder AddAssignment(
			string classroomName,
			string assignmentGroupName,
			string assignmentName,
			IDictionary<string, DateTime> sectionDueDates,
			IDictionary<string, string[]> questionsByCategory,
			bool isPrivate = false,
			bool combinedSubmissions = false,
			bool answerInOrder = false)
		{
			var classroom = _buildContext.Classrooms
				.Include(c => c.Sections)
				.Single(c => c.Name == classroomName);

			var sections = classroom.Sections
				.ToDictionary(s => s.Name, s => s.Id);

			var allQuestions = _buildContext.Questions
				.Include(q => q.QuestionCategory)
				.ToList()
				.GroupBy(q => q.QuestionCategory.Name)
				.ToDictionary(group => group.Key, group => group.ToDictionary(q => q.Name, q => q.Id));

			var assignment = new Assignment()
			{
				ClassroomId = classroom.Id,
				Name = assignmentName,
				GroupName = assignmentGroupName,
				IsPrivate = isPrivate,
				CombinedSubmissions = combinedSubmissions,
				AnswerInOrder = answerInOrder,
				DueDates = sectionDueDates?.Select
				(
					kvp => new AssignmentDueDate()
					{
						SectionId = sections[kvp.Key],
						DueDate = kvp.Value
					}
				)?.ToList(),
				Questions = questionsByCategory
					.SelectMany
					(
						qs => qs.Value,
						(kvp, questionName) => new
						{
							Category = kvp.Key,
							Name = questionName
						}
					)
					.Select
					( 
						(q, index) => new AssignmentQuestion()
						{
							QuestionId = allQuestions[q.Category][q.Name],
							Points = 1.0,
							Name = q.Name,
							Order = index
						}
					).ToList()
			};

			_buildContext.Assignments.Add(assignment);
			_buildContext.SaveChanges();

			return this;
		}

		/// <summary>
		/// Adds a classroom gradebook to the database.
		/// </summary>
		public TestDatabaseBuilder AddClassroomGradebook(
			string classroomName,
			string gradebookName)
		{
			var classroom = _buildContext.Classrooms
				.Single(c => c.Name == classroomName);

			var classroomGradebook = new ClassroomGradebook()
			{
				Name = gradebookName,
				ClassroomId = classroom.Id
			};

			_buildContext.ClassroomGradebooks.Add(classroomGradebook);
			_buildContext.SaveChanges();

			return this;
		}

		/// <summary>
		/// Adds a classroom gradebook to the database.
		/// </summary>
		public TestDatabaseBuilder AddSectionGradebook(
			string classroomName,
			string gradebookName,
			string sectionName,
			DateTime lastTransferDate)
		{
			var classroom = _buildContext.Classrooms
				.Include(c => c.Sections)
				.Include(c => c.ClassroomGradebooks)
				.Single(c => c.Name == classroomName);

			var section = classroom.Sections
				.Single(s => s.Name == sectionName);

			var classroomGradebook = classroom.ClassroomGradebooks
				.Single(cg => cg.Name == gradebookName);

			var sectionGradebook = new SectionGradebook()
			{
				ClassroomGradebookId = classroomGradebook.Id,
				SectionId = section.Id,
				LastTransferDate = lastTransferDate
			};

			_buildContext.SectionGradebooks.Add(sectionGradebook);
			_buildContext.SaveChanges();

			return this;
		}

		/// <summary>
		/// Adds a classroom gradebook to the database.
		/// </summary>
		public TestDatabaseBuilder AddGradebook(
			string classroomName,
			string gradebookName,
			string sectionName,
			DateTime lastTransferDate)
		{
			var classroom = _buildContext.Classrooms
				.Single(c => c.Name == classroomName);

			var section = _buildContext.Sections
				.Where(s => s.Classroom.Name == classroomName)
				.Single(s=> s.Name == sectionName);

			var classroomGradebook = new ClassroomGradebook()
			{
				Name = gradebookName,
				ClassroomId = classroom.Id,
				SectionGradebooks = Collections.CreateList
				(
					new SectionGradebook()
					{
						SectionId = section.Id,
						LastTransferDate = lastTransferDate
					}	
				)
			};

			_buildContext.ClassroomGradebooks.Add(classroomGradebook);
			_buildContext.SaveChanges();

			return this;
		}

		/// <summary>
		/// Adds a project to the database.
		/// </summary>
		public TestDatabaseBuilder AddProject(
			string classroomName,
			string projectName,
			bool explicitSubmissions = true)
		{
			var classroom = _buildContext.Classrooms
				.Single(c => c.Name == classroomName);

			var project = new Project()
			{
				Name = projectName,
				ExplicitSubmissionRequired = explicitSubmissions,
				ClassroomId = classroom.Id
			};

			_buildContext.Projects.Add(project);
			_buildContext.SaveChanges();

			return this;
		}

		/// <summary>
		/// Adds a project to the database.
		/// </summary>
		public TestDatabaseBuilder AddProjectTestClass(
			string classroomName,
			string projectName,
			string testClassName)
		{
			var project = _buildContext.Projects
				.Where(p => p.Classroom.Name == classroomName)
				.Single(p => p.Name == projectName);

			_buildContext.TestClasses.Add
			(
				new TestClass()
				{
					ProjectId = project.Id,
					ClassName = testClassName
				}
			);

			_buildContext.SaveChanges();

			return this;
		}

		/// <summary>
		/// Adds a project to the database.
		/// </summary>
		public TestDatabaseBuilder AddProjectPath(
			string classroomName,
			string projectName,
			string path,
			FileType pathType)
		{
			if (pathType == FileType.Public)
			{
				throw new ArgumentOutOfRangeException(nameof(pathType));
			}

			var project = _buildContext.Projects
				.Where(p => p.Classroom.Name == classroomName)
				.Single(p => p.Name == projectName);

			if (pathType == FileType.Immutable)
			{
				_buildContext.ImmutableFilePaths.Add
				(
					new ImmutableFilePath()
					{
						ProjectId = project.Id,
						Path = path
					}
				);
			}
			else if (pathType == FileType.Private)
			{
				_buildContext.PrivateFilePaths.Add
				(
					new PrivateFilePath()
					{
						ProjectId = project.Id,
						Path = path
					}
				);
			}
			else
			{
				throw new ArgumentOutOfRangeException(nameof(pathType));
			}

			_buildContext.SaveChanges();

			return this;
		}

		/// <summary>
		/// Adds a project to the database.
		/// </summary>
		public TestDatabaseBuilder AddCommit(
			string classroomName,
			string projectName,
			string userName,
			string commitSha,
			DateTime pushDate,
			Build build = null,
			string buildJobId = null,
			string buildRequestToken = null)
		{
			var project = _buildContext.Projects
				.Where(p => p.Classroom.Name == classroomName)
				.Single(p => p.Name == projectName);

			var user = _buildContext.Users
				.Single(u => u.UserName == userName);

			_buildContext.Commits.Add
			(
				new Commit()
				{
					ProjectId = project.Id,
					UserId = user.Id,
					PushDate = pushDate,
					Sha = commitSha,
					Build = build,
					BuildJobId = buildJobId,
					BuildRequestToken = buildRequestToken,
				}
			);

			_buildContext.SaveChanges();

			return this;
		}

		/// <summary>
		/// Adds a checkpoint to the database.
		/// </summary>
		public TestDatabaseBuilder AddCheckpoint(
			string classroomName,
			string projectName,
			string checkpointName)
		{
			var project = _buildContext.Projects
				.Where(p => p.Classroom.Name == classroomName)
				.Single(p => p.Name == projectName);

			_buildContext.Checkpoints.Add
			(
				new Checkpoint()
				{
					ProjectId = project.Id,
					Name = checkpointName,
					DisplayName = $"{checkpointName} DisplayName"
				}
			);

			_buildContext.SaveChanges();

			return this;
		}

		/// <summary>
		/// Adds a checkpoint due date to the database.
		/// </summary>
		public TestDatabaseBuilder AddCheckpointDueDate(
			string classroomName,
			string projectName,
			string checkpointName,
			string sectionName,
			DateTime dueDate)
		{
			var checkpoint = _buildContext.Checkpoints
				.Where(c => c.Project.Classroom.Name == classroomName)
				.Where(c => c.Project.Name == projectName)
				.Single(c => c.Name == checkpointName);

			var section = _buildContext.Sections
				.Where(s => s.Classroom.Name == classroomName)
				.Single(s => s.Name == sectionName);

			_buildContext.CheckpointDates.Add
			(
				new CheckpointDates()
				{
					CheckpointId = checkpoint.Id,
					SectionId = section.Id,
					DueDate = dueDate
				}
			);

			_buildContext.SaveChanges();

			return this;
		}

		/// <summary>
		/// Adds a checkpoint test class to the database.
		/// </summary>
		public TestDatabaseBuilder AddCheckpointTestClass(
			string classroomName,
			string projectName,
			string checkpointName,
			string testClassName,
			bool required)
		{
			var checkpoint = _buildContext.Checkpoints
				.Where(c => c.Project.Classroom.Name == classroomName)
				.Where(c => c.Project.Name == projectName)
				.Single(c => c.Name == checkpointName);

			var testClass = _buildContext.TestClasses
				.Where(tc => tc.Project.Classroom.Name == classroomName)
				.Where(tc => tc.Project.Name == projectName)
				.Single(tc => tc.ClassName == testClassName);

			_buildContext.CheckpointTestClasses.Add
			(
				new CheckpointTestClass()
				{
					CheckpointId = checkpoint.Id,
					TestClassId = testClass.Id,
					Required = required
				}
			);

			_buildContext.SaveChanges();

			return this;
		}

		/// <summary>
		/// Adds a submission to the database.
		/// </summary>
		public TestDatabaseBuilder AddSubmission(
			string classroomName,
			string projectName,
			string checkpointName,
			string userName,
			string commitSha,
			DateTime submissionDate,
			int pullRequest = 0,
			string feedback = null,
			bool sentFeedback = false,
			bool readFeedback = false)
		{
			var checkpoint = _buildContext.Checkpoints
				.Where(c => c.Project.Classroom.Name == classroomName)
				.Where(c => c.Project.Name == projectName)
				.Single(c => c.Name == checkpointName);

			var commit = _buildContext.Commits
				.Where(c => c.Project.Classroom.Name == classroomName)
				.Where(c => c.Project.Name == projectName)
				.Where(c => c.User.UserName == userName)
				.Single(c => c.Sha == commitSha);

			_buildContext.Submissions.Add
			(
				new Submission()
				{
					CheckpointId = checkpoint.Id,
					CommitId = commit.Id,
					DateSubmitted = submissionDate,
					PullRequestNumber = pullRequest,
					Feedback = feedback,
					DateFeedbackSaved = feedback != null ? (DateTime?)submissionDate : null,
					FeedbackSent = sentFeedback,
					DateFeedbackRead = readFeedback ? (DateTime?)submissionDate : null
				}
			);

			_buildContext.SaveChanges();

			return this;
		}

		/// <summary>
		/// Creates a new DatabaseContext with the given data.
		/// </summary>
		public TestDatabase Build()
		{
			var dbContext = new DatabaseContext(_store.Options);

			return new TestDatabase(_store, dbContext);
		}
	}
}
