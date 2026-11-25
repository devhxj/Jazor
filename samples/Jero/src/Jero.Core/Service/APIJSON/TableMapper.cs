namespace Jero.Core.Service;

/// <summary>
/// 表名映射
/// </summary>
public class TableMapper : ITransient
{
    private readonly Dictionary<string, string> _options = new(StringComparer.OrdinalIgnoreCase);

    public TableMapper(IOptions<Dictionary<string, string>> options)
    {
        foreach (var item in options.Value)
        {
            _options.Add(item.Key, item.Value);
        }
    }

    /// <summary>
    /// 获取表别名
    /// </summary>
    /// <param name="oldname"></param>
    /// <returns></returns>
    public string GetTableName(string oldname)
    {
        return _options.ContainsKey(oldname) ? _options[oldname] : oldname;
    }
}