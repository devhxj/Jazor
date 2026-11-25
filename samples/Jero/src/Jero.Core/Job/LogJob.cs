namespace Jero.Core;

/// <summary>
/// 清理日志作业任务
/// </summary>
[JobDetail("job_log", Description = "清理操作日志", GroupName = "default", Concurrent = false)]
[Daily(TriggerId = "trigger_log", Description = "清理操作日志")]
public class LogJob : IJob
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger _logger;

    public LogJob(IServiceScopeFactory scopeFactory, ILoggerFactory loggerFactory)
    {
        _scopeFactory = scopeFactory;
        _logger = loggerFactory.CreateLogger(CommonConst.SysLogCategoryName);
    }

    public async Task ExecuteAsync(JobExecutingContext context, CancellationToken stoppingToken)
    {
        using var serviceScope = _scopeFactory.CreateScope();

        var db = serviceScope.ServiceProvider.GetRequiredService<ISqlSugarClient>().CopyNew();
        var sysConfigService = serviceScope.ServiceProvider.GetRequiredService<SysConfigService>();

        var daysAgo = await sysConfigService.GetConfigValue<int>(ConfigConst.SysLogRetentionDays); // 日志保留天数
        await db.Deleteable<SysLogVis>().Where(u => u.CreateTime < DateTime.Now.AddDays(-daysAgo)).ExecuteCommandAsync(stoppingToken); // 删除访问日志
        await db.Deleteable<SysLogOp>().Where(u => u.CreateTime < DateTime.Now.AddDays(-daysAgo)).ExecuteCommandAsync(stoppingToken); // 删除操作日志
        await db.Deleteable<SysLogDiff>().Where(u => u.CreateTime < DateTime.Now.AddDays(-daysAgo)).ExecuteCommandAsync(stoppingToken); // 删除差异日志

        string msg = $"【{DateTime.Now}】清理系统日志成功，删除 {daysAgo} 天前的日志数据！";
        var originColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(msg);
        Console.ForegroundColor = originColor;

        // 自定义日志
        _logger.LogInformation(msg);
    }
}