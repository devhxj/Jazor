namespace Jero.Plugin.SaveLife;

/// <summary>
/// 医线生机小程序助手消息表
/// </summary>
[SugarTable("sl_agent_message", "医线生机小程序助手消息表")]
[SugarIndex("index_{table}_C", nameof(ConversationId), OrderByType.Asc)]
[SugarIndex("uq_{table}_CM", nameof(ConversationId), OrderByType.Asc, nameof(ClientMessageId), OrderByType.Asc, true)]
[SugarIndex("uq_{table}_RM", nameof(ReplyToMessageId), OrderByType.Asc, true)]
public class SaveLifeAgentMessage : SaveLifeEntityBaseDel
{
    /// <summary>
    /// 会话 Id
    /// </summary>
    [SugarColumn(ColumnDescription = "会话 Id")]
    public long ConversationId { get; set; }

    /// <summary>
    /// 消息生成时对应的患者档案 Id
    /// </summary>
    [SugarColumn(ColumnDescription = "患者档案 Id", IsNullable = true)]
    public long? PatientProfileId { get; set; }

    /// <summary>
    /// 消息接收时的患者上下文版本
    /// </summary>
    [SugarColumn(ColumnDescription = "患者上下文版本", DefaultValue = "1")]
    public long PatientContextVersion { get; set; } = 1;

    /// <summary>
    /// 会话内用户消息轮次序号
    /// </summary>
    [SugarColumn(ColumnDescription = "轮次序号", IsNullable = true)]
    public long? TurnSequence { get; set; }

    /// <summary>
    /// 用户轮次状态 pending/processing/completed/failed/superseded
    /// </summary>
    [MaxLength(32)]
    [SugarColumn(ColumnDescription = "轮次状态", Length = 32, IsNullable = true)]
    public string? TurnStatus { get; set; }

    /// <summary>
    /// 取得生成租约时的会话状态修订号
    /// </summary>
    [SugarColumn(ColumnDescription = "轮次状态修订号", IsNullable = true)]
    public long? TurnStateRevision { get; set; }

    /// <summary>
    /// 自动恢复所引用的来源用户消息 Id
    /// </summary>
    [SugarColumn(ColumnDescription = "上下文来源消息 Id", IsNullable = true)]
    public long? ContextSourceMessageId { get; set; }

    /// <summary>
    /// 外部用户 Id
    /// </summary>
    [Required, MaxLength(64)]
    [SugarColumn(ColumnDescription = "外部用户 Id", Length = 64)]
    public string ExternalUserId { get; set; } = string.Empty;

    /// <summary>
    /// 角色 user/assistant/system
    /// </summary>
    [Required, MaxLength(32)]
    [SugarColumn(ColumnDescription = "角色", Length = 32)]
    public string Role { get; set; } = "user";

    /// <summary>
    /// 消息内容
    /// </summary>
    [Required]
    [SugarColumn(ColumnDescription = "消息内容", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 客户端消息幂等 Id，仅用户消息使用
    /// </summary>
    [MaxLength(64)]
    [SugarColumn(ColumnDescription = "客户端消息幂等 Id", Length = 64, IsNullable = true)]
    public string? ClientMessageId { get; set; }

    /// <summary>
    /// 回复对应的用户消息 Id，仅助手消息使用
    /// </summary>
    [SugarColumn(ColumnDescription = "回复对应的用户消息 Id", IsNullable = true)]
    public long? ReplyToMessageId { get; set; }

    /// <summary>
    /// 回复生成状态 processing/completed/failed
    /// </summary>
    [MaxLength(32)]
    [SugarColumn(ColumnDescription = "回复生成状态", Length = 32, IsNullable = true)]
    public string? GenerationStatus { get; set; }

    /// <summary>
    /// 回复生成开始时间
    /// </summary>
    [SugarColumn(ColumnDescription = "回复生成开始时间", IsNullable = true)]
    public DateTime? GenerationStartedTime { get; set; }

    /// <summary>
    /// 助手回复生成租约所有者
    /// </summary>
    [MaxLength(64)]
    [SugarColumn(ColumnDescription = "回复生成租约所有者", Length = 64, IsNullable = true)]
    public string? GenerationLeaseOwner { get; set; }

    /// <summary>
    /// 助手回复生成租约过期时间
    /// </summary>
    [SugarColumn(ColumnDescription = "回复生成租约过期时间", IsNullable = true)]
    public DateTime? GenerationLeaseExpiresTime { get; set; }

    /// <summary>
    /// 消息元数据 JSON
    /// </summary>
    [SugarColumn(ColumnDescription = "消息元数据 JSON", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? MetadataJson { get; set; }
}

public static class SaveLifeAgentTurnStatuses
{
    public const string Pending = "pending";
    public const string Processing = "processing";
    public const string Completed = "completed";
    public const string Failed = "failed";
    public const string Superseded = "superseded";
}
