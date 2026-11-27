namespace Jero.Application.Modules.DingTalk;

public class DingTalkFieldValueVo
{
    /// <summary>
    /// 第几条的明细标识，下标从0开始
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("item_index")]
    public int ItemIndex { get; set; }

    /// <summary>
    /// 字段展示值，选项类型字段对应选项的value
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// 字段取值，选项类型字段对应选项的key
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;
}
