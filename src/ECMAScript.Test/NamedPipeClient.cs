using System.Buffers;
using System.IO.Pipes;

namespace ECMAScript.Test;

public record PipeMessage(int CommandType, byte[] Body);

public class NamedPipeClient(
	string pipeName,
	int bufferSize = 8192,
	int maxMessageSize = 1024 * 1024,
	TimeSpan? connectTimeout = null) : IDisposable
{
	private readonly string pipeName = pipeName;
	private readonly int bufferSize = bufferSize;
	private readonly int maxMessageSize = maxMessageSize;
	private readonly TimeSpan connectTimeout = connectTimeout ?? TimeSpan.FromSeconds(5);
	private NamedPipeClientStream? client;
	private bool disposed;

	public async Task<PipeMessage> RequestAsync(PipeMessage message,CancellationToken cancellationToken = default)
	{
		ObjectDisposedException.ThrowIf(disposed, nameof(NamedPipeClient));

		// 使用ArrayPool租用缓冲区
		var headerBuffer = ArrayPool<byte>.Shared.Rent(8);
		var bodyBuffer = ArrayPool<byte>.Shared.Rent(message.Body.Length);

		try
		{
			// 准备消息头
			BitConverter.TryWriteBytes(headerBuffer.AsSpan(0, 4), message.CommandType);
			BitConverter.TryWriteBytes(headerBuffer.AsSpan(4, 8), message.Body.Length);

			// 复制消息体到租用的缓冲区
			message.Body.CopyTo(bodyBuffer.AsSpan(0, message.Body.Length));

			// 创建新连接
			using (client = new NamedPipeClientStream(
				".",
				pipeName,
				PipeDirection.InOut,
				PipeOptions.Asynchronous | PipeOptions.WriteThrough))
			{
				// 连接服务器
				var connectTask = client.ConnectAsync(cancellationToken);
				var timeoutTask = Task.Delay(connectTimeout, cancellationToken);

				if (await Task.WhenAny(connectTask, timeoutTask) == timeoutTask)
					throw new TimeoutException($"连接管道超时: {pipeName}");

				await connectTask; // 确保连接成功

				// 发送消息头
				await client.WriteAsync(headerBuffer.AsMemory(0, 8), cancellationToken);
				await client.WriteAsync(bodyBuffer.AsMemory(0, message.Body.Length), cancellationToken);
				await client.FlushAsync(cancellationToken);

				// 读取响应头
				int bytesRead = await client.ReadAsync(headerBuffer.AsMemory(0, 8), cancellationToken);
				if (bytesRead < 8)
					throw new InvalidDataException("响应头不完整");

				int responseCommandType = BitConverter.ToInt32(headerBuffer, 0);
				int responseLength = BitConverter.ToInt32(headerBuffer, 4);

				if (responseLength < 0 || responseLength > maxMessageSize)
					throw new InvalidDataException($"无效的响应长度: {responseLength}");

				// 读取响应体
				byte[] responseBody = ArrayPool<byte>.Shared.Rent(responseLength);
				try
				{
					int totalRead = 0;
					while (totalRead < responseLength)
					{
						int read = await client.ReadAsync(responseBody.AsMemory(totalRead, 
							Math.Min(bufferSize, responseLength - totalRead)), cancellationToken);

						if (read == 0)
							throw new EndOfStreamException("响应体不完整");

						totalRead += read;
					}

					// 创建响应消息
					return new PipeMessage(responseCommandType, responseBody.AsSpan(0, responseLength).ToArray());
				}
				finally
				{
					ArrayPool<byte>.Shared.Return(responseBody);
				}
			}
		}
		finally
		{
			// 归还租用的缓冲区
			ArrayPool<byte>.Shared.Return(headerBuffer);
			ArrayPool<byte>.Shared.Return(bodyBuffer);
		}
	}

	public void Dispose()
	{
		if (!disposed)
		{
			client?.Dispose();
			disposed = true;
		}

		GC.SuppressFinalize(this);
	}
}

