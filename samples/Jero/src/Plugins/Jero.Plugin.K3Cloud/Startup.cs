


using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
namespace Jero.Plugin.K3Cloud;

[AppStartup(100)]
public class Startup : AppStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddConfigurableOptions<K3CloudOptions>();

        services.AddHttpClient("K3Cloud", client =>
        {
            client.BaseAddress = new Uri(App.GetConfig<K3CloudOptions>("K3Cloud", true).Url);
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
    }
}