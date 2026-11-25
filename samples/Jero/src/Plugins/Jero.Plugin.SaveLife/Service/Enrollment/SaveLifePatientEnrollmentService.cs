namespace Jero.Plugin.SaveLife.Service;

public sealed class SaveLifePatientEnrollmentSubmissionInput
{
    public string ExternalUserId { get; set; } = string.Empty;

    public long? PatientProfileId { get; set; }

    public long ProjectId { get; set; }

    public SaveLifeEnrollmentSubmissionFields Fields { get; set; } = new();
}

public sealed class SaveLifePatientEnrollmentSubmissionResult
{
    public SaveLifeEnrollmentRecord Enrollment { get; set; } = null!;

    public SaveLifeEnrollmentValidationResult Validation { get; set; } = null!;
}

public interface ISaveLifePatientEnrollmentService
{
    Task<long> ResolvePatientProfileIdAsync(
        string externalUserId,
        long? requestedPatientProfileId,
        bool allowSelfFallback);

    Task<SaveLifePatientEnrollmentSubmissionResult> SubmitConfirmedAsync(
        SaveLifePatientEnrollmentSubmissionInput input);

    Task<SaveLifePatientEnrollmentSubmissionResult> SubmitConfirmedInTransactionAsync(
        ISqlSugarClient db,
        SaveLifePatientEnrollmentSubmissionInput input,
        CancellationToken cancellationToken = default);
}

public static class SaveLifePatientEnrollmentPolicy
{
    public static long ResolveConversationPatientProfileId(
        long? currentPatientProfileId,
        long? requestedPatientProfileId)
    {
        if (currentPatientProfileId is not > 0)
            throw Oops.Oh("请先确认本次报名对应的患者").StatusCode(409);
        if (requestedPatientProfileId is > 0 &&
            requestedPatientProfileId.Value != currentPatientProfileId.Value)
        {
            throw Oops.Oh("当前患者已变更，请根据最新患者重新确认参与意向").StatusCode(409);
        }

        return currentPatientProfileId.Value;
    }

    public static string ResolveConversationPhase(string status)
    {
        return status switch
        {
            "trial_required" => "intention",
            "considering" => "hesitate_intention",
            _ => $"{status}_intention"
        };
    }
}

public sealed class SaveLifePatientEnrollmentService : ISaveLifePatientEnrollmentService, ITransient
{
    private readonly SqlSugarRepository<SaveLifeEnrollmentRecord> _recordRep;
    private readonly SqlSugarRepository<SaveLifeClinicalTrial> _trialRep;
    private readonly SqlSugarRepository<SaveLifeUserProfile> _profileRep;
    private readonly SqlSugarRepository<SaveLifePatientProfile> _patientRep;

    public SaveLifePatientEnrollmentService(
        SqlSugarRepository<SaveLifeEnrollmentRecord> recordRep,
        SqlSugarRepository<SaveLifeClinicalTrial> trialRep,
        SqlSugarRepository<SaveLifeUserProfile> profileRep,
        SqlSugarRepository<SaveLifePatientProfile> patientRep)
    {
        _recordRep = recordRep;
        _trialRep = trialRep;
        _profileRep = profileRep;
        _patientRep = patientRep;
    }

    public async Task<long> ResolvePatientProfileIdAsync(
        string externalUserId,
        long? requestedPatientProfileId,
        bool allowSelfFallback)
    {
        var patient = await ResolvePatientProfileAsync(
            externalUserId,
            requestedPatientProfileId,
            allowSelfFallback);
        return patient.Id;
    }

    public async Task<SaveLifePatientEnrollmentSubmissionResult> SubmitConfirmedAsync(
        SaveLifePatientEnrollmentSubmissionInput input)
    {
        return await SubmitConfirmedInTransactionAsync(_recordRep.Context, input);
    }

