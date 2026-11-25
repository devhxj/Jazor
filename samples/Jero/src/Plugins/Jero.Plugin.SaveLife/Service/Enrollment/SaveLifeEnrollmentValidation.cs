namespace Jero.Plugin.SaveLife.Service;

public sealed class SaveLifeEnrollmentValidationResult
{
    public List<string> MissingFields { get; } = new();

    public bool CanSubmit => MissingFields.Count == 0;

    public string Summary => string.Join("、", MissingFields);
}

public sealed class SaveLifeEnrollmentSubmissionFields
{
    public string? ContactName { get; set; }

    public string? ContactPhone { get; set; }

    public string? DiseaseDescription { get; set; }

    public string? MedicalHistory { get; set; }

    public string? DiagnosisReport { get; set; }

    public string? MedicationStatus { get; set; }

    public string? OtherInfo { get; set; }

    public string? Notes { get; set; }
}

public static class SaveLifeEnrollmentValidation
{
    private static readonly HashSet<string> ReusableSubmissionStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "draft",
        "pending"
    };

    private static readonly HashSet<string> TerminalEnrollmentStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "cancelled",
        "rejected",
        "closed"
    };

    public static SaveLifeEnrollmentValidationResult ValidateForSubmission(
        SaveLifeUserProfile? profile,
        string? diseaseDescription,
        string? medicalHistory)
    {
        var result = new SaveLifeEnrollmentValidationResult();
        result.MissingFields.AddRange(GetMissingProfileFields(profile));

        if (string.IsNullOrWhiteSpace(diseaseDescription) && string.IsNullOrWhiteSpace(medicalHistory))
            AddMissing(result.MissingFields, "本次报名病情描述或既往病史");

        return result;
    }

    public static SaveLifeEnrollmentValidationResult ValidateForSubmission(
        SaveLifeUserProfile? profile,
        SaveLifeEnrollmentSubmissionFields? fields)
    {
        var safeFields = fields ?? new SaveLifeEnrollmentSubmissionFields();
        var validationProfile = BuildEffectiveProfile(profile, safeFields);
        return ValidateForSubmission(
            validationProfile,
            safeFields.DiseaseDescription,
            FirstNotEmpty(safeFields.MedicalHistory, validationProfile?.MedicalHistory));
    }

    public static SaveLifeEnrollmentValidationResult ValidatePatientForSubmission(
        SaveLifeUserProfile? accountProfile,
        SaveLifePatientProfile? patientProfile,
        SaveLifeEnrollmentSubmissionFields? fields)
    {
        var safeFields = fields ?? new SaveLifeEnrollmentSubmissionFields();
        var result = new SaveLifeEnrollmentValidationResult();

        if (string.IsNullOrWhiteSpace(FirstNotEmpty(safeFields.ContactName, accountProfile?.RealName)))
            result.MissingFields.Add("联系人姓名");
        if (string.IsNullOrWhiteSpace(FirstNotEmpty(safeFields.ContactPhone, accountProfile?.Phone)))
            result.MissingFields.Add("联系电话");

        if (patientProfile == null)
        {
            result.MissingFields.Add("患者档案");
            return result;
        }

        if (string.IsNullOrWhiteSpace(patientProfile.Gender))
            result.MissingFields.Add("患者性别");
        if (!patientProfile.BirthDate.HasValue)
            result.MissingFields.Add("患者出生日期");
        if (string.IsNullOrWhiteSpace(FirstNotEmpty(patientProfile.City, patientProfile.Residence)))
            result.MissingFields.Add("患者常住地");
        if (string.IsNullOrWhiteSpace(FirstNotEmpty(
                safeFields.DiseaseDescription,
                safeFields.DiagnosisReport,
                patientProfile.Diagnosis,
                patientProfile.DiseaseStatus)))
        {
            result.MissingFields.Add("患者诊断或病情描述");
        }
        if (string.IsNullOrWhiteSpace(FirstNotEmpty(
                safeFields.MedicalHistory,
                safeFields.MedicationStatus,
                patientProfile.TreatmentHistory,
                patientProfile.CurrentTreatment,
                patientProfile.MedicalHistory)))
        {
            result.MissingFields.Add("患者治疗经过或既往病史");
        }

        return result;
    }

    public static string ResolveSubmissionStatus(string requestedStatus, SaveLifeEnrollmentValidationResult validation)
    {
        var normalized = NormalizeStatus(requestedStatus);
        if (string.IsNullOrWhiteSpace(normalized))
            normalized = "draft";

        return string.Equals(normalized, "pending", StringComparison.OrdinalIgnoreCase) && !validation.CanSubmit
            ? "draft"
            : normalized;
    }

    public static bool CanReuseForSubmission(SaveLifeEnrollmentRecord? record)
    {
        return record != null &&
            (ReusableSubmissionStatuses.Contains(NormalizeStatus(record.Status)) ||
             IsTerminalEnrollmentStatus(record.Status));
    }

    public static bool IsTerminalEnrollmentStatus(string? status)
    {
        return TerminalEnrollmentStatuses.Contains(NormalizeStatus(status));
    }

    public static bool IsBlockingExistingSubmission(SaveLifeEnrollmentRecord? record)
    {
        return record != null &&
               !CanReuseForSubmission(record) &&
               !IsTerminalEnrollmentStatus(record.Status);
    }

    public static void PrepareForSubmission(
        SaveLifeEnrollmentRecord record,
        string requestedStatus,
        DateTime submittedTime)
    {
        if (record == null)
            throw new ArgumentNullException(nameof(record));

        var wasTerminal = record.IsDelete || IsTerminalEnrollmentStatus(record.Status);
        record.IsDelete = false;
        record.DeleteTime = null;
        record.Status = string.IsNullOrWhiteSpace(NormalizeStatus(requestedStatus))
            ? "draft"
            : NormalizeStatus(requestedStatus);

        if (wasTerminal)
        {
            record.SubmittedTime = submittedTime;
            record.AuditTime = null;
            record.AuditRemark = null;
        }
        else
        {
            record.SubmittedTime ??= submittedTime;
        }
    }

    public static List<string> GetMissingProfileFields(SaveLifeUserProfile? profile)
    {
        if (profile == null)
            return new List<string> { "个人资料" };

        var missing = new List<string>();
        if (string.IsNullOrWhiteSpace(profile.RealName))
            missing.Add("真实姓名");
        if (string.IsNullOrWhiteSpace(profile.Gender))
            missing.Add("性别");
        if (!profile.BirthDate.HasValue)
            missing.Add("出生日期");
        if (string.IsNullOrWhiteSpace(profile.Phone))
            missing.Add("手机号");
        if (string.IsNullOrWhiteSpace(profile.Address))
            missing.Add("常住地址");
        if (string.IsNullOrWhiteSpace(profile.MedicalHistory))
            missing.Add("既往病史");

        return missing;
    }

    public static void ApplySubmissionFields(
        SaveLifeEnrollmentRecord record,
        SaveLifeEnrollmentSubmissionFields? fields,
        SaveLifeUserProfile? profile)
    {
        ApplySubmissionFields(record, fields, profile, null);
    }

    public static void ApplySubmissionFields(
        SaveLifeEnrollmentRecord record,
        SaveLifeEnrollmentSubmissionFields? fields,
        SaveLifeUserProfile? profile,
        SaveLifePatientProfile? patientProfile)
    {
        if (record == null)
            throw new ArgumentNullException(nameof(record));

        var safeFields = fields ?? new SaveLifeEnrollmentSubmissionFields();

        record.ContactName = PickWithFallback(record.ContactName, safeFields.ContactName, profile?.RealName, 64);
        record.ContactPhone = PickWithFallback(record.ContactPhone, safeFields.ContactPhone, profile?.Phone, 32);
        record.DiseaseDescription = PickWithFallback(
            record.DiseaseDescription,
            safeFields.DiseaseDescription,
            FirstNotEmpty(patientProfile?.Diagnosis, patientProfile?.DiseaseStatus),
            512);
        record.MedicalHistory = PickWithFallback(
            record.MedicalHistory,
            safeFields.MedicalHistory,
            FirstNotEmpty(
                patientProfile?.TreatmentHistory,
                patientProfile?.CurrentTreatment,
                patientProfile?.MedicalHistory,
                profile?.MedicalHistory),
            4000);
        record.DiagnosisReport = Pick(record.DiagnosisReport, safeFields.DiagnosisReport, 4000);
        record.MedicationStatus = PickWithFallback(
            record.MedicationStatus,
            safeFields.MedicationStatus,
            patientProfile?.CurrentTreatment,
            4000);
        record.OtherInfo = Pick(record.OtherInfo, safeFields.OtherInfo, 4000);
        record.Notes = Pick(record.Notes, safeFields.Notes, 4000);
    }

    private static SaveLifeUserProfile? BuildEffectiveProfile(
        SaveLifeUserProfile? profile,
        SaveLifeEnrollmentSubmissionFields fields)
    {
        if (profile == null)
            return null;

        return new SaveLifeUserProfile
        {
            RealName = FirstNotEmpty(profile.RealName, fields.ContactName),
            Gender = profile.Gender,
            BirthDate = profile.BirthDate,
            Phone = FirstNotEmpty(profile.Phone, fields.ContactPhone),
            Address = profile.Address,
            MedicalHistory = FirstNotEmpty(profile.MedicalHistory, fields.MedicalHistory)
        };
    }

    private static void AddMissing(List<string> missingFields, string field)
    {
        if (!missingFields.Contains(field))
            missingFields.Add(field);
    }

    private static string NormalizeStatus(string? status)
    {
        var normalized = status?.Trim();
        return string.IsNullOrWhiteSpace(normalized) ? string.Empty : normalized;
    }

    private static string? PickWithFallback(
        string? current,
        string? primary,
        string? fallback,
        int maxLength)
    {
        var normalizedPrimary = NormalizeText(primary, maxLength);
        if (!string.IsNullOrWhiteSpace(normalizedPrimary))
            return normalizedPrimary;

        if (!string.IsNullOrWhiteSpace(current))
            return current.Length <= maxLength ? current : current[..maxLength];

        return NormalizeText(fallback, maxLength);
    }

    private static string? Pick(string? current, string? candidate, int maxLength)
    {
        return NormalizeText(candidate, maxLength) ?? NormalizeText(current, maxLength);
    }

    private static string? FirstNotEmpty(params string?[] values)
    {
        return values
            .Select(u => u?.Trim())
            .FirstOrDefault(u => !string.IsNullOrWhiteSpace(u));
    }

    private static string? NormalizeText(string? value, int maxLength)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
            return null;

        return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }
}
