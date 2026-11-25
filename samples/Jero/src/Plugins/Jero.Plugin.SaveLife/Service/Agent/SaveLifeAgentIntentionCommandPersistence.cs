using System.Diagnostics.CodeAnalysis;
using System.Runtime.ExceptionServices;
using System.Text.Json;
using SqlSugar;

namespace Jero.Plugin.SaveLife.Service;

internal static class SaveLifeAgentIntentionCommandPersistence
{
    private const int MaximumAttempts = 40;
    private static readonly JsonSerializerOptions ResultJsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public static async Task<SaveLifeAgentIntentionCommandReplay<T>> TryReplayAsync<T>(
        ISqlSugarClient db,
        SaveLifeAgentIntentionCommandRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(request);
        ValidateRequest(request);
        var existing = await FindCommandAsync(db, request, cancellationToken);
        return existing == null
            ? SaveLifeAgentIntentionCommandReplay<T>.NotFound()
            : SaveLifeAgentIntentionCommandReplay<T>.Found(Replay<T>(existing, request));
    }

    public static async Task<T> ExecuteAsync<T>(
        ISqlSugarClient db,
        SaveLifeAgentIntentionCommandRequest request,
        Func<ISqlSugarClient, SaveLifeAgentConversation, Task<SaveLifeAgentIntentionCommandPreparation<T>>> prepare,
        DateTime now,
        Func<long> idGenerator,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(prepare);
        ArgumentNullException.ThrowIfNull(idGenerator);
        ValidateRequest(request);

        for (var attempt = 1; attempt <= MaximumAttempts; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                var transaction = await db.Ado.UseTranAsync(async () =>
                {
                    var existing = await FindCommandAsync(db, request, cancellationToken);
                    if (existing != null)
                        return Replay<T>(existing, request);

                    var conversation = await db.Queryable<SaveLifeAgentConversation>()
                        .ClearFilter()
                        .Where(item =>
                            item.Id == request.ConversationId &&
                            item.ExternalUserId == request.ExternalUserId &&
                            item.Status == "active" &&
                            !item.IsDelete)
                        .FirstAsync(cancellationToken);
                    if (conversation == null)
                        throw Oops.Oh("会话不存在、已关闭或不属于当前用户").StatusCode(404);

                    EnsureConversationCurrent(conversation, request, now);
                    var hasPendingTurn = await db.Queryable<SaveLifeAgentMessage>()
                        .ClearFilter()
                        .AnyAsync(item =>
                            item.ConversationId == conversation.Id &&
                            item.Role == "user" &&
                            (item.TurnStatus == SaveLifeAgentTurnStatuses.Pending ||
                             item.TurnStatus == SaveLifeAgentTurnStatuses.Processing) &&
                            !item.IsDelete,
                            cancellationToken);
                    if (hasPendingTurn)
                        throw Oops.Oh("当前会话仍有未完成轮次，请完成后重新确认参与意向").StatusCode(409);

                    var payload = PayloadFor(conversation.PatientContextVersion, request);
                    var payloadHash = SaveLifeAgentIntentionCommandPolicy.ComputePayloadHash(payload);
                    var command = new SaveLifeAgentBusinessCommand
                    {
                        Id = idGenerator(),
                        TenantId = conversation.TenantId,
                        ExternalUserId = conversation.ExternalUserId,
                        ConversationId = conversation.Id,
                        CommandType = SaveLifeAgentIntentionCommandPolicy.CommandType,
                        ClientCommandId = request.ClientCommandId,
                        PayloadHashAlgorithm = payloadHash.Algorithm,
                        PayloadHash = payloadHash.Value,
                        PatientContextVersion = conversation.PatientContextVersion,
                        Status = SaveLifeAgentBusinessCommandStatuses.Processing,
                        CreateTime = now
                    };
                    var commandInserted = await db.Insertable(command)
                        .ExecuteCommandAsync(cancellationToken);
                    if (commandInserted != 1)
                        throw new InvalidOperationException("业务命令占位写入失败");

                    var preparation = await prepare(db, conversation);
                    ArgumentNullException.ThrowIfNull(preparation);
                    ArgumentNullException.ThrowIfNull(preparation.Result);
                    var turnSequence = Math.Max(1, conversation.NextTurnSequence);
                    var userMessage = CreateUserMessage(
                        conversation,
                        request,
                        preparation,
                        turnSequence,
                        now,
                        idGenerator());
                    var userInserted = await db.Insertable(userMessage)
                        .ExecuteCommandAsync(cancellationToken);
                    if (userInserted != 1)
                        throw new InvalidOperationException("意向确认消息写入失败");

                    var assistantMessage = CreateAssistantMessage(
                        conversation,
                        userMessage,
                        preparation,
                        now,
                        idGenerator());
                    var assistantInserted = await db.Insertable(assistantMessage)
                        .ExecuteCommandAsync(cancellationToken);
                    if (assistantInserted != 1)
                        throw new InvalidOperationException("意向结果消息写入失败");

                    var nextRevision = checked(conversation.StateRevision + 1);
                    var conversationUpdated = await db.Updateable<SaveLifeAgentConversation>()
                        .SetColumns(item => new SaveLifeAgentConversation
                        {
                            NextTurnSequence = turnSequence + 1,
                            StateRevision = nextRevision,
                            Status = preparation.ConversationStatus,
                            Phase = preparation.ConversationPhase,
                            LastMessage = Limit(preparation.AssistantContent, 512),
                            EnrollmentRecordId = preparation.EnrollmentRecordId,
                            GenerationLeaseOwner = null,
                            GenerationLeaseTurnSequence = null,
                            GenerationLeaseExpiresTime = null,
                            UpdateTime = now
                        })
                        .Where(item =>
                            item.Id == conversation.Id &&
                            item.ExternalUserId == conversation.ExternalUserId &&
                            item.CurrentPatientProfileId == request.PatientProfileId &&
                            item.PatientContextVersion == conversation.PatientContextVersion &&
                            item.StateRevision == request.ExpectedStateRevision &&
                            item.NextTurnSequence == conversation.NextTurnSequence &&
                            item.Status == "active" &&
                            !item.IsDelete &&
                            (item.GenerationLeaseOwner == null ||
                             item.GenerationLeaseOwner == string.Empty ||
                             item.GenerationLeaseExpiresTime == null ||
                             item.GenerationLeaseExpiresTime <= now))
                        .ExecuteCommandAsync(cancellationToken);
                    if (conversationUpdated != 1)
                        throw SaveLifeAgentTurnConsistencyPolicy.CreateTurnSupersededException();

                    var resultJson = JsonSerializer.Serialize(preparation.Result, ResultJsonOptions);
                    var commandCompleted = await db.Updateable<SaveLifeAgentBusinessCommand>()
                        .SetColumns(item => new SaveLifeAgentBusinessCommand
                        {
                            Status = SaveLifeAgentBusinessCommandStatuses.Completed,
                            ResultJson = resultJson,
                            UserMessageId = userMessage.Id,
                            AssistantMessageId = assistantMessage.Id,
                            EnrollmentRecordId = preparation.EnrollmentRecordId,
                            UpdateTime = now
                        })
                        .Where(item =>
                            item.Id == command.Id &&
                            item.Status == SaveLifeAgentBusinessCommandStatuses.Processing &&
                            !item.IsDelete)
                        .ExecuteCommandAsync(cancellationToken);
                    if (commandCompleted != 1)
                        throw new InvalidOperationException("业务命令结果写入失败");

                    return preparation.Result;
                }, _ => { });
                if (!transaction.IsSuccess)
                    Rethrow(transaction.ErrorException, "意向命令事务失败");

                return transaction.Data;
            }
            catch (Exception error) when (
                attempt < MaximumAttempts &&
                (IsSqliteBusy(error) || IsCommandScopeConflict(error)))
            {
                await Task.Delay(TimeSpan.FromMilliseconds(Math.Min(10 * attempt, 100)), cancellationToken);
            }
        }

