namespace Jero.Plugin.DingTalk;

public class DingTalkWorkflowProcessInstancesOutput
{
    /// <summary>
    /// 请求Id
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("request_id")]
    public string RequestId { get; set; } = string.Empty;

    public string code { get; set; } = string.Empty;
    public string message { get; set; } = string.Empty;

    /// <summary>
    /// 是否还有更多数据
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("instanceId")]
    public string instanceId { get; set; } = string.Empty;
}
