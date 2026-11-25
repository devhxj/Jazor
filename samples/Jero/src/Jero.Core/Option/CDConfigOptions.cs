namespace Jero.Core;

/// <summary>
/// CI/CD 配置选项
/// </summary>
public class CDConfigOptions : IConfigurableOptions
{
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    public string Owner { get; set; } = string.Empty;
    /// <summary>
    /// 仓库名
    /// </summary>
    public string Repo { get; set; } = string.Empty;
    /// <summary>
    /// 分支名
    /// </summary>
    public string Branch { get; set; } = string.Empty;
    /// <summary>
    /// 用户授权码
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;
    /// <summary>
    /// 更新间隔限制（分钟）0 不限制
    /// </summary>
    public int UpdateInterval { get; set; }

    /// <summary>
    /// 保留备份文件的数量, 0 不限制
    /// </summary>
    public int BackupCount { get; set; }

    /// <summary>
    /// 输出目录配置
    /// </summary>
    public string BackendOutput { get; set; } = string.Empty;
    /// <summary>
    /// 发布配置选项
    /// </summary>
    public PublishOptions Publish { get; set; } = new();
    /// <summary>
    /// 排除文件列表
    /// </summary>
    public List<string> ExcludeFiles { get; set; } = [];
}

/// <summary>
/// 编译发布配置选项
/// </summary>
public class PublishOptions
{
    /// <summary>
    /// 发布环境配置
    /// </summary>
    public string Configuration { get; set; } = string.Empty;
    /// <summary>
    /// 目标框架
    /// </summary>
    public string TargetFramework { get; set; } = string.Empty;
    /// <summary>
    /// 运行环境
    /// </summary>
    public string RuntimeIdentifier { get; set; } = string.Empty;
}