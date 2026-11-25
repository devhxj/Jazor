namespace Jero.Core;

/// <summary>
/// 枚举值合规性校验特性
/// </summary>
[SuppressSniffer]
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Enum | AttributeTargets.Field, AllowMultiple = true)]
public class EnumAttribute : ValidationAttribute, ITransient
{
    /// <summary>
    /// 枚举值合规性校验特性
    /// </summary>
    /// <param name="errorMessage"></param>
    public EnumAttribute(string errorMessage = "枚举值不合法！")
    {
        ErrorMessage = errorMessage;
    }

    /// <summary>
    /// 枚举值合规性校验
    /// </summary>
    /// <param name="value"></param>
    /// <param name="validationContext"></param>
    /// <returns></returns>
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(validationContext.MemberName))
            return new ValidationResult("缺少待校验属性名");

        // 获取属性的类型
        var property = validationContext.ObjectType.GetProperty(validationContext.MemberName);
        if (property == null)
            return new ValidationResult($"未知属性: {validationContext.MemberName}");

        var propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

        // 检查属性类型是否为枚举或可空枚举类型
        if (!propertyType.IsEnum)
            return new ValidationResult($"属性类型'{validationContext.MemberName}'不是有效的枚举类型！");

        // 检查枚举值是否有效
        if (value == null && Nullable.GetUnderlyingType(property.PropertyType) == null)
            return new ValidationResult($"提示：{ErrorMessage}|枚举值不能为 null！");

        if (value != null && !Enum.IsDefined(propertyType, value))
            return new ValidationResult($"提示：{ErrorMessage}|枚举值【{value}】不是有效的【{propertyType.Name}】枚举类型值！");

        return ValidationResult.Success;
    }
}
