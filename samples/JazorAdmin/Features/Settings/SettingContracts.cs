namespace JazorAdmin.Features.Settings;

public sealed record SettingCreate(
    string Key,
    string Group,
    string Label,
    string? Description,
    string Kind,
    string Value);

public sealed record SettingUpdate(
    string Group,
    string Label,
    string? Description,
    string Kind,
    string Value);

public sealed record SettingView(
    string Key,
    string Group,
    string Label,
    string? Description,
    string Kind,
    string Value,
    string UpdatedAt);
