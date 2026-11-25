namespace Jero.Plugin.WorkWeixin.Proxy;

/// <summary>
/// 部门Id列表输出参数
/// </summary>
public class DepartmentIdOutput : BaseWorkOutput
{
    /// <summary>
    /// id
    /// </summary>
    [JsonPropertyName("department_id")]
    public List<DepartmentItemOutput> DepartmentList { get; set; } = [];
}

/// <summary>
/// 部门Id输出参数
/// </summary>
public class DepartmentItemOutput
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
    /// 序号
    /// </summary>
    [JsonPropertyName("order")]
    public int? Order { get; set; }
}

/// <summary>
/// 部门输出参数
/// </summary>
public class DepartmentOutput
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
    /// 部门负责人列表
    /// </summary>
    [JsonPropertyName("department_leader")]
    public List<string> Leaders { get; set; } = [];

    /// <summary>
    /// 序号
    /// </summary>
    [JsonPropertyName("order")]
    public int? Order { get; set; }
}
