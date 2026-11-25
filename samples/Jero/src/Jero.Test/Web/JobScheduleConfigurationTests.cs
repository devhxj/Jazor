// Jero 项目的版权、商标、专利和其他相关权利均受相应法律法规和许可证的要求。
using System.Reflection;
using Furion.Schedule;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Jero.Web;
using Xunit;

namespace Jero.Test.Web;

public class JobScheduleConfigurationTests
{
    [Fact]
    public void IsJobScheduleEnabled_WithoutConfiguration_DefaultsToEnabled()
    {
        var configuration = new ConfigurationBuilder().Build();

        Assert.True(IsJobScheduleEnabled(configuration));
    }

    [Fact]
    public void IsJobScheduleEnabled_WhenExplicitlyDisabled_ReturnsFalse()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JobSchedule:Enabled"] = "false"
            })
            .Build();

        Assert.False(IsJobScheduleEnabled(configuration));
    }

    [Fact]
    public void ConfigureJobScheduleServices_WhenDisabled_StillRegistersSchedulerFactory()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JobSchedule:Enabled"] = "false"
            })
            .Build();
        var services = new ServiceCollection();

        ConfigureJobScheduleServices(services, configuration);

        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(ISchedulerFactory));
    }

    private static bool IsJobScheduleEnabled(IConfiguration configuration)
    {
        var method = typeof(Startup).GetMethod(
            "IsJobScheduleEnabled",
            BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(method);
        return Assert.IsType<bool>(method!.Invoke(null, [configuration]));
    }

    private static void ConfigureJobScheduleServices(
        IServiceCollection services,
        IConfiguration configuration)
    {
        var method = typeof(Startup).GetMethod(
            "ConfigureJobScheduleServices",
            BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(method);
        method!.Invoke(null, [services, configuration]);
    }
}
