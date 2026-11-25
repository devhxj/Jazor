namespace Jero.Plugin.SaveLife.Service;

public static class SaveLifeKnowledgeBaseRetryPolicy
{
    private const int MaxRetryDelayDays = 7;
    private const int ConfigurationRetryDelayDays = 7;

    public static bool IsRetryDue(DateTime? nextRetryTime, DateTime now)
    {
        return !nextRetryTime.HasValue || nextRetryTime.Value <= now;
    }

    public static DateTime CalculateNextRetryTime(int retryCount, string? error, DateTime now)
    {
        if (IsConfigurationError(error))
            return now.AddDays(ConfigurationRetryDelayDays);

        var exponent = Math.Clamp(retryCount - 1, 0, 3);
        var delayDays = Math.Min(MaxRetryDelayDays, 1 << exponent);
        return now.AddDays(delayDays);
    }

    public static bool IsConfigurationError(string? error)
    {
        if (string.IsNullOrWhiteSpace(error))
            return false;

        return error.Contains("未配置", StringComparison.OrdinalIgnoreCase) ||
            error.Contains("配置", StringComparison.OrdinalIgnoreCase) ||
            error.Contains("configuration", StringComparison.OrdinalIgnoreCase) ||
            error.Contains("api key", StringComparison.OrdinalIgnoreCase);
    }
}
