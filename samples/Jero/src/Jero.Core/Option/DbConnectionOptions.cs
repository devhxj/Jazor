namespace Jero.Core;

/// <summary>
/// 数据库配置选项
/// </summary>
public sealed class DbConnectionOptions : IConfigurableOptions<DbConnectionOptions>
{
    /// <summary>
    /// 启用控制台打印SQL
    /// </summary>
    public bool EnableConsoleSql { get; set; }

    /// <summary>
    /// 超级管理员是否忽略逻辑删除过滤器
    /// </summary>
    public bool SuperAdminIgnoreIDeletedFilter { get; set; }

    /// <summary>
    /// 数据库集合
    /// </summary>
    public List<DbConnectionConfig> ConnectionConfigs { get; set; } = [];
    public void PostConfigure(DbConnectionOptions options, IConfiguration configuration)
    {
        foreach (var dbConfig in options.ConnectionConfigs)
        {
            if (dbConfig.ConfigId == null || string.IsNullOrWhiteSpace(dbConfig.ConfigId.ToString()))
                dbConfig.ConfigId = SqlSugarConst.MainConfigId;
        }
    }
}

/// <summary>
/// 数据库连接配置
/// </summary>
public sealed class DbConnectionConfig : ConnectionConfig
{
    /// <summary>
    /// 数据库名称
    /// </summary>
    public string DbNickName { get; set; } = string.Empty;
    /// <summary>
    /// 数据库配置
    /// </summary>
    public DbSettings DbSettings { get; set; } = new();
    /// <summary>
    /// 表配置
    /// </summary>
    public TableSettings TableSettings { get; set; } = new();
    /// <summary>
    /// 种子配置
    /// </summary>
    public SeedSettings SeedSettings { get; set; } = new();
    /// <summary>
    /// 隔离方式
    /// </summary>
    public TenantTypeEnum TenantType { get; set; } = TenantTypeEnum.Id;

    /// <summary>
    /// 数据库存储目录（仅SqlServer支持指定目录创建）
    /// </summary>
    public string DatabaseDirectory { get; set; } = string.Empty;
}

/// <summary>
/// 数据库配置
/// </summary>
public sealed class DbSettings
{
    /// <summary>
    /// 启用库表初始化
    /// </summary>
    public bool EnableInitDb { get; set; }

    /// <summary>
    /// 启用视图初始化
    /// </summary>
    public bool EnableInitView { get; set; }

    /// <summary>
    /// 启用库表差异日志
    /// </summary>
    public bool EnableDiffLog { get; set; }

    /// <summary>
    /// 启用驼峰转下划线
    /// </summary>
    public bool EnableUnderLine { get; set; }

    /// <summary>
    /// 启用数据库连接串加密策略
    /// </summary>
    public bool EnableConnStringEncrypt { get; set; }
}

/// <summary>
/// 表配置
/// </summary>
public sealed class TableSettings
{
    /// <summary>
    /// 启用表初始化
    /// </summary>
    public bool EnableInitTable { get; set; }

    /// <summary>
    /// 启用表增量更新
    /// </summary>
    public bool EnableIncreTable { get; set; }
}

/// <summary>
/// 种子配置
/// </summary>
public sealed class SeedSettings
{
    /// <summary>
    /// 启用种子初始化
    /// </summary>
    public bool EnableInitSeed { get; set; }

    /// <summary>
    /// 启用种子增量更新
    /// </summary>
    public bool EnableIncreSeed { get; set; }
}