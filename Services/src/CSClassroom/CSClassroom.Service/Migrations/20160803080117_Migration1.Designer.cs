using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using CSC.CSClassroom.Service.Data;

namespace CSClassroom.Service.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20160803080117_Migration1")]
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

                    b.Property<int>("GroupId");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 50);

                    b.HasKey("Id");

                    b.HasIndex("GroupId");

                    b.ToTable("Classrooms");
                });

            modelBuilder.Entity("CSC.CSClassroom.Model.Classrooms.Group", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("DisplayName")
                        .IsRequired();

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 50);

                    b.HasKey("Id");

                    b.ToTable("Groups");
                });

            modelBuilder.Entity("CSC.CSClassroom.Model.Exercises.CodeQuestionTest", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("CodeQuestionId");

                    b.Property<string>("Discriminator")
                        .IsRequired();

                    b.Property<string>("ExpectedOuptut");

                    b.Property<string>("ExpectedReturnValue");

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.HasIndex("CodeQuestionId");

                    b.ToTable("CodeQuestionTests");

                    b.HasDiscriminator<string>("Discriminator").HasValue("CodeQuestionTest");
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

                    b.Property<int?>("ClassroomId");

                    b.Property<int>("GroupId");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 50);

                    b.HasKey("Id");

                    b.HasIndex("ClassroomId");

                    b.HasIndex("GroupId");

                    b.ToTable("QuestionCategories");
                });

            modelBuilder.Entity("CSC.CSClassroom.Model.Exercises.ClassQuestionTest", b =>
                {
                    b.HasBaseType("CSC.CSClassroom.Model.Exercises.CodeQuestionTest");

                    b.Property<string>("MethodBody");

                    b.Property<string>("ReturnType");

                    b.ToTable("ClassQuestionTest");

                    b.HasDiscriminator().HasValue("ClassQuestionTest");
                });

            modelBuilder.Entity("CSC.CSClassroom.Model.Exercises.MethodQuestionTest", b =>
                {
                    b.HasBaseType("CSC.CSClassroom.Model.Exercises.CodeQuestionTest");

                    b.Property<string>("ParameterValues");

                    b.ToTable("MethodQuestionTest");

                    b.HasDiscriminator().HasValue("MethodQuestionTest");
                });

            modelBuilder.Entity("CSC.CSClassroom.Model.Exercises.CodeQuestion", b =>
                {
                    b.HasBaseType("CSC.CSClassroom.Model.Exercises.Question");

                    b.Property<int?>("CodeQuestionId");

                    b.Property<string>("ImportedClasses");

                    b.HasIndex("CodeQuestionId");

                    b.ToTable("CodeQuestion");

                    b.HasDiscriminator().HasValue("CodeQuestion");
                });

            modelBuilder.Entity("CSC.CSClassroom.Model.Exercises.ClassQuestion", b =>
                {
                    b.HasBaseType("CSC.CSClassroom.Model.Exercises.CodeQuestion");

                    b.Property<string>("ClassName")
                        .IsRequired();

                    b.Property<string>("FileTemplate")
                        .IsRequired();

                    b.ToTable("ClassQuestion");

                    b.HasDiscriminator().HasValue("ClassQuestion");
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

            modelBuilder.Entity("CSC.CSClassroom.Model.Classrooms.Classroom", b =>
                {
                    b.HasOne("CSC.CSClassroom.Model.Classrooms.Group", "Group")
                        .WithMany("Classrooms")
                        .HasForeignKey("GroupId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("CSC.CSClassroom.Model.Exercises.CodeQuestionTest", b =>
                {
                    b.HasOne("CSC.CSClassroom.Model.Exercises.CodeQuestion", "CodeQuestion")
                        .WithMany()
                        .HasForeignKey("CodeQuestionId")
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
                    b.HasOne("CSC.CSClassroom.Model.Classrooms.Classroom")
                        .WithMany("Categories")
                        .HasForeignKey("ClassroomId");

                    b.HasOne("CSC.CSClassroom.Model.Classrooms.Group", "Group")
                        .WithMany()
                        .HasForeignKey("GroupId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("CSC.CSClassroom.Model.Exercises.CodeQuestion", b =>
                {
                    b.HasOne("CSC.CSClassroom.Model.Exercises.CodeQuestion")
                        .WithMany("Tests")
                        .HasForeignKey("CodeQuestionId");
                });
        }
    }
}
