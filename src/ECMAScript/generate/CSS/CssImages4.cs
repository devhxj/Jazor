namespace ECMAScript.CSS;

/// <summary>
/// CSSStyleDeclaration
/// </summary>
[ECMAScript]
[Description("@#CSSStyleDeclaration")]
public partial class CSSStyleDeclaration
{
    /// <summary>
    /// objectFit
    /// </summary>
    [Description("@#objectFit")]
    public extern string ObjectFit { get; set; }

    /// <summary>
    /// imageResolution
    /// </summary>
    [Description("@#imageResolution")]
    public extern string ImageResolution { get; set; }

    /// <summary>
    /// cssText
    /// </summary>
    [Description("@#cssText")]
    public extern string CssText { get; set; }

    /// <summary>
    /// length
    /// </summary>
    [Description("@#length")]
    public extern uint Length { get; }

    /// <summary>
    /// item
    /// </summary>
    /// <param name="index">index</param>
    [Description("@#item")]
    public extern string GetItem(uint index);

    /// <summary>
    /// getPropertyValue
    /// </summary>
    /// <param name="property">property</param>
    [Description("@#getPropertyValue")]
    public extern string GetPropertyValue(string property);

    /// <summary>
    /// getPropertyPriority
    /// </summary>
    /// <param name="property">property</param>
    [Description("@#getPropertyPriority")]
    public extern string GetPropertyPriority(string property);

    /// <summary>
    /// setProperty
    /// </summary>
    /// <param name="property">property</param>
    /// <param name="value">value</param>
    /// <param name="priority">priority</param>
    [Description("@#setProperty")]
    public extern void SetProperty(string property, string value, string? priority = default);

    /// <summary>
    /// removeProperty
    /// </summary>
    /// <param name="property">property</param>
    [Description("@#removeProperty")]
    public extern string RemoveProperty(string property);

    /// <summary>
    /// parentRule
    /// </summary>
    [Description("@#parentRule")]
    public extern CSSRule? ParentRule { get; }
}