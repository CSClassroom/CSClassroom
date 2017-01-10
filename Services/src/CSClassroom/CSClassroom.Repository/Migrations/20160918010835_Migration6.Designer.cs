using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CSC.CSClassroom.Repository.Migrations
{
	[DbContext(typeof(DatabaseContext))]
	[Migration("20160918010835_Migration6")]
	partial class Migration6
	{
		protected override void BuildTargetModel(ModelBuilder modelBuilder)
		{
			modelBuilder
				.HasAnnotation("ProductVersion", "1.0.0-rtm-21431");

			modelBuilder.Entity("CSC.CSClassroom.Model.Classrooms.Classroom", b =>
				{
					b.Property<int>("Id")
						.ValueGeneratedOnAdd();

					b.Property<string>("DisplayName")
						.IsRequired();

					b.Property<string>("GitHubOrganization")
						.IsRequired();

					b.Property<string>("Name")
						.IsRequired()
						.HasAnnotation("MaxLength", 50);

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
						.HasAnnotation("MaxLength", 50);

					b.HasKey("Id");

					b.HasIndex("ClassroomId");

					b.HasIndex("ClassroomId", "Name")
						.IsUnique();

					b.ToTable("Sections");
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Exercises.CodeConstraint", b =>
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

			modelBuilder.Entity("CSC.CSClassroom.Model.Exercises.CodeQuestionTest", b =>
				{
					b.Property<int>("Id")
						.ValueGeneratedOnAdd();

					b.Property<string>("Discriminator")
						.IsRequired();

					b.Property<string>("ExpectedOutput");

					b.Property<string>("ExpectedReturnValue");

					b.Property<string>("Name")
						.IsRequired();

					b.Property<int>("Order");

					b.HasKey("Id");

					b.ToTable("CodeQuestionTests");

					b.HasDiscriminator<string>("Discriminator").HasValue("CodeQuestionTest");
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Exercises.ImportedClass", b =>
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

			modelBuilder.Entity("CSC.CSClassroom.Model.Exercises.MultipleChoiceQuestionChoice", b =>
				{
					b.Property<int>("Id")
						.ValueGeneratedOnAdd();

					b.Property<bool>("Correct");

					b.Property<int>("MultipleChoiceQuestionId");

					b.Property<int>("Order");

					b.Property<string>("Value");

					b.HasKey("Id");

					b.HasIndex("MultipleChoiceQuestionId");

					b.ToTable("MultipleChoiceQuestionChoices");
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Exercises.Question", b =>
				{
					b.Property<int>("Id")
						.ValueGeneratedOnAdd();

					b.Property<bool>("AllowPartialCredit");

					b.Property<string>("Description")
						.IsRequired();

					b.Property<string>("Discriminator")
						.IsRequired();

					b.Property<bool>("IsPrivate");

					b.Property<string>("Name")
						.IsRequired()
						.HasAnnotation("MaxLength", 50);

					b.Property<int>("QuestionCategoryId");

					b.HasKey("Id");

					b.HasIndex("QuestionCategoryId");

					b.HasIndex("QuestionCategoryId", "Name")
						.IsUnique();

					b.ToTable("Questions");

					b.HasDiscriminator<string>("Discriminator").HasValue("Question");
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Exercises.QuestionCategory", b =>
				{
					b.Property<int>("Id")
						.ValueGeneratedOnAdd();

					b.Property<int>("ClassroomId");

					b.Property<bool>("IsPrivate");

					b.Property<string>("Name")
						.IsRequired()
						.HasAnnotation("MaxLength", 50);

					b.HasKey("Id");

					b.HasIndex("ClassroomId");

					b.HasIndex("ClassroomId", "Name")
						.IsUnique();

					b.ToTable("QuestionCategories");
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Exercises.RequiredMethod", b =>
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

			modelBuilder.Entity("CSC.CSClassroom.Model.Exercises.ShortAnswerQuestionBlank", b =>
				{
					b.Property<int>("Id")
						.ValueGeneratedOnAdd();

					b.Property<string>("Answer");

					b.Property<string>("Name");

					b.Property<int>("Order");

					b.Property<int>("ShortAnswerQuestionId");

					b.HasKey("Id");

					b.HasIndex("ShortAnswerQuestionId");

					b.ToTable("ShortAnswerQuestionBlanks");
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Exercises.UserQuestionData", b =>
				{
					b.Property<int>("Id")
						.ValueGeneratedOnAdd();

					b.Property<string>("CachedQuestionData");

					b.Property<DateTime?>("CachedQuestionDataTime");

					b.Property<string>("LastQuestionSubmission");

					b.Property<int>("NumAttempts");

					b.Property<int>("QuestionId");

					b.Property<int?>("Seed");

					b.Property<int>("UserId");

					b.HasKey("Id");

					b.HasIndex("QuestionId", "UserId")
						.IsUnique();

					b.ToTable("UserQuestionData");
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Exercises.UserQuestionSubmission", b =>
				{
					b.Property<int>("Id")
						.ValueGeneratedOnAdd();

					b.Property<DateTime>("DateSubmitted");

					b.Property<double>("Score");

					b.Property<int>("UserQuestionDataId");

					b.HasKey("Id");

					b.HasIndex("UserQuestionDataId");

					b.ToTable("UserQuestionSubmission");
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

					b.Property<DateTime>("DueDate");

					b.Property<string>("Name")
						.HasAnnotation("MaxLength", 50);

					b.Property<int>("ProjectId");

					b.HasKey("Id");

					b.HasIndex("ProjectId");

					b.HasIndex("ProjectId", "Name")
						.IsUnique();

					b.ToTable("Checkpoints");
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Projects.Commit", b =>
				{
					b.Property<int>("Id")
						.ValueGeneratedOnAdd();

					b.Property<string>("BuildRequestToken");

					b.Property<string>("Message");

					b.Property<int>("ProjectId");

					b.Property<DateTime>("PushDate");

					b.Property<string>("Sha");

					b.Property<int>("UserId");

					b.HasKey("Id");

					b.HasIndex("ProjectId", "Sha")
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
						.HasAnnotation("MaxLength", 50);

					b.HasKey("Id");

					b.HasIndex("ClassroomId");

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

					b.Property<DateTime>("DateSubmitted");

					b.HasKey("Id");

					b.ToTable("Submissions");
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Projects.TestClass", b =>
				{
					b.Property<int>("Id")
						.ValueGeneratedOnAdd();

					b.Property<string>("ClassName");

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

					b.Property<string>("TestName");

					b.HasKey("Id");

					b.HasIndex("BuildId");

					b.HasIndex("BuildId", "ClassName", "TestName")
						.IsUnique();

					b.ToTable("TestResults");
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

					b.HasIndex("UserId");

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

					b.HasIndex("ClassroomMembershipId");

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

			modelBuilder.Entity("CSC.CSClassroom.Model.Exercises.ClassQuestionTest", b =>
				{
					b.HasBaseType("CSC.CSClassroom.Model.Exercises.CodeQuestionTest");

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

			modelBuilder.Entity("CSC.CSClassroom.Model.Exercises.MethodQuestionTest", b =>
				{
					b.HasBaseType("CSC.CSClassroom.Model.Exercises.CodeQuestionTest");

					b.Property<int>("MethodQuestionId");

					b.Property<string>("ParameterValues");

					b.HasIndex("MethodQuestionId");

					b.ToTable("MethodQuestionTest");

					b.HasDiscriminator().HasValue("MethodQuestionTest");
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Exercises.ProgramQuestionTest", b =>
				{
					b.HasBaseType("CSC.CSClassroom.Model.Exercises.CodeQuestionTest");

					b.Property<string>("CommandLineArguments");

					b.Property<int>("ProgramQuestionId");

					b.Property<string>("TestDescription")
						.IsRequired();

					b.HasIndex("ProgramQuestionId");

					b.ToTable("ProgramQuestionTest");

					b.HasDiscriminator().HasValue("ProgramQuestionTest");
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Exercises.CodeQuestion", b =>
				{
					b.HasBaseType("CSC.CSClassroom.Model.Exercises.Question");

					b.Property<string>("InitialSubmission");

					b.ToTable("CodeQuestion");

					b.HasDiscriminator().HasValue("CodeQuestion");
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Exercises.MultipleChoiceQuestion", b =>
				{
					b.HasBaseType("CSC.CSClassroom.Model.Exercises.Question");

					b.Property<bool>("AllowMultipleCorrectAnswers");

					b.ToTable("MultipleChoiceQuestion");

					b.HasDiscriminator().HasValue("MultipleChoiceQuestion");
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Exercises.ShortAnswerQuestion", b =>
				{
					b.HasBaseType("CSC.CSClassroom.Model.Exercises.Question");


					b.ToTable("ShortAnswerQuestion");

					b.HasDiscriminator().HasValue("ShortAnswerQuestion");
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Exercises.ClassQuestion", b =>
				{
					b.HasBaseType("CSC.CSClassroom.Model.Exercises.CodeQuestion");

					b.Property<bool>("AllowPublicFields");

					b.Property<string>("ClassName")
						.IsRequired();

					b.Property<string>("FileTemplate")
						.IsRequired();

					b.ToTable("ClassQuestion");

					b.HasDiscriminator().HasValue("ClassQuestion");
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Exercises.GeneratedQuestionTemplate", b =>
				{
					b.HasBaseType("CSC.CSClassroom.Model.Exercises.CodeQuestion");

					b.Property<DateTime>("DateModified");

					b.Property<string>("FullGeneratorFileContents");

					b.Property<int>("FullGeneratorFileLineOffset");

					b.Property<string>("GeneratorContents");

					b.ToTable("GeneratedQuestionTemplate");

					b.HasDiscriminator().HasValue("GeneratedQuestionTemplate");
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Exercises.MethodQuestion", b =>
				{
					b.HasBaseType("CSC.CSClassroom.Model.Exercises.CodeQuestion");

					b.Property<string>("MethodName")
						.IsRequired();

					b.Property<string>("ParameterTypes")
						.IsRequired();

					b.Property<string>("ReturnType")
						.IsRequired();

					b.ToTable("MethodQuestion");

					b.HasDiscriminator().HasValue("MethodQuestion");
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Exercises.ProgramQuestion", b =>
				{
					b.HasBaseType("CSC.CSClassroom.Model.Exercises.CodeQuestion");

					b.Property<string>("ProgramClassName")
						.IsRequired();

					b.ToTable("ProgramQuestion");

					b.HasDiscriminator().HasValue("ProgramQuestion");
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Classrooms.Section", b =>
				{
					b.HasOne("CSC.CSClassroom.Model.Classrooms.Classroom", "Classroom")
						.WithMany("Classrooms")
						.HasForeignKey("ClassroomId")
						.OnDelete(DeleteBehavior.Cascade);
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Exercises.CodeConstraint", b =>
				{
					b.HasOne("CSC.CSClassroom.Model.Exercises.CodeQuestion")
						.WithMany("CodeConstraints")
						.HasForeignKey("CodeQuestionId")
						.OnDelete(DeleteBehavior.Cascade);
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Exercises.ImportedClass", b =>
				{
					b.HasOne("CSC.CSClassroom.Model.Exercises.CodeQuestion")
						.WithMany("ImportedClasses")
						.HasForeignKey("CodeQuestionId")
						.OnDelete(DeleteBehavior.Cascade);
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Exercises.MultipleChoiceQuestionChoice", b =>
				{
					b.HasOne("CSC.CSClassroom.Model.Exercises.MultipleChoiceQuestion", "MultipleChoiceQuestion")
						.WithMany("Choices")
						.HasForeignKey("MultipleChoiceQuestionId")
						.OnDelete(DeleteBehavior.Cascade);
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Exercises.Question", b =>
				{
					b.HasOne("CSC.CSClassroom.Model.Exercises.QuestionCategory", "QuestionCategory")
						.WithMany("Questions")
						.HasForeignKey("QuestionCategoryId")
						.OnDelete(DeleteBehavior.Cascade);
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Exercises.QuestionCategory", b =>
				{
					b.HasOne("CSC.CSClassroom.Model.Classrooms.Classroom", "Classroom")
						.WithMany("Categories")
						.HasForeignKey("ClassroomId")
						.OnDelete(DeleteBehavior.Cascade);
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Exercises.RequiredMethod", b =>
				{
					b.HasOne("CSC.CSClassroom.Model.Exercises.ClassQuestion", "ClassQuestion")
						.WithMany("RequiredMethods")
						.HasForeignKey("ClassQuestionId")
						.OnDelete(DeleteBehavior.Cascade);
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Exercises.ShortAnswerQuestionBlank", b =>
				{
					b.HasOne("CSC.CSClassroom.Model.Exercises.ShortAnswerQuestion", "ShortAnswerQuestion")
						.WithMany("Blanks")
						.HasForeignKey("ShortAnswerQuestionId")
						.OnDelete(DeleteBehavior.Cascade);
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Exercises.UserQuestionSubmission", b =>
				{
					b.HasOne("CSC.CSClassroom.Model.Exercises.UserQuestionData")
						.WithMany("Submissions")
						.HasForeignKey("UserQuestionDataId")
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
					b.HasOne("CSC.CSClassroom.Model.Projects.Project")
						.WithMany("Checkpoints")
						.HasForeignKey("ProjectId")
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

			modelBuilder.Entity("CSC.CSClassroom.Model.Projects.TestClass", b =>
				{
					b.HasOne("CSC.CSClassroom.Model.Projects.Project")
						.WithMany("TestClasses")
						.HasForeignKey("ProjectId")
						.OnDelete(DeleteBehavior.Cascade);
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Projects.TestResult", b =>
				{
					b.HasOne("CSC.CSClassroom.Model.Projects.Build")
						.WithMany("TestResults")
						.HasForeignKey("BuildId")
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
						.WithMany()
						.HasForeignKey("SectionId")
						.OnDelete(DeleteBehavior.Cascade);
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Exercises.ClassQuestionTest", b =>
				{
					b.HasOne("CSC.CSClassroom.Model.Exercises.ClassQuestion", "ClassQuestion")
						.WithMany("Tests")
						.HasForeignKey("ClassQuestionId")
						.OnDelete(DeleteBehavior.Cascade);
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Exercises.MethodQuestionTest", b =>
				{
					b.HasOne("CSC.CSClassroom.Model.Exercises.MethodQuestion", "MethodQuestion")
						.WithMany("Tests")
						.HasForeignKey("MethodQuestionId")
						.OnDelete(DeleteBehavior.Cascade);
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Exercises.ProgramQuestionTest", b =>
				{
					b.HasOne("CSC.CSClassroom.Model.Exercises.ProgramQuestion", "ProgramQuestion")
						.WithMany("Tests")
						.HasForeignKey("ProgramQuestionId")
						.OnDelete(DeleteBehavior.Cascade);
				});
		}
	}
}
