namespace Jero.Plugin.DingTalk;

[SugarTable("ding_talk_wokerflow_log", "钉钉审批日志")]
public class DingTalkWokerflowLog
{
    /// <summary>
    /// 审批实例ID
    /// </summary>
    [SugarColumn(ColumnDescription = "审批实例ID", IsPrimaryKey = true, IsIdentity = false)]
    public string instanceId { get; set; } = string.Empty;

    /// <summary>
    /// 审批单号
    /// </summary>
    [SugarColumn(ColumnDescription = "审批单号")]
    public string? WorkflowId { get; set; }

    /// <summary>
    /// 来源单据
    /// </summary>
    [SugarColumn(ColumnDescription = "来源单据")]
    public string SourceDocument { get; set; } = string.Empty;

    /// <summary>
    /// 审批完成时间
    /// </summary>
    [SugarColumn(ColumnDescription = "审批完成时间")]
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// 其他信息
    /// </summary>
    [SugarColumn(ColumnDescription = "其他信息", IsJson = true)]
    public Dictionary<string, object>? other_info { get; set; }

    /// <summary>
    /// 是否回传结果给第三方
    /// </summary>
    [SugarColumn(ColumnDescription = "是否回传结果")]
    public bool? isReturn { get; set; }

    /// <summary>
    /// 审批状态
    /// </summary>
    /// <remarks>
    /// RUNNING：审批中 TERMINATED：已撤销 COMPLETED：审批完成
    /// /// </remarks>
    [SugarColumn(ColumnDescription = "审批状态")]
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// 任务ID
    /// </summary>
    [SugarColumn(ColumnDescription = "任务ID")]
    public long? taskId { get; set; }

    /// <summary>
    /// 审批结果 agree：同意 refuse：拒绝
    /// </summary>
    [SugarColumn(ColumnDescription = "审批结果")]
    public string? Result { get; set; }

    /// <summary>
    /// 创建者姓名
    /// </summary>
    [SugarColumn(ColumnDescription = "创建者姓名", Length = 64, IsOnlyIgnoreUpdate = true)]
    public string? CreateUserName { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [SugarColumn(ColumnDescription = "创建时间", IsNullable = true, IsOnlyIgnoreUpdate = true)]
    public DateTime CreateTime { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    [SugarColumn(ColumnDescription = "更新时间")]
    public virtual DateTime? UpdateTime { get; set; }
}
