namespace Jero.Core;

/// <summary>
/// Redis 消息扩展
/// </summary>
/// <typeparam name="T"></typeparam>
public class EventConsumer<T> : IDisposable
{
    /// <summary>
    ///
    /// </summary>
    private Task? _consumerTask;

    /// <summary>
    ///
    /// </summary>
    private CancellationTokenSource? _consumerCts;

    /// <summary>
    /// 消费者
    /// </summary>
    public IProducerConsumer<T> Consumer { get; }

    /// <summary>
    /// 消息回调
    /// </summary>
    public event EventHandler<T>? Received;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="consumer"></param>
    public EventConsumer(IProducerConsumer<T> consumer) => Consumer = consumer;

    /// <summary>
    /// 启动
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    public void Start()
    {
        if (Consumer is null)
        {
            throw new InvalidOperationException("Subscribe first using the Consumer.Subscribe() function");
        }
        if (_consumerTask != null)
        {
            return;
        }
        _consumerCts = new CancellationTokenSource();
        var ct = _consumerCts.Token;
        _consumerTask = Task.Factory.StartNew(async () =>
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    var cr = Consumer.TakeOne(10);
                    if (cr == null) continue;
                    Received?.Invoke(this, cr);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"消息消费异常: {ex.Message}");
                    await Task.Delay(1000); // 短暂等待后继续尝试
                }
            }
        }, ct, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    }

    /// <summary>
    /// 停止
    /// </summary>
    /// <returns></returns>
    public async Task Stop()
    {
        if (_consumerCts == null || _consumerTask == null) return;
        _consumerCts.Cancel();
        try
        {
            await _consumerTask;
        }
        finally
        {
            _consumerTask = null;
            _consumerCts = null;
        }
    }

    /// <summary>
    /// 释放
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// 释放
    /// </summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (_consumerTask != null)
            {
                Stop().Wait();
            }
        }
    }
}
