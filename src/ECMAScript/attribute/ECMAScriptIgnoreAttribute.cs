using System;

namespace ECMAScript;

/// <summary>
/// 用于标记方法和属性被 Jazor编译器忽略
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field, Inherited = false)]
public sealed class ECMAScriptIgnoreAttribute : Attribute
{

}
