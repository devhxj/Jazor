namespace Jero.Core.Service;

/// <summary>
/// 自定义模板引擎
/// </summary>
public class CustomViewEngine : ViewEngineModel
{
    /// <summary>
    /// 库定位器
    /// </summary>
    public string ConfigId { get; set; } = SqlSugarConst.MainConfigId;

    public string AuthorName { get; set; } = string.Empty;
    public string BusName { get; set; } = string.Empty;
    public string NameSpace { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public string LowerClassName { get; set; } = string.Empty;
    public string ProjectLastName { get; set; } = string.Empty;
    public string PagePath { get; set; } = "main";
    public string FrontendNamespace => "Jero.Web.App." + SysCodeGenService.GeneratedComponent(PagePath, ClassName)[1..^"/Index".Length].Replace('/', '.');
    public string RuntimeTransferModule => "generated" + SysCodeGenService.GeneratedComponent(PagePath, ClassName)["/Generated".Length..^"/Index".Length] + "/file-transfer.js";

    public string PrintType { get; set; } = string.Empty;
    public string PrintName { get; set; } = string.Empty;
    public bool HasLikeQuery { get; set; }

    public bool HasJoinTable { get; set; }

    public bool HasEnumField { get; set; }

    public bool HasDictField { get; set; }

    public bool HasConstField { get; set; }

    public bool HasSetStatus => TableField.Any(IsStatus);
    public bool HasFileTransfer => UploadFieldList.Count > 0 || ImportFieldList.Count > 0;
    public bool HasCustomPrint => PrintType == "custom";

    public List<CodeGenConfig> TableField { get; set; } = [];
    public List<CodeGenConfig> ImportFieldList { get; set; } = [];
    public List<CodeGenConfig> UploadFieldList { get; set; } = [];
    public List<CodeGenConfig> QueryWhetherList { get; set; } = [];
    public List<CodeGenConfig> ApiTreeFieldList { get; set; } = [];
    public List<CodeGenConfig> DropdownFieldList { get; set; } = [];
    public List<CodeGenConfig> AddUpdateFieldList { get; set; } = [];
    public List<CodeGenConfig> PrimaryKeyFieldList { get; set; } = [];
    public List<TableUniqueConfigItem> TableUniqueConfigList { get; set; } = [];
    public List<CodeGenConfig> IgnoreUpdateFieldList => TableField.Where(u => u.WhetherAddUpdate == "N" && u.ColumnKey != "True" && u.WhetherCommon != "Y").ToList();

    /// <summary>
    /// 格式化主键查询条件
    /// 例： PrimaryKeysFormat(" || ", "u.{0} == input.{0}")
    /// 单主键返回 u.Id == input.Id
    /// 组合主键返回 u.Id == input.Id || u.FkId == input.FkId
    /// </summary>
    /// <param name="separator">分隔符</param>
    /// <param name="format">模板字符串</param>
    /// <param name="lowerFirstLetter">字段首字母小写</param>
    /// <returns></returns>
    public string PrimaryKeysFormat(string separator, string format, bool lowerFirstLetter = false) => string.Join(separator, PrimaryKeyFieldList.Select(u => string.Format(format, lowerFirstLetter ? u.LowerPropertyName : u.PropertyName)));

    /// <summary>
    /// 注入的服务
    /// </summary>
    /// <returns></returns>
    public Dictionary<string, string> InjectServiceMap
    {
        get
        {
            var injectMap = new Dictionary<string, string>();
            if (UploadFieldList.Count > 0) injectMap.Add(nameof(SysFileService), ToLowerFirstLetter(nameof(SysFileService)));
            if (DropdownFieldList.Count > 0 || ImportFieldList.Count > 0) injectMap.Add(nameof(ISqlSugarClient), ToLowerFirstLetter(nameof(ISqlSugarClient).TrimStart('I')));
            if (ImportFieldList.Any(c => c.EffectType == "DictSelector")) injectMap.Add(nameof(SysDictTypeService), ToLowerFirstLetter(nameof(SysDictTypeService)));
            return injectMap;
        }
    }

    /// <summary>
    /// 服务构造参数
    /// </summary>
    public string InjectServiceArgs => InjectServiceMap.Count > 0 ? ", " + string.Join(", ", InjectServiceMap.Select(kv => $"{kv.Key} {kv.Value}")) : "";

    /// <summary>
    /// 默认值列表
    /// </summary>
    /// <summary>
    /// 判断字段是否为状态字段
    /// </summary>
    /// <param name="column"></param>
    /// <returns></returns>
    public bool IsStatus(CodeGenConfig column) => column.NetType == nameof(StatusEnum);

    /// <summary>
    /// 获取首字母小写字符串
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public string ToLowerFirstLetter(string text) => string.IsNullOrWhiteSpace(text) ? text : text[..1].ToLower() + text[1..];

    /// <summary>
    /// 将基本字段类型转为可空类型
    /// </summary>
    /// <param name="netType"></param>
    /// <returns></returns>
    public string GetNullableNetType(string netType) => Regex.IsMatch(netType, "(.*?Enum|bool|char|int|long|double|float|decimal)[?]?") ? netType.TrimEnd('?') + "?" : netType;

    public string FrontendType(CodeGenConfig column) => column.NetType.TrimEnd('?') switch
    {
        "bool" => "bool?",
        "byte" or "sbyte" or "short" or "ushort" or "int" or "uint" or "double" or "float" or "decimal" => "double?",
        _ when column.NetType.TrimEnd('?').EndsWith("Enum", StringComparison.Ordinal) => "double?",
        // The Jero JSON protocol serializes Int64 values as strings to retain all digits.
        _ => "string?"
    };

    public string? FrontendDefault(CodeGenConfig column)
    {
        var value = column.DefaultValue.Trim().Trim('(', ')', '\'');
        if (value.Length == 0)
            return IsStatus(column) ? "1" : column.PropertyName == nameof(SysUser.OrderNo) ? "100" : null;

        return FrontendType(column) switch
        {
            "bool?" => value is "1" or "true" ? "true" : "false",
            "double?" => double.TryParse(value, System.Globalization.CultureInfo.InvariantCulture, out var number) && double.IsFinite(number)
                ? number.ToString("R", System.Globalization.CultureInfo.InvariantCulture) : null,
            _ when column.EffectType == "DatePicker" => null,
            _ => SourceString(value)
        };
    }

    public string SourceString(string value) => System.Text.Json.JsonSerializer.Serialize(value);

    public string RowKeyParameters => string.Join(", ", PrimaryKeyFieldList.Select(column => $"{FrontendType(column)} @{column.LowerPropertyName}"));

    public bool IsFrontendSelector(CodeGenConfig column) =>
        column.IsSelectorEffectType || column.EffectType == "ForeignKey";

    public string SelectorEndpoint(CodeGenConfig column) => column.EffectType switch
    {
        "DictSelector" => "/api/sysDictData/dataList?value=" + Uri.EscapeDataString(column.DictTypeCode),
        "EnumSelector" => "/api/sysEnum/enumDataList?enumName=" + Uri.EscapeDataString(column.DictTypeCode),
        "ConstSelector" => "/api/sysConst/data?typeName=" + Uri.EscapeDataString(column.DictTypeCode),
        _ => throw new InvalidOperationException(column.EffectType)
    };

    public string SelectorAssignment(CodeGenConfig column) => FrontendType(column) switch
    {
        "double?" => "value == \"\" ? (double?)null : (double)Global.NumberValue(value)",
        "bool?" => "value == \"\" ? (bool?)null : value == \"true\"",
        _ => "value == \"\" ? null : value"
    };
}