        throw new InvalidOperationException("意向命令在最大并发重试次数内未完成");
    }

    private static async Task<SaveLifeAgentBusinessCommand?> FindCommandAsync(
        ISqlSugarClient db,
        SaveLifeAgentIntentionCommandRequest request,
        CancellationToken cancellationToken)
    {
        return await db.Queryable<SaveLifeAgentBusinessCommand>()
            .ClearFilter()
            .Where(item =>
                item.ExternalUserId == request.ExternalUserId &&
                item.ConversationId == request.ConversationId &&
                item.CommandType == SaveLifeAgentIntentionCommandPolicy.CommandType &&
                item.ClientCommandId == request.ClientCommandId)
            .FirstAsync(cancellationToken);
    }

    private static T Replay<T>(
        SaveLifeAgentBusinessCommand existing,
        SaveLifeAgentIntentionCommandRequest request)
    {
        var payloadHash = SaveLifeAgentIntentionCommandPolicy.ComputePayloadHash(
            PayloadFor(existing.PatientContextVersion, request));
        if (!string.Equals(existing.PayloadHashAlgorithm, payloadHash.Algorithm, StringComparison.Ordinal) ||
            !string.Equals(existing.PayloadHash, payloadHash.Value, StringComparison.Ordinal))
        {
            throw Oops.Oh("客户端命令 Id 已被其他意向载荷使用").StatusCode(409);
        }

        if (existing.Status != SaveLifeAgentBusinessCommandStatuses.Completed ||
            string.IsNullOrWhiteSpace(existing.ResultJson))
        {
            throw Oops.Oh("意向命令仍在处理中，请稍后使用相同命令 Id 重试").StatusCode(409);
        }

        return JsonSerializer.Deserialize<T>(existing.ResultJson, ResultJsonOptions)
            ?? throw new InvalidOperationException("已完成业务命令缺少有效结果");
    }

    private static void EnsureConversationCurrent(
        SaveLifeAgentConversation conversation,
        SaveLifeAgentIntentionCommandRequest request,
        DateTime now)
    {
        if (conversation.CurrentPatientProfileId != request.PatientProfileId ||
            conversation.PatientContextVersion <= 0)
        {
            throw SaveLifeAgentTurnConsistencyPolicy.CreateException();
        }

        if (conversation.StateRevision != request.ExpectedStateRevision)
            throw SaveLifeAgentTurnConsistencyPolicy.CreateTurnSupersededException();

        var hasActiveLease = !string.IsNullOrWhiteSpace(conversation.GenerationLeaseOwner) &&
            conversation.GenerationLeaseExpiresTime.HasValue &&
            conversation.GenerationLeaseExpiresTime.Value > now;
        if (hasActiveLease)
            throw Oops.Oh("当前会话仍有回复正在生成，请完成后重新确认参与意向").StatusCode(409);

        if (conversation.NextTurnSequence <= 0)
            throw Oops.Oh("当前会话仍有未完成轮次，请完成后重新确认参与意向").StatusCode(409);
    }

    private static SaveLifeAgentMessage CreateUserMessage<T>(
        SaveLifeAgentConversation conversation,
        SaveLifeAgentIntentionCommandRequest request,
        SaveLifeAgentIntentionCommandPreparation<T> preparation,
        long turnSequence,
        DateTime now,
        long id)
    {
        return new SaveLifeAgentMessage
        {
            Id = id,
            TenantId = conversation.TenantId,
            ConversationId = conversation.Id,
            PatientProfileId = conversation.CurrentPatientProfileId,
            PatientContextVersion = conversation.PatientContextVersion,
            TurnSequence = turnSequence,
            TurnStatus = SaveLifeAgentTurnStatuses.Completed,
            TurnStateRevision = request.ExpectedStateRevision,
            ExternalUserId = conversation.ExternalUserId,
            Role = "user",
            Content = preparation.UserContent,
            CreateTime = now
        };
    }

    private static SaveLifeAgentMessage CreateAssistantMessage<T>(
        SaveLifeAgentConversation conversation,
        SaveLifeAgentMessage userMessage,
        SaveLifeAgentIntentionCommandPreparation<T> preparation,
        DateTime now,
        long id)
    {
        return new SaveLifeAgentMessage
        {
            Id = id,
            TenantId = conversation.TenantId,
            ConversationId = conversation.Id,
            PatientProfileId = conversation.CurrentPatientProfileId,
            PatientContextVersion = conversation.PatientContextVersion,
            ExternalUserId = conversation.ExternalUserId,
            Role = "assistant",
            Content = preparation.AssistantContent,
            MetadataJson = preparation.AssistantMetadataJson,
            ReplyToMessageId = userMessage.Id,
            GenerationStatus = "completed",
            GenerationStartedTime = now,
            CreateTime = now
        };
    }

    private static SaveLifeAgentIntentionCommandPayload PayloadFor(
        long patientContextVersion,
        SaveLifeAgentIntentionCommandRequest request)
    {
        return new SaveLifeAgentIntentionCommandPayload(
            request.PatientProfileId,
            patientContextVersion,
            request.ProjectId,
            request.ExpectedStateRevision,
            request.Intention,
            request.ContactName,
            request.ContactPhone);
    }

    private static void ValidateRequest(SaveLifeAgentIntentionCommandRequest request)
    {
        if (request.ConversationId <= 0 ||
            request.PatientProfileId <= 0 ||
            request.ProjectId < 0 ||
            request.ExpectedStateRevision < 0 ||
            string.IsNullOrWhiteSpace(request.ExternalUserId) ||
            request.ExternalUserId.Length > 64 ||
            string.IsNullOrWhiteSpace(request.ClientCommandId) ||
            request.ClientCommandId.Length > 128 ||
            string.IsNullOrWhiteSpace(request.Intention))
        {
            throw Oops.Oh("意向命令参数不完整或格式不正确").StatusCode(400);
        }
    }

    private static bool IsSqliteBusy(Exception? exception)
    {
        return ExceptionChainContains(exception, "database is locked") ||
            ExceptionChainContains(exception, "database is busy");
    }

    private static bool IsCommandScopeConflict(Exception? exception)
    {
        return ExceptionChainContains(exception, SaveLifeAgentCommandMigration.CommandScopeUniqueIndexName) ||
            ExceptionChainContains(exception, "sl_agent_business_command.TenantId");
    }

    private static bool ExceptionChainContains(Exception? exception, string value)
    {
        for (var current = exception; current != null; current = current.InnerException)
        {
            if (current.Message.Contains(value, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    [DoesNotReturn]
    private static void Rethrow(Exception? exception, string fallbackMessage)
    {
        if (exception == null)
            throw new InvalidOperationException(fallbackMessage);

        ExceptionDispatchInfo.Capture(exception).Throw();
        throw new InvalidOperationException(fallbackMessage);
    }

    private static string Limit(string value, int length)
    {
        return value.Length <= length ? value : value[..length];
    }
}

internal sealed record SaveLifeAgentIntentionCommandRequest(
    long? TenantId,
    string ExternalUserId,
    long ConversationId,
    string ClientCommandId,
    long ExpectedStateRevision,
    long PatientProfileId,
    long ProjectId,
    string Intention,
    string? ContactName = null,
    string? ContactPhone = null);

internal sealed record SaveLifeAgentIntentionCommandPreparation<T>(
    T Result,
    string UserContent,
    string AssistantContent,
    string? AssistantMetadataJson,
    string ConversationStatus,
    string ConversationPhase,
    long? EnrollmentRecordId);

internal sealed record SaveLifeAgentIntentionCommandReplay<T>(bool Exists, T? Result)
{
    public static SaveLifeAgentIntentionCommandReplay<T> NotFound() => new(false, default);

    public static SaveLifeAgentIntentionCommandReplay<T> Found(T result) => new(true, result);
}

internal static class SaveLifeAgentBusinessCommandStatuses
{
    public const string Processing = "processing";
    public const string Completed = "completed";
}
