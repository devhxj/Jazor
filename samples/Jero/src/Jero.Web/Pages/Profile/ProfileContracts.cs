namespace Jero.Web.Pages.Profile;

public sealed record ProfileSettingItem
{
    public bool Value { get; set; }
    public string FieldName { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}
