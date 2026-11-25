namespace Jero.Plugin.SaveLife.Service;

internal static class SaveLifePatientProfileProposalPolicy
{
    internal static readonly TimeSpan Lifetime = TimeSpan.FromDays(7);

    public static string NormalizeSourceType(string? sourceType)
    {
        var normalized = sourceType?.Trim().ToLowerInvariant();
        return normalized switch
        {
            SaveLifePatientProfileProposalSources.UserMessage => normalized,
            SaveLifePatientProfileProposalSources.Material => normalized,
            _ => throw new SaveLifeAgentProfileProposalValidationException("档案提案来源类型无效")
        };
    }

    public static SaveLifePatientProfileProposalSource NormalizeSource(
        string? sourceType,
        long? sourceMaterialId,
        string? evidence,
        string? currentUserMessage,
        IReadOnlyList<SaveLifeAgentPreparedMaterialEvidence> authorizedMaterials)
    {
        ArgumentNullException.ThrowIfNull(authorizedMaterials);

        var normalizedType = NormalizeSourceType(sourceType);
        var normalizedEvidence = NormalizeEvidence(evidence);
        if (normalizedType == SaveLifePatientProfileProposalSources.UserMessage)
        {
            if (sourceMaterialId.HasValue ||
                string.IsNullOrEmpty(currentUserMessage) ||
                !currentUserMessage.Contains(normalizedEvidence, StringComparison.Ordinal))
            {
                throw new SaveLifeAgentProfileProposalValidationException(
                    "消息来源的档案提案证据必须逐字来自最新消息");
            }

            return new SaveLifePatientProfileProposalSource(
                normalizedType,
                null,
                normalizedEvidence);
        }

        if (sourceMaterialId is not > 0)
            throw new SaveLifeAgentProfileProposalValidationException("材料来源的档案提案缺少材料Id");
        var material = authorizedMaterials.FirstOrDefault(item => item.MaterialId == sourceMaterialId.Value)
            ?? throw new SaveLifeAgentProfileProposalValidationException("档案提案材料未在本轮授权");
        if (material.Kind == SaveLifeAgentModelContentTypes.Text)
        {
            if (string.IsNullOrEmpty(material.ExtractedText) ||
                !material.ExtractedText.Contains(normalizedEvidence, StringComparison.Ordinal))
            {
                throw new SaveLifeAgentProfileProposalValidationException(
                    "材料来源的档案提案证据必须逐字来自提取文本");
            }
        }
        else if (material.Kind != SaveLifeAgentModelContentTypes.Image)
        {
            throw new SaveLifeAgentProfileProposalValidationException("该材料类型不能用于档案提案");
        }

        return new SaveLifePatientProfileProposalSource(
            normalizedType,
            sourceMaterialId,
            normalizedEvidence);
    }

    public static string NormalizeValue(string field, string? value)
    {
        if (!SaveLifePatientProfileProposalFields.IsAllowed(field))
            throw new SaveLifeAgentProfileProposalValidationException("档案提案字段不在允许范围内");

        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
            throw new SaveLifeAgentProfileProposalValidationException("档案提案值不能为空");
        if (normalized.Length > GetMaximumLength(field))
            throw new SaveLifeAgentProfileProposalValidationException("档案提案值超过字段长度限制");

        return normalized;
    }

    public static string NormalizeEvidence(string? evidence)
    {
        var normalized = evidence?.Trim();
        if (string.IsNullOrWhiteSpace(normalized) || normalized.Length > 500)
            throw new SaveLifeAgentProfileProposalValidationException("档案提案证据无效");

        return normalized;
    }

    public static string? ReadValue(SaveLifePatientProfile patient, string field)
    {
        return field switch
        {
            SaveLifePatientProfileProposalFields.City => patient.City,
            SaveLifePatientProfileProposalFields.Diagnosis => patient.Diagnosis,
            SaveLifePatientProfileProposalFields.DiseaseStage => patient.DiseaseStage,
            SaveLifePatientProfileProposalFields.DiseaseStatus => patient.DiseaseStatus,
            SaveLifePatientProfileProposalFields.TreatmentHistory => patient.TreatmentHistory,
            SaveLifePatientProfileProposalFields.CurrentTreatment => patient.CurrentTreatment,
            SaveLifePatientProfileProposalFields.Pathology => patient.Pathology,
            SaveLifePatientProfileProposalFields.Biomarkers => patient.Biomarkers,
            SaveLifePatientProfileProposalFields.Comorbidities => patient.Comorbidities,
            SaveLifePatientProfileProposalFields.MedicalHistory => patient.MedicalHistory,
            SaveLifePatientProfileProposalFields.Allergies => patient.Allergies,
            _ => throw new SaveLifeAgentProfileProposalValidationException("档案提案字段不在允许范围内")
        };
    }

