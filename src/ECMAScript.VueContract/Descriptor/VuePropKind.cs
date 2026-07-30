namespace ECMAScript.VueContract.Descriptor;

/// <summary>描述 Vue prop 在组件 contract 中的语义类别。</summary>
/// <remarks>枚举值只用于编译期分类，最终 prop 仍按 Vue runtime 的普通值传递。</remarks>
public enum VuePropKind
{
    Normal,
    Model,
    HtmlLike,
    LibrarySpecific
}
