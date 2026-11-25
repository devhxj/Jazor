namespace Jero.Plugin.DingTalk;

/// <summary>
/// 钉钉基础响应结果
/// </summary>
/// <typeparam name="T">Data</typeparam>
public class DingTalkBaseResponse<T>
    where T : new()
{
    /// <summary>
    /// 返回结果
    /// </summary>
    public T Result { get; set; } = new();

    /// <summary>
    /// 返回码
    /// </summary>
    public int ErrCode { get; set; }

    /// <summary>
    /// 返回码描述。
    /// </summary>
    public string ErrMsg { get; set; } = string.Empty;

    /// <summary>
    /// 是否调用成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 请求Id
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("request_id")]
    public string RequestId { get; set; } = string.Empty;
}
