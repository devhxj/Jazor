namespace Jero.Application.Modules.DingTalk;

public class GetDingTalkTokenOutput
{
    /// <summary>
    /// 生成的access_token
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// access_token的过期时间，单位秒
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    /// <summary>
    /// 返回码描述
    /// </summary>
    public string ErrMsg { get; set; } = string.Empty;

    /// <summary>
    /// 返回码
    /// </summary>
    public int ErrCode { get; set; }
}
