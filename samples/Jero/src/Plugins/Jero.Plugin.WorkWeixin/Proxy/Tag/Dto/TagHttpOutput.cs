namespace Jero.Plugin.WorkWeixin.Proxy;

/// <summary>
/// 新增标签输出参数
/// </summary>
public class TagIdHttpOutput : BaseWorkOutput
{
    /// <summary>
    /// 标签Id
    /// </summary>
    [JsonPropertyName("tagid")]
    public long? TagId { get; set; }
}

/// <summary>
/// 标签列表输出参数
/// </summary>
public class TagListHttpOutput : BaseWorkOutput
{
    /// <summary>
    /// 标签Id
    /// </summary>
    [JsonPropertyName("taglist")]
    public List<TagHttpInput> TagList { get; set; } = [];
}
