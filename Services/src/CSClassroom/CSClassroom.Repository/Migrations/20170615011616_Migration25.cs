using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CSC.CSClassroom.Repository.Migrations
{
	public partial class Migration25 : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<bool>(
				name: "Regex",
				table: "ShortAnswerQuestionBlanks",
				nullable: false,
				defaultValue: false);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
				name: "Regex",
				table: "ShortAnswerQuestionBlanks");
		}
	}
}
