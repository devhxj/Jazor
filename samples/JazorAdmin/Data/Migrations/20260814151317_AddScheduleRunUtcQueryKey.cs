using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JazorAdmin.Data.Migrations;

/// <inheritdoc />
public partial class _20260814151317_AddScheduleRunUtcQueryKey : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_ScheduleRuns_ScheduleKey_StartedAt",
            table: "ScheduleRuns");

        migrationBuilder.AddColumn<DateTime>(
            name: "StartedAtUtc",
            table: "ScheduleRuns",
            type: "TEXT",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_ScheduleRuns_ScheduleKey_StartedAtUtc",
            table: "ScheduleRuns",
            columns: new[] { "ScheduleKey", "StartedAtUtc" });

        migrationBuilder.CreateIndex(
            name: "IX_ScheduleRuns_Status_StartedAtUtc",
            table: "ScheduleRuns",
            columns: new[] { "Status", "StartedAtUtc" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_ScheduleRuns_ScheduleKey_StartedAtUtc",
            table: "ScheduleRuns");

        migrationBuilder.DropIndex(
            name: "IX_ScheduleRuns_Status_StartedAtUtc",
            table: "ScheduleRuns");

        migrationBuilder.DropColumn(
            name: "StartedAtUtc",
            table: "ScheduleRuns");

        migrationBuilder.CreateIndex(
            name: "IX_ScheduleRuns_ScheduleKey_StartedAt",
            table: "ScheduleRuns",
            columns: new[] { "ScheduleKey", "StartedAt" });
    }
}
