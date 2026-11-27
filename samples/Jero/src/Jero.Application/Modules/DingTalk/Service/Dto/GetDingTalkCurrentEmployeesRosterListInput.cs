namespace Jero.Application.Modules.DingTalk;

public class GetDingTalkCurrentEmployeesRosterListInput
{
    /// <summary>
    /// 员工的userId列表，多个userid之间使用逗号分隔，一次最多支持传100个值。
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("userid_list")]
    public string UserIdList { get; set; } = string.Empty;

    /// <summary>
    /// 需要获取的花名册字段field_code值列表，多个字段之间使用逗号分隔，一次最多支持传100个值。
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("field_filter_list")]
    public string FieldFilterList { get; set; } = string.Empty;

    /// <summary>
    /// 应用的AgentId
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("agentid")]
    public string AgentId { get; set; } = string.Empty;
}
