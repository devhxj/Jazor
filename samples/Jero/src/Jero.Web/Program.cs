using System.Text;

// 控制台统一以 UTF-8 输出中文日志，且首条输出不带 BOM。
Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

Serve.Run(RunOptions.Default.AddWebComponent<JeroHostComponent>());

public class JeroHostComponent : IWebComponent
{
    private static readonly string[] _suppressedLogCategories = ["Microsoft.Hosting", "Microsoft.AspNetCore"];

    public void Load(WebApplicationBuilder builder, ComponentContext componentContext)
    {
        // 设置日志过滤
        builder.Logging.AddFilter((_, category, logLevel) =>
        {
            if (logLevel < LogLevel.Information)
            {
                return false;
            }

            return !_suppressedLogCategories.Any(prefix => category?.StartsWith(prefix, StringComparison.Ordinal) == true);
        });

        // 先读 Kestrel 配置，再覆盖连接保活和请求头超时。
        // MaxRequestBodySize 为 null 时 Kestrel 不主动限制请求体大小。
        builder.WebHost.ConfigureKestrel((context, options) =>
        {
            options.Configure(context.Configuration.GetSection("Kestrel"));
            options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(30);
            options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(30);
            options.Limits.MaxRequestBodySize = null;
        });
    }
}
