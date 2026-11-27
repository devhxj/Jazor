namespace Jero.Web.Application;

public sealed record CacheValuePreview
{
    public string TypeLabel { get; init; } = "unknown";
    public string SizeLabel { get; init; } = string.Empty;
    public string DisplayText { get; init; } = string.Empty;
}
