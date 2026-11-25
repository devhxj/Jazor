// Jero 项目的版权、商标、专利和其他相关权利均受相应法律法规和许可证的要求。
using Microsoft.Extensions.DependencyInjection;
using Jero.Plugin.SaveLife;
using Xunit;

namespace Jero.Test.SaveLife;

public class SaveLifeMiniAppHttpClientTests
{
    [Fact]
    public void ConfigureServices_MiniAppHttpClient_UsesShortTimeout()
    {
        var services = new ServiceCollection();
        new Startup().ConfigureServices(services);
        using var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IHttpClientFactory>();

        var client = factory.CreateClient(SaveLifeConst.MiniAppHttpClientName);

        Assert.Equal(TimeSpan.FromSeconds(8), client.Timeout);
    }
}
