// Exposes filtered, bounded audit history for platform operators.
// 向平台管理员提供可按时间、操作者、对象和操作类型筛选的有界审计历史。
using JazorAdmin.Authorization;
using JazorAdmin.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JazorAdmin.Features.Audit;

public static class AuditEndpoints
{
    private const int QueryLimit = 200;

    public static IEndpointRouteBuilder MapAuditEndpoints(this IEndpointRouteBuilder app)
    {
        var audit = app.MapGroup("/api/audit")
            .WithTags("Audit")
            .RequireAuthorization(PolicyKeys.PlatformAdministrator);

        audit.MapGet("/", ListAsync);
        return app;
    }

    private static async Task<IResult> ListAsync(
        [AsParameters] AuditQuery request,
        AdminDbContext database,
        CancellationToken cancellationToken)
    {
        if (request.From is { } from && request.To is { } to && from > to)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["from"] = ["From must be earlier than or equal to To."]
            });
        }

        var query = database.AuditEvents.AsNoTracking();
        if (request.From is { } lowerBound)
            query = query.Where(item => item.OccurredAtUtc >= lowerBound.UtcDateTime);
        if (request.To is { } upperBound)
            query = query.Where(item => item.OccurredAtUtc <= upperBound.UtcDateTime);

        var actor = Normalize(request.Actor);
        if (actor is not null)
        {
            query = query.Where(item =>
                item.ActorId != null && item.ActorId.Contains(actor) ||
                item.ActorName != null && item.ActorName.Contains(actor));
        }

        var target = Normalize(request.Object);
        if (target is not null)
        {
            query = query.Where(item =>
                item.ObjectType.Contains(target) ||
                item.ObjectId.Contains(target) ||
                item.Summary != null && item.Summary.Contains(target));
        }

        var action = Normalize(request.Action);
        if (action is not null)
            query = query.Where(item => item.Action == action);

        var events = await query
            .OrderByDescending(item => item.OccurredAtUtc)
            .ThenByDescending(item => item.Id)
            .Take(QueryLimit)
            .ToArrayAsync(cancellationToken);
        return Results.Ok(events.Select(ToView).ToArray());
    }

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static AuditEventView ToView(AuditEvent item)
        => new(
            item.Id.ToString(),
            item.OccurredAt.ToString("O"),
            item.ActorId,
            item.ActorName,
            item.Action,
            item.ObjectType,
            item.ObjectId,
            item.Summary);
}
