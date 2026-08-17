using System.Reflection;
using Acornima.Ast;
using Jazor.Compiler;
using Jazor.RazorVue.RazorSdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class ChildrenToSlotIntrinsicScenarioTests
{
    public static IEnumerable<TestDataRow<ChildrenToSlotSuccessScenario>> SuccessCases
        => ChildrenToSlotIntrinsicScenarioCatalog.Successes.Select(static scenario =>
            new TestDataRow<ChildrenToSlotSuccessScenario>(scenario)
            {
                DisplayName = scenario.Id
            });

    public static IEnumerable<TestDataRow<ChildrenToSlotRejectionScenario>> RejectionCases
        => ChildrenToSlotIntrinsicScenarioCatalog.Rejections.Select(static scenario =>
            new TestDataRow<ChildrenToSlotRejectionScenario>(scenario)
            {
                DisplayName = scenario.Id
            });

    public static IEnumerable<TestDataRow<ChildrenToSlotImportFailureScenario>> ImportFailureCases
        => ChildrenToSlotIntrinsicScenarioCatalog.ImportFailures.Select(static scenario =>
            new TestDataRow<ChildrenToSlotImportFailureScenario>(scenario)
            {
                DisplayName = scenario.Id
            });

    public static IEnumerable<TestDataRow<ChildrenToSlotAuthoringFailureScenario>> AuthoringFailureCases
        => ChildrenToSlotIntrinsicScenarioCatalog.AuthoringFailures.Select(static scenario =>
            new TestDataRow<ChildrenToSlotAuthoringFailureScenario>(scenario)
            {
                DisplayName = scenario.Id
            });

    [TestMethod]
    public void ScenarioCatalogs_HaveUniqueIdsDimensionsKindsAndInputs()
    {
        var allIds = ChildrenToSlotIntrinsicScenarioCatalog.Successes.Select(static scenario => scenario.Id)
            .Concat(ChildrenToSlotIntrinsicScenarioCatalog.Rejections.Select(static scenario => scenario.Id))
            .Concat(ChildrenToSlotIntrinsicScenarioCatalog.ImportFailures.Select(static scenario => scenario.Id))
            .Concat(ChildrenToSlotIntrinsicScenarioCatalog.AuthoringFailures.Select(static scenario => scenario.Id))
            .ToArray();
        var allInputs = ChildrenToSlotIntrinsicScenarioCatalog.Successes.Select(static scenario => scenario.InputIdentity)
            .Concat(ChildrenToSlotIntrinsicScenarioCatalog.Rejections.Select(static scenario => scenario.InputIdentity))
            .Concat(ChildrenToSlotIntrinsicScenarioCatalog.ImportFailures.Select(static scenario => scenario.InputIdentity))
            .Concat(ChildrenToSlotIntrinsicScenarioCatalog.AuthoringFailures.Select(static scenario => scenario.InputIdentity))
            .ToArray();

        Assert.IsNotEmpty(allIds);
        Assert.HasCount(allIds.Length, allIds.Distinct(StringComparer.Ordinal));
        Assert.HasCount(allInputs.Length, allInputs.Distinct(StringComparer.Ordinal));
        Assert.IsTrue(allIds.All(static id => id.StartsWith("children-to-slot.", StringComparison.Ordinal)));
        Assert.IsTrue(ChildrenToSlotIntrinsicScenarioCatalog.Successes.All(static scenario =>
            !string.IsNullOrWhiteSpace(scenario.Dimension)));
        Assert.IsTrue(ChildrenToSlotIntrinsicScenarioCatalog.Rejections.All(static scenario =>
            !string.IsNullOrWhiteSpace(scenario.Dimension)));
        Assert.IsTrue(ChildrenToSlotIntrinsicScenarioCatalog.ImportFailures.All(static scenario =>
            !string.IsNullOrWhiteSpace(scenario.Dimension) && scenario.ExpectedMessageFragments.Count > 0));
        Assert.IsTrue(ChildrenToSlotIntrinsicScenarioCatalog.AuthoringFailures.All(static scenario =>
            !string.IsNullOrWhiteSpace(scenario.Dimension) && scenario.ExpectedMessageFragments.Count > 0));
        Assert.HasCount(
            Enum.GetValues<ChildrenToSlotSuccessKind>().Length,
            ChildrenToSlotIntrinsicScenarioCatalog.Successes.Select(static scenario => scenario.Kind).Distinct());
        Assert.HasCount(
            Enum.GetValues<ChildrenToSlotRejectionKind>().Length,
            ChildrenToSlotIntrinsicScenarioCatalog.Rejections.Select(static scenario => scenario.Kind).Distinct());
        Assert.HasCount(
            Enum.GetValues<ChildrenToSlotImportFailureKind>().Length,
            ChildrenToSlotIntrinsicScenarioCatalog.ImportFailures.Select(static scenario => scenario.Kind).Distinct());
        Assert.HasCount(
            Enum.GetValues<ChildrenToSlotAuthoringFailureKind>().Length,
            ChildrenToSlotIntrinsicScenarioCatalog.AuthoringFailures.Select(static scenario => scenario.Kind).Distinct());
    }

    [TestMethod]
    [DynamicData(nameof(SuccessCases))]
    public void TryBuild_RecognizesHostContractsAndBuildsDefaultSlotAst(ChildrenToSlotSuccessScenario scenario)
    {
        var fixture = Compile(scenario.Source, scenario.Id);
        var arguments = CreateArguments(
            fixture.Operation.TargetMethod.Parameters.Length,
            scenario.ChildExpressionKind);
        var probe = new ChildrenToSlotServiceProbe(
            scenario.ModulePath,
            ChildrenToSlotImportBehavior.ReturnFactory);

        var built = ChildrenToSlotIntrinsic.TryBuild(
            fixture.Operation,
            fixture.Operation.TargetMethod,
            arguments,
            probe.CreateServices(),
            out var expression);

        Assert.IsTrue(built, scenario.Id);
        Assert.IsNotNull(expression, scenario.Id);
        Assert.AreEqual(1, probe.ImportAttempts, scenario.Id);
        AssertDefaultSlotAst(
            expression,
            arguments,
            scenario.HasProps,
            scenario.ChildExpressionKind == ChildrenToSlotChildExpressionKind.Identifier,
            scenario.Id);
    }

    [TestMethod]
    [DynamicData(nameof(RejectionCases))]
    public void TryBuild_LeavesNonMatchingHostCallsForNormalLowering(ChildrenToSlotRejectionScenario scenario)
    {
        var fixture = Compile(scenario.Source, scenario.Id);
        var argumentCount = scenario.ArgumentCountOverride ?? fixture.Operation.TargetMethod.Parameters.Length;
        var arguments = CreateArguments(argumentCount, ChildrenToSlotChildExpressionKind.Identifier);
        var probe = new ChildrenToSlotServiceProbe(
            scenario.ModulePath,
            ChildrenToSlotImportBehavior.ReturnFactory);

        var built = ChildrenToSlotIntrinsic.TryBuild(
            fixture.Operation,
            fixture.Operation.TargetMethod,
            arguments,
            probe.CreateServices(),
            out var expression);

        Assert.IsFalse(built, scenario.Id);
        Assert.IsNull(expression, scenario.Id);
        Assert.AreEqual(0, probe.ImportAttempts, scenario.Id);
    }

    [TestMethod]
    [DynamicData(nameof(ImportFailureCases))]
    public void TryBuild_ReportsRuntimeImportResolutionFailure(ChildrenToSlotImportFailureScenario scenario)
    {
        var fixture = Compile(scenario.Source, scenario.Id);
        var arguments = CreateArguments(
            fixture.Operation.TargetMethod.Parameters.Length,
            ChildrenToSlotChildExpressionKind.Identifier);
        var probe = new ChildrenToSlotServiceProbe("runtime", scenario.ImportBehavior);

        var exception = Assert.ThrowsExactly<OperationTransformationException>(() =>
            ChildrenToSlotIntrinsic.TryBuild(
                fixture.Operation,
                fixture.Operation.TargetMethod,
                arguments,
                probe.CreateServices(),
                out _));

        Assert.AreEqual(1, probe.ImportAttempts, scenario.Id);
        foreach (var expected in scenario.ExpectedMessageFragments)
            StringAssert.Contains(exception.Message, expected, StringComparison.Ordinal, scenario.Id);
        Assert.AreEqual(
            "ChildrenToSlotIntrinsicScenario.g.cs",
            Path.GetFileName(exception.Data["location.path"] as string),
            scenario.Id);
    }

    [TestMethod]
    [DynamicData(nameof(AuthoringFailureCases))]
    public void TryBuild_RejectsInvalidTypedDefaultSlotContracts(ChildrenToSlotAuthoringFailureScenario scenario)
    {
        var fixture = Compile(scenario.Source, scenario.Id);
        var arguments = CreateArguments(
            fixture.Operation.TargetMethod.Parameters.Length,
            ChildrenToSlotChildExpressionKind.Identifier);
        var probe = new ChildrenToSlotServiceProbe(
            "runtime",
            ChildrenToSlotImportBehavior.ReturnFactory);

        var exception = Assert.ThrowsExactly<OperationTransformationException>(() =>
            ChildrenToSlotIntrinsic.TryBuild(
                fixture.Operation,
                fixture.Operation.TargetMethod,
                arguments,
                probe.CreateServices(),
                out _));

        Assert.AreEqual(0, probe.ImportAttempts, scenario.Id);
        foreach (var expected in scenario.ExpectedMessageFragments)
            StringAssert.Contains(exception.Message, expected, StringComparison.Ordinal, scenario.Id);
    }

    [TestMethod]
    public void TryBuild_RejectsImplicitUntypedAndAmbiguousDefaultSlotContracts()
    {
        AssertAuthoringFailure(
            """
            public static class ScenarioModule
            {
                public static SlotHost.IVNode Invoke(SlotHost.IVueComponent component, SlotHost.IVNode child)
                    => SlotHost.h(component, child);
            }
            """,
            "implicit-untyped-default-slot",
            "Implicit component child content has no explicit slot contract");
        AssertAuthoringFailure(
            """
            public sealed class MultipleDefaultSlots
            {
                [System.ComponentModel.Description("@#default")]
                public SlotHost.Slot First => null!;

                [System.ComponentModel.Description("@#default")]
                public SlotHost.Slot Second => null!;
            }

            public static class ScenarioModule
            {
                public static SlotHost.IVNode Invoke(
                    SlotHost.IVueSlotComponent<MultipleDefaultSlots> component,
                    SlotHost.IVNode child)
                    => SlotHost.h(component, child);
            }
            """,
            "multiple-default-slots",
            "declares more than one explicit default slot");
        AssertAuthoringFailure(
            """
            public delegate SlotHost.IVNode ScopedSlot(int value);

            public sealed class ScopedDefaultSlots
            {
                [System.ComponentModel.Description("@#default")]
                public ScopedSlot Content => null!;
            }

            public static class ScenarioModule
            {
                public static SlotHost.IVNode Invoke(
                    SlotHost.IVueSlotComponent<ScopedDefaultSlots> component,
                    SlotHost.IVNode child)
                    => SlotHost.h(component, child);
            }
            """,
            "scoped-default-slot",
            "expects slot scope");
    }

    [TestMethod]
    public void BuildDefaultSlotObject_UsesStringKeyWhenSlotNameIsNotIdentifier()
    {
        var method = typeof(ChildrenToSlotIntrinsic).GetMethod(
            "BuildDefaultSlotObject",
            BindingFlags.NonPublic | BindingFlags.Static);
        Assert.IsNotNull(method);

        var child = new Identifier("content");
        var slotObject = (ObjectExpression)method!.Invoke(null, [child, "item-value"])!;
        var property = (ObjectProperty)slotObject.Properties.Single();

        Assert.IsInstanceOfType<StringLiteral>(property.Key);
        Assert.AreEqual("item-value", ((StringLiteral)property.Key).Value);
        var callback = (ArrowFunctionExpression)property.Value;
        Assert.AreSame(child, callback.Body);
    }

    private static IReadOnlyList<Expression> CreateArguments(
        int count,
        ChildrenToSlotChildExpressionKind childExpressionKind)
    {
        var arguments = Enumerable.Range(0, count)
            .Select(static index => (Expression)new Identifier($"value{index}"))
            .ToArray();
        if (arguments.Length > 0 && childExpressionKind == ChildrenToSlotChildExpressionKind.NullLiteral)
            arguments[^1] = new NullLiteral("null");
        return arguments;
    }

    private static void AssertAuthoringFailure(string source, string scenarioId, string expectedMessage)
    {
        var fixture = Compile(source, scenarioId);
        var arguments = CreateArguments(
            fixture.Operation.TargetMethod.Parameters.Length,
            ChildrenToSlotChildExpressionKind.Identifier);
        var probe = new ChildrenToSlotServiceProbe(
            "runtime",
            ChildrenToSlotImportBehavior.ReturnFactory);

        var exception = Assert.ThrowsExactly<OperationTransformationException>(() =>
            ChildrenToSlotIntrinsic.TryBuild(
                fixture.Operation,
                fixture.Operation.TargetMethod,
                arguments,
                probe.CreateServices(),
                out _));

        Assert.AreEqual(0, probe.ImportAttempts, scenarioId);
        StringAssert.Contains(exception.Message, expectedMessage, StringComparison.Ordinal, scenarioId);
    }

    private static void AssertDefaultSlotAst(
        Expression expression,
        IReadOnlyList<Expression> inputs,
        bool hasProps,
        bool expectsSingleEvaluationWrapper,
        string scenarioId)
    {
        Assert.IsInstanceOfType<CallExpression>(expression, scenarioId);
        var outerCall = (CallExpression)expression;
        var expectedArgumentCount = hasProps ? 3 : 2;

        if (!expectsSingleEvaluationWrapper)
        {
            Assert.IsInstanceOfType<Identifier>(outerCall.Callee, scenarioId);
            Assert.AreEqual("h", ((Identifier)outerCall.Callee).Name, scenarioId);
            Assert.AreEqual(expectedArgumentCount, outerCall.Arguments.Count, scenarioId);
            Assert.IsTrue(ReferenceEquals(inputs[0], outerCall.Arguments[0]), scenarioId);
            if (hasProps)
                Assert.IsTrue(ReferenceEquals(inputs[1], outerCall.Arguments[1]), scenarioId);
            AssertDefaultSlotObject(
                outerCall.Arguments[outerCall.Arguments.Count - 1],
                inputs[^1],
                scenarioId);
            return;
        }

        Assert.IsInstanceOfType<ArrowFunctionExpression>(outerCall.Callee, scenarioId);
        var wrapper = (ArrowFunctionExpression)outerCall.Callee;
        Assert.AreEqual(expectedArgumentCount, wrapper.Params.Count, scenarioId);
        Assert.AreEqual(expectedArgumentCount, outerCall.Arguments.Count, scenarioId);
        for (var index = 0; index < inputs.Count; index++)
            Assert.IsTrue(ReferenceEquals(inputs[index], outerCall.Arguments[index]), scenarioId);

        var expectedParameterNames = hasProps
            ? new[] { "__component", "__props", "__slot0" }
            : new[] { "__component", "__slot0" };
        var parameters = wrapper.Params.Cast<Identifier>().ToArray();
        CollectionAssert.AreEqual(expectedParameterNames, parameters.Select(static parameter => parameter.Name).ToArray(), scenarioId);

        Assert.IsInstanceOfType<CallExpression>(wrapper.Body, scenarioId);
        var renderCall = (CallExpression)wrapper.Body;
        Assert.IsInstanceOfType<Identifier>(renderCall.Callee, scenarioId);
        Assert.AreEqual("h", ((Identifier)renderCall.Callee).Name, scenarioId);
        Assert.AreEqual(expectedArgumentCount, renderCall.Arguments.Count, scenarioId);
        Assert.IsTrue(ReferenceEquals(parameters[0], renderCall.Arguments[0]), scenarioId);
        if (hasProps)
            Assert.IsTrue(ReferenceEquals(parameters[1], renderCall.Arguments[1]), scenarioId);
        AssertDefaultSlotObject(
            renderCall.Arguments[renderCall.Arguments.Count - 1],
            parameters[^1],
            scenarioId);
    }

    private static void AssertDefaultSlotObject(
        Expression expression,
        Expression expectedChild,
        string scenarioId)
    {
        Assert.IsInstanceOfType<ObjectExpression>(expression, scenarioId);
        var slotObject = (ObjectExpression)expression;
        Assert.AreEqual(1, slotObject.Properties.Count, scenarioId);
        Assert.IsInstanceOfType<ObjectProperty>(slotObject.Properties[0], scenarioId);
        var property = (ObjectProperty)slotObject.Properties[0];
        Assert.IsInstanceOfType<Identifier>(property.Key, scenarioId);
        Assert.AreEqual("default", ((Identifier)property.Key).Name, scenarioId);
        Assert.IsInstanceOfType<ArrowFunctionExpression>(property.Value, scenarioId);
        var callback = (ArrowFunctionExpression)property.Value;
        Assert.AreEqual(0, callback.Params.Count, scenarioId);
        Assert.IsTrue(ReferenceEquals(expectedChild, callback.Body), scenarioId);
    }

    private static ChildrenToSlotFixture Compile(string source, string scenarioId)
    {
        var hostTree = CSharpSyntaxTree.ParseText(
            ChildrenToSlotIntrinsicScenarioCatalog.StandardHostSource,
            TestMetadataReferences.PreviewParseOptions,
            path: "ChildrenToSlotIntrinsicHost.g.cs");
        var scenarioTree = CSharpSyntaxTree.ParseText(
            source,
            TestMetadataReferences.PreviewParseOptions,
            path: "ChildrenToSlotIntrinsicScenario.g.cs");
        var compilation = CSharpCompilation.Create(
            assemblyName: "ChildrenToSlotIntrinsicScenarios_" + Guid.NewGuid().ToString("N"),
            syntaxTrees: [hostTree, scenarioTree],
            references: TestMetadataReferences.Net11,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.HasCount(
            0,
            errors,
            $"{scenarioId}:{Environment.NewLine}" +
            string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));

        var invocation = scenarioTree.GetRoot()
            .DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Single();
        var operation = compilation.GetSemanticModel(scenarioTree).GetOperation(invocation) as IInvocationOperation
            ?? throw new InvalidOperationException($"{scenarioId}: invocation operation was not available.");
        return new ChildrenToSlotFixture(operation);
    }

    private sealed record ChildrenToSlotFixture(IInvocationOperation Operation);
}

