namespace JazorAdmin.Features.Sso;

public sealed record ScopeCreate(
    string Name,
    string DisplayName,
    string? Description,
    string[] Resources);

public sealed record ScopeUpdate(
    string DisplayName,
    string? Description,
    string[] Resources);

public sealed record ScopeView(
    string Id,
    string Name,
    string DisplayName,
    string? Description,
    string[] Resources);
