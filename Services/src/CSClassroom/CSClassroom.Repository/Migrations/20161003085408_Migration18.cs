using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CSC.CSClassroom.Repository.Migrations
{
	public partial class Migration18 : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<DateTime>(
				name: "AssignmentsLastGradedDate",
				table: "Sections",
				nullable: false,
				defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

			migrationBuilder.AddColumn<TimeSpan>(
				name: "DefaultTimeDue",
				table: "Classrooms",
				nullable: false,
				defaultValue: new TimeSpan(0, 0, 0, 0, 0));
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
				name: "AssignmentsLastGradedDate",
				table: "Sections");

			migrationBuilder.DropColumn(
				name: "DefaultTimeDue",
				table: "Classrooms");
		}
	}
}
