namespace Jero.Plugin.WorkWeixin.Proxy;

/// <summary>
/// 创建部门输入参数
/// </summary>
public class DepartmentHttpInput
{
    /// <summary>
    /// 部门名称
    /// </summary>
    [JsonPropertyName("id")]
    public long? Id { get; set; }

    /// <summary>
    /// 父部门id
    /// </summary>
    [JsonPropertyName("parentid")]
    public long? ParentId { get; set; }

    /// <summary>
    /// 部门名称
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 英文名称
    /// </summary>
    [JsonPropertyName("name_en")]
    public string NameEn { get; set; } = string.Empty;

    /// <summary>
    /// 序号
    /// </summary>
    [JsonPropertyName("order")]
    public int? Order { get; set; }
}
