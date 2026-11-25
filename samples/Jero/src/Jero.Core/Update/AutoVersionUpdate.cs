


#if NET10_0_OR_GREATER

using Microsoft.AspNetCore.Builder;

namespace Jero.Core.Update;

/// <summary>
/// 自动版本更新中间件拓展
/// </summary>
/// <remarks>
/// 使用方法
/// 1.在 Jero.Web 的 Startup.cs 中的 Configure 方法中调用 app.UseAutoVersionUpdate()。
/// 2.在入口项目 Jero.Web 的根目录下创建一个名为 UpdateScripts 的文件夹，并在其中放置 .sql 后缀的脚本文件
/// 3.脚本文件命名格式为版本号，例如 1.0.0.sql、1.0.1.sql 等，版本号应符合语义化版本规范。
/// 4.脚本的属性：复制到输出目录，设置为：始终复制。
/// 5.设置主节点的 Jero.Application 的 Configuration/App.json 的 WorkerId 为 1。
/// 6.设置入口项目 Jero.Web.csproj 的 Version。
/// ==================================================
/// 更新新版本时
/// 1.需在 UpdateScripts 文件夹中添加新的脚本文件，脚本文件名应为新版本号。
/// 2.设置入口项目 Jero.Web.csproj 的 Version
/// </remarks>
[SuppressSniffer]
public static class AutoVersionUpdate
{
    /// <summary>
    /// 使用自动版本更新中间件
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseAutoVersionUpdate(this IApplicationBuilder app)
    {
        AutoVersionUpdateLog.Info("AutoVersionUpdate 中间件运行");

        var snowIdOpt = App.GetConfig<SnowIdOptions>("SnowId", true);
        if (snowIdOpt.WorkerId != 1)
        {
            AutoVersionUpdateLog.Handle("非主节点，不执行脚本");
            return app;
        }

        var currentVersion = GetEntryAssemblyCurrentVersion();
        AutoVersionUpdateLog.Handle($"当前版本：{currentVersion}");

        var historyVersionInfo = GetEntryAssemblyHistoryVersionInfo();
        var historyVersion = historyVersionInfo.Version;
        var historyDate = historyVersionInfo.Date;
        var historyIsRunScript = historyVersionInfo.IsRunScript;

        AutoVersionUpdateLog.Handle($"历史版本：{historyVersion},更新时间：{historyDate}，是否已执行{historyIsRunScript}");

        // 历史版本为空、版本号相同，不执行脚本
        if (historyVersion == string.Empty)
        {
            AutoVersionUpdateLog.Handle("历史版本为空，默认为最新版本，不执行脚本");

            // 保存当前版本信息
            SetEntryAssemblyCurrentVersion(currentVersion, true);

            return app;
        }
        else if (AutoVersionUpdateVersionComparer.Compare(currentVersion, historyVersion) <= 0 && historyIsRunScript)
        {
            AutoVersionUpdateLog.Handle("当前版本号不高于历史版本号，且已执行过脚本，不再执行");

            // 保存当前版本信息
            SetEntryAssemblyCurrentVersion(currentVersion, true);

            return app;
        }
        else
        {
            AutoVersionUpdateLog.Handle("当前版本号高于历史版本号，或版本号相同但未执行过脚本，开始执行脚本");

            var scriptSqlVersions = GetScriptSqlVersions();

            // 若不存在当前版本及以前版本的待执行脚本，则只保存当前版本信息，不执行脚本
            if (scriptSqlVersions.Count == 0 ||
                scriptSqlVersions.All(s => AutoVersionUpdateVersionComparer.Compare(s.Version, currentVersion) > 0 ||
                    AutoVersionUpdateVersionComparer.Compare(s.Version, historyVersion) < 0 ||
                    (AutoVersionUpdateVersionComparer.Compare(s.Version, historyVersion) == 0 && historyIsRunScript)))
            {
                AutoVersionUpdateLog.Handle("不存在当前版本及以前版本的待执行脚本，只保存当前版本信息，不执行脚本");

                // 保存当前版本信息
                SetEntryAssemblyCurrentVersion(currentVersion, false);

                return app;
            }

            // 执行脚本
            foreach (var sqlFileInfo in scriptSqlVersions)
            {
                var sqlVersion = sqlFileInfo.Version;
                var compareWithHistoryVersion = AutoVersionUpdateVersionComparer.Compare(sqlVersion, historyVersion);

                // 只执行大于历史版本的脚本，或者当前版本但未执行过
                if (compareWithHistoryVersion < 0)
                {
                    AutoVersionUpdateLog.Handle($"版本{sqlVersion}低于历史版本，跳过");
                    continue;
                }
                if (compareWithHistoryVersion == 0 && historyIsRunScript)
                {
                    AutoVersionUpdateLog.Handle($"版本{sqlVersion}等于历史版本，且已执行过脚本，跳过");
                    continue;
                }
                if (AutoVersionUpdateVersionComparer.Compare(sqlVersion, currentVersion) > 0)
                {
                    AutoVersionUpdateLog.Handle($"版本{sqlVersion}高于当前版本，跳过");
                    continue;
                }

                // 执行脚本
                var sql = File.ReadAllText(sqlFileInfo.FilePath);
                if (!string.IsNullOrWhiteSpace(sql))
                {
                    AutoVersionUpdateLog.Handle($"执行版本{sqlVersion}脚本");

                    HandleSqlScript(app, sql, sqlVersion);
                }
                else
                {
                    AutoVersionUpdateLog.Handle($"版本{sqlVersion}脚本为空，跳过");
                }
            }
        }

        AutoVersionUpdateLog.Success("AutoVersionUpdate 中间件结束");

        return app;
    }

    #region 辅助方法

    /// <summary>
    /// 获取入口程序集当前版本信息
    /// </summary>
    /// <returns></returns>
    private static string GetEntryAssemblyCurrentVersion()
    {
        var entryAssemblyVersion = Assembly.GetEntryAssembly()?.GetName().Version;
        if (entryAssemblyVersion is null)
        {
            return "0.0.0";
        }

        var build = Math.Max(0, entryAssemblyVersion.Build);
        return $"{entryAssemblyVersion.Major}.{entryAssemblyVersion.Minor}.{build}";
    }

    /// <summary>
    /// 设置入口程序集当前版本信息
    /// </summary>
    /// <param name="version"></param>
    /// <param name="isRunScript"></param>
    private static void SetEntryAssemblyCurrentVersion(string version, bool isRunScript)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "version.txt");
        var now = DateTime.Now;
        File.WriteAllText(path, $"{version}^{now:yyyy-MM-dd HH:mm:ss}^{isRunScript}");
    }

    /// <summary>
    /// 获取入口程序集上一次运行版本信息
    /// </summary>
    /// <returns></returns>
    private static HistoryVersionInfo GetEntryAssemblyHistoryVersionInfo()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "version.txt");

        // 检查文件是否存在
        if (File.Exists(path))
        {
            // 文件存在时读取内容
            var info = File.ReadAllText(path);

            if (info.Contains('^'))
            {
                var parts = info.Split('^');
                var version = parts.Length > 0 ? parts[0].ToString() : string.Empty;
                var date = parts.Length > 1 ? parts[1] : string.Empty;
                var isRunScript = parts.Length > 2 ? parts[2].ToBoolean() : false;

                return new HistoryVersionInfo(version, date, isRunScript);
            }
        }

        // 文件不存在或内容格式不正确时返回默认值
        return new HistoryVersionInfo(string.Empty, string.Empty, false);
    }

    /// <summary>
    /// 获取程序目录下的脚本 SQL 文件版本
    /// </summary>
    /// <returns></returns>
    private static List<SqlFileInfo> GetScriptSqlVersions()
    {
        // 获取所有脚本文件
        var path = Path.Combine(AppContext.BaseDirectory, "UpdateScripts");
        if (!Directory.Exists(path))
        {
            AutoVersionUpdateLog.Handle("UpdateScripts 目录不存在，跳过脚本扫描");
            return [];
        }

        var scriptFiles = Directory.GetFiles(path, "*.sql").ToList();

        var sqlVersions = scriptFiles
            .Select(s => new SqlFileInfo(Path.GetFileNameWithoutExtension(s), s))
            .OrderBy(s => s.Version, Comparer<string>.Create(AutoVersionUpdateVersionComparer.Compare)).ToList();
        return sqlVersions;
    }

    /// <summary>
    /// 保存当前版本信息
    /// </summary>
    /// <param name="app"></param>
    /// <param name="sql"></param>
    /// <param name="sqlVersion"></param>
    private static void HandleSqlScript(IApplicationBuilder app, string sql, string sqlVersion)
    {
        using var scope = App.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ISqlSugarClient>();

        var isSuccess = false;

        try
        {
            // 开启事务
            dbContext.Ado.BeginTran();
            dbContext.Ado.ExecuteCommand(sql);
            dbContext.Ado.CommitTran();
            isSuccess = true;
        }
        catch (Exception ex)
        {
            dbContext.Ado.RollbackTran();
            AutoVersionUpdateLog.Error($"AutoVersionUpdate 执行 SQL 脚本出错，版本：{sqlVersion}，错误：{ex.Message}");
        }
        finally
        {
            if (isSuccess)
            {
                // 保存当前版本信息
                SetEntryAssemblyCurrentVersion(sqlVersion, true);
            }
        }
    }

    #endregion 辅助方法
}

public record SqlFileInfo(string Version, string FilePath);
public record HistoryVersionInfo(string Version, string Date, bool IsRunScript);

file static class AutoVersionUpdateLog
{
    public static void Info(string message)
    {
        Write(LogLevel.Information, message);
    }

    public static void Handle(string message)
    {
        Write(LogLevel.Information, message);
    }

    public static void Success(string message)
    {
        Write(LogLevel.Information, message);
    }

    public static void Error(string message)
    {
        Write(LogLevel.Error, message);
    }

    private static void Write(LogLevel logLevel, string message)
    {
        try
        {
            var loggerFactory = App.GetService<ILoggerFactory>();
            var logger = loggerFactory?.CreateLogger("AutoVersionUpdate");
            if (logger is not null)
            {
                logger.Log(logLevel, "{Message}", message);
                return;
            }
        }
        catch
        {
            // 应用服务提供器尚未就绪时回退到控制台。
        }

        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{logLevel}] {message}");
    }
}

#endif // NET10_0_OR_GREATER
