using Microsoft.EntityFrameworkCore.Migrations;

namespace CSC.CSClassroom.Repository.Migrations
{
	public partial class Migration9 : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<string>(
				name: "DisplayName",
				table: "TestClasses",
				nullable: true);

			migrationBuilder.CreateIndex(
				name: "IX_Commits_ProjectId",
				table: "Commits",
				column: "ProjectId");

			migrationBuilder.CreateIndex(
				name: "IX_Commits_UserId",
				table: "Commits",
				column: "UserId");

			migrationBuilder.AddForeignKey(
				name: "FK_Commits_Projects_ProjectId",
				table: "Commits",
				column: "ProjectId",
				principalTable: "Projects",
				principalColumn: "Id",
				onDelete: ReferentialAction.Cascade);

			migrationBuilder.AddForeignKey(
				name: "FK_Commits_Users_UserId",
				table: "Commits",
				column: "UserId",
				principalTable: "Users",
				principalColumn: "Id",
				onDelete: ReferentialAction.Cascade);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropForeignKey(
				name: "FK_Commits_Projects_ProjectId",
				table: "Commits");

			migrationBuilder.DropForeignKey(
				name: "FK_Commits_Users_UserId",
				table: "Commits");

			migrationBuilder.DropIndex(
				name: "IX_Commits_ProjectId",
				table: "Commits");

			migrationBuilder.DropIndex(
				name: "IX_Commits_UserId",
				table: "Commits");

			migrationBuilder.DropColumn(
				name: "DisplayName",
				table: "TestClasses");
		}
	}
}
