using System.Text.Json.Serialization;

namespace Jero.Application.Modules.WorkWeixin.Proxy;

/// <summary>
/// 标签输入参数
/// </summary>
public class TagHttpInput
{
    /// <summary>
    /// 标签id
    /// </summary>
    [JsonPropertyName("tagid")]
    public long? TagId { get; set; }

    /// <summary>
    /// 标签名称
    /// </summary>
    [JsonPropertyName("tagname")]
    public string TagName { get; set; } = string.Empty;
}

/// <summary>
/// 增加标签成员输入参数
/// </summary>
public class TagUsersTagInput
{
    /// <summary>
    /// 标签id
    /// </summary>
    [JsonPropertyName("tagid")]
    public long TagId { get; set; }

    /// <summary>
    /// 企业成员ID列表
    /// </summary>
    [JsonPropertyName("userlist")]
    public List<string> UserList { get; set; } = [];

    /// <summary>
    /// 企业部门ID列表
    /// </summary>
    [JsonPropertyName("partylist")]
    public List<long> PartyList { get; set; } = [];
}
