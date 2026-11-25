using Elastic.Clients.Elasticsearch;
using System.Text.Json;

namespace Jero.Core;

/// <summary>
/// ES日志写入器
/// </summary>
public class ElasticSearchLoggingWriter : IDatabaseLoggingWriter, IDisposable
{
    private readonly IServiceScope _serviceScope;
    private readonly ElasticsearchClient _esClient;
    private readonly SysConfigService _sysConfigService;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public ElasticSearchLoggingWriter(IServiceScopeFactory scopeFactory, IOptions<JsonOptions> jsonOptions)
    {
        _serviceScope = scopeFactory.CreateScope();
        _esClient = _serviceScope.ServiceProvider.GetRequiredService<ElasticSearchClientContainer>()!.Logging;
        _sysConfigService = _serviceScope.ServiceProvider.GetRequiredService<SysConfigService>();
        _jsonSerializerOptions = jsonOptions.Value.JsonSerializerOptions;
    }

    public async Task WriteAsync(LogMessage logMsg, bool flush)
    {
        // 是否启用操作日志
        var sysOpLogEnabled = await _sysConfigService.GetConfigValue<bool>(ConfigConst.SysOpLog);
        if (!sysOpLogEnabled) return;

        if (CreateOperationLog(logMsg) is { } sysLogOp)
            await _esClient.IndexAsync(sysLogOp);
    }

    public async Task WriteAsync(IReadOnlyList<LogMessage> batchLogMsgs, bool flush)
    {
        // 是否启用操作日志
        var sysOpLogEnabled = await _sysConfigService.GetConfigValue<bool>(ConfigConst.SysOpLog);
        if (!sysOpLogEnabled) return;

        var list = new List<SysLogOp>();
        foreach (var logMsg in batchLogMsgs)
        {
            if (CreateOperationLog(logMsg) is { } sysLogOp)
                list.Add(sysLogOp);
        }

        if (list.Count > 0)
        {
            await _esClient.IndexManyAsync(list);
        }
    }

    private SysLogOp? CreateOperationLog(LogMessage logMsg)
    {
        var jsonStr = logMsg.Context?.Get("loggingMonitor")?.ToString();
        if (string.IsNullOrWhiteSpace(jsonStr)) return null;

        var loggingMonitor = LoggingJsonSerializer.Deserialize(jsonStr);

        // 不记录登录退出日志
        if (loggingMonitor.Property("actionName").Text() is "userInfo" or "logout")
            return null;

        // 获取当前操作者
        var claims = LoggingJsonSerializer.Claims(loggingMonitor);
        var account = claims.GetValueOrDefault(ClaimConst.Account, string.Empty);
        var realName = claims.GetValueOrDefault(ClaimConst.RealName, string.Empty);
        var tenantId = claims.GetValueOrDefault(ClaimConst.TenantId, string.Empty);
        var userId = claims.GetValueOrDefault(ClaimConst.UserId, string.Empty);
        var remoteIPv4 = loggingMonitor.Property("remoteIPv4").Text() ?? string.Empty;
        (string ipLocation, double? longitude, double? latitude) = CommonUtil.GetIpAddress(remoteIPv4);

        var returnInformation = loggingMonitor.Property("returnInformation");
        var exception = loggingMonitor.Property("exception");
        return new SysLogOp
        {
            Id = DateTime.Now.Ticks,
            ControllerName = loggingMonitor.Property("controllerName").Text(),
            ActionName = loggingMonitor.Property("actionTypeName").Text(),
            DisplayTitle = loggingMonitor.Property("displayTitle").Text(),
            Status = returnInformation.Property("httpStatusCode").Text(),
            RemoteIp = remoteIPv4,
            Location = ipLocation,
            Longitude = (decimal?)longitude,
            Latitude = (decimal?)latitude,
            Browser = loggingMonitor.Property("userAgent").Text(),
            Os = loggingMonitor.Property("osDescription").Text() + " " + loggingMonitor.Property("osArchitecture").Text(),
            Elapsed = loggingMonitor.Property("timeOperationElapsedMilliseconds").Integer(),
            LogDateTime = logMsg.LogDateTime,
            Account = account,
            RealName = realName,
            HttpMethod = loggingMonitor.Property("httpMethod").Text(),
            RequestUrl = loggingMonitor.Property("requestUrl").Text(),
            RequestParam = LoggingJsonSerializer.RequestParam(loggingMonitor, _jsonSerializerOptions),
            ReturnResult = LoggingJsonSerializer.SerializePayload(returnInformation, _jsonSerializerOptions),
            EventId = logMsg.EventId.Id,
            ThreadId = logMsg.ThreadId,
            TraceId = logMsg.TraceId,
            Exception = exception.HasValue() ? LoggingJsonSerializer.SerializePayload(exception, _jsonSerializerOptions) : null,
            Message = logMsg.Message,
            CreateUserId = string.IsNullOrWhiteSpace(userId) ? 0 : long.Parse(userId),
            TenantId = string.IsNullOrWhiteSpace(tenantId) ? 0 : long.Parse(tenantId)
        };
    }

	/// <summary>
	/// 释放服务作用域
	/// </summary>
	public void Dispose()
	{
		_serviceScope.Dispose();
	}
}
