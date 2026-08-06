namespace JazorAdmin.Features.Configuration;

public sealed record AuthorizationView(
    string Id,
    string? ApplicationId,
    string? ClientId,
    string? Subject,
    string Status,
    string Type,
    string[] Scopes,
    string? CreatedAt);

public sealed record TokenView(
    string Id,
    string? ApplicationId,
    string? ClientId,
    string? AuthorizationId,
    string? Subject,
    string Status,
    string Type,
    string? CreatedAt,
    string? ExpiresAt,
    string? RedeemedAt);
