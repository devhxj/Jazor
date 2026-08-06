using System;

namespace ECMAScript.VueContract;

/// <summary>
/// Maps a Razor event parameter to a Vue event name that cannot be inferred by convention.
/// 映射无法通过约定推断的 Razor 事件参数与 Vue 原始事件名。
/// </summary>
/// <remarks>
/// Payload typing remains owned by <c>EventCallback&lt;T&gt;</c>. This attribute only
/// carries exceptional raw names such as <c>click:close</c> or <c>loadstart</c>.
/// payload 类型由 <c>EventCallback&lt;T&gt;</c> 提供；本特性只承载
/// <c>click:close</c>、<c>loadstart</c> 等异常原始名称。
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class VueLibraryEmitAttribute(string razorAlias) : Attribute
{
    public string RazorAlias { get; } = razorAlias;

    public string? Name { get; set; }
}
