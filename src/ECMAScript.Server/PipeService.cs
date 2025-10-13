using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Buffers;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;

namespace ECMAScript.Server;

public class NamedPipeServerBackgroundService : BackgroundService
{
	private readonly ILogger<NamedPipeServerBackgroundService> _logger;
	private readonly string _pipeName;
	private readonly int _maxConnections;
	private readonly int _bufferSize;
	private readonly int _maxMessageSize;
	private readonly Channel<NamedPipeServerStream> _channel;
	private readonly CancellationTokenSource _cts = new();
	private record PipeMessage(int CommandType, byte[] Body);
	private record CommandType0Request(string Name, string DotExt, string FullPath, string Content);

	public NamedPipeServerBackgroundService(
		ILogger<NamedPipeServerBackgroundService> logger,
		string pipeName = "ECMAScript",
		int maxConnections = 10,
		int bufferSize = 8192,
		int maxMessageSize = 1024 * 1024)
	{
		_logger = logger;
		_pipeName = pipeName;
		_maxConnections = maxConnections;
		_bufferSize = bufferSize;
		_maxMessageSize = maxMessageSize;
		_channel = Channel.CreateBounded<NamedPipeServerStream>(new BoundedChannelOptions(maxConnections)
		{
			FullMode = BoundedChannelFullMode.Wait
		});
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		// 启动连接处理器
		var processorTask = ProcessConnectionsAsync(stoppingToken);

		try
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				var server = new NamedPipeServerStream(
					_pipeName,
					PipeDirection.InOut,
					NamedPipeServerStream.MaxAllowedServerInstances,
					PipeTransmissionMode.Byte,
					PipeOptions.Asynchronous);

				try
				{
					await server.WaitForConnectionAsync(stoppingToken);
					await _channel.Writer.WriteAsync(server, stoppingToken);
				}
				catch (OperationCanceledException)
				{
					server.Dispose();
					break;
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Error accepting connection");
					server.Dispose();
				}
			}
		}
		finally
		{
			_cts.Cancel();
			await processorTask;
		}
	}

	public override async Task StopAsync(CancellationToken cancellationToken)
	{
		_cts.Cancel();
		await base.StopAsync(cancellationToken);
	}

	private async Task ProcessConnectionsAsync(CancellationToken stoppingToken)
	{
		var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, _cts.Token);
		var tasks = new List<Task>(_maxConnections);

		try
		{
			await foreach (var server in _channel.Reader.ReadAllAsync(linkedCts.Token))
			{
				if (tasks.Count >= _maxConnections)
				{
					var completedTask = await Task.WhenAny(tasks);
					tasks.Remove(completedTask);
				}

				var task = HandleAsync(server, linkedCts.Token);
				tasks.Add(task);
			}
		}
		finally
		{
			await Task.WhenAll(tasks);
		}
	}

	private async Task HandleAsync(NamedPipeServerStream ss, CancellationToken stoppingToken)
	{
		try
		{
			using (ss)
			{
				var buffer = new byte[_bufferSize];
				var ms = new MemoryStream();

				while (!stoppingToken.IsCancellationRequested && ss.IsConnected)
				{
					// 读取消息头 (4字节长度 + 4字节命令类型)
					var headerBytes = await ReadHeaderAsync(ss, 8, stoppingToken);
					if (headerBytes.Length == 0) 
						break;

					var commandType = BitConverter.ToInt32(headerBytes, 0);
					var messageLength = BitConverter.ToInt32(headerBytes, 4);

					// 验证消息长度
					if (messageLength <= 0 || messageLength > _maxMessageSize)
					{
						_logger.LogWarning("Invalid message length");
						break;
					}

					// 读取消息体
					ms.SetLength(0);
					var remainingBytes = messageLength;
					while (remainingBytes > 0)
					{
						var bytesToRead = Math.Min(remainingBytes, _bufferSize);
						var bytesRead = await ss.ReadAsync(buffer.AsMemory(0, bytesToRead), stoppingToken);
						if (bytesRead == 0) break;

						await ms.WriteAsync(buffer.AsMemory(0, bytesRead), stoppingToken);
						remainingBytes -= bytesRead;
					}

					if (remainingBytes > 0) 
						break;

					// 处理消息
					PipeMessage response;
					try
					{
						response = commandType switch
						{
							1 => await ProcessCommandType0Async(ms, stoppingToken),
							_ => new PipeMessage(-1, Encoding.UTF8.GetBytes($"Command type {commandType} not supported"))
						};
					}
					catch (Exception ex)
					{
						response = new PipeMessage(-1, Encoding.UTF8.GetBytes(ex.Message));
					}

					// 发送响应
					await ResponseAsync(ss, response, stoppingToken);
				}
			}
		}
		catch (OperationCanceledException)
		{
			// 正常取消
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error handling connection");
		}
	}

	private static async Task<byte[]> ReadHeaderAsync(NamedPipeServerStream ss, int count, CancellationToken stoppingToken)
	{
		var buffer = new byte[count];
		var totalRead = 0;

		while (totalRead < count)
		{
			var bytesRead = await ss.ReadAsync(buffer.AsMemory(totalRead, count - totalRead), stoppingToken);
			if (bytesRead == 0)
				break;

			totalRead += bytesRead;
		}

		return totalRead == count ? buffer : [];
	}

	private static async Task<PipeMessage> ProcessCommandType0Async(MemoryStream ms, CancellationToken stoppingToken)
	{
		ms.Position = 0;
		var request = JsonSerializer.Deserialize<CommandType0Request>(ms)
			?? throw new InvalidOperationException("Invalid request format");

		var directory = Path.GetDirectoryName(request.FullPath);
		if (!string.IsNullOrEmpty(directory))
			Directory.CreateDirectory(directory);

		await File.WriteAllTextAsync(request.FullPath, request.Content, stoppingToken);
		return new PipeMessage(0, Encoding.UTF8.GetBytes("Success"));
	}

	private async Task ResponseAsync(NamedPipeServerStream ss, PipeMessage message, CancellationToken stoppingToken)
	{
		// 响应格式: [4字节命令类型][4字节总长度][消息体]
		var totalLength = 8 + message.Body.Length; // 4字节命令类型 + 4字节长度 + 消息体长度
		var buffer = ArrayPool<byte>.Shared.Rent(totalLength);

		try
		{
			BitConverter.TryWriteBytes(new Span<byte>(buffer, 0, 4), message.CommandType);
			BitConverter.TryWriteBytes(new Span<byte>(buffer, 4, 4), message.Body.Length);
			message.Body.AsSpan().CopyTo(new Span<byte>(buffer, 8, message.Body.Length));
			await ss.WriteAsync(new ReadOnlyMemory<byte>(buffer, 0, totalLength), stoppingToken);
		}
		catch (OperationCanceledException)
		{
			// 正常取消，无需处理
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error sending message");
			throw;
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(buffer);
		}
	}
}

