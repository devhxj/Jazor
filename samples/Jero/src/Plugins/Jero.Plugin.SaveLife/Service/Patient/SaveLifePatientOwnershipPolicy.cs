using System.Security.Cryptography;
using System.Text;

namespace Jero.Plugin.SaveLife.Service;

public interface ISaveLifePatientIdGenerator
{
    long NextId();
}

public sealed class SaveLifePatientIdGenerator : ISaveLifePatientIdGenerator, ITransient
{
    public long NextId()
    {
        return Yitter.IdGenerator.YitIdHelper.NextId();
    }
}

public static class SaveLifePatientOwnershipPolicy
{
    private static readonly HashSet<string> Relationships =
    [
        SaveLifePatientRelationships.Self,
        SaveLifePatientRelationships.Parent,
        SaveLifePatientRelationships.Child,
        SaveLifePatientRelationships.Spouse,
        SaveLifePatientRelationships.Relative,
        SaveLifePatientRelationships.Other
    ];

    public static bool IsOwnedBy(SaveLifePatientProfile? patient, string ownerExternalUserId)
    {
        return patient != null &&
            !patient.IsDelete &&
            !string.IsNullOrWhiteSpace(ownerExternalUserId) &&
            string.Equals(
                patient.OwnerExternalUserId,
                ownerExternalUserId.Trim(),
                StringComparison.Ordinal);
    }

    public static void EnsureOwnedBy(SaveLifePatientProfile? patient, string ownerExternalUserId)
    {
        if (!IsOwnedBy(patient, ownerExternalUserId))
            throw Oops.Oh("患者档案不存在或无权访问").StatusCode(404);
    }

    public static SaveLifePatientRelationship NormalizeRelationship(
        string? relationship,
        string? description)
    {
        var code = relationship?.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(code) || !Relationships.Contains(code))
            throw Oops.Oh("患者关系取值不正确").StatusCode(400);

        if (code == SaveLifePatientRelationships.Self)
            return new SaveLifePatientRelationship(code, "本人");

        var normalizedDescription = SaveLifeMiniAppProfileHelper.NormalizeText(
            description,
            64,
            "具体关系");
        if (normalizedDescription == null)
            throw Oops.Oh("请填写与患者的具体关系，例如母亲、父亲或配偶").StatusCode(400);

        return new SaveLifePatientRelationship(code, normalizedDescription);
    }

    public static void EnsureRelationshipChangeAllowed(
        string currentRelationship,
        string requestedRelationship)
    {
        var changesSelfIdentity =
            (currentRelationship == SaveLifePatientRelationships.Self) !=
            (requestedRelationship == SaveLifePatientRelationships.Self);
        if (changesSelfIdentity)
            throw Oops.Oh("本人档案身份不能通过普通更新修改").StatusCode(409);
    }
}

public static class SaveLifePatientOnboardingPolicy
{
    public static string NormalizeClientCommandId(string? clientCommandId)
    {
        return SaveLifeMiniAppProfileHelper.NormalizeText(
                clientCommandId,
                64,
                "患者建档命令 Id")
            ?? throw Oops.Oh("患者建档命令 Id 不能为空").StatusCode(400);
    }

    public static string BuildAccountPatientKey(
        string ownerExternalUserId,
        string clientCommandId)
    {
        var owner = ownerExternalUserId?.Trim();
        if (string.IsNullOrWhiteSpace(owner))
            throw new ArgumentException("账号外部用户 Id 不能为空", nameof(ownerExternalUserId));

        var command = NormalizeClientCommandId(clientCommandId);
        var digest = SHA256.HashData(Encoding.UTF8.GetBytes($"{owner}\n{command}"));
        return $"onboarding:{Convert.ToHexString(digest).ToLowerInvariant()}";
    }

    public static void EnsureIdempotentPayload(
        SaveLifePatientProfile patient,
        SaveLifePatientRelationship relationship,
        string displayName)
    {
        ArgumentNullException.ThrowIfNull(patient);
        if (!string.Equals(
                patient.RelationshipToAccount,
                relationship.Code,
                StringComparison.Ordinal) ||
            !string.Equals(patient.DisplayName, displayName, StringComparison.Ordinal))
        {
            throw Oops.Oh("患者建档命令 Id 已用于其他患者信息").StatusCode(409);
        }
    }
}

