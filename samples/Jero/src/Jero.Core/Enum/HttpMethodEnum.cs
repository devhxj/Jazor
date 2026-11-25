namespace Jero.Core;

/// <summary>
/// HTTP请求方法枚举
/// </summary>
[Description("HTTP请求方法枚举")]
public enum HttpMethodEnum
{
    /// <summary>
    ///  HTTP "GET" method.
    /// </summary>
    [Description("HTTP \"GET\" method.")]
    Get,

    /// <summary>
    ///  HTTP "POST" method.
    /// </summary>
    [Description("HTTP \"POST\" method.")]
    Post,

    /// <summary>
    /// HTTP "PUT" method.
    /// </summary>
    [Description(" HTTP \"PUT\" method.")]
    Put,

    /// <summary>
    /// HTTP "DELETE" method.
    /// </summary>
    [Description("HTTP \"DELETE\" method.")]
    Delete,

    /// <summary>
    /// HTTP "PATCH" method.
    /// </summary>
    [Description("HTTP \"PATCH\" method. ")]
    Patch,

    /// <summary>
    /// HTTP "HEAD" method.
    /// </summary>
    [Description("HTTP \"HEAD\" method.")]
    Head,

    /// <summary>
    /// HTTP "OPTIONS" method.
    /// </summary>
    [Description("HTTP \"OPTIONS\" method.")]
    Options,

    /// <summary>
    /// HTTP "TRACE" method.
    /// </summary>
    [Description(" HTTP \"TRACE\" method.")]
    Trace,

    /// <summary>
    ///  HTTP "CONNECT" method.
    /// </summary>
    [Description("HTTP \"CONNECT\" method.")]
    Connect
}