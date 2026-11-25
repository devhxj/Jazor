using System.Text;

Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

Serve.Run(RunOptions.Default.AddWebComponent<JeroHostComponent>());

public class JeroHostComponent : IWebComponent
{
    private static readonly string[] SuppressedLogCategories = ["Microsoft.Hosting", "Microsoft.AspNetCore"];

    public void Load(WebApplicationBuilder builder, ComponentContext componentContext)
    {
        // 设置日志过滤
        builder.Logging.AddFilter((_, category, logLevel) =>
        {
            if (logLevel < LogLevel.Information)
            {
                return false;
            }

            return !SuppressedLogCategories.Any(prefix => category?.StartsWith(prefix, StringComparison.Ordinal) == true);
        });

        // 设置接口超时时间和上传大小-Kestrel
        builder.WebHost.ConfigureKestrel((context, options) =>
        {
            options.Configure(context.Configuration.GetSection("Kestrel"));
            options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(30);
            options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(30);
            options.Limits.MaxRequestBodySize = null;
        });
    }
}
