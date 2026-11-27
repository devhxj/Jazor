using static ECMAScript.Vue;

namespace Jero.Web.Application;

public sealed record AppUnreadNotice : VueProps
{
    public string Id { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public int Type { get; init; }
    public string PublicUserName { get; init; } = string.Empty;
    public string PublicTime { get; init; } = string.Empty;
}
