using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CSC.CSClassroom.Repository.Migrations
{
	public partial class Migration17 : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
				name: "DueDate",
				table: "Checkpoints");

			migrationBuilder.CreateTable(
				name: "CheckpointDates",
				columns: table => new
				{
					Id = table.Column<int>(nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
					CheckpointId = table.Column<int>(nullable: false),
					DueDate = table.Column<DateTime>(nullable: false),
					SectionId = table.Column<int>(nullable: false),
					StartDate = table.Column<DateTime>(nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_CheckpointDates", x => x.Id);
					table.ForeignKey(
						name: "FK_CheckpointDates_Checkpoints_CheckpointId",
						column: x => x.CheckpointId,
						principalTable: "Checkpoints",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "FK_CheckpointDates_Sections_SectionId",
						column: x => x.SectionId,
						principalTable: "Sections",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.AddColumn<DateTime>(
				name: "DateFeedbackRead",
				table: "Submissions",
				nullable: true);

			migrationBuilder.AddColumn<DateTime>(
				name: "DateFeedbackSaved",
				table: "Submissions",
				nullable: true);

			migrationBuilder.AddColumn<string>(
				name: "Feedback",
				table: "Submissions",
				nullable: true);

			migrationBuilder.AddColumn<int>(
				name: "PullRequestNumber",
				table: "Submissions",
				nullable: false,
				defaultValue: 0);

			migrationBuilder.CreateIndex(
				name: "IX_Submissions_CheckpointId",
				table: "Submissions",
				column: "CheckpointId");

			migrationBuilder.CreateIndex(
				name: "IX_Submissions_CommitId",
				table: "Submissions",
				column: "CommitId");

			migrationBuilder.CreateIndex(
				name: "IX_Submissions_CheckpointId_CommitId_DateSubmitted",
				table: "Submissions",
				columns: new[] { "CheckpointId", "CommitId", "DateSubmitted" },
				unique: true);

			migrationBuilder.CreateIndex(
				name: "IX_CheckpointDates_CheckpointId",
				table: "CheckpointDates",
				column: "CheckpointId");

			migrationBuilder.CreateIndex(
				name: "IX_CheckpointDates_SectionId",
				table: "CheckpointDates",
				column: "SectionId");

			migrationBuilder.CreateIndex(
				name: "IX_CheckpointDates_CheckpointId_SectionId",
				table: "CheckpointDates",
				columns: new[] { "CheckpointId", "SectionId" },
				unique: true);

			migrationBuilder.AddForeignKey(
				name: "FK_Submissions_Checkpoints_CheckpointId",
				table: "Submissions",
				column: "CheckpointId",
				principalTable: "Checkpoints",
				principalColumn: "Id",
				onDelete: ReferentialAction.Cascade);

			migrationBuilder.AddForeignKey(
				name: "FK_Submissions_Commits_CommitId",
				table: "Submissions",
				column: "CommitId",
				principalTable: "Commits",
				principalColumn: "Id",
				onDelete: ReferentialAction.Cascade);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropForeignKey(
				name: "FK_Submissions_Checkpoints_CheckpointId",
				table: "Submissions");

			migrationBuilder.DropForeignKey(
				name: "FK_Submissions_Commits_CommitId",
				table: "Submissions");

			migrationBuilder.DropIndex(
				name: "IX_Submissions_CheckpointId",
				table: "Submissions");

			migrationBuilder.DropIndex(
				name: "IX_Submissions_CommitId",
				table: "Submissions");

			migrationBuilder.DropIndex(
				name: "IX_Submissions_CheckpointId_CommitId_DateSubmitted",
				table: "Submissions");

			migrationBuilder.DropColumn(
				name: "DateFeedbackRead",
				table: "Submissions");

			migrationBuilder.DropColumn(
				name: "DateFeedbackSaved",
				table: "Submissions");

			migrationBuilder.DropColumn(
				name: "Feedback",
				table: "Submissions");

			migrationBuilder.DropColumn(
				name: "PullRequestNumber",
				table: "Submissions");

			migrationBuilder.DropTable(
				name: "CheckpointDates");

			migrationBuilder.AddColumn<DateTime>(
				name: "DueDate",
				table: "Checkpoints",
				nullable: false,
				defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
		}
	}
}
