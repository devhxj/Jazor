namespace Jero.Plugin.SaveLife;

/// <summary>
/// 医线生机助手可重放业务命令表
/// </summary>
[SugarTable("sl_agent_business_command", "医线生机助手可重放业务命令表")]
[SugarIndex("index_{table}_C", nameof(ConversationId), OrderByType.Asc)]
[SugarIndex("index_{table}_S", nameof(Status), OrderByType.Asc)]
public class SaveLifeAgentBusinessCommand : SaveLifeEntityBaseDel
{
    [Required, MaxLength(64)]
    [SugarColumn(ColumnDescription = "外部用户 Id", Length = 64)]
    public string ExternalUserId { get; set; } = string.Empty;

    [SugarColumn(ColumnDescription = "会话 Id")]
    public long ConversationId { get; set; }

    [Required, MaxLength(32)]
    [SugarColumn(ColumnDescription = "命令类型", Length = 32)]
    public string CommandType { get; set; } = string.Empty;

    [Required, MaxLength(128)]
    [SugarColumn(ColumnDescription = "客户端命令 Id", Length = 128)]
    public string ClientCommandId { get; set; } = string.Empty;

    [Required, MaxLength(32)]
    [SugarColumn(ColumnDescription = "载荷摘要算法", Length = 32)]
    public string PayloadHashAlgorithm { get; set; } = string.Empty;

    [Required, MaxLength(64)]
    [SugarColumn(ColumnDescription = "载荷摘要", Length = 64)]
    public string PayloadHash { get; set; } = string.Empty;

    [Required, MaxLength(32)]
    [SugarColumn(ColumnDescription = "命令状态", Length = 32)]
    public string Status { get; set; } = string.Empty;

    [SugarColumn(
        ColumnDescription = "首次执行结果 JSON",
        ColumnDataType = StaticConfig.CodeFirst_BigString,
        IsNullable = true)]
    public string? ResultJson { get; set; }

    [SugarColumn(ColumnDescription = "用户确认消息 Id", IsNullable = true)]
    public long? UserMessageId { get; set; }

    [SugarColumn(ColumnDescription = "助手结果消息 Id", IsNullable = true)]
    public long? AssistantMessageId { get; set; }

    [SugarColumn(ColumnDescription = "报名记录 Id", IsNullable = true)]
    public long? EnrollmentRecordId { get; set; }

    [SugarColumn(ColumnDescription = "执行时患者上下文版本")]
    public long PatientContextVersion { get; set; }
}
