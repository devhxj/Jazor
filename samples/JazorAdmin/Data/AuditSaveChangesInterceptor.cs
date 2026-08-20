// Captures management writes from one EF interception point instead of scattering audit calls
// through endpoints. HTTP scope deliberately excludes startup seeding and Quartz background work.
// 统一 EF 拦截点采集管理写入，避免各 Endpoint 分散埋点；HTTP 范围天然排除初始化和 Quartz 后台任务。
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using OpenIddict.Abstractions;
using OpenIddict.EntityFrameworkCore.Models;

namespace JazorAdmin.Data;

public sealed class AuditSaveChangesInterceptor(
    IHttpContextAccessor httpContextAccessor,
    IServiceScopeFactory scopeFactory) : SaveChangesInterceptor
{
    public const string FeatureKey = "feature.audit.enabled";

    public const string Created = "created";
    public const string Updated = "updated";
    public const string Deleted = "deleted";
    public const string Granted = "granted";
    public const string Revoked = "revoked";
    public const string Issued = "issued";

    private bool? _enabled;

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        Capture(eventData.Context as AdminDbContext);
        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        Capture(eventData.Context as AdminDbContext);
        return ValueTask.FromResult(result);
    }

    private void Capture(AdminDbContext? database)
    {
        var context = httpContextAccessor.HttpContext;
        if (database is null || context is null)
            return;

        // Session establishment is an authentication concern, not an administration action. Keep
        // the audit stream focused on managed platform resources instead of login bookkeeping.
        // 会话建立属于认证行为而非管理操作；排除登录 API，避免审计流被认证账务写入淹没。
        var isManagementRequest = context.Request.Path.StartsWithSegments("/api") &&
                                  !context.Request.Path.StartsWithSegments("/api/auth");
        var isProtocolRequest = context.Request.Path.StartsWithSegments("/connect");
        if (!isManagementRequest && !isProtocolRequest)
            return;

        database.ChangeTracker.DetectChanges();
        var changes = database.ChangeTracker.Entries()
            .Where(static entry => entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Select(Describe)
            .Where(static change => change is not null)
            .Cast<AuditChange>()
            .ToArray();
        if (changes.Length == 0 || !IsEnabled())
            return;

        var actor = ResolveActor(context.User);
        foreach (var change in changes)
        {
            // API management writes require an authenticated caller. OpenIddict token exchange has
            // no local cookie on its second request, so its persisted subject becomes the actor.
            // 管理 API 必须有已认证操作者；令牌交换第二跳没有本地 Cookie 时，使用持久 subject。
            if (!isManagementRequest && !change.IsProtocol)
                continue;

            var actorId = actor.Id ?? change.Subject;
            if (string.IsNullOrWhiteSpace(actorId))
                continue;

            database.AuditEvents.Add(new AuditEvent
            {
                OccurredAt = DateTimeOffset.UtcNow,
                ActorId = actorId,
                ActorName = actor.Name ?? change.Subject,
                Action = change.Action,
                ObjectType = change.ObjectType,
                ObjectId = change.ObjectId,
                Summary = change.Summary
            });
        }
    }

    private bool IsEnabled()
    {
        if (_enabled is { } enabled)
            return enabled;

        // This lookup uses a sibling scope rather than the context currently entering SaveChanges.
        // It avoids re-entering EF's save pipeline and keeps the feature flag as persisted state.
        // 读取使用同级 scope，避免在正在 SaveChanges 的上下文中二次查询，同时保持开关持久化。
        using var scope = scopeFactory.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<AdminDbContext>();
        var value = database.Settings
            .AsNoTracking()
            .Where(setting => setting.Key == FeatureKey)
            .Select(setting => setting.Value)
            .FirstOrDefault();
        _enabled = string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
        return _enabled.Value;
    }

    private static Actor ResolveActor(ClaimsPrincipal principal)
    {
        var actorId = principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? principal.FindFirstValue(OpenIddictConstants.Claims.Subject);
        var actorName = principal.Identity?.Name
            ?? principal.FindFirstValue(ClaimTypes.Email)
            ?? principal.FindFirstValue(OpenIddictConstants.Claims.Email);
        return new Actor(actorId, actorName);
    }

    private static AuditChange? Describe(EntityEntry entry)
    {
        if (entry.Entity is AuditEvent)
            return null;

        var (objectType, action, isProtocol) = entry.Entity switch
        {
            Organization => ("organization", ActionFor(entry.State), false),
            OrganizationMembership => ("organization-membership", ActionFor(entry.State), false),
            OrganizationRole => ("organization-role", ActionFor(entry.State), false),
            OrganizationMembershipRole => ("membership-role", GrantActionFor(entry.State), false),
            ResourceOperationGrant => ("resource-grant", GrantActionFor(entry.State), false),
            Setting => ("setting", ActionFor(entry.State), false),
            Schedule => ("schedule", ActionFor(entry.State), false),
            AdminUser => ("account", ActionFor(entry.State), false),
            IdentityUserRole<string> => ("account-role", GrantActionFor(entry.State), false),
            OpenIddictEntityFrameworkCoreApplication => ("sso-application", ActionFor(entry.State), false),
            OpenIddictEntityFrameworkCoreScope => ("sso-scope", ActionFor(entry.State), false),
            OpenIddictEntityFrameworkCoreAuthorization => DescribeAuthorization(entry),
            OpenIddictEntityFrameworkCoreToken => DescribeToken(entry),
            _ => (null, null, false)
        };

        if (objectType is null || action is null)
            return null;

        return new AuditChange(
            action,
            objectType,
            ObjectId(entry),
            Summary(entry),
            Subject(entry),
            isProtocol);
    }

    private static (string? ObjectType, string? Action, bool IsProtocol) DescribeAuthorization(EntityEntry entry)
    {
        if (entry.State == EntityState.Added)
            return ("sso-authorization", Granted, true);

        return IsTransitionToRevoked(entry)
            ? ("sso-authorization", Revoked, true)
            : (null, null, true);
    }

    private static (string? ObjectType, string? Action, bool IsProtocol) DescribeToken(EntityEntry entry)
    {
        if (entry.State == EntityState.Added)
            return ("oidc-token", Issued, true);

        return IsTransitionToRevoked(entry)
            ? ("oidc-token", Revoked, true)
            : (null, null, true);
    }

    private static bool IsTransitionToRevoked(EntityEntry entry)
    {
        var status = entry.Property("Status");
        return entry.State == EntityState.Modified &&
               status.IsModified &&
               string.Equals(status.CurrentValue as string, OpenIddictConstants.Statuses.Revoked, StringComparison.Ordinal) &&
               !string.Equals(status.OriginalValue as string, OpenIddictConstants.Statuses.Revoked, StringComparison.Ordinal);
    }

    private static string ActionFor(EntityState state)
        => state switch
        {
            EntityState.Added => Created,
            EntityState.Deleted => Deleted,
            _ => Updated
        };

    private static string GrantActionFor(EntityState state)
        => state == EntityState.Deleted ? Revoked : Granted;

    private static string ObjectId(EntityEntry entry)
    {
        var key = entry.Metadata.FindPrimaryKey();
        if (key is null)
            return entry.Metadata.ClrType.Name;

        var values = new string[key.Properties.Count];
        for (var index = 0; index < key.Properties.Count; index++)
        {
            var property = entry.Property(key.Properties[index].Name);
            var value = entry.State == EntityState.Deleted ? property.OriginalValue : property.CurrentValue;
            values[index] = value?.ToString() ?? string.Empty;
        }

        return string.Join("/", values);
    }

    private static string? Summary(EntityEntry entry)
    {
        foreach (var propertyName in new[]
                 {
                     "DisplayName", "Name", "Code", "Key", "Email", "ClientId", "Type", "Subject",
                     "ResourceKey", "OperationKey"
                 })
        {
            var property = entry.Metadata.FindProperty(propertyName);
            if (property is null)
                continue;

            var value = entry.State == EntityState.Deleted
                ? entry.Property(propertyName).OriginalValue
                : entry.Property(propertyName).CurrentValue;
            if (value is not null && !string.IsNullOrWhiteSpace(value.ToString()))
                return value.ToString();
        }

        return null;
    }

    private static string? Subject(EntityEntry entry)
    {
        var property = entry.Metadata.FindProperty("Subject");
        if (property is null)
            return null;

        var value = entry.State == EntityState.Deleted
            ? entry.Property("Subject").OriginalValue
            : entry.Property("Subject").CurrentValue;
        return value as string;
    }

    private sealed record Actor(string? Id, string? Name);

    private sealed record AuditChange(
        string Action,
        string ObjectType,
        string ObjectId,
        string? Summary,
        string? Subject,
        bool IsProtocol);
}
