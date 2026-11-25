using System.Text.Json;

namespace Jero.Plugin.SaveLife;

/// <summary>
/// 医线生机小程序助手会话表
/// </summary>
[SugarTable("sl_agent_conversation", "医线生机小程序助手会话表")]
[SugarIndex("index_{table}_E", nameof(ExternalUserId), OrderByType.Asc)]
[SugarIndex("index_{table}_S", nameof(Status), OrderByType.Asc)]
public class SaveLifeAgentConversation : SaveLifeEntityBaseDel
{
    /// <summary>
    /// 外部用户 Id
    /// </summary>
    [Required, MaxLength(64)]
    [SugarColumn(ColumnDescription = "外部用户 Id", Length = 64)]
    public string ExternalUserId { get; set; } = string.Empty;

    /// <summary>
    /// 当前患者档案 Id
    /// </summary>
    [SugarColumn(ColumnDescription = "当前患者档案 Id", IsNullable = true)]
    public long? CurrentPatientProfileId { get; set; }

    /// <summary>
    /// 当前患者上下文版本
    /// </summary>
    [SugarColumn(ColumnDescription = "患者上下文版本", DefaultValue = "1")]
    public long PatientContextVersion { get; set; } = 1;

    /// <summary>
    /// 下一个待分配的用户消息轮次序号
    /// </summary>
    [SugarColumn(ColumnDescription = "下一个轮次序号", DefaultValue = "1")]
    public long NextTurnSequence { get; set; } = 1;

    /// <summary>
    /// 会话业务状态修订号
    /// </summary>
    [SugarColumn(ColumnDescription = "会话状态修订号", DefaultValue = "0")]
    public long StateRevision { get; set; }

    /// <summary>
    /// 当前生成租约所有者
    /// </summary>
    [MaxLength(64)]
    [SugarColumn(ColumnDescription = "生成租约所有者", Length = 64, IsNullable = true)]
    public string? GenerationLeaseOwner { get; set; }

    /// <summary>
    /// 当前生成租约对应的轮次
    /// </summary>
    [SugarColumn(ColumnDescription = "生成租约轮次", IsNullable = true)]
    public long? GenerationLeaseTurnSequence { get; set; }

    /// <summary>
    /// 当前生成租约过期时间
    /// </summary>
    [SugarColumn(ColumnDescription = "生成租约过期时间", IsNullable = true)]
    public DateTime? GenerationLeaseExpiresTime { get; set; }

    /// <summary>
    /// 结构化流程状态版本
    /// </summary>
    [SugarColumn(ColumnDescription = "结构化流程状态版本", IsNullable = true)]
    public int? FlowStateVersion { get; set; }

    /// <summary>
    /// 结构化流程状态 JSON
    /// </summary>
    [SugarColumn(
        ColumnDescription = "结构化流程状态 JSON",
        ColumnDataType = StaticConfig.CodeFirst_BigString,
        IsNullable = true)]
    public string? FlowStateJson { get; set; }

    /// <summary>
    /// Microsoft Agent Framework 会话状态 JSON。
    /// </summary>
    [SugarColumn(
        ColumnDescription = "MAF 会话状态 JSON",
        ColumnDataType = StaticConfig.CodeFirst_BigString,
        IsNullable = true)]
    public string? AgentSessionJson { get; set; }

    /// <summary>
    /// 会话状态
    /// </summary>
    [Required, MaxLength(32)]
    [SugarColumn(ColumnDescription = "会话状态", Length = 32)]
    public string Status { get; set; } = "active";

    /// <summary>
    /// 当前阶段
    /// </summary>
    [MaxLength(32)]
    [SugarColumn(ColumnDescription = "当前阶段", Length = 32)]
    public string? Phase { get; set; }

