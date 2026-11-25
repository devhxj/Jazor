namespace Jero.Plugin.SaveLife.Service;

public static class SaveLifeAgentTrialSelection
{
    public const string RecruitingStatus = "recruiting";

    public static long? ResolveCandidateTrialId(
        long? inputTrialId,
        IReadOnlyList<SaveLifeAgentMatchedTrialOutput> matchedTrials)
    {
        if (inputTrialId.HasValue && inputTrialId.Value > 0)
            return inputTrialId.Value;

        var firstMatched = matchedTrials.FirstOrDefault();
        if (firstMatched == null)
            return null;

        return ParsePositiveId(firstMatched.Id, "项目Id格式不正确");
    }

    public static long? ResolveConfirmedTrialId(long? inputTrialId)
    {
        return inputTrialId is > 0 ? inputTrialId.Value : null;
    }

    public static bool IsRecruiting(SaveLifeClinicalTrial? trial)
    {
        return string.Equals(trial?.Status, RecruitingStatus, StringComparison.OrdinalIgnoreCase);
    }

    public static SaveLifeAgentTrialMatchingGate EvaluateMatchingGate(
        SaveLifePatientProfileOutput? patient)
    {
        var missingFields = new List<string>();
        if (patient == null || !long.TryParse(patient.Id, out var patientId) || patientId <= 0)
            missingFields.Add(SaveLifeAgentTrialMatchingFields.PatientProfile);
        if (string.IsNullOrWhiteSpace(patient?.Diagnosis))
            missingFields.Add(SaveLifeAgentTrialMatchingFields.Diagnosis);
        if (string.IsNullOrWhiteSpace(patient?.DiseaseStage) &&
            string.IsNullOrWhiteSpace(patient?.DiseaseStatus))
        {
            missingFields.Add(SaveLifeAgentTrialMatchingFields.DiseaseStageOrStatus);
        }
        if (string.IsNullOrWhiteSpace(patient?.TreatmentHistory) &&
            string.IsNullOrWhiteSpace(patient?.CurrentTreatment))
        {
            missingFields.Add(SaveLifeAgentTrialMatchingFields.Treatment);
        }

        return new SaveLifeAgentTrialMatchingGate
        {
            WasEvaluated = true,
            CanMatch = missingFields.Count == 0,
            MissingFields = missingFields
        };
    }

    private static long ParsePositiveId(string value, string errorMessage)
    {
        if (long.TryParse(value, out var id) && id > 0)
            return id;

        throw Oops.Oh(errorMessage).StatusCode(400);
    }
}

public sealed class SaveLifeAgentTrialMatchingGate
{
    public bool WasEvaluated { get; set; }

    public bool CanMatch { get; set; }

    public List<string> MissingFields { get; set; } = [];
}

public static class SaveLifeAgentTrialMatchingFields
{
    public const string PatientProfile = "patient_profile_id";
    public const string Diagnosis = "diagnosis";
    public const string DiseaseStageOrStatus = "disease_stage_or_status";
    public const string Treatment = "treatment_history_or_current_treatment";
}
