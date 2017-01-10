using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CSC.CSClassroom.Repository.Migrations
{
	public partial class Migration5 : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "Commits",
				columns: table => new
				{
					Id = table.Column<int>(nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
					BuildRequestToken = table.Column<string>(nullable: true),
					Message = table.Column<string>(nullable: true),
					ProjectId = table.Column<int>(nullable: false),
					PushDate = table.Column<DateTime>(nullable: false),
					Sha = table.Column<string>(nullable: true),
					UserId = table.Column<int>(nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Commits", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "Projects",
				columns: table => new
				{
					Id = table.Column<int>(nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
					BuildCommits = table.Column<bool>(nullable: false),
					ClassroomId = table.Column<int>(nullable: false),
					ExplicitSubmissionRequired = table.Column<bool>(nullable: false),
					Name = table.Column<string>(maxLength: 50, nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Projects", x => x.Id);
					table.ForeignKey(
						name: "FK_Projects_Classrooms_ClassroomId",
						column: x => x.ClassroomId,
						principalTable: "Classrooms",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "Submissions",
				columns: table => new
				{
					Id = table.Column<int>(nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
					CheckpointId = table.Column<int>(nullable: false),
					CommitId = table.Column<int>(nullable: false),
					DateSubmitted = table.Column<DateTime>(nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Submissions", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "Build",
				columns: table => new
				{
					Id = table.Column<int>(nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
					CommitId = table.Column<int>(nullable: false),
					DateCompleted = table.Column<DateTime>(nullable: false),
					DateStarted = table.Column<DateTime>(nullable: false),
					Output = table.Column<string>(nullable: true),
					Status = table.Column<int>(nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Build", x => x.Id);
					table.ForeignKey(
						name: "FK_Build_Commits_CommitId",
						column: x => x.CommitId,
						principalTable: "Commits",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "Checkpoints",
				columns: table => new
				{
					Id = table.Column<int>(nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
					DisplayName = table.Column<string>(nullable: true),
					DueDate = table.Column<DateTime>(nullable: false),
					Name = table.Column<string>(maxLength: 50, nullable: true),
					ProjectId = table.Column<int>(nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Checkpoints", x => x.Id);
					table.ForeignKey(
						name: "FK_Checkpoints_Projects_ProjectId",
						column: x => x.ProjectId,
						principalTable: "Projects",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "ImmutableFilePaths",
				columns: table => new
				{
					Id = table.Column<int>(nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
					Path = table.Column<string>(nullable: true),
					ProjectId = table.Column<int>(nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_ImmutableFilePaths", x => x.Id);
					table.ForeignKey(
						name: "FK_ImmutableFilePaths_Projects_ProjectId",
						column: x => x.ProjectId,
						principalTable: "Projects",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "PrivateFilePaths",
				columns: table => new
				{
					Id = table.Column<int>(nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
					Path = table.Column<string>(nullable: true),
					ProjectId = table.Column<int>(nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_PrivateFilePaths", x => x.Id);
					table.ForeignKey(
						name: "FK_PrivateFilePaths_Projects_ProjectId",
						column: x => x.ProjectId,
						principalTable: "Projects",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "TestClasses",
				columns: table => new
				{
					Id = table.Column<int>(nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
					ClassName = table.Column<string>(nullable: true),
					Order = table.Column<int>(nullable: false),
					ProjectId = table.Column<int>(nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_TestClasses", x => x.Id);
					table.ForeignKey(
						name: "FK_TestClasses_Projects_ProjectId",
						column: x => x.ProjectId,
						principalTable: "Projects",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateIndex(
				name: "IX_Build_CommitId",
				table: "Build",
				column: "CommitId",
				unique: true);

			migrationBuilder.CreateIndex(
				name: "IX_Checkpoints_ProjectId",
				table: "Checkpoints",
				column: "ProjectId");

			migrationBuilder.CreateIndex(
				name: "IX_Checkpoints_ProjectId_Name",
				table: "Checkpoints",
				columns: new[] { "ProjectId", "Name" },
				unique: true);

			migrationBuilder.CreateIndex(
				name: "IX_Commits_ProjectId_Sha",
				table: "Commits",
				columns: new[] { "ProjectId", "Sha" },
				unique: true);

			migrationBuilder.CreateIndex(
				name: "IX_ImmutableFilePaths_ProjectId",
				table: "ImmutableFilePaths",
				column: "ProjectId");

			migrationBuilder.CreateIndex(
				name: "IX_PrivateFilePaths_ProjectId",
				table: "PrivateFilePaths",
				column: "ProjectId");

			migrationBuilder.CreateIndex(
				name: "IX_Projects_ClassroomId",
				table: "Projects",
				column: "ClassroomId");

			migrationBuilder.CreateIndex(
				name: "IX_Projects_ClassroomId_Name",
				table: "Projects",
				columns: new[] { "ClassroomId", "Name" },
				unique: true);

			migrationBuilder.CreateIndex(
				name: "IX_TestClasses_ProjectId",
				table: "TestClasses",
				column: "ProjectId");
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "Build");

			migrationBuilder.DropTable(
				name: "Checkpoints");

			migrationBuilder.DropTable(
				name: "ImmutableFilePaths");

			migrationBuilder.DropTable(
				name: "PrivateFilePaths");

			migrationBuilder.DropTable(
				name: "Submissions");

			migrationBuilder.DropTable(
				name: "TestClasses");

			migrationBuilder.DropTable(
				name: "Commits");

			migrationBuilder.DropTable(
				name: "Projects");
		}
	}
}
