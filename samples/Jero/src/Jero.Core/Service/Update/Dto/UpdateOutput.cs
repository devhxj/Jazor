namespace Jero.Core.Service;

public class BackupOutput
{
    /// <summary>
    /// 文件路径
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public string FilePath { get; set; } = string.Empty;
    /// <summary>
    /// 文件名
    /// </summary>
    public string FileName { get; set; } = string.Empty;
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreateTime { get; set; }
}