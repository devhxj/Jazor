namespace Jero.Core;

/// <summary>
/// 基本数据类型扩展（作为NewLife.Core的Utility的补充）
/// </summary>
public static class DataTypeExtension
{
    /// <summary>转为SByte整数，转换失败时返回默认值。</summary>
    /// <remarks></remarks>
    /// <param name="value">待转换对象</param>
    /// <param name="defaultValue">默认值。待转换对象无效时使用</param>
    /// <returns></returns>
    public static sbyte ToSByte(this object value, sbyte defaultValue = default)
    {
        if (value is sbyte num) return num;
        if (value == null || value == DBNull.Value) return defaultValue;

        if (sbyte.TryParse(value.ToString(), out var result))
            return result;
        else
            return defaultValue;
    }

    /// <summary>转为Byte整数，转换失败时返回默认值。</summary>
    /// <remarks></remarks>
    /// <param name="value">待转换对象</param>
    /// <param name="defaultValue">默认值。待转换对象无效时使用</param>
    /// <returns></returns>
    public static byte ToByte(this object value, byte defaultValue = default)
    {
        if (value is byte num) return num;
        if (value == null || value == DBNull.Value) return defaultValue;

        if (byte.TryParse(value.ToString(), out var result))
            return result;
        else
            return defaultValue;
    }

    /// <summary>转为Int16整数，转换失败时返回默认值。</summary>
    /// <remarks></remarks>
    /// <param name="value">待转换对象</param>
    /// <param name="defaultValue">默认值。待转换对象无效时使用</param>
    /// <returns></returns>
    public static short ToInt16(this object value, short defaultValue = default)
    {
        if (value is short num) return num;
        if (value == null || value == DBNull.Value) return defaultValue;

        if (short.TryParse(value.ToString(), out var result))
            return result;
        else
            return defaultValue;
    }

    /// <summary>转为UInt16整数，转换失败时返回默认值。</summary>
    /// <remarks></remarks>
    /// <param name="value">待转换对象</param>
    /// <param name="defaultValue">默认值。待转换对象无效时使用</param>
    /// <returns></returns>
    public static ushort ToUInt16(this object value, ushort defaultValue = default)
    {
        if (value is ushort num) return num;
        if (value == null || value == DBNull.Value) return defaultValue;

        if (ushort.TryParse(value.ToString(), out var result))
            return result;
        else
            return defaultValue;
    }

    /// <summary>转为UInt32整数，转换失败时返回默认值。</summary>
    /// <remarks></remarks>
    /// <param name="value">待转换对象</param>
    /// <param name="defaultValue">默认值。待转换对象无效时使用</param>
    /// <returns></returns>
    public static uint ToUInt32(this object value, uint defaultValue = default)
    {
        if (value is uint num) return num;
        if (value == null || value == DBNull.Value) return defaultValue;

        if (uint.TryParse(value.ToString(), out var result))
            return result;
        else
            return defaultValue;
    }

    /// <summary>转为UInt64整数，转换失败时返回默认值。</summary>
    /// <remarks></remarks>
    /// <param name="value">待转换对象</param>
    /// <param name="defaultValue">默认值。待转换对象无效时使用</param>
    /// <returns></returns>
    public static ulong ToUInt64(this object value, ulong defaultValue = default)
    {
        if (value is ulong num) return num;
        if (value == null || value == DBNull.Value) return defaultValue;

        if (ulong.TryParse(value.ToString(), out var result))
            return result;
        else
            return defaultValue;
    }
}