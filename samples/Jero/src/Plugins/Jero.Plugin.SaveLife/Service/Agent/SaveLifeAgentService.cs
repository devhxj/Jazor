using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Jero.Plugin.SaveLife.Service;

/// <summary>
/// 小程序智能助手的会话、历史和业务确认接口。
/// </summary>
[Authorize]
[ApiDescriptionSettings(SaveLifeConst.GroupName, Order = 90)]
public class SaveLifeAgentService : IDynamicApiController, ITransient
{
    private static readonly EventId HistoryClearedEvent =
        new(7013, "save_life.agent.history.cleared");

    private const int MaxLoadedConversationMessages =
        SaveLifeAgentOptions.MaximumHistoryRounds * 2 + 1;

    private static readonly JsonSerializerOptions MessageMetadataJsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly SqlSugarRepository<SaveLifeAgentConversation> _conversationRep;
    private readonly SqlSugarRepository<SaveLifeAgentMessage> _messageRep;
    private readonly SqlSugarRepository<SaveLifeClinicalTrial> _trialRep;
    private readonly SqlSugarRepository<SaveLifeDiseaseKnowledge> _knowledgeRep;
    private readonly SqlSugarRepository<SaveLifeEnrollmentRecord> _enrollmentRep;
    private readonly SqlSugarRepository<SaveLifeUserProfile> _profileRep;
    private readonly SqlSugarRepository<SaveLifePatientProfile> _patientProfileRep;
    private readonly ISaveLifePatientEnrollmentService _patientEnrollmentService;
    private readonly ISaveLifePrivacyConsentGuard _privacyConsentGuard;
    private readonly ILogger<SaveLifeAgentService> _logger;

    public SaveLifeAgentService(
        SqlSugarRepository<SaveLifeAgentConversation> conversationRep,
        SqlSugarRepository<SaveLifeAgentMessage> messageRep,
        SqlSugarRepository<SaveLifeClinicalTrial> trialRep,
        SqlSugarRepository<SaveLifeDiseaseKnowledge> knowledgeRep,
        SqlSugarRepository<SaveLifeEnrollmentRecord> enrollmentRep,
        SqlSugarRepository<SaveLifeUserProfile> profileRep,
        SqlSugarRepository<SaveLifePatientProfile> patientProfileRep,
        ISaveLifePatientEnrollmentService patientEnrollmentService,
        ISaveLifePrivacyConsentGuard privacyConsentGuard,
        ILogger<SaveLifeAgentService> logger)
    {
        _conversationRep = conversationRep;
        _messageRep = messageRep;
        _trialRep = trialRep;
        _knowledgeRep = knowledgeRep;
        _enrollmentRep = enrollmentRep;
        _profileRep = profileRep;
        _patientProfileRep = patientProfileRep;
        _patientEnrollmentService = patientEnrollmentService;
        _privacyConsentGuard = privacyConsentGuard;
        _logger = logger;
    }

    /// <summary>
    /// 创建小程序助手会话。
    /// </summary>
    [HttpPost("/api/v1/agent/conversations"), NonUnify]
    [ApiDescriptionSettings(Name = "CreateConversation")]
    [SuppressMonitor]
    public async Task<SaveLifeAgentConversationOutput> CreateConversation(
        CreateSaveLifeAgentConversationInput input)
    {
        var externalUserId = GetCurrentMiniAppOpenId();
        await _privacyConsentGuard.EnsureConsentedAsync(externalUserId);

        if (!string.IsNullOrWhiteSpace(input.ConversationId))
        {
            var restoredConversation = await GetConversation(input.ConversationId);
            await EnsureConversationAccountProfile(restoredConversation);
            return ToConversationOutput(restoredConversation);
        }

        var requestedContext = await ResolveBusinessContext(
            input.TrialId,
            input.KnowledgeId,
            input.EnrollmentId,
            input.ContextType,
            input.ContextId,
            externalUserId);
        var cancellationToken = App.HttpContext?.RequestAborted ?? default;
        var latestConversation = await SaveLifeAgentActiveConversationPersistence.FindActiveAsync(
            _conversationRep.Context,
            externalUserId,
            cancellationToken);
        var conversation = SaveLifeAgentConversationPolicy.ShouldReuseLatest(
                latestConversation,
                requestedContext)
            ? latestConversation
            : null;

        if (conversation == null)
        {
            conversation = new SaveLifeAgentConversation
            {
                ExternalUserId = externalUserId,
                Status = "active",
                Phase = "created",
                LastMessage = "会话已创建"
            };
            conversation = await SaveLifeAgentActiveConversationPersistence.CreateOrGetActiveAsync(
                _conversationRep.Context,
                conversation,
                cancellationToken);
        }

        await EnsureConversationAccountProfile(conversation);
        return ToConversationOutput(conversation);
    }

