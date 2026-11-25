namespace Jero.Plugin.DingTalk;

public class DingTalkRoleResult
{
    [System.Text.Json.Serialization.JsonPropertyName("id")]
    public long id { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("name")]
    public string name { get; set; } = string.Empty;
}
