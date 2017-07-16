using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CSC.CSClassroom.Repository.Migrations
{
	public partial class Migration23 : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AlterColumn<string>(
				name: "Name",
				table: "CodeQuestionTests",
				nullable: true,
				oldClrType: typeof(string));
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AlterColumn<string>(
				name: "Name",
				table: "CodeQuestionTests",
				nullable: false,
				oldClrType: typeof(string),
				oldNullable: true);
		}
	}
}
