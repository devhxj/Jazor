namespace Jero.Plugin.DingTalk;

public class DingTalkRoleListResult
{
    [System.Text.Json.Serialization.JsonPropertyName("groupId")]
    public long groupId { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("name")]
    public string name { get; set; } = string.Empty;

    [System.Text.Json.Serialization.JsonPropertyName("roles")]
    public List<DingTalkRoleResult> roles { get; set; } = [];
}
