using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace CSC.CSClassroom.Repository.Migrations
{
	public partial class Migration3 : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropUniqueConstraint(
				name: "AK_Users_UniqueId",
				table: "Users");

			migrationBuilder.DropUniqueConstraint(
				name: "AK_SectionMemberships_ClassroomMembershipId_SectionId",
				table: "SectionMemberships");

			migrationBuilder.DropUniqueConstraint(
				name: "AK_ClassroomMemberships_UserId_ClassroomId",
				table: "ClassroomMemberships");

			migrationBuilder.DropUniqueConstraint(
				name: "AK_QuestionCategories_ClassroomId_Name",
				table: "QuestionCategories");

			migrationBuilder.DropUniqueConstraint(
				name: "AK_Questions_QuestionCategoryId_Name",
				table: "Questions");

			migrationBuilder.DropUniqueConstraint(
				name: "AK_Sections_ClassroomId_Name",
				table: "Sections");

			migrationBuilder.DropUniqueConstraint(
				name: "AK_Classrooms_Name",
				table: "Classrooms");

			migrationBuilder.DropColumn(
				name: "RandomizeAnswers",
				table: "Questions");

			migrationBuilder.CreateTable(
				name: "UserQuestionData",
				columns: table => new
				{
					Id = table.Column<int>(nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
					CachedQuestionData = table.Column<string>(nullable: true),
					CachedQuestionDataTime = table.Column<DateTime>(nullable: true),
					LastQuestionSubmission = table.Column<string>(nullable: true),
					NumAttempts = table.Column<int>(nullable: false),
					QuestionId = table.Column<int>(nullable: false),
					Seed = table.Column<int>(nullable: true),
					UserId = table.Column<int>(nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_UserQuestionData", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "UserQuestionSubmission",
				columns: table => new
				{
					Id = table.Column<int>(nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
					DateSubmitted = table.Column<DateTime>(nullable: false),
					Score = table.Column<double>(nullable: false),
					UserQuestionDataId = table.Column<int>(nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_UserQuestionSubmission", x => x.Id);
					table.ForeignKey(
						name: "FK_UserQuestionSubmission_UserQuestionData_UserQuestionDataId",
						column: x => x.UserQuestionDataId,
						principalTable: "UserQuestionData",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.AddColumn<bool>(
				name: "IsPrivate",
				table: "QuestionCategories",
				nullable: false,
				defaultValue: false);

			migrationBuilder.AddColumn<bool>(
				name: "AllowPartialCredit",
				table: "Questions",
				nullable: false,
				defaultValue: false);

			migrationBuilder.AddColumn<bool>(
				name: "IsPrivate",
				table: "Questions",
				nullable: false,
				defaultValue: false);

			migrationBuilder.AddColumn<DateTime>(
				name: "DateModified",
				table: "Questions",
				nullable: true);

			migrationBuilder.AddColumn<string>(
				name: "FullGeneratorFileContents",
				table: "Questions",
				nullable: true);

			migrationBuilder.AddColumn<int>(
				name: "FullGeneratorFileLineOffset",
				table: "Questions",
				nullable: true);

			migrationBuilder.AlterColumn<string>(
				name: "UniqueId",
				table: "Users",
				nullable: true);

			migrationBuilder.CreateIndex(
				name: "IX_Users_UniqueId",
				table: "Users",
				column: "UniqueId",
				unique: true);

			migrationBuilder.CreateIndex(
				name: "IX_SectionMemberships_SectionId",
				table: "SectionMemberships",
				column: "SectionId");

			migrationBuilder.CreateIndex(
				name: "IX_SectionMemberships_ClassroomMembershipId_SectionId",
				table: "SectionMemberships",
				columns: new[] { "ClassroomMembershipId", "SectionId" },
				unique: true);

			migrationBuilder.CreateIndex(
				name: "IX_ClassroomMemberships_UserId_ClassroomId",
				table: "ClassroomMemberships",
				columns: new[] { "UserId", "ClassroomId" },
				unique: true);

			migrationBuilder.CreateIndex(
				name: "IX_QuestionCategories_ClassroomId_Name",
				table: "QuestionCategories",
				columns: new[] { "ClassroomId", "Name" },
				unique: true);

			migrationBuilder.CreateIndex(
				name: "IX_Questions_QuestionCategoryId_Name",
				table: "Questions",
				columns: new[] { "QuestionCategoryId", "Name" },
				unique: true);

			migrationBuilder.CreateIndex(
				name: "IX_Sections_ClassroomId_Name",
				table: "Sections",
				columns: new[] { "ClassroomId", "Name" },
				unique: true);

			migrationBuilder.CreateIndex(
				name: "IX_Classrooms_Name",
				table: "Classrooms",
				column: "Name",
				unique: true);

			migrationBuilder.CreateIndex(
				name: "IX_UserQuestionData_QuestionId_UserId",
				table: "UserQuestionData",
				columns: new[] { "QuestionId", "UserId" },
				unique: true);

			migrationBuilder.CreateIndex(
				name: "IX_UserQuestionSubmission_UserQuestionDataId",
				table: "UserQuestionSubmission",
				column: "UserQuestionDataId");

			migrationBuilder.AddForeignKey(
				name: "FK_SectionMemberships_Sections_SectionId",
				table: "SectionMemberships",
				column: "SectionId",
				principalTable: "Sections",
				principalColumn: "Id",
				onDelete: ReferentialAction.Cascade);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropForeignKey(
				name: "FK_SectionMemberships_Sections_SectionId",
				table: "SectionMemberships");

			migrationBuilder.DropIndex(
				name: "IX_Users_UniqueId",
				table: "Users");

			migrationBuilder.DropIndex(
				name: "IX_SectionMemberships_SectionId",
				table: "SectionMemberships");

			migrationBuilder.DropIndex(
				name: "IX_SectionMemberships_ClassroomMembershipId_SectionId",
				table: "SectionMemberships");

			migrationBuilder.DropIndex(
				name: "IX_ClassroomMemberships_UserId_ClassroomId",
				table: "ClassroomMemberships");

			migrationBuilder.DropIndex(
				name: "IX_QuestionCategories_ClassroomId_Name",
				table: "QuestionCategories");

			migrationBuilder.DropIndex(
				name: "IX_Questions_QuestionCategoryId_Name",
				table: "Questions");

			migrationBuilder.DropIndex(
				name: "IX_Sections_ClassroomId_Name",
				table: "Sections");

			migrationBuilder.DropIndex(
				name: "IX_Classrooms_Name",
				table: "Classrooms");

			migrationBuilder.DropColumn(
				name: "IsPrivate",
				table: "QuestionCategories");

			migrationBuilder.DropColumn(
				name: "AllowPartialCredit",
				table: "Questions");

			migrationBuilder.DropColumn(
				name: "IsPrivate",
				table: "Questions");

			migrationBuilder.DropColumn(
				name: "DateModified",
				table: "Questions");

			migrationBuilder.DropColumn(
				name: "FullGeneratorFileContents",
				table: "Questions");

			migrationBuilder.DropColumn(
				name: "FullGeneratorFileLineOffset",
				table: "Questions");

			migrationBuilder.DropTable(
				name: "UserQuestionSubmission");

			migrationBuilder.DropTable(
				name: "UserQuestionData");

			migrationBuilder.AddColumn<bool>(
				name: "RandomizeAnswers",
				table: "Questions",
				nullable: true);

			migrationBuilder.AlterColumn<string>(
				name: "UniqueId",
				table: "Users",
				nullable: false);

			migrationBuilder.AddUniqueConstraint(
				name: "AK_Users_UniqueId",
				table: "Users",
				column: "UniqueId");

			migrationBuilder.AddUniqueConstraint(
				name: "AK_SectionMemberships_ClassroomMembershipId_SectionId",
				table: "SectionMemberships",
				columns: new[] { "ClassroomMembershipId", "SectionId" });

			migrationBuilder.AddUniqueConstraint(
				name: "AK_ClassroomMemberships_UserId_ClassroomId",
				table: "ClassroomMemberships",
				columns: new[] { "UserId", "ClassroomId" });

			migrationBuilder.AddUniqueConstraint(
				name: "AK_QuestionCategories_ClassroomId_Name",
				table: "QuestionCategories",
				columns: new[] { "ClassroomId", "Name" });

			migrationBuilder.AddUniqueConstraint(
				name: "AK_Questions_QuestionCategoryId_Name",
				table: "Questions",
				columns: new[] { "QuestionCategoryId", "Name" });

			migrationBuilder.AddUniqueConstraint(
				name: "AK_Sections_ClassroomId_Name",
				table: "Sections",
				columns: new[] { "ClassroomId", "Name" });

			migrationBuilder.AddUniqueConstraint(
				name: "AK_Classrooms_Name",
				table: "Classrooms",
				column: "Name");
		}
	}
}
