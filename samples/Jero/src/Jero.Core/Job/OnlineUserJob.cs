


namespace Jero.Core;

/// <summary>
/// 清理在线用户作业任务
/// </summary>
[JobDetail("job_onlineUser", Description = "清理在线用户", GroupName = "default", Concurrent = false)]
[PeriodSeconds(1, TriggerId = "trigger_onlineUser", Description = "清理在线用户", MaxNumberOfRuns = 1, RunOnStart = true)]
public class OnlineUserJob : IJob
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger _logger;

    public OnlineUserJob(IServiceScopeFactory scopeFactory, ILoggerFactory loggerFactory)
    {
        _scopeFactory = scopeFactory;
        _logger = loggerFactory.CreateLogger(CommonConst.SysLogCategoryName);
    }

    public async Task ExecuteAsync(JobExecutingContext context, CancellationToken stoppingToken)
    {
        using var serviceScope = _scopeFactory.CreateScope();

        var db = serviceScope.ServiceProvider.GetRequiredService<ISqlSugarClient>().CopyNew();
        await db.Deleteable<SysOnlineUser>().ExecuteCommandAsync(stoppingToken);

        string msg = $"【{DateTime.Now}】清理在线用户成功！服务已重启...";
        var originColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(msg);
        Console.ForegroundColor = originColor;

        // 自定义日志
        _logger.LogInformation(msg);

        // 缓存租户列表
        await serviceScope.ServiceProvider.GetRequiredService<SysTenantService>().CacheTenant();

        // 加载动态程序集
        await serviceScope.ServiceProvider.GetRequiredService<SysPluginService>().LoadAllAssembly();
        msg = $"【启动任务】加载动态程序集 {DateTime.Now}";
        Console.WriteLine(msg);
    }
}