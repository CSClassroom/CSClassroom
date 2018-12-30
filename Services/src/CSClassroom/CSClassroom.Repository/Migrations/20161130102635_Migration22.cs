using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace CSC.CSClassroom.Repository.Migrations
{
	public partial class Migration22 : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "PrerequisiteQuestions",
				columns: table => new
				{
					Id = table.Column<int>(nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
					FirstQuestionId = table.Column<int>(nullable: false),
					Order = table.Column<int>(nullable: false),
					SecondQuestionId = table.Column<int>(nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_PrerequisiteQuestions", x => x.Id);
					table.ForeignKey(
						name: "FK_PrerequisiteQuestions_Questions_FirstQuestionId",
						column: x => x.FirstQuestionId,
						principalTable: "Questions",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "FK_PrerequisiteQuestions_Questions_SecondQuestionId",
						column: x => x.SecondQuestionId,
						principalTable: "Questions",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateIndex(
				name: "IX_PrerequisiteQuestions_FirstQuestionId",
				table: "PrerequisiteQuestions",
				column: "FirstQuestionId");

			migrationBuilder.CreateIndex(
				name: "IX_PrerequisiteQuestions_SecondQuestionId",
				table: "PrerequisiteQuestions",
				column: "SecondQuestionId");
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "PrerequisiteQuestions");
		}
	}
}
