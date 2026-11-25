namespace Jero.Core.Service;

/// <summary>
/// 代码生成详细配置参数
/// </summary>
public class CodeGenConfig
{
    private static string LowerFirst(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value[..1].ToLower() + value[1..];
    }

    private static string TrimSuffix(string value, string suffix)
    {
        if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(suffix))
            return value;

        return value.EndsWith(suffix, StringComparison.Ordinal)
            ? value[..^suffix.Length]
            : value;
    }

    /// <summary>
    /// 主键Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 代码生成主表ID
    /// </summary>
    public long CodeGenId { get; set; }

    /// <summary>
    /// 数据库字段名
    /// </summary>
    public string ColumnName { get; set; } = string.Empty;
    /// <summary>
    /// 主外键
    /// </summary>
    public string ColumnKey { get; set; } = string.Empty;
    /// <summary>
    /// 实体属性名
    /// </summary>
    public string PropertyName { get; set; } = string.Empty;
    /// <summary>
    /// 字段数据长度
    /// </summary>
    public int ColumnLength { get; set; }

    /// <summary>
    /// 数据库字段名(首字母小写)
    /// </summary>
    public string LowerPropertyName => LowerFirst(PropertyName);

    /// <summary>
    /// 字段描述
    /// </summary>
    public string ColumnComment { get; set; } = string.Empty;
    /// <summary>
    /// .NET类型
    /// </summary>
    public string NetType { get; set; } = string.Empty;
    /// <summary>
    /// 数据库中类型（物理类型）
    /// </summary>
    public string DataType { get; set; } = string.Empty;
    /// <summary>
    /// 字段数据默认值
    /// </summary>
    public string DefaultValue { get; set; } = string.Empty;
    /// <summary>
    /// 可空.NET类型
    /// </summary>
    public string NullableNetType
    {
        get
        {
            var netType = NetType ?? string.Empty;
            return Regex.IsMatch(netType, "(.*?Enum|bool|char|int|long|double|float|decimal)[?]?")
                ? netType.TrimEnd('?') + "?"
                : netType;
        }
    }

    /// <summary>
    /// 作用类型（字典）
    /// </summary>
    public string EffectType { get; set; } = string.Empty;
    /// <summary>
    /// 外键库标识
    /// </summary>
    public string FkConfigId { get; set; } = string.Empty;
    /// <summary>
    /// 外键实体名称
    /// </summary>
    public string FkEntityName { get; set; } = string.Empty;
    /// <summary>
    /// 外键表名称
    /// </summary>
    public string FkTableName { get; set; } = string.Empty;
    /// <summary>
    /// 外键实体名称(首字母小写)
    /// </summary>
    public string LowerFkEntityName => LowerFirst(FkEntityName);

    /// <summary>
    /// 外键链接字段
    /// </summary>
    public string FkLinkColumnName { get; set; } = string.Empty;
    /// <summary>
    /// 外键显示字段
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public string FkDisplayColumns { get; set; } = string.Empty;
    /// <summary>
    /// 外键显示字段
    /// </summary>
    public List<string> FkDisplayColumnList { get; set; } = [];
    /// <summary>
    /// 外键显示字段(首字母小写)
    /// </summary>
    public List<string> LowerFkDisplayColumnsList => FkDisplayColumnList.Select(LowerFirst).ToList();

    /// <summary>
    /// 外键显示字段.NET类型
    /// </summary>
    public string FkColumnNetType { get; set; } = string.Empty;
    /// <summary>
    /// 父级字段
    /// </summary>
    public string PidColumn { get; set; } = string.Empty;
    /// <summary>
    /// 字典code
    /// </summary>
    public string DictTypeCode { get; set; } = string.Empty;
    /// <summary>
    /// 查询方式
    /// </summary>
    public string QueryType { get; set; } = string.Empty;
    /// <summary>
    /// 是否是查询条件
    /// </summary>
    public string WhetherQuery { get; set; } = string.Empty;
    /// <summary>
    /// 列表是否缩进（字典）
    /// </summary>
    public string WhetherRetract { get; set; } = string.Empty;
    /// <summary>
    /// 是否必填（字典）
    /// </summary>
    public string WhetherRequired { get; set; } = string.Empty;
    /// <summary>
    /// 是否可排序（字典）
    /// </summary>
    public string WhetherSortable { get; set; } = string.Empty;
    /// <summary>
    /// 列表显示
    /// </summary>
    public string WhetherTable { get; set; } = string.Empty;
    /// <summary>
    /// 增改
    /// </summary>
    public string WhetherAddUpdate { get; set; } = string.Empty;
    /// <summary>
    /// 导入
    /// </summary>
    public string WhetherImport { get; set; } = string.Empty;
    /// <summary>
    /// 是否是通用字段
    /// </summary>
    public string WhetherCommon { get; set; } = string.Empty;
    /// <summary>
    /// 排序
    /// </summary>
    public int OrderNo { get; set; }

    /// <summary>
    /// 是否是选择器控件
    /// </summary>
    public bool IsSelectorEffectType => Regex.IsMatch(EffectType ?? "", "Selector$|ForeignKey", RegexOptions.IgnoreCase);

    /// <summary>
    /// 去掉尾部Id的属性名
    /// </summary>
    public string PropertyNameTrimEndId => TrimSuffix(PropertyName, "Id");

    /// <summary>
    /// 去掉尾部Id的属性名
    /// </summary>
    public string LowerPropertyNameTrimEndId => TrimSuffix(LowerPropertyName, "Id");

    /// <summary>
    /// 扩展属性名称
    /// </summary>
    public string ExtendedPropertyName => EffectType switch
    {
        "ForeignKey" => $"{TrimSuffix(PropertyName, "Id")}FkDisplayName",
        "ApiTreeSelector" => $"{TrimSuffix(PropertyName, "Id")}DisplayName",
        "DictSelector" => $"{TrimSuffix(PropertyName, "Id")}DictLabel",
        "Upload" => $"{TrimSuffix(PropertyName, "Id")}Attachment",
        "Upload_SingleFile" => $"{TrimSuffix(PropertyName, "Id")}Attachment",
        _ => PropertyName
    };

    /// <summary>
    /// 首字母小写的扩展属性名称
    /// </summary>
    public string LowerExtendedPropertyName
    {
        get
        {
            var displayPropertyName = ExtendedPropertyName;
            return LowerFirst(displayPropertyName);
        }
    }

    /// <summary>
    /// 获取外键显示值语句
    /// </summary>
    /// <param name="tableAlias">表别名</param>
    /// <param name="separator">多字段时的连接符</param>
    /// <returns></returns>
    public string GetDisplayColumn(string tableAlias, string separator = "-") => "$\"" + string.Join(separator, FkDisplayColumnList?.Select(name => $"{{{tableAlias}.{name}}}") ?? new List<string>()) + "\"";
}
