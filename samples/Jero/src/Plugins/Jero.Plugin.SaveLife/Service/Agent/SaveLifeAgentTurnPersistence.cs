using SqlSugar;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.ExceptionServices;

namespace Jero.Plugin.SaveLife.Service;

internal static class SaveLifeAgentTurnPersistence
{
    private const int MaximumAcceptAttempts = 20;

    public static readonly TimeSpan DefaultGenerationLeaseDuration = TimeSpan.FromMinutes(6);

    public static async Task<SaveLifeAgentGenerationReservation> AcquireGenerationAsync(
        ISqlSugarClient db,
        long userMessageId,
        string externalUserId,
        string leaseOwner,
        DateTime now,
        TimeSpan leaseDuration,
        Func<long> idGenerator,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(idGenerator);
        var normalizedUserId = NormalizeRequired(externalUserId, 64, nameof(externalUserId));
        var normalizedLeaseOwner = NormalizeRequired(leaseOwner, 64, nameof(leaseOwner));
        if (leaseDuration <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(leaseDuration));
        var expiresTime = now.Add(leaseDuration);

        var transaction = await db.Ado.UseTranAsync(async () =>
        {
            var userMessage = await db.Queryable<SaveLifeAgentMessage>()
                .ClearFilter()
                .Where(item =>
                    item.Id == userMessageId &&
                    item.ExternalUserId == normalizedUserId &&
                    item.Role == "user" &&
                    !item.IsDelete)
                .FirstAsync(cancellationToken);
            if (userMessage?.TurnSequence is not > 0)
                throw new InvalidOperationException("用户消息缺少有效轮次序号");

            var assistantMessage = await db.Queryable<SaveLifeAgentMessage>()
                .ClearFilter()
                .Where(item =>
                    item.ReplyToMessageId == userMessage.Id &&
                    item.Role == "assistant" &&
                    !item.IsDelete)
                .FirstAsync(cancellationToken);
            if (userMessage.TurnStatus == SaveLifeAgentTurnStatuses.Completed)
            {
                if (assistantMessage?.GenerationStatus != "completed")
                    throw new InvalidOperationException("已完成轮次缺少已完成助手回复");

                return SaveLifeAgentGenerationReservation.Replay(assistantMessage);
            }

            if (userMessage.TurnStatus == SaveLifeAgentTurnStatuses.Failed)
            {
                var hasLaterTurn = await db.Queryable<SaveLifeAgentMessage>()
                    .ClearFilter()
                    .AnyAsync(item =>
                        item.ConversationId == userMessage.ConversationId &&
                        item.Role == "user" &&
                        item.TurnSequence > userMessage.TurnSequence &&
                        !item.IsDelete);
                if (hasLaterTurn)
                    throw SaveLifeAgentTurnConsistencyPolicy.CreateTurnSupersededException();
            }
            else
            {
                var earliest = await db.Queryable<SaveLifeAgentMessage>()
                    .ClearFilter()
                    .Where(item =>
                        item.ConversationId == userMessage.ConversationId &&
                        item.Role == "user" &&
                        (item.TurnStatus == SaveLifeAgentTurnStatuses.Pending ||
                         item.TurnStatus == SaveLifeAgentTurnStatuses.Processing) &&
                        !item.IsDelete)
                    .OrderBy(item => item.TurnSequence)
                    .FirstAsync(cancellationToken);
                if (earliest?.Id != userMessage.Id)
                {
                    throw new SaveLifeAgentTurnBlockedException(
                        SaveLifeAgentTurnBlockReasons.EarlierTurnPending);
                }
            }

            var conversation = await db.Queryable<SaveLifeAgentConversation>()
                .ClearFilter()
                .Where(item =>
                    item.Id == userMessage.ConversationId &&
                    item.ExternalUserId == normalizedUserId &&
                    item.Status == "active" &&
                    !item.IsDelete)
                .FirstAsync(cancellationToken);
            if (conversation == null)
                throw new InvalidOperationException("会话不存在或已关闭");
            SaveLifeAgentTurnConsistencyPolicy.EnsureCurrent(conversation, userMessage);

            var hasActiveLease = !string.IsNullOrWhiteSpace(conversation.GenerationLeaseOwner) &&
                conversation.GenerationLeaseExpiresTime.HasValue &&
                conversation.GenerationLeaseExpiresTime.Value > now;
            if (hasActiveLease)
            {
                throw new SaveLifeAgentTurnBlockedException(
                    SaveLifeAgentTurnBlockReasons.GenerationInProgress);
            }

            var claimed = await db.Updateable<SaveLifeAgentConversation>()
                .SetColumns(item => new SaveLifeAgentConversation
                {
                    GenerationLeaseOwner = normalizedLeaseOwner,
                    GenerationLeaseTurnSequence = userMessage.TurnSequence,
                    GenerationLeaseExpiresTime = expiresTime,
                    UpdateTime = now
                })
                .Where(item =>
                    item.Id == conversation.Id &&
                    item.StateRevision == conversation.StateRevision &&
                    item.Status == "active" &&
                    !item.IsDelete &&
                    (item.GenerationLeaseOwner == null ||
                     item.GenerationLeaseOwner == string.Empty ||
                     item.GenerationLeaseExpiresTime == null ||
                     item.GenerationLeaseExpiresTime <= now))
                .ExecuteCommandAsync(cancellationToken);
            if (claimed != 1)
            {
                throw new SaveLifeAgentTurnBlockedException(
                    SaveLifeAgentTurnBlockReasons.GenerationInProgress);
            }

            var userClaimed = await db.Updateable<SaveLifeAgentMessage>()
                .SetColumns(item => new SaveLifeAgentMessage
                {
                    TurnStatus = SaveLifeAgentTurnStatuses.Processing,
                    TurnStateRevision = conversation.StateRevision,
                    UpdateTime = now
                })
                .Where(item =>
                    item.Id == userMessage.Id &&
                    item.TurnSequence == userMessage.TurnSequence &&
                    (item.TurnStatus == SaveLifeAgentTurnStatuses.Pending ||
                     item.TurnStatus == SaveLifeAgentTurnStatuses.Failed ||
                     item.TurnStatus == SaveLifeAgentTurnStatuses.Processing) &&
                    !item.IsDelete)
                .ExecuteCommandAsync(cancellationToken);
            if (userClaimed != 1)
                throw SaveLifeAgentTurnConsistencyPolicy.CreateTurnSupersededException();

            if (assistantMessage == null)
            {
                assistantMessage = new SaveLifeAgentMessage
                {
                    Id = idGenerator(),
                    TenantId = userMessage.TenantId,
                    ConversationId = userMessage.ConversationId,
                    ExternalUserId = userMessage.ExternalUserId,
                    Role = "assistant",
                    Content = string.Empty,
                    ReplyToMessageId = userMessage.Id,
                    GenerationStatus = "processing",
                    GenerationStartedTime = now,
                    GenerationLeaseOwner = normalizedLeaseOwner,
                    GenerationLeaseExpiresTime = expiresTime,
                    CreateTime = now
                };
                SaveLifeAgentPatientBinding.BindReply(userMessage, assistantMessage);
                var inserted = await db.Insertable(assistantMessage).ExecuteCommandAsync(cancellationToken);
                if (inserted != 1)
                    throw new InvalidOperationException("助手回复租约创建失败");
            }
            else
            {
                var replyClaimed = await db.Updateable<SaveLifeAgentMessage>()
                    .SetColumns(item => new SaveLifeAgentMessage
                    {
                        Content = string.Empty,
                        MetadataJson = null,
                        GenerationStatus = "processing",
                        GenerationStartedTime = now,
                        GenerationLeaseOwner = normalizedLeaseOwner,
                        GenerationLeaseExpiresTime = expiresTime,
                        UpdateTime = now
                    })
                    .Where(item => item.Id == assistantMessage.Id && !item.IsDelete)
                    .ExecuteCommandAsync(cancellationToken);
                if (replyClaimed != 1)
                    throw new InvalidOperationException("助手回复租约接管失败");
                assistantMessage.Content = string.Empty;
                assistantMessage.MetadataJson = null;
                assistantMessage.GenerationStatus = "processing";
                assistantMessage.GenerationStartedTime = now;
                assistantMessage.GenerationLeaseOwner = normalizedLeaseOwner;
                assistantMessage.GenerationLeaseExpiresTime = expiresTime;
            }

            userMessage.TurnStatus = SaveLifeAgentTurnStatuses.Processing;
            userMessage.TurnStateRevision = conversation.StateRevision;
            conversation.GenerationLeaseOwner = normalizedLeaseOwner;
            conversation.GenerationLeaseTurnSequence = userMessage.TurnSequence;
            conversation.GenerationLeaseExpiresTime = expiresTime;
            return SaveLifeAgentGenerationReservation.Acquired(new SaveLifeAgentGenerationLease(
                normalizedLeaseOwner,
                userMessage.TurnSequence.Value,
                conversation.StateRevision,
                expiresTime,
                conversation,
                userMessage,
                assistantMessage));
        }, _ => { });
        if (!transaction.IsSuccess)
            Rethrow(transaction.ErrorException, "生成租约事务失败");

        return transaction.Data;
    }

