


using System.Globalization;
namespace Jero.Core;

using System;

/// <summary>
/// 安全的基本数学运算方法类
/// </summary>
public static class SafeMath
{
    /// <summary>
    /// 安全加法
    /// </summary>
    /// <param name="left">左操作数</param>
    /// <param name="right">右操作数</param>
    /// <param name="precision">保留小数位数</param>
    /// <param name="defaultValue">默认值</param>
    /// <param name="throwOnError">是否抛出异常</param>
    /// <returns></returns>
    public static T Add<T>(object? left, object? right, int precision = 2, T defaultValue = default, bool throwOnError = true) where T : struct, IComparable, IConvertible, IFormattable
    {
        return PerformOperation(left, right, (a, b) => a + b, precision, defaultValue, throwOnError);
    }

    /// <summary>
    /// 安全减法
    /// </summary>
    /// <param name="left">左操作数</param>
    /// <param name="right">右操作数</param>
    /// <param name="precision">保留小数位数</param>
    /// <param name="defaultValue">默认值</param>
    /// <param name="throwOnError">是否抛出异常</param>
    public static T Sub<T>(object? left, object? right, int precision = 2, T defaultValue = default, bool throwOnError = true) where T : struct, IComparable, IConvertible, IFormattable
    {
        return PerformOperation(left, right, (a, b) => a - b, precision, defaultValue, throwOnError);
    }

    /// <summary>
    /// 安全乘法
    /// </summary>
    /// <param name="left">左操作数</param>
    /// <param name="right">右操作数</param>
    /// <param name="precision">保留小数位数</param>
    /// <param name="defaultValue">默认值</param>
    /// <param name="throwOnError">是否抛出异常</param>
    public static T Mult<T>(object? left, object? right, int precision = 2, T defaultValue = default, bool throwOnError = true) where T : struct, IComparable, IConvertible, IFormattable
    {
        return PerformOperation(left, right, (a, b) => a * b, precision, defaultValue, throwOnError);
    }

    /// <summary>
    /// 安全除法
    /// </summary>
    /// <param name="left">左操作数</param>
    /// <param name="right">右操作数</param>
    /// <param name="precision">保留小数位数</param>
    /// <param name="defaultValue">默认值</param>
    /// <param name="throwOnDivideByZero">是否抛出除以零异常</param>
    public static T Div<T>(object? left, object? right, int precision = 2, T defaultValue = default, bool throwOnDivideByZero = true) where T : struct, IComparable, IConvertible, IFormattable
    {
        return PerformOperation(left, right, (a, b) =>
        {
            if (b != 0) return a / b;
            if (throwOnDivideByZero) throw new DivideByZeroException("除数不能为0");
            return SafeConvert<decimal>(defaultValue);
        }, precision, defaultValue, throwOnDivideByZero);
    }

    /// <summary>
    /// 安全类型转换
    /// </summary>
    /// <param name="value">数据源</param>
    /// <param name="defaultValue">默认值</param>
    public static T SafeConvert<T>(object? value, T defaultValue = default) where T : struct, IComparable, IConvertible, IFormattable
    {
        if (value == null) return defaultValue;
        try
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }
        catch
        {
            return defaultValue;
        }
    }

    /// <summary>
    /// 执行数学运算
    /// </summary>
    private static T PerformOperation<T>(object? left, object? right, Func<decimal, decimal, decimal> operation, int precision, T defaultValue, bool throwOnError) where T : struct, IComparable, IConvertible, IFormattable
    {
        try
        {
            decimal leftValue = ConvertToDecimal(left);
            decimal rightValue = ConvertToDecimal(right);

            decimal result = operation(leftValue, rightValue);
            return SafeConvert(Math.Round(result, precision, MidpointRounding.AwayFromZero), defaultValue);
        }
        catch
        {
            if (throwOnError) throw;
            return defaultValue;
        }
    }

    /// <summary>
    /// 将输入值转换为 decimal
    /// </summary>
    public static decimal ConvertToDecimal(object? value)
    {
        return value switch
        {
            null => 0m,
            int intValue => intValue,
            float floatValue => (decimal)floatValue,
            double doubleValue => (decimal)doubleValue,
            decimal decimalValue => decimalValue,
            long longValue => longValue,
            short shortValue => shortValue,
            byte byteValue => byteValue,
            string stringValue when decimal.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal parsedValue) => parsedValue, // 尝试解析字符串
            _ => throw new InvalidCastException($"不支持的类型: {value.GetType().Name}")
        };
    }
}
