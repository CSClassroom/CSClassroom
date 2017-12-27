using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace CSC.CSClassroom.Repository.Migrations
{
    public partial class Migration29 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "DailyLatenessDeduction",
                table: "Classrooms",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MaxLatenessDeduction",
                table: "Classrooms",
                nullable: false,
                defaultValue: 0.0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DailyLatenessDeduction",
                table: "Classrooms");

            migrationBuilder.DropColumn(
                name: "MaxLatenessDeduction",
                table: "Classrooms");
        }
    }
}
