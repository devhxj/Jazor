namespace Jero.Web.App.SystemUpdate;

public sealed partial record SystemBackupView
{
    public string fileName { get; init; } = string.Empty;
    public string createTime { get; init; } = string.Empty;
}
