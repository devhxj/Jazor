namespace ECMAScript;

/// <summary>
/// ContentDescription
/// </summary>
[ECMAScript]
[Description("@#ContentDescription")]
public record ContentDescription(
    [property: Description("@#id")]string? Id = default,
	[property: Description("@#title")]string? Title = default,
	[property: Description("@#description")]string? Description = default,
	[property: Description("@#category")]ContentCategory Category = ContentCategory.Empty,
	[property: Description("@#icons")]ImageResource[]? Icons = default,
	[property: Description("@#url")]string? Url = default);

/// <summary>
/// ContentIndexEventInit
/// </summary>
[ECMAScript]
[Description("@#ContentIndexEventInit")]
public record ContentIndexEventInit(
    [property: Description("@#id")]string? Id = default): ExtendableEventInit;

/// <summary>
/// ContentIndex
/// </summary>
[ECMAScript]
[Description("@#ContentIndex")]
public class ContentIndex
{
    /// <summary>
    /// add
    /// </summary>
    /// <param name="description">description</param>
    [Description("@#add")]
    public extern PromiseResult<object> Add(ContentDescription description);

    /// <summary>
    /// delete
    /// </summary>
    /// <param name="id">id</param>
    [Description("@#delete")]
    public extern PromiseResult<object> Delete(string id);

    /// <summary>
    /// getAll
    /// </summary>
    [Description("@#getAll")]
    public extern PromiseResult<ContentDescription[]> GetAll();
}

/// <summary>
/// ContentIndexEvent
/// </summary>
[ECMAScript]
[Description("@#ContentIndexEvent")]
public class ContentIndexEvent(string type, ExtendableEventInit eventInitDict) : ExtendableEvent(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="init">init</param>
    public extern ContentIndexEvent(string type, ContentIndexEventInit init);

    /// <summary>
    /// id
    /// </summary>
    [Description("@#id")]
    public extern string Id { get; }
}