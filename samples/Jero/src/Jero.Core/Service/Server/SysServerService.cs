


namespace Jero.Core.Service;

/// <summary>
/// 系统服务器监控服务 🧩
/// </summary>
[ApiDescriptionSettings(Order = 290, Description = "服务器监控")]
public class SysServerService : IDynamicApiController, ITransient
{
    public SysServerService()
    {
    }

#if NET10_0_OR_GREATER

    /// <summary>
    /// 获取服务器硬件信息
    /// </summary>
    /// <returns></returns>
    [DisplayName("获取服务器硬件信息")]
    public SystemInfo HardwareInfo()
    {
        return SysServerInfoProvider.GetSystemInfo();
    }

    /// <summary>
    /// 获取服务器运行时信息
    /// </summary>
    /// <returns></returns>
    [DisplayName("获取服务器运行时信息")]
    public RuntimeInfo RuntimeInfo()
    {
        return SysServerInfoProvider.GetRuntimeInfo();
    }

    /// <summary>
    /// 获取框架主要程序集
    /// </summary>
    /// <returns></returns>
    [DisplayName("获取框架主要程序集")]
    public List<NuGetPackage> NuGetPackagesInfo()
    {
        return SysServerInfoProvider.GetNuGetPackages("Jero");
    }

#endif // NET10_0_OR_GREATER

    /// <summary>
    /// 获取服务器配置信息 🔖
    /// </summary>
    /// <returns></returns>
    [DisplayName("获取服务器配置信息")]
    public dynamic GetServerBase()
    {
        return new
        {
            HostName = Environment.MachineName, // 主机名称
            SystemOs = ComputerUtil.GetOSInfo(),//RuntimeInformation.OSDescription, // 操作系统
            OsArchitecture = Environment.OSVersion.Platform.ToString() + " " + RuntimeInformation.OSArchitecture.ToString(), // 系统架构
            ProcessorCount = Environment.ProcessorCount + " 核", // CPU核心数
            SysRunTime = ComputerUtil.GetRunTime(), // 系统运行时间
            RemoteIp = ComputerUtil.GetIpFromOnline(), // 外网地址
            LocalIp = App.HttpContext?.Connection?.LocalIpAddress!.MapToIPv4().ToString(), // 本地地址
            FrameworkDescription = RuntimeInformation.FrameworkDescription + " / " + App.GetOptions<DbConnectionOptions>().ConnectionConfigs[0].DbType.ToString(), // NET框架 + 数据库类型
            Environment = App.HostEnvironment.IsDevelopment() ? "Development" : "Production",
            Wwwroot = App.WebHostEnvironment.WebRootPath, // 网站根目录
            Stage = App.HostEnvironment.IsStaging() ? "Stage环境" : "非Stage环境", // 是否Stage环境
        };
    }

    /// <summary>
    /// 获取服务器使用信息 🔖
    /// </summary>
    /// <returns></returns>
    [DisplayName("获取服务器使用信息")]
    public dynamic GetServerUsed()
    {
        var programStartTime = Process.GetCurrentProcess().StartTime;
        var totalMilliseconds = (DateTime.Now - programStartTime).TotalMilliseconds.ToString();
        var ts = totalMilliseconds.Contains('.') ? totalMilliseconds.Split('.')[0] : totalMilliseconds;
        var programRunTime = DateTimeUtil.FormatTime(ts.ParseToLong());

        var memoryMetrics = ComputerUtil.GetComputerInfo();
        return new
        {
            memoryMetrics.FreeRam, // 空闲内存
            memoryMetrics.UsedRam, // 已用内存
            memoryMetrics.TotalRam, // 总内存
            memoryMetrics.RamRate, // 内存使用率
            memoryMetrics.CpuRates, // Cpu使用率多CPU未完成
            memoryMetrics.CpuRate, // Cpu 1使用率
            StartTime = programStartTime.ToString("yyyy-MM-dd HH:mm:ss"), // 服务启动时间
            RunTime = programRunTime, // 服务运行时间
        };
    }

    /// <summary>
    /// 获取服务器磁盘信息 🔖
    /// </summary>
    /// <returns></returns>
    [DisplayName("获取服务器磁盘信息")]
    public dynamic GetServerDisk()
    {
        return ComputerUtil.GetDiskInfos();
    }

