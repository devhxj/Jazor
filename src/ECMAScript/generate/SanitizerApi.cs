namespace ECMAScript;

/// <summary>
/// SetHTMLOptions
/// </summary>
[ECMAScript]
[Description("@#SetHTMLOptions")]
public record SetHTMLOptions(
    [property: Description("@#sanitizer")]Either<Sanitizer, SanitizerConfig, SanitizerPresets>? Sanitizer = default);

/// <summary>
/// SetHTMLUnsafeOptions
/// </summary>
[ECMAScript]
[Description("@#SetHTMLUnsafeOptions")]
public record SetHTMLUnsafeOptions(
    [property: Description("@#sanitizer")]Either<Sanitizer, SanitizerConfig, SanitizerPresets>? Sanitizer = default);

/// <summary>
/// SanitizerElementNamespace
/// </summary>
[ECMAScript]
[Description("@#SanitizerElementNamespace")]
public record SanitizerElementNamespace(
    [property: Description("@#name")]string? Name = default,
	[property: Description("@#namespace")]string? Namespace = "http://www.w3.org/1999/xhtml");

/// <summary>
/// SanitizerElementNamespaceWithAttributes
/// </summary>
[ECMAScript]
[Description("@#SanitizerElementNamespaceWithAttributes")]
public record SanitizerElementNamespaceWithAttributes(
    [property: Description("@#attributes")]SanitizerAttribute[]? Attributes = default,
	[property: Description("@#removeAttributes")]SanitizerAttribute[]? RemoveAttributes = default): SanitizerElementNamespace;

/// <summary>
/// SanitizerAttributeNamespace
/// </summary>
[ECMAScript]
[Description("@#SanitizerAttributeNamespace")]
public record SanitizerAttributeNamespace(
    [property: Description("@#name")]string? Name = default,
	[property: Description("@#namespace")]string? Namespace = null);

/// <summary>
/// SanitizerConfig
/// </summary>
[ECMAScript]
[Description("@#SanitizerConfig")]
public record SanitizerConfig(
    [property: Description("@#elements")]SanitizerElementWithAttributes[]? Elements = default,
	[property: Description("@#removeElements")]SanitizerElement[]? RemoveElements = default,
	[property: Description("@#replaceWithChildrenElements")]SanitizerElement[]? ReplaceWithChildrenElements = default,
	[property: Description("@#attributes")]SanitizerAttribute[]? Attributes = default,
	[property: Description("@#removeAttributes")]SanitizerAttribute[]? RemoveAttributes = default,
	[property: Description("@#comments")]bool Comments = default,
	[property: Description("@#dataAttributes")]bool DataAttributes = default);

/// <summary>
/// Sanitizer
/// </summary>
[ECMAScript]
[Description("@#Sanitizer")]
public class Sanitizer
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="configuration">configuration</param>
    public extern Sanitizer(Either<SanitizerConfig, SanitizerPresets> configuration);

    /// <summary>
    /// get
    /// </summary>
    [Description("@#get")]
    public extern SanitizerConfig Get();

    /// <summary>
    /// allowElement
    /// </summary>
    /// <param name="element">element</param>
    [Description("@#allowElement")]
    public extern void AllowElement(SanitizerElementWithAttributes element);

    /// <summary>
    /// removeElement
    /// </summary>
    /// <param name="element">element</param>
    [Description("@#removeElement")]
    public extern void RemoveElement(SanitizerElement element);

    /// <summary>
    /// replaceElementWithChildren
    /// </summary>
    /// <param name="element">element</param>
    [Description("@#replaceElementWithChildren")]
    public extern void ReplaceElementWithChildren(SanitizerElement element);

    /// <summary>
    /// allowAttribute
    /// </summary>
    /// <param name="attribute">attribute</param>
    [Description("@#allowAttribute")]
    public extern void AllowAttribute(SanitizerAttribute attribute);

    /// <summary>
    /// removeAttribute
    /// </summary>
    /// <param name="attribute">attribute</param>
    [Description("@#removeAttribute")]
    public extern void RemoveAttribute(SanitizerAttribute attribute);

    /// <summary>
    /// setComments
    /// </summary>
    /// <param name="allow">allow</param>
    [Description("@#setComments")]
    public extern void SetComments(bool allow);

    /// <summary>
    /// setDataAttributes
    /// </summary>
    /// <param name="allow">allow</param>
    [Description("@#setDataAttributes")]
    public extern void SetDataAttributes(bool allow);

    /// <summary>
    /// removeUnsafe
    /// </summary>
    [Description("@#removeUnsafe")]
    public extern void RemoveUnsafe();
}