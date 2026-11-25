namespace Jero.Core.Service;

/// <summary>
/// 系统枚举服务 🧩
/// </summary>
[ApiDescriptionSettings(Order = 275)]
public class SysEnumService : IDynamicApiController, ITransient
{
    private readonly EnumOptions _enumOptions;

    public SysEnumService(IOptions<EnumOptions> enumOptions)
    {
        _enumOptions = enumOptions.Value;
    }

    /// <summary>
    /// 获取所有枚举类型 🔖
    /// </summary>
    /// <returns></returns>
    [DisplayName("获取所有枚举类型")]
    public List<EnumTypeOutput> GetEnumTypeList()
    {
        var enumTypeList = App.EffectiveTypes.Where(t => t.IsEnum)
            .Where(t =>
            {
                var assemblyName = t.Assembly.GetName().Name;
                return !string.IsNullOrWhiteSpace(assemblyName) &&
                       (_enumOptions.EntityAssemblyNames.Contains(assemblyName) ||
                        _enumOptions.EntityAssemblyNames.Any(name => assemblyName.Contains(name)));
            })
            .Where(t => t.GetCustomAttributes(typeof(IgnoreEnumToDictAttribute), false).Length == 0) // 排除有忽略转字典特性类型
            .Where(t => t.GetCustomAttributes(typeof(ErrorCodeTypeAttribute), false).Length == 0) // 排除错误代码类型
            .OrderBy(u => u.Name).ThenBy(u => u.FullName)
            .ToList();

        // 如果存在同名枚举类，则依次增加 "_序号" 后缀
        var list = enumTypeList.Select(GetEnumDescription).ToList();
        foreach (var enumType in list.GroupBy(u => u.TypeName).Where(g => g.Count() > 1))
        {
            int i = 1;
            foreach (var item in list.Where(u => u.TypeName == enumType.Key).Skip(1)) item.TypeName = $"{item.TypeName}_{i++}";
        }
        return list;
    }

    /// <summary>
    /// 获取字典描述
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private static EnumTypeOutput GetEnumDescription(Type type)
    {
        string description = type.Name;
        var attrs = type.GetCustomAttributes(typeof(DescriptionAttribute), false);
        if (attrs.Length != 0)
        {
            var att = ((DescriptionAttribute[])attrs)[0];
            description = att.Description;
        }
        var enumType = App.EffectiveTypes.FirstOrDefault(t => t.IsEnum && t.Name == type.Name);
        return new EnumTypeOutput
        {
            TypeDescribe = description,
            TypeName = type.Name,
            TypeRemark = description,
            TypeFullName = type.FullName ?? type.Name,
            EnumEntities = (enumType ?? type).EnumToList()
        };
    }

    /// <summary>
    /// 通过枚举类型获取枚举值集合 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("通过枚举类型获取枚举值集合")]
    public List<EnumEntity> GetEnumDataList([FromQuery] EnumInput input)
    {
        var enumType = App.EffectiveTypes.FirstOrDefault(u => u.IsEnum && u.Name == input.EnumName);
        if (enumType is not { IsEnum: true }) throw Oops.Oh(ErrorCodeEnum.D1503);

        return enumType.EnumToList();
    }

    /// <summary>
    /// 通过实体的字段名获取相关枚举值集合（目前仅支持枚举类型） 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("通过实体的字段名获取相关枚举值集合")]
    public static List<EnumEntity> GetEnumDataListByField([FromQuery] QueryEnumDataInput input)
    {
        // 获取实体类型属性
        Type entityType = App.EffectiveTypes.FirstOrDefault(u => u.Name == input.EntityName) ?? throw Oops.Oh(ErrorCodeEnum.D1504);

        // 获取字段类型
        var fieldType = entityType.GetProperties().FirstOrDefault(u => u.Name == input.FieldName)?.PropertyType;
        if (fieldType is not { IsEnum: true }) throw Oops.Oh(ErrorCodeEnum.D1503);

        return fieldType.EnumToList();
    }
}