public enum ChildrenToSlotSuccessKind
{
    FallbackComponentContract,
    DerivedVueChild,
    NodeArray,
    NodeEnumerable,
    HostNumber,
    MappedString,
    MappedNumber,
    MappedBoolean,
    ConcreteProps,
    ConstrainedProps,
    NullChildAst,
    InheritedDefaultSlot,
    OpenGenericSlotContract,
    TypedPropsAndSlots
}

public enum ChildrenToSlotRejectionKind
{
    InstanceRenderFactory,
    WrongRuntimeName,
    BlankModulePath,
    MissingNodeContract,
    UnsupportedArity,
    ArrayReceiver,
    UnsupportedChild,
    MissingPropsContract,
    ArrayProps,
    TranslatedArgumentCount
}

public enum ChildrenToSlotImportFailureKind
{
    BuilderRejected,
    MissingFactoryExpression
}

public enum ChildrenToSlotAuthoringFailureKind
{
    NonGenericConcreteSlotMissingDefault,
    ExcludedDefaultSlotMembers,
    ArrayValuedDefaultSlot,
    OpenSlotTypeParameter,
    OpenTypedComponentSlotTypeParameter
}

public enum ChildrenToSlotChildExpressionKind
{
    Identifier,
    NullLiteral
}

public enum ChildrenToSlotImportBehavior
{
    ReturnFactory,
    ReturnFalse,
    ReturnNull
}

