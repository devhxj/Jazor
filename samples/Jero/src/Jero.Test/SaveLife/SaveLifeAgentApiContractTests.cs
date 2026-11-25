using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Jero.Plugin.SaveLife.Service;
using Xunit;

namespace Jero.Test.SaveLife;

public class SaveLifeAgentApiContractTests
{
    [Theory]
    [InlineData(nameof(SaveLifeAgentService.CreateConversation))]
    [InlineData(nameof(SaveLifeAgentService.GetConversations))]
    [InlineData(nameof(SaveLifeAgentService.ClearConversations))]
    [InlineData(nameof(SaveLifeAgentService.GetConversationMessages))]
    [InlineData(nameof(SaveLifeAgentService.GetConversationMessagesPage))]
    public void BusinessEndpoints_SuppressRawRequestAndResponseMonitoring(string methodName)
    {
        var method = typeof(SaveLifeAgentService).GetMethod(
            methodName,
            BindingFlags.Instance | BindingFlags.Public);

        Assert.NotNull(method);
        Assert.NotNull(method.GetCustomAttribute<SuppressMonitorAttribute>());
    }

    [Fact]
    public void LegacyChatWorkflowMethods_AreNotPublicRoutes()
    {
        var methods = new[]
        {
            typeof(SaveLifeAgentService).GetMethod(nameof(SaveLifeAgentService.SubmitIntention)),
            typeof(SaveLifePatientProfileService).GetMethod(
                nameof(SaveLifePatientProfileService.CreateAndSelectPatient)),
            typeof(SaveLifePatientProfileService).GetMethod(
                nameof(SaveLifePatientProfileService.DeferPatientOnboarding)),
            typeof(SaveLifePatientProfileService).GetMethod(
                nameof(SaveLifePatientProfileService.ConfirmProfileProposal))
        };

        Assert.All(methods, method =>
        {
            Assert.NotNull(method);
            Assert.NotNull(method!.GetCustomAttribute<NonActionAttribute>());
            Assert.Empty(method.GetCustomAttributes<HttpMethodAttribute>());
        });
    }

    [Fact]
    public void LegacyGenerationEndpoints_AreNotExposedByBusinessService()
    {
        var publicMethods = typeof(SaveLifeAgentService)
            .GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .Select(method => method.Name)
            .ToHashSet(StringComparer.Ordinal);

        Assert.DoesNotContain("ReceiveMessage", publicMethods);
        Assert.DoesNotContain("GenerateReply", publicMethods);
        Assert.DoesNotContain("ChatSync", publicMethods);
        Assert.DoesNotContain("RunAgUi", publicMethods);
    }

    [Fact]
    public void ClearConversations_UsesIdempotentCollectionDeleteRoute()
    {
        var method = typeof(SaveLifeAgentService).GetMethod(
            nameof(SaveLifeAgentService.ClearConversations),
            BindingFlags.Instance | BindingFlags.Public);
        var attribute = method?.GetCustomAttribute<HttpDeleteAttribute>();

        Assert.NotNull(attribute);
        Assert.Equal("/api/v1/agent/conversations", attribute.Template);
        Assert.Empty(method!.GetParameters());
    }

    [Fact]
    public void MessageHistory_ExposesSameMessageGuidanceAndReferences()
    {
        Assert.NotNull(typeof(SaveLifeAgentMessageOutput).GetProperty(
            nameof(SaveLifeAgentMessageOutput.GuidanceText)));
        Assert.NotNull(typeof(SaveLifeAgentMessageOutput).GetProperty(
            nameof(SaveLifeAgentMessageOutput.SuggestedActions)));
        Assert.NotNull(typeof(SaveLifeAgentMessageOutput).GetProperty(
            nameof(SaveLifeAgentMessageOutput.MatchedTrials)));
        Assert.NotNull(typeof(SaveLifeAgentMessageOutput).GetProperty(
            nameof(SaveLifeAgentMessageOutput.MatchedKnowledge)));
        Assert.NotNull(typeof(SaveLifeAgentMatchedTrialOutput).GetProperty(
            nameof(SaveLifeAgentMatchedTrialOutput.ResultLevel)));
        Assert.Null(typeof(SaveLifeAgentMessagePageOutput).GetProperty("State"));
        Assert.Null(typeof(SaveLifeAgentMessageOutput).GetProperty("ClarificationOptions"));
        Assert.NotNull(typeof(SaveLifeAgentMessageOutput).GetProperty(
            nameof(SaveLifeAgentMessageOutput.ProfileProposals)));
        Assert.NotNull(typeof(SaveLifePatientProfileProposalOutput).GetProperty(
            nameof(SaveLifePatientProfileProposalOutput.SourceType)));
        Assert.NotNull(typeof(SaveLifePatientProfileProposalOutput).GetProperty(
            nameof(SaveLifePatientProfileProposalOutput.SourceMaterialId)));
        Assert.Null(typeof(SaveLifeAgentMessageOutput).GetProperty("PatientOnboarding"));
    }

    [Fact]
    public void ProfileProposalDecision_UsesSingleAuthenticatedPatchResource()
    {
        var method = typeof(SaveLifePatientProfileService).GetMethod(
            nameof(SaveLifePatientProfileService.ResolveProfileProposal),
            BindingFlags.Instance | BindingFlags.Public);
        var patch = method?.GetCustomAttribute<HttpPatchAttribute>();

        Assert.NotNull(method);
        Assert.NotNull(patch);
        Assert.Equal("/api/v1/save-life/profile-proposals/{proposalId}", patch.Template);
        Assert.NotNull(method!.GetCustomAttribute<SuppressMonitorAttribute>());
        Assert.Equal(
            ["proposalId", "input"],
            method.GetParameters().Select(parameter => parameter.Name));
        Assert.Equal(
            [nameof(DecideSaveLifePatientProfileProposalInput.Decision)],
            typeof(DecideSaveLifePatientProfileProposalInput)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Select(property => property.Name));
    }
}
