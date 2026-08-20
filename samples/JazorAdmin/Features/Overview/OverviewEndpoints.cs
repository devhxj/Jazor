// Provides real platform statistics for the administration dashboard.
// 为管理台仪表盘提供真实的平台统计数据：Identity、组织、SSO、配置与 Quartz 执行序列。
using JazorAdmin.Authentication;
using JazorAdmin.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OpenIddict.EntityFrameworkCore.Models;

namespace JazorAdmin.Features.Overview;

public sealed record OverviewDailyRunView(string Date, int Succeeded, int Failed);

public sealed record OverviewDailyAuditView(string Date, int SignIns, int TokenIssuances);

public sealed record PortalApplicationView(string ClientId, string DisplayName, string LaunchUri);

public sealed record OverviewView(
    int Accounts,
    int EnabledAccounts,
    int Organizations,
    int OrganizationRoles,
    int PlatformRoles,
    int Applications,
    int Scopes,
    int Authorizations,
    int Tokens,
    int Settings,
    int Schedules,
    int EnabledSchedules,
    OverviewDailyRunView[] RecentRuns,
    int AuditEvents,
    int TokenIssuances,
    OverviewDailyAuditView[] RecentAudit,
    PortalApplicationView[] PortalApplications);

public static class OverviewEndpoints
{
    public static IEndpointRouteBuilder MapOverviewEndpoints(this IEndpointRouteBuilder app)
    {
        var overview = app.MapGroup("/api/overview")
            .WithTags("Overview")
            .RequireAuthorization();

        overview.MapGet("/", GetAsync);
        return app;
    }

    private static async Task<IResult> GetAsync(
        AdminDbContext database,
        IOptions<DemoClientOptions> demoClientOptions,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var accounts = await database.Users.AsNoTracking().ToArrayAsync(cancellationToken);
        var enabledAccounts = 0;
        foreach (var account in accounts)
        {
            if (account.LockoutEnd is null || account.LockoutEnd <= now)
                enabledAccounts++;
        }

        // OpenIddict 默认实体经 UseOpenIddict 映射在同一个 DbContext 中，直接走 SQL COUNT。
        var applications = await database.Set<OpenIddictEntityFrameworkCoreApplication>().CountAsync(cancellationToken);
        var scopes = await database.Set<OpenIddictEntityFrameworkCoreScope>().CountAsync(cancellationToken);
        var authorizations = await database.Set<OpenIddictEntityFrameworkCoreAuthorization>().CountAsync(cancellationToken);
        var tokens = await database.Set<OpenIddictEntityFrameworkCoreToken>().CountAsync(cancellationToken);

        var schedules = await database.Schedules.CountAsync(cancellationToken);
        var enabledSchedules = await database.Schedules
            .Where(schedule => schedule.Enabled)
            .CountAsync(cancellationToken);

        var firstDay = DateTime.UtcNow.Date.AddDays(-6);
        var runs = await database.ScheduleRuns
            .AsNoTracking()
            .Where(run =>
                (run.Status == "succeeded" || run.Status == "failed") &&
                run.StartedAtUtc >= firstDay)
            .Select(run => new { run.Status, run.StartedAtUtc })
            .ToArrayAsync(cancellationToken);

        // The UTC key bounds the result in SQLite; grouping stays in .NET because the displayed
        // contract is a fixed seven-day UTC sequence rather than a provider-specific date function.
        // UTC 键先在 SQLite 中收窄结果；分桶留在 .NET，保证输出是固定 7 天 UTC 序列而非依赖
        // 数据库特定日期函数。
        var buckets = new Dictionary<DateTime, int[]>();
        foreach (var run in runs)
        {
            var day = run.StartedAtUtc!.Value.Date;

            if (!buckets.TryGetValue(day, out var counts))
            {
                counts = new int[2];
                buckets[day] = counts;
            }

            if (run.Status == "succeeded")
                counts[0]++;
            else
                counts[1]++;
        }

        var series = new OverviewDailyRunView[7];
        for (var offset = 0; offset < 7; offset++)
        {
            var day = firstDay.AddDays(offset);
            buckets.TryGetValue(day, out var counts);
            series[offset] = new OverviewDailyRunView(day.ToString("yyyy-MM-dd"), counts?[0] ?? 0, counts?[1] ?? 0);
        }

        var auditFirstDay = DateTime.UtcNow.Date.AddDays(-6);
        var auditValues = await database.AuditEvents
            .AsNoTracking()
            .Where(item => item.OccurredAtUtc >= auditFirstDay)
            .Select(item => new { item.OccurredAtUtc, item.Action, item.ObjectType, item.Summary })
            .ToArrayAsync(cancellationToken);
        var auditBuckets = new Dictionary<DateTime, int[]>();
        var tokenIssuances = 0;
        foreach (var audit in auditValues)
        {
            if (audit.ObjectType != "oidc-token" || audit.Action != AuditSaveChangesInterceptor.Issued)
                continue;

            tokenIssuances++;
            var day = audit.OccurredAtUtc.Date;
            if (!auditBuckets.TryGetValue(day, out var counts))
            {
                counts = new int[2];
                auditBuckets[day] = counts;
            }

            // One authorization_code issuance represents the interactive sign-in leg; access and
            // refresh tokens remain visible in the second series as total token activity.
            // 每次 authorization_code 签发对应一次交互登录；access/refresh 仍计入令牌活动总量。
            if (string.Equals(audit.Summary, "authorization_code", StringComparison.Ordinal))
                counts[0]++;
            counts[1]++;
        }

        var auditSeries = new OverviewDailyAuditView[7];
        for (var offset = 0; offset < 7; offset++)
        {
            var day = auditFirstDay.AddDays(offset);
            auditBuckets.TryGetValue(day, out var counts);
            auditSeries[offset] = new OverviewDailyAuditView(
                day.ToString("yyyy-MM-dd"),
                counts?[0] ?? 0,
                counts?[1] ?? 0);
        }

        var demo = demoClientOptions.Value;
        PortalApplicationView[] portalApplications = !demo.HasPortalLaunch
            ? []
            : [new PortalApplicationView(demo.ClientId, "JazorAdmin Operations Demo", demo.LaunchUri!)];

        var view = new OverviewView(
            accounts.Length,
            enabledAccounts,
            await database.Organizations.CountAsync(cancellationToken),
            await database.OrganizationRoles.CountAsync(cancellationToken),
            await database.Roles.CountAsync(cancellationToken),
            applications,
            scopes,
            authorizations,
            tokens,
            await database.Settings.CountAsync(cancellationToken),
            schedules,
            enabledSchedules,
            series,
            auditValues.Length,
            tokenIssuances,
            auditSeries,
            portalApplications);

        return Results.Ok(view);
    }
}
