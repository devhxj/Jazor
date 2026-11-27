using System.Text.Json.Serialization;

namespace Jero.Application.Modules.WorkWeixin.Proxy;

public class AuthAccessTokenHttpOutput : BaseWorkOutput
{
    /// <summary>
    /// 获取到的凭证
    /// </summary>
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// 凭证的有效时间（秒）
    /// </summary>
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
}
