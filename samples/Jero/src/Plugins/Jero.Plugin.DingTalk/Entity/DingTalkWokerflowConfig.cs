namespace Jero.Plugin.DingTalk;

[SugarTable("ding_talk_wokerflow_config", "审批配置表")]
public class DingTalkWokerflowConfig
{
    /// <summary>
    /// 审批名
    /// </summary>
    [SugarColumn(ColumnDescription = "审批名", IsPrimaryKey = true, IsIdentity = false)]
    public string WorkflowName { get; set; } = string.Empty;

    /// <summary>
    /// 审批单Id
    /// </summary>
    [SugarColumn(ColumnDescription = "审批单Id")]
    public string ProcessCode { get; set; } = string.Empty;
}
