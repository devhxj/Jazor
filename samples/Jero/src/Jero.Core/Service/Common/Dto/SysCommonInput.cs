namespace Jero.Core.Service;

/// <summary>
/// 接口压测输入参数
/// </summary>
public class StressTestInput
{
    /// <summary>
    /// 接口请求地址
    /// </summary>
    /// <example>https://github.com/devhxj/Jazor</example>
    [Required(ErrorMessage = "接口请求地址不能为空")]
    public string RequestUri { get; set; } = string.Empty;
    /// <summary>
    /// 请求方式
    /// </summary>
    [Required(ErrorMessage = "请求方式不能为空")]
    public string RequestMethod { get; set; } = nameof(HttpMethod.Get);

    /// <summary>
    /// 每轮请求量
    /// </summary>
    /// <example>100</example>
    [Required(ErrorMessage = "每轮请求量不能为空")]
    [Range(1, 100000, ErrorMessage = "每轮请求量必须为1-100000")]
    public int? NumberOfRequests { get; set; }

    /// <summary>
    /// 压测轮数
    /// </summary>
    /// <example>5</example>
    [Required(ErrorMessage = "压测轮数不能为空")]
    [Range(1, 10000, ErrorMessage = "压测轮数必须为1-10000")]
    public int? NumberOfRounds { get; set; }

    /// <summary>
    /// 最大并行量（默认为当前主机逻辑处理器的数量）
    /// </summary>
    /// <example>500</example>
    [Range(0, 10000, ErrorMessage = "最大并行量必须为0-10000")]
    public int? MaxDegreeOfParallelism { get; set; } = Environment.ProcessorCount;

    /// <summary>
    /// 请求参数
    /// </summary>
    public List<KeyValuePair<string, string>> RequestParameters { get; set; } = new();

    /// <summary>
    /// 请求头参数
    /// </summary>
    public Dictionary<string, string> Headers { get; set; } = new();

    /// <summary>
    /// 路径参数
    /// </summary>
    public Dictionary<string, string> PathParameters { get; set; } = new();

    /// <summary>
    /// Query参数
    /// </summary>
    public Dictionary<string, string> QueryParameters { get; set; } = new();
}