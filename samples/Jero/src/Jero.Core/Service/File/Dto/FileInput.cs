namespace Jero.Core.Service;

/// <summary>
/// 文件分页查询
/// </summary>
public class PageFileInput : BasePageInput
{
    /// <summary>
    /// 文件名称
    /// </summary>
    public string FileName { get; set; } = string.Empty;
    /// <summary>
    /// 文件路径
    /// </summary>
    public string FilePath { get; set; } = string.Empty;
    /// <summary>
    /// 文件后缀
    /// </summary>
    public string? Suffix { get; set; }

    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTime? StartTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTime? EndTime { get; set; }
}

/// <summary>
/// 上传文件
/// </summary>
public class UploadFileInput
{
    /// <summary>
    /// 文件
    /// </summary>
    [Required]
    public IFormFile? File { get; set; }

    /// <summary>
    /// 文件类别
    /// </summary>
    public string FileType { get; set; } = string.Empty;
    /// <summary>
    /// 指定存储路径
    /// </summary>
    public string? FilePath { get; set; }

    /// <summary>
    /// 是否公开
    /// </summary>
    public bool IsPublic { get; set; } = false;

    /// <summary>
    /// 允许格式：.jpeg.jpg.png.bmp.gif.tif
    /// </summary>
    public string AllowSuffix { get; set; } = string.Empty;
    /// <summary>
    /// 指定存储桶名称
    /// </summary>
    public string? BucketName { get; set; }

    /// <summary>
    /// 指定存储提供者ID
    /// </summary>
    public long? ProviderId { get; set; }

    /// <summary>
    /// 业务数据Id
    /// </summary>
    public long? DataId { get; set; }
}

/// <summary>
/// 上传文件Base64
/// </summary>
public class UploadFileFromBase64Input
{
    /// <summary>
    /// 文件名
    /// </summary>
    public string FileName { get; set; } = string.Empty;
    /// <summary>
    /// 文件内容
    /// </summary>
    public string FileDataBase64 { get; set; } = string.Empty;
    /// <summary>
    /// 文件类型( "image/jpeg",)
    /// </summary>
    public string ContentType { get; set; } = string.Empty;
}

/// <summary>
/// 查询关联查询输入
/// </summary>
public class RelationQueryInput
{
    /// <summary>
    /// 关联对象名称
    /// </summary>
    public string RelationName { get; set; } = string.Empty;
    /// <summary>
    /// 关联对象Id
    /// </summary>
    public long? RelationId { get; set; }

    /// <summary>
    /// 文件类型：多个以","分割
    /// </summary>
    public string FileTypes { get; set; } = string.Empty;
    /// <summary>
    /// 所属Id
    /// </summary>
    public long? BelongId { get; set; }

    /// <summary>
    /// 文件类型分割
    /// </summary>
    /// <returns></returns>
    public string[] GetFileTypeBS()
    {
        return FileTypes.Split(',');
    }
}
