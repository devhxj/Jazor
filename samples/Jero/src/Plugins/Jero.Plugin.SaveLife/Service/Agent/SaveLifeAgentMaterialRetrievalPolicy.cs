namespace Jero.Plugin.SaveLife.Service;

public sealed class SaveLifeAgentMaterialRetrievalPlan
{
    public bool CanSearch { get; init; }

    public string ExternalUserId { get; init; } = string.Empty;

    public long? PatientProfileId { get; init; }

    public long? EnrollmentRecordId { get; init; }

    public bool HasMaterialIntent { get; init; }

    public IReadOnlyList<long> RequestedMaterialIds { get; init; } = [];

    public IReadOnlyList<string> SearchTerms { get; init; } = Array.Empty<string>();
}

public static class SaveLifeAgentMaterialRetrievalPolicy
{
    private static readonly HashSet<string> RetrievableStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "submitted",
        "approved",
        "need_supplement",
        "rejected"
    };

    private static readonly HashSet<string> RequiredMaterialSatisfiedStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "submitted",
        "approved"
    };

    private static readonly HashSet<string> SupplementRequiredStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "need_supplement",
        "rejected"
    };

    public static SaveLifeAgentMaterialRetrievalPlan Build(
        SaveLifeAgentRetrievalQuery retrievalQuery,
        string? externalUserId,
        long? enrollmentRecordId,
        long? patientProfileId = null,
        IReadOnlyList<long>? requestedMaterialIds = null,
        bool hasMaterialIntent = false)
    {
        var normalizedPatientProfileId = patientProfileId is > 0 ? patientProfileId : null;
        if (string.IsNullOrWhiteSpace(externalUserId) || !normalizedPatientProfileId.HasValue)
            return new SaveLifeAgentMaterialRetrievalPlan();

        var normalizedEnrollmentRecordId = enrollmentRecordId is > 0 ? enrollmentRecordId : null;
        var normalizedMaterialIds = (requestedMaterialIds ?? [])
            .Where(id => id > 0)
            .Distinct()
            .ToList();
        if (!normalizedEnrollmentRecordId.HasValue && !hasMaterialIntent && normalizedMaterialIds.Count == 0)
            return new SaveLifeAgentMaterialRetrievalPlan
            {
                ExternalUserId = externalUserId.Trim(),
                PatientProfileId = normalizedPatientProfileId,
                HasMaterialIntent = false
            };

        return new SaveLifeAgentMaterialRetrievalPlan
        {
            CanSearch = true,
            ExternalUserId = externalUserId.Trim(),
            PatientProfileId = normalizedPatientProfileId,
            EnrollmentRecordId = normalizedEnrollmentRecordId,
            HasMaterialIntent = hasMaterialIntent || normalizedMaterialIds.Count > 0,
            RequestedMaterialIds = normalizedMaterialIds,
            SearchTerms = normalizedMaterialIds.Count > 0 || normalizedEnrollmentRecordId.HasValue && !hasMaterialIntent
                ? Array.Empty<string>()
                : retrievalQuery.SearchTerms
        };
    }

    public static bool IsEligibleForRetrieval(SaveLifeEnrollmentMaterial? material, string externalUserId)
    {
        return material != null &&
            !material.IsDelete &&
            string.Equals(material.ExternalUserId, externalUserId, StringComparison.Ordinal) &&
            !string.IsNullOrWhiteSpace(material.Status) &&
            RetrievableStatuses.Contains(material.Status.Trim()) &&
            GetReadinessStatus(material) == SaveLifeAgentMaterialReadinessStatuses.Ready;
    }

    public static bool IsInScope(
        SaveLifeEnrollmentMaterial? material,
        SaveLifeAgentMaterialRetrievalPlan plan)
    {
        if (material == null || material.IsDelete)
            return false;

        if (!string.Equals(material.ExternalUserId, plan.ExternalUserId, StringComparison.Ordinal) ||
            !plan.PatientProfileId.HasValue ||
            material.PatientProfileId != plan.PatientProfileId)
        {
            return false;
        }

        if (plan.EnrollmentRecordId.HasValue)
        {
            return SaveLifeEnrollmentMaterialHelper.IsEffectiveForEnrollment(
                material,
                plan.ExternalUserId,
                plan.EnrollmentRecordId.Value);
        }

        return string.Equals(material.ExternalUserId, plan.ExternalUserId, StringComparison.Ordinal) &&
            material.EnrollmentRecordId <= 0;
    }

    public static string GetReadinessStatus(SaveLifeEnrollmentMaterial? material)
    {
        if (material == null || material.IsDelete)
            return SaveLifeAgentMaterialReadinessStatuses.Failed;

        var extractStatus = material.ExtractStatus?.Trim().ToLowerInvariant();
        var indexStatus = material.IndexStatus?.Trim().ToLowerInvariant();
        if (extractStatus == "synced" && indexStatus == "synced")
            return SaveLifeAgentMaterialReadinessStatuses.Ready;
        if (extractStatus is "failed" or "skipped" || indexStatus is "failed" or "skipped")
            return SaveLifeAgentMaterialReadinessStatuses.Failed;

        return SaveLifeAgentMaterialReadinessStatuses.Processing;
    }

    public static bool CanSatisfyRequiredMaterial(SaveLifeEnrollmentMaterial? material)
    {
        return material != null &&
            !material.IsDelete &&
            !string.IsNullOrWhiteSpace(material.Status) &&
            RequiredMaterialSatisfiedStatuses.Contains(material.Status.Trim());
    }

    public static bool RequiresSupplement(SaveLifeEnrollmentMaterial? material)
    {
        return material != null &&
            !material.IsDelete &&
            !string.IsNullOrWhiteSpace(material.Status) &&
            SupplementRequiredStatuses.Contains(material.Status.Trim());
    }

    public static double ScoreCandidate(
        SaveLifeAgentVectorIndex candidate,
        SaveLifeEnrollmentMaterial material,
        SaveLifeAgentMaterialRetrievalPlan plan,
        double semanticScore)
    {
        var score = Math.Max(0, semanticScore);

        if (plan.EnrollmentRecordId.HasValue)
        {
            score += material.EnrollmentRecordId == plan.EnrollmentRecordId.Value ? 2D : -0.5D;
        }

        var matchedTermCount = CountMatchedTerms(candidate, material, plan.SearchTerms);
        if (matchedTermCount > 0)
            score += Math.Min(1D, matchedTermCount * 0.2D);

        if (CanSatisfyRequiredMaterial(material))
            score += 0.15D;

        if (RequiresSupplement(material))
            score += 0.1D;

        return score;
    }

    private static int CountMatchedTerms(
        SaveLifeAgentVectorIndex candidate,
        SaveLifeEnrollmentMaterial material,
        IReadOnlyList<string> searchTerms)
    {
        if (searchTerms.Count == 0)
            return 0;

        var text = Normalize(string.Join("\n", new[]
        {
            candidate.Title,
            candidate.Content,
            SaveLifeEnrollmentMaterialHelper.GetMaterialTypeText(material.MaterialType),
            material.FileName,
            material.Remark,
            material.ReviewRemark,
            material.SupplementRequest,
            material.ExtractedText
        }.Where(u => !string.IsNullOrWhiteSpace(u))));

        return searchTerms.Count(term => text.Contains(Normalize(term), StringComparison.OrdinalIgnoreCase));
    }

    private static string Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : value.Trim().ToLowerInvariant();
    }
}

public static class SaveLifeAgentMaterialReadinessStatuses
{
    public const string Ready = "ready";
    public const string Processing = "processing";
    public const string Failed = "failed";
}

public static class SaveLifeAgentMaterialReadinessMessages
{
    public const string Processing = "您上传的病例资料仍在处理中，暂时无法读取内容，请稍后重试。";
    public const string Failed = "病例资料解析或索引失败，暂时无法读取内容，请稍后重试或重新上传。";
    public const string SelectedUnavailable = "本轮选择的病例资料不可用，请重新选择或上传。";
}
