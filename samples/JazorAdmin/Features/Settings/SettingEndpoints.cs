// Maps editable system settings. The API keeps values as authored strings while validating the selected value kind.
// 配置值按原样持久化，并在写入时按类型验证，避免给运行配置引入弱类型 object 边界。
using System.Text.Json;
using JazorAdmin.Authorization;
using JazorAdmin.Data;
using Microsoft.EntityFrameworkCore;

namespace JazorAdmin.Features.Settings;

public static class SettingEndpoints
{
    private static readonly string[] Kinds = ["text", "boolean", "number", "json"];

    public static IEndpointRouteBuilder MapSettingEndpoints(this IEndpointRouteBuilder app)
    {
        var settings = app.MapGroup("/api/settings")
            .WithTags("Settings")
            .RequireAuthorization(PolicyKeys.PlatformAdministrator);

        settings.MapGet("/", ListAsync);
        settings.MapPost("/", CreateAsync);
        settings.MapPut("/{key}", UpdateAsync);
        settings.MapDelete("/{key}", DeleteAsync);
        return app;
    }

    private static async Task<IResult> ListAsync(AdminDbContext database, CancellationToken cancellationToken)
        => Results.Ok(await database.Settings
            .AsNoTracking()
            .OrderBy(setting => setting.Group)
            .ThenBy(setting => setting.Key)
            .Select(setting => ToView(setting))
            .ToArrayAsync(cancellationToken));

    private static async Task<IResult> CreateAsync(
        SettingCreate request,
        AdminDbContext database,
        CancellationToken cancellationToken)
    {
        if (!TryValidate(request.Key, request.Group, request.Label, request.Kind, request.Value, out var errors))
            return Results.ValidationProblem(errors);

        var key = request.Key.Trim();
        if (await database.Settings.FindAsync([key], cancellationToken) is not null)
            return Results.Conflict(new { message = "A setting with this key already exists." });

        var setting = new Setting { Key = key };
        Apply(setting, request.Group, request.Label, request.Description, request.Kind, request.Value);
        database.Settings.Add(setting);
        await database.SaveChangesAsync(cancellationToken);
        return Results.Created("/api/settings/" + key, ToView(setting));
    }

    private static async Task<IResult> UpdateAsync(
        string key,
        SettingUpdate request,
        AdminDbContext database,
        CancellationToken cancellationToken)
    {
        var setting = await database.Settings.FindAsync([key], cancellationToken);
        if (setting is null)
            return Results.NotFound();
        if (!TryValidate(key, request.Group, request.Label, request.Kind, request.Value, out var errors))
            return Results.ValidationProblem(errors);

        Apply(setting, request.Group, request.Label, request.Description, request.Kind, request.Value);
        await database.SaveChangesAsync(cancellationToken);
        return Results.Ok(ToView(setting));
    }

    private static async Task<IResult> DeleteAsync(
        string key,
        AdminDbContext database,
        CancellationToken cancellationToken)
    {
        var setting = await database.Settings.FindAsync([key], cancellationToken);
        if (setting is null)
            return Results.NotFound();

        database.Settings.Remove(setting);
        await database.SaveChangesAsync(cancellationToken);
        return Results.NoContent();
    }

    private static bool TryValidate(
        string? key,
        string? group,
        string? label,
        string? kind,
        string? value,
        out Dictionary<string, string[]> errors)
    {
        errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(key) || key.Trim().Length > 128)
            errors["key"] = ["Key is required and must not exceed 128 characters."];
        if (string.IsNullOrWhiteSpace(group) || group.Trim().Length > 64)
            errors["group"] = ["Group is required and must not exceed 64 characters."];
        if (string.IsNullOrWhiteSpace(label) || label.Trim().Length > 128)
            errors["label"] = ["Label is required and must not exceed 128 characters."];
        if (kind is null || !Kinds.Contains(kind, StringComparer.Ordinal))
            errors["kind"] = ["Kind must be text, boolean, number, or json."];
        if (value is null || value.Length > 8_000)
            errors["value"] = ["Value is required and must not exceed 8000 characters."];
        else if (kind == "boolean" && !bool.TryParse(value, out _))
            errors["value"] = ["Boolean values must be true or false."];
        else if (kind == "number" && !decimal.TryParse(value, out _))
            errors["value"] = ["Number values must be valid decimal values."];
        else if (kind == "json")
        {
            try
            {
                using var _ = JsonDocument.Parse(value);
            }
            catch (JsonException)
            {
                errors["value"] = ["JSON values must be valid JSON."];
            }
        }

        return errors.Count == 0;
    }

    private static void Apply(
        Setting setting,
        string group,
        string label,
        string? description,
        string kind,
        string value)
    {
        setting.Group = group.Trim();
        setting.Label = label.Trim();
        setting.Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        setting.Kind = kind;
        setting.Value = value;
        setting.UpdatedAt = DateTimeOffset.UtcNow;
    }

    private static SettingView ToView(Setting setting)
        => new(
            setting.Key,
            setting.Group,
            setting.Label,
            setting.Description,
            setting.Kind,
            setting.Value,
            setting.UpdatedAt.ToString("O"));
}
