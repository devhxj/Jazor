


using System.IO.Compression;
using System.Net;
using System.Security.Cryptography;
using System.Text.Json.Nodes;
namespace Jero.Core.Service;

/// <summary>
/// 系统更新管理服务 🧩
/// </summary>
[ApiDescriptionSettings(Order = 390)]
public class SysUpdateService : IDynamicApiController, ITransient
{
    private static readonly string[] RuntimeMutableOutputPatterns =
    [
        "Jero.db",
        "Jero.db-shm",
        "Jero.db-wal",
        "Jero.Log.db",
        "Jero.Log.db-shm",
        "Jero.Log.db-wal",
        "wwwroot\\avatar",
        "wwwroot\\upload",
    ];

    private readonly SysCacheService _sysCacheService;
    private readonly CDConfigOptions _cdConfigOptions;

    public SysUpdateService(IOptions<CDConfigOptions> giteeOptions, SysCacheService sysCacheService)
    {
        _cdConfigOptions = giteeOptions.Value;
        _sysCacheService = sysCacheService;
    }

    /// <summary>
    /// 备份列表
    /// </summary>
    /// <returns></returns>
    [DisplayName("备份列表")]
    [ApiDescriptionSettings(Name = "List"), HttpPost]
    public Task<List<BackupOutput>> List()
    {
        var rootPath = Path.GetFullPath(Path.Combine(_cdConfigOptions.BackendOutput, ".."));
        return Task.FromResult(Directory.GetFiles(rootPath, _cdConfigOptions.Repo + "_*.zip", SearchOption.TopDirectoryOnly)
            .Select(filePath =>
            {
                var file = new FileInfo(filePath);
                return new BackupOutput
                {
                    CreateTime = file.CreationTime,
                    FilePath = filePath,
                    FileName = file.Name
                };
            })
            .OrderByDescending(u => u.CreateTime)
            .ToList());
    }

    /// <summary>
    /// 还原
    /// </summary>
    /// <returns></returns>
    [DisplayName("还原")]
    [ApiDescriptionSettings(Name = "Restore"), HttpPost]
    public async Task Restore(RestoreInput input)
    {
        // 检查参数
        CheckConfig();
        try
        {
            var file = (await List()).FirstOrDefault(u => u.FileName.EqualIgnoreCase(input.FileName));
            if (file == null)
            {
                PrintfLog("文件不存在...");
                return;
            }

            PrintfLog("正在还原...");
            using ZipArchive archive = new(File.OpenRead(file.FilePath), ZipArchiveMode.Read, leaveOpen: false);
            archive.ExtractToDirectory(_cdConfigOptions.BackendOutput, true);
            PrintfLog("还原成功...");
        }
        catch (Exception ex)
        {
            PrintfLog("发生异常：" + ex.Message);
            throw;
        }
    }

