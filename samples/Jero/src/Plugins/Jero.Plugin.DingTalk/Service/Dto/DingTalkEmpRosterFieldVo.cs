namespace Jero.Plugin.DingTalk;

public class DingTalkEmpRosterFieldVo
{
    /// <summary>
    /// 企业的corpid
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("corp_id")]
    public string CorpId { get; set; } = string.Empty;

    /// <summary>
    /// 返回的字段信息列表
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("field_data_list")]
    public List<DingTalkEmpFieldDataVo> FieldDataList { get; set; } = [];

    /// <summary>
    /// 员工的userid
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("userid")]
    public string UserId { get; set; } = string.Empty;
}
