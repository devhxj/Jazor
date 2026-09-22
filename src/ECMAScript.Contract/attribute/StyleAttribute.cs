namespace ECMAScript;

/// <summary>
/// Declares a style side-effect import for a binding entry.
/// 声明绑定入口的样式 side-effect 导入。
/// </summary>
/// <remarks>
/// <para>
/// <c>[Style]</c> 是"绑定 CSS 导入"：只产生一条纯副作用边，不引入任何绑定的标识符。
/// 它与 <see cref="ECMAScriptAttribute"/> 的区别就是要不要绑定名字。
/// </para>
/// <para>
/// 可重复声明；**声明顺序即导入顺序，也是 CSS 层叠顺序**。
/// </para>
/// <para>
/// 只声明 specifier，不做形态判断，也不从扩展名推断是 JS 模块还是样式表。
/// 后续处理不属于 Jazor：依赖恢复由 Deno 负责，打包与 CSS 抽取由所选标准构建工具负责，
/// 开发期模块请求由 dev server 负责。
/// </para>
/// <para>
/// 依附于入口：挂在 <c>[ECMAScript("specifier")]</c> 声明的入口上，"入口"由该特性唯一确定。
/// 只声明 <c>[Style]</c> 而没有对应 <c>[ECMAScript]</c> 入口是构建错误，不允许样式边悬空。
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
public sealed class StyleAttribute : Attribute
{
    /// <summary>Gets the style side-effect import specifier.</summary>
    public string Specifier { get; }

    /// <summary>Declares a style side-effect import for the owning binding entry.</summary>
    /// <param name="specifier">The exact package or module specifier of the style entry.</param>
    public StyleAttribute(string specifier)
    {
        if (string.IsNullOrWhiteSpace(specifier))
            throw new ArgumentException("A style declaration requires a specifier.", nameof(specifier));

        Specifier = specifier;
    }
}
