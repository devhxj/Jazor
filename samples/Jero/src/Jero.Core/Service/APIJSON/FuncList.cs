using System.Globalization;
using System.Text.Json.Nodes;

namespace Jero.Core.Service;

/// <summary>
/// 自定义方法
/// </summary>
public class FuncList
{
    /// <summary>
    /// 字符串相加
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public string Merge(object? a, object? b)
    {
        return Convert.ToString(a) + Convert.ToString(b);
    }

    /// <summary>
    /// 对象合并
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public object MergeObj(object a, object b)
    {
        return new { a, b };
    }

    /// <summary>
    /// 是否包含
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public bool IsContain(object? a, object? b)
    {
        var source = Convert.ToString(a) ?? string.Empty;
        var target = Convert.ToString(b) ?? string.Empty;
        return source.Split(',').Contains(target);
    }

    internal static object TransJsonObjectToSugarPara(JsonNode? value) =>
        value is JsonArray array ? TransJsonArrayToSugarPara(array) : ApiJsonValue.ToClrValue(value, sqlParameter: true) ?? DBNull.Value;

    internal static object TransJsonArrayToSugarPara(JsonArray array)
    {
        if (array.Count == 0)
            return Array.Empty<object>();

        // The first element controls the SQL parameter array type. Subsequent scalar
        // values use CLR conversion, including numeric strings and mixed numeric types.
        return ApiJsonValue.ToClrValue(array[0], sqlParameter: true) switch
        {
            long => ConvertArray<long>(array),
            decimal => ConvertArray<decimal>(array),
            bool => ConvertArray<bool>(array),
            DateTime => ConvertArray<DateTime>(array),
            string => array.Select(node => node switch
            {
                null => null,
                JsonValue value when value.TryGetValue<string>(out var text) => text,
                _ => ApiJsonValue.ToText(node)
            }).ToArray(),
            _ => array.Select(node => ApiJsonValue.ToClrValue(node, sqlParameter: true)).ToArray()
        };
    }

    private static T[] ConvertArray<T>(JsonArray array) =>
        array.Select(node => (T)Convert.ChangeType(ApiJsonValue.ToClrValue(node, sqlParameter: true), typeof(T), CultureInfo.InvariantCulture)!).ToArray();

    /// <summary>
    /// 获取字符串里的值的真正类型
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string GetValueCSharpType(string input)
    {
        if (DateTime.TryParse(input, out _))
            return "DateTime";
        else if (int.TryParse(input, out _))
            return "int";
        else if (long.TryParse(input, out _))
            return "long";
        else if (decimal.TryParse(input, out _))
            return "decimal";
        else if (bool.TryParse(input, out _))
            return "bool";
        else
            return "string";
    }
}
