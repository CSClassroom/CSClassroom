using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace CSC.CSClassroom.Repository.Migrations
{
	public partial class Migration19 : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
				name: "AssignmentsLastGradedDate",
				table: "Sections");

			migrationBuilder.CreateTable(
				name: "ClassroomGradebooks",
				columns: table => new
				{
					Id = table.Column<int>(nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
					ClassroomId = table.Column<int>(nullable: false),
					Name = table.Column<string>(nullable: false),
					Order = table.Column<int>(nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_ClassroomGradebooks", x => x.Id);
					table.ForeignKey(
						name: "FK_ClassroomGradebooks_Classrooms_ClassroomId",
						column: x => x.ClassroomId,
						principalTable: "Classrooms",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "SectionGradebooks",
				columns: table => new
				{
					Id = table.Column<int>(nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
					ClassroomGradebookId = table.Column<int>(nullable: false),
					LastTransferDate = table.Column<DateTime>(nullable: false),
					SectionId = table.Column<int>(nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_SectionGradebooks", x => x.Id);
					table.ForeignKey(
						name: "FK_SectionGradebooks_ClassroomGradebooks_ClassroomGradebookId",
						column: x => x.ClassroomGradebookId,
						principalTable: "ClassroomGradebooks",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "FK_SectionGradebooks_Sections_SectionId",
						column: x => x.SectionId,
						principalTable: "Sections",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateIndex(
				name: "IX_ClassroomGradebooks_ClassroomId",
				table: "ClassroomGradebooks",
				column: "ClassroomId");

			migrationBuilder.CreateIndex(
				name: "IX_ClassroomGradebooks_ClassroomId_Name",
				table: "ClassroomGradebooks",
				columns: new[] { "ClassroomId", "Name" },
				unique: true);

			migrationBuilder.CreateIndex(
				name: "IX_SectionGradebooks_ClassroomGradebookId",
				table: "SectionGradebooks",
				column: "ClassroomGradebookId");

			migrationBuilder.CreateIndex(
				name: "IX_SectionGradebooks_SectionId",
				table: "SectionGradebooks",
				column: "SectionId");

			migrationBuilder.CreateIndex(
				name: "IX_SectionGradebooks_ClassroomGradebookId_SectionId",
				table: "SectionGradebooks",
				columns: new[] { "ClassroomGradebookId", "SectionId" },
				unique: true);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "SectionGradebooks");

			migrationBuilder.DropTable(
				name: "ClassroomGradebooks");

			migrationBuilder.AddColumn<DateTime>(
				name: "AssignmentsLastGradedDate",
				table: "Sections",
				nullable: false,
				defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
		}
	}
}
