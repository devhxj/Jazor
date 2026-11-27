using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace Jero.Application.Modules.DingTalk;

[AppStartup(100)]
public class Startup : AppStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddConfigurableOptions<DingTalkOptions>();

        services.AddHttpRemote(builder =>
        {
            builder.AddHttpDeclarative<IDingTalkApi>();
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
    }
}