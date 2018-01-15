using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace CSC.CSClassroom.Repository.Migrations
{
    public partial class Migration30 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AnnouncementSections_AnnouncementId",
                table: "AnnouncementSections");

            migrationBuilder.AddColumn<bool>(
                name: "AllowStudentMessages",
                table: "Sections",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Conversations",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Actionable = table.Column<bool>(nullable: false),
                    ClassroomId = table.Column<int>(nullable: false),
                    CreatorId = table.Column<int>(nullable: false),
                    OwnerId = table.Column<int>(nullable: true),
                    StudentId = table.Column<int>(nullable: false),
                    Subject = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conversations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Conversations_Classrooms_ClassroomId",
                        column: x => x.ClassroomId,
                        principalTable: "Classrooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Conversations_Users_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Conversations_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Conversations_Users_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SectionRecipients",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    ClassroomMembershipId = table.Column<int>(nullable: false),
                    EmailAnnouncements = table.Column<bool>(nullable: false),
                    EmailMessages = table.Column<bool>(nullable: false),
                    SectionId = table.Column<int>(nullable: false),
                    ViewAnnouncements = table.Column<bool>(nullable: false),
                    ViewMessages = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SectionRecipients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SectionRecipients_ClassroomMemberships_ClassroomMembershipId",
                        column: x => x.ClassroomMembershipId,
                        principalTable: "ClassroomMemberships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SectionRecipients_Sections_SectionId",
                        column: x => x.SectionId,
                        principalTable: "Sections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    AuthorId = table.Column<int>(nullable: false),
                    Contents = table.Column<string>(nullable: true),
                    ConversationId = table.Column<int>(nullable: false),
                    Sent = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Messages_Users_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Messages_Conversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "Conversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Attachments",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    ContentType = table.Column<string>(nullable: true),
                    FileName = table.Column<string>(nullable: true),
                    MessageId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Attachments_Messages_MessageId",
                        column: x => x.MessageId,
                        principalTable: "Messages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AttachmentData",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    AttachmentId = table.Column<int>(nullable: false),
                    FileContents = table.Column<byte[]>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttachmentData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AttachmentData_Attachments_AttachmentId",
                        column: x => x.AttachmentId,
                        principalTable: "Attachments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnnouncementSections_AnnouncementId_SectionId",
                table: "AnnouncementSections",
                columns: new[] { "AnnouncementId", "SectionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AttachmentData_AttachmentId",
                table: "AttachmentData",
                column: "AttachmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_MessageId",
                table: "Attachments",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_ClassroomId",
                table: "Conversations",
                column: "ClassroomId");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_CreatorId",
                table: "Conversations",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_OwnerId",
                table: "Conversations",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_StudentId",
                table: "Conversations",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_AuthorId",
                table: "Messages",
                column: "AuthorId");

            // NpgSql doesn't work with filtered indices, so we will create this manually.
            migrationBuilder.Sql(_createdMessagesFilteredIndex);

            migrationBuilder.CreateIndex(
                name: "IX_SectionRecipients_SectionId",
                table: "SectionRecipients",
                column: "SectionId");

            migrationBuilder.CreateIndex(
                name: "IX_SectionRecipients_ClassroomMembershipId_SectionId",
                table: "SectionRecipients",
                columns: new[] { "ClassroomMembershipId", "SectionId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AttachmentData");

            migrationBuilder.DropTable(
                name: "SectionRecipients");

            migrationBuilder.DropTable(
                name: "Attachments");

            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "Conversations");

            migrationBuilder.DropIndex(
                name: "IX_AnnouncementSections_AnnouncementId_SectionId",
                table: "AnnouncementSections");

            migrationBuilder.DropColumn(
                name: "AllowStudentMessages",
                table: "Sections");

            migrationBuilder.CreateIndex(
                name: "IX_AnnouncementSections_AnnouncementId",
                table: "AnnouncementSections",
                column: "AnnouncementId");
        }

        private const string _createdMessagesFilteredIndex = @" 
            CREATE UNIQUE INDEX ""IX_Messages_ConversationId_AuthorId"" ON ""Messages"" 
            (""ConversationId"", ""AuthorId"") WHERE ""Sent"" IS NULL;";
    }
}