    /// <summary>
    /// 获取框架主要程序集 🔖
    /// </summary>
    /// <returns></returns>
    [DisplayName("获取框架主要程序集")]
    public dynamic GetAssemblyList()
    {
        var furionAssembly = typeof(App).Assembly.GetName();
        var sqlSugarAssembly = typeof(ISqlSugarClient).Assembly.GetName();
        var yitIdAssembly = typeof(YitIdHelper).Assembly.GetName();
        var redisAssembly = typeof(Redis).Assembly.GetName();
        var jsonAssembly = typeof(System.Text.Json.JsonSerializer).Assembly.GetName();
        var excelAssembly = typeof(IExcelImporter).Assembly.GetName();
        var pdfAssembly = typeof(Magicodes.ExporterAndImporter.Pdf.IPdfExporter).Assembly.GetName();
        var wordAssembly = typeof(Magicodes.ExporterAndImporter.Word.IWordExporter).Assembly.GetName();
        var captchaAssembly = typeof(Lazy.Captcha.Core.ICaptcha).Assembly.GetName();
        var wechatApiAssembly = typeof(WechatApiClient).Assembly.GetName();
        var wechatTenpayAssembly = typeof(WechatTenpayClient).Assembly.GetName();
        var ossAssembly = typeof(OnceMi.AspNetCore.OSS.IOSSServiceFactory).Assembly.GetName();
        var parserAssembly = typeof(Parser).Assembly.GetName();
        var elasticsearchClientAssembly = typeof(Elastic.Clients.Elasticsearch.ElasticsearchClient).Assembly.GetName();
        var limitAssembly = typeof(AspNetCoreRateLimit.IpRateLimitMiddleware).Assembly.GetName();
        var htmlParserAssembly = typeof(AngleSharp.Html.Parser.HtmlParser).Assembly.GetName();
        var fluentEmailAssembly = typeof(MailKit.Net.Smtp.SmtpClient).Assembly.GetName();
        var qRCodeGeneratorAssembly = typeof(QRCoder.QRCodeGenerator).Assembly.GetName();
        var alibabaSendSmsRequestAssembly = typeof(AlibabaCloud.SDK.Dysmsapi20170525.Models.SendSmsRequest).Assembly.GetName();
        var tencentSendSmsRequestAssembly = typeof(TencentCloud.Sms.V20190711.Models.SendSmsRequest).Assembly.GetName();
        var rabbitMQAssembly = typeof(RabbitMQEventSourceStore).Assembly.GetName();
        var ldapConnectionAssembly = typeof(Novell.Directory.Ldap.LdapConnection).Assembly.GetName();
        var ipToolAssembly = typeof(IPTools.Core.IpTool).Assembly.GetName();
        var weixinAuthenticationOptionsAssembly = typeof(AspNet.Security.OAuth.Weixin.WeixinAuthenticationOptions).Assembly.GetName();
        var giteeAuthenticationOptionsAssembly = typeof(AspNet.Security.OAuth.Gitee.GiteeAuthenticationOptions).Assembly.GetName();
        var hashidsAssembly = typeof(HashidsNet.Hashids).Assembly.GetName();
        var sftpClientAssembly = typeof(Renci.SshNet.SftpClient).Assembly.GetName();
        var hardwareInfoAssembly = typeof(Hardware.Info.HardwareInfo).Assembly.GetName();

        return new[]
        {
            new { furionAssembly.Name, furionAssembly.Version },
            new { sqlSugarAssembly.Name, sqlSugarAssembly.Version },
            new { yitIdAssembly.Name, yitIdAssembly.Version },
            new { redisAssembly.Name, redisAssembly.Version },
            new { jsonAssembly.Name, jsonAssembly.Version },
            new { excelAssembly.Name, excelAssembly.Version },
            new { pdfAssembly.Name, pdfAssembly.Version },
            new { wordAssembly.Name, wordAssembly.Version },
            new { captchaAssembly.Name, captchaAssembly.Version },
            new { wechatApiAssembly.Name, wechatApiAssembly.Version },
            new { wechatTenpayAssembly.Name, wechatTenpayAssembly.Version },
            new { ossAssembly.Name, ossAssembly.Version },
            new { parserAssembly.Name, parserAssembly.Version },
            new { elasticsearchClientAssembly.Name, elasticsearchClientAssembly.Version },
            new { limitAssembly.Name, limitAssembly.Version },
            new { htmlParserAssembly.Name, htmlParserAssembly.Version },
            new { fluentEmailAssembly.Name, fluentEmailAssembly.Version },
            new { qRCodeGeneratorAssembly.Name, qRCodeGeneratorAssembly.Version },
            new { alibabaSendSmsRequestAssembly.Name, alibabaSendSmsRequestAssembly.Version },
            new { tencentSendSmsRequestAssembly.Name, tencentSendSmsRequestAssembly.Version },
            new { rabbitMQAssembly.Name, rabbitMQAssembly.Version },
            new { ldapConnectionAssembly.Name, ldapConnectionAssembly.Version },
            new { ipToolAssembly.Name, ipToolAssembly.Version },
            new { weixinAuthenticationOptionsAssembly.Name, weixinAuthenticationOptionsAssembly.Version },
            new { giteeAuthenticationOptionsAssembly.Name, giteeAuthenticationOptionsAssembly.Version },
            new { hashidsAssembly.Name, hashidsAssembly.Version },
            new { sftpClientAssembly.Name, sftpClientAssembly.Version },
            new { hardwareInfoAssembly.Name, hardwareInfoAssembly.Version },
        };
    }
}
