namespace Jero.Plugin.SaveLife.Service;

/// <summary>
/// 后台报名记录状态流转规则。
/// </summary>
public static class SaveLifeEnrollmentStatusPolicy
{
    public const string Pending = "pending";

    public const string NeedSupplement = "need_supplement";

    public const string Approved = "approved";

    public const string Rejected = "rejected";

    public const string Contacted = "contacted";

    public static bool CanAudit(string? currentStatus, string? nextStatus)
    {
        var current = Normalize(currentStatus);
        var next = Normalize(nextStatus);
        return (current is Pending or NeedSupplement) && (next is Approved or Rejected);
    }

    public static bool CanContact(string? currentStatus)
    {
        return Normalize(currentStatus) == Approved;
    }

    public static string Normalize(string? status)
    {
        return status?.Trim().ToLowerInvariant() ?? string.Empty;
    }
}
