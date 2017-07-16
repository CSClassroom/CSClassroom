using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CSC.CSClassroom.Repository.Migrations
{
	public partial class Migration24 : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropForeignKey(
				name: "FK_UserQuestionData_Questions_QuestionId",
				table: "UserQuestionData");

			migrationBuilder.DropIndex(
				name: "IX_QuestionCategories_ClassroomId_Name",
				table: "QuestionCategories");

			migrationBuilder.DropIndex(
				name: "IX_AssignmentQuestions_AssignmentId_QuestionId",
				table: "AssignmentQuestions");

			migrationBuilder.DropColumn(
				name: "IsPrivate",
				table: "QuestionCategories");

			migrationBuilder.DropColumn(
				name: "IsPrivate",
				table: "Questions");

			migrationBuilder.AddColumn<int>(
				name: "AssignmentQuestionId",
				table: "UserQuestionData",
				defaultValue: 0);

			migrationBuilder.DropIndex(
				name: "IX_UserQuestionData_QuestionId_UserId",
				table: "UserQuestionData");

			migrationBuilder.AlterColumn<int>(
				name: "QuestionId",
				table: "UserQuestionData",
				nullable: true);

			migrationBuilder.Sql(_populateAssignmentQuestionId);
			migrationBuilder.Sql(_removeUserQuestionDataWithoutAssignmentQuestions);
			migrationBuilder.Sql(_copyUserQuestionDataToOtherAssignmentQuestions);
			migrationBuilder.Sql(_copyUserQuestionSubmissionsToOtherAssignmentQuestions);

			migrationBuilder.DropColumn(
				name: "QuestionId",
				table: "UserQuestionData");

			migrationBuilder.CreateIndex(
				name: "IX_UserQuestionData_AssignmentQuestionId_UserId",
				table: "UserQuestionData",
				columns: new[] { "AssignmentQuestionId", "UserId" },
				unique: true);

			migrationBuilder.AddColumn<string>(
				name: "CachedQuestionData",
				table: "UserQuestionSubmission",
				nullable: true);

			migrationBuilder.AddColumn<int>(
				name: "Seed",
				table: "UserQuestionSubmission",
				nullable: true);

			migrationBuilder.AddColumn<string>(
				name: "SubmissionContents",
				table: "UserQuestionSubmission",
				nullable: true);

			migrationBuilder.AlterColumn<string>(
				name: "Name",
				table: "QuestionCategories",
				maxLength: 100,
				nullable: false,
				oldClrType: typeof(string),
				oldMaxLength: 50);

			migrationBuilder.AddColumn<int>(
				name: "RandomlySelectedQuestionId",
				table: "QuestionCategories",
				nullable: true);

			migrationBuilder.AddColumn<int>(
				name: "NumSeeds",
				table: "Questions",
				nullable: true);

			migrationBuilder.AddColumn<string>(
				name: "Explanation",
				table: "Questions",
				nullable: true);

			migrationBuilder.AddColumn<string>(
				name: "Explanation",
				table: "MultipleChoiceQuestionChoices",
				nullable: true);

			migrationBuilder.AddColumn<string>(
				name: "Name",
				table: "AssignmentQuestions",
				nullable: true);

			migrationBuilder.AlterColumn<string>(
				name: "Name",
				table: "Assignments",
				maxLength: 100,
				nullable: false,
				oldClrType: typeof(string),
				oldMaxLength: 50);

			migrationBuilder.AlterColumn<string>(
				name: "GroupName",
				table: "Assignments",
				maxLength: 100,
				nullable: true,
				oldClrType: typeof(string),
				oldMaxLength: 50,
				oldNullable: true);

			migrationBuilder.Sql(_populateAssignmentQuestionName);

			migrationBuilder.AddColumn<bool>(
				name: "AnswerInOrder",
				table: "Assignments",
				nullable: false,
				defaultValue: false);

			migrationBuilder.AddColumn<bool>(
				name: "CombinedSubmissions",
				table: "Assignments",
				nullable: false,
				defaultValue: false);

			migrationBuilder.AddColumn<bool>(
				name: "IsPrivate",
				table: "Assignments",
				nullable: false,
				defaultValue: false);

			migrationBuilder.AddColumn<int>(
				name: "MaxAttempts",
				table: "Assignments",
				nullable: true);

			migrationBuilder.AddColumn<bool>(
				name: "OnlyShowCombinedScore",
				table: "Assignments",
				nullable: false,
				defaultValue: false);

			migrationBuilder.CreateIndex(
				name: "IX_QuestionCategories_RandomlySelectedQuestionId",
				table: "QuestionCategories",
				column: "RandomlySelectedQuestionId",
				unique: true);

			migrationBuilder.CreateIndex(
				name: "IX_QuestionCategories_ClassroomId_Name_RandomlySelectedQuestionId",
				table: "QuestionCategories",
				columns: new[] { "ClassroomId", "Name", "RandomlySelectedQuestionId" },
				unique: true);

			migrationBuilder.CreateIndex(
				name: "IX_AssignmentQuestions_AssignmentId_Name",
				table: "AssignmentQuestions",
				columns: new[] { "AssignmentId", "Name" },
				unique: true);

			migrationBuilder.AddForeignKey(
				name: "FK_QuestionCategories_Questions_RandomlySelectedQuestionId",
				table: "QuestionCategories",
				column: "RandomlySelectedQuestionId",
				principalTable: "Questions",
				principalColumn: "Id",
				onDelete: ReferentialAction.Cascade);

			migrationBuilder.AddForeignKey(
				name: "FK_UserQuestionData_AssignmentQuestions_AssignmentQuestionId",
				table: "UserQuestionData",
				column: "AssignmentQuestionId",
				principalTable: "AssignmentQuestions",
				principalColumn: "Id",
				onDelete: ReferentialAction.Cascade);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropForeignKey(
				name: "FK_QuestionCategories_Questions_RandomlySelectedQuestionId",
				table: "QuestionCategories");

			migrationBuilder.DropForeignKey(
				name: "FK_UserQuestionData_AssignmentQuestions_AssignmentQuestionId",
				table: "UserQuestionData");

			migrationBuilder.DropIndex(
				name: "IX_QuestionCategories_RandomlySelectedQuestionId",
				table: "QuestionCategories");

			migrationBuilder.DropIndex(
				name: "IX_QuestionCategories_ClassroomId_Name_RandomlySelectedQuestionId",
				table: "QuestionCategories");

			migrationBuilder.DropIndex(
				name: "IX_AssignmentQuestions_AssignmentId_Name",
				table: "AssignmentQuestions");

			migrationBuilder.DropColumn(
				name: "CachedQuestionData",
				table: "UserQuestionSubmission");

			migrationBuilder.DropColumn(
				name: "Seed",
				table: "UserQuestionSubmission");

			migrationBuilder.DropColumn(
				name: "SubmissionContents",
				table: "UserQuestionSubmission");

			migrationBuilder.DropColumn(
				name: "RandomlySelectedQuestionId",
				table: "QuestionCategories");

			migrationBuilder.DropColumn(
				name: "NumSeeds",
				table: "Questions");

			migrationBuilder.DropColumn(
				name: "Explanation",
				table: "Questions");

			migrationBuilder.DropColumn(
				name: "Explanation",
				table: "MultipleChoiceQuestionChoices");

			migrationBuilder.DropColumn(
				name: "Name",
				table: "AssignmentQuestions");

			migrationBuilder.DropColumn(
				name: "AnswerInOrder",
				table: "Assignments");

			migrationBuilder.DropColumn(
				name: "CombinedSubmissions",
				table: "Assignments");

			migrationBuilder.DropColumn(
				name: "IsPrivate",
				table: "Assignments");

			migrationBuilder.DropColumn(
				name: "MaxAttempts",
				table: "Assignments");

			migrationBuilder.DropColumn(
				name: "OnlyShowCombinedScore",
				table: "Assignments");

			migrationBuilder.AddColumn<int>(
				name: "QuestionId",
				table: "UserQuestionData",
				defaultValue: 0);

			migrationBuilder.DropIndex(
				name: "IX_UserQuestionData_AssignmentQuestionId_UserId",
				table: "UserQuestionData");

			migrationBuilder.Sql(_populateQuestionId);
			migrationBuilder.Sql(_removeDuplicateUserQuestionData);

			migrationBuilder.DropColumn(
				name: "AssignmentQuestionId",
				table: "UserQuestionData");

			migrationBuilder.CreateIndex(
				name: "IX_UserQuestionData_QuestionId_UserId",
				table: "UserQuestionData",
				columns: new[] { "QuestionId", "UserId" },
				unique: true);

			migrationBuilder.AlterColumn<string>(
				name: "Name",
				table: "QuestionCategories",
				maxLength: 50,
				nullable: false,
				oldClrType: typeof(string),
				oldMaxLength: 100);

			migrationBuilder.AddColumn<bool>(
				name: "IsPrivate",
				table: "QuestionCategories",
				nullable: false,
				defaultValue: false);

			migrationBuilder.AddColumn<bool>(
				name: "IsPrivate",
				table: "Questions",
				nullable: false,
				defaultValue: false);

			migrationBuilder.AlterColumn<string>(
				name: "Name",
				table: "Assignments",
				maxLength: 50,
				nullable: false,
				oldClrType: typeof(string),
				oldMaxLength: 100);

			migrationBuilder.AlterColumn<string>(
				name: "GroupName",
				table: "Assignments",
				maxLength: 50,
				nullable: true,
				oldClrType: typeof(string),
				oldMaxLength: 100,
				oldNullable: true);

			migrationBuilder.CreateIndex(
				name: "IX_QuestionCategories_ClassroomId_Name",
				table: "QuestionCategories",
				columns: new[] { "ClassroomId", "Name" },
				unique: true);

			migrationBuilder.CreateIndex(
				name: "IX_AssignmentQuestions_AssignmentId_QuestionId",
				table: "AssignmentQuestions",
				columns: new[] { "AssignmentId", "QuestionId" },
				unique: true);

			migrationBuilder.AddForeignKey(
				name: "FK_UserQuestionData_Questions_QuestionId",
				table: "UserQuestionData",
				column: "QuestionId",
				principalTable: "Questions",
				principalColumn: "Id",
				onDelete: ReferentialAction.Cascade);
		}

		private const string _populateAssignmentQuestionName = @" 
			UPDATE ""AssignmentQuestions"" SET ""Name"" = ""Questions"".""Name""
			FROM ""Questions""
			WHERE ""AssignmentQuestions"".""QuestionId"" = ""Questions"".""Id""";

		private const string _populateAssignmentQuestionId = @" 
			UPDATE ""UserQuestionData"" SET ""AssignmentQuestionId"" = ""AssignmentQuestions"".""Id""
			FROM ""AssignmentQuestions""
			WHERE ""UserQuestionData"".""QuestionId"" = ""AssignmentQuestions"".""QuestionId""";

		private const string _removeUserQuestionDataWithoutAssignmentQuestions = @"
			DELETE FROM ""UserQuestionData"" WHERE ""AssignmentQuestionId"" = 0";

		private const string _copyUserQuestionDataToOtherAssignmentQuestions = @"
			INSERT INTO ""UserQuestionData"" (""CachedQuestionData"", ""CachedQuestionDataTime"", ""LastQuestionSubmission"", ""NumAttempts"", ""Seed"", ""UserId"", ""AssignmentQuestionId"") (
				SELECT  ""ActualUserQuestionData"".""CachedQuestionData"", 
						""ActualUserQuestionData"".""CachedQuestionDataTime"", 
						""ActualUserQuestionData"".""LastQuestionSubmission"", 
						""ActualUserQuestionData"".""NumAttempts"", 
						""ActualUserQuestionData"".""Seed"", 
						""ActualUserQuestionData"".""UserId"", 
						""AllAssignmentQuestions"".""Id""
				FROM ""UserQuestionData"" AS ""ActualUserQuestionData""
				INNER JOIN ""AssignmentQuestions"" AS ""AssignmentQuestionsWithData"" ON ""AssignmentQuestionsWithData"".""Id"" = ""ActualUserQuestionData"".""AssignmentQuestionId""
				INNER JOIN ""AssignmentQuestions"" AS ""AllAssignmentQuestions"" ON ""AssignmentQuestionsWithData"".""QuestionId"" = ""AllAssignmentQuestions"".""QuestionId""
				LEFT OUTER JOIN ""UserQuestionData"" AS ""HypotheticalUserQuestionData"" ON ""HypotheticalUserQuestionData"".""AssignmentQuestionId"" = ""AllAssignmentQuestions"".""Id"" AND ""ActualUserQuestionData"".""UserId"" = ""HypotheticalUserQuestionData"".""UserId""
				WHERE ""HypotheticalUserQuestionData"".""Id"" IS null
			)";

		private const string _copyUserQuestionSubmissionsToOtherAssignmentQuestions = @"
			INSERT INTO ""UserQuestionSubmission"" (""DateSubmitted"", ""Score"", ""UserQuestionDataId"") (
				SELECT  ""ActualUserQuestionSubmission"".""DateSubmitted"", 
						""ActualUserQuestionSubmission"".""Score"", 
						""AllUserQuestionData"".""Id""
				FROM ""UserQuestionSubmission"" AS ""ActualUserQuestionSubmission""
				INNER JOIN ""UserQuestionData"" AS ""UserQuestionDataWithSubmission"" ON ""UserQuestionDataWithSubmission"".""Id"" = ""ActualUserQuestionSubmission"".""UserQuestionDataId""
				INNER JOIN ""AssignmentQuestions"" AS ""AssignmentQuestionsWithData"" ON ""AssignmentQuestionsWithData"".""Id"" = ""UserQuestionDataWithSubmission"".""AssignmentQuestionId""
				INNER JOIN ""AssignmentQuestions"" AS ""AllAssignmentQuestions"" ON ""AssignmentQuestionsWithData"".""QuestionId"" = ""AllAssignmentQuestions"".""QuestionId""
				INNER JOIN ""UserQuestionData"" AS ""AllUserQuestionData"" ON ""AllAssignmentQuestions"".""Id"" = ""AllUserQuestionData"".""AssignmentQuestionId"" AND ""UserQuestionDataWithSubmission"".""UserId"" = ""AllUserQuestionData"".""UserId""
				LEFT OUTER JOIN ""UserQuestionSubmission"" AS ""HypotheticalUserQuestionSubmission"" ON ""HypotheticalUserQuestionSubmission"".""UserQuestionDataId"" = ""AllUserQuestionData"".""Id""
				WHERE ""HypotheticalUserQuestionSubmission"".""Id"" IS null
			)";

		private const string _populateQuestionId = @"
			UPDATE ""UserQuestionData"" SET ""QuestionId"" = ""AssignmentQuestions"".""QuestionId""
			FROM ""AssignmentQuestions""
			WHERE ""UserQuestionData"".""AssignmentQuestionId"" = ""AssignmentQuestions"".""Id""";

		private const string _removeDuplicateUserQuestionData = @"
			DELETE FROM ""UserQuestionData""
			WHERE ""Id"" IN (
				SELECT ""Id"" FROM (
					SELECT ""Id"", 
							ROW_NUMBER() OVER (
								partition BY ""UserId"", ""QuestionId"" ORDER BY ""Id""
							) AS rnum
					FROM ""UserQuestionData"") t
				WHERE t.rnum > 1
			)";
	}
}
