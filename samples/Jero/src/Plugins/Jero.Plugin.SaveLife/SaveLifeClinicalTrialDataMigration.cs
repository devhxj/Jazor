using System.Text.RegularExpressions;
using SqlSugar;

namespace Jero.Plugin.SaveLife;

public sealed class SaveLifeClinicalTrialDataMigrationReport
{
    public int TotalCount { get; internal set; }

    public int ChangedCount { get; internal set; }

    public int IndicationBackfilledCount { get; internal set; }

    public int DiseaseAreaCorrectedCount { get; internal set; }

    public int ConflictCount { get; internal set; }

    public int AmbiguousCount { get; internal set; }

    public int UnresolvedCount { get; internal set; }
}

public static class SaveLifeClinicalTrialDataMigration
{
    private const int MaxIndicationLength = 512;

    private static readonly Regex IndicationPattern = new(
        @"(?:^|[；;\r\n])\s*适应症\s*[：:]\s*(?<value>[^；;\r\n]+)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public static SaveLifeClinicalTrialDataMigrationReport Apply(ISqlSugarClient db)
    {
        var report = new SaveLifeClinicalTrialDataMigrationReport();
        var columns = db.DbMaintenance.GetColumnInfosByTableName("sl_clinical_trial", false)
            ?? new List<DbColumnInfo>();
        if (columns.Count == 0 || columns.All(u => !u.DbColumnName.Equals("Indication", StringComparison.OrdinalIgnoreCase)))
            return report;

        report.TotalCount = db.Queryable<SaveLifeClinicalTrial>()
            .ClearFilter()
            .Where(u => !u.IsDelete)
            .Count();
        report.UnresolvedCount = db.Queryable<SaveLifeClinicalTrial>()
            .ClearFilter()
            .Where(u => !u.IsDelete && (u.DiseaseArea == null || u.DiseaseArea == ""))
            .Count();
        var trials = db.Queryable<SaveLifeClinicalTrial>()
            .ClearFilter()
            .Where(u => !u.IsDelete)
            .Where(u =>
                u.Indication == null ||
                u.Indication == "" ||
                u.Title.Contains("冠心病") ||
                u.Title.Contains("冠状动脉") ||
                u.Title.Contains("急性冠脉") ||
                u.Title.Contains("心肌梗死") ||
                u.Title.Contains("心绞痛") ||
                u.Title.Contains("高胆固醇血症") ||
                u.Title.Contains("高脂血症") ||
                u.Title.Contains("血脂异常") ||
                u.Title.Contains("匹伐他汀") ||
                u.Title.Contains("阴道炎") ||
                u.Title.Contains("阴道病") ||
                u.Title.Contains("外阴阴道") ||
                u.Indication!.Contains("冠心病") ||
                u.Indication.Contains("冠状动脉") ||
                u.Indication.Contains("急性冠脉") ||
                u.Indication.Contains("心肌梗死") ||
                u.Indication.Contains("心绞痛") ||
                u.Indication.Contains("高胆固醇血症") ||
                u.Indication.Contains("高脂血症") ||
                u.Indication.Contains("血脂异常") ||
                u.Indication.Contains("匹伐他汀") ||
                u.Indication.Contains("阴道炎") ||
                u.Indication.Contains("阴道病") ||
                u.Indication.Contains("外阴阴道"))
            .ToList();

        db.Ado.BeginTran();
        try
        {
            foreach (var trial in trials)
            {
                var originalIndication = Normalize(trial.Indication);
                var indication = originalIndication
                    ?? ExtractIndication(trial.Requirements, trial.Description);
                var resolution = SaveLifeDiseaseAreaResolver.Resolve(
                    trial.DiseaseArea,
                    trial.Title,
                    indication);

                if (resolution.HasConflict)
                    report.ConflictCount++;
                if (resolution.IsAmbiguous)
                    report.AmbiguousCount++;
                if (resolution.IsAmbiguous && !string.IsNullOrWhiteSpace(resolution.DiseaseArea))
                    report.UnresolvedCount++;

                var indicationChanged = originalIndication == null && indication != null;
                var diseaseAreaChanged = !string.Equals(
                    Normalize(trial.DiseaseArea),
                    resolution.DiseaseArea,
                    StringComparison.Ordinal);
                if (!indicationChanged && !diseaseAreaChanged)
                    continue;

                if (indicationChanged)
                    report.IndicationBackfilledCount++;
                if (diseaseAreaChanged)
                    report.DiseaseAreaCorrectedCount++;

                trial.Indication = indication;
                trial.DiseaseArea = resolution.DiseaseArea;
                trial.IsInKnowledgeBase = false;
                trial.KnowledgeBaseError = null;
                trial.KnowledgeBaseRetryCount = 0;
                trial.KnowledgeBaseLastAttemptTime = null;
                trial.KnowledgeBaseNextRetryTime = null;
                trial.UpdateTime = DateTime.Now;
                db.Updateable(trial)
                    .UpdateColumns(u => new
                    {
                        u.Indication,
                        u.DiseaseArea,
                        u.IsInKnowledgeBase,
                        u.KnowledgeBaseError,
                        u.KnowledgeBaseRetryCount,
                        u.KnowledgeBaseLastAttemptTime,
                        u.KnowledgeBaseNextRetryTime,
                        u.UpdateTime
                    })
                    .ExecuteCommand();
                report.ChangedCount++;
            }

            db.Ado.CommitTran();
            return report;
        }
        catch
        {
            db.Ado.RollbackTran();
            throw;
        }
    }

    public static string? ExtractIndication(params string?[] sources)
    {
        foreach (var source in sources)
        {
            if (string.IsNullOrWhiteSpace(source))
                continue;

            var match = IndicationPattern.Match(source);
            if (!match.Success)
                continue;

            var value = Normalize(match.Groups["value"].Value)?.TrimEnd('；', ';');
            if (string.IsNullOrWhiteSpace(value))
                continue;

            return value.Length <= MaxIndicationLength
                ? value
                : value[..MaxIndicationLength];
        }

        return null;
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
