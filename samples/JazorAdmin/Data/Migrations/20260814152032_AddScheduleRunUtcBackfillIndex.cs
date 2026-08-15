using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JazorAdmin.Data.Migrations;

/// <inheritdoc />
public partial class _20260814152032_AddScheduleRunUtcBackfillIndex : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateIndex(
            name: "IX_ScheduleRuns_StartedAtUtc",
            table: "ScheduleRuns",
            column: "StartedAtUtc");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_ScheduleRuns_StartedAtUtc",
            table: "ScheduleRuns");
    }
}
