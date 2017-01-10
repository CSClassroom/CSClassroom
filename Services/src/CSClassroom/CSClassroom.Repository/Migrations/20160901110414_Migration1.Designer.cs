using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CSC.CSClassroom.Repository.Migrations
{
	[DbContext(typeof(DatabaseContext))]
	[Migration("20160901110414_Migration1")]
	partial class Migration1
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

					b.ToTable("Sections");
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

					b.Property<string>("Description")
						.IsRequired();

					b.Property<string>("Discriminator")
						.IsRequired();

					b.Property<string>("Name")
						.IsRequired()
						.HasAnnotation("MaxLength", 50);

					b.Property<int>("QuestionCategoryId");

					b.HasKey("Id");

					b.HasIndex("QuestionCategoryId");

					b.ToTable("Questions");

					b.HasDiscriminator<string>("Discriminator").HasValue("Question");
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Exercises.QuestionCategory", b =>
				{
					b.Property<int>("Id")
						.ValueGeneratedOnAdd();

					b.Property<int>("ClassroomId");

					b.Property<string>("Name")
						.IsRequired()
						.HasAnnotation("MaxLength", 50);

					b.HasKey("Id");

					b.HasIndex("ClassroomId");

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

					b.ToTable("Users");
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Exercises.ClassQuestionTest", b =>
				{
					b.HasBaseType("CSC.CSClassroom.Model.Exercises.CodeQuestionTest");

					b.Property<int>("ClassQuestionId");

					b.Property<string>("Description");

					b.Property<string>("MethodBody");

					b.Property<string>("ReturnType");

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

					b.Property<string>("TestDescription");

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

					b.Property<bool>("RandomizeAnswers");

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

			modelBuilder.Entity("CSC.CSClassroom.Model.Exercises.GeneratedQuestion", b =>
				{
					b.HasBaseType("CSC.CSClassroom.Model.Exercises.CodeQuestion");

					b.Property<string>("GeneratorContents");

					b.ToTable("GeneratedQuestion");

					b.HasDiscriminator().HasValue("GeneratedQuestion");
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

			modelBuilder.Entity("CSC.CSClassroom.Model.Users.ClassroomMembership", b =>
				{
					b.HasOne("CSC.CSClassroom.Model.Classrooms.Classroom", "Classroom")
						.WithMany()
						.HasForeignKey("ClassroomId")
						.OnDelete(DeleteBehavior.Cascade);

					b.HasOne("CSC.CSClassroom.Model.Users.User")
						.WithMany("ClassroomMemberships")
						.HasForeignKey("UserId")
						.OnDelete(DeleteBehavior.Cascade);
				});

			modelBuilder.Entity("CSC.CSClassroom.Model.Users.SectionMembership", b =>
				{
					b.HasOne("CSC.CSClassroom.Model.Users.ClassroomMembership")
						.WithMany("SectionMemberships")
						.HasForeignKey("ClassroomMembershipId")
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
