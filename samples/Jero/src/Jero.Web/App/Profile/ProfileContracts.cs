namespace Jero.Web.App.Profile;

public sealed record ProfileSettingItem
{
    public bool value { get; set; }
    public string fieldName { get; init; } = string.Empty;
    public string label { get; init; } = string.Empty;
    public string description { get; init; } = string.Empty;
}
