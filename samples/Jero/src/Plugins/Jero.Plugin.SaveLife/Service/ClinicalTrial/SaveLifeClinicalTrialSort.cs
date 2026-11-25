namespace Jero.Plugin.SaveLife.Service;

public static class SaveLifeClinicalTrialSort
{
    public static ISugarQueryable<SaveLifeClinicalTrial> OrderByPinnedFirst(
        this ISugarQueryable<SaveLifeClinicalTrial> query)
    {
        return query
            .OrderBy(u => u.IsPinned, OrderByType.Desc)
            .OrderBy(u => u.CdeRegistrationNumber, OrderByType.Desc);
    }

    public static int GetPinnedRank(bool isPinned)
    {
        return isPinned ? 0 : 1;
    }

    public static int GetRecruitingRank(string? status)
    {
        return string.Equals(
            status,
            SaveLifeAgentTrialSelection.RecruitingStatus,
            StringComparison.OrdinalIgnoreCase)
            ? 0
            : 1;
    }

    public static IOrderedEnumerable<SaveLifeAgentTrialReference> OrderMatchesByRelevance(
        this IEnumerable<SaveLifeAgentTrialReference> trials)
    {
        ArgumentNullException.ThrowIfNull(trials);

        return trials
            .OrderBy(trial => GetRecruitingRank(trial.Status))
            .ThenByDescending(trial => trial.Score)
            .ThenBy(trial => GetPinnedRank(trial.IsPinned))
            .ThenBy(trial => trial.SortOrder)
            .ThenByDescending(trial => trial.Title);
    }
}
