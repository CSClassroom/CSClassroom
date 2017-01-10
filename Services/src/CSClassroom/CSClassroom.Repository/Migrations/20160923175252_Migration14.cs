using Microsoft.EntityFrameworkCore.Migrations;

namespace CSC.CSClassroom.Repository.Migrations
{
	public partial class Migration14 : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateIndex(
				name: "IX_UserQuestionData_QuestionId",
				table: "UserQuestionData",
				column: "QuestionId");

			migrationBuilder.CreateIndex(
				name: "IX_UserQuestionData_UserId",
				table: "UserQuestionData",
				column: "UserId");

			migrationBuilder.AddForeignKey(
				name: "FK_UserQuestionData_Questions_QuestionId",
				table: "UserQuestionData",
				column: "QuestionId",
				principalTable: "Questions",
				principalColumn: "Id",
				onDelete: ReferentialAction.Cascade);

			migrationBuilder.AddForeignKey(
				name: "FK_UserQuestionData_Users_UserId",
				table: "UserQuestionData",
				column: "UserId",
				principalTable: "Users",
				principalColumn: "Id",
				onDelete: ReferentialAction.Cascade);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropForeignKey(
				name: "FK_UserQuestionData_Questions_QuestionId",
				table: "UserQuestionData");

			migrationBuilder.DropForeignKey(
				name: "FK_UserQuestionData_Users_UserId",
				table: "UserQuestionData");

			migrationBuilder.DropIndex(
				name: "IX_UserQuestionData_QuestionId",
				table: "UserQuestionData");

			migrationBuilder.DropIndex(
				name: "IX_UserQuestionData_UserId",
				table: "UserQuestionData");
		}
	}
}
