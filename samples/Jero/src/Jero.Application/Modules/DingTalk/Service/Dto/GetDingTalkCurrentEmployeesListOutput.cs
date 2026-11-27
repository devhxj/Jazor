namespace Jero.Application.Modules.DingTalk;

public class GetDingTalkCurrentEmployeesListOutput
{
    /// <summary>
    /// 查询到的员工userId列表
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("data_list")]
    public List<string> DataList { get; set; } = [];

    /// <summary>
    /// 下一次分页调用的offset值，当返回结果里没有next_cursor时，表示分页结束。
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("next_cursor")]
    public int? NextCursor { get; set; }
}
