using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace CSC.CSClassroom.Repository.Migrations
{
	public partial class Migration1 : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "Classrooms",
				columns: table => new
				{
					Id = table.Column<int>(nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
					DisplayName = table.Column<string>(nullable: false),
					GitHubOrganization = table.Column<string>(nullable: false),
					Name = table.Column<string>(maxLength: 50, nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Classrooms", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "Users",
				columns: table => new
				{
					Id = table.Column<int>(nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
					EmailAddress = table.Column<string>(nullable: true),
					EmailAddressConfirmed = table.Column<bool>(nullable: false),
					EmailConfirmationCode = table.Column<string>(nullable: true),
					FirstName = table.Column<string>(nullable: true),
					GitHubLogin = table.Column<string>(nullable: true),
					LastName = table.Column<string>(nullable: true),
					SuperUser = table.Column<bool>(nullable: false),
					UniqueId = table.Column<string>(nullable: true),
					UserName = table.Column<string>(nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Users", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "Sections",
				columns: table => new
				{
					Id = table.Column<int>(nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
					AllowNewRegistrations = table.Column<bool>(nullable: false),
					ClassroomId = table.Column<int>(nullable: false),
					DisplayName = table.Column<string>(nullable: false),
					Name = table.Column<string>(maxLength: 50, nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Sections", x => x.Id);
					table.ForeignKey(
						name: "FK_Sections_Classrooms_ClassroomId",
						column: x => x.ClassroomId,
						principalTable: "Classrooms",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "QuestionCategories",
				columns: table => new
				{
					Id = table.Column<int>(nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
					ClassroomId = table.Column<int>(nullable: false),
					Name = table.Column<string>(maxLength: 50, nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_QuestionCategories", x => x.Id);
					table.ForeignKey(
						name: "FK_QuestionCategories_Classrooms_ClassroomId",
						column: x => x.ClassroomId,
						principalTable: "Classrooms",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "ClassroomMemberships",
				columns: table => new
				{
					Id = table.Column<int>(nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
					ClassroomId = table.Column<int>(nullable: false),
					GitHubTeam = table.Column<string>(nullable: true),
					InGitHubOrganization = table.Column<bool>(nullable: false),
					Role = table.Column<int>(nullable: false),
					UserId = table.Column<int>(nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_ClassroomMemberships", x => x.Id);
					table.ForeignKey(
						name: "FK_ClassroomMemberships_Classrooms_ClassroomId",
						column: x => x.ClassroomId,
						principalTable: "Classrooms",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "FK_ClassroomMemberships_Users_UserId",
						column: x => x.UserId,
						principalTable: "Users",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "Questions",
				columns: table => new
				{
					Id = table.Column<int>(nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
					Description = table.Column<string>(nullable: false),
					Discriminator = table.Column<string>(nullable: false),
					Name = table.Column<string>(maxLength: 50, nullable: false),
					QuestionCategoryId = table.Column<int>(nullable: false),
					AllowPublicFields = table.Column<bool>(nullable: true),
					ClassName = table.Column<string>(nullable: true),
					FileTemplate = table.Column<string>(nullable: true),
					InitialSubmission = table.Column<string>(nullable: true),
					GeneratorContents = table.Column<string>(nullable: true),
					MethodName = table.Column<string>(nullable: true),
					ParameterTypes = table.Column<string>(nullable: true),
					ReturnType = table.Column<string>(nullable: true),
					AllowMultipleCorrectAnswers = table.Column<bool>(nullable: true),
					RandomizeAnswers = table.Column<bool>(nullable: true),
					ProgramClassName = table.Column<string>(nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Questions", x => x.Id);
					table.ForeignKey(
						name: "FK_Questions_QuestionCategories_QuestionCategoryId",
						column: x => x.QuestionCategoryId,
						principalTable: "QuestionCategories",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "SectionMemberships",
				columns: table => new
				{
					Id = table.Column<int>(nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
					ClassroomMembershipId = table.Column<int>(nullable: false),
					Role = table.Column<int>(nullable: false),
					SectionId = table.Column<int>(nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_SectionMemberships", x => x.Id);
					table.ForeignKey(
						name: "FK_SectionMemberships_ClassroomMemberships_ClassroomMembershipId",
						column: x => x.ClassroomMembershipId,
						principalTable: "ClassroomMemberships",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "CodeQuestionTests",
				columns: table => new
				{
					Id = table.Column<int>(nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
					Discriminator = table.Column<string>(nullable: false),
					ExpectedOutput = table.Column<string>(nullable: true),
					ExpectedReturnValue = table.Column<string>(nullable: true),
					Name = table.Column<string>(nullable: false),
					Order = table.Column<int>(nullable: false),
					ClassQuestionId = table.Column<int>(nullable: true),
					Description = table.Column<string>(nullable: true),
					MethodBody = table.Column<string>(nullable: true),
					ReturnType = table.Column<string>(nullable: true),
					MethodQuestionId = table.Column<int>(nullable: true),
					ParameterValues = table.Column<string>(nullable: true),
					CommandLineArguments = table.Column<string>(nullable: true),
					ProgramQuestionId = table.Column<int>(nullable: true),
					TestDescription = table.Column<string>(nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_CodeQuestionTests", x => x.Id);
					table.ForeignKey(
						name: "FK_CodeQuestionTests_Questions_ClassQuestionId",
						column: x => x.ClassQuestionId,
						principalTable: "Questions",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "FK_CodeQuestionTests_Questions_MethodQuestionId",
						column: x => x.MethodQuestionId,
						principalTable: "Questions",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "FK_CodeQuestionTests_Questions_ProgramQuestionId",
						column: x => x.ProgramQuestionId,
						principalTable: "Questions",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "ImportedClasses",
				columns: table => new
				{
					Id = table.Column<int>(nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
					ClassName = table.Column<string>(nullable: false),
					CodeQuestionId = table.Column<int>(nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_ImportedClasses", x => x.Id);
					table.ForeignKey(
						name: "FK_ImportedClasses_Questions_CodeQuestionId",
						column: x => x.CodeQuestionId,
						principalTable: "Questions",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "MultipleChoiceQuestionChoices",
				columns: table => new
				{
					Id = table.Column<int>(nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
					Correct = table.Column<bool>(nullable: false),
					MultipleChoiceQuestionId = table.Column<int>(nullable: false),
					Order = table.Column<int>(nullable: false),
					Value = table.Column<string>(nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_MultipleChoiceQuestionChoices", x => x.Id);
					table.ForeignKey(
						name: "FK_MultipleChoiceQuestionChoices_Questions_MultipleChoiceQuestionId",
						column: x => x.MultipleChoiceQuestionId,
						principalTable: "Questions",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "RequiredMethods",
				columns: table => new
				{
					Id = table.Column<int>(nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
					ClassQuestionId = table.Column<int>(nullable: false),
					IsPublic = table.Column<bool>(nullable: false),
					IsStatic = table.Column<bool>(nullable: false),
					Name = table.Column<string>(nullable: true),
					ParamTypes = table.Column<string>(nullable: true),
					ReturnType = table.Column<string>(nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_RequiredMethods", x => x.Id);
					table.ForeignKey(
						name: "FK_RequiredMethods_Questions_ClassQuestionId",
						column: x => x.ClassQuestionId,
						principalTable: "Questions",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "ShortAnswerQuestionBlanks",
				columns: table => new
				{
					Id = table.Column<int>(nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
					Answer = table.Column<string>(nullable: true),
					Name = table.Column<string>(nullable: true),
					Order = table.Column<int>(nullable: false),
					ShortAnswerQuestionId = table.Column<int>(nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_ShortAnswerQuestionBlanks", x => x.Id);
					table.ForeignKey(
						name: "FK_ShortAnswerQuestionBlanks_Questions_ShortAnswerQuestionId",
						column: x => x.ShortAnswerQuestionId,
						principalTable: "Questions",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateIndex(
				name: "IX_Sections_ClassroomId",
				table: "Sections",
				column: "ClassroomId");

			migrationBuilder.CreateIndex(
				name: "IX_CodeQuestionTests_ClassQuestionId",
				table: "CodeQuestionTests",
				column: "ClassQuestionId");

			migrationBuilder.CreateIndex(
				name: "IX_CodeQuestionTests_MethodQuestionId",
				table: "CodeQuestionTests",
				column: "MethodQuestionId");

			migrationBuilder.CreateIndex(
				name: "IX_CodeQuestionTests_ProgramQuestionId",
				table: "CodeQuestionTests",
				column: "ProgramQuestionId");

			migrationBuilder.CreateIndex(
				name: "IX_ImportedClasses_CodeQuestionId",
				table: "ImportedClasses",
				column: "CodeQuestionId");

			migrationBuilder.CreateIndex(
				name: "IX_MultipleChoiceQuestionChoices_MultipleChoiceQuestionId",
				table: "MultipleChoiceQuestionChoices",
				column: "MultipleChoiceQuestionId");

			migrationBuilder.CreateIndex(
				name: "IX_Questions_QuestionCategoryId",
				table: "Questions",
				column: "QuestionCategoryId");

			migrationBuilder.CreateIndex(
				name: "IX_QuestionCategories_ClassroomId",
				table: "QuestionCategories",
				column: "ClassroomId");

			migrationBuilder.CreateIndex(
				name: "IX_RequiredMethods_ClassQuestionId",
				table: "RequiredMethods",
				column: "ClassQuestionId");

			migrationBuilder.CreateIndex(
				name: "IX_ShortAnswerQuestionBlanks_ShortAnswerQuestionId",
				table: "ShortAnswerQuestionBlanks",
				column: "ShortAnswerQuestionId");

			migrationBuilder.CreateIndex(
				name: "IX_ClassroomMemberships_ClassroomId",
				table: "ClassroomMemberships",
				column: "ClassroomId");

			migrationBuilder.CreateIndex(
				name: "IX_ClassroomMemberships_UserId",
				table: "ClassroomMemberships",
				column: "UserId");

			migrationBuilder.CreateIndex(
				name: "IX_SectionMemberships_ClassroomMembershipId",
				table: "SectionMemberships",
				column: "ClassroomMembershipId");
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "Sections");

			migrationBuilder.DropTable(
				name: "CodeQuestionTests");

			migrationBuilder.DropTable(
				name: "ImportedClasses");

			migrationBuilder.DropTable(
				name: "MultipleChoiceQuestionChoices");

			migrationBuilder.DropTable(
				name: "RequiredMethods");

			migrationBuilder.DropTable(
				name: "ShortAnswerQuestionBlanks");

			migrationBuilder.DropTable(
				name: "SectionMemberships");

			migrationBuilder.DropTable(
				name: "Questions");

			migrationBuilder.DropTable(
				name: "ClassroomMemberships");

			migrationBuilder.DropTable(
				name: "QuestionCategories");

			migrationBuilder.DropTable(
				name: "Users");

			migrationBuilder.DropTable(
				name: "Classrooms");
		}
	}
}
