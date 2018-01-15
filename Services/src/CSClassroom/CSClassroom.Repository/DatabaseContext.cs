using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Model.Communications;
using CSC.CSClassroom.Model.Projects;
using CSC.CSClassroom.Model.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace CSC.CSClassroom.Repository
{
	/// <summary>
	/// The database context.
	/// </summary>
	public class DatabaseContext : DbContext
	{
		/// <summary>
		/// The users table.
		/// </summary>
		public DbSet<User> Users { get; set; }

		/// <summary>
		/// The classroom memberships table.
		/// </summary>
		public DbSet<SectionMembership> SectionMemberships { get; set; }

		/// <summary>
		/// The section memberships table.
		/// </summary>
		public DbSet<ClassroomMembership> ClassroomMemberships { get; set; }

		/// <summary>
		/// Additional contacts for a user.
		/// </summary>
		public DbSet<AdditionalContact> AdditionalContacts { get; set; }

		/// <summary>
		/// The groups table.
		/// </summary>
		public DbSet<Classroom> Classrooms { get; set; }

		/// <summary>
		/// The classrooms table.
		/// </summary>
		public DbSet<Section> Sections { get; set; }

		/// <summary>
		/// The question categories table.
		/// </summary>
		public DbSet<QuestionCategory> QuestionCategories { get; set; }

		/// <summary>
		/// The questions table.
		/// </summary>
		public DbSet<Question> Questions { get; set; }

		/// <summary>
		/// Set of multiple choice questions in the questions table.
		/// </summary>
		public DbSet<MultipleChoiceQuestion> MultipleChoiceQuestions { get; set; }

		/// <summary>
		/// Multiple choice question options.
		/// </summary>
		public DbSet<MultipleChoiceQuestionChoice> MultipleChoiceQuestionChoices { get; set; }

		/// <summary>
		/// Set of short answer questions in the questions table.
		/// </summary>
		public DbSet<ShortAnswerQuestion> ShortAnswerQuestions { get; set; }

		/// <summary>
		/// Short answer question blanks.
		/// </summary>
		public DbSet<ShortAnswerQuestionBlank> ShortAnswerQuestionBlanks { get; set; }

		/// <summary>
		/// Set of randomly selected questions in the questions table.
		/// </summary>
		public DbSet<RandomlySelectedQuestion> RandomlySelectedQuestions { get; set; }

		/// <summary>
		/// Set of generated questions in the questions table.
		/// </summary>
		public DbSet<GeneratedQuestionTemplate> GeneratedQuestions { get; set; }

		/// <summary>
		/// Set of code questions in the questions table.
		/// Note: The name begins with an A, due to the following bug:
		/// https://github.com/aspnet/EntityFramework/issues/5547
		/// </summary>
		public DbSet<CodeQuestion> ACodeQuestions { get; set; }

		/// <summary>
		/// Imported classes.
		/// </summary>
		public DbSet<ImportedClass> ImportedClasses { get; set; }

		/// <summary>
		/// Code constraints.
		/// </summary>
		public DbSet<CodeConstraint> CodeConstraints { get; set; }

		/// <summary>
		/// Set of class questions in the questions table.
		/// </summary>
		public DbSet<ClassQuestion> ClassQuestions { get; set; }

		/// <summary>
		/// Set of method questions in the questions table.
		/// </summary>
		public DbSet<MethodQuestion> MethodQuestions { get; set; }

		/// <summary>
		/// Set of prograqm questions in the questions table.
		/// </summary>
		public DbSet<ProgramQuestion> ProgramQuestions { get; set; }

		/// <summary>
		/// The code question tests table.
		/// </summary>
		public DbSet<CodeQuestionTest> CodeQuestionTests { get; set; }

		/// <summary>
		/// The method question tests in the code question tests table.
		/// </summary>
		public DbSet<MethodQuestionTest> MethodQuestionTests { get; set; }

		/// <summary>
		/// The class question tests in the code question tests table.
		/// </summary>
		public DbSet<ClassQuestionTest> ClassQuestionTests { get; set; }

		/// <summary>
		/// The program question tests in the code question tests table.
		/// </summary>
		public DbSet<ProgramQuestionTest> ProgramQuestionTests { get; set; }

		/// <summary>
		/// Required methods for a class test.
		/// </summary>
		public DbSet<RequiredMethod> RequiredMethods { get; set; }

		/// <summary>
		/// User question submissions.
		/// </summary>
		public DbSet<UserQuestionData> UserQuestionData { get; set; }

		/// <summary>
		/// User question submissions.
		/// </summary>
		public DbSet<UserQuestionSubmission> UserQuestionSubmissions { get; set; }

		/// <summary>
		/// Prerequisite questions.
		/// </summary>
		public DbSet<PrerequisiteQuestion> PrerequisiteQuestions { get; set; }

		/// <summary>
		/// The projects table.
		/// </summary>
		public DbSet<Project> Projects { get; set; }

		/// <summary>
		/// The private file paths table.
		/// </summary>
		public DbSet<PrivateFilePath> PrivateFilePaths { get; set; }

		/// <summary>
		/// The immutable file paths table.
		/// </summary>
		public DbSet<ImmutableFilePath> ImmutableFilePaths { get; set; }

		/// <summary>
		/// The test classes table.
		/// </summary>
		public DbSet<TestClass> TestClasses { get; set; }

		/// <summary>
		/// The commits table.
		/// </summary>
		public DbSet<Commit> Commits { get; set; }

		/// <summary>
		/// The builds table.
		/// </summary>
		public DbSet<Build> Builds { get; set; }

		/// <summary>
		/// The test results table.
		/// </summary>
		public DbSet<TestResult> TestResults { get; set; }

		/// <summary>
		/// The checkpoints table.
		/// </summary>
		public DbSet<Checkpoint> Checkpoints { get; set; }

		/// <summary>
		/// The checkpoint test classes table.
		/// </summary>
		public DbSet<CheckpointTestClass> CheckpointTestClasses { get; set; }

		/// <summary>
		/// The submissions table.
		/// </summary>
		public DbSet<Submission> Submissions { get; set; }

		/// <summary>
		/// The assignments table.
		/// </summary>
		public DbSet<Assignment> Assignments { get; set; }

		/// <summary>
		/// The assignment questions table.
		/// </summary>
		public DbSet<AssignmentQuestion> AssignmentQuestions { get; set; }

		/// <summary>
		/// The assignments table.
		/// </summary>
		public DbSet<AssignmentDueDate> AssignmentDueDates { get; set; }

		/// <summary>
		/// The checkpoint dates table.
		/// </summary>
		public DbSet<CheckpointDates> CheckpointDates { get; set; }

		/// <summary>
		/// The classroom gradebooks table.
		/// </summary>
		public DbSet<ClassroomGradebook> ClassroomGradebooks { get; set; }

		/// <summary>
		/// The section gradebooks status table.
		/// </summary>
		public DbSet<SectionGradebook> SectionGradebooks { get; set; }

		/// <summary>
		/// The announcements table.
		/// </summary>
		public DbSet<Announcement> Announcements { get; set; }

		/// <summary>
		/// The announcement sections table.
		/// </summary>
		public DbSet<AnnouncementSection> AnnouncementSections { get; set; }
		
		/// <summary>
		/// The section recipients table.
		/// </summary>
		public DbSet<SectionRecipient> SectionRecipients { get; set; }
		
		/// <summary>
		/// The conversations table.
		/// </summary>
		public DbSet<Conversation> Conversations { get; set; }
		
		/// <summary>
		/// The messages table.
		/// </summary>
		public DbSet<Message> Messages { get; set; }
		
		/// <summary>
		/// The attachments table.
		/// </summary>
		public DbSet<Attachment> Attachments { get; set; }
		
		/// <summary>
		/// The table containing all attachment data.
		/// </summary>
		public DbSet<AttachmentData> AttachmentData { get; set; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public DatabaseContext(DbContextOptions<DatabaseContext> options)
			: base(options)
		{
		}

		/// <summary>
		/// Called when the model is being created.
		/// </summary>
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			SetupCustomTableNames(modelBuilder);
			SetupIndices(modelBuilder);
			SetupCustomRelationships(modelBuilder);
			SetupDefaultValues(modelBuilder);
		}

		/// <summary>
		/// Sets up tables with custom names.
		/// </summary>
		private static void SetupCustomTableNames(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Build>().ToTable("Build");
			modelBuilder.Entity<UserQuestionSubmission>().ToTable("UserQuestionSubmission");
		}

		private static void SetupIndices(ModelBuilder builder)
		{
			SetupIndex<User>(builder, u => u.UniqueId);
			SetupIndex<Classroom>(builder, c => c.Name);
			SetupIndex<Section>(builder, s => new {s.ClassroomId, s.Name});
			SetupIndex<ClassroomMembership>(builder, cm => new {cm.UserId, cm.ClassroomId});
			SetupIndex<SectionMembership>(builder, sm => new {sm.ClassroomMembershipId, sm.SectionId});
			SetupIndex<AdditionalContact>(builder, ac => new {ac.UserId, ac.EmailAddress});
			SetupIndex<QuestionCategory>(builder, qc => new {qc.ClassroomId, qc.Name, qc.RandomlySelectedQuestionId});
			SetupIndex<Question>(builder, q => new {q.QuestionCategoryId, q.Name});
			SetupIndex<UserQuestionData>(builder, uqd => new {uqd.AssignmentQuestionId, uqd.UserId});
			SetupIndex<Project>(builder, p => new {p.ClassroomId, p.Name});
			SetupIndex<Checkpoint>(builder, c => new {c.ProjectId, c.Name});
			SetupIndex<Commit>(builder, c => new {c.ProjectId, c.UserId, c.Sha});
			SetupIndex<Build>(builder, b => new {b.CommitId});
			SetupIndex<TestResult>(builder, tr => new {tr.BuildId, tr.ClassName, tr.TestName});
			SetupIndex<Assignment>(builder, a => new {a.ClassroomId, a.Name});
			SetupIndex<AssignmentQuestion>(builder, aq => new {aq.AssignmentId, aq.Name});
			SetupIndex<AssignmentDueDate>(builder, aq => new {aq.AssignmentId, aq.SectionId});
			SetupIndex<Submission>(builder, s => new {s.CheckpointId, s.CommitId, s.DateSubmitted});
			SetupIndex<CheckpointDates>(builder, cd => new {cd.CheckpointId, cd.SectionId});
			SetupIndex<ClassroomGradebook>(builder, g => new {g.ClassroomId, g.Name});
			SetupIndex<SectionGradebook>(builder, g => new {g.ClassroomGradebookId, g.SectionId});
			SetupIndex<CheckpointTestClass>(builder, tc => new {tc.CheckpointId, tc.TestClassId});
			SetupIndex<Announcement>(builder, a => a.DatePosted);
			SetupIndex<AnnouncementSection>(builder, a => new {a.AnnouncementId, a.SectionId});
			SetupIndex<SectionRecipient>(builder, sr => new {sr.ClassroomMembershipId, sr.SectionId});
			SetupIndex<Message>(builder, m => new {m.ConversationId, m.AuthorId}, "Sent IS NULL");
		}

		/// <summary>
		/// Sets up a unique index.
		/// </summary>
		private static void SetupIndex<TEntity>(
			ModelBuilder modelBuilder, 
			Expression<Func<TEntity, object>> expression,
			string filter = null) where TEntity : class
		{
			var builder = modelBuilder.Entity<TEntity>().HasIndex(expression);

			if (filter != null)
			{
				builder.HasFilter(filter);
			}
			
			builder.IsUnique();
		}

		/// <summary>
		/// Sets up custom relationships.
		/// </summary>
		private static void SetupCustomRelationships(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Commit>()
				.HasOne(c => c.Build)
				.WithOne(b => b.Commit)
				.HasForeignKey<Build>(b => b.CommitId);

			modelBuilder.Entity<RandomlySelectedQuestion>()
				.HasOne(rsq => rsq.ChoicesCategory)
				.WithOne(qc => qc.RandomlySelectedQuestion)
				.HasForeignKey<QuestionCategory>(qc => qc.RandomlySelectedQuestionId)
				.IsRequired(false)
				.OnDelete(DeleteBehavior.Cascade);
		}

		/// <summary>
		/// Sets up default values.
		/// </summary>
		private static void SetupDefaultValues(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<ClassQuestion>()
				.Property(c => c.ClassSubmissionType)
				.HasDefaultValue(ClassSubmissionType.Custom);
		}

		/// <summary>
		/// Removes all objects that were previously in a collection,
		/// but no longer should be.
		/// </summary>
		/// <typeparam name="TItemType">The item type.</typeparam>
		/// <param name="dbSet">The database set containing objects of the item type.</param>
		/// <param name="getId">A function that returns the ID of an item.</param>
		/// <param name="oldItemsQuery">A query that gets all existing objects in the collection.</param>
		/// <param name="itemsToKeep">The items to keep. All others will be reemovd.</param>
		public void RemoveUnwantedObjects<TItemType>(
			DbSet<TItemType> dbSet,
			Func<TItemType, int> getId,
			Expression<Func<TItemType, bool>> oldItemsQuery,
			ICollection<TItemType> itemsToKeep) where TItemType : class
		{
			foreach (var oldItem in dbSet.Where(oldItemsQuery))
			{
				if (itemsToKeep == null || !itemsToKeep.Any(itemToKeep => getId(itemToKeep) == getId(oldItem)))
				{
					dbSet.Remove(oldItem);
				}
				else
				{
					Entry(oldItem).State = EntityState.Detached;
				}
			}
		}
	}
}
