using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CSC.CSClassroom.Repository.Migrations
{
	public partial class Migration4 : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "CodeConstraints",
				columns: table => new
				{
					Id = table.Column<int>(nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
					CodeQuestionId = table.Column<int>(nullable: false),
					ErrorMessage = table.Column<string>(nullable: false),
					Frequency = table.Column<int>(nullable: false),
					Order = table.Column<int>(nullable: false),
					Regex = table.Column<string>(nullable: false),
					Type = table.Column<int>(nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_CodeConstraints", x => x.Id);
					table.ForeignKey(
						name: "FK_CodeConstraints_Questions_CodeQuestionId",
						column: x => x.CodeQuestionId,
						principalTable: "Questions",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateIndex(
				name: "IX_CodeConstraints_CodeQuestionId",
				table: "CodeConstraints",
				column: "CodeQuestionId");
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "CodeConstraints");
		}
	}
}