    public static async Task<SaveLifeAgentTurnCompletionResult<T>> CompleteGenerationAsync<T>(
        ISqlSugarClient db,
        SaveLifeAgentGenerationLease lease,
        Func<SaveLifeAgentConversation, SaveLifeAgentMessage, Task<SaveLifeAgentTurnCompletion<T>>> prepare,
        DateTime completedTime,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(lease);
        ArgumentNullException.ThrowIfNull(prepare);
        var transaction = await db.Ado.UseTranAsync(async () =>
        {
            var conversation = await LoadConversationForCommitAsync(
                db,
                lease,
                completedTime,
                cancellationToken);
            var userMessage = await db.Queryable<SaveLifeAgentMessage>()
                .ClearFilter()
                .Where(item =>
                    item.Id == lease.UserMessage.Id &&
                    item.TurnSequence == lease.TurnSequence &&
                    item.TurnStatus == SaveLifeAgentTurnStatuses.Processing &&
                    item.TurnStateRevision == lease.StateRevision &&
                    !item.IsDelete)
                .FirstAsync(cancellationToken);
            var assistantMessage = await db.Queryable<SaveLifeAgentMessage>()
                .ClearFilter()
                .Where(item =>
                    item.Id == lease.AssistantMessage.Id &&
                    item.ReplyToMessageId == lease.UserMessage.Id &&
                    item.GenerationStatus == "processing" &&
                    item.GenerationLeaseOwner == lease.Owner &&
                    !item.IsDelete)
                .FirstAsync(cancellationToken);
            if (userMessage == null || assistantMessage == null)
                throw SaveLifeAgentTurnConsistencyPolicy.CreateTurnSupersededException();
            SaveLifeAgentTurnConsistencyPolicy.EnsureCurrent(conversation, userMessage);

            var completion = await prepare(conversation, assistantMessage);
            var nextRevision = checked(lease.StateRevision + 1);
            var conversationUpdated = await db.Updateable<SaveLifeAgentConversation>()
                .SetColumns(item => new SaveLifeAgentConversation
                {
                    FlowStateJson = conversation.FlowStateJson,
                    FlowStateVersion = conversation.FlowStateVersion,
                    LastMessage = conversation.LastMessage,
                    MatchedTrialsJson = conversation.MatchedTrialsJson,
                    Phase = conversation.Phase,
                    ContextType = conversation.ContextType,
                    ContextId = conversation.ContextId,
                    EnrollmentRecordId = conversation.EnrollmentRecordId,
                    StateRevision = nextRevision,
                    GenerationLeaseOwner = null,
                    GenerationLeaseTurnSequence = null,
                    GenerationLeaseExpiresTime = null,
                    UpdateTime = completedTime
                })
                .Where(item =>
                    item.Id == conversation.Id &&
                    item.StateRevision == lease.StateRevision &&
                    item.GenerationLeaseOwner == lease.Owner &&
                    item.GenerationLeaseTurnSequence == lease.TurnSequence &&
                    item.GenerationLeaseExpiresTime == lease.ExpiresTime &&
                    item.GenerationLeaseExpiresTime > completedTime &&
                    item.PatientContextVersion == userMessage.PatientContextVersion &&
                    item.Status == "active" &&
                    !item.IsDelete)
                .ExecuteCommandAsync(cancellationToken);
            if (conversationUpdated != 1)
                throw SaveLifeAgentTurnConsistencyPolicy.CreateTurnSupersededException();

            var assistantUpdated = await db.Updateable<SaveLifeAgentMessage>()
                .SetColumns(item => new SaveLifeAgentMessage
                {
                    Content = completion.Content,
                    MetadataJson = completion.MetadataJson,
                    GenerationStatus = "completed",
                    GenerationLeaseOwner = null,
                    GenerationLeaseExpiresTime = null,
                    UpdateTime = completedTime
                })
                .Where(item =>
                    item.Id == assistantMessage.Id &&
                    item.GenerationStatus == "processing" &&
                    item.GenerationLeaseOwner == lease.Owner &&
                    !item.IsDelete)
                .ExecuteCommandAsync(cancellationToken);
            if (assistantUpdated != 1)
                throw SaveLifeAgentTurnConsistencyPolicy.CreateTurnSupersededException();

            var userUpdated = await db.Updateable<SaveLifeAgentMessage>()
                .SetColumns(item => new SaveLifeAgentMessage
                {
                    TurnStatus = SaveLifeAgentTurnStatuses.Completed,
                    UpdateTime = completedTime
                })
                .Where(item =>
                    item.Id == userMessage.Id &&
                    item.TurnStatus == SaveLifeAgentTurnStatuses.Processing &&
                    item.TurnStateRevision == lease.StateRevision &&
                    !item.IsDelete)
                .ExecuteCommandAsync(cancellationToken);
            if (userUpdated != 1)
                throw SaveLifeAgentTurnConsistencyPolicy.CreateTurnSupersededException();

            conversation.StateRevision = nextRevision;
            conversation.GenerationLeaseOwner = null;
            conversation.GenerationLeaseTurnSequence = null;
            conversation.GenerationLeaseExpiresTime = null;
            assistantMessage.Content = completion.Content;
            assistantMessage.MetadataJson = completion.MetadataJson;
            assistantMessage.GenerationStatus = "completed";
            assistantMessage.GenerationLeaseOwner = null;
            assistantMessage.GenerationLeaseExpiresTime = null;
            userMessage.TurnStatus = SaveLifeAgentTurnStatuses.Completed;
            return new SaveLifeAgentTurnCompletionResult<T>(
                completion.Data,
                conversation,
                userMessage,
                assistantMessage);
        }, _ => { });
        if (!transaction.IsSuccess)
            Rethrow(transaction.ErrorException, "生成完成事务失败");

        return transaction.Data;
    }

