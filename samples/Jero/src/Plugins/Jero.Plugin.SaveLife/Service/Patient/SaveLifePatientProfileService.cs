namespace Jero.Plugin.SaveLife.Service;

/// <summary>
/// 小程序患者档案资源服务
/// </summary>
[Authorize]
[ApiDescriptionSettings(SaveLifeConst.GroupName, Order = 145)]
public class SaveLifePatientProfileService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<SaveLifePatientProfile> _patientRep;
    private readonly SqlSugarRepository<SaveLifeAgentConversation> _conversationRep;
    private readonly SqlSugarRepository<SaveLifeUserProfile> _legacyProfileRep;
    private readonly ISaveLifePrivacyConsentGuard _privacyConsentGuard;
    private readonly ISaveLifePatientIdGenerator _patientIdGenerator;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SaveLifePatientProfileService(
        SqlSugarRepository<SaveLifePatientProfile> patientRep,
        SqlSugarRepository<SaveLifeAgentConversation> conversationRep,
        SqlSugarRepository<SaveLifeUserProfile> legacyProfileRep,
        ISaveLifePrivacyConsentGuard privacyConsentGuard,
        ISaveLifePatientIdGenerator patientIdGenerator,
        IHttpContextAccessor httpContextAccessor)
    {
        _patientRep = patientRep;
        _conversationRep = conversationRep;
        _legacyProfileRep = legacyProfileRep;
        _privacyConsentGuard = privacyConsentGuard;
        _patientIdGenerator = patientIdGenerator;
        _httpContextAccessor = httpContextAccessor;
    }

    [HttpGet("/api/v1/save-life/patients"), NonUnify]
    [ApiDescriptionSettings(Name = "GetPatients")]
    public async Task<SaveLifePatientProfileListOutput> GetPatients(
        [FromQuery] QuerySaveLifePatientProfilesInput? input = null)
    {
        var ownerExternalUserId = GetCurrentMiniAppOpenId();
        await _privacyConsentGuard.EnsureConsentedAsync(ownerExternalUserId);
        await GetOrCreateSelfPatient(ownerExternalUserId);

        var currentPatientProfileId = await GetCurrentPatientProfileId(ownerExternalUserId);
        var page = Math.Max(input?.Page ?? 1, 1);
        var pageSize = Math.Clamp(input?.PageSize ?? 20, 1, 50);
        var query = _patientRep.AsQueryable()
            .Where(u =>
                u.OwnerExternalUserId == ownerExternalUserId &&
                !u.IsDelete);
        var total = await query.CountAsync();
        var patients = await query
            .OrderBy(u => u.IsDefault, OrderByType.Desc)
            .OrderBy(u => u.CreateTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new SaveLifePatientProfileListOutput
        {
            Items = patients
                .Select(patient => SaveLifePatientProfileMapper.ToOutput(patient, currentPatientProfileId))
                .ToList(),
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }

    [HttpPost("/api/v1/save-life/patients"), NonUnify]
    [ApiDescriptionSettings(Name = "CreatePatient")]
    public async Task<SaveLifePatientProfileOutput> CreatePatient(
        CreateSaveLifePatientProfileInput input)
    {
        var ownerExternalUserId = GetCurrentMiniAppOpenId();
        await _privacyConsentGuard.EnsureConsentedAsync(ownerExternalUserId);
        await _privacyConsentGuard.EnsureSensitiveInformationConsentedAsync(ownerExternalUserId);
        var relationship = SaveLifePatientOwnershipPolicy.NormalizeRelationship(
            input.RelationshipToAccount,
            input.RelationshipDescription);
        if (relationship.Code == SaveLifePatientRelationships.Self &&
            await _patientRep.AsQueryable().AnyAsync(u =>
                u.OwnerExternalUserId == ownerExternalUserId &&
                u.RelationshipToAccount == SaveLifePatientRelationships.Self))
        {
            throw Oops.Oh("本人患者档案已存在").StatusCode(409);
        }

        var patientId = _patientIdGenerator.NextId();
        if (patientId <= 0)
            throw new InvalidOperationException("患者档案 Id 生成失败");
        var legacyProfile = relationship.Code == SaveLifePatientRelationships.Self
            ? await _legacyProfileRep.GetFirstAsync(u => u.ExternalUserId == ownerExternalUserId)
            : null;
        var patient = new SaveLifePatientProfile
        {
            Id = patientId,
            OwnerExternalUserId = ownerExternalUserId,
            AccountPatientKey = relationship.Code == SaveLifePatientRelationships.Self
                ? SaveLifePatientProfileKeys.ForSelf(ownerExternalUserId)
                : SaveLifePatientProfileKeys.ForOther(patientId),
            SourceUserProfileId = legacyProfile?.Id,
            RelationshipToAccount = relationship.Code,
            RelationshipDescription = relationship.Description,
            IsDefault = relationship.Code == SaveLifePatientRelationships.Self
        };
        ApplyCreateInput(patient, input);

        try
        {
            await _patientRep.InsertAsync(patient);
        }
        catch
        {
            if (relationship.Code == SaveLifePatientRelationships.Self &&
                await _patientRep.AsQueryable().AnyAsync(u =>
                    u.OwnerExternalUserId == ownerExternalUserId &&
                    u.RelationshipToAccount == SaveLifePatientRelationships.Self))
            {
                throw Oops.Oh("本人患者档案已存在").StatusCode(409);
            }

            throw;
        }
        if (patient.RelationshipToAccount == SaveLifePatientRelationships.Self)
            await SyncPatientToLegacyProfile(patient);

        if (_httpContextAccessor.HttpContext != null)
            _httpContextAccessor.HttpContext.Response.StatusCode = StatusCodes.Status201Created;

        return SaveLifePatientProfileMapper.ToOutput(
            patient,
            await GetCurrentPatientProfileId(ownerExternalUserId));
    }

    [NonAction]
    public async Task<SaveLifePatientProfileOutput> CreateAndSelectPatient(
        string conversationId,
        CreateAndSelectSaveLifePatientInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        var ownerExternalUserId = GetCurrentMiniAppOpenId();
        await _privacyConsentGuard.EnsureConsentedAsync(ownerExternalUserId);
        await _privacyConsentGuard.EnsureSensitiveInformationConsentedAsync(ownerExternalUserId);
        var parsedConversationId = ParseId(conversationId, "会话 Id 格式不正确");
        var clientCommandId = SaveLifePatientOnboardingPolicy.NormalizeClientCommandId(
            input.ClientCommandId);
        var displayName = NormalizeRequired(input.DisplayName, 64, "患者称呼不能为空");
        var relationship = SaveLifePatientOwnershipPolicy.NormalizeRelationship(
            input.RelationshipToAccount,
            displayName);
        var pendingConsultationId = ParseOptionalId(
            input.PendingConsultationId,
            "待继续咨询 Id 格式不正确");
        var accountPatientKey = relationship.Code == SaveLifePatientRelationships.Self
            ? SaveLifePatientProfileKeys.ForSelf(ownerExternalUserId)
            : SaveLifePatientOnboardingPolicy.BuildAccountPatientKey(
                ownerExternalUserId,
                clientCommandId);
        var db = _patientRep.Context;
        var now = DateTime.Now;

        var transaction = await db.Ado.UseTranAsync(async () =>
        {
            var conversation = await db.Queryable<SaveLifeAgentConversation>()
                .Where(item =>
                    item.Id == parsedConversationId &&
                    item.ExternalUserId == ownerExternalUserId &&
                    item.Status == "active" &&
                    !item.IsDelete)
                .FirstAsync();
            if (conversation == null)
                throw Oops.Oh("会话不存在或无权创建患者").StatusCode(404);

            var patient = await db.Queryable<SaveLifePatientProfile>()
                .Where(item =>
                    item.OwnerExternalUserId == ownerExternalUserId &&
                    item.AccountPatientKey == accountPatientKey &&
                    !item.IsDelete)
                .FirstAsync();
            var created = patient == null;
            if (patient == null)
            {
                var legacyProfile = relationship.Code == SaveLifePatientRelationships.Self
                    ? await db.Queryable<SaveLifeUserProfile>()
                        .Where(item =>
                            item.ExternalUserId == ownerExternalUserId &&
                            !item.IsDelete)
                        .FirstAsync()
                    : null;
                patient = CreateOnboardingPatient(
                    conversation,
                    legacyProfile,
                    ownerExternalUserId,
                    accountPatientKey,
                    relationship,
                    displayName,
                    now);
                var inserted = await db.Insertable(patient).ExecuteCommandAsync();
                if (inserted != 1)
                    throw Oops.Oh("患者档案创建失败，请重试").StatusCode(409);
            }
            else if (relationship.Code != SaveLifePatientRelationships.Self)
            {
                SaveLifePatientOnboardingPolicy.EnsureIdempotentPayload(
                    patient,
                    relationship,
                    displayName);
            }

            await SaveLifePatientSwitchPolicy.ConfirmInTransactionAsync(
                db,
                conversation.Id,
                ownerExternalUserId,
                patient.Id,
                now,
                pendingConsultationId);
            return new PatientOnboardingTransactionResult(patient, created);
        }, _ => { });

        if (!transaction.IsSuccess)
            throw transaction.ErrorException ?? new InvalidOperationException("患者创建和选择事务失败");

        if (_httpContextAccessor.HttpContext != null)
        {
            _httpContextAccessor.HttpContext.Response.StatusCode = transaction.Data.Created
                ? StatusCodes.Status201Created
                : StatusCodes.Status200OK;
        }

        return SaveLifePatientProfileMapper.ToOutput(
            transaction.Data.Patient,
            transaction.Data.Patient.Id);
    }

    [HttpGet("/api/v1/save-life/patients/{patientProfileId}"), NonUnify]
    [ApiDescriptionSettings(Name = "GetPatient")]
    public async Task<SaveLifePatientProfileOutput> GetPatient(long patientProfileId)
    {
        var ownerExternalUserId = GetCurrentMiniAppOpenId();
        await _privacyConsentGuard.EnsureConsentedAsync(ownerExternalUserId);
        var patient = await GetOwnedPatient(patientProfileId, ownerExternalUserId);

        return SaveLifePatientProfileMapper.ToOutput(
            patient,
            await GetCurrentPatientProfileId(ownerExternalUserId));
    }

    [HttpPatch("/api/v1/save-life/patients/{patientProfileId}"), NonUnify]
    [ApiDescriptionSettings(Name = "UpdatePatient")]
    public async Task<SaveLifePatientProfileOutput> UpdatePatient(
        long patientProfileId,
        UpdateSaveLifePatientProfileInput input)
    {
        var ownerExternalUserId = GetCurrentMiniAppOpenId();
        await _privacyConsentGuard.EnsureConsentedAsync(ownerExternalUserId);
        await _privacyConsentGuard.EnsureSensitiveInformationConsentedAsync(ownerExternalUserId);
        var patient = await GetOwnedPatient(patientProfileId, ownerExternalUserId);
        ApplyUpdateInput(patient, input);
        patient.UpdateTime = DateTime.Now;
        await _patientRep.AsUpdateable(patient).ExecuteCommandAsync();

        if (patient.RelationshipToAccount == SaveLifePatientRelationships.Self)
            await SyncPatientToLegacyProfile(patient);

        return SaveLifePatientProfileMapper.ToOutput(
            patient,
            await GetCurrentPatientProfileId(ownerExternalUserId));
    }

    [HttpPut("/api/v1/agent/conversations/{conversationId}/current-patient"), NonUnify]
    [ApiDescriptionSettings(Name = "SwitchCurrentPatient")]
    public async Task<SaveLifePatientProfileOutput> SwitchCurrentPatient(
        string conversationId,
        SwitchSaveLifeCurrentPatientInput input)
    {
        var ownerExternalUserId = GetCurrentMiniAppOpenId();
        await _privacyConsentGuard.EnsureConsentedAsync(ownerExternalUserId);
        var parsedConversationId = ParseId(conversationId, "会话 Id 格式不正确");
        var patientProfileId = ParseId(input.PatientProfileId, "患者档案 Id 格式不正确");
        var pendingConsultationId = ParseOptionalId(
            input.PendingConsultationId,
            "待继续咨询 Id 格式不正确");
        var patient = await GetOwnedPatient(patientProfileId, ownerExternalUserId);
        var conversation = await _conversationRep.GetFirstAsync(u =>
            u.Id == parsedConversationId &&
            u.ExternalUserId == ownerExternalUserId &&
            u.Status == "active");
        if (conversation == null)
            throw Oops.Oh("会话不存在或无权切换患者").StatusCode(404);

        await SaveLifePatientSwitchPolicy.ConfirmAsync(
            _patientRep.Context,
            conversation,
            patient,
            DateTime.Now,
            pendingConsultationId);

        return SaveLifePatientProfileMapper.ToOutput(patient, patient.Id);
    }

    [NonAction]
    public async Task<SaveLifePatientOnboardingDeferralOutput> DeferPatientOnboarding(
        string conversationId,
        DeferSaveLifePatientOnboardingInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        var ownerExternalUserId = GetCurrentMiniAppOpenId();
        await _privacyConsentGuard.EnsureConsentedAsync(ownerExternalUserId);
        var parsedConversationId = ParseId(conversationId, "会话 Id 格式不正确");
        var pendingConsultationId = ParseId(
            input.PendingConsultationId,
            "待继续咨询 Id 格式不正确");
        var db = _conversationRep.Context;
        var conversation = await db.Queryable<SaveLifeAgentConversation>()
            .Where(item =>
                item.Id == parsedConversationId &&
                item.ExternalUserId == ownerExternalUserId &&
                item.Status == "active" &&
                !item.IsDelete)
            .FirstAsync();
        if (conversation == null)
            throw Oops.Oh("会话不存在或无权暂缓患者引导").StatusCode(404);
        if (conversation.CurrentPatientProfileId is > 0)
            throw Oops.Oh("当前会话已有患者，无需暂缓患者引导").StatusCode(409);

        var state = SaveLifeAgentConversationStateCodec.Deserialize(conversation.FlowStateJson);
        if (state.PendingConsultation == null ||
            state.PendingConsultation.Id != pendingConsultationId ||
            state.PendingConsultation.Status != SaveLifeAgentPendingConsultationStatuses.Pending)
        {
            throw Oops.Oh("待继续咨询已变化，请刷新后重试").StatusCode(409);
        }

        if (!state.IsGeneralGuidanceDeferred)
        {
            var originalFlowStateJson = conversation.FlowStateJson;
            state.IsGeneralGuidanceDeferred = true;
            var nextFlowStateJson = SaveLifeAgentConversationStateCodec.Serialize(state);
            var now = DateTime.Now;
            try
            {
                await SaveLifeAgentConversationRevisionGuard.AdvanceAsync(
                    db,
                    conversation,
                    item => new SaveLifeAgentConversation
                    {
                        FlowStateVersion = SaveLifeAgentConversationState.CurrentVersion,
                        FlowStateJson = nextFlowStateJson
                    },
                    now,
                    item => item.FlowStateJson == originalFlowStateJson);
            }
            catch (SaveLifeAgentUnavailableException error)
                when (error.Code == SaveLifeAgentFailureCodes.TurnSuperseded)
            {
                var latest = await db.Queryable<SaveLifeAgentConversation>()
                    .Where(item =>
                        item.Id == conversation.Id &&
                        item.ExternalUserId == ownerExternalUserId &&
                        item.Status == "active" &&
                        !item.IsDelete)
                    .FirstAsync();
                var latestState = latest == null
                    ? null
                    : SaveLifeAgentConversationStateCodec.Deserialize(latest.FlowStateJson);
                if (latestState?.IsGeneralGuidanceDeferred != true ||
                    latestState.PendingConsultation?.Id != pendingConsultationId ||
                    latestState.PendingConsultation.Status != SaveLifeAgentPendingConsultationStatuses.Pending)
                {
                    throw Oops.Oh("患者引导状态已变化，请刷新后重试").StatusCode(409);
                }
            }
        }

        return new SaveLifePatientOnboardingDeferralOutput
        {
            PendingConsultationId = pendingConsultationId.ToString(),
            IsDeferred = true
        };
    }

    [HttpPatch("/api/v1/save-life/profile-proposals/{proposalId}"), NonUnify]
    [ApiDescriptionSettings(Name = "ResolveProfileProposal")]
    [SuppressMonitor]
    public Task<SaveLifePatientProfileProposalConfirmationOutput> ResolveProfileProposal(
        long proposalId,
        DecideSaveLifePatientProfileProposalInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        return ResolveProfileProposalCore(
            proposalId,
            NormalizeProfileProposalDecision(input.Decision),
            expectedConversationId: null,
            expectedPatientProfileId: null,
            validateSourceOwnership: true);
    }

    [NonAction]
    public Task<SaveLifePatientProfileProposalConfirmationOutput> ConfirmProfileProposal(
        long proposalId,
        ConfirmSaveLifePatientProfileProposalInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        return ResolveProfileProposalCore(
            proposalId,
            decision: "confirm",
            ParseId(input.ConversationId, "会话 Id 格式不正确"),
            ParseId(input.PatientProfileId, "患者档案 Id 格式不正确"),
            validateSourceOwnership: false);
    }

    private async Task<SaveLifePatientProfileProposalConfirmationOutput> ResolveProfileProposalCore(
        long proposalId,
        string decision,
        long? expectedConversationId,
        long? expectedPatientProfileId,
        bool validateSourceOwnership)
    {
        if (proposalId <= 0)
            throw Oops.Oh("档案提案 Id 格式不正确").StatusCode(400);

        var ownerExternalUserId = GetCurrentMiniAppOpenId();
        await _privacyConsentGuard.EnsureConsentedAsync(ownerExternalUserId);
        if (decision == "confirm")
            await _privacyConsentGuard.EnsureSensitiveInformationConsentedAsync(ownerExternalUserId);
        var db = _patientRep.Context;
        var now = DateTime.Now;

        var transaction = await db.Ado.UseTranAsync(async () =>
        {
            var proposal = await db.Queryable<SaveLifePatientProfileProposal>()
                .Where(item =>
                    item.Id == proposalId &&
                    item.OwnerExternalUserId == ownerExternalUserId &&
                    !item.IsDelete)
                .FirstAsync();
            if (proposal == null)
                throw Oops.Oh("档案提案不存在或无权处理").StatusCode(404);
            if ((expectedConversationId.HasValue &&
                 proposal.ConversationId != expectedConversationId.Value) ||
                (expectedPatientProfileId.HasValue &&
                 proposal.PatientProfileId != expectedPatientProfileId.Value))
            {
                throw Oops.Oh("档案提案上下文不一致").StatusCode(404);
            }

            var conversation = await db.Queryable<SaveLifeAgentConversation>()
                .Where(item =>
                    item.Id == proposal.ConversationId &&
                    item.ExternalUserId == ownerExternalUserId &&
                    item.Status == "active" &&
                    !item.IsDelete)
                .FirstAsync();
            if (conversation == null)
                throw Oops.Oh("会话不存在或无权处理档案提案").StatusCode(404);
            if (conversation.CurrentPatientProfileId != proposal.PatientProfileId)
                throw Oops.Oh("当前患者已变更，请重新确认档案信息").StatusCode(409);

            var patient = await db.Queryable<SaveLifePatientProfile>()
                .Where(item =>
                    item.Id == proposal.PatientProfileId &&
                    item.OwnerExternalUserId == ownerExternalUserId &&
                    !item.IsDelete)
                .FirstAsync();
            SaveLifePatientOwnershipPolicy.EnsureOwnedBy(patient, ownerExternalUserId);

            var resolvedStatus = decision == "confirm"
                ? SaveLifePatientProfileProposalStatuses.Confirmed
                : SaveLifePatientProfileProposalStatuses.Rejected;
            if (proposal.Status == resolvedStatus)
            {
                var currentState = SaveLifeAgentConversationStateCodec.Deserialize(
                    conversation.FlowStateJson);
                return ProfileProposalConfirmationTransactionResult.Success(
                    proposal,
                    patient!,
                    currentState.PendingQuestion);
            }
            if (proposal.Status != SaveLifePatientProfileProposalStatuses.Pending)
                throw Oops.Oh("档案提案已按其他决定处理").StatusCode(409);

            if (proposal.ExpiresTime <= now)
            {
                await ResolveProposal(
                    db,
                    proposal,
                    SaveLifePatientProfileProposalStatuses.Expired,
                    now);
                return ProfileProposalConfirmationTransactionResult.Conflict(
                    "档案提案已过期，请根据最新消息重新确认");
            }

            if (validateSourceOwnership)
            {
                await EnsureProfileProposalSourceOwnership(
                    db,
                    proposal,
                    ownerExternalUserId);
            }

            if (decision == "reject")
            {
                await ResolveProposal(
                    db,
                    proposal,
                    SaveLifePatientProfileProposalStatuses.Rejected,
                    now);
                var currentState = SaveLifeAgentConversationStateCodec.Deserialize(
                    conversation.FlowStateJson);
                return ProfileProposalConfirmationTransactionResult.Success(
                    proposal,
                    patient!,
                    currentState.PendingQuestion);
            }

            var currentValue = SaveLifePatientProfileProposalPolicy.ReadValue(
                patient!,
                proposal.FieldName);
            if (!SaveLifePatientProfileProposalPolicy.ValuesEqual(currentValue, proposal.OldValue))
            {
                await ResolveProposal(
                    db,
                    proposal,
                    SaveLifePatientProfileProposalStatuses.Superseded,
                    now);
                return ProfileProposalConfirmationTransactionResult.Conflict(
                    "患者档案已更新，请根据最新内容重新确认");
            }

            SaveLifePatientProfileProposalPolicy.ApplyValue(
                patient!,
                proposal.FieldName,
                proposal.ProposedValue);
            patient!.UpdateTime = now;
            var updatedPatient = await db.Updateable(patient)
                .UpdateColumns(
                [
                    SaveLifePatientProfileProposalPolicy.GetPatientPropertyName(proposal.FieldName),
                    nameof(SaveLifePatientProfile.UpdateTime)
                ])
                .ExecuteCommandAsync();
            if (updatedPatient != 1)
                throw Oops.Oh("患者档案更新失败，请重试").StatusCode(409);

            if (patient.RelationshipToAccount == SaveLifePatientRelationships.Self)
                await SyncConfirmedFieldToLegacyProfile(db, patient, proposal.FieldName, now);

            var resolved = await db.Updateable<SaveLifePatientProfileProposal>()
                .SetColumns(item => new SaveLifePatientProfileProposal
                {
                    Status = SaveLifePatientProfileProposalStatuses.Confirmed,
                    ResolvedTime = now,
                    UpdateTime = now
                })
                .Where(item =>
                    item.Id == proposal.Id &&
                    item.Status == SaveLifePatientProfileProposalStatuses.Pending &&
                    item.ExpiresTime > now &&
                    !item.IsDelete)
                .ExecuteCommandAsync();
            if (resolved != 1)
                throw Oops.Oh("档案提案状态已变化，请刷新后重试").StatusCode(409);

            var flowState = SaveLifeAgentConversationStateCodec.Deserialize(
                conversation.FlowStateJson);
            flowState.PendingQuestion = null;
            var flowStateJson = SaveLifeAgentConversationStateCodec.Serialize(flowState);
            conversation.FlowStateVersion = SaveLifeAgentConversationState.CurrentVersion;
            conversation.FlowStateJson = flowStateJson;
            conversation.StateRevision = await SaveLifeAgentConversationRevisionGuard.AdvanceAsync(
                db,
                conversation,
                item => new SaveLifeAgentConversation
                {
                    FlowStateVersion = SaveLifeAgentConversationState.CurrentVersion,
                    FlowStateJson = flowStateJson
                },
                now);

            proposal.Status = SaveLifePatientProfileProposalStatuses.Confirmed;
            proposal.ResolvedTime = now;
            proposal.UpdateTime = now;
            return ProfileProposalConfirmationTransactionResult.Success(
                proposal,
                patient,
                flowState.PendingQuestion);
        }, _ => { });

        if (!transaction.IsSuccess)
            throw transaction.ErrorException ?? new InvalidOperationException("档案提案确认事务失败");
        if (!string.IsNullOrWhiteSpace(transaction.Data.ConflictMessage))
            throw Oops.Oh(transaction.Data.ConflictMessage).StatusCode(409);

        return transaction.Data.Output!;
    }

    private static async Task EnsureProfileProposalSourceOwnership(
        ISqlSugarClient db,
        SaveLifePatientProfileProposal proposal,
        string ownerExternalUserId)
    {
        if (string.IsNullOrWhiteSpace(proposal.Evidence))
            throw Oops.Oh("档案提案来源证据无效").StatusCode(409);

        var sourceMessageIds = new[]
        {
            proposal.SourceUserMessageId,
            proposal.SourceAssistantMessageId
        };
        var sourceMessages = await db.Queryable<SaveLifeAgentMessage>()
            .Where(item =>
                sourceMessageIds.Contains(item.Id) &&
                item.ConversationId == proposal.ConversationId &&
                item.ExternalUserId == ownerExternalUserId &&
                item.PatientProfileId == proposal.PatientProfileId &&
                !item.IsDelete)
            .ToListAsync();
        var userMessage = sourceMessages.FirstOrDefault(item =>
            item.Id == proposal.SourceUserMessageId &&
            item.Role == "user");
        var assistantMessage = sourceMessages.FirstOrDefault(item =>
            item.Id == proposal.SourceAssistantMessageId &&
            item.Role == "assistant" &&
            item.ReplyToMessageId == proposal.SourceUserMessageId &&
            item.GenerationStatus == "completed");
        if (userMessage == null || assistantMessage == null)
            throw Oops.Oh("档案提案来源消息已变化或无权处理").StatusCode(409);

        if (proposal.SourceType == SaveLifePatientProfileProposalSources.UserMessage)
        {
            if (proposal.SourceMaterialId.HasValue ||
                !userMessage.Content.Contains(proposal.Evidence, StringComparison.Ordinal))
            {
                throw Oops.Oh("档案提案来源消息已变化或无权处理").StatusCode(409);
            }
            return;
        }

        if (proposal.SourceType != SaveLifePatientProfileProposalSources.Material ||
            proposal.SourceMaterialId is not > 0)
        {
            throw Oops.Oh("档案提案来源类型无效").StatusCode(409);
        }

        var sourceMaterialId = proposal.SourceMaterialId.Value;
        var material = await db.Queryable<SaveLifeEnrollmentMaterial>()
            .Where(item =>
                item.Id == sourceMaterialId &&
                item.ExternalUserId == ownerExternalUserId &&
                item.PatientProfileId == proposal.PatientProfileId &&
                !item.IsDelete)
            .FirstAsync();
        if (material == null)
            throw Oops.Oh("档案提案来源材料已变化或无权处理").StatusCode(409);

        if (!string.IsNullOrWhiteSpace(material.ExtractedText))
        {
            if (!material.ExtractedText.Contains(proposal.Evidence, StringComparison.Ordinal))
                throw Oops.Oh("档案提案来源材料证据已变化").StatusCode(409);
            return;
        }

        var suffix = SaveLifeEnrollmentMaterialHelper.NormalizeExtension(
            material.FileSuffix ?? material.FileName);
        var contentType = SaveLifeEnrollmentMaterialHelper.ResolveContentType(
            suffix,
            material.ContentType);
        if (suffix is not (".jpg" or ".png") ||
            !contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            throw Oops.Oh("档案提案来源材料证据已变化").StatusCode(409);
        }
    }

    private async Task<SaveLifePatientProfile> GetOwnedPatient(
        long patientProfileId,
        string ownerExternalUserId)
    {
        if (patientProfileId <= 0)
            throw Oops.Oh("患者档案 Id 格式不正确").StatusCode(400);

        var patient = await _patientRep.GetFirstAsync(u =>
            u.Id == patientProfileId &&
            u.OwnerExternalUserId == ownerExternalUserId);
        SaveLifePatientOwnershipPolicy.EnsureOwnedBy(patient, ownerExternalUserId);
        return patient!;
    }

    private static async Task ResolveProposal(
        ISqlSugarClient db,
        SaveLifePatientProfileProposal proposal,
        string status,
        DateTime resolvedTime)
    {
        var affected = await db.Updateable<SaveLifePatientProfileProposal>()
            .SetColumns(item => new SaveLifePatientProfileProposal
            {
                Status = status,
                ResolvedTime = resolvedTime,
                UpdateTime = resolvedTime
            })
            .Where(item =>
                item.Id == proposal.Id &&
                item.Status == SaveLifePatientProfileProposalStatuses.Pending &&
                !item.IsDelete)
            .ExecuteCommandAsync();
        if (affected != 1)
            throw Oops.Oh("档案提案状态已变化，请刷新后重试").StatusCode(409);

        proposal.Status = status;
        proposal.ResolvedTime = resolvedTime;
        proposal.UpdateTime = resolvedTime;
    }

    private static async Task SyncConfirmedFieldToLegacyProfile(
        ISqlSugarClient db,
        SaveLifePatientProfile patient,
        string field,
        DateTime updateTime)
    {
        if (field is not (SaveLifePatientProfileProposalFields.MedicalHistory or
            SaveLifePatientProfileProposalFields.Allergies))
        {
            return;
        }

        var legacyProfile = await db.Queryable<SaveLifeUserProfile>()
            .Where(item => item.ExternalUserId == patient.OwnerExternalUserId && !item.IsDelete)
            .FirstAsync();
        if (legacyProfile == null)
            return;

        legacyProfile.UpdateTime = updateTime;
        if (field == SaveLifePatientProfileProposalFields.MedicalHistory)
        {
            legacyProfile.MedicalHistory = patient.MedicalHistory;
            var updatedLegacy = await db.Updateable(legacyProfile)
                .UpdateColumns(item => new { item.MedicalHistory, item.UpdateTime })
                .ExecuteCommandAsync();
            if (updatedLegacy != 1)
                throw new InvalidOperationException("本人兼容档案同步失败");
            return;
        }

        legacyProfile.Allergies = patient.Allergies;
        var updatedAllergies = await db.Updateable(legacyProfile)
            .UpdateColumns(item => new { item.Allergies, item.UpdateTime })
            .ExecuteCommandAsync();
        if (updatedAllergies != 1)
            throw new InvalidOperationException("本人兼容档案同步失败");
    }

    private async Task<long?> GetCurrentPatientProfileId(string ownerExternalUserId)
    {
        var conversation = await _conversationRep.AsQueryable()
            .Where(u =>
                u.ExternalUserId == ownerExternalUserId &&
                u.Status == "active" &&
                u.CurrentPatientProfileId != null)
            .OrderBy(u => u.UpdateTime, OrderByType.Desc)
            .OrderBy(u => u.CreateTime, OrderByType.Desc)
            .FirstAsync();
        if (conversation?.CurrentPatientProfileId is > 0)
            return conversation.CurrentPatientProfileId;

        var preferredPatient = await _patientRep.AsQueryable()
            .Where(u =>
                u.OwnerExternalUserId == ownerExternalUserId &&
                u.IsDefault &&
                !u.IsDelete)
            .OrderBy(u => u.UpdateTime, OrderByType.Desc)
            .OrderBy(u => u.CreateTime, OrderByType.Desc)
            .FirstAsync();
        if (preferredPatient != null)
            return preferredPatient.Id;

        return (await _patientRep.AsQueryable()
            .Where(u =>
                u.OwnerExternalUserId == ownerExternalUserId &&
                u.RelationshipToAccount == SaveLifePatientRelationships.Self &&
                !u.IsDelete)
            .FirstAsync())?.Id;
    }

    private async Task<SaveLifePatientProfile?> GetOrCreateSelfPatient(string ownerExternalUserId)
    {
        async Task<SaveLifePatientProfile?> LoadExisting()
        {
            return (SaveLifePatientProfile?)await _patientRep.AsQueryable()
                .Where(u =>
                    u.OwnerExternalUserId == ownerExternalUserId &&
                    u.RelationshipToAccount == SaveLifePatientRelationships.Self)
                .FirstAsync();
        }

        var existing = await LoadExisting();
        if (existing != null)
            return existing;

        var legacyProfile = await _legacyProfileRep.GetFirstAsync(u =>
            u.ExternalUserId == ownerExternalUserId);
        if (legacyProfile == null)
            return null;

        return await SaveLifeAtomicUpsert.ExecuteAsync(
            LoadExisting,
            () => SaveLifePatientProfileFactory.CreateSelf(legacyProfile, ownerExternalUserId),
            _ => { },
            async patient => await _patientRep.InsertAsync(patient),
            _ => Task.CompletedTask);
    }

    private async Task SyncPatientToLegacyProfile(SaveLifePatientProfile patient)
    {
        var legacyProfile = await _legacyProfileRep.GetFirstAsync(u =>
            u.ExternalUserId == patient.OwnerExternalUserId);
        if (legacyProfile == null)
            return;

        SaveLifePatientProfileCompatibility.ApplyPatientToLegacy(patient, legacyProfile);
        await _legacyProfileRep.AsUpdateable(legacyProfile)
            .UpdateColumns(u => new
            {
                u.RealName,
                u.Gender,
                u.BirthDate,
                u.Address,
                u.Ethnicity,
                u.MedicalHistory,
                u.Allergies,
                u.UpdateTime
            })
            .ExecuteCommandAsync();
    }

    private string GetCurrentMiniAppOpenId()
    {
        var openId = _httpContextAccessor.HttpContext?.User
            .FindFirst(ClaimConst.OpenId)?.Value?.Trim();
        if (!string.IsNullOrWhiteSpace(openId))
            return openId;

        throw Oops.Oh("小程序登录态无效，请重新登录").StatusCode(400);
    }

    private static void ApplyCreateInput(
        SaveLifePatientProfile patient,
        CreateSaveLifePatientProfileInput input)
    {
        patient.DisplayName = NormalizeRequired(input.DisplayName, 64, "患者称呼不能为空");
        patient.Gender = SaveLifeMiniAppProfileHelper.NormalizeGender(input.Gender);
        patient.BirthDate = SaveLifeMiniAppProfileHelper.NormalizeBirthDate(input.BirthDate);
        patient.Ethnicity = NormalizeOptional(input.Ethnicity, 32, "民族");
        patient.City = NormalizeOptional(input.City, 128, "所在城市");
        patient.Residence = NormalizeOptional(input.Residence, 256, "居住信息");
        patient.Diagnosis = NormalizeOptional(input.Diagnosis, 256, "疾病或诊断");
        patient.DiseaseStage = NormalizeOptional(input.DiseaseStage, 128, "疾病阶段");
        patient.DiseaseStatus = NormalizeOptional(input.DiseaseStatus, 256, "当前病情状态");
        patient.TreatmentHistory = NormalizeOptional(input.TreatmentHistory, 8000, "既往治疗");
        patient.CurrentTreatment = NormalizeOptional(input.CurrentTreatment, 8000, "当前治疗");
        patient.Pathology = NormalizeOptional(input.Pathology, 8000, "病理信息");
        patient.Biomarkers = NormalizeOptional(input.Biomarkers, 8000, "分子标志物");
        patient.Comorbidities = NormalizeOptional(input.Comorbidities, 8000, "合并疾病");
        patient.MedicalHistory = NormalizeOptional(input.MedicalHistory, 8000, "既往病史");
        patient.Allergies = NormalizeOptional(input.Allergies, 8000, "过敏史");
    }

    private SaveLifePatientProfile CreateOnboardingPatient(
        SaveLifeAgentConversation conversation,
        SaveLifeUserProfile? legacyProfile,
        string ownerExternalUserId,
        string accountPatientKey,
        SaveLifePatientRelationship relationship,
        string displayName,
        DateTime createTime)
    {
        if (relationship.Code == SaveLifePatientRelationships.Self && legacyProfile != null)
            return SaveLifePatientProfileFactory.CreateSelf(legacyProfile, ownerExternalUserId);

        var patientId = _patientIdGenerator.NextId();
        if (patientId <= 0)
            throw new InvalidOperationException("患者档案 Id 生成失败");

        return new SaveLifePatientProfile
        {
            Id = patientId,
            TenantId = conversation.TenantId,
            OwnerExternalUserId = ownerExternalUserId,
            AccountPatientKey = accountPatientKey,
            RelationshipToAccount = relationship.Code,
            RelationshipDescription = relationship.Description,
            DisplayName = relationship.Code == SaveLifePatientRelationships.Self
                ? "本人"
                : displayName,
            IsDefault = relationship.Code == SaveLifePatientRelationships.Self,
            CreateTime = createTime
        };
    }

    private static void ApplyUpdateInput(
        SaveLifePatientProfile patient,
        UpdateSaveLifePatientProfileInput input)
    {
        if (input.RelationshipToAccount != null || input.RelationshipDescription != null)
        {
            var relationship = SaveLifePatientOwnershipPolicy.NormalizeRelationship(
                input.RelationshipToAccount ?? patient.RelationshipToAccount,
                input.RelationshipDescription ?? patient.RelationshipDescription);
            SaveLifePatientOwnershipPolicy.EnsureRelationshipChangeAllowed(
                patient.RelationshipToAccount,
                relationship.Code);
            patient.RelationshipToAccount = relationship.Code;
            patient.RelationshipDescription = relationship.Description;
        }

        if (input.DisplayName != null)
            patient.DisplayName = NormalizeRequired(input.DisplayName, 64, "患者称呼不能为空");
        if (input.Gender != null)
            patient.Gender = SaveLifeMiniAppProfileHelper.NormalizeGender(input.Gender);
        if (input.BirthDate.HasValue)
            patient.BirthDate = SaveLifeMiniAppProfileHelper.NormalizeBirthDate(input.BirthDate);
        ApplyOptionalText(input.Ethnicity, value => patient.Ethnicity = value, 32, "民族");
        ApplyOptionalText(input.City, value => patient.City = value, 128, "所在城市");
        ApplyOptionalText(input.Residence, value => patient.Residence = value, 256, "居住信息");
        ApplyOptionalText(input.Diagnosis, value => patient.Diagnosis = value, 256, "疾病或诊断");
        ApplyOptionalText(input.DiseaseStage, value => patient.DiseaseStage = value, 128, "疾病阶段");
        ApplyOptionalText(input.DiseaseStatus, value => patient.DiseaseStatus = value, 256, "当前病情状态");
        ApplyOptionalText(input.TreatmentHistory, value => patient.TreatmentHistory = value, 8000, "既往治疗");
        ApplyOptionalText(input.CurrentTreatment, value => patient.CurrentTreatment = value, 8000, "当前治疗");
        ApplyOptionalText(input.Pathology, value => patient.Pathology = value, 8000, "病理信息");
        ApplyOptionalText(input.Biomarkers, value => patient.Biomarkers = value, 8000, "分子标志物");
        ApplyOptionalText(input.Comorbidities, value => patient.Comorbidities = value, 8000, "合并疾病");
        ApplyOptionalText(input.MedicalHistory, value => patient.MedicalHistory = value, 8000, "既往病史");
        ApplyOptionalText(input.Allergies, value => patient.Allergies = value, 8000, "过敏史");
    }

    private static void ApplyOptionalText(
        string? input,
        Action<string?> setter,
        int maxLength,
        string fieldName)
    {
        if (input != null)
            setter(NormalizeOptional(input, maxLength, fieldName));
    }

    private static string NormalizeRequired(
        string? value,
        int maxLength,
        string errorMessage)
    {
        return SaveLifeMiniAppProfileHelper.NormalizeText(value, maxLength, "患者称呼")
            ?? throw Oops.Oh(errorMessage).StatusCode(400);
    }

    private static string NormalizeProfileProposalDecision(string? decision)
    {
        var normalized = decision?.Trim().ToLowerInvariant();
        return normalized is "confirm" or "reject"
            ? normalized
            : throw Oops.Oh("档案提案决定只能是 confirm 或 reject").StatusCode(400);
    }

    private static string? NormalizeOptional(
        string? value,
        int maxLength,
        string fieldName)
    {
        return SaveLifeMiniAppProfileHelper.NormalizeText(value, maxLength, fieldName);
    }

    private static long ParseId(string? value, string errorMessage)
    {
        if (!long.TryParse(value?.Trim(), out var id) || id <= 0)
            throw Oops.Oh(errorMessage).StatusCode(400);

        return id;
    }

    private static long? ParseOptionalId(string? value, string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return ParseId(value, errorMessage);
    }

    private sealed record PatientOnboardingTransactionResult(
        SaveLifePatientProfile Patient,
        bool Created);

    private sealed class ProfileProposalConfirmationTransactionResult
    {
        public SaveLifePatientProfileProposalConfirmationOutput? Output { get; private init; }

        public string? ConflictMessage { get; private init; }

        public static ProfileProposalConfirmationTransactionResult Success(
            SaveLifePatientProfileProposal proposal,
            SaveLifePatientProfile patient,
            SaveLifeAgentPendingQuestionState? pendingQuestion)
        {
            var patientOutput = SaveLifePatientProfileMapper.ToOutput(patient, patient.Id);
            List<SaveLifeAgentSuggestedActionOutput> suggestedActions = [];
            if (proposal.Status == SaveLifePatientProfileProposalStatuses.Confirmed &&
                SaveLifeAgentTrialSelection.EvaluateMatchingGate(patientOutput).CanMatch)
            {
                suggestedActions.Add(new SaveLifeAgentSuggestedActionOutput
                {
                    Type = SaveLifeAgentSuggestedActionTypes.StartScreening,
                    Label = "查看相关项目",
                    TargetType = "trial"
                });
            }

            return new ProfileProposalConfirmationTransactionResult
            {
                Output = new SaveLifePatientProfileProposalConfirmationOutput
                {
                    Proposal = SaveLifePatientProfileProposalPolicy.ToOutput(proposal),
                    Patient = patientOutput,
                    PendingQuestion = pendingQuestion == null
                        ? null
                        : new SaveLifeAgentPendingQuestionOutput
                        {
                            Key = pendingQuestion.Key,
                            Prompt = pendingQuestion.Prompt
                        },
                    SuggestedActions = suggestedActions
                }
            };
        }

        public static ProfileProposalConfirmationTransactionResult Conflict(string message)
        {
            return new ProfileProposalConfirmationTransactionResult
            {
                ConflictMessage = message
            };
        }
    }
}
