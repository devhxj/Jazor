namespace Jero.Core;

/// <summary>
/// 事件执行器-超时控制、失败重试熔断等等
/// </summary>
public class RetryEventHandlerExecutor : IEventHandlerExecutor
{
	public async Task ExecuteAsync(EventHandlerExecutingContext context, Func<EventHandlerExecutingContext, Task> handler, CancellationToken cancellationToken)
	{
		var eventSubscribeAttribute = context.Attribute;
		//// 判断是否自定义了重试失败回调服务
		//var fallbackPolicyService = eventSubscribeAttribute?.FallbackPolicy == null
		//    ? null
		//    : App.GetRequiredService(eventSubscribeAttribute.FallbackPolicy) as IEventFallbackPolicy;

		await Retry.InvokeAsync(async () =>
		{
			try
			{
				await handler(context);
			}
			catch (Exception ex)
			{
				Log.Error($"Invoke EventHandler {context.Source.EventId} Error", ex);
				throw;
			}
		}
		, eventSubscribeAttribute?.NumRetries ?? 0
		, eventSubscribeAttribute?.RetryTimeout ?? 1000
		, exceptionTypes: eventSubscribeAttribute?.ExceptionTypes
		//, fallbackPolicy: fallbackPolicyService == null ? null : async (Exception ex) => { await fallbackPolicyService.CallbackAsync(context, ex); }
		, retryAction: (total, times) =>
		{
			// 输出重试日志
			Log.Warning($"Retrying {times}/{total} times for  EventHandler {context.Source.EventId}");
		});
	}
}