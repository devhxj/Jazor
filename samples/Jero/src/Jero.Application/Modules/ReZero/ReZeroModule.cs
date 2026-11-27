using Jero.Application.Modules.ReZero.Service;
using ReZero;
using ReZero.SuperAPI;

namespace Jero.Application.Modules.ReZero;

[AppStartup(100)]
public sealed class ReZeroModule : AppStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        if (!App.GetConfig<bool>("ReZero:Enabled"))
            return;

        var reZeroOpt = App.GetConfig<ReZeroOptions>("ReZero", true);

        // 获取默认数据库配置（第一个）
        var dbOptions = App.GetConfig<DbConnectionOptions>("DbConnection", true);
        var superAPIOption = new SuperAPIOptions()
        {
            DatabaseOptions = new DatabaseOptions()
            {
                ConnectionConfig = new SuperAPIConnectionConfig()
                {
                    DbType = dbOptions.ConnectionConfigs[0].DbType,
                    ConnectionString = dbOptions.ConnectionConfigs[0].ConnectionString
                }
            },
            UiOptions = new UiOptions() { DefaultIndexSource = "/index.html" },
            InterfaceOptions = new InterfaceOptions()
            {
                AuthorizationLocalStorageName = reZeroOpt.AccessTokenKey, // 浏览器本地存储LocalStorage存储Token的键名
                SuperApiAop = new SuperApiAop() // 超级API拦截器
            }
        };

        // 注册超级API
        services.AddReZeroServices(api =>
        {
            api.EnableSuperApi(superAPIOption);
        });
    }
}