    /// <summary>
    /// 从远端更新系统
    /// </summary>
    /// <returns></returns>
    [DisplayName("系统更新")]
    [ApiDescriptionSettings(Name = "Update"), HttpPost]
    public async Task Update()
    {
        var originColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"【{DateTime.Now}】从远端仓库部署项目");
        try
        {
            PrintfLog("----------------------------从远端仓库部署项目-开始----------------------------");

            // 检查参数
            CheckConfig();

            // 检查操作间隔
            if (_cdConfigOptions.UpdateInterval > 0)
            {
                if (_sysCacheService.Get<bool>(CacheConst.KeySysUpdateInterval)) throw Oops.Oh("请勿频繁操作");
                _sysCacheService.Set(CacheConst.KeySysUpdateInterval, true, TimeSpan.FromMinutes(_cdConfigOptions.UpdateInterval));
            }

            PrintfLog($"客户端host：{App.HttpContext.Request.Host}");
            PrintfLog($"客户端IP：{App.HttpContext.GetRemoteIpAddressToIPv4(true)}");
            PrintfLog($"仓库地址：https://gitee.com/{_cdConfigOptions.Owner}/{_cdConfigOptions.Repo}.git");
            PrintfLog($"仓库分支：{_cdConfigOptions.Branch}");

            // 获取解压后的根目录
            var rootPath = Path.GetFullPath(Path.Combine(_cdConfigOptions.BackendOutput, ".."));
            var tempDir = Path.Combine(rootPath, $"{_cdConfigOptions.Repo}-{_cdConfigOptions.Branch}");

            PrintfLog("清理旧文件...");
            FileHelper.TryDelete(tempDir);

            PrintfLog("拉取远端代码...");
            var stream = await GiteeHelper.DownloadRepoZip(_cdConfigOptions.Owner, _cdConfigOptions.Repo,
                _cdConfigOptions.AccessToken, _cdConfigOptions.Branch);

            PrintfLog("文件包解压...");
            using ZipArchive archive = new(stream, ZipArchiveMode.Read, leaveOpen: false);
            archive.ExtractToDirectory(rootPath);

            // 项目目录
            var entryProjectName = "Jero.Web"; // 启动项目目录
            var tempOutput = Path.Combine(rootPath, $"{_cdConfigOptions.Repo}_temp");
            if (Path.GetFullPath(tempOutput).Equals(
                    Path.GetFullPath(_cdConfigOptions.BackendOutput),
                    OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
                throw Oops.Oh("临时发布目录不能与正式发布目录相同");

            // Never let a failed publish reuse output from a previous update.
            FileHelper.TryDelete(tempOutput);
            if (Directory.Exists(tempOutput))
                throw Oops.Oh($"无法清理临时发布目录: {tempOutput}");

            PrintfLog("编译项目...");
            PrintfLog($"发布版本：{_cdConfigOptions.Publish.Configuration}");
            PrintfLog($"目标框架：{_cdConfigOptions.Publish.TargetFramework}");
            PrintfLog($"运行环境：{_cdConfigOptions.Publish.RuntimeIdentifier}");
            var option = _cdConfigOptions.Publish;
            var backendSourceDir = Path.Combine(tempDir, "samples", "Jero", "src");
            var entryProjectDir = Path.Combine(backendSourceDir, entryProjectName);
            var args = $"publish \"{Path.Combine(entryProjectName, $"{entryProjectName}.csproj")}\" -c {option.Configuration} -f {option.TargetFramework} -r {option.RuntimeIdentifier} --output \"{tempOutput}\"";
            await RunCommandAsync("dotnet", "tool restore", backendSourceDir);
            await RunCommandAsync("dotnet", args, backendSourceDir);
            ValidatePublishOutput(tempOutput);

            PrintfLog("复制 wwwroot 目录...");
            var wwwrootDir = Path.Combine(entryProjectDir, "wwwroot");
            FileHelper.CopyDirectory(wwwrootDir, Path.Combine(tempOutput, "wwwroot"), true);

            var excludePatterns = RuntimeMutableOutputPatterns.Concat(_cdConfigOptions.ExcludeFiles ?? []);
            FileHelper.DeleteByPatterns(tempOutput, excludePatterns, filePath => PrintfLog($"排除发布覆盖：{filePath}"));
            ValidatePublishOutput(tempOutput);

            PrintfLog("备份原项目文件...");
            string backupPath = Path.Combine(rootPath, $"{_cdConfigOptions.Repo}_{DateTime.Now:yyyy_MM_dd}.zip");
            if (File.Exists(backupPath)) File.Delete(backupPath);
            ZipFile.CreateFromDirectory(_cdConfigOptions.BackendOutput, backupPath);

            // Vite hashes chunk names, so copying over the previous bundle would retain stale files.
            var liveAssets = Path.Combine(_cdConfigOptions.BackendOutput, "App", "dist");
            if (Directory.Exists(liveAssets) && !FileHelper.TryDelete(liveAssets))
                throw Oops.Oh($"无法清理正式目录中的旧前端文件: {liveAssets}");

            FileHelper.CopyDirectory(tempOutput, _cdConfigOptions.BackendOutput, true);
            ValidatePublishOutput(_cdConfigOptions.BackendOutput);

            PrintfLog("清理文件...");
            FileHelper.TryDelete(tempOutput);
            FileHelper.TryDelete(tempDir);

            if (_cdConfigOptions.BackupCount > 0)
            {
                var fileList = await List();
                if (fileList.Count > _cdConfigOptions.BackupCount)
                    PrintfLog("清除多余的备份文件...");
                while (fileList.Count > _cdConfigOptions.BackupCount)
                {
                    var last = fileList.Last();
                    FileHelper.TryDelete(last.FilePath);
                    fileList.Remove(last);
                }
            }

            PrintfLog("重启项目后生效...");
        }
        catch (Exception ex)
        {
            PrintfLog("发生异常：" + ex.Message);
            throw;
        }
        finally
        {
            PrintfLog("----------------------------从远端仓库部署项目-结束----------------------------");
            Console.ForegroundColor = originColor;
        }
    }

    /// <summary>
    /// 仓库WebHook接口
    /// </summary>
    /// <returns></returns>
    [AllowAnonymous]
    [DisplayName("仓库WebHook接口")]
    [ApiDescriptionSettings(Name = "WebHook"), HttpPost]
    public async Task WebHook(JsonObject input)
    {
        if (!_cdConfigOptions.Enabled) throw Oops.Oh("未启用持续部署功能");
        PrintfLog("----------------------------收到WebHook请求-开始----------------------------");

        try
        {
            // 获取请求头信息
            var even = App.HttpContext.Request.Headers.FirstOrDefault(u => u.Key == "X-Gitee-Event").Value
                .FirstOrDefault();
            var ua = App.HttpContext.Request.Headers.FirstOrDefault(u => u.Key == "User-Agent").Value.FirstOrDefault();

            var timestamp = input["timestamp"]?.ToString();
            var token = input["sign"]?.ToString();
            PrintfLog("User-Agent：" + ua);
            PrintfLog("Gitee-Event：" + even);
            PrintfLog("Gitee-Token：" + token);
            PrintfLog("Gitee-Timestamp：" + timestamp);

            PrintfLog("开始验签...");
            var secret = GetWebHookKey();
            var stringToSign = $"{timestamp}\n{secret}";
            using var mac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var signData = mac.ComputeHash(Encoding.UTF8.GetBytes(stringToSign));
            var encodedSignData = Convert.ToBase64String(signData);
            var calculatedSignature = WebUtility.UrlEncode(encodedSignData);

            if (calculatedSignature != token) throw Oops.Oh("非法签名");
            PrintfLog("验签成功...");

            var hookName = input["hook_name"]?.GetValue<string>();
            PrintfLog("Hook-Name：" + hookName);

            switch (hookName)
            {
                // 提交修改
                case "push_hooks":
                    {
                        var commitList = input["commits"]?.AsArray() ?? [];
                        foreach (var commit in commitList)
                        {
                            var author = commit?["author"];
                            PrintfLog("Commit-Message：" + commit?["message"]);
                            PrintfLog("Commit-Time：" + commit?["timestamp"]);
                            PrintfLog("Commit-Author：" + author?["username"]);
                            PrintfLog("Modified-List：" + author?["modified"]?.AsArray().Select(item => item?.ToString()).Join());
                            PrintfLog("----------------------------------------------------------");
                        }

                        break;
                    }
                // 合并 Pull Request
                case "merge_request_hooks":
                    {
                        var pull = input["pull_request"];
                        var user = pull?["user"];
                        PrintfLog("Pull-Request-Title：" + pull?["message"]);
                        PrintfLog("Pull-Request-Time：" + pull?["created_at"]);
                        PrintfLog("Pull-Request-Author：" + user?["username"]);
                        PrintfLog("Pull-Request-Body：" + pull?["body"]);
                        break;
                    }
                // 新的issue
                case "issue_hooks":
                    {
                        var issue = input["issue"];
                        var user = issue?["user"];
                        var labelList = issue?["labels"]?.AsArray();
                        PrintfLog("Issue-UserName：" + user?["username"]);
                        PrintfLog("Issue-Labels：" + labelList?.Select(u => u?["name"]).Join());
                        PrintfLog("Issue-Title：" + issue?["title"]);
                        PrintfLog("Issue-Time：" + issue?["created_at"]);
                        PrintfLog("Issue-Body：" + issue?["body"]);
                        return;
                    }
                // 评论
                case "note_hooks":
                    {
                        var comment = input["comment"];
                        var user = input["user"];
                        PrintfLog("comment-UserName：" + user?["username"]);
                        PrintfLog("comment-Time：" + comment?["created_at"]);
                        PrintfLog("comment-Content：" + comment?["body"]);
                        return;
                    }
                default:
                    return;
            }

            var updateInterval = _cdConfigOptions.UpdateInterval;
            try
            {
                _cdConfigOptions.UpdateInterval = 0;
                await Update();
            }
            finally
            {
                _cdConfigOptions.UpdateInterval = updateInterval;
            }
        }
        finally
        {
            PrintfLog("----------------------------收到WebHook请求-结束----------------------------");
        }
    }

    /// <summary>
    /// 获取WebHook接口密钥
    /// </summary>
    /// <returns></returns>
    [DisplayName("获取WebHook接口密钥")]
    [ApiDescriptionSettings(Name = "WebHookKey"), HttpGet]
    public string GetWebHookKey()
    {
        return CryptogramUtil.Encrypt(_cdConfigOptions.AccessToken);
    }

    /// <summary>
    /// 获取日志列表
    /// </summary>
    /// <returns></returns>
    [DisplayName("获取日志列表")]
    [ApiDescriptionSettings(Name = "Logs"), HttpGet]
    public List<string> LogList()
    {
        return _sysCacheService.Get<List<string>>(CacheConst.KeySysUpdateLog) ?? new();
    }

    /// <summary>
    /// 清空日志
    /// </summary>
    /// <returns></returns>
    [DisplayName("清空日志")]
    [ApiDescriptionSettings(Name = "Clear"), HttpGet]
    public void ClearLog()
    {
        _sysCacheService.Remove(CacheConst.KeySysUpdateLog);
    }

    /// <summary>
    /// 检查参数
    /// </summary>
    /// <returns></returns>
    private void CheckConfig()
    {
        PrintfLog("检查CD配置参数...");

        if (_cdConfigOptions == null) throw Oops.Oh("CDConfig配置不能为空");

        if (string.IsNullOrWhiteSpace(_cdConfigOptions.Owner)) throw Oops.Oh("仓库用户名不能为空");

        if (string.IsNullOrWhiteSpace(_cdConfigOptions.Repo)) throw Oops.Oh("仓库名不能为空");

        if (string.IsNullOrWhiteSpace(_cdConfigOptions.Branch)) throw Oops.Oh("分支名不能为空");

        if (string.IsNullOrWhiteSpace(_cdConfigOptions.AccessToken)) throw Oops.Oh("授权信息不能为空");

        if (string.IsNullOrWhiteSpace(_cdConfigOptions.BackendOutput)) throw Oops.Oh("部署目录不能为空");

        if (_cdConfigOptions.Publish == null) throw Oops.Oh("编译配置不能为空");

        if (string.IsNullOrWhiteSpace(_cdConfigOptions.Publish.Configuration)) throw Oops.Oh("运行环境编译配置不能为空");

        if (string.IsNullOrWhiteSpace(_cdConfigOptions.Publish.TargetFramework)) throw Oops.Oh(".NET版本编译配置不能为空");

        if (string.IsNullOrWhiteSpace(_cdConfigOptions.Publish.RuntimeIdentifier)) throw Oops.Oh("运行平台配置不能为空");
    }

    /// <summary>
    /// 打印日志
    /// </summary>
    /// <param name="message"></param>
    private void PrintfLog(string message)
    {
        var logList = _sysCacheService.Get<List<string>>(CacheConst.KeySysUpdateLog) ?? new();

        var content = $"【{DateTime.Now}】 {message}";

        Console.WriteLine(content);

        logList.Add(content);

        _sysCacheService.Set(CacheConst.KeySysUpdateLog, logList);
    }

    /// <summary>
    /// 执行命令
    /// </summary>
    /// <param name="command">命令</param>
    /// <param name="arguments">参数</param>
    /// <param name="workingDirectory">工作目录</param>
    private async Task RunCommandAsync(string command, string arguments, string workingDirectory)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = command,
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process();
        process.StartInfo = processStartInfo;
        if (!process.Start())
            throw Oops.Oh($"无法启动发布命令: {command}");

        // Drain stdout and stderr concurrently so a verbose build cannot deadlock
        // on a full redirected pipe. The exit code is part of the publish result.
        var standardOutputTask = process.StandardOutput.ReadToEndAsync();
        var standardErrorTask = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        var standardOutput = await standardOutputTask;
        var standardError = await standardErrorTask;
        foreach (var line in standardOutput.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries))
            PrintfLog(line.Trim());
        foreach (var line in standardError.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries))
            PrintfLog("[stderr] " + line.Trim());

        if (process.ExitCode != 0)
            throw Oops.Oh($"发布命令失败，退出码: {process.ExitCode}");
    }

    private static void ValidatePublishOutput(string outputDirectory)
    {
        if (!Directory.Exists(outputDirectory))
            throw Oops.Oh($"发布目录不存在: {outputDirectory}");

        var jeroDll = Path.Combine(outputDirectory, "Jero.Web.dll");
        var jeroExe = Path.Combine(outputDirectory, "Jero.Web.exe");
        if (!File.Exists(jeroDll) && !File.Exists(jeroExe))
            throw Oops.Oh($"发布目录缺少 Jero.Web.dll/Jero.Web.exe: {outputDirectory}");

        var frontendEntry = Path.Combine(outputDirectory, "App", "dist", "index.html");
        if (!File.Exists(frontendEntry))
            throw Oops.Oh($"发布目录缺少 Jazor 前端入口文件: {frontendEntry}");
    }
}
