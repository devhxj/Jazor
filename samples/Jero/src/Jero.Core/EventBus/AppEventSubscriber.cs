namespace Jero.Core;

/// <summary>
/// 事件订阅
/// </summary>
public class AppEventSubscriber(IServiceScopeFactory scopeFactory) : IEventSubscriber, ISingleton, IDisposable
{
    private static readonly ISugarQueryable<SysTenant> SysTenantQueryable = App.GetService<ISqlSugarClient>().Queryable<SysTenant>();
    private readonly IServiceScope _serviceScope = scopeFactory.CreateScope();

	/// <summary>
	/// 增加异常日志
	/// </summary>
	/// <param name="context"></param>
	/// <returns></returns>
	[EventSubscribe(CommonConst.AddExLog)]
    public static async Task CreateExLog(EventHandlerExecutingContext context)
    {
        // 切换日志独立数据库
        var db = SqlSugarSetup.ITenant.IsAnyConnection(SqlSugarConst.LogConfigId)
            ? SqlSugarSetup.ITenant.GetConnectionScope(SqlSugarConst.LogConfigId)
            : SqlSugarSetup.ITenant.GetConnectionScope(SqlSugarConst.MainConfigId);
        await db.CopyNew().Insertable((SysLogEx)context.Source.Payload).ExecuteCommandAsync();
    }

    /// <summary>
    /// 发送异常邮件
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    [EventSubscribe(CommonConst.SendErrorMail)]
    public async Task SendOrderErrorMail(EventHandlerExecutingContext context)
    {
		long.TryParse(App.HttpContext?.User.FindFirst(ClaimConst.TenantId)?.Value, out var tenantId);
        var tenant = await SysTenantQueryable.FirstAsync(t => t.Id == tenantId);
        var title = $"{tenant?.Title} 系统异常";
        var payload = context.Source.Payload is Exception exception
            ? LoggingJsonSerializer.GetExceptionDetails(exception)
            : context.Source.Payload;
        await _serviceScope.ServiceProvider.GetRequiredService<SysEmailService>().SendEmail(JSON.Serialize(payload), title);
    }

    /// <summary>
    /// 释放服务作用域
    /// </summary>
    public void Dispose()
    {
        _serviceScope.Dispose();
    }
}
