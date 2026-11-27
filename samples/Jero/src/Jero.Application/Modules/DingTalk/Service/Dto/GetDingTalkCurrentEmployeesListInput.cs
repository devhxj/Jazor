namespace Jero.Application.Modules.DingTalk;

/// <summary>
/// 获取在职员工列表参数
/// </summary>
public class GetDingTalkCurrentEmployeesListInput
{
    /// <summary>
    /// 在职员工状态筛选，可以查询多个状态。不同状态之间使用英文逗号分隔。2：试用期、3：正式、5：待离职、-1：无状态
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("status_list")]
    public string StatusList { get; set; } = string.Empty;

    /// <summary>
    /// 分页游标，从0开始。根据返回结果里的next_cursor是否为空来判断是否还有下一页，且再次调用时offset设置成next_cursor的值。
    /// </summary>
    public int Offset { get; set; }

    /// <summary>
    /// 分页大小，最大50。
    /// </summary>
    public int Size { get; set; }
}