    /// <summary>
    /// 获取用户历史会话。
    /// </summary>
    [HttpGet("/api/v1/agent/conversations"), NonUnify]
    [ApiDescriptionSettings(Name = "GetConversations")]
    [SuppressMonitor]
    public async Task<List<SaveLifeAgentConversationOutput>> GetConversations(
        [FromQuery] QuerySaveLifeAgentConversationInput? input = null)
    {
        var externalUserId = GetCurrentMiniAppOpenId();
        await _privacyConsentGuard.EnsureConsentedAsync(externalUserId);
        var page = NormalizePage(input?.Page ?? 1);
        var pageSize = NormalizeConversationPageSize(input?.PageSize ?? 20);

        var conversations = await _conversationRep.AsQueryable()
            .Where(item => item.ExternalUserId == externalUserId)
            .OrderBy(item => item.UpdateTime, OrderByType.Desc)
            .OrderBy(item => item.CreateTime, OrderByType.Desc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return conversations.Select(ToConversationOutput).ToList();
    }

    /// <summary>
    /// 清空当前小程序用户的助手咨询记录。
    /// </summary>
    [HttpDelete("/api/v1/agent/conversations"), NonUnify]
    [ApiDescriptionSettings(Name = "ClearConversations")]
    [SuppressMonitor]
    public async Task<SaveLifeAgentClearConversationsOutput> ClearConversations()
    {
        var externalUserId = GetCurrentMiniAppOpenId();
        var result = await SaveLifeAgentConversationClearPersistence.ClearAsync(
            _conversationRep.Context,
            externalUserId,
            DateTime.Now,
            App.HttpContext?.RequestAborted ?? default);
        _logger.LogInformation(
            HistoryClearedEvent,
            "SaveLife assistant history cleared. ConversationCount={ConversationCount} MessageCount={MessageCount} ProfileProposalCount={ProfileProposalCount} BusinessCommandCount={BusinessCommandCount} AccountCorrelationId={AccountCorrelationId} TraceId={TraceId}",
            result.ConversationCount,
            result.MessageCount,
            result.ProfileProposalCount,
            result.BusinessCommandCount,
            SaveLifeAgentDiagnostics.BuildCorrelationId("account", externalUserId, externalUserId),
            ResolveTraceId());

        return new SaveLifeAgentClearConversationsOutput
        {
            ClearedConversationCount = result.ConversationCount,
            ClearedMessageCount = result.MessageCount
        };
    }

    /// <summary>
    /// 获取会话消息。
    /// </summary>
    [HttpGet("/api/v1/agent/conversations/{conversationId}/messages"), NonUnify]
    [ApiDescriptionSettings(Name = "GetConversationMessages")]
    [SuppressMonitor]
    public async Task<List<SaveLifeAgentMessageOutput>> GetConversationMessages(string conversationId)
    {
        var conversation = await GetConversation(conversationId);
        await _privacyConsentGuard.EnsureConsentedAsync(conversation.ExternalUserId);
        return await LoadMessageOutputs(conversation);
    }

    /// <summary>
    /// 分页获取连续会话消息和结构化会话状态。
    /// </summary>
    [HttpGet("/api/v1/agent/conversations/{conversationId}/messages/page"), NonUnify]
    [ApiDescriptionSettings(Name = "GetConversationMessagesPage")]
    [SuppressMonitor]
    public async Task<SaveLifeAgentMessagePageOutput> GetConversationMessagesPage(
        string conversationId,
        [FromQuery] QuerySaveLifeAgentMessagePageInput? input = null)
    {
        var conversation = await GetConversation(conversationId);
        await _privacyConsentGuard.EnsureConsentedAsync(conversation.ExternalUserId);
        var pageSize = NormalizeConversationMessagePageSize(input?.PageSize ?? 20);
        var beforeMessageId = ParseOptionalMessageCursor(input?.BeforeMessageId);

        var messageQuery = ApplyConversationScope(
            _messageRep.AsQueryable(),
            conversation.Id)
            .Where(item =>
                !item.IsDelete &&
                (item.Role != "assistant" ||
                  item.GenerationStatus == null ||
                  item.GenerationStatus == "completed"));
        var messages = await messageQuery
            .WhereIF(beforeMessageId.HasValue, item => item.Id < beforeMessageId!.Value)
            .OrderBy(item => item.Id, OrderByType.Desc)
            .Take(pageSize + 1)
            .ToListAsync();
        var hasMore = messages.Count > pageSize;
        var pageMessages = messages.Take(pageSize).ToList();
        var nextCursor = hasMore ? pageMessages[^1].Id.ToString() : null;
        pageMessages.Reverse();

        return new SaveLifeAgentMessagePageOutput
        {
            HasMore = hasMore,
            Items = await BuildMessageOutputsAsync(conversation, pageMessages),
            NextCursor = nextCursor
        };
    }

    /// <summary>
    /// 提交参与意向并联动报名记录。
    /// </summary>
    [NonAction]
    public async Task<SaveLifeAgentIntentionOutput> SubmitIntention(
        SubmitSaveLifeAgentIntentionInput input,
        CancellationToken cancellationToken = default)
    {
        var conversation = await GetConversation(input.ConversationId);
        await _privacyConsentGuard.EnsureConsentedAsync(conversation.ExternalUserId);

        var clientCommandId = NormalizeRequired(input.ClientCommandId, "客户端命令Id不能为空");
        var intention = SaveLifeAgentIntentionCommandPolicy.NormalizeChoice(input.Intention);
        if (input.StateRevision <= 0)
            throw Oops.Oh("会话状态修订号无效，请重新发起参与确认").StatusCode(400);
        if (input.PatientProfileId is not > 0)
            throw Oops.Oh("患者档案Id不能为空").StatusCode(400);
        input.ContactName = NormalizeOptional(input.ContactName, 64);
        input.ContactPhone = NormalizeOptional(input.ContactPhone, 32);

        var requestedStatus = SaveLifeAgentReplyBuilder.MapIntentionStatus(intention);
        var selectedTrialId = SaveLifeAgentTrialSelection.ResolveConfirmedTrialId(input.TrialId);
        var request = new SaveLifeAgentIntentionCommandRequest(
            conversation.TenantId,
            conversation.ExternalUserId,
            conversation.Id,
            clientCommandId,
            input.StateRevision,
            input.PatientProfileId.Value,
            selectedTrialId ?? 0,
            intention,
            input.ContactName,
            input.ContactPhone);
        var replay = await SaveLifeAgentIntentionCommandPersistence.TryReplayAsync<SaveLifeAgentIntentionOutput>(
            _conversationRep.Context,
            request,
            cancellationToken);
        if (replay.Exists)
            return replay.Result!;

        var confirmedPatientProfileId = SaveLifePatientEnrollmentPolicy.ResolveConversationPatientProfileId(
            conversation.CurrentPatientProfileId,
            input.PatientProfileId);
        var messages = await LoadMessageOutputs(conversation);
        var signal = SaveLifeAgentReplyBuilder.ExtractSignal(
            messages.Where(item => item.Role == "user").Select(item => item.Content));
        var currentPatient = (await LoadTurnConversationState(conversation)).CurrentPatient;
        signal.CurrentPatient = currentPatient;
        signal.DiseaseKeyword ??= currentPatient?.Diagnosis;
        signal.City ??= currentPatient?.City;
        var submissionFields = requestedStatus == "pending"
            ? BuildAgentSubmissionFields(input, signal, messages, intention, conversation.Id)
            : new SaveLifeEnrollmentSubmissionFields();

        return await SaveLifeAgentIntentionCommandPersistence.ExecuteAsync(
            _conversationRep.Context,
            request,
            async (db, persistedConversation) =>
            {
                var status = requestedStatus;
                long? enrollmentRecordId = null;
                SaveLifeEnrollmentValidationResult? validation = null;
                string content;

                if (requestedStatus == "pending" && selectedTrialId.HasValue)
                {
                    var result = await _patientEnrollmentService.SubmitConfirmedInTransactionAsync(
                        db,
                        new SaveLifePatientEnrollmentSubmissionInput
                        {
                            ExternalUserId = persistedConversation.ExternalUserId,
                            PatientProfileId = confirmedPatientProfileId,
                            ProjectId = selectedTrialId.Value,
                            Fields = submissionFields
                        },
                        cancellationToken);
                    var enrollment = result.Enrollment;
                    validation = result.Validation;
                    status = enrollment.Status;
                    enrollmentRecordId = enrollment.Id;
                    content = SaveLifeEnrollmentValidation.IsBlockingExistingSubmission(enrollment)
                        ? BuildExistingEnrollmentReply(enrollment)
                        : BuildIntentionReply(
                            intention,
                            status,
                            BuildSubmissionValidationReply(requestedStatus, validation));
                }
                else if (requestedStatus == "pending")
                {
                    status = "trial_required";
                    content = BuildMissingTrialIntentionReply(null);
                }
                else if (requestedStatus == "draft")
                {
                    status = "considering";
                    content = "好的，您可以先继续了解项目，决定参与后再明确确认；目前没有创建报名记录。";
                }
                else
                {
                    content = BuildIntentionReply(intention, status);
                }

                var validationReply = BuildSubmissionValidationReply(requestedStatus, validation);
                var suggestedActions = SaveLifeAgentSuggestedActionBuilder.BuildForIntention(
                    status,
                    enrollmentRecordId?.ToString(),
                    selectedTrialId?.ToString(),
                    !string.IsNullOrWhiteSpace(validationReply));
                var output = new SaveLifeAgentIntentionOutput
                {
                    ConversationId = persistedConversation.Id.ToString(),
                    Status = status,
                    EnrollmentRecordId = enrollmentRecordId?.ToString(),
                    Content = content,
                    SuggestedActions = suggestedActions
                };
                var metadataJson = JSON.Serialize(new
                {
                    intention,
                    status,
                    enrollmentRecordId,
                    ModelProvider = "service",
                    RunStatus = "service_completed",
                    suggestedActions
                });
                return new SaveLifeAgentIntentionCommandPreparation<SaveLifeAgentIntentionOutput>(
                    output,
                    $"我选择：{intention}",
                    content,
                    metadataJson,
                    status == "closed" ? "closed" : "active",
                    SaveLifePatientEnrollmentPolicy.ResolveConversationPhase(status),
                    enrollmentRecordId);
            },
            DateTime.Now,
            Yitter.IdGenerator.YitIdHelper.NextId,
            cancellationToken);
    }

    private async Task<SaveLifeAgentConversation> GetConversation(string conversationId)
    {
        var id = ParseId(conversationId, "会话Id格式不正确");
        var externalUserId = GetCurrentMiniAppOpenId();
        var conversation = await _conversationRep.GetFirstAsync(item =>
            item.Id == id && item.ExternalUserId == externalUserId);
        return conversation ?? throw Oops.Oh("会话不存在或不属于当前用户").StatusCode(404);
    }

    private async Task EnsureConversationAccountProfile(SaveLifeAgentConversation conversation)
    {
        var originalPatientId = conversation.CurrentPatientProfileId;
        var originalStateVersion = conversation.FlowStateVersion;
        var originalPatientContextVersion = conversation.PatientContextVersion;
        var selfProfile = await GetOrCreateSelfPatient(conversation.ExternalUserId);
        BindConversationToAccountProfile(conversation, selfProfile);
        if (conversation.CurrentPatientProfileId == originalPatientId &&
            conversation.FlowStateVersion == originalStateVersion &&
            conversation.PatientContextVersion == originalPatientContextVersion)
        {
            return;
        }

        await SaveLifeAgentConversationRevisionGuard.AdvanceAsync(
            _conversationRep.Context,
            conversation,
            item => new SaveLifeAgentConversation
            {
                CurrentPatientProfileId = conversation.CurrentPatientProfileId,
                FlowStateVersion = conversation.FlowStateVersion,
                PatientContextVersion = conversation.PatientContextVersion,
                AgentSessionJson = null
            },
            DateTime.Now);
    }

    internal static void BindConversationToAccountProfile(
        SaveLifeAgentConversation conversation,
        SaveLifePatientProfile? selfProfile)
    {
        ArgumentNullException.ThrowIfNull(conversation);
        var originalProfileId = conversation.CurrentPatientProfileId;
        var expectedProfileKey = SaveLifePatientProfileKeys.ForSelf(conversation.ExternalUserId);
        var hasOwnedSelfProfile = selfProfile is
        {
            Id: > 0,
            IsDelete: false
        } &&
            string.Equals(
                selfProfile.OwnerExternalUserId,
                conversation.ExternalUserId,
                StringComparison.Ordinal) &&
            string.Equals(
                selfProfile.AccountPatientKey,
                expectedProfileKey,
                StringComparison.Ordinal);

        conversation.CurrentPatientProfileId = hasOwnedSelfProfile ? selfProfile!.Id : null;
        conversation.FlowStateVersion ??= SaveLifeAgentConversationState.CurrentVersion;
        conversation.PatientContextVersion = Math.Max(1, conversation.PatientContextVersion);
        if (originalProfileId.HasValue && originalProfileId != conversation.CurrentPatientProfileId)
            conversation.PatientContextVersion = checked(conversation.PatientContextVersion + 1);
    }

    private async Task<SaveLifePatientProfile?> GetOrCreateSelfPatient(string externalUserId)
    {
        var ownerExternalUserId = externalUserId.Trim();
        var selfKey = SaveLifePatientProfileKeys.ForSelf(ownerExternalUserId);

        async Task<SaveLifePatientProfile?> LoadExisting()
        {
            return (SaveLifePatientProfile?)await _patientProfileRep.AsQueryable()
                .Where(item =>
                    item.OwnerExternalUserId == ownerExternalUserId &&
                    item.AccountPatientKey == selfKey &&
                    !item.IsDelete)
                .FirstAsync();
        }

        var existing = await LoadExisting();
        if (existing != null)
            return existing;

        var profile = await _profileRep.GetFirstAsync(item => item.ExternalUserId == ownerExternalUserId);
        if (profile == null)
            return null;

        return await SaveLifeAtomicUpsert.ExecuteAsync(
            LoadExisting,
            () => SaveLifePatientProfileFactory.CreateSelf(profile, ownerExternalUserId),
            _ => { },
            async patient => await _patientProfileRep.InsertAsync(patient),
            _ => Task.CompletedTask);
    }

    internal static ISugarQueryable<SaveLifeAgentMessage> ApplyConversationScope(
        ISugarQueryable<SaveLifeAgentMessage> query,
        long conversationId)
    {
        ArgumentNullException.ThrowIfNull(query);
        if (conversationId <= 0)
            throw new ArgumentOutOfRangeException(nameof(conversationId));
        return query.Where(item => item.ConversationId == conversationId);
    }

    private async Task<List<SaveLifeAgentMessageOutput>> LoadMessageOutputs(
        SaveLifeAgentConversation conversation)
    {
        var query = ApplyConversationScope(
            _messageRep.AsQueryable(),
            conversation.Id)
            .Where(item =>
                !item.IsDelete &&
                (item.Role != "assistant" ||
                  item.GenerationStatus == null ||
                  item.GenerationStatus == "completed"));
        var messages = await query
            .OrderBy(item => item.CreateTime, OrderByType.Desc)
            .OrderBy(item => item.Id, OrderByType.Desc)
            .Take(MaxLoadedConversationMessages)
            .ToListAsync();
        messages.Reverse();
        return await BuildMessageOutputsAsync(conversation, messages);
    }

    private async Task<List<SaveLifeAgentMessageOutput>> BuildMessageOutputsAsync(
        SaveLifeAgentConversation conversation,
        IReadOnlyList<SaveLifeAgentMessage> messages)
    {
        var outputs = messages.Select(ToMessageOutput).ToList();
        await AttachProfileProposalsAsync(
            _messageRep.Context,
            conversation.Id,
            conversation.ExternalUserId,
            outputs,
            DateTime.Now,
            App.HttpContext?.RequestAborted ?? default);
        return outputs;
    }

    internal static async Task AttachProfileProposalsAsync(
        ISqlSugarClient db,
        long conversationId,
        string externalUserId,
        IReadOnlyList<SaveLifeAgentMessageOutput> messages,
        DateTime now,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(messages);
        var assistantMessageIds = messages
            .Where(message => message.Role == "assistant")
            .Select(message => long.TryParse(message.Id, out var id) ? id : 0)
            .Where(id => id > 0)
            .Distinct()
            .ToList();
        if (assistantMessageIds.Count == 0)
            return;

        var proposals = await db.Queryable<SaveLifePatientProfileProposal>()
            .Where(item =>
                item.ConversationId == conversationId &&
                item.OwnerExternalUserId == externalUserId &&
                assistantMessageIds.Contains(item.SourceAssistantMessageId) &&
                !item.IsDelete)
            .OrderBy(item => item.Id)
            .ToListAsync(cancellationToken);
        var proposalsByMessage = proposals
            .GroupBy(item => item.SourceAssistantMessageId)
            .ToDictionary(
                group => group.Key,
                group => group.Select(item =>
                {
                    var output = SaveLifePatientProfileProposalPolicy.ToOutput(item);
                    if (output.Status == SaveLifePatientProfileProposalStatuses.Pending &&
                        output.ExpiresAt <= now)
                    {
                        output.Status = SaveLifePatientProfileProposalStatuses.Expired;
                    }
                    return output;
                }).ToList());
        foreach (var message in messages)
        {
            message.ProfileProposals = long.TryParse(message.Id, out var messageId) &&
                proposalsByMessage.TryGetValue(messageId, out var messageProposals)
                ? messageProposals
                : [];
        }
    }

    private static string BuildIntentionReply(
        string intention,
        string status,
        string? overrideContent = null)
    {
        if (!string.IsNullOrWhiteSpace(overrideContent))
            return overrideContent;

        return status switch
        {
            "pending" => "您的意向已提交。请保持电话畅通，工作人员会在1-3个工作日内联系您；您的信息仅用于临床试验初步筛查对接。",
            "draft" => "已为您记录意向草稿。您后续可随时回到小程序继续确认，有疑问也可以继续咨询我。",
            _ => "已记录您暂不参与的选择。感谢您的了解，后续如有需求可随时回来继续查询。"
        };
    }

    private static string? BuildSubmissionValidationReply(
        string requestedStatus,
        SaveLifeEnrollmentValidationResult? validation)
    {
        if (requestedStatus != "pending" || validation == null || validation.CanSubmit)
            return null;
        return $"已为您记录意向草稿。请先完善资料：{validation.Summary}，完善后再确认报名。";
    }

    private static string BuildExistingEnrollmentReply(SaveLifeEnrollmentRecord enrollment)
    {
        var title = string.IsNullOrWhiteSpace(enrollment.ProjectTitle)
            ? "该项目"
            : $"“{enrollment.ProjectTitle}”";
        return enrollment.Status switch
        {
            "draft" => $"已为您保留{title}的意向草稿。请继续完善健康档案和病例资料，完善后可再次确认报名。",
            "pending" => $"您已提交过{title}的报名申请，请在报名详情查看审核进度或补充病例资料。",
            "need_supplement" => $"您已有{title}的报名记录，当前需要补充资料。{enrollment.AuditRemark}",
            "approved" => $"您报名的{title}已通过审核，请留意机构联系。",
            "contacted" => $"您报名的{title}已有机构联系记录，请在报名详情查看进度。",
            _ => $"您已有{title}的报名记录，请在报名详情查看当前状态。"
        };
    }

    private static string BuildMissingTrialIntentionReply(
        SaveLifeEnrollmentValidationResult? validation)
    {
        var profileHint = validation is { CanSubmit: false }
            ? $" 同时请先完善资料：{validation.Summary}。"
            : string.Empty;
        return $"还没有确认具体招募项目，暂不能提交报名。请先告诉我疾病、所在城市、既往治疗情况，或从项目详情页进入助手后再确认参与意向。{profileHint}";
    }

    private static SaveLifeEnrollmentSubmissionFields BuildAgentSubmissionFields(
        SubmitSaveLifeAgentIntentionInput input,
        SaveLifeAgentPatientSignal signal,
        IReadOnlyList<SaveLifeAgentMessageOutput> messages,
        string intention,
        long conversationId)
    {
        return new SaveLifeEnrollmentSubmissionFields
        {
            ContactName = input.ContactName,
            ContactPhone = input.ContactPhone ?? signal.Phone,
            DiseaseDescription = signal.DiseaseKeyword,
            Notes = $"{SaveLifeAgentReplyBuilder.BuildEnrollmentNotes(intention, messages)}\n\n助手会话Id：{conversationId}"
        };
    }

    private static SaveLifeAgentConversationOutput ToConversationOutput(
        SaveLifeAgentConversation conversation)
    {
        return new SaveLifeAgentConversationOutput
        {
            Id = conversation.Id.ToString(),
            UserId = conversation.ExternalUserId,
            Status = conversation.Status,
            Phase = conversation.Phase,
            LastMessage = conversation.LastMessage,
            ContextType = conversation.ContextType,
            ContextId = conversation.ContextId?.ToString(),
            CreatedAt = conversation.CreateTime,
            UpdatedAt = conversation.UpdateTime
        };
    }

    private static SaveLifeAgentMessageOutput ToMessageOutput(SaveLifeAgentMessage message)
    {
        var metadata = ParseMessageMetadata(message.MetadataJson);
        return new SaveLifeAgentMessageOutput
        {
            Id = message.Id.ToString(),
            ConversationId = message.ConversationId.ToString(),
            PatientProfileId = message.PatientProfileId?.ToString(),
            Role = message.Role,
            Content = message.Content,
            ClientMessageId = message.ClientMessageId,
            ReplyToMessageId = message.ReplyToMessageId?.ToString(),
            GenerationStatus = message.GenerationStatus,
            CreatedAt = message.CreateTime,
            MatchedTrials = metadata?.MatchedTrials ?? [],
            MatchedKnowledge = metadata?.MatchedKnowledge ?? [],
            MatchedMaterials = metadata?.MatchedMaterials ?? [],
            ProfileProposals = metadata?.ProfileProposals ?? [],
            BusinessContext = metadata?.BusinessContext,
            SuggestedActions = metadata?.SuggestedActions ?? [],
            GuidanceText = metadata?.GuidanceText,
            SuggestedFollowUps = metadata?.SuggestedFollowUps ?? [],
            ModelProvider = metadata?.ModelProvider,
            UsedModel = metadata?.UsedModel,
            RunStatus = metadata?.RunStatus,
            FallbackReason = metadata?.FallbackReason,
            ErrorSummary = metadata?.ErrorSummary,
            RetrievalDiagnostics = metadata?.RetrievalDiagnostics,
            RetrievalHitCounts = metadata?.RetrievalHitCounts ?? new(),
            StageTimings = metadata?.StageTimings ?? new()
        };
    }

    internal static string SerializeMessageMetadata(SaveLifeAgentMessageMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        return JsonSerializer.Serialize(metadata, MessageMetadataJsonOptions);
    }

    private static SaveLifeAgentMessageMetadata? ParseMessageMetadata(string? metadataJson)
    {
        if (string.IsNullOrWhiteSpace(metadataJson))
            return null;
        try
        {
            return JsonSerializer.Deserialize<SaveLifeAgentMessageMetadata>(
                metadataJson,
                MessageMetadataJsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private async Task<SaveLifeAgentConversationStateOutput> LoadTurnConversationState(
        SaveLifeAgentConversation conversation)
    {
        SaveLifePatientProfile? currentPatient = null;
        if (conversation.CurrentPatientProfileId.HasValue)
        {
            currentPatient = await _patientProfileRep.GetFirstAsync(item =>
                item.Id == conversation.CurrentPatientProfileId.Value &&
                item.OwnerExternalUserId == conversation.ExternalUserId &&
                !item.IsDelete);
        }
        return SaveLifeAgentHistoryPolicy.BuildConversationState(conversation, currentPatient);
    }

    private async Task<SaveLifeAgentBusinessContextOutput?> ResolveBusinessContext(
        long? trialId,
        long? knowledgeId,
        long? enrollmentId,
        string? contextType,
        long? contextId,
        string externalUserId)
    {
        if (!trialId.HasValue && !knowledgeId.HasValue && !enrollmentId.HasValue && contextId.HasValue)
        {
            switch (contextType?.Trim().ToLowerInvariant())
            {
                case "trial":
                    trialId = contextId;
                    break;
                case "knowledge":
                    knowledgeId = contextId;
                    break;
                case "enrollment":
                    enrollmentId = contextId;
                    break;
            }
        }
        return await ResolveBusinessContext(trialId, knowledgeId, enrollmentId, externalUserId);
    }

    private async Task<SaveLifeAgentBusinessContextOutput?> ResolveBusinessContext(
        long? trialId,
        long? knowledgeId,
        long? enrollmentId,
        string externalUserId)
    {
        if (enrollmentId is > 0)
        {
            var enrollment = await _enrollmentRep.GetFirstAsync(item =>
                    item.Id == enrollmentId.Value &&
                    item.ExternalUserId == externalUserId &&
                    !item.IsDelete)
                ?? throw Oops.Oh("报名记录不存在、已删除或不属于当前用户").StatusCode(404);
            return new SaveLifeAgentBusinessContextOutput
            {
                ContextType = "enrollment",
                ContextId = enrollment.Id.ToString(),
                Title = enrollment.ProjectTitle,
                Summary = BuildEnrollmentContextSummary(enrollment)
            };
        }

        if (trialId is > 0)
        {
            var trial = await _trialRep.GetFirstAsync(item =>
                    item.Id == trialId.Value && item.Status == "recruiting")
                ?? throw Oops.Oh("招募项目不存在或已下架").StatusCode(404);
            return new SaveLifeAgentBusinessContextOutput
            {
                ContextType = "trial",
                ContextId = trial.Id.ToString(),
                Title = trial.Title,
                Summary = string.Join("\n", new[]
                {
                    FormatContextField("疾病领域", trial.DiseaseArea),
                    FormatContextField("适应症", trial.Indication),
                    FormatContextField("临床阶段", trial.ClinicalPhase),
                    FormatContextField("地点", trial.Location),
                    FormatContextField("项目描述", Limit(trial.Description, 600)),
                    FormatContextField("受试者权益", Limit(trial.Benefits, 800)),
                    FormatContextField("招募摘要", Limit(trial.Requirements, 600)),
                    FormatContextField("入选标准", Limit(trial.InclusionCriteria, 800)),
                    FormatContextField("排除标准", Limit(trial.ExclusionCriteria, 800)),
                    FormatContextField("参研单位", Limit(trial.Sites, 600))
                }.Where(value => !string.IsNullOrWhiteSpace(value)))
            };
        }

        if (knowledgeId is > 0)
        {
            var knowledge = await _knowledgeRep.GetFirstAsync(item =>
                    item.Id == knowledgeId.Value && item.IsPublished)
                ?? throw Oops.Oh("疾病知识不存在或已下架").StatusCode(404);
            return new SaveLifeAgentBusinessContextOutput
            {
                ContextType = "knowledge",
                ContextId = knowledge.Id.ToString(),
                Title = knowledge.Title,
                Summary = string.Join("\n", new[]
                {
                    knowledge.DiseaseName,
                    knowledge.Summary,
                    Limit(SaveLifeKnowledgeContentSanitizer.ExtractPlainText(knowledge.Content), 600),
                    knowledge.Tags
                }.Where(value => !string.IsNullOrWhiteSpace(value)))
            };
        }

        return null;
    }

    private static int NormalizePage(int page) => page <= 0 ? 1 : page;

    private static int NormalizeConversationPageSize(int pageSize)
    {
        return pageSize <= 0 ? 20 : Math.Min(pageSize, 50);
    }

    private static int NormalizeConversationMessagePageSize(int pageSize)
    {
        return pageSize <= 0 ? 20 : Math.Min(pageSize, 50);
    }

    private static long? ParseOptionalMessageCursor(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;
        if (long.TryParse(value, out var cursor) && cursor > 0)
            return cursor;
        throw Oops.Oh("消息游标格式不正确").StatusCode(400);
    }

    private static string? FormatContextField(string label, string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : $"{label}：{value.Trim()}";
    }

    private static string BuildEnrollmentContextSummary(SaveLifeEnrollmentRecord enrollment)
    {
        return string.Join("\n", new[]
        {
            $"报名状态：{enrollment.Status}",
            enrollment.DiseaseDescription,
            enrollment.MedicalHistory,
            enrollment.Notes
        }.Where(value => !string.IsNullOrWhiteSpace(value)));
    }

    private static string ResolveTraceId()
    {
        var traceId = App.HttpContext?.TraceIdentifier;
        if (string.IsNullOrWhiteSpace(traceId))
            traceId = Activity.Current?.TraceId.ToString();
        return SaveLifeAgentDiagnostics.SanitizeText(traceId, 64) ?? "unknown";
    }

    private static string GetCurrentMiniAppOpenId()
    {
        var httpContext = App.HttpContext
            ?? throw Oops.Oh("登录态无效，请重新登录").StatusCode(401);
        return SaveLifeAgentSimulationIdentity.ResolveExternalUserId(httpContext);
    }

    private static long ParseId(string value, string errorMessage)
    {
        if (long.TryParse(value, out var id) && id > 0)
            return id;
        throw Oops.Oh(errorMessage).StatusCode(400);
    }

    private static string NormalizeRequired(string value, string errorMessage)
    {
        var normalized = value?.Trim();
        return !string.IsNullOrWhiteSpace(normalized)
            ? normalized
            : throw Oops.Oh(errorMessage).StatusCode(400);
    }

    private static string? NormalizeOptional(string? value, int maximumLength)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
            return null;
        if (normalized.Length > maximumLength)
            throw Oops.Oh("输入内容超过长度限制").StatusCode(400);
        return normalized;
    }

    private static string? Limit(string? value, int maximumLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return value;
        return value.Length <= maximumLength ? value : value[..maximumLength];
    }
}
