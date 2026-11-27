using Jero.Application.Modules.WorkWeixin.Option;
using Jero.Application.Modules.WorkWeixin.Proxy;
using Jero.Application.Modules.WorkWeixin.Proxy.AppChat;

namespace Jero.Application.Modules.WorkWeixin;

[AppStartup(100)]
public sealed class WorkWeixinModule : AppStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        if (!App.GetConfig<bool>("WorkWeixin:Enabled"))
            return;

        services.AddConfigurableOptions<WorkWeixinOptions>();
        services.AddHttpRemote(builder =>
        {
            builder.AddHttpDeclarative<IWorkWeixinAuthHttp>();
            builder.AddHttpDeclarative<IWorkWeixinAppChatHttp>();
            builder.AddHttpDeclarative<IDepartmentHttp>();
            builder.AddHttpDeclarative<ITagHttp>();
        });
    }
}
