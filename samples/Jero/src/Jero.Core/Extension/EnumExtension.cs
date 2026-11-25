namespace Jero.Core;

/// <summary>
/// 枚举拓展
/// </summary>
public static class EnumExtension
{
    // 枚举显示字典缓存
    private static readonly ConcurrentDictionary<Type, Dictionary<int, string>> EnumDisplayValueDict = new();

    // 枚举值字典缓存
    private static readonly ConcurrentDictionary<Type, Dictionary<int, string>> EnumNameValueDict = new();

    // 枚举类型缓存
    private static readonly ConcurrentDictionary<Assembly, ConcurrentDictionary<string, Type>> EnumTypeDict = new();

    /// <summary>
    /// 获取枚举对象Key与名称的字典（缓存）
    /// </summary>
    /// <param name="enumType"></param>
    /// <returns></returns>
    public static Dictionary<int, string> GetEnumDictionary(this Type enumType)
    {
        if (!enumType.IsEnum)
            throw new ArgumentException("Type '" + enumType.Name + "' is not an enum.");

        // 查询缓存
        var enumDic = EnumNameValueDict.TryGetValue(enumType, out var value) ? value : new Dictionary<int, string>();
        if (enumDic.Count != 0)
            return enumDic;
        // 取枚举类型的Key/Value字典集合
        enumDic = GetEnumDictionaryItems(enumType);

        // 缓存
        EnumNameValueDict[enumType] = enumDic;

        return enumDic;
    }

    /// <summary>
    /// 获取枚举对象Key与名称的字典
    /// </summary>
    /// <param name="enumType"></param>
    /// <returns></returns>
    private static Dictionary<int, string> GetEnumDictionaryItems(this Type enumType)
    {
        // 获取类型的字段，初始化一个有限长度的字典
        var enumFields = enumType.GetFields(BindingFlags.Public | BindingFlags.Static);
        Dictionary<int, string> enumDic = new(enumFields.Length);

        // 遍历字段数组获取key和name
        foreach (var enumField in enumFields)
        {
            var intValue = (int)enumField.GetValue(enumType)!;
            enumDic[intValue] = enumField.Name;
        }

        return enumDic;
    }

    /// <summary>
    /// 获取枚举类型key与描述的字典（缓存）
    /// </summary>
    /// <param name="enumType"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static Dictionary<int, string> GetEnumDescDictionary(this Type enumType)
    {
        if (!enumType.IsEnum)
            throw new ArgumentException("Type '" + enumType.Name + "' is not an enum.");

        // 查询缓存
        var enumDic = EnumDisplayValueDict.TryGetValue(enumType, out var value)
            ? value
            : new Dictionary<int, string>();
        if (enumDic.Count != 0)
            return enumDic;
        // 取枚举类型的Key/Value字典集合
        enumDic = GetEnumDescDictionaryItems(enumType);

        // 缓存
        EnumDisplayValueDict[enumType] = enumDic;

        return enumDic;
    }

    /// <summary>
    /// 获取枚举类型key与描述的字典（没有描述则获取name）
    /// </summary>
    /// <param name="enumType"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private static Dictionary<int, string> GetEnumDescDictionaryItems(this Type enumType)
    {
        // 获取类型的字段，初始化一个有限长度的字典
        var enumFields = enumType.GetFields(BindingFlags.Public | BindingFlags.Static);
        Dictionary<int, string> enumDic = new(enumFields.Length);

        // 遍历字段数组获取key和name
        foreach (var enumField in enumFields)
        {
            var intValue = (int)enumField.GetValue(enumType)!;
            var desc = enumField.GetDescriptionValue<DescriptionAttribute>();
            enumDic[intValue] = desc != null && !string.IsNullOrEmpty(desc.Description) ? desc.Description : enumField.Name;
        }

        return enumDic;
    }

    /// <summary>
    /// 从程序集中查找指定枚举类型
    /// </summary>
    /// <param name="assembly"></param>
    /// <param name="typeName"></param>
    /// <returns></returns>
    public static Type? TryToGetEnumType(Assembly assembly, string typeName)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        if (string.IsNullOrWhiteSpace(typeName)) return null;

        // 按名称查找
        var enumTypes = EnumTypeDict.GetOrAdd(assembly, LoadEnumTypeDict);
        return enumTypes.TryGetValue(typeName, out var value) ? value : null;
    }

    /// <summary>
    /// 从程序集中加载所有枚举类型
    /// </summary>
    /// <param name="assembly"></param>
    /// <returns></returns>
    private static ConcurrentDictionary<string, Type> LoadEnumTypeDict(Assembly assembly)
    {
        // 取程序集中所有类型
        var typeArray = assembly.GetTypes();

        // 过滤非枚举类型，转成字典格式并返回
        var dict = typeArray.Where(o => o.IsEnum).ToDictionary(o => o.Name, o => o);
        ConcurrentDictionary<string, Type> enumTypeDict = new(dict);
        return enumTypeDict;
    }

    /// <summary>
    /// 获取枚举的Description
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string? GetEnumDescription(this Enum value)
    {
        return value.GetType().GetField(value.ToString())?.GetCustomAttribute<DescriptionAttribute>()?.Description;
    }

    /// <summary>
    /// 获取枚举的Description
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string? GetEnumDescription(this object value)
    {
        return value.GetType().GetField(value.ToString()!)?.GetCustomAttribute<DescriptionAttribute>()?.Description;
    }

    /// <summary>
    /// 根据枚举值描述获取枚举值
    /// </summary>
    /// <typeparam name="T">枚举类型</typeparam>
    /// <param name="description">枚举描述</param>
    /// <returns>枚举值</returns>
    public static T GetValueByDescription<T>(this string description) where T : struct, Enum
    {
        var type = typeof(T);
        var dict = GetEnumDescDictionary(type);

        if (dict.ContainsValue(description))
            return (T)Enum.ToObject(type, dict.FirstOrDefault(x => x.Value == description).Key);
        else
            return default;
    }

    /// <summary>
    /// 根据枚举值描述获取枚举值（非泛型版本）
    /// </summary>
    /// <param name="description">枚举描述</param>
    /// <param name="enumType">枚举类型</param>
    /// <returns>枚举值</returns>
    public static object GetValueByDescription(this string description, Type enumType)
    {
        if (!enumType.IsEnum)
            throw new ArgumentException("Type '" + enumType.Name + "' is not an enum.");

        var dict = GetEnumDescDictionary(enumType);
        var value = dict.FirstOrDefault(x => x.Value == description).Key;

        return Enum.ToObject(enumType, value);
    }

    /// <summary>
    /// 获取枚举的Theme
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string? GetTheme(this object value)
    {
        return value.GetType().GetField(value.ToString()!)?.GetCustomAttribute<ThemeAttribute>()?.Theme;
    }

    /// <summary>
    /// 将枚举转成枚举信息集合
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static List<EnumEntity> EnumToList(this Type type)
    {
        if (!type.IsEnum)
            throw new ArgumentException("Type '" + type.Name + "' is not an enum.");
        var arr = Enum.GetNames(type);
        return arr.Select(sl =>
        {
            var item = Enum.Parse(type, sl);
            return new EnumEntity
            {
                Name = item.ToString() ?? string.Empty,
                Describe = item.GetEnumDescription() ?? item.ToString() ?? string.Empty,
                Theme = item.GetTheme() ?? string.Empty,
                Value = item.GetHashCode()
            };
        }).ToList();
    }

    /// <summary>
    /// 枚举ToList
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="type"></param>
    /// <returns></returns>
    public static List<T> EnumToList<T>(this Type type)
    {
        if (!type.IsEnum)
            throw new ArgumentException("Type '" + type.Name + "' is not an enum.");
        var arr = Enum.GetNames(type);
        return arr.Select(name => (T)Enum.Parse(type, name)).ToList();
    }
}

/// <summary>
/// 枚举实体
/// </summary>
public class EnumEntity
{
    /// <summary>
    /// 枚举的描述
    /// </summary>
    public string Describe { get; set; } = string.Empty;
    /// <summary>
    /// 枚举的样式
    /// </summary>
    public string Theme { get; set; } = string.Empty;
    /// <summary>
    /// 枚举名称
    /// </summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>
    /// 枚举对象的值
    /// </summary>
    public int Value { get; set; }
}
