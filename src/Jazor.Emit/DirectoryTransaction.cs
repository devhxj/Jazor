namespace Jazor.Emit;

/// <summary>
/// 提交事务里的目录重命名助手。Windows 上 <c>Directory.Move</c> 要求目录树内没有打开的句柄，
/// 而刚写完并校验过哈希的文件常被 Defender / 索引器短暂持有，导致偶发
/// <c>IOException: Access to the path ... is denied</c>；并行构建时尤其明显。
/// 重试只吸收这类瞬时句柄占用，不改变永久失败的传播语义。
/// </summary>
internal static class DirectoryTransaction
{
    private const int Attempts = 5;
    private const int RetryDelayMilliseconds = 100;

    internal static void Move(string source, string destination)
    {
        for (var attempt = 0; ; attempt++)
        {
            try
            {
                Directory.Move(source, destination);
                return;
            }
            catch (IOException) when (attempt < Attempts - 1)
            {
                Thread.Sleep(RetryDelayMilliseconds);
            }
            catch (UnauthorizedAccessException) when (attempt < Attempts - 1)
            {
                Thread.Sleep(RetryDelayMilliseconds);
            }
        }
    }
}
