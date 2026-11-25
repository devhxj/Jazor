namespace Jero.Core;

/// <summary>
/// 字典值合规性校验特性
/// </summary>
[SuppressSniffer]
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
public class DictAttribute : ValidationAttribute, ITransient
{
    /// <summary>
    /// 字典编码
    /// </summary>
    public string DictTypeCode { get; }

    /// <summary>
    /// 是否允许空字符串
    /// </summary>
    public bool AllowEmptyStrings { get; set; } = false;

    /// <summary>
    /// 允许空值，有值才验证，默认 false
    /// </summary>
    public bool AllowNullValue { get; set; } = false;

    /// <summary>
    /// 字典值合规性校验特性
    /// </summary>
    /// <param name="dictTypeCode"></param>
    /// <param name="errorMessage"></param>
    public DictAttribute(string dictTypeCode = "", string errorMessage = "字典值不合法！")
    {
        DictTypeCode = dictTypeCode;
        ErrorMessage = errorMessage;
    }

    /// <summary>
    /// 字典值合规性校验
    /// </summary>
    /// <param name="value"></param>
    /// <param name="validationContext"></param>
    /// <returns></returns>
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        // 判断是否允许空值
        if (AllowNullValue && value == null) return ValidationResult.Success;

        // 获取属性的类型
        var property = validationContext.ObjectType.GetProperty(validationContext.MemberName!);
        if (property == null) return new ValidationResult($"未知属性: {validationContext.MemberName}");

        string importHeaderName = GetImporterHeaderName(property, validationContext.MemberName ?? property.Name);

        var propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

        // 先尝试从 ValidationContext 的依赖注入容器中拿服务，拿不到或类型不匹配时，再从全局的 App 容器中获取
        if (validationContext.GetService(typeof(SysDictDataService)) is not SysDictDataService sysDictDataService)
            sysDictDataService = App.GetRequiredService<SysDictDataService>();

        // 获取字典值列表
        var dictDataList = sysDictDataService.GetDataList(DictTypeCode).GetAwaiter().GetResult();

        // 使用 HashSet 来提高查找效率
        var dictHash = new HashSet<string>(dictDataList.Select(u => u.Value));

        // 判断是否为集合类型
        if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(List<>))
        {
            // 如果是空集合并且允许空值，则直接返回成功
            if (value == null && AllowNullValue) return ValidationResult.Success;

            // 处理集合为空的情况
            var collection = value as IEnumerable;
            if (collection == null) return ValidationResult.Success;

            // 获取集合的元素类型
            var elementType = propertyType.GetGenericArguments()[0];
            var underlyingElementType = Nullable.GetUnderlyingType(elementType) ?? elementType;

            // 如果元素类型是枚举，则逐个验证
            if (underlyingElementType.IsEnum)
            {
                foreach (var item in collection)
                {
                    if (item == null && AllowNullValue) continue;

                    if (!Enum.IsDefined(underlyingElementType, item!))
                        return new ValidationResult($"提示：{ErrorMessage}|枚举值【{item}】不是有效的【{underlyingElementType.Name}】枚举类型值！", [importHeaderName]);
                }
                return ValidationResult.Success;
            }

            foreach (var item in collection)
            {
                if (item == null && AllowNullValue) continue;

                var itemString = item?.ToString();
                if (itemString == null)
                    return new ValidationResult($"提示：{ErrorMessage}|字典【{DictTypeCode}】不允许空值！", [importHeaderName]);

                if (!dictHash.Contains(itemString))
                    return new ValidationResult($"提示：{ErrorMessage}|字典【{DictTypeCode}】不包含【{itemString}】！", [importHeaderName]);
            }

            return ValidationResult.Success;
        }

        var valueAsString = value?.ToString();

        // 是否忽略空字符串
        if (AllowEmptyStrings && string.IsNullOrEmpty(valueAsString)) return ValidationResult.Success;

        // 枚举类型验证
        if (propertyType.IsEnum)
        {
            if (!Enum.IsDefined(propertyType, value!)) return new ValidationResult($"提示：{ErrorMessage}|枚举值【{value}】不是有效的【{propertyType.Name}】枚举类型值！", [importHeaderName]);
            return ValidationResult.Success;
        }

        if (valueAsString == null)
            return new ValidationResult($"提示：{ErrorMessage}|字典【{DictTypeCode}】不允许空值！", [importHeaderName]);

        if (!dictHash.Contains(valueAsString))
            return new ValidationResult($"提示：{ErrorMessage}|字典【{DictTypeCode}】不包含【{valueAsString}】！", [importHeaderName]);

        return ValidationResult.Success;
    }

    /// <summary>
    /// 获取本字段上 [ImporterHeader(Name = "xxx")] 里的Name，如果没有则使用defaultName.
    /// 用于在从excel导入数据时，能让调用者知道是哪个字段验证失败，而不是抛异常
    /// </summary>
    private static string GetImporterHeaderName(PropertyInfo property, string defaultName)
    {
        var importerHeader = property.GetCustomAttribute<ImporterHeaderAttribute>();
        string importerHeaderName = importerHeader?.Name ?? defaultName;
        return importerHeaderName;
    }
}
