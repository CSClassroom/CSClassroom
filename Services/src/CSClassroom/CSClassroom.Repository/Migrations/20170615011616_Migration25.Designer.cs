using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using CSC.CSClassroom.Repository;
using CSC.CSClassroom.Model.Projects;
using CSC.CSClassroom.Model.Questions;
using CSC.CSClassroom.Model.Users;

namespace CSC.CSClassroom.Repository.Migrations
{
	[DbContext(typeof(DatabaseContext))]
	[Migration("20170615011616_Migration25")]
	partial class Migration25
	{
		protected override void BuildTargetModel(ModelBuilder modelBuilder)
		{
			modelBuilder
				.HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
				.HasAnnotation("ProductVersion", "1.1.2");

			modelBuilder.Entity("CSC.CSClassroom.Model.Classrooms.Classroom", b =>
				{
					b.Property<int>("Id")
						.ValueGeneratedOnAdd();

					b.Property<TimeSpan>("DefaultTimeDue");

					b.Property<string>("DisplayName")
						.IsRequired();

					b.Property<string>("GitHubOrganization")
						.IsRequired();

					b.Property<string>("Name")
						.IsRequired()
						.HasMaxLength(50);

					b.HasKey("Id");

					b.HasIndex("Name")
						.IsUnique();

					b.ToTable("Classrooms");
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Classrooms.Section", b =>
				{
					b.Property<int>("Id")
						.ValueGeneratedOnAdd();

					b.Property<bool>("AllowNewRegistrations");

					b.Property<int>("ClassroomId");

					b.Property<string>("DisplayName")
						.IsRequired();

					b.Property<string>("Name")
						.IsRequired()
						.HasMaxLength(50);

					b.HasKey("Id");

					b.HasIndex("ClassroomId", "Name")
						.IsUnique();

					b.ToTable("Sections");
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Projects.Build", b =>
				{
					b.Property<int>("Id")
						.ValueGeneratedOnAdd();

					b.Property<int>("CommitId");

					b.Property<DateTime>("DateCompleted");

					b.Property<DateTime>("DateStarted");

					b.Property<string>("Output");

					b.Property<int>("Status");

					b.HasKey("Id");

					b.HasIndex("CommitId")
						.IsUnique();

					b.ToTable("Build");
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Projects.Checkpoint", b =>
				{
					b.Property<int>("Id")
						.ValueGeneratedOnAdd();

					b.Property<string>("DisplayName");

					b.Property<string>("Name")
						.HasMaxLength(50);

					b.Property<int>("ProjectId");

					b.HasKey("Id");

					b.HasIndex("ProjectId", "Name")
						.IsUnique();

					b.ToTable("Checkpoints");
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Projects.CheckpointDates", b =>
				{
					b.Property<int>("Id")
						.ValueGeneratedOnAdd();

					b.Property<int>("CheckpointId");

					b.Property<DateTime>("DueDate");

					b.Property<int>("SectionId");

					b.Property<DateTime?>("StartDate");

					b.HasKey("Id");

					b.HasIndex("SectionId");

					b.HasIndex("CheckpointId", "SectionId")
						.IsUnique();

					b.ToTable("CheckpointDates");
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Projects.CheckpointTestClass", b =>
				{
					b.Property<int>("Id")
						.ValueGeneratedOnAdd();

					b.Property<int>("CheckpointId");

					b.Property<bool>("Required");

					b.Property<int>("TestClassId");

					b.HasKey("Id");

					b.HasIndex("TestClassId");

					b.HasIndex("CheckpointId", "TestClassId")
						.IsUnique();

					b.ToTable("CheckpointTestClasses");
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Projects.Commit", b =>
				{
					b.Property<int>("Id")
						.ValueGeneratedOnAdd();

					b.Property<string>("BuildJobId");

					b.Property<string>("BuildRequestToken");

					b.Property<DateTime>("CommitDate");

					b.Property<string>("Message");

					b.Property<int>("ProjectId");

					b.Property<DateTime>("PushDate");

					b.Property<string>("Sha");

					b.Property<int>("UserId");

					b.HasKey("Id");

					b.HasIndex("UserId");

					b.HasIndex("ProjectId", "UserId", "Sha")
						.IsUnique();

					b.ToTable("Commits");
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Projects.ImmutableFilePath", b =>
				{
					b.Property<int>("Id")
						.ValueGeneratedOnAdd();

					b.Property<string>("Path");

					b.Property<int>("ProjectId");

					b.HasKey("Id");

					b.HasIndex("ProjectId");

					b.ToTable("ImmutableFilePaths");
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Projects.PrivateFilePath", b =>
				{
					b.Property<int>("Id")
						.ValueGeneratedOnAdd();

					b.Property<string>("Path");

					b.Property<int>("ProjectId");

					b.HasKey("Id");

					b.HasIndex("ProjectId");

					b.ToTable("PrivateFilePaths");
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Projects.Project", b =>
				{
					b.Property<int>("Id")
						.ValueGeneratedOnAdd();

					b.Property<bool>("BuildCommits");

					b.Property<int>("ClassroomId");

					b.Property<bool>("ExplicitSubmissionRequired");

					b.Property<string>("Name")
						.IsRequired()
						.HasMaxLength(50);

					b.HasKey("Id");

					b.HasIndex("ClassroomId", "Name")
						.IsUnique();

					b.ToTable("Projects");
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Projects.Submission", b =>
				{
					b.Property<int>("Id")
						.ValueGeneratedOnAdd();

					b.Property<int>("CheckpointId");

					b.Property<int>("CommitId");

					b.Property<DateTime?>("DateFeedbackRead");

					b.Property<DateTime?>("DateFeedbackSaved");

					b.Property<DateTime>("DateSubmitted");

					b.Property<string>("Feedback");

					b.Property<bool>("FeedbackSent");

					b.Property<int>("PullRequestNumber");

					b.HasKey("Id");

					b.HasIndex("CommitId");

					b.HasIndex("CheckpointId", "CommitId", "DateSubmitted")
						.IsUnique();

					b.ToTable("Submissions");
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Projects.TestClass", b =>
				{
					b.Property<int>("Id")
						.ValueGeneratedOnAdd();

					b.Property<string>("ClassName");

					b.Property<string>("DisplayName");

					b.Property<int>("Order");

					b.Property<int>("ProjectId");

					b.HasKey("Id");

					b.HasIndex("ProjectId");

					b.ToTable("TestClasses");
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Projects.TestResult", b =>
				{
					b.Property<int>("Id")
						.ValueGeneratedOnAdd();

					b.Property<int>("BuildId");

					b.Property<string>("ClassName");

					b.Property<string>("FailureMessage");

					b.Property<string>("FailureOutput");

					b.Property<string>("FailureTrace");

					b.Property<bool>("PreviouslySucceeded");

					b.Property<bool>("Succeeded");

					b.Property<string>("TestName");

					b.HasKey("Id");

					b.HasIndex("BuildId", "ClassName", "TestName")
						.IsUnique();

					b.ToTable("TestResults");
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Questions.Assignment", b =>
				{
					b.Property<int>("Id")
						.ValueGeneratedOnAdd();

					b.Property<bool>("AnswerInOrder");

					b.Property<int>("ClassroomId");

					b.Property<bool>("CombinedSubmissions");

					b.Property<string>("GroupName")
						.HasMaxLength(100);

					b.Property<bool>("IsPrivate");

					b.Property<int?>("MaxAttempts");

					b.Property<string>("Name")
						.IsRequired()
						.HasMaxLength(100);

					b.Property<bool>("OnlyShowCombinedScore");

					b.HasKey("Id");

					b.HasIndex("ClassroomId", "Name")
						.IsUnique();

					b.ToTable("Assignments");
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Questions.AssignmentDueDate", b =>
				{
					b.Property<int>("Id")
						.ValueGeneratedOnAdd();

					b.Property<int>("AssignmentId");

					b.Property<DateTime>("DueDate");

					b.Property<int>("SectionId");

					b.HasKey("Id");

					b.HasIndex("SectionId");

					b.HasIndex("AssignmentId", "SectionId")
						.IsUnique();

					b.ToTable("AssignmentDueDates");
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Questions.AssignmentQuestion", b =>
				{
					b.Property<int>("Id")
						.ValueGeneratedOnAdd();

					b.Property<int>("AssignmentId");

					b.Property<string>("Name");

					b.Property<int>("Order");

					b.Property<double>("Points");

					b.Property<int>("QuestionId");

					b.HasKey("Id");

					b.HasIndex("QuestionId");

					b.HasIndex("AssignmentId", "Name")
						.IsUnique();

					b.ToTable("AssignmentQuestions");
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Questions.ClassroomGradebook", b =>
				{
					b.Property<int>("Id")
						.ValueGeneratedOnAdd();

					b.Property<int>("ClassroomId");

					b.Property<string>("Name")
						.IsRequired();

					b.Property<int>("Order");

					b.HasKey("Id");

					b.HasIndex("ClassroomId", "Name")
						.IsUnique();

					b.ToTable("ClassroomGradebooks");
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Questions.CodeConstraint", b =>
				{
					b.Property<int>("Id")
						.ValueGeneratedOnAdd();

					b.Property<int>("CodeQuestionId");

					b.Property<string>("ErrorMessage")
						.IsRequired();

					b.Property<int>("Frequency");

					b.Property<int>("Order");

					b.Property<string>("Regex")
						.IsRequired();

					b.Property<int>("Type");

					b.HasKey("Id");

					b.HasIndex("CodeQuestionId");

					b.ToTable("CodeConstraints");
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Questions.CodeQuestionTest", b =>
				{
					b.Property<int>("Id")
						.ValueGeneratedOnAdd();

					b.Property<string>("Discriminator")
						.IsRequired();

					b.Property<string>("ExpectedOutput");

					b.Property<string>("ExpectedReturnValue");

					b.Property<string>("Name");

					b.Property<int>("Order");

					b.HasKey("Id");

					b.ToTable("CodeQuestionTests");

					b.HasDiscriminator<string>("Discriminator").HasValue("CodeQuestionTest");
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Questions.ImportedClass", b =>
				{
					b.Property<int>("Id")
						.ValueGeneratedOnAdd();

					b.Property<string>("ClassName")
						.IsRequired();

					b.Property<int>("CodeQuestionId");

					b.HasKey("Id");

					b.HasIndex("CodeQuestionId");

					b.ToTable("ImportedClasses");
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Questions.MultipleChoiceQuestionChoice", b =>
				{
					b.Property<int>("Id")
						.ValueGeneratedOnAdd();

					b.Property<bool>("Correct");

					b.Property<string>("Explanation");

					b.Property<int>("MultipleChoiceQuestionId");

					b.Property<int>("Order");

					b.Property<string>("Value");

					b.HasKey("Id");

					b.HasIndex("MultipleChoiceQuestionId");

					b.ToTable("MultipleChoiceQuestionChoices");
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Questions.PrerequisiteQuestion", b =>
				{
					b.Property<int>("Id")
						.ValueGeneratedOnAdd();

					b.Property<int>("FirstQuestionId");

					b.Property<int>("Order");

					b.Property<int>("SecondQuestionId");

					b.HasKey("Id");

					b.HasIndex("FirstQuestionId");

					b.HasIndex("SecondQuestionId");

					b.ToTable("PrerequisiteQuestions");
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Questions.Question", b =>
				{
					b.Property<int>("Id")
						.ValueGeneratedOnAdd();

					b.Property<bool>("AllowPartialCredit");

					b.Property<string>("Description")
						.IsRequired();

					b.Property<string>("Discriminator")
						.IsRequired();

					b.Property<string>("Name")
						.IsRequired()
						.HasMaxLength(50);

					b.Property<int>("QuestionCategoryId");

					b.HasKey("Id");

					b.HasIndex("QuestionCategoryId", "Name")
						.IsUnique();

					b.ToTable("Questions");

					b.HasDiscriminator<string>("Discriminator").HasValue("Question");
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Questions.QuestionCategory", b =>
				{
					b.Property<int>("Id")
						.ValueGeneratedOnAdd();

					b.Property<int>("ClassroomId");

					b.Property<string>("Name")
						.IsRequired()
						.HasMaxLength(100);

					b.Property<int?>("RandomlySelectedQuestionId");

					b.HasKey("Id");

					b.HasIndex("RandomlySelectedQuestionId")
						.IsUnique();

					b.HasIndex("ClassroomId", "Name", "RandomlySelectedQuestionId")
						.IsUnique();

					b.ToTable("QuestionCategories");
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Questions.RequiredMethod", b =>
				{
					b.Property<int>("Id")
						.ValueGeneratedOnAdd();

					b.Property<int>("ClassQuestionId");

					b.Property<bool>("IsPublic");

					b.Property<bool>("IsStatic");

					b.Property<string>("Name");

					b.Property<string>("ParamTypes");

					b.Property<string>("ReturnType");

					b.HasKey("Id");

					b.HasIndex("ClassQuestionId");

					b.ToTable("RequiredMethods");
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Questions.SectionGradebook", b =>
				{
					b.Property<int>("Id")
						.ValueGeneratedOnAdd();

					b.Property<int>("ClassroomGradebookId");

					b.Property<DateTime>("LastTransferDate");

					b.Property<int>("SectionId");

					b.HasKey("Id");

					b.HasIndex("SectionId");

					b.HasIndex("ClassroomGradebookId", "SectionId")
						.IsUnique();

					b.ToTable("SectionGradebooks");
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Questions.ShortAnswerQuestionBlank", b =>
				{
					b.Property<int>("Id")
						.ValueGeneratedOnAdd();

					b.Property<string>("Answer");

					b.Property<string>("Name");

					b.Property<int>("Order");

					b.Property<bool>("Regex");

					b.Property<int>("ShortAnswerQuestionId");

					b.HasKey("Id");

					b.HasIndex("ShortAnswerQuestionId");

					b.ToTable("ShortAnswerQuestionBlanks");
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Questions.UserQuestionData", b =>
				{
					b.Property<int>("Id")
						.ValueGeneratedOnAdd();

					b.Property<int>("AssignmentQuestionId");

					b.Property<string>("CachedQuestionData");

					b.Property<DateTime?>("CachedQuestionDataTime");

					b.Property<string>("LastQuestionSubmission");

					b.Property<int>("NumAttempts");

					b.Property<int?>("Seed");

					b.Property<int>("UserId");

					b.HasKey("Id");

					b.HasIndex("UserId");

					b.HasIndex("AssignmentQuestionId", "UserId")
						.IsUnique();

					b.ToTable("UserQuestionData");
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Questions.UserQuestionSubmission", b =>
				{
					b.Property<int>("Id")
						.ValueGeneratedOnAdd();

					b.Property<string>("CachedQuestionData");

					b.Property<DateTime>("DateSubmitted");

					b.Property<double>("Score");

					b.Property<int?>("Seed");

					b.Property<string>("SubmissionContents");

					b.Property<int>("UserQuestionDataId");

					b.HasKey("Id");

					b.HasIndex("UserQuestionDataId");

					b.ToTable("UserQuestionSubmission");
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Users.ClassroomMembership", b =>
				{
					b.Property<int>("Id")
						.ValueGeneratedOnAdd();

					b.Property<int>("ClassroomId");

					b.Property<string>("GitHubTeam");

					b.Property<bool>("InGitHubOrganization");

					b.Property<int>("Role");

					b.Property<int>("UserId");

					b.HasKey("Id");

					b.HasIndex("ClassroomId");

					b.HasIndex("UserId", "ClassroomId")
						.IsUnique();

					b.ToTable("ClassroomMemberships");
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Users.SectionMembership", b =>
				{
					b.Property<int>("Id")
						.ValueGeneratedOnAdd();

					b.Property<int>("ClassroomMembershipId");

					b.Property<int>("Role");

					b.Property<int>("SectionId");

					b.HasKey("Id");

					b.HasIndex("SectionId");

					b.HasIndex("ClassroomMembershipId", "SectionId")
						.IsUnique();

					b.ToTable("SectionMemberships");
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Users.User", b =>
				{
					b.Property<int>("Id")
						.ValueGeneratedOnAdd();

					b.Property<string>("EmailAddress");

					b.Property<bool>("EmailAddressConfirmed");

					b.Property<string>("EmailConfirmationCode");

					b.Property<string>("FirstName");

					b.Property<string>("GitHubLogin");

					b.Property<string>("LastName");

					b.Property<bool>("SuperUser");

					b.Property<string>("UniqueId");

					b.Property<string>("UserName");

					b.HasKey("Id");

					b.HasIndex("UniqueId")
						.IsUnique();

					b.ToTable("Users");
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Questions.ClassQuestionTest", b =>
				{
					b.HasBaseType("CSC.CSClassroom.Model.Questions.CodeQuestionTest");

					b.Property<int>("ClassQuestionId");

					b.Property<string>("Description")
						.IsRequired();

					b.Property<string>("MethodBody");

					b.Property<string>("ReturnType")
						.IsRequired();

					b.HasIndex("ClassQuestionId");

					b.ToTable("ClassQuestionTest");

					b.HasDiscriminator().HasValue("ClassQuestionTest");
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Questions.MethodQuestionTest", b =>
				{
					b.HasBaseType("CSC.CSClassroom.Model.Questions.CodeQuestionTest");

					b.Property<int>("MethodQuestionId");

					b.Property<string>("ParameterValues");

					b.HasIndex("MethodQuestionId");

					b.ToTable("MethodQuestionTest");

					b.HasDiscriminator().HasValue("MethodQuestionTest");
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Questions.ProgramQuestionTest", b =>
				{
					b.HasBaseType("CSC.CSClassroom.Model.Questions.CodeQuestionTest");

					b.Property<string>("CommandLineArguments");

					b.Property<int>("ProgramQuestionId");

					b.Property<string>("TestDescription")
						.IsRequired();

					b.HasIndex("ProgramQuestionId");

					b.ToTable("ProgramQuestionTest");

					b.HasDiscriminator().HasValue("ProgramQuestionTest");
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Questions.CodeQuestion", b =>
				{
					b.HasBaseType("CSC.CSClassroom.Model.Questions.Question");

					b.Property<string>("InitialSubmission");

					b.ToTable("CodeQuestion");

					b.HasDiscriminator().HasValue("CodeQuestion");
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Questions.MultipleChoiceQuestion", b =>
				{
					b.HasBaseType("CSC.CSClassroom.Model.Questions.Question");

					b.Property<bool>("AllowMultipleCorrectAnswers");

					b.ToTable("MultipleChoiceQuestion");

					b.HasDiscriminator().HasValue("MultipleChoiceQuestion");
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Questions.RandomlySelectedQuestion", b =>
				{
					b.HasBaseType("CSC.CSClassroom.Model.Questions.Question");


					b.ToTable("RandomlySelectedQuestion");

					b.HasDiscriminator().HasValue("RandomlySelectedQuestion");
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Questions.ShortAnswerQuestion", b =>
				{
					b.HasBaseType("CSC.CSClassroom.Model.Questions.Question");

					b.Property<string>("Explanation");

					b.ToTable("ShortAnswerQuestion");

					b.HasDiscriminator().HasValue("ShortAnswerQuestion");
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Questions.ClassQuestion", b =>
				{
					b.HasBaseType("CSC.CSClassroom.Model.Questions.CodeQuestion");

					b.Property<bool>("AllowPublicFields");

					b.Property<string>("ClassName")
						.IsRequired();

					b.Property<int>("ClassSubmissionType")
						.ValueGeneratedOnAdd()
						.HasDefaultValue(0);

					b.Property<string>("FileTemplate")
						.IsRequired();

					b.ToTable("ClassQuestion");

					b.HasDiscriminator().HasValue("ClassQuestion");
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Questions.GeneratedQuestionTemplate", b =>
				{
					b.HasBaseType("CSC.CSClassroom.Model.Questions.CodeQuestion");

					b.Property<DateTime>("DateModified");

					b.Property<string>("FullGeneratorFileContents");

					b.Property<int>("FullGeneratorFileLineOffset");

					b.Property<string>("GeneratorContents");

					b.Property<int?>("NumSeeds");

					b.ToTable("GeneratedQuestionTemplate");

					b.HasDiscriminator().HasValue("GeneratedQuestionTemplate");
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Questions.MethodQuestion", b =>
				{
					b.HasBaseType("CSC.CSClassroom.Model.Questions.CodeQuestion");

					b.Property<string>("MethodName")
						.IsRequired();

					b.Property<string>("ParameterTypes")
						.IsRequired();

					b.Property<string>("ReturnType")
						.IsRequired();

					b.ToTable("MethodQuestion");

					b.HasDiscriminator().HasValue("MethodQuestion");
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Questions.ProgramQuestion", b =>
				{
					b.HasBaseType("CSC.CSClassroom.Model.Questions.CodeQuestion");

					b.Property<string>("ProgramClassName")
						.IsRequired();

					b.ToTable("ProgramQuestion");

					b.HasDiscriminator().HasValue("ProgramQuestion");
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Classrooms.Section", b =>
				{
					b.HasOne("CSC.CSClassroom.Model.Classrooms.Classroom", "Classroom")
						.WithMany("Sections")
						.HasForeignKey("ClassroomId")
						.OnDelete(DeleteBehavior.Cascade);
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Projects.Build", b =>
				{
					b.HasOne("CSC.CSClassroom.Model.Projects.Commit", "Commit")
						.WithOne("Build")
						.HasForeignKey("CSC.CSClassroom.Model.Projects.Build", "CommitId")
						.OnDelete(DeleteBehavior.Cascade);
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Projects.Checkpoint", b =>
				{
					b.HasOne("CSC.CSClassroom.Model.Projects.Project", "Project")
						.WithMany("Checkpoints")
						.HasForeignKey("ProjectId")
						.OnDelete(DeleteBehavior.Cascade);
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Projects.CheckpointDates", b =>
				{
					b.HasOne("CSC.CSClassroom.Model.Projects.Checkpoint", "Checkpoint")
						.WithMany("SectionDates")
						.HasForeignKey("CheckpointId")
						.OnDelete(DeleteBehavior.Cascade);

					b.HasOne("CSC.CSClassroom.Model.Classrooms.Section", "Section")
						.WithMany()
						.HasForeignKey("SectionId")
						.OnDelete(DeleteBehavior.Cascade);
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Projects.CheckpointTestClass", b =>
				{
					b.HasOne("CSC.CSClassroom.Model.Projects.Checkpoint", "Checkpoint")
						.WithMany("TestClasses")
						.HasForeignKey("CheckpointId")
						.OnDelete(DeleteBehavior.Cascade);

					b.HasOne("CSC.CSClassroom.Model.Projects.TestClass", "TestClass")
						.WithMany("CheckpointTestClasses")
						.HasForeignKey("TestClassId")
						.OnDelete(DeleteBehavior.Cascade);
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Projects.Commit", b =>
				{
					b.HasOne("CSC.CSClassroom.Model.Projects.Project", "Project")
						.WithMany()
						.HasForeignKey("ProjectId")
						.OnDelete(DeleteBehavior.Cascade);

					b.HasOne("CSC.CSClassroom.Model.Users.User", "User")
						.WithMany("Commits")
						.HasForeignKey("UserId")
						.OnDelete(DeleteBehavior.Cascade);
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Projects.ImmutableFilePath", b =>
				{
					b.HasOne("CSC.CSClassroom.Model.Projects.Project")
						.WithMany("ImmutableFilePaths")
						.HasForeignKey("ProjectId")
						.OnDelete(DeleteBehavior.Cascade);
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Projects.PrivateFilePath", b =>
				{
					b.HasOne("CSC.CSClassroom.Model.Projects.Project")
						.WithMany("PrivateFilePaths")
						.HasForeignKey("ProjectId")
						.OnDelete(DeleteBehavior.Cascade);
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Projects.Project", b =>
				{
					b.HasOne("CSC.CSClassroom.Model.Classrooms.Classroom", "Classroom")
						.WithMany()
						.HasForeignKey("ClassroomId")
						.OnDelete(DeleteBehavior.Cascade);
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Projects.Submission", b =>
				{
					b.HasOne("CSC.CSClassroom.Model.Projects.Checkpoint", "Checkpoint")
						.WithMany("Submissions")
						.HasForeignKey("CheckpointId")
						.OnDelete(DeleteBehavior.Cascade);

					b.HasOne("CSC.CSClassroom.Model.Projects.Commit", "Commit")
						.WithMany("Submissions")
						.HasForeignKey("CommitId")
						.OnDelete(DeleteBehavior.Cascade);
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Projects.TestClass", b =>
				{
					b.HasOne("CSC.CSClassroom.Model.Projects.Project", "Project")
						.WithMany("TestClasses")
						.HasForeignKey("ProjectId")
						.OnDelete(DeleteBehavior.Cascade);
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Projects.TestResult", b =>
				{
					b.HasOne("CSC.CSClassroom.Model.Projects.Build", "Build")
						.WithMany("TestResults")
						.HasForeignKey("BuildId")
						.OnDelete(DeleteBehavior.Cascade);
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Questions.Assignment", b =>
				{
					b.HasOne("CSC.CSClassroom.Model.Classrooms.Classroom", "Classroom")
						.WithMany()
						.HasForeignKey("ClassroomId")
						.OnDelete(DeleteBehavior.Cascade);
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Questions.AssignmentDueDate", b =>
				{
					b.HasOne("CSC.CSClassroom.Model.Questions.Assignment", "Assignment")
						.WithMany("DueDates")
						.HasForeignKey("AssignmentId")
						.OnDelete(DeleteBehavior.Cascade);

					b.HasOne("CSC.CSClassroom.Model.Classrooms.Section", "Section")
						.WithMany()
						.HasForeignKey("SectionId")
						.OnDelete(DeleteBehavior.Cascade);
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Questions.AssignmentQuestion", b =>
				{
					b.HasOne("CSC.CSClassroom.Model.Questions.Assignment", "Assignment")
						.WithMany("Questions")
						.HasForeignKey("AssignmentId")
						.OnDelete(DeleteBehavior.Cascade);

					b.HasOne("CSC.CSClassroom.Model.Questions.Question", "Question")
						.WithMany("AssignmentQuestions")
						.HasForeignKey("QuestionId")
						.OnDelete(DeleteBehavior.Cascade);
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Questions.ClassroomGradebook", b =>
				{
					b.HasOne("CSC.CSClassroom.Model.Classrooms.Classroom", "Classroom")
						.WithMany("ClassroomGradebooks")
						.HasForeignKey("ClassroomId")
						.OnDelete(DeleteBehavior.Cascade);
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Questions.CodeConstraint", b =>
				{
					b.HasOne("CSC.CSClassroom.Model.Questions.CodeQuestion")
						.WithMany("CodeConstraints")
						.HasForeignKey("CodeQuestionId")
						.OnDelete(DeleteBehavior.Cascade);
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Questions.ImportedClass", b =>
				{
					b.HasOne("CSC.CSClassroom.Model.Questions.CodeQuestion")
						.WithMany("ImportedClasses")
						.HasForeignKey("CodeQuestionId")
						.OnDelete(DeleteBehavior.Cascade);
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Questions.MultipleChoiceQuestionChoice", b =>
				{
					b.HasOne("CSC.CSClassroom.Model.Questions.MultipleChoiceQuestion", "MultipleChoiceQuestion")
						.WithMany("Choices")
						.HasForeignKey("MultipleChoiceQuestionId")
						.OnDelete(DeleteBehavior.Cascade);
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Questions.PrerequisiteQuestion", b =>
				{
					b.HasOne("CSC.CSClassroom.Model.Questions.Question", "FirstQuestion")
						.WithMany()
						.HasForeignKey("FirstQuestionId")
						.OnDelete(DeleteBehavior.Cascade);

					b.HasOne("CSC.CSClassroom.Model.Questions.Question", "SecondQuestion")
						.WithMany()
						.HasForeignKey("SecondQuestionId")
						.OnDelete(DeleteBehavior.Cascade);
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Questions.Question", b =>
				{
					b.HasOne("CSC.CSClassroom.Model.Questions.QuestionCategory", "QuestionCategory")
						.WithMany("Questions")
						.HasForeignKey("QuestionCategoryId")
						.OnDelete(DeleteBehavior.Cascade);
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Questions.QuestionCategory", b =>
				{
					b.HasOne("CSC.CSClassroom.Model.Classrooms.Classroom", "Classroom")
						.WithMany("Categories")
						.HasForeignKey("ClassroomId")
						.OnDelete(DeleteBehavior.Cascade);

					b.HasOne("CSC.CSClassroom.Model.Questions.RandomlySelectedQuestion", "RandomlySelectedQuestion")
						.WithOne("ChoicesCategory")
						.HasForeignKey("CSC.CSClassroom.Model.Questions.QuestionCategory", "RandomlySelectedQuestionId")
						.OnDelete(DeleteBehavior.Cascade);
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Questions.RequiredMethod", b =>
				{
					b.HasOne("CSC.CSClassroom.Model.Questions.ClassQuestion", "ClassQuestion")
						.WithMany("RequiredMethods")
						.HasForeignKey("ClassQuestionId")
						.OnDelete(DeleteBehavior.Cascade);
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Questions.SectionGradebook", b =>
				{
					b.HasOne("CSC.CSClassroom.Model.Questions.ClassroomGradebook", "ClassroomGradebook")
						.WithMany("SectionGradebooks")
						.HasForeignKey("ClassroomGradebookId")
						.OnDelete(DeleteBehavior.Cascade);

					b.HasOne("CSC.CSClassroom.Model.Classrooms.Section", "Section")
						.WithMany("SectionGradebooks")
						.HasForeignKey("SectionId")
						.OnDelete(DeleteBehavior.Cascade);
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Questions.ShortAnswerQuestionBlank", b =>
				{
					b.HasOne("CSC.CSClassroom.Model.Questions.ShortAnswerQuestion", "ShortAnswerQuestion")
						.WithMany("Blanks")
						.HasForeignKey("ShortAnswerQuestionId")
						.OnDelete(DeleteBehavior.Cascade);
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Questions.UserQuestionData", b =>
				{
					b.HasOne("CSC.CSClassroom.Model.Questions.AssignmentQuestion", "AssignmentQuestion")
						.WithMany()
						.HasForeignKey("AssignmentQuestionId")
						.OnDelete(DeleteBehavior.Cascade);

					b.HasOne("CSC.CSClassroom.Model.Users.User", "User")
						.WithMany()
						.HasForeignKey("UserId")
						.OnDelete(DeleteBehavior.Cascade);
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Questions.UserQuestionSubmission", b =>
				{
					b.HasOne("CSC.CSClassroom.Model.Questions.UserQuestionData", "UserQuestionData")
						.WithMany("Submissions")
						.HasForeignKey("UserQuestionDataId")
						.OnDelete(DeleteBehavior.Cascade);
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Users.ClassroomMembership", b =>
				{
					b.HasOne("CSC.CSClassroom.Model.Classrooms.Classroom", "Classroom")
						.WithMany()
						.HasForeignKey("ClassroomId")
						.OnDelete(DeleteBehavior.Cascade);

					b.HasOne("CSC.CSClassroom.Model.Users.User", "User")
						.WithMany("ClassroomMemberships")
						.HasForeignKey("UserId")
						.OnDelete(DeleteBehavior.Cascade);
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Users.SectionMembership", b =>
				{
					b.HasOne("CSC.CSClassroom.Model.Users.ClassroomMembership", "ClassroomMembership")
						.WithMany("SectionMemberships")
						.HasForeignKey("ClassroomMembershipId")
						.OnDelete(DeleteBehavior.Cascade);

					b.HasOne("CSC.CSClassroom.Model.Classrooms.Section", "Section")
						.WithMany("SectionMemberships")
						.HasForeignKey("SectionId")
						.OnDelete(DeleteBehavior.Cascade);
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Questions.ClassQuestionTest", b =>
				{
					b.HasOne("CSC.CSClassroom.Model.Questions.ClassQuestion", "ClassQuestion")
						.WithMany("Tests")
						.HasForeignKey("ClassQuestionId")
						.OnDelete(DeleteBehavior.Cascade);
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Questions.MethodQuestionTest", b =>
				{
					b.HasOne("CSC.CSClassroom.Model.Questions.MethodQuestion", "MethodQuestion")
						.WithMany("Tests")
						.HasForeignKey("MethodQuestionId")
						.OnDelete(DeleteBehavior.Cascade);
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Questions.ProgramQuestionTest", b =>
				{
					b.HasOne("CSC.CSClassroom.Model.Questions.ProgramQuestion", "ProgramQuestion")
						.WithMany("Tests")
						.HasForeignKey("ProgramQuestionId")
						.OnDelete(DeleteBehavior.Cascade);
				});
		}
	}
}
