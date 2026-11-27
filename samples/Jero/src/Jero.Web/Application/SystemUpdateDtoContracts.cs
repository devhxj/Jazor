using static ECMAScript.Vue;

namespace Jero.Web.Application;

public sealed record SystemBackupView : VueProps
{
    public string FileName { get; init; } = string.Empty;
    public string CreateTime { get; init; } = string.Empty;
}