    public static void ApplyValue(SaveLifePatientProfile patient, string field, string value)
    {
        var normalized = NormalizeValue(field, value);
        switch (field)
        {
            case SaveLifePatientProfileProposalFields.City:
                patient.City = normalized;
                break;
            case SaveLifePatientProfileProposalFields.Diagnosis:
                patient.Diagnosis = normalized;
                break;
            case SaveLifePatientProfileProposalFields.DiseaseStage:
                patient.DiseaseStage = normalized;
                break;
            case SaveLifePatientProfileProposalFields.DiseaseStatus:
                patient.DiseaseStatus = normalized;
                break;
            case SaveLifePatientProfileProposalFields.TreatmentHistory:
                patient.TreatmentHistory = normalized;
                break;
            case SaveLifePatientProfileProposalFields.CurrentTreatment:
                patient.CurrentTreatment = normalized;
                break;
            case SaveLifePatientProfileProposalFields.Pathology:
                patient.Pathology = normalized;
                break;
            case SaveLifePatientProfileProposalFields.Biomarkers:
                patient.Biomarkers = normalized;
                break;
            case SaveLifePatientProfileProposalFields.Comorbidities:
                patient.Comorbidities = normalized;
                break;
            case SaveLifePatientProfileProposalFields.MedicalHistory:
                patient.MedicalHistory = normalized;
                break;
            case SaveLifePatientProfileProposalFields.Allergies:
                patient.Allergies = normalized;
                break;
        }
    }

    public static bool ValuesEqual(string? left, string? right)
    {
        return string.Equals(
            NormalizeComparableValue(left),
            NormalizeComparableValue(right),
            StringComparison.Ordinal);
    }

    public static string GetPatientPropertyName(string field)
    {
        return field switch
        {
            SaveLifePatientProfileProposalFields.City => nameof(SaveLifePatientProfile.City),
            SaveLifePatientProfileProposalFields.Diagnosis => nameof(SaveLifePatientProfile.Diagnosis),
            SaveLifePatientProfileProposalFields.DiseaseStage => nameof(SaveLifePatientProfile.DiseaseStage),
            SaveLifePatientProfileProposalFields.DiseaseStatus => nameof(SaveLifePatientProfile.DiseaseStatus),
            SaveLifePatientProfileProposalFields.TreatmentHistory => nameof(SaveLifePatientProfile.TreatmentHistory),
            SaveLifePatientProfileProposalFields.CurrentTreatment => nameof(SaveLifePatientProfile.CurrentTreatment),
            SaveLifePatientProfileProposalFields.Pathology => nameof(SaveLifePatientProfile.Pathology),
            SaveLifePatientProfileProposalFields.Biomarkers => nameof(SaveLifePatientProfile.Biomarkers),
            SaveLifePatientProfileProposalFields.Comorbidities => nameof(SaveLifePatientProfile.Comorbidities),
            SaveLifePatientProfileProposalFields.MedicalHistory => nameof(SaveLifePatientProfile.MedicalHistory),
            SaveLifePatientProfileProposalFields.Allergies => nameof(SaveLifePatientProfile.Allergies),
            _ => throw new SaveLifeAgentProfileProposalValidationException("档案提案字段不在允许范围内")
        };
    }

    public static SaveLifePatientProfileProposalOutput ToOutput(
        SaveLifePatientProfileProposal proposal)
    {
        return new SaveLifePatientProfileProposalOutput
        {
            Id = proposal.Id.ToString(),
            ConversationId = proposal.ConversationId.ToString(),
            PatientProfileId = proposal.PatientProfileId.ToString(),
            SourceUserMessageId = proposal.SourceUserMessageId.ToString(),
            SourceAssistantMessageId = proposal.SourceAssistantMessageId.ToString(),
            SourceType = proposal.SourceType,
            SourceMaterialId = proposal.SourceMaterialId?.ToString(),
            Field = proposal.FieldName,
            FieldLabel = GetLabel(proposal.FieldName),
            OldValue = proposal.OldValue,
            ProposedValue = proposal.ProposedValue,
            Evidence = proposal.Evidence,
            Status = proposal.Status,
            ExpiresAt = proposal.ExpiresTime,
            ResolvedAt = proposal.ResolvedTime
        };
    }

