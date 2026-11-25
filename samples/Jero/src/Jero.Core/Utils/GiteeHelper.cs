namespace Jero.Core;

/// <summary>
/// Gitee接口帮助类
/// </summary>
public class GiteeHelper
{
    private const string BaseUrl = "https://gitee.com/api/v5/repos/";
    private static readonly HttpClient Client = new();

    /// <summary>
    /// 下载仓库 zip
    /// </summary>
    /// <remarks>https://gitee.com/api/v5/swagger#/getV5ReposOwnerRepoZipball</remarks>
    /// <returns></returns>
    public static async Task<Stream> DownloadRepoZip(string owner, string repo, string? accessToken = null, string? @ref = null)
    {
        if (string.IsNullOrWhiteSpace(owner)) throw Oops.Bah($"参数 {nameof(owner)} 不能为空");
        if (string.IsNullOrWhiteSpace(repo)) throw Oops.Bah($"参数 {nameof(repo)} 不能为空");
        var query = BuilderQueryString(new
        {
            access_token = accessToken,
            @ref
        });
        return await Client.GetStreamAsync($"{BaseUrl}{owner}/{repo}/zipball?{query}");
    }

    /// <summary>
    /// 构建Query参数
    /// </summary>
    /// <returns></returns>
    private static string BuilderQueryString(object? obj)
    {
        if (obj == null) return string.Empty;
        var query = HttpUtility.ParseQueryString(string.Empty);
        foreach (var prop in obj.GetType().GetProperties())
        {
            var val = prop.GetValue(obj);
            if (val == null) continue;

            // 以元组形式校验参数集
            var name = prop.Name.Trim('@');
            if (val is Tuple<object, string> { Item1: not null } tuple)
            {
                if (!tuple.Item2.Split(",").Any(x => x.Trim().Equals(tuple.Item1))) throw Oops.Oh($"参数 {name} 的值只能为：{tuple.Item2}");
                query[name] = tuple.Item1.ToString();
                continue;
            }
            query[name] = val.ToString();
        }
        return query.ToString() ?? string.Empty;
    }
}