    /// <summary>
    /// 匹配项目 JSON
    /// </summary>
    [SugarColumn(ColumnDescription = "匹配项目 JSON", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? MatchedTrialsJson { get; set; }

    /// <summary>
    /// 当前业务上下文类型 trial/knowledge/enrollment
    /// </summary>
    [MaxLength(32)]
    [SugarColumn(ColumnDescription = "当前业务上下文类型", Length = 32)]
    public string? ContextType { get; set; }

    /// <summary>
    /// 当前业务上下文 Id
    /// </summary>
    [SugarColumn(ColumnDescription = "当前业务上下文 Id")]
    public long? ContextId { get; set; }

    /// <summary>
    /// 已提交报名记录 Id
    /// </summary>
    [SugarColumn(ColumnDescription = "已提交报名记录 Id")]
    public long? EnrollmentRecordId { get; set; }

    /// <summary>
    /// 最近消息摘要
    /// </summary>
    [MaxLength(512)]
    [SugarColumn(ColumnDescription = "最近消息摘要", Length = 512)]
    public string? LastMessage { get; set; }
}

public sealed class SaveLifeAgentConversationState
{
    public const int CurrentVersion = 2;

    public int Version { get; set; } = CurrentVersion;

    public SaveLifeAgentPendingQuestionState? PendingQuestion { get; set; }

    public SaveLifeAgentPendingConsultationState? PendingConsultation { get; set; }

    public bool IsGeneralGuidanceDeferred { get; set; }

    public bool GeneralTrialOptOut { get; set; }

    public List<long> TrialOptOutPatientProfileIds { get; set; } = [];

    public bool TrialOptOut { get; set; }

    public long? SelectedTrialId { get; set; }

    public long? PendingPatientSwitchProfileId { get; set; }
}

public sealed class SaveLifeAgentPendingQuestionState
{
    public string Key { get; set; } = string.Empty;

    public string Prompt { get; set; } = string.Empty;
}

public sealed class SaveLifeAgentPendingConsultationState
{
    public long Id { get; set; }

    public long SourceUserMessageId { get; set; }

    public string GoalType { get; set; } = string.Empty;

    public string SubjectHint { get; set; } = SaveLifeAgentSubjectScopes.Unknown;

    public DateTime CreatedTime { get; set; }

    public string Status { get; set; } = SaveLifeAgentPendingConsultationStatuses.Pending;
}

public static class SaveLifeAgentPendingConsultationGoals
{
    public const string IllnessHelp = "illness_help";
}

public static class SaveLifeAgentPendingConsultationStatuses
{
    public const string Pending = "pending";
    public const string Consumed = "consumed";
    public const string Cancelled = "cancelled";
    public const string Superseded = "superseded";
}

public static class SaveLifeAgentSubjectScopes
{
    public const string General = "general";
    public const string CurrentPatient = "current_patient";
    public const string Self = "self";
    public const string Other = "other";
    public const string Unknown = "unknown";

    internal static readonly HashSet<string> All =
    [
        General,
        CurrentPatient,
        Self,
        Other,
        Unknown
    ];
}

public static class SaveLifeAgentConversationStateCodec
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public static string Serialize(SaveLifeAgentConversationState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        state.Version = SaveLifeAgentConversationState.CurrentVersion;
        return JsonSerializer.Serialize(state, JsonOptions);
    }

    public static SaveLifeAgentConversationState Deserialize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return new SaveLifeAgentConversationState();

        var state = JsonSerializer.Deserialize<SaveLifeAgentConversationState>(value, JsonOptions)
            ?? new SaveLifeAgentConversationState();
        if (state.Version <= 0)
            state.Version = SaveLifeAgentConversationState.CurrentVersion;
        if (state.Version > SaveLifeAgentConversationState.CurrentVersion)
            throw new NotSupportedException($"不支持的助手会话状态版本：{state.Version}");

        state.TrialOptOutPatientProfileIds ??= [];
        state.Version = SaveLifeAgentConversationState.CurrentVersion;

        return state;
    }
}