    private static int GetMaximumLength(string field)
    {
        return field switch
        {
            SaveLifePatientProfileProposalFields.City => 128,
            SaveLifePatientProfileProposalFields.Diagnosis => 256,
            SaveLifePatientProfileProposalFields.DiseaseStage => 128,
            SaveLifePatientProfileProposalFields.DiseaseStatus => 256,
            _ => 8000
        };
    }

    private static string? NormalizeComparableValue(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string GetLabel(string field)
    {
        return field switch
        {
            SaveLifePatientProfileProposalFields.City => "所在城市",
            SaveLifePatientProfileProposalFields.Diagnosis => "疾病或诊断",
            SaveLifePatientProfileProposalFields.DiseaseStage => "疾病阶段",
            SaveLifePatientProfileProposalFields.DiseaseStatus => "当前病情状态",
            SaveLifePatientProfileProposalFields.TreatmentHistory => "既往治疗",
            SaveLifePatientProfileProposalFields.CurrentTreatment => "当前治疗",
            SaveLifePatientProfileProposalFields.Pathology => "病理信息",
            SaveLifePatientProfileProposalFields.Biomarkers => "分子标志物",
            SaveLifePatientProfileProposalFields.Comorbidities => "合并疾病",
            SaveLifePatientProfileProposalFields.MedicalHistory => "既往病史",
            SaveLifePatientProfileProposalFields.Allergies => "过敏史",
            _ => field
        };
    }
}

internal sealed record SaveLifePatientProfileProposalSource(
    string SourceType,
    long? SourceMaterialId,
    string Evidence);

internal sealed class SaveLifeAgentProfileUpdateProposalOutput
{
    public long? Id { get; set; }

    public string Field { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;

    public string SourceType { get; set; } = SaveLifePatientProfileProposalSources.UserMessage;

    public long? SourceMaterialId { get; set; }

    public string Evidence { get; set; } = string.Empty;

    public DateTime? ExpiresTime { get; set; }
}

internal static class SaveLifePatientProfileProposalPersistence
{
    public static async Task<List<SaveLifePatientProfileProposalOutput>> CreatePendingAsync(
        ISqlSugarClient db,
        SaveLifeAgentConversation conversation,
        SaveLifeAgentMessage userMessage,
        long assistantMessageId,
        SaveLifePatientProfile patient,
        IReadOnlyList<SaveLifeAgentProfileUpdateProposalOutput> requestedProposals,
        DateTime now,
        Func<long> nextId,
        IReadOnlyList<SaveLifeAgentPreparedMaterialEvidence>? authorizedMaterials = null)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(conversation);
        ArgumentNullException.ThrowIfNull(userMessage);
        ArgumentNullException.ThrowIfNull(patient);
        ArgumentNullException.ThrowIfNull(requestedProposals);
        ArgumentNullException.ThrowIfNull(nextId);

        if (requestedProposals.Count == 0)
            return [];
        if (requestedProposals.Count > 1)
            throw new SaveLifeAgentProfileProposalValidationException("单轮档案提案不能超过 1 项");
        if (assistantMessageId <= 0 ||
            conversation.CurrentPatientProfileId != patient.Id ||
            userMessage.PatientProfileId != patient.Id ||
            userMessage.ConversationId != conversation.Id ||
            !string.Equals(conversation.ExternalUserId, patient.OwnerExternalUserId, StringComparison.Ordinal) ||
            !string.Equals(userMessage.ExternalUserId, patient.OwnerExternalUserId, StringComparison.Ordinal))
        {
            throw new SaveLifeAgentProfileProposalValidationException("档案提案患者上下文不一致");
        }
        if (requestedProposals.Any(item => item == null))
            throw new SaveLifeAgentProfileProposalValidationException("档案提案不能为空");