    public async Task<SaveLifePatientEnrollmentSubmissionResult> SubmitConfirmedInTransactionAsync(
        ISqlSugarClient db,
        SaveLifePatientEnrollmentSubmissionInput input,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(input);
        var externalUserId = NormalizeExternalUserId(input.ExternalUserId);
        if (input.ProjectId <= 0)
            throw Oops.Oh("请先选择并确认具体招募项目").StatusCode(409);

        var patient = await ResolvePatientProfileAsync(
            db,
            externalUserId,
            input.PatientProfileId,
            allowSelfFallback: false,
            cancellationToken: cancellationToken);
        var trial = await db.Queryable<SaveLifeClinicalTrial>().FirstAsync(item =>
            item.Id == input.ProjectId &&
            item.Status == SaveLifeAgentTrialSelection.RecruitingStatus &&
            !item.IsDelete,
            cancellationToken);
        if (trial == null)
            throw Oops.Oh("招募项目不存在或当前不在招募中").StatusCode(409);

        var profile = await db.Queryable<SaveLifeUserProfile>().FirstAsync(item =>
            item.ExternalUserId == externalUserId &&
            !item.IsDelete,
            cancellationToken);
        var fields = input.Fields ?? new SaveLifeEnrollmentSubmissionFields();
        var validation = SaveLifeEnrollmentValidation.ValidatePatientForSubmission(
            profile,
            patient,
            fields);
        var status = validation.CanSubmit ? "pending" : "draft";
        var submittedTime = DateTime.Now;
        var enrollment = await SaveLifeAtomicUpsert.ExecuteAsync(
            async () => await db.Queryable<SaveLifeEnrollmentRecord>()
                .ClearFilter()
                .Where(item =>
                    item.PatientProfileId == patient.Id &&
                    item.ProjectId == trial.Id)
                .FirstAsync(cancellationToken),
            () => new SaveLifeEnrollmentRecord
            {
                ExternalUserId = externalUserId,
                PatientProfileId = patient.Id,
                ProjectId = trial.Id
            },
            entity =>
            {
                if (SaveLifeEnrollmentValidation.IsBlockingExistingSubmission(entity))
                    return;

                entity.ExternalUserId = externalUserId;
                entity.PatientProfileId = patient.Id;
                entity.UserProfileId = profile?.Id;
                entity.ProjectId = trial.Id;
                entity.ProjectTitle = trial.Title;
                SaveLifeEnrollmentValidation.PrepareForSubmission(entity, status, submittedTime);
                SaveLifeEnrollmentValidation.ApplySubmissionFields(entity, fields, profile, patient);
            },
            async entity =>
            {
                await db.Insertable(entity).ExecuteCommandAsync(cancellationToken);
            },
            async entity =>
            {
                await db.Updateable(entity).ExecuteCommandAsync(cancellationToken);
            });

        return new SaveLifePatientEnrollmentSubmissionResult
        {
            Enrollment = enrollment,
            Validation = validation
        };
    }

    private async Task<SaveLifePatientProfile> ResolvePatientProfileAsync(
        string externalUserId,
        long? requestedPatientProfileId,
        bool allowSelfFallback)
    {
        return await ResolvePatientProfileAsync(
            _patientRep.Context,
            externalUserId,
            requestedPatientProfileId,
            allowSelfFallback,
            cancellationToken: default);
    }

    private static async Task<SaveLifePatientProfile> ResolvePatientProfileAsync(
        ISqlSugarClient db,
        string externalUserId,
        long? requestedPatientProfileId,
        bool allowSelfFallback,
        CancellationToken cancellationToken)
    {
        var owner = NormalizeExternalUserId(externalUserId);
        if (requestedPatientProfileId.HasValue && requestedPatientProfileId.Value <= 0)
            throw Oops.Oh("患者档案 Id 格式不正确").StatusCode(400);
        if (!requestedPatientProfileId.HasValue && !allowSelfFallback)
            throw Oops.Oh("请先确认本次报名对应的患者").StatusCode(409);

        SaveLifePatientProfile? patient = null;
        if (requestedPatientProfileId is > 0)
        {
            var patientProfileId = requestedPatientProfileId.Value;
            patient = await db.Queryable<SaveLifePatientProfile>().FirstAsync(item =>
                item.Id == patientProfileId &&
                item.OwnerExternalUserId == owner &&
                !item.IsDelete,
                cancellationToken);
        }
        else if (allowSelfFallback)
        {
            patient = await db.Queryable<SaveLifePatientProfile>().FirstAsync(item =>
                item.OwnerExternalUserId == owner &&
                item.RelationshipToAccount == SaveLifePatientRelationships.Self &&
                !item.IsDelete,
                cancellationToken);
        }

        SaveLifePatientOwnershipPolicy.EnsureOwnedBy(patient, owner);
        return patient!;
    }

    private static string NormalizeExternalUserId(string? externalUserId)
    {
        var normalized = externalUserId?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
            throw Oops.Oh("报名账号不能为空").StatusCode(400);

        return normalized;
    }
}
