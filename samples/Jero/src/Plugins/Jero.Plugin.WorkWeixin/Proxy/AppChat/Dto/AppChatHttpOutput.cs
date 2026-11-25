namespace Jero.Plugin.WorkWeixin.Proxy;

public class CreatAppChatOutput : BaseWorkOutput
{
    /// <summary>
    /// 群聊的唯一标志
    /// </summary>
    [JsonPropertyName("chatid")]
    public string ChatId { get; set; } = string.Empty;
}
