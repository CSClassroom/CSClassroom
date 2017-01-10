using Microsoft.EntityFrameworkCore.Migrations;

namespace CSC.CSClassroom.Repository.Migrations
{
	public partial class Migration12 : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropIndex(
				name: "IX_Commits_ProjectId_Sha",
				table: "Commits");

			migrationBuilder.CreateIndex(
				name: "IX_Commits_ProjectId_UserId_Sha",
				table: "Commits",
				columns: new[] { "ProjectId", "UserId", "Sha" },
				unique: true);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropIndex(
				name: "IX_Commits_ProjectId_UserId_Sha",
				table: "Commits");

			migrationBuilder.CreateIndex(
				name: "IX_Commits_ProjectId_Sha",
				table: "Commits",
				columns: new[] { "ProjectId", "Sha" },
				unique: true);
		}
	}
}
