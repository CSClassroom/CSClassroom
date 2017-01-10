using Microsoft.EntityFrameworkCore.Migrations;

namespace CSC.CSClassroom.Repository.Migrations
{
	public partial class Migration2 : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
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

		protected override void Down(MigrationBuilder migrationBuilder)
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

			migrationBuilder.AlterColumn<string>(
				name: "UniqueId",
				table: "Users",
				nullable: true);
		}
	}
}
