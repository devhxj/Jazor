using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JazorAdmin.Data.Migrations;

/// <inheritdoc />
public partial class _20260806232403_ProductCenters : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Schedules",
            columns: table => new
            {
                Key = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                Description = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                Cron = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                Enabled = table.Column<bool>(type: "INTEGER", nullable: false),
                LastRunAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                LastStatus = table.Column<string>(type: "TEXT", maxLength: 32, nullable: true),
                LastMessage = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Schedules", x => x.Key);
            });

        migrationBuilder.CreateTable(
            name: "Settings",
            columns: table => new
            {
                Key = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                Group = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                Label = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                Description = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                Kind = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                Value = table.Column<string>(type: "TEXT", maxLength: 8000, nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Settings", x => x.Key);
            });

        migrationBuilder.CreateTable(
            name: "ScheduleRuns",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                ScheduleKey = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                Trigger = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                Status = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                StartedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                FinishedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                Message = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ScheduleRuns", x => x.Id);
                table.ForeignKey(
                    name: "FK_ScheduleRuns_Schedules_ScheduleKey",
                    column: x => x.ScheduleKey,
                    principalTable: "Schedules",
                    principalColumn: "Key",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ScheduleRuns_ScheduleKey_StartedAt",
            table: "ScheduleRuns",
            columns: new[] { "ScheduleKey", "StartedAt" });

        migrationBuilder.CreateIndex(
            name: "IX_Settings_Group_Key",
            table: "Settings",
            columns: new[] { "Group", "Key" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "ScheduleRuns");

        migrationBuilder.DropTable(
            name: "Settings");

        migrationBuilder.DropTable(
            name: "Schedules");
    }
}
