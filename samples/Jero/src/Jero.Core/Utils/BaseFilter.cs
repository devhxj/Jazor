namespace Jero.Core;

/// <summary>
/// 模糊查询条件
/// </summary>
public class Search
{
    /// <summary>
    /// 字段名称集合
    /// </summary>
    public List<string> Fields { get; set; } = [];
    /// <summary>
    /// 关键字
    /// </summary>
    public string? Keyword { get; set; }
}

/// <summary>
/// 筛选过滤条件
/// </summary>
public class Filter
{
    /// <summary>
    /// 过滤条件
    /// </summary>
    public FilterLogicEnum? Logic { get; set; }

    /// <summary>
    /// 筛选过滤条件子项
    /// </summary>
    public IEnumerable<Filter>? Filters { get; set; }

    /// <summary>
    /// 字段名称
    /// </summary>
    public string? Field { get; set; }

    /// <summary>
    /// 逻辑运算符
    /// </summary>
    public FilterOperatorEnum? Operator { get; set; }

    /// <summary>
    /// 字段值
    /// </summary>
    public object? Value { get; set; }
}

/// <summary>
/// 过滤条件基类
/// </summary>
public abstract class BaseFilter
{
    /// <summary>
    /// 模糊查询条件
    /// </summary>
    public Search? Search { get; set; }

    /// <summary>
    /// 模糊查询关键字
    /// </summary>
    public string? Keyword { get; set; }

    /// <summary>
    /// 筛选过滤条件
    /// </summary>
    public Filter? Filter { get; set; }
}