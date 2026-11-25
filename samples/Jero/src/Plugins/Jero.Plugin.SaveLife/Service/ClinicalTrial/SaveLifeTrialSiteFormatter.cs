namespace Jero.Plugin.SaveLife.Service;

public static class SaveLifeTrialSiteFormatter
{
    public static string? FormatInstitutionNames(string? sites)
    {
        var normalized = sites?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
            return null;

        try
        {
            using var document = JsonDocument.Parse(normalized);
            var names = document.RootElement.ValueKind switch
            {
                JsonValueKind.Array => document.RootElement.EnumerateArray()
                    .Select(ReadInstitutionName),
                JsonValueKind.Object => new[] { ReadInstitutionName(document.RootElement) }
                    .AsEnumerable(),
                JsonValueKind.String => new[] { document.RootElement.GetString() }
                    .AsEnumerable(),
                _ => Array.Empty<string?>()
            };

            var result = names
                .SelectMany(SplitInstitutionNames)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            return result.Count == 0 ? normalized : string.Join("、", result);
        }
        catch (JsonException)
        {
            var result = SplitInstitutionNames(normalized)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            return result.Count == 0 ? normalized : string.Join("、", result);
        }
    }

    private static string? ReadInstitutionName(JsonElement item)
    {
        if (item.ValueKind == JsonValueKind.String)
            return item.GetString()?.Trim();
        if (item.ValueKind != JsonValueKind.Object)
            return null;

        foreach (var name in new[]
        {
            "institution_name", "institutionName", "hospital_name", "hospitalName",
            "institution", "siteName", "SiteName", "name"
        })
        {
            if (item.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String)
            {
                var result = value.GetString()?.Trim();
                if (!string.IsNullOrWhiteSpace(result))
                    return result;
            }
        }

        return null;
    }

    private static IEnumerable<string> SplitInstitutionNames(string? value)
    {
        return (value ?? string.Empty)
            .Split(['；', ';', '|', '/', '\r', '\n', '、'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(u => !string.IsNullOrWhiteSpace(u));
    }
}
