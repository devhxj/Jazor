namespace Jero.Application.Modules.DingTalk;

public class DingTalkCreateAndDeliverOutput
{
    /// <summary>
    /// 返回结果
    /// </summary>
    public bool success { get; set; }

    /// <summary>
    /// 创建卡片结果
    /// </summary>
    public DingTalkCreateAndDeliverResult result { get; set; } = new();

    public string code { get; set; } = string.Empty;
    public string requestid { get; set; } = string.Empty;
    public string message { get; set; } = string.Empty;
}

public class DingTalkCreateAndDeliverResult
{
    /// <summary>
    /// 用于业务方后续查看已读列表的查询key
    /// </summary>
    public string processQueryKey { get; set; } = string.Empty;
}