public sealed record ChildrenToSlotSuccessScenario(
    string Id,
    string Dimension,
    ChildrenToSlotSuccessKind Kind,
    string Source,
    string ModulePath,
    bool HasProps,
    ChildrenToSlotChildExpressionKind ChildExpressionKind)
{
    public string InputIdentity => $"{Kind}|{ModulePath}|{HasProps}|{ChildExpressionKind}|{Source}";
}

public sealed record ChildrenToSlotRejectionScenario(
    string Id,
    string Dimension,
    ChildrenToSlotRejectionKind Kind,
    string Source,
    string ModulePath,
    int? ArgumentCountOverride = null)
{
    public string InputIdentity => $"{Kind}|{ModulePath}|{ArgumentCountOverride}|{Source}";
}

public sealed record ChildrenToSlotImportFailureScenario(
    string Id,
    string Dimension,
    ChildrenToSlotImportFailureKind Kind,
    string Source,
    ChildrenToSlotImportBehavior ImportBehavior,
    IReadOnlyList<string> ExpectedMessageFragments)
{
    public string InputIdentity => $"{Kind}|{ImportBehavior}|{Source}";
}

public sealed record ChildrenToSlotAuthoringFailureScenario(
    string Id,
    string Dimension,
    ChildrenToSlotAuthoringFailureKind Kind,
    string Source,
    IReadOnlyList<string> ExpectedMessageFragments)
{
    public string InputIdentity => $"{Kind}|{Source}";
}

