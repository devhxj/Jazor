using System;
using System.Runtime.Versioning;

namespace ECMAScript;

/// <summary>
/// 用于标记类使用 ECMAScript 语法校验
/// </summary>
[AttributeUsage(AttributeTargets.All, Inherited = false)]
[SupportedOSPlatform("browser")]
internal sealed class ECMAScriptAttribute : Attribute
{
}
