using AGUI.Server;
using System.Text.Json;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Jero.Plugin.SaveLife.Service;

public sealed class SaveLifeMafSessionStore(
    IHttpContextAccessor httpContextAccessor) : AgentSessionStore
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public override async ValueTask<AgentSession> GetSessionAsync(
        AIAgent agent,
        string conversationId,
        CancellationToken cancellationToken = default)
    {
        var conversation = await GetAuthorizedConversationAsync(conversationId, cancellationToken);
        if (string.IsNullOrWhiteSpace(conversation.AgentSessionJson))
            return await agent.CreateSessionAsync(cancellationToken);

        try
        {
            using var document = JsonDocument.Parse(conversation.AgentSessionJson);
            return await agent.DeserializeSessionAsync(
                document.RootElement,
                cancellationToken: cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            GetServices()
                .GetRequiredService<ILogger<SaveLifeMafSessionStore>>()
                .LogWarning(
                    exception,
                    "Invalid or incompatible SaveLife MAF session was discarded. ConversationId={ConversationId}",
                    conversation.Id);
            return await agent.CreateSessionAsync(cancellationToken);
        }
    }

    public override async ValueTask SaveSessionAsync(
        AIAgent agent,
        string conversationId,
        AgentSession session,
        CancellationToken cancellationToken = default)
    {
        var conversation = await GetAuthorizedConversationAsync(conversationId, cancellationToken);
        var serialized = await agent.SerializeSessionAsync(
            session,
            cancellationToken: cancellationToken);
        var externalUserId = GetExternalUserId();
        var services = GetServices();
        var expectedPatientContextVersion = services
            .GetService<SaveLifeMafRequestContext>()?
            .ExpectedPatientContextVersion;
        if (expectedPatientContextVersion.HasValue &&
            conversation.PatientContextVersion != expectedPatientContextVersion.Value)
        {
            services.GetRequiredService<ILogger<SaveLifeMafSessionStore>>().LogInformation(
                "Discarded stale SaveLife MAF session after patient context changed. ConversationId={ConversationId}",
                conversation.Id);
            return;
        }

        var repository = services
            .GetRequiredService<SqlSugarRepository<SaveLifeAgentConversation>>();
        var serializedJson = serialized.GetRawText();
        var updateTime = DateTime.Now;
        var updateable = repository.Context.Updateable<SaveLifeAgentConversation>()
            .SetColumns(item => new SaveLifeAgentConversation
            {
                AgentSessionJson = serializedJson,
                UpdateTime = updateTime
            })
            .Where(item =>
                item.Id == conversation.Id &&
                item.ExternalUserId == externalUserId &&
                item.Status == "active" &&
                !item.IsDelete);
        if (expectedPatientContextVersion.HasValue)
        {
            var patientContextVersion = expectedPatientContextVersion.Value;
            updateable = updateable.Where(item =>
                item.PatientContextVersion == patientContextVersion);
        }

        var updated = await updateable.ExecuteCommandAsync(cancellationToken);
        if (updated == 0 && expectedPatientContextVersion.HasValue)
        {
            services.GetRequiredService<ILogger<SaveLifeMafSessionStore>>().LogInformation(
                "Discarded stale SaveLife MAF session during concurrent conversation update. ConversationId={ConversationId}",
                conversation.Id);
            return;
        }
        if (updated != 1)
            throw Oops.Oh("会话状态已变化，请重试").StatusCode(409);
    }

    private async Task<SaveLifeAgentConversation> GetAuthorizedConversationAsync(
        string conversationId,
        CancellationToken cancellationToken)
    {
        if (!long.TryParse(conversationId, out var id) || id <= 0)
            throw Oops.Oh("会话Id格式不正确").StatusCode(400);

        var externalUserId = GetExternalUserId();
        var repository = GetServices()
            .GetRequiredService<SqlSugarRepository<SaveLifeAgentConversation>>();
        var conversation = await repository.AsQueryable()
            .Where(item =>
                item.Id == id &&
                item.ExternalUserId == externalUserId &&
                item.Status == "active" &&
                !item.IsDelete)
            .FirstAsync(cancellationToken);
        return conversation
            ?? throw Oops.Oh("会话不存在或已关闭").StatusCode(404);
    }

    private IServiceProvider GetServices()
    {
        return _httpContextAccessor.HttpContext?.RequestServices
            ?? throw new InvalidOperationException("SaveLife MAF session requires an active HTTP request.");
    }

    private string GetExternalUserId()
    {
        var context = _httpContextAccessor.HttpContext
            ?? throw Oops.Oh("登录态无效，请重新登录").StatusCode(401);
        return SaveLifeAgentSimulationIdentity.ResolveExternalUserId(context);
    }
}

public sealed record SaveLifeMafRequestEnvelope(
    string ThreadId,
    string RunId,
    SaveLifeMafMessageEnvelope LatestUserMessage,
    string? BusinessContextJson)
{
    private const string ForwardedPropertiesKey = "saveLife";
    private const string BusinessContextDescription = "SaveLife business context";

    public static SaveLifeMafRequestEnvelope FromChatOptions(
        ChatOptions options,
        IEnumerable<ChatMessage> messages)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(messages);

        var latestUserMessage = messages.LastOrDefault(message =>
            message.Role == ChatRole.User && !string.IsNullOrWhiteSpace(message.Text));
        if (latestUserMessage == null)
            throw Oops.Oh("消息不能为空").StatusCode(400);

        if (!options.TryGetRunAgentInput(out var input))
            throw Oops.Oh("AG-UI 请求属性不能为空").StatusCode(400);

        var businessContextJson = input.Context?
            .Where(item => string.Equals(item.Description?.Trim(), BusinessContextDescription, StringComparison.Ordinal))
            .Select(item => item.Value?.Trim())
            .FirstOrDefault(value => !string.IsNullOrEmpty(value));

        IReadOnlyList<string> materialIds = [];
        var attachmentUnavailable = false;
        if (input.ForwardedProperties is { } root)
        {
            if (root.ValueKind == JsonValueKind.Object &&
                root.TryGetProperty(ForwardedPropertiesKey, out var forwarded) &&
                forwarded.ValueKind == JsonValueKind.Object)
            {
                attachmentUnavailable =
                    forwarded.TryGetProperty("attachmentUnavailable", out var unavailable) &&
                    unavailable.ValueKind == JsonValueKind.True;
                if (forwarded.TryGetProperty("materialIds", out var ids) &&
                    ids.ValueKind == JsonValueKind.Array)
                {
                    materialIds = ids.EnumerateArray()
                        .Where(item => item.ValueKind == JsonValueKind.String)
                        .Select(item => item.GetString()?.Trim() ?? string.Empty)
                        .Where(item => item.Length > 0)
                        .Distinct(StringComparer.Ordinal)
                        .ToList();
                }
            }
        }

        return new SaveLifeMafRequestEnvelope(
            input.ThreadId?.Trim() ?? string.Empty,
            input.RunId?.Trim() ?? string.Empty,
            new SaveLifeMafMessageEnvelope(
                latestUserMessage.MessageId?.Trim() ?? string.Empty,
                latestUserMessage.Text?.Trim() ?? string.Empty,
                materialIds,
                attachmentUnavailable),
            businessContextJson);
    }
}

public sealed record SaveLifeMafMessageEnvelope(
    string Id,
    string Content,
    IReadOnlyList<string> MaterialIds,
    bool AttachmentUnavailable);

internal sealed class SaveLifeMafBusinessContextInput
{
    public string? ContextType { get; set; }

    public string? ContextId { get; set; }

    public string? TrialId { get; set; }

    public string? KnowledgeId { get; set; }

    public string? EnrollmentId { get; set; }

    public string? PatientProfileId { get; set; }
}
