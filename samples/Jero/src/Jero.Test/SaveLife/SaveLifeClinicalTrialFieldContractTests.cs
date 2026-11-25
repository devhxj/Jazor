using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Runtime.CompilerServices;
using Jero.Plugin.SaveLife;
using Jero.Plugin.SaveLife.Service;
using Xunit;

namespace Jero.Test.SaveLife;

public class SaveLifeClinicalTrialFieldContractTests
{
    [Theory]
    [InlineData(typeof(SaveLifeClinicalTrial))]
    [InlineData(typeof(SaveLifeClinicalTrialBaseInput))]
    [InlineData(typeof(SaveLifeClinicalTrialListOutput))]
    [InlineData(typeof(SaveLifeMiniAppClinicalTrialOutput))]
    [InlineData(typeof(SaveLifeAgentMatchedTrialOutput))]
    [InlineData(typeof(SaveLifeAgentTrialReference))]
    public void TrialContracts_ExposeIndicationSeparatelyFromDiseaseArea(Type contractType)
    {
        Assert.NotNull(contractType.GetProperty("DiseaseArea"));
        Assert.NotNull(contractType.GetProperty("Indication"));
    }

    [Theory]
    [InlineData(typeof(SaveLifeAgentMatchedTrialOutput))]
    [InlineData(typeof(SaveLifeAgentTrialReference))]
    public void AgentTrialContracts_ExposeProjectDescriptionAndParticipantBenefits(Type contractType)
    {
        Assert.NotNull(contractType.GetProperty("Description"));
        Assert.NotNull(contractType.GetProperty("Benefits"));
    }

    [Fact]
    public void MiniAppTrialContract_ExposesRecruitmentProgressAndSchedule()
    {
        var contractType = typeof(SaveLifeMiniAppClinicalTrialOutput);

        Assert.NotNull(contractType.GetProperty("TargetCount"));
        Assert.NotNull(contractType.GetProperty("EnrolledCount"));
        Assert.NotNull(contractType.GetProperty("StartDate"));
        Assert.NotNull(contractType.GetProperty("EndDate"));
        Assert.NotNull(contractType.GetProperty("ParticipantType"));
    }

    [Fact]
    public void MiniAppKnowledgeContract_ExposesArticleAuthor()
    {
        Assert.NotNull(typeof(SaveLifeMiniAppKnowledgeOutput).GetProperty("Author"));
    }

    [Fact]
    public void CdeRegistrationNumber_IsARequiredUniqueBusinessKey()
    {
        var entityProperty = typeof(SaveLifeClinicalTrial).GetProperty(
            nameof(SaveLifeClinicalTrial.CdeRegistrationNumber))!;
        var inputProperty = typeof(SaveLifeClinicalTrialBaseInput).GetProperty(
            nameof(SaveLifeClinicalTrialBaseInput.CdeRegistrationNumber))!;
        var nullability = new NullabilityInfoContext();

        Assert.Equal(NullabilityState.NotNull, nullability.Create(entityProperty).WriteState);
        Assert.Equal(NullabilityState.NotNull, nullability.Create(inputProperty).WriteState);
        Assert.NotNull(entityProperty.GetCustomAttribute<RequiredAttribute>());
        Assert.NotNull(inputProperty.GetCustomAttribute<RequiredAttribute>());

        var hasUniqueCdeIndex = typeof(SaveLifeClinicalTrial).GetCustomAttributesData()
            .Where(attribute => attribute.AttributeType.Name.StartsWith("SugarIndex", StringComparison.Ordinal))
            .Any(attribute =>
                attribute.ConstructorArguments.Any(argument =>
                    argument.ArgumentType == typeof(string) &&
                    string.Equals(argument.Value as string, entityProperty.Name, StringComparison.Ordinal)) &&
                (attribute.ConstructorArguments.Any(argument =>
                     argument.ArgumentType == typeof(bool) && Equals(argument.Value, true)) ||
                 attribute.NamedArguments.Any(argument =>
                     argument.MemberName == "IsUnique" && Equals(argument.TypedValue.Value, true))));

        Assert.True(hasUniqueCdeIndex);
    }

    [Theory]
    [InlineData(typeof(SaveLifeClinicalTrialService), nameof(SaveLifeClinicalTrialService.Update))]
    [InlineData(typeof(SaveLifeProjectImportRecordService), nameof(SaveLifeProjectImportRecordService.ImportProjectFile))]
    public void TrialContentChanges_RequeueTheProjectForKnowledgeBaseIndexing(
        Type serviceType,
        string methodName)
    {
        var endpoint = serviceType.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public)!;
        var queueMethod = typeof(ISaveLifePublicKnowledgeBaseIndexProcessor).GetMethod(
            nameof(ISaveLifePublicKnowledgeBaseIndexProcessor.QueueTrialAsync),
            BindingFlags.Instance | BindingFlags.Public)!;

        Assert.True(AsyncMethodCalls(endpoint, queueMethod));
    }

    [Fact]
    public void ProjectImportOverwrite_QueuesOnlyAfterTheUpdatedRowIsCommitted()
    {
        var endpoint = typeof(SaveLifeProjectImportRecordService).GetMethod(
            nameof(SaveLifeProjectImportRecordService.ImportProjectFile),
            BindingFlags.Instance | BindingFlags.Public)!;

        Assert.DoesNotContain(
            endpoint.GetCustomAttributes(),
            attribute => attribute.GetType().Name == "UnitOfWorkAttribute");
    }

    private static bool AsyncMethodCalls(MethodInfo asyncMethod, MethodInfo calledMethod)
    {
        var stateMachine = asyncMethod.GetCustomAttribute<AsyncStateMachineAttribute>()?.StateMachineType;
        var moveNext = stateMachine?.GetMethod("MoveNext", BindingFlags.Instance | BindingFlags.NonPublic);
        var il = moveNext?.GetMethodBody()?.GetILAsByteArray();
        return il != null && il.AsSpan().IndexOf(BitConverter.GetBytes(calledMethod.MetadataToken)) >= 0;
    }
}
