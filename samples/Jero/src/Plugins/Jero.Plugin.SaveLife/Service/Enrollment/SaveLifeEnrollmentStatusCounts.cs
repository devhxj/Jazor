namespace Jero.Plugin.SaveLife.Service;

public static class SaveLifeEnrollmentStatusCounts
{
    public static async Task<SaveLifeMiniAppEnrollmentStatusCountsOutput> QueryAsync(
        ISugarQueryable<SaveLifeEnrollmentRecord> query)
    {
        var rows = await query
            .GroupBy(u => u.Status)
            .Select(u => new SaveLifeEnrollmentStatusCountRow
            {
                Status = u.Status,
                Count = SqlFunc.AggregateCount(u.Status)
            })
            .ToListAsync();

        return Build(rows);
    }

    public static SaveLifeMiniAppEnrollmentStatusCountsOutput Build(IEnumerable<string?> statuses)
    {
        var counts = statuses
            .Where(u => !string.IsNullOrWhiteSpace(u))
            .GroupBy(u => u!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(u => u.Key, u => u.Count(), StringComparer.OrdinalIgnoreCase);

        return new SaveLifeMiniAppEnrollmentStatusCountsOutput
        {
            Total = counts.Values.Sum(),
            Pending = GetCount(counts, "pending"),
            Approved = GetCount(counts, "approved"),
            Contacted = GetCount(counts, "contacted"),
            Rejected = GetCount(counts, "rejected"),
            NeedSupplement = GetCount(counts, "need_supplement"),
            Draft = GetCount(counts, "draft"),
            Cancelled = GetCount(counts, "cancelled"),
            Closed = GetCount(counts, "closed")
        };
    }

    private static SaveLifeMiniAppEnrollmentStatusCountsOutput Build(
        IEnumerable<SaveLifeEnrollmentStatusCountRow> rows)
    {
        var counts = rows
            .Where(u => !string.IsNullOrWhiteSpace(u.Status))
            .GroupBy(u => u.Status!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(u => u.Key, u => u.Sum(row => row.Count), StringComparer.OrdinalIgnoreCase);

        return new SaveLifeMiniAppEnrollmentStatusCountsOutput
        {
            Total = counts.Values.Sum(),
            Pending = GetCount(counts, "pending"),
            Approved = GetCount(counts, "approved"),
            Contacted = GetCount(counts, "contacted"),
            Rejected = GetCount(counts, "rejected"),
            NeedSupplement = GetCount(counts, "need_supplement"),
            Draft = GetCount(counts, "draft"),
            Cancelled = GetCount(counts, "cancelled"),
            Closed = GetCount(counts, "closed")
        };
    }

    private static int GetCount(IReadOnlyDictionary<string, int> counts, string status)
    {
        return counts.TryGetValue(status, out var count) ? count : 0;
    }

    private sealed class SaveLifeEnrollmentStatusCountRow
    {
        public string? Status { get; set; }

        public int Count { get; set; }
    }
}
