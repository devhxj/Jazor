namespace Jero.Plugin.DingTalk;

public class DingTalkDeptOutput
{
    /// <summary>
    /// 上级部门Id
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("parent_id")]
    public long parent_id { get; set; }

    /// <summary>
    /// 部门名
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("name")]
    public string name { get; set; } = string.Empty;

    /// <summary>
    /// 部门Id
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("dept_id")]
    public long dept_id { get; set; }
}
