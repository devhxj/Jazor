namespace Jero.Core.Service;

/// <summary>
/// 作业执行监视器
/// </summary>
public class JobMonitor : IJobMonitor
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<JobMonitor> _logger;

    public JobMonitor(IServiceScopeFactory serviceScopeFactory, IEventPublisher eventPublisher, ILogger<JobMonitor> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public Task OnExecutingAsync(JobExecutingContext context, CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }

    public async Task OnExecutedAsync(JobExecutedContext context, CancellationToken stoppingToken)
    {
        if (context.Exception == null) return;

        var exception = $"定时任务【{context.Trigger.Description}】错误：{context.Exception}";
        // 将作业异常信息记录到本地
        _logger.LogError(exception);

        using var scope = _serviceScopeFactory.CreateScope();
        var sysConfigService = scope.ServiceProvider.GetRequiredService<SysConfigService>();
        if (await sysConfigService.GetConfigValue<bool>(ConfigConst.SysErrorMail))
        {
            // 将作业异常信息发送到邮件
            await _eventPublisher.PublishAsync(CommonConst.SendErrorMail, exception, stoppingToken);
        }
    }
}