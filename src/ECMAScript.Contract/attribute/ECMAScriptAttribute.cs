namespace ECMAScript;

/// <summary>
/// Marks a declaration as an ECMAScript host contract that Jazor can validate and lower.
/// 标记声明为可由 Jazor 校验和 lowering 的 ECMAScript 宿主契约。
/// </summary>
/// <remarks>
/// The attribute carries at most one argument: the exact external ESM import specifier that must
/// appear in emitted JavaScript. The parameterless form is an ambient host contract that does not
/// bind an external module.
///
/// 该特性最多一个参数：生成 JavaScript 中必须出现的精确外部 ESM import specifier。
/// 无参形式是环境宿主契约，不绑定外部模块。
///
/// 组件身份不由该特性表达。派生自 <c>Microsoft.AspNetCore.Components.ComponentBase</c>
/// 且实现 Vue marker 的类型即组件，声明面与普通绑定相同。
///
/// 导出名由名字机制给出（<c>ECMAScriptName</c> / <c>Description("@#...")</c>），不是该特性的参数。
///
/// 当特性标在具体成员上时，该成员的 specifier 优先于宿主类型上的 specifier。这样一个
/// C# 宿主类型可以映射多个细粒度 ESM 入口，而编译器和 Emit 仍能按调用点收集 tree-shaking roots。
/// </remarks>
[AttributeUsage(AttributeTargets.All, Inherited = false)]
public class ECMAScriptAttribute : Attribute
{
    /// <summary>Gets the exact ESM import specifier, or <see langword="null"/> for an ambient contract.</summary>
    public string? Import { get; }

    /// <summary>Creates an ambient host contract that does not bind an external module.</summary>
    public ECMAScriptAttribute()
    {
        Import = null;
    }

    /// <summary>Creates an external module binding.</summary>
    /// <param name="import">The exact package or module import specifier preserved in generated JavaScript.</param>
    public ECMAScriptAttribute(string import)
    {
        if (string.IsNullOrWhiteSpace(import))
            throw new ArgumentException("An ECMAScript binding requires an import specifier.", nameof(import));

        Import = import;
    }
}
