using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JazorAdmin.Data.Migrations;

/// <inheritdoc />
public partial class _20260819094537_AddAuditEvents : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "AuditEvents",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                OccurredAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                OccurredAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                ActorId = table.Column<string>(type: "TEXT", maxLength: 400, nullable: true),
                ActorName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                Action = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                ObjectType = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                ObjectId = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                Summary = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AuditEvents", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_AuditEvents_Action_OccurredAtUtc",
            table: "AuditEvents",
            columns: new[] { "Action", "OccurredAtUtc" });

        migrationBuilder.CreateIndex(
            name: "IX_AuditEvents_ActorId_OccurredAtUtc",
            table: "AuditEvents",
            columns: new[] { "ActorId", "OccurredAtUtc" });

        migrationBuilder.CreateIndex(
            name: "IX_AuditEvents_ObjectType_OccurredAtUtc",
            table: "AuditEvents",
            columns: new[] { "ObjectType", "OccurredAtUtc" });

        migrationBuilder.CreateIndex(
            name: "IX_AuditEvents_OccurredAtUtc",
            table: "AuditEvents",
            column: "OccurredAtUtc");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "AuditEvents");
    }
}
