namespace Jero.Plugin.DingTalk;

public class DingTalkEmpFieldDataVo
{
    /// <summary>
    /// 字段名称
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("field_name")]
    public string FieldName { get; set; } = string.Empty;

    /// <summary>
    /// 字段标识
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("field_code")]
    public string FieldCode { get; set; } = string.Empty;

    /// <summary>
    /// 分组标识
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("group_id")]
    public string GroupId { get; set; } = string.Empty;

    /// <summary>
    ///
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("field_value_list")]
    public List<DingTalkFieldValueVo> FieldValueList { get; set; } = [];
}
