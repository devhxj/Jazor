using Jero.Application.Modules.K3Cloud.Service;

namespace Jero.Application.Modules.K3Cloud;

[AppStartup(100)]
public sealed class K3CloudModule : AppStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        if (!App.GetConfig<bool>("K3Cloud:Enabled"))
            return;

        services.AddConfigurableOptions<K3CloudOptions>();
        services.AddHttpRemote(builder => builder.AddHttpDeclarative<IK3CloudApi>());

        services.AddHttpClient("K3Cloud", client =>
        {
            client.BaseAddress = new Uri(App.GetConfig<K3CloudOptions>("K3Cloud", true).Url);
        });
    }
}
