using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace CSC.CSClassroom.Repository.Migrations
{
	public partial class Migration6 : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "TestResults",
				columns: table => new
				{
					Id = table.Column<int>(nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
					BuildId = table.Column<int>(nullable: false),
					ClassName = table.Column<string>(nullable: true),
					FailureMessage = table.Column<string>(nullable: true),
					FailureOutput = table.Column<string>(nullable: true),
					FailureTrace = table.Column<string>(nullable: true),
					TestName = table.Column<string>(nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_TestResults", x => x.Id);
					table.ForeignKey(
						name: "FK_TestResults_Build_BuildId",
						column: x => x.BuildId,
						principalTable: "Build",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateIndex(
				name: "IX_TestResults_BuildId",
				table: "TestResults",
				column: "BuildId");

			migrationBuilder.CreateIndex(
				name: "IX_TestResults_BuildId_ClassName_TestName",
				table: "TestResults",
				columns: new[] { "BuildId", "ClassName", "TestName" },
				unique: true);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "TestResults");
		}
	}
}