        authorizedMaterials ??= [];
        var normalized = requestedProposals.Select(item =>
        {
            var source = SaveLifePatientProfileProposalPolicy.NormalizeSource(
                item.SourceType,
                item.SourceMaterialId,
                item.Evidence,
                userMessage.Content,
                authorizedMaterials);
            return new
            {
                item.Id,
                item.ExpiresTime,
                Field = item.Field,
                Value = SaveLifePatientProfileProposalPolicy.NormalizeValue(item.Field, item.Value),
                source.SourceType,
                source.SourceMaterialId,
                source.Evidence
            };
        }).ToList();

        // The caller owns the transaction that also completes the assistant reply.
        var fields = normalized.Select(item => item.Field).ToList();
        await db.Updateable<SaveLifePatientProfileProposal>()
            .SetColumns(item => new SaveLifePatientProfileProposal
            {
                Status = SaveLifePatientProfileProposalStatuses.Expired,
                ResolvedTime = now,
                UpdateTime = now
            })
            .Where(item =>
                item.OwnerExternalUserId == patient.OwnerExternalUserId &&
                item.PatientProfileId == patient.Id &&
                item.Status == SaveLifePatientProfileProposalStatuses.Pending &&
                item.ExpiresTime <= now &&
                !item.IsDelete)
            .ExecuteCommandAsync();
        await db.Updateable<SaveLifePatientProfileProposal>()
            .SetColumns(item => new SaveLifePatientProfileProposal
            {
                Status = SaveLifePatientProfileProposalStatuses.Superseded,
                ResolvedTime = now,
                UpdateTime = now
            })
            .Where(item =>
                item.OwnerExternalUserId == patient.OwnerExternalUserId &&
                item.PatientProfileId == patient.Id &&
                fields.Contains(item.FieldName) &&
                item.SourceUserMessageId != userMessage.Id &&
                item.Status == SaveLifePatientProfileProposalStatuses.Pending &&
                !item.IsDelete)
            .ExecuteCommandAsync();

        var created = new List<SaveLifePatientProfileProposalOutput>(normalized.Count);
        foreach (var item in normalized)
        {
            var existing = await db.Queryable<SaveLifePatientProfileProposal>()
                .Where(proposal =>
                    proposal.OwnerExternalUserId == patient.OwnerExternalUserId &&
                    proposal.ConversationId == conversation.Id &&
                    proposal.PatientProfileId == patient.Id &&
                    proposal.SourceUserMessageId == userMessage.Id &&
                    proposal.FieldName == item.Field &&
                    !proposal.IsDelete)
                .FirstAsync();
            if (existing != null)
            {
                if (!SaveLifePatientProfileProposalPolicy.ValuesEqual(existing.ProposedValue, item.Value) ||
                    !string.Equals(existing.SourceType, item.SourceType, StringComparison.Ordinal) ||
                    existing.SourceMaterialId != item.SourceMaterialId ||
                    !string.Equals(existing.Evidence, item.Evidence, StringComparison.Ordinal))
                {
                    throw new SaveLifeAgentProfileProposalValidationException("同一来源消息的档案提案不一致");
                }

                created.Add(SaveLifePatientProfileProposalPolicy.ToOutput(existing));
                continue;
            }

            var id = item.Id is > 0 ? item.Id.Value : nextId();
            if (id <= 0)
                throw new InvalidOperationException("档案提案 Id 生成失败");
            var proposal = new SaveLifePatientProfileProposal
            {
                Id = id,
                TenantId = patient.TenantId,
                OwnerExternalUserId = patient.OwnerExternalUserId,
                ConversationId = conversation.Id,
                PatientProfileId = patient.Id,
                SourceUserMessageId = userMessage.Id,
                SourceAssistantMessageId = assistantMessageId,
                SourceType = item.SourceType,
                SourceMaterialId = item.SourceMaterialId,
                FieldName = item.Field,
                OldValue = SaveLifePatientProfileProposalPolicy.ReadValue(patient, item.Field),
                ProposedValue = item.Value,
                Evidence = item.Evidence,
                Status = SaveLifePatientProfileProposalStatuses.Pending,
                ExpiresTime = item.ExpiresTime ?? now.Add(SaveLifePatientProfileProposalPolicy.Lifetime),
                CreateTime = now
            };
            await db.Insertable(proposal).ExecuteCommandAsync();
            created.Add(SaveLifePatientProfileProposalPolicy.ToOutput(proposal));
        }

        return created;
    }
}