internal sealed class ChildrenToSlotServiceProbe
{
    private readonly string _modulePath;
    private readonly ChildrenToSlotImportBehavior _importBehavior;

    public ChildrenToSlotServiceProbe(
        string modulePath,
        ChildrenToSlotImportBehavior importBehavior)
    {
        _modulePath = modulePath;
        _importBehavior = importBehavior;
    }

    public int ImportAttempts { get; private set; }

    public ChildrenToSlotIntrinsic.Services CreateServices()
        => new(
            new SenseArgument(UseImportAliases: true),
            TryBuildImportedModuleMember,
            GetModuleImportPath,
            GetMapperType,
            EnumerateNamedTypeHierarchyBaseFirst,
            static (operation, message) => new OperationTransformationException(operation, message));

    private bool TryBuildImportedModuleMember(
        ITypeSymbol containingType,
        string memberName,
        SenseArgument context,
        out Expression? expression)
    {
        ImportAttempts++;
        switch (_importBehavior)
        {
            case ChildrenToSlotImportBehavior.ReturnFactory:
                expression = new Identifier("h");
                return true;
            case ChildrenToSlotImportBehavior.ReturnFalse:
                expression = new Identifier("ignored");
                return false;
            case ChildrenToSlotImportBehavior.ReturnNull:
                expression = null;
                return true;
            default:
                throw new InvalidOperationException($"Unsupported import behavior '{_importBehavior}'.");
        }
    }

    private string? GetModuleImportPath(ITypeSymbol symbol) => _modulePath;

    private static (TypeMapper Mapper, string TypeName) GetMapperType(ITypeSymbol symbol)
        => symbol.OriginalDefinition.SpecialType switch
        {
            SpecialType.System_String or SpecialType.System_Char => (TypeMapper.String, "String"),
            SpecialType.System_Boolean => (TypeMapper.Boolean, "Boolean"),
            SpecialType.System_SByte or
            SpecialType.System_Byte or
            SpecialType.System_Int16 or
            SpecialType.System_UInt16 or
            SpecialType.System_Int32 or
            SpecialType.System_UInt32 or
            SpecialType.System_Single or
            SpecialType.System_Double or
            SpecialType.System_Decimal => (TypeMapper.Number, "Number"),
            _ when string.Equals(symbol.Name, "Number", StringComparison.Ordinal) => (TypeMapper.Number, "Number"),
            _ => (TypeMapper.Unknown, symbol.Name)
        };

    private static IEnumerable<INamedTypeSymbol> EnumerateNamedTypeHierarchyBaseFirst(INamedTypeSymbol type)
    {
        var hierarchy = new Stack<INamedTypeSymbol>();
        for (var current = type; current is not null; current = current.BaseType)
            hierarchy.Push(current);
        return hierarchy;
    }
}

internal static class ChildrenToSlotIntrinsicScenarioCatalog
{
    public const string StandardHostSource = """
        using System.Collections.Generic;

        public static partial class SlotHost
        {
            public interface IVNode
            {
            }

            public abstract class VueProps
            {
            }

            public class VueChild
            {
            }

            public interface IVueComponent
            {
            }

            public interface IVueComponent<TProps> : IVueComponent
                where TProps : VueProps
            {
            }

            public interface IVueSlotComponent<TSlots> : IVueComponent
            {
            }

            public interface IVueComponent<TProps, TSlots> : IVueComponent
                where TProps : VueProps
            {
            }

            public sealed class Number
            {
            }

            public delegate IVNode Slot();

            public sealed class DefaultSlots
            {
                [System.ComponentModel.Description("@#default")]
                public Slot Content => null!;
            }

            [System.ComponentModel.Description("@#h")]
            public static extern IVNode h(IVueComponent component, IVNode child);

            [System.ComponentModel.Description("@#h")]
            public static extern IVNode h(IVueComponent component, IVNode[] children);

            [System.ComponentModel.Description("@#h")]
            public static extern IVNode h(IVueComponent component, IEnumerable<IVNode> children);

            [System.ComponentModel.Description("@#h")]
            public static extern IVNode h(IVueComponent component, VueChild child);

            [System.ComponentModel.Description("@#h")]
            public static extern IVNode h(IVueComponent component, Number child);

            [System.ComponentModel.Description("@#h")]
            public static extern IVNode h(IVueComponent component, string child);

            [System.ComponentModel.Description("@#h")]
            public static extern IVNode h(IVueComponent component, int child);

            [System.ComponentModel.Description("@#h")]
            public static extern IVNode h(IVueComponent component, bool child);

            [System.ComponentModel.Description("@#h")]
            public static extern IVNode h(IVueComponent component, VueProps props, IVNode child);

            [System.ComponentModel.Description("@#h")]
            public static extern IVNode h<TProps>(IVueComponent<TProps> component, TProps props, IVNode child)
                where TProps : VueProps;

            [System.ComponentModel.Description("@#h")]
            public static extern IVNode h<TSlots>(IVueSlotComponent<TSlots> component, IVNode child);

            [System.ComponentModel.Description("@#h")]
            public static extern IVNode h<TSlots>(IVueSlotComponent<TSlots> component, IVNode[] children);

            [System.ComponentModel.Description("@#h")]
            public static extern IVNode h<TSlots>(IVueSlotComponent<TSlots> component, IEnumerable<IVNode> children);

            [System.ComponentModel.Description("@#h")]
            public static extern IVNode h<TSlots>(IVueSlotComponent<TSlots> component, VueChild child);

            [System.ComponentModel.Description("@#h")]
            public static extern IVNode h<TSlots>(IVueSlotComponent<TSlots> component, Number child);

            [System.ComponentModel.Description("@#h")]
            public static extern IVNode h<TSlots>(IVueSlotComponent<TSlots> component, string child);

            [System.ComponentModel.Description("@#h")]
            public static extern IVNode h<TSlots>(IVueSlotComponent<TSlots> component, int child);

            [System.ComponentModel.Description("@#h")]
            public static extern IVNode h<TSlots>(IVueSlotComponent<TSlots> component, bool child);

            [System.ComponentModel.Description("@#h")]
            public static extern IVNode h<TProps, TSlots>(IVueComponent<TProps, TSlots> component, TProps props, IVNode child)
                where TProps : VueProps;
        }
        """;

    public static IReadOnlyList<ChildrenToSlotSuccessScenario> Successes { get; } =
    [
        Success(
            "fallback-component-contract",
            "untyped-component-contract-recovered-from-slot-component-interface",
            ChildrenToSlotSuccessKind.FallbackComponentContract,
            """
            namespace Contracts
            {
                public interface IComponentShape
                {
                }

                public interface IVueComponent
                {
                }
            }

            public static class AlternateHost
            {
                public interface IVNode
                {
                }

                public delegate IVNode Slot();

                public sealed class DefaultSlots
                {
                    [System.ComponentModel.Description("@#default")]
                    public Slot Content => null!;
                }

                public interface IVueSlotComponent<TSlots> :
                    Contracts.IComponentShape,
                    Contracts.IVueComponent
                {
                }

                public static extern IVNode h<TSlots>(IVueSlotComponent<TSlots> component, IVNode child);
            }

            public static class ScenarioModule
            {
                public static AlternateHost.IVNode Invoke(
                    AlternateHost.IVueSlotComponent<AlternateHost.DefaultSlots> component,
                    AlternateHost.IVNode child)
                    => AlternateHost.h(component, child);
            }
            """,
            hasProps: false),
        Success(
            "derived-vue-child",
            "host-child-wrapper-derived-parameter",
            ChildrenToSlotSuccessKind.DerivedVueChild,
            """
            public static partial class SlotHost
            {
                public sealed class RichChild : VueChild
                {
                }

                [System.ComponentModel.Description("@#h")]
                public static extern IVNode h<TSlots>(IVueSlotComponent<TSlots> component, RichChild child);
            }

            public static class ScenarioModule
            {
                public static SlotHost.IVNode Invoke(
                    SlotHost.IVueSlotComponent<SlotHost.DefaultSlots> component,
                    SlotHost.RichChild child)
                    => SlotHost.h(component, child);
            }
            """,
            hasProps: false),
        Success(
            "node-array",
            "node-array-child-domain",
            ChildrenToSlotSuccessKind.NodeArray,
            """
            public static class ScenarioModule
            {
                public static SlotHost.IVNode Invoke(
                    SlotHost.IVueSlotComponent<SlotHost.DefaultSlots> component,
                    SlotHost.IVNode[] children)
                    => SlotHost.h(component, children);
            }
            """,
            hasProps: false),
        Success(
            "node-enumerable",
            "enumerable-node-child-domain",
            ChildrenToSlotSuccessKind.NodeEnumerable,
            """
            public static class ScenarioModule
            {
                public static SlotHost.IVNode Invoke(
                    SlotHost.IVueSlotComponent<SlotHost.DefaultSlots> component,
                    System.Collections.Generic.IEnumerable<SlotHost.IVNode> children)
                    => SlotHost.h(component, children);
            }
            """,
            hasProps: false),
        Success(
            "host-number",
            "host-number-child-domain",
            ChildrenToSlotSuccessKind.HostNumber,
            """
            public static class ScenarioModule
            {
                public static SlotHost.IVNode Invoke(
                    SlotHost.IVueSlotComponent<SlotHost.DefaultSlots> component,
                    SlotHost.Number child)
                    => SlotHost.h(component, child);
            }
            """,
            hasProps: false),
        Success(
            "mapped-string",
            "compiler-string-mapper-child-domain",
            ChildrenToSlotSuccessKind.MappedString,
            """
            public static class ScenarioModule
            {
                public static SlotHost.IVNode Invoke(
                    SlotHost.IVueSlotComponent<SlotHost.DefaultSlots> component,
                    string child)
                    => SlotHost.h(component, child);
            }
            """,
            hasProps: false),
        Success(
            "mapped-number",
            "compiler-number-mapper-child-domain",
            ChildrenToSlotSuccessKind.MappedNumber,
            """
            public static class ScenarioModule
            {
                public static SlotHost.IVNode Invoke(
                    SlotHost.IVueSlotComponent<SlotHost.DefaultSlots> component,
                    int child)
                    => SlotHost.h(component, child);
            }
            """,
            hasProps: false),
        Success(
            "mapped-boolean",
            "compiler-boolean-mapper-child-domain",
            ChildrenToSlotSuccessKind.MappedBoolean,
            """
            public static class ScenarioModule
            {
                public static SlotHost.IVNode Invoke(
                    SlotHost.IVueSlotComponent<SlotHost.DefaultSlots> component,
                    bool child)
                    => SlotHost.h(component, child);
            }
            """,
            hasProps: false),
        Success(
            "concrete-props",
            "concrete-props-derived-from-host-base",
            ChildrenToSlotSuccessKind.ConcreteProps,
            """
            public sealed class EditorProps : SlotHost.VueProps
            {
            }

            public static class ScenarioModule
            {
                public static SlotHost.IVNode Invoke(
                    SlotHost.IVueComponent<EditorProps, SlotHost.DefaultSlots> component,
                    EditorProps props,
                    SlotHost.IVNode child)
                    => SlotHost.h(component, props, child);
            }
            """,
            hasProps: true),
        Success(
            "constrained-props",
            "type-parameter-props-constrained-to-host-base",
            ChildrenToSlotSuccessKind.ConstrainedProps,
            """
            public static class ScenarioModule
            {
                public static SlotHost.IVNode Invoke<TProps>(
                    SlotHost.IVueComponent<TProps, SlotHost.DefaultSlots> component,
                    TProps props,
                    SlotHost.IVNode child)
                    where TProps : SlotHost.VueProps
                    => SlotHost.h(component, props, child);
            }
            """,
            hasProps: true),
        Success(
            "null-child-ast",
            "literal-null-child-does-not-need-single-evaluation-wrapper",
            ChildrenToSlotSuccessKind.NullChildAst,
            """
            public static class ScenarioModule
            {
                public static SlotHost.IVNode Invoke(SlotHost.IVueComponent component)
                    => SlotHost.h((SlotHost.IVueSlotComponent<SlotHost.DefaultSlots>)component, (SlotHost.IVNode)null!);
            }
            """,
            hasProps: false,
            childExpressionKind: ChildrenToSlotChildExpressionKind.NullLiteral),
        Success(
            "inherited-default-slot",
            "typed-slot-contract-inherits-public-default-member",
            ChildrenToSlotSuccessKind.InheritedDefaultSlot,
            """
            public abstract class BaseSlots
            {
                [System.ComponentModel.Description("@#default")]
                public SlotHost.Slot Default => null!;
            }

            public sealed class EditorSlots : BaseSlots
            {
            }

            public static class ScenarioModule
            {
                public static SlotHost.IVNode Invoke(
                    SlotHost.IVueSlotComponent<EditorSlots> component,
                    SlotHost.IVNode child)
                    => SlotHost.h(component, child);
            }
            """,
            hasProps: false),
        Success(
            "open-generic-slot-contract",
            "open-slot-type-parameter-defers-concrete-default-member-validation",
            ChildrenToSlotSuccessKind.OpenGenericSlotContract,
            """
            public static class ScenarioModule
            {
                public sealed class GenericDefaultSlots<TValue>
                {
                    [System.ComponentModel.Description("@#default")]
                    public SlotHost.Slot Content => null!;
                }

                public static SlotHost.IVNode Invoke<TValue>(
                    SlotHost.IVueSlotComponent<GenericDefaultSlots<TValue>> component,
                    SlotHost.IVNode child)
                    => SlotHost.h(component, child);
            }
            """,
            hasProps: false),
        Success(
            "typed-props-and-slots",
            "two-argument-component-contract-validates-concrete-slot-type",
            ChildrenToSlotSuccessKind.TypedPropsAndSlots,
            """
            public sealed class DialogProps : SlotHost.VueProps
            {
            }

            public sealed class DialogSlots
            {
                [System.ComponentModel.Description("@#default")]
                public SlotHost.Slot Default => null!;
            }

            public static class ScenarioModule
            {
                public static SlotHost.IVNode Invoke(
                    SlotHost.IVueComponent<DialogProps, DialogSlots> component,
                    DialogProps props,
                    SlotHost.IVNode child)
                    => SlotHost.h(component, props, child);
            }
            """,
            hasProps: true)
    ];

    public static IReadOnlyList<ChildrenToSlotRejectionScenario> Rejections { get; } =
    [
        Rejection(
            "instance-render-factory",
            "render-factory-must-be-static",
            ChildrenToSlotRejectionKind.InstanceRenderFactory,
            """
            public sealed class InstanceHost
            {
                public interface IVNode
                {
                }

                public interface IVueComponent
                {
                }

                public IVNode h(IVueComponent component, IVNode child) => child;
            }

            public static class ScenarioModule
            {
                public static InstanceHost.IVNode Invoke(
                    InstanceHost host,
                    InstanceHost.IVueComponent component,
                    InstanceHost.IVNode child)
                    => host.h(component, child);
            }
            """),
        Rejection(
            "wrong-runtime-name",
            "ordinary-static-method-with-non-h-name",
            ChildrenToSlotRejectionKind.WrongRuntimeName,
            """
            public static partial class SlotHost
            {
                public static extern IVNode render(IVueComponent component, IVNode child);
            }

            public static class ScenarioModule
            {
                public static SlotHost.IVNode Invoke(
                    SlotHost.IVueComponent component,
                    SlotHost.IVNode child)
                    => SlotHost.render(component, child);
            }
            """),
        Rejection(
            "blank-module-path",
            "render-factory-requires-nonblank-module-import",
            ChildrenToSlotRejectionKind.BlankModulePath,
            StandardInvocationSource,
            modulePath: " "),
        Rejection(
            "missing-node-contract",
            "host-contract-must-declare-node-type",
            ChildrenToSlotRejectionKind.MissingNodeContract,
            """
            public static class NoNodeHost
            {
                public interface IVueComponent
                {
                }

                public static extern int h(IVueComponent component, int child);
            }

            public static class ScenarioModule
            {
                public static int Invoke(NoNodeHost.IVueComponent component, int child)
                    => NoNodeHost.h(component, child);
            }
            """),
        Rejection(
            "unsupported-arity",
            "default-slot-sugar-only-classifies-two-or-three-parameter-overloads",
            ChildrenToSlotRejectionKind.UnsupportedArity,
            """
            public static partial class SlotHost
            {
                [System.ComponentModel.Description("@#h")]
                public static extern IVNode h(
                    IVueComponent component,
                    VueProps props,
                    IVNode child,
                    bool hydrate);
            }

            public static class ScenarioModule
            {
                public static SlotHost.IVNode Invoke(
                    SlotHost.IVueComponent component,
                    SlotHost.VueProps props,
                    SlotHost.IVNode child)
                    => SlotHost.h(component, props, child, true);
            }
            """),
        Rejection(
            "array-receiver",
            "component-receiver-must-be-named-host-contract",
            ChildrenToSlotRejectionKind.ArrayReceiver,
            """
            public static partial class SlotHost
            {
                [System.ComponentModel.Description("@#h")]
                public static extern IVNode h(IVueComponent[] components, IVNode child);
            }

            public static class ScenarioModule
            {
                public static SlotHost.IVNode Invoke(
                    SlotHost.IVueComponent[] components,
                    SlotHost.IVNode child)
                    => SlotHost.h(components, child);
            }
            """),
        Rejection(
            "unsupported-child",
            "date-time-is-not-a-vue-child-domain",
            ChildrenToSlotRejectionKind.UnsupportedChild,
            """
            public static partial class SlotHost
            {
                [System.ComponentModel.Description("@#h")]
                public static extern IVNode h(IVueComponent component, System.DateTime child);
            }

            public static class ScenarioModule
            {
                public static SlotHost.IVNode Invoke(
                    SlotHost.IVueComponent component,
                    System.DateTime child)
                    => SlotHost.h(component, child);
            }
            """),
        Rejection(
            "missing-props-contract",
            "three-parameter-host-must-declare-props-base",
            ChildrenToSlotRejectionKind.MissingPropsContract,
            """
            public static class NoPropsHost
            {
                public interface IVNode
                {
                }

                public interface IVueComponent
                {
                }

                public static extern IVNode h(IVueComponent component, string props, IVNode child);
            }

            public static class ScenarioModule
            {
                public static NoPropsHost.IVNode Invoke(
                    NoPropsHost.IVueComponent component,
                    string props,
                    NoPropsHost.IVNode child)
                    => NoPropsHost.h(component, props, child);
            }
            """),
        Rejection(
            "array-props",
            "props-parameter-must-be-named-or-constrained-type",
            ChildrenToSlotRejectionKind.ArrayProps,
            """
            public static partial class SlotHost
            {
                [System.ComponentModel.Description("@#h")]
                public static extern IVNode h(IVueComponent component, IVNode[] props, IVNode child);
            }

            public static class ScenarioModule
            {
                public static SlotHost.IVNode Invoke(
                    SlotHost.IVueComponent component,
                    SlotHost.IVNode[] props,
                    SlotHost.IVNode child)
                    => SlotHost.h(component, props, child);
            }
            """),
        Rejection(
            "translated-argument-count",
            "translated-ast-arguments-must-match-method-contract",
            ChildrenToSlotRejectionKind.TranslatedArgumentCount,
            StandardInvocationSource,
            argumentCountOverride: 1)
    ];

    public static IReadOnlyList<ChildrenToSlotImportFailureScenario> ImportFailures { get; } =
    [
        ImportFailure(
            "builder-rejected",
            "runtime-import-builder-declines-host-member",
            ChildrenToSlotImportFailureKind.BuilderRejected,
            ChildrenToSlotImportBehavior.ReturnFalse),
        ImportFailure(
            "missing-factory-expression",
            "runtime-import-builder-returns-no-ast-expression",
            ChildrenToSlotImportFailureKind.MissingFactoryExpression,
            ChildrenToSlotImportBehavior.ReturnNull)
    ];

    public static IReadOnlyList<ChildrenToSlotAuthoringFailureScenario> AuthoringFailures { get; } =
    [
        AuthoringFailure(
            "non-generic-concrete-slot-missing-default",
            "concrete-slot-contract-on-non-generic-adapter-is-still-validated",
            ChildrenToSlotAuthoringFailureKind.NonGenericConcreteSlotMissingDefault,
            """
            public sealed class MissingDefaultSlots
            {
                public SlotHost.Slot Header => null!;
            }

            public static partial class SlotHost
            {
                [System.ComponentModel.Description("@#h")]
                public static extern IVNode h(
                    IVueSlotComponent<MissingDefaultSlots> component,
                    IVNode child);
            }

            public static class ScenarioModule
            {
                public static SlotHost.IVNode Invoke(
                    SlotHost.IVueSlotComponent<MissingDefaultSlots> component,
                    SlotHost.IVNode child)
                    => SlotHost.h(component, child);
            }
            """,
            ["MissingDefaultSlots", "does not declare an explicit default slot"]),
        AuthoringFailure(
            "excluded-default-slot-members",
            "static-private-indexer-and-write-only-properties-do-not-form-slot-contract",
            ChildrenToSlotAuthoringFailureKind.ExcludedDefaultSlotMembers,
            """
            public sealed class FilteredSlots
            {
                [System.ComponentModel.Description("@#default")]
                public static SlotHost.Slot StaticDefault => null!;

                [System.ComponentModel.Description("@#default")]
                private SlotHost.Slot PrivateDefault => null!;

                [System.ComponentModel.Description("@#default")]
                public SlotHost.Slot this[int index] => null!;

                [System.ComponentModel.Description("@#default")]
                public SlotHost.Slot WriteOnlyDefault
                {
                    set
                    {
                    }
                }
            }

            public static class ScenarioModule
            {
                public static SlotHost.IVNode Invoke(
                    SlotHost.IVueSlotComponent<FilteredSlots> component,
                    SlotHost.IVNode child)
                    => SlotHost.h(component, child);
            }
            """,
            ["FilteredSlots", "does not declare an explicit default slot"]),
        AuthoringFailure(
            "array-valued-default-slot",
            "default-slot-member-must-be-a-node-returning-delegate",
            ChildrenToSlotAuthoringFailureKind.ArrayValuedDefaultSlot,
            """
            public sealed class ArrayDefaultSlots
            {
                [System.ComponentModel.Description("@#default")]
                public SlotHost.IVNode[] Content => [];
            }

            public static class ScenarioModule
            {
                public static SlotHost.IVNode Invoke(
                    SlotHost.IVueSlotComponent<ArrayDefaultSlots> component,
                    SlotHost.IVNode child)
                    => SlotHost.h(component, child);
            }
            """,
            ["ArrayDefaultSlots.Content", "must be a delegate returning the host IVNode type"]),
        AuthoringFailure(
            "open-slot-type-parameter",
            "open-slot-component-type-parameter-cannot-prove-a-default-slot-contract",
            ChildrenToSlotAuthoringFailureKind.OpenSlotTypeParameter,
            """
            public static class ScenarioModule
            {
                public static SlotHost.IVNode Invoke<TSlots>(
                    SlotHost.IVueSlotComponent<TSlots> component,
                    SlotHost.IVNode child)
                    => SlotHost.h(component, child);
            }
            """,
            ["requires a typed slot contract"]),
        AuthoringFailure(
            "open-typed-component-slot-type-parameter",
            "open-typed-component-slot-parameter-cannot-prove-a-default-slot-contract",
            ChildrenToSlotAuthoringFailureKind.OpenTypedComponentSlotTypeParameter,
            """
            public static class ScenarioModule
            {
                public static SlotHost.IVNode Invoke<TProps, TSlots>(
                    SlotHost.IVueComponent<TProps, TSlots> component,
                    TProps props,
                    SlotHost.IVNode child)
                    where TProps : SlotHost.VueProps
                    => SlotHost.h(component, props, child);
            }
            """,
            ["requires a typed slot contract"])
    ];

    private const string StandardInvocationSource = """
        public static class ScenarioModule
        {
            public static SlotHost.IVNode Invoke(
                SlotHost.IVueSlotComponent<SlotHost.DefaultSlots> component,
                SlotHost.IVNode child)
                => SlotHost.h(component, child);
        }
        """;

    private static ChildrenToSlotSuccessScenario Success(
        string id,
        string dimension,
        ChildrenToSlotSuccessKind kind,
        string source,
        bool hasProps,
        string modulePath = "runtime",
        ChildrenToSlotChildExpressionKind childExpressionKind = ChildrenToSlotChildExpressionKind.Identifier)
        => new(
            $"children-to-slot.success.{id}",
            dimension,
            kind,
            source,
            modulePath,
            hasProps,
            childExpressionKind);

    private static ChildrenToSlotRejectionScenario Rejection(
        string id,
        string dimension,
        ChildrenToSlotRejectionKind kind,
        string source,
        string modulePath = "runtime",
        int? argumentCountOverride = null)
        => new(
            $"children-to-slot.rejection.{id}",
            dimension,
            kind,
            source,
            modulePath,
            argumentCountOverride);

    private static ChildrenToSlotImportFailureScenario ImportFailure(
        string id,
        string dimension,
        ChildrenToSlotImportFailureKind kind,
        ChildrenToSlotImportBehavior importBehavior)
        => new(
            $"children-to-slot.import-failure.{id}",
            dimension,
            kind,
            StandardInvocationSource,
            importBehavior,
            ["SlotHost.h", "解析运行时导入"]);

    private static ChildrenToSlotAuthoringFailureScenario AuthoringFailure(
        string id,
        string dimension,
        ChildrenToSlotAuthoringFailureKind kind,
        string source,
        IReadOnlyList<string> expectedMessageFragments)
        => new(
            $"children-to-slot.authoring-failure.{id}",
            dimension,
            kind,
            source,
            expectedMessageFragments);
}
