namespace Jero.Application.Modules.DingTalk;

public class DingTalkRoleSimplelistResult
{
    [System.Text.Json.Serialization.JsonPropertyName("userid")]
    public string userid { get; set; } = string.Empty;

    [System.Text.Json.Serialization.JsonPropertyName("name")]
    public string name { get; set; } = string.Empty;
}
