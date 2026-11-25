using System.Text.Json;

namespace Jero.Core;

/// <summary>
/// 数据库日志写入器
/// </summary>
public class DatabaseLoggingWriter : IDatabaseLoggingWriter, IDisposable
{
    private readonly IServiceScope _serviceScope;
    private readonly ISqlSugarClient _db;
    private readonly SysConfigService _sysConfigService; // 参数配置服务
    private readonly ILogger<DatabaseLoggingWriter> _logger; // 日志组件
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly IEventPublisher _eventPublisher;

    public DatabaseLoggingWriter(
        IServiceScopeFactory scopeFactory,
        IOptions<JsonOptions> jsonOptions)
    {
        _serviceScope = scopeFactory.CreateScope();
        //_db = _serviceScope.ServiceProvider.GetRequiredService<ISqlSugarClient>();
        _sysConfigService = _serviceScope.ServiceProvider.GetRequiredService<SysConfigService>();
        _logger = _serviceScope.ServiceProvider.GetRequiredService<ILogger<DatabaseLoggingWriter>>();
        _eventPublisher = _serviceScope.ServiceProvider.GetRequiredService<IEventPublisher>();
        _jsonSerializerOptions = jsonOptions.Value.JsonSerializerOptions;

        // 切换日志独立数据库
        _db = SqlSugarSetup.ITenant.IsAnyConnection(SqlSugarConst.LogConfigId)
            ? SqlSugarSetup.ITenant.GetConnectionScope(SqlSugarConst.LogConfigId)
            : SqlSugarSetup.ITenant.GetConnectionScope(SqlSugarConst.MainConfigId);
    }