public static class SaveLifePatientSwitchPolicy
{
    public static async Task ConfirmAsync(
        ISqlSugarClient db,
        SaveLifeAgentConversation conversation,
        SaveLifePatientProfile targetPatient,
        DateTime confirmedTime,
        long? pendingConsultationId = null)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(conversation);
        ArgumentNullException.ThrowIfNull(targetPatient);

        var owner = conversation.ExternalUserId?.Trim();
        if (string.IsNullOrWhiteSpace(owner))
            throw Oops.Oh("会话不存在或无权切换患者").StatusCode(404);

        SaveLifePatientOwnershipPolicy.EnsureOwnedBy(targetPatient, owner);

        var transaction = await db.Ado.UseTranAsync(
            () => ConfirmInTransactionAsync(
                db,
                conversation.Id,
                owner,
                targetPatient.Id,
                confirmedTime,
                pendingConsultationId),
            _ => { });

        if (!transaction.IsSuccess)
            throw transaction.ErrorException ?? new InvalidOperationException("患者切换事务失败");

        targetPatient.IsDefault = true;
        CopySwitchState(transaction.Data, conversation);
    }

    internal static async Task<SaveLifeAgentConversation> ConfirmInTransactionAsync(
        ISqlSugarClient db,
        long conversationId,
        string ownerExternalUserId,
        long targetPatientProfileId,
        DateTime confirmedTime,
        long? pendingConsultationId = null)
    {
        var persistedConversation = await db.Queryable<SaveLifeAgentConversation>()
            .Where(item =>
                item.Id == conversationId &&
                item.ExternalUserId == ownerExternalUserId &&
                item.Status == "active" &&
                !item.IsDelete)
            .FirstAsync();
        if (persistedConversation == null)
            throw Oops.Oh("会话不存在或无权切换患者").StatusCode(404);

        var persistedTarget = await db.Queryable<SaveLifePatientProfile>()
            .Where(item =>
                item.Id == targetPatientProfileId &&
                item.OwnerExternalUserId == ownerExternalUserId &&
                !item.IsDelete)
            .FirstAsync();
        SaveLifePatientOwnershipPolicy.EnsureOwnedBy(persistedTarget, ownerExternalUserId);

        await db.Updateable<SaveLifePatientProfile>()
            .SetColumns(item => new SaveLifePatientProfile { IsDefault = false })
            .Where(item =>
                item.OwnerExternalUserId == ownerExternalUserId &&
                item.Id != persistedTarget!.Id &&
                item.IsDefault &&
                !item.IsDelete)
            .ExecuteCommandAsync();
        var selectedDefault = await db.Updateable<SaveLifePatientProfile>()
            .SetColumns(item => new SaveLifePatientProfile { IsDefault = true })
            .Where(item =>
                item.Id == persistedTarget!.Id &&
                item.OwnerExternalUserId == ownerExternalUserId &&
                !item.IsDelete)
            .ExecuteCommandAsync();
        if (selectedDefault != 1)
            throw new InvalidOperationException("当前患者偏好保存失败");
        persistedTarget!.IsDefault = true;

        if (persistedConversation.CurrentPatientProfileId == persistedTarget.Id)
            return persistedConversation;

        var nextState = BuildStateAfterSwitch(
            persistedConversation,
            persistedTarget.Id,
            pendingConsultationId);
        persistedConversation.CurrentPatientProfileId = persistedTarget.Id;
        persistedConversation.PatientContextVersion = checked(
            Math.Max(1, persistedConversation.PatientContextVersion) + 1);
        persistedConversation.FlowStateVersion = SaveLifeAgentConversationState.CurrentVersion;
        persistedConversation.FlowStateJson = SaveLifeAgentConversationStateCodec.Serialize(nextState);
        persistedConversation.AgentSessionJson = null;
        persistedConversation.Phase = "created";
        persistedConversation.MatchedTrialsJson = null;
        persistedConversation.ContextType = null;
        persistedConversation.ContextId = null;
        persistedConversation.EnrollmentRecordId = null;
        persistedConversation.UpdateTime = confirmedTime;

        await SaveLifeAgentConversationRevisionGuard.AdvanceAsync(
            db,
            persistedConversation,
            item => new SaveLifeAgentConversation
            {
                CurrentPatientProfileId = persistedConversation.CurrentPatientProfileId,
                PatientContextVersion = persistedConversation.PatientContextVersion,
                FlowStateVersion = persistedConversation.FlowStateVersion,
                FlowStateJson = persistedConversation.FlowStateJson,
                AgentSessionJson = null,
                Phase = persistedConversation.Phase,
                MatchedTrialsJson = persistedConversation.MatchedTrialsJson,
                ContextType = persistedConversation.ContextType,
                ContextId = persistedConversation.ContextId,
                EnrollmentRecordId = persistedConversation.EnrollmentRecordId
            },
            confirmedTime);

        await db.Updateable<SaveLifePatientProfileProposal>()
            .SetColumns(item => new SaveLifePatientProfileProposal
            {
                Status = SaveLifePatientProfileProposalStatuses.Rejected,
                ResolvedTime = confirmedTime,
                UpdateTime = confirmedTime
            })
            .Where(item =>
                item.ConversationId == persistedConversation.Id &&
                item.OwnerExternalUserId == ownerExternalUserId &&
                item.Status == SaveLifePatientProfileProposalStatuses.Pending &&
                !item.IsDelete)
            .ExecuteCommandAsync();

        return persistedConversation;
    }

    private static SaveLifeAgentConversationState BuildStateAfterSwitch(
        SaveLifeAgentConversation conversation,
        long targetPatientProfileId,
        long? pendingConsultationId)
    {
        var current = SaveLifeAgentConversationStateCodec.Deserialize(conversation.FlowStateJson);
        SaveLifeAgentTrialOptOutPolicy.NormalizePatientScopes(current);
        SaveLifeAgentTrialOptOutPolicy.MigrateLegacyScope(
            current,
            conversation.CurrentPatientProfileId);
        SaveLifeAgentPendingConsultationState? pending = null;
        if (pendingConsultationId.HasValue)
        {
            pending = current.PendingConsultation;
            if (pending == null ||
                pending.Id != pendingConsultationId.Value ||
                pending.Status != SaveLifeAgentPendingConsultationStatuses.Pending)
            {
                throw Oops.Oh("待继续咨询已变化，请刷新后重试").StatusCode(409);
            }
        }

        return new SaveLifeAgentConversationState
        {
            GeneralTrialOptOut = current.GeneralTrialOptOut,
            IsGeneralGuidanceDeferred = current.IsGeneralGuidanceDeferred,
            PendingConsultation = pending == null
                ? null
                : SaveLifeAgentPendingConsultationPolicy.Preserve(pending).Current,
            TrialOptOutPatientProfileIds = [.. current.TrialOptOutPatientProfileIds],
            TrialOptOut = SaveLifeAgentTrialOptOutPolicy.IsSuppressed(
                current,
                targetPatientProfileId)
        };
    }

    private static void CopySwitchState(
        SaveLifeAgentConversation source,
        SaveLifeAgentConversation destination)
    {
        destination.CurrentPatientProfileId = source.CurrentPatientProfileId;
        destination.PatientContextVersion = source.PatientContextVersion;
        destination.FlowStateVersion = source.FlowStateVersion;
        destination.FlowStateJson = source.FlowStateJson;
        destination.AgentSessionJson = source.AgentSessionJson;
        destination.Phase = source.Phase;
        destination.MatchedTrialsJson = source.MatchedTrialsJson;
        destination.ContextType = source.ContextType;
        destination.ContextId = source.ContextId;
        destination.EnrollmentRecordId = source.EnrollmentRecordId;
        destination.UpdateTime = source.UpdateTime;
        destination.StateRevision = source.StateRevision;
    }
}

public sealed record SaveLifePatientRelationship(string Code, string Description);

public static class SaveLifePatientProfileMapper
{
    public static SaveLifePatientProfileOutput ToOutput(
        SaveLifePatientProfile patient,
        long? currentPatientProfileId)
    {
        ArgumentNullException.ThrowIfNull(patient);
        var completeness = SaveLifePatientProfileCompleteness.Calculate(patient);

        return new SaveLifePatientProfileOutput
        {
            Id = patient.Id.ToString(),
            RelationshipToAccount = patient.RelationshipToAccount,
            RelationshipDescription = patient.RelationshipDescription,
            DisplayName = patient.DisplayName,
            IsDefault = patient.IsDefault,
            IsCurrent = currentPatientProfileId == patient.Id,
            ProfileCompleteness = completeness,
            Gender = patient.Gender,
            BirthDate = patient.BirthDate,
            Ethnicity = patient.Ethnicity,
            City = patient.City,
            Residence = patient.Residence,
            Diagnosis = patient.Diagnosis,
            DiseaseStage = patient.DiseaseStage,
            DiseaseStatus = patient.DiseaseStatus,
            TreatmentHistory = patient.TreatmentHistory,
            CurrentTreatment = patient.CurrentTreatment,
            Pathology = patient.Pathology,
            Biomarkers = patient.Biomarkers,
            Comorbidities = patient.Comorbidities,
            MedicalHistory = patient.MedicalHistory,
            Allergies = patient.Allergies,
            CreatedAt = patient.CreateTime,
            UpdatedAt = patient.UpdateTime
        };
    }
}

public static class SaveLifePatientProfileCompleteness
{
    public static int Calculate(SaveLifePatientProfile patient)
    {
        ArgumentNullException.ThrowIfNull(patient);

        var score = 0;
        if (!string.IsNullOrWhiteSpace(patient.DisplayName) &&
            !string.IsNullOrWhiteSpace(patient.Gender) &&
            patient.BirthDate.HasValue)
        {
            score += 25;
        }
        if (!string.IsNullOrWhiteSpace(patient.City) || !string.IsNullOrWhiteSpace(patient.Residence))
            score += 15;
        if (!string.IsNullOrWhiteSpace(patient.Diagnosis))
            score += 25;
        if (!string.IsNullOrWhiteSpace(patient.DiseaseStage) || !string.IsNullOrWhiteSpace(patient.DiseaseStatus))
            score += 15;
        if (!string.IsNullOrWhiteSpace(patient.TreatmentHistory) || !string.IsNullOrWhiteSpace(patient.CurrentTreatment))
            score += 10;
        if (!string.IsNullOrWhiteSpace(patient.Pathology) || !string.IsNullOrWhiteSpace(patient.Biomarkers))
            score += 10;

        return score;
    }
}

public static class SaveLifePatientProfileCompatibility
{
    public static void ApplyPatientToLegacy(
        SaveLifePatientProfile patient,
        SaveLifeUserProfile legacyProfile)
    {
        ArgumentNullException.ThrowIfNull(patient);
        ArgumentNullException.ThrowIfNull(legacyProfile);

        legacyProfile.RealName = patient.DisplayName;
        legacyProfile.Gender = patient.Gender;
        legacyProfile.BirthDate = patient.BirthDate;
        legacyProfile.Address = patient.Residence;
        legacyProfile.Ethnicity = patient.Ethnicity;
        legacyProfile.MedicalHistory = patient.MedicalHistory;
        legacyProfile.Allergies = patient.Allergies;
        legacyProfile.UpdateTime = DateTime.Now;
    }

    public static void ApplyLegacyToSelf(
        SaveLifeUserProfile legacyProfile,
        SaveLifePatientProfile selfPatient)
    {
        ArgumentNullException.ThrowIfNull(legacyProfile);
        ArgumentNullException.ThrowIfNull(selfPatient);

        selfPatient.DisplayName = string.IsNullOrWhiteSpace(legacyProfile.RealName)
            ? "本人"
            : legacyProfile.RealName.Trim();
        selfPatient.Gender = legacyProfile.Gender;
        selfPatient.BirthDate = legacyProfile.BirthDate;
        selfPatient.Residence = legacyProfile.Address;
        selfPatient.Ethnicity = legacyProfile.Ethnicity;
        selfPatient.MedicalHistory = legacyProfile.MedicalHistory;
        selfPatient.Allergies = legacyProfile.Allergies;
        selfPatient.UpdateTime = DateTime.Now;
    }
}
