using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Furion.DynamicApiController;
using Jero.Plugin.SaveLife.Service;
using Xunit;

namespace Jero.Test.SaveLife;

public sealed class SaveLifeAgentSimulationApiContractTests
{
    [Fact]
    public void Service_RequiresAuthorization()
    {
        Assert.NotNull(typeof(SaveLifeAgentSimulationService)
            .GetCustomAttribute<AuthorizeAttribute>());
    }

    [Theory]
    [InlineData(nameof(SaveLifeAgentSimulationService.Prepare), "Prepare")]
    [InlineData(nameof(SaveLifeAgentSimulationService.Clear), "Clear")]
    public void SimulationEndpoints_ArePostActions(string methodName, string actionName)
    {
        var method = typeof(SaveLifeAgentSimulationService).GetMethod(
            methodName,
            BindingFlags.Instance | BindingFlags.Public);

        Assert.NotNull(method);
        var httpPost = method!.GetCustomAttribute<HttpPostAttribute>();
        Assert.NotNull(httpPost);
        Assert.Equal(actionName, method.GetCustomAttribute<ApiDescriptionSettingsAttribute>()?.Name);
        Assert.Empty(httpPost!.Template ?? string.Empty);
    }

    [Fact]
    public void Outputs_ExposeSimulationDiagnosticsAndClearCounts()
    {
        Assert.NotNull(typeof(SaveLifeAgentSimulationOutput).GetProperty(nameof(SaveLifeAgentSimulationOutput.ProfileId)));
        Assert.NotNull(typeof(SaveLifeAgentSimulationOutput).GetProperty(nameof(SaveLifeAgentSimulationOutput.PatientProfileId)));
        Assert.NotNull(typeof(SaveLifeAgentSimulationClearOutput).GetProperty(nameof(SaveLifeAgentSimulationClearOutput.ClearedMessageCount)));
        Assert.NotNull(typeof(SaveLifeAgentSimulationClearOutput).GetProperty(nameof(SaveLifeAgentSimulationClearOutput.ClearedBusinessCommandCount)));
    }
}