    public async Task WriteAsync(LogMessage logMsg, bool flush)
    {
        var jsonStr = logMsg.Context?.Get("loggingMonitor")?.ToString();
        if (string.IsNullOrWhiteSpace(jsonStr))
        {
            await _db.Insertable(new SysLogOp
            {
                DisplayTitle = "自定义操作日志",
                LogDateTime = logMsg.LogDateTime,
                EventId = logMsg.EventId.Id,
                ThreadId = logMsg.ThreadId,
                TraceId = logMsg.TraceId,
                Exception = logMsg.Exception == null ? null : LoggingJsonSerializer.SerializeException(logMsg.Exception, _jsonSerializerOptions),
                Message = logMsg.Message,
                LogLevel = logMsg.LogLevel,
                Status = "200",
            }).ExecuteCommandAsync();
            return;
        }

        var loggingMonitor = LoggingJsonSerializer.Deserialize(jsonStr);
        // 记录数据校验日志
        if (loggingMonitor.Property("validation").HasValue() && !await _sysConfigService.GetConfigValue<bool>(ConfigConst.SysValidationLog)) return;

        // 获取当前操作者
        var claims = LoggingJsonSerializer.Claims(loggingMonitor);
        var account = claims.GetValueOrDefault(ClaimConst.Account, string.Empty);
        var realName = claims.GetValueOrDefault(ClaimConst.RealName, string.Empty);
        var tenantId = claims.GetValueOrDefault(ClaimConst.TenantId, string.Empty);
        var userId = claims.GetValueOrDefault(ClaimConst.UserId, string.Empty);

        // 优先获取 X-Forwarded-For 头部信息携带的IP地址（如nginx代理配置转发）
        var remoteIPv4 = loggingMonitor.Property("requestHeaders").Items()
            .FirstOrDefault(header => string.Equals(header.Property("key").Text(), "X-Forwarded-For", StringComparison.OrdinalIgnoreCase))
            .Property("value").Text();

        if (string.IsNullOrEmpty(remoteIPv4))
            remoteIPv4 = loggingMonitor.Property("remoteIPv4").Text();

        remoteIPv4 = remoteIPv4?.Split(',', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()?.Trim() ?? string.Empty;

        (string ipLocation, double? longitude, double? latitude) = CommonUtil.GetIpAddress(remoteIPv4);

        var browser = "";
        var os = "";
        if (loggingMonitor.Property("userAgent").Text() is { } userAgent)
        {
            var client = Parser.GetDefault().Parse(userAgent);
            browser = $"{client.UA.Family} {client.UA.Major}.{client.UA.Minor} / {client.Device.Family}";
            os = $"{client.OS.Family} {client.OS.Major} {client.OS.Minor}";
        }

        // 捕捉异常，否则会由于 unhandled exception 导致程序崩溃
        try
        {
            // 记录异常日志-发送邮件
            var returnInformation = loggingMonitor.Property("returnInformation");
            var exception = loggingMonitor.Property("exception");
            if (logMsg.Exception != null || exception.HasValue())
            {
                await _db.Insertable(new SysLogEx
                {
                    ControllerName = loggingMonitor.Property("controllerName").Text(),
                    ActionName = loggingMonitor.Property("actionTypeName").Text(),
                    DisplayTitle = loggingMonitor.Property("displayTitle").Text(),
                    Status = returnInformation.Property("httpStatusCode").Text(),
                    RemoteIp = remoteIPv4,
                    Location = ipLocation,
                    Longitude = (decimal?)longitude,
                    Latitude = (decimal?)latitude,
                    Browser = browser,
                    Os = os,
                    Elapsed = loggingMonitor.Property("timeOperationElapsedMilliseconds").Integer(),
                    LogDateTime = logMsg.LogDateTime,
                    Account = account,
                    RealName = realName,
                    HttpMethod = loggingMonitor.Property("httpMethod").Text(),
                    RequestUrl = loggingMonitor.Property("requestUrl").Text(),
                    RequestParam = LoggingJsonSerializer.RequestParam(loggingMonitor, _jsonSerializerOptions),
                    ReturnResult = returnInformation.HasValue() ? LoggingJsonSerializer.SerializePayload(returnInformation, _jsonSerializerOptions) : null,
                    EventId = logMsg.EventId.Id,
                    ThreadId = logMsg.ThreadId,
                    TraceId = logMsg.TraceId,
                    Exception = exception.HasValue()
                        ? LoggingJsonSerializer.SerializePayload(exception, _jsonSerializerOptions)
                        : LoggingJsonSerializer.SerializeException(logMsg.Exception!, _jsonSerializerOptions),
                    Message = logMsg.Message,
                    CreateUserId = string.IsNullOrWhiteSpace(userId) ? 0 : long.Parse(userId),
                    TenantId = string.IsNullOrWhiteSpace(tenantId) ? 0 : long.Parse(tenantId),
                    LogLevel = logMsg.LogLevel
                }).ExecuteCommandAsync();

                // 将异常日志发送到邮件
                if (await _sysConfigService.GetConfigValue<bool>(ConfigConst.SysErrorMail))
                {
                    await _eventPublisher.PublishAsync(CommonConst.SendErrorMail, (object?)logMsg.Exception ?? exception);
                }

                return;
            }

            // 记录访问日志-登录退出
            if (loggingMonitor.Property("actionName").Text() is "userInfo" or "logout")
            {
                await _db.Insertable(new SysLogVis
                {
                    ControllerName = loggingMonitor.Property("controllerName").Text(),
                    ActionName = loggingMonitor.Property("actionTypeName").Text(),
                    DisplayTitle = loggingMonitor.Property("displayTitle").Text(),
                    Status = returnInformation.Property("httpStatusCode").Text(),
                    RemoteIp = remoteIPv4,
                    Location = ipLocation,
                    Longitude = (decimal?)longitude,
                    Latitude = (decimal?)latitude,
                    Browser = browser,
                    Os = os,
                    Elapsed = loggingMonitor.Property("timeOperationElapsedMilliseconds").Integer(),
                    LogDateTime = logMsg.LogDateTime,
                    Account = account,
                    RealName = realName,
                    CreateUserId = string.IsNullOrWhiteSpace(userId) ? 0 : long.Parse(userId),
                    TenantId = string.IsNullOrWhiteSpace(tenantId) ? 0 : long.Parse(tenantId),
                    LogLevel = logMsg.LogLevel
                }).ExecuteCommandAsync();
                return;
            }

            // 记录操作日志
            if (!await _sysConfigService.GetConfigValue<bool>(ConfigConst.SysOpLog)) return;
            await _db.Insertable(new SysLogOp
            {
                ControllerName = loggingMonitor.Property("controllerName").Text(),
                ActionName = loggingMonitor.Property("actionTypeName").Text(),
                DisplayTitle = loggingMonitor.Property("displayTitle").Text(),
                Status = returnInformation.Property("httpStatusCode").Text(),
                RemoteIp = remoteIPv4,
                Location = ipLocation,
                Longitude = (decimal?)longitude,
                Latitude = (decimal?)latitude,
                Browser = browser,
                Os = os,
                Elapsed = loggingMonitor.Property("timeOperationElapsedMilliseconds").Integer(),
                LogDateTime = logMsg.LogDateTime,
                Account = account,
                RealName = realName,
                HttpMethod = loggingMonitor.Property("httpMethod").Text(),
                RequestUrl = loggingMonitor.Property("requestUrl").Text(),
                RequestParam = LoggingJsonSerializer.RequestParam(loggingMonitor, _jsonSerializerOptions),
                ReturnResult = returnInformation.HasValue() ? LoggingJsonSerializer.SerializePayload(returnInformation, _jsonSerializerOptions) : null,
                EventId = logMsg.EventId.Id,
                ThreadId = logMsg.ThreadId,
                TraceId = logMsg.TraceId,
                Message = logMsg.Message,
                CreateUserId = string.IsNullOrWhiteSpace(userId) ? 0 : long.Parse(userId),
                TenantId = string.IsNullOrWhiteSpace(tenantId) ? 0 : long.Parse(tenantId),
                LogLevel = logMsg.LogLevel
            }).ExecuteCommandAsync();

            await Task.Delay(50); // 延迟 0.05 秒写入数据库，有效减少高频写入数据库导致死锁问题
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "操作日志入库");
        }
    }

    public async Task WriteAsync(IReadOnlyList<LogMessage> batchLogMsgs, bool flush)
    {
        await ProcessBatchAsync(
            batchLogMsgs,
            logMsg => WriteAsync(logMsg, flush),
            (logMsg, exception) => _logger.LogError(
                exception,
                "日志批量入库时单条处理失败，EventId={EventId}，TraceId={TraceId}",
                logMsg.EventId.Id,
                logMsg.TraceId));
    }

    internal static async Task ProcessBatchAsync<T>(
        IReadOnlyList<T> items,
        Func<T, Task> processAsync,
        Action<T, Exception> onError)
    {
        foreach (var item in items)
        {
            try
            {
                await processAsync(item);
            }
            catch (Exception exception)
            {
                onError(item, exception);
            }
        }
    }

    /// <summary>
	/// 释放服务作用域
	/// </summary>
	public void Dispose()
    {
        _serviceScope.Dispose();
    }
}