    public static async Task<bool> FailGenerationAsync(
        ISqlSugarClient db,
        SaveLifeAgentGenerationLease lease,
        string? metadataJson,
        DateTime failedTime,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(lease);
        var transaction = await db.Ado.UseTranAsync(async () =>
        {
            var conversation = await db.Queryable<SaveLifeAgentConversation>()
                .ClearFilter()
                .Where(item =>
                    item.Id == lease.Conversation.Id &&
                    item.GenerationLeaseOwner == lease.Owner &&
                    item.GenerationLeaseTurnSequence == lease.TurnSequence &&
                    !item.IsDelete)
                .FirstAsync(cancellationToken);
            if (conversation == null)
                return false;

            var assistantUpdated = await db.Updateable<SaveLifeAgentMessage>()
                .SetColumns(item => new SaveLifeAgentMessage
                {
                    Content = string.Empty,
                    MetadataJson = metadataJson,
                    GenerationStatus = "failed",
                    GenerationLeaseOwner = null,
                    GenerationLeaseExpiresTime = null,
                    UpdateTime = failedTime
                })
                .Where(item =>
                    item.Id == lease.AssistantMessage.Id &&
                    item.GenerationStatus == "processing" &&
                    item.GenerationLeaseOwner == lease.Owner &&
                    !item.IsDelete)
                .ExecuteCommandAsync(cancellationToken);
            var userUpdated = await db.Updateable<SaveLifeAgentMessage>()
                .SetColumns(item => new SaveLifeAgentMessage
                {
                    TurnStatus = SaveLifeAgentTurnStatuses.Failed,
                    UpdateTime = failedTime
                })
                .Where(item =>
                    item.Id == lease.UserMessage.Id &&
                    item.TurnStatus == SaveLifeAgentTurnStatuses.Processing &&
                    item.TurnStateRevision == lease.StateRevision &&
                    !item.IsDelete)
                .ExecuteCommandAsync(cancellationToken);
            var conversationUpdated = await db.Updateable<SaveLifeAgentConversation>()
                .SetColumns(item => new SaveLifeAgentConversation
                {
                    GenerationLeaseOwner = null,
                    GenerationLeaseTurnSequence = null,
                    GenerationLeaseExpiresTime = null,
                    UpdateTime = failedTime
                })
                .Where(item =>
                    item.Id == conversation.Id &&
                    item.GenerationLeaseOwner == lease.Owner &&
                    item.GenerationLeaseTurnSequence == lease.TurnSequence &&
                    !item.IsDelete)
                .ExecuteCommandAsync(cancellationToken);
            if (assistantUpdated != 1 || userUpdated != 1 || conversationUpdated != 1)
                throw new InvalidOperationException("生成失败状态未完整持久化");

            return true;
        }, _ => { });
        if (!transaction.IsSuccess)
            Rethrow(transaction.ErrorException, "生成失败事务失败");

        return transaction.Data;
    }

    public static async Task<SaveLifeAgentMessage> AcceptUserMessageAsync(
        ISqlSugarClient db,
        long conversationId,
        string externalUserId,
        string content,
        string clientMessageId,
        long? contextSourceMessageId,
        long? expectedPatientProfileId,
        long expectedPatientContextVersion,
        DateTime acceptedTime,
        Func<long> idGenerator,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(idGenerator);
        var normalizedUserId = NormalizeRequired(externalUserId, 64, nameof(externalUserId));
        var normalizedContent = NormalizeRequired(content, int.MaxValue, nameof(content));
        var normalizedClientMessageId = NormalizeRequired(clientMessageId, 64, nameof(clientMessageId));

        for (var attempt = 0; attempt < MaximumAcceptAttempts; attempt++)
        {
            var transaction = await db.Ado.UseTranAsync(async () =>
            {
                var existing = await FindByClientMessageIdAsync(
                    db,
                    conversationId,
                    normalizedUserId,
                    normalizedClientMessageId,
                    cancellationToken);
                if (existing != null)
                {
                    EnsureIdempotent(existing, normalizedContent, contextSourceMessageId);
                    return existing;
                }

                var conversation = await db.Queryable<SaveLifeAgentConversation>()
                    .ClearFilter()
                    .Where(item =>
                        item.Id == conversationId &&
                        item.ExternalUserId == normalizedUserId &&
                        item.Status == "active" &&
                        !item.IsDelete)
                    .FirstAsync(cancellationToken);
                if (conversation == null)
                    throw new InvalidOperationException("会话不存在或已关闭");
                if (conversation.CurrentPatientProfileId != expectedPatientProfileId ||
                    conversation.PatientContextVersion != expectedPatientContextVersion)
                {
                    throw SaveLifeAgentTurnConsistencyPolicy.CreateException();
                }

                var sequence = Math.Max(1, conversation.NextTurnSequence);
                var advanced = await db.Updateable<SaveLifeAgentConversation>()
                    .SetColumns(item => new SaveLifeAgentConversation
                    {
                        NextTurnSequence = sequence + 1,
                        LastMessage = Limit(normalizedContent, 512),
                        UpdateTime = acceptedTime
                    })
                    .Where(item =>
                        item.Id == conversation.Id &&
                        item.ExternalUserId == normalizedUserId &&
                        item.NextTurnSequence == conversation.NextTurnSequence &&
                        item.Status == "active" &&
                        !item.IsDelete)
                    .ExecuteCommandAsync(cancellationToken);
                if (advanced != 1)
                    throw new SaveLifeAgentTurnAcceptContentionException();

                var message = new SaveLifeAgentMessage
                {
                    Id = idGenerator(),
                    TenantId = conversation.TenantId,
                    ConversationId = conversation.Id,
                    ExternalUserId = normalizedUserId,
                    PatientProfileId = conversation.CurrentPatientProfileId,
                    PatientContextVersion = Math.Max(1, conversation.PatientContextVersion),
                    ContextSourceMessageId = contextSourceMessageId,
                    Role = "user",
                    Content = normalizedContent,
                    ClientMessageId = normalizedClientMessageId,
                    TurnSequence = sequence,
                    TurnStatus = SaveLifeAgentTurnStatuses.Pending,
                    CreateTime = acceptedTime
                };
                var inserted = await db.Insertable(message).ExecuteCommandAsync(cancellationToken);
                if (inserted != 1)
                    throw new InvalidOperationException("用户消息接收失败");

                return message;
            }, _ => { });
            if (transaction.IsSuccess)
                return transaction.Data;

            var replay = await FindByClientMessageIdAsync(
                db,
                conversationId,
                normalizedUserId,
                normalizedClientMessageId,
                cancellationToken);
            if (replay != null)
            {
                EnsureIdempotent(replay, normalizedContent, contextSourceMessageId);
                return replay;
            }

            if (transaction.ErrorException is not SaveLifeAgentTurnAcceptContentionException &&
                !IsSqliteBusy(transaction.ErrorException))
            {
                if (transaction.ErrorException == null)
                    throw new InvalidOperationException("用户消息接收事务失败");

                ExceptionDispatchInfo.Capture(transaction.ErrorException).Throw();
            }

            await Task.Delay(TimeSpan.FromMilliseconds(5 * (attempt + 1)), cancellationToken);
        }

        throw new InvalidOperationException("用户消息接收竞争持续超限，请重试");
    }

    private static async Task<SaveLifeAgentMessage?> FindByClientMessageIdAsync(
        ISqlSugarClient db,
        long conversationId,
        string externalUserId,
        string clientMessageId,
        CancellationToken cancellationToken)
    {
        var rows = await db.Queryable<SaveLifeAgentMessage>()
            .ClearFilter()
            .Where(item =>
                item.ConversationId == conversationId &&
                item.ExternalUserId == externalUserId &&
                item.ClientMessageId == clientMessageId &&
                !item.IsDelete)
            .Take(1)
            .ToListAsync(cancellationToken);
        return rows.FirstOrDefault();
    }

    private static void EnsureIdempotent(
        SaveLifeAgentMessage existing,
        string content,
        long? contextSourceMessageId)
    {
        if (existing.Role != "user" ||
            existing.Content != content ||
            existing.ContextSourceMessageId != contextSourceMessageId)
        {
            throw new SaveLifeAgentTurnIdempotencyConflictException();
        }
    }

    private static bool IsSqliteBusy(Exception? exception)
    {
        for (var current = exception; current != null; current = current.InnerException)
        {
            if (current.Message.Contains("database is locked", StringComparison.OrdinalIgnoreCase) ||
                current.Message.Contains("database is busy", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static async Task<SaveLifeAgentConversation> LoadConversationForCommitAsync(
        ISqlSugarClient db,
        SaveLifeAgentGenerationLease lease,
        DateTime completedTime,
        CancellationToken cancellationToken)
    {
        var conversation = await db.Queryable<SaveLifeAgentConversation>()
            .ClearFilter()
            .Where(item =>
                item.Id == lease.Conversation.Id &&
                item.ExternalUserId == lease.Conversation.ExternalUserId &&
                item.StateRevision == lease.StateRevision &&
                item.GenerationLeaseOwner == lease.Owner &&
                item.GenerationLeaseTurnSequence == lease.TurnSequence &&
                item.GenerationLeaseExpiresTime == lease.ExpiresTime &&
                item.GenerationLeaseExpiresTime > completedTime &&
                item.Status == "active" &&
                !item.IsDelete)
            .FirstAsync(cancellationToken);
        if (conversation != null)
            return conversation;

        var latest = await db.Queryable<SaveLifeAgentConversation>()
            .ClearFilter()
            .Where(item => item.Id == lease.Conversation.Id && !item.IsDelete)
            .FirstAsync(cancellationToken);
        if (latest != null &&
            (latest.CurrentPatientProfileId != lease.UserMessage.PatientProfileId ||
             latest.PatientContextVersion != lease.UserMessage.PatientContextVersion))
        {
            throw SaveLifeAgentTurnConsistencyPolicy.CreateException();
        }

        throw SaveLifeAgentTurnConsistencyPolicy.CreateTurnSupersededException();
    }

    [DoesNotReturn]
    private static void Rethrow(Exception? exception, string fallbackMessage)
    {
        if (exception == null)
            throw new InvalidOperationException(fallbackMessage);

        ExceptionDispatchInfo.Capture(exception).Throw();
        throw new InvalidOperationException(fallbackMessage);
    }

    private static string NormalizeRequired(string? value, int maximumLength, string parameterName)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized) || normalized.Length > maximumLength)
            throw new ArgumentException("值为空或超过长度限制", parameterName);

        return normalized;
    }

    private static string Limit(string value, int length)
    {
        return value.Length <= length ? value : value[..length];
    }

    private sealed class SaveLifeAgentTurnAcceptContentionException : Exception;
}

internal sealed record SaveLifeAgentGenerationLease(
    string Owner,
    long TurnSequence,
    long StateRevision,
    DateTime ExpiresTime,
    SaveLifeAgentConversation Conversation,
    SaveLifeAgentMessage UserMessage,
    SaveLifeAgentMessage AssistantMessage);

internal sealed record SaveLifeAgentGenerationReservation(
    bool OwnsGeneration,
    SaveLifeAgentGenerationLease? Lease,
    SaveLifeAgentMessage AssistantMessage)
{
    public static SaveLifeAgentGenerationReservation Acquired(SaveLifeAgentGenerationLease lease)
    {
        return new SaveLifeAgentGenerationReservation(true, lease, lease.AssistantMessage);
    }

    public static SaveLifeAgentGenerationReservation Replay(SaveLifeAgentMessage assistantMessage)
    {
        return new SaveLifeAgentGenerationReservation(false, null, assistantMessage);
    }
}

internal sealed record SaveLifeAgentTurnCompletion<T>(
    string Content,
    string? MetadataJson,
    T Data);

internal sealed record SaveLifeAgentTurnCompletionResult<T>(
    T Data,
    SaveLifeAgentConversation Conversation,
    SaveLifeAgentMessage UserMessage,
    SaveLifeAgentMessage AssistantMessage);

internal static class SaveLifeAgentTurnBlockReasons
{
    public const string EarlierTurnPending = "earlier_turn_pending";
    public const string GenerationInProgress = "generation_in_progress";
}

internal sealed class SaveLifeAgentTurnBlockedException : InvalidOperationException
{
    public SaveLifeAgentTurnBlockedException(string reason)
        : base("会话中已有需要先完成的轮次")
    {
        Reason = reason;
    }

    public string Reason { get; }
}

internal sealed class SaveLifeAgentTurnIdempotencyConflictException : InvalidOperationException
{
    public SaveLifeAgentTurnIdempotencyConflictException()
        : base("客户端消息 Id 已被其他内容使用")
    {
    }
}
