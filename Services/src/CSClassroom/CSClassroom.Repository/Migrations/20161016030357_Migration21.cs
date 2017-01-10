using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CSC.CSClassroom.Repository.Migrations
{
	public partial class Migration21 : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "CheckpointTestClasses",
				columns: table => new
				{
					Id = table.Column<int>(nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
					CheckpointId = table.Column<int>(nullable: false),
					Required = table.Column<bool>(nullable: false),
					TestClassId = table.Column<int>(nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_CheckpointTestClasses", x => x.Id);
					table.ForeignKey(
						name: "FK_CheckpointTestClasses_Checkpoints_CheckpointId",
						column: x => x.CheckpointId,
						principalTable: "Checkpoints",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "FK_CheckpointTestClasses_TestClasses_TestClassId",
						column: x => x.TestClassId,
						principalTable: "TestClasses",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateIndex(
				name: "IX_CheckpointTestClasses_CheckpointId",
				table: "CheckpointTestClasses",
				column: "CheckpointId");

			migrationBuilder.CreateIndex(
				name: "IX_CheckpointTestClasses_TestClassId",
				table: "CheckpointTestClasses",
				column: "TestClassId");

			migrationBuilder.CreateIndex(
				name: "IX_CheckpointTestClasses_CheckpointId_TestClassId",
				table: "CheckpointTestClasses",
				columns: new[] { "CheckpointId", "TestClassId" },
				unique: true);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "CheckpointTestClasses");
		}
	}
}
