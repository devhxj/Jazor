using System.Text.RegularExpressions;

namespace Jero.Plugin.SaveLife.Service;

public static class SaveLifeCdeRegistrationNumber
{
    public static string Normalize(string? value)
    {
        var normalized = value?.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(normalized))
            return string.Empty;

        return Regex.Replace(normalized, @"[\s\p{P}\p{S}]+", string.Empty);
    }
}
