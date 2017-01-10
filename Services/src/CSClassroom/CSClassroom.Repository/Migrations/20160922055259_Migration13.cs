using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CSC.CSClassroom.Repository.Migrations
{
	public partial class Migration13 : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "Assignments",
				columns: table => new
				{
					Id = table.Column<int>(nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
					ClassroomId = table.Column<int>(nullable: false),
					GroupName = table.Column<string>(maxLength: 50, nullable: true),
					Name = table.Column<string>(maxLength: 50, nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Assignments", x => x.Id);
					table.ForeignKey(
						name: "FK_Assignments_Classrooms_ClassroomId",
						column: x => x.ClassroomId,
						principalTable: "Classrooms",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "AssignmentDueDates",
				columns: table => new
				{
					Id = table.Column<int>(nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
					AssignmentId = table.Column<int>(nullable: false),
					DueDate = table.Column<DateTime>(nullable: false),
					SectionId = table.Column<int>(nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_AssignmentDueDates", x => x.Id);
					table.ForeignKey(
						name: "FK_AssignmentDueDates_Assignments_AssignmentId",
						column: x => x.AssignmentId,
						principalTable: "Assignments",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "FK_AssignmentDueDates_Sections_SectionId",
						column: x => x.SectionId,
						principalTable: "Sections",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "AssignmentQuestions",
				columns: table => new
				{
					Id = table.Column<int>(nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
					AssignmentId = table.Column<int>(nullable: false),
					Order = table.Column<int>(nullable: false),
					Points = table.Column<double>(nullable: false),
					QuestionId = table.Column<int>(nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_AssignmentQuestions", x => x.Id);
					table.ForeignKey(
						name: "FK_AssignmentQuestions_Assignments_AssignmentId",
						column: x => x.AssignmentId,
						principalTable: "Assignments",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "FK_AssignmentQuestions_Questions_QuestionId",
						column: x => x.QuestionId,
						principalTable: "Questions",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateIndex(
				name: "IX_Assignments_ClassroomId",
				table: "Assignments",
				column: "ClassroomId");

			migrationBuilder.CreateIndex(
				name: "IX_Assignments_ClassroomId_Name",
				table: "Assignments",
				columns: new[] { "ClassroomId", "Name" },
				unique: true);

			migrationBuilder.CreateIndex(
				name: "IX_AssignmentDueDates_AssignmentId",
				table: "AssignmentDueDates",
				column: "AssignmentId");

			migrationBuilder.CreateIndex(
				name: "IX_AssignmentDueDates_SectionId",
				table: "AssignmentDueDates",
				column: "SectionId");

			migrationBuilder.CreateIndex(
				name: "IX_AssignmentDueDates_AssignmentId_SectionId",
				table: "AssignmentDueDates",
				columns: new[] { "AssignmentId", "SectionId" },
				unique: true);

			migrationBuilder.CreateIndex(
				name: "IX_AssignmentQuestions_AssignmentId",
				table: "AssignmentQuestions",
				column: "AssignmentId");

			migrationBuilder.CreateIndex(
				name: "IX_AssignmentQuestions_QuestionId",
				table: "AssignmentQuestions",
				column: "QuestionId");

			migrationBuilder.CreateIndex(
				name: "IX_AssignmentQuestions_AssignmentId_QuestionId",
				table: "AssignmentQuestions",
				columns: new[] { "AssignmentId", "QuestionId" },
				unique: true);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "AssignmentDueDates");

			migrationBuilder.DropTable(
				name: "AssignmentQuestions");

			migrationBuilder.DropTable(
				name: "Assignments");
		}
	}
}
