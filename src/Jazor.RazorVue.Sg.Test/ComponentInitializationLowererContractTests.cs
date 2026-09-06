using System.Collections.Immutable;
using System.Reflection;
using Acornima.Ast;
using Jazor.Compiler;
using Jazor.RazorVue.Generation;
using Jazor.RazorVue.RazorSdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class ComponentInitializationLowererContractTests
{
    [TestMethod]
    public void Build_ReturnsEmptyWithoutExplicitSourceConstructors()
    {
        var fixture = CreateFixture();

        var result = Build(fixture.NoConstructor);

        Assert.IsEmpty(result.ImportDeclarations);
        Assert.IsEmpty(result.Phases);
        Assert.IsFalse(result.HasExplicitConstructors);
    }

    [TestMethod]
    public void Build_ReplaysInitializerOnlyBlockAndExpressionConstructorPhases()
    {
        var fixture = CreateFixture();
        var existingImports = CreateExistingImports();

        var result = Build(fixture.Initialized, existingImports);

        Assert.HasCount(3, result.Phases);
        Assert.IsTrue(result.HasExplicitConstructors);
        Assert.IsNull(result.Phases[0].ConstructorStatement);
        Assert.IsNotNull(result.Phases[1].ConstructorStatement);
        Assert.IsNotNull(result.Phases[2].ConstructorStatement);
        Assert.IsEmpty(result.ImportDeclarations);
    }

    [TestMethod]
    public void Build_StopsBeforeConstructorReplayWhenCancellationIsRequested()
    {
        var fixture = CreateFixture();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        Assert.Throws<OperationCanceledException>(() =>
            Build(fixture.Initialized, cancellationToken: cancellation.Token));
    }

    [TestMethod]
    public void Build_AllocatesConstructorImportsWithoutCapturingComponentStateNames()
    {
        var fixture = CreateFixture();
        var stateReference = fixture.Importing.Closure.ComponentSymbol
            .GetMembers("ref")
            .OfType<IFieldSymbol>()
            .Single();
        var declaredNames = new Dictionary<ISymbol, string>(SymbolEqualityComparer.Default)
        {
            [stateReference] = "ref"
        };

        var result = Build(fixture.Importing, declaredNames: declaredNames);

        Assert.HasCount(1, result.ImportDeclarations);
        var specifier = Assert.IsInstanceOfType<ImportSpecifier>(
            result.ImportDeclarations[0].Specifiers.Single());
        Assert.AreEqual("ref", Assert.IsInstanceOfType<Identifier>(specifier.Imported).Name);
        var local = specifier.Local.Name;
        Assert.AreNotEqual("ref", local);
        Assert.IsTrue(result.HasExplicitConstructors);
        StringAssert.Contains(
            result.Phases.Single(static phase => phase.ConstructorStatement is not null)
                .ConstructorStatement!
                .ToKnRECMAScript(),
            local + "(3)",
            StringComparison.Ordinal);
    }

    [TestMethod]
    public void Build_ReusesExistingConstructorImportBinding()
    {
        var fixture = CreateFixture();
        var existingImports = ImportDeclarationFactory.Create(
            "vue",
            [new ImportSpecifier(new Identifier("ref"), new Identifier("existingRef"))]);

        var result = Build(fixture.Importing, existingImports);

        Assert.IsEmpty(result.ImportDeclarations);
        StringAssert.Contains(
            result.Phases.Single(static phase => phase.ConstructorStatement is not null)
                .ConstructorStatement!
                .ToKnRECMAScript(),
            "existingRef(3)",
            StringComparison.Ordinal);
    }

    [TestMethod]
    public void Build_LowersReferenceServiceConstructorThroughTypedVueInject()
    {
        var fixture = CreateFixture();

        var result = Build(fixture.ServiceConstructor);

        var phase = result.Phases.Single(static candidate => candidate.ConstructorStatement is not null);
        Assert.HasCount(1, phase.ConstructorParameters);
        Assert.AreEqual("service", phase.ConstructorParameters[0].Name);
        Assert.AreEqual("jazor:service:InitializationContracts.BrowserService", phase.ConstructorParameters[0].ServiceKey);
        var script = phase.ConstructorStatement!.ToKnRECMAScript();
        StringAssert.Contains(script, "inject(\"jazor:service:InitializationContracts.BrowserService\")", StringComparison.Ordinal);
        StringAssert.Contains(script, "service", StringComparison.Ordinal);
    }

    [TestMethod]
    public void PrivateLoweringHelpers_RejectImplicitConstructorWithoutSourceBody()
    {
        var fixture = CreateFixture();
        var implicitConstructor = fixture.NoConstructor.Closure.ComponentSymbol.InstanceConstructors.Single();

        var failure = Assert.Throws<TargetInvocationException>(() =>
            Invoke<Statement>(
                "LowerConstructorBody",
                fixture.NoConstructor.Compilation,
                fixture.NoConstructor.Closure,
                implicitConstructor,
                new Dictionary<ISymbol, string>(SymbolEqualityComparer.Default),
                new SenseArgument(Sense.FunctionBody, UseImportAliases: true),
                CancellationToken.None));

        StringAssert.Contains(
            failure.InnerException!.Message,
            "without a lowerable source body",
            StringComparison.Ordinal);
    }

    [TestMethod]
    public void PrivateLoweringHelpers_RejectExplicitConstructorDeclarationWithoutBody()
    {
        var fixture = CreateFixture();
        var source = CSharpSyntaxTree.ParseText(
            "namespace MissingConstructorBody { public sealed class Shape { public Shape(); } }",
            new CSharpParseOptions(LanguageVersion.Preview),
            path: "MissingConstructorBody.cs");
        var compilation = CSharpCompilation.Create(
            "Jazor.RazorVue.MissingConstructorBody.Contracts",
            [source],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var shape = compilation.GetTypeByMetadataName("MissingConstructorBody.Shape");
        Assert.IsNotNull(shape);
        var constructor = shape!.InstanceConstructors.Single(static candidate => !candidate.IsImplicitlyDeclared);

        var failure = Assert.Throws<TargetInvocationException>(() =>
            Invoke<Statement>(
                "LowerConstructorBody",
                compilation,
                fixture.NoConstructor.Closure,
                constructor,
                new Dictionary<ISymbol, string>(SymbolEqualityComparer.Default),
                new SenseArgument(Sense.FunctionBody, UseImportAliases: true),
                CancellationToken.None));

        StringAssert.Contains(
            failure.InnerException!.Message,
            "without a lowerable source body",
            StringComparison.Ordinal);
    }

    [TestMethod]
    public void PrivateLoweringHelpers_MaterializeEveryAcceptedBodyShapeAndPreserveImportBindings()
    {
        var functionBody = new FunctionBody(NodeList.Empty<Statement>(), strict: true);
        Assert.AreSame(
            functionBody,
            Invoke<FunctionBody>("MaterializeFunctionBody", functionBody, new SenseArgument()));

        var nestedBlock = new NestedBlockStatement(NodeList.From<Statement>(
            new NonSpecialExpressionStatement(new Identifier("nested"))));
        Assert.AreEqual(
            1,
            Invoke<FunctionBody>("MaterializeFunctionBody", nestedBlock, new SenseArgument()).Body.Count);

        var statement = new NonSpecialExpressionStatement(new Identifier("statement"));
        Assert.AreEqual(
            1,
            Invoke<FunctionBody>("MaterializeFunctionBody", statement, new SenseArgument()).Body.Count);

        var expressionWithoutTemporaries = Invoke<FunctionBody>(
            "MaterializeFunctionBody",
            new Identifier("plainExpression"),
            new SenseArgument());
        Assert.HasCount(1, expressionWithoutTemporaries.Body);
        Assert.IsInstanceOfType<NonSpecialExpressionStatement>(expressionWithoutTemporaries.Body[0]);

        var expressionArgument = new SenseArgument();
        expressionArgument.AddVarDeclarator(
            new VariableDeclarator(new Identifier("temporary"), new NumericLiteral(1, "1")),
            depth: 0);
        var materializedExpression = Invoke<FunctionBody>(
            "MaterializeFunctionBody",
            new Identifier("expression"),
            expressionArgument);
        Assert.AreEqual(2, materializedExpression.Body.Count);
        Assert.IsInstanceOfType<VariableDeclaration>(materializedExpression.Body[0]);
        Assert.IsInstanceOfType<NonSpecialExpressionStatement>(materializedExpression.Body[1]);

        var unsupportedBody = Assert.Throws<TargetInvocationException>(() =>
            Invoke<FunctionBody>(
                "MaterializeFunctionBody",
                new ImportDefaultSpecifier(new Identifier("unsupported")),
                new SenseArgument()));
        StringAssert.Contains(
            unsupportedBody.InnerException!.Message,
            "unsupported compiler body node",
            StringComparison.Ordinal);

        var importBindings = new Dictionary<string, string>(StringComparer.Ordinal);
        var importLocalBindings = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var declaration in CreateExistingImports())
        {
            InvokeVoid(
                "AddExistingImportBindings",
                declaration,
                importBindings,
                importLocalBindings);
        }

        Assert.AreEqual("defaultLocal", importBindings["default-module\0default"]);
        Assert.AreEqual("namespaceLocal", importBindings["namespace-module\0*"]);
        Assert.AreEqual("namedLocal", importBindings["named-module\0named"]);
        Assert.AreEqual("literalLocal", importBindings["named-module\0literal-name"]);
        Assert.AreEqual("namedLocal", importBindings["other-module\0other"]);
        Assert.AreEqual("named-module\0named", importLocalBindings["namedLocal"]);

        var malformed = new ImportDeclaration(
            NodeList.From<ImportDeclarationSpecifier>(
                new ImportSpecifier(new NumericLiteral(1, "1"), new Identifier("one"))),
            new StringLiteral("malformed-module", "\"malformed-module\""),
            NodeList.From<ImportAttribute>());
        var malformedImport = Assert.Throws<TargetInvocationException>(() =>
            InvokeVoid("AddExistingImportBindings", malformed, importBindings, importLocalBindings));
        Assert.IsInstanceOfType<NotSupportedException>(malformedImport.InnerException);
    }

    private static ComponentInitializationBuildResult Build(
        ComponentFixture fixture,
        IEnumerable<ImportDeclaration>? existingImports = null,
        IReadOnlyDictionary<ISymbol, string>? declaredNames = null,
        CancellationToken cancellationToken = default)
        => ComponentInitializationLowerer.Build(
            fixture.Compilation,
            fixture.Closure,
            declaredNames ?? new Dictionary<ISymbol, string>(SymbolEqualityComparer.Default),
            existingImports ?? Enumerable.Empty<ImportDeclaration>(),
            ["state", "props"],
            cancellationToken);

    private static ImmutableArray<ImportDeclaration> CreateExistingImports()
        =>
        [
            .. ImportDeclarationFactory.Create(
                "default-module",
                [new ImportDefaultSpecifier(new Identifier("defaultLocal"))]),
            .. ImportDeclarationFactory.Create(
                "namespace-module",
                [new ImportNamespaceSpecifier(new Identifier("namespaceLocal"))]),
            .. ImportDeclarationFactory.Create(
                "named-module",
                [
                    new ImportSpecifier(new Identifier("named"), new Identifier("namedLocal")),
                    new ImportSpecifier(
                        new StringLiteral("literal-name", "\"literal-name\""),
                        new Identifier("literalLocal"))
                ]),
            .. ImportDeclarationFactory.Create(
                "named-module",
                [new ImportSpecifier(new Identifier("named"), new Identifier("ignoredDuplicate"))]),
            .. ImportDeclarationFactory.Create(
                "other-module",
                [new ImportSpecifier(new Identifier("other"), new Identifier("namedLocal"))]),
            .. ImportDeclarationFactory.Create(
                "other-module",
                [new ImportSpecifier(new Identifier("other"), new Identifier("otherLocal"))])
        ];

    private static Fixture CreateFixture()
    {
        var source = CSharpSyntaxTree.ParseText(
            """
            using ECMAScript;
            using static ECMAScript.Vue;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace InitializationContracts;

            public abstract class InitializerOnlyBase : ComponentBase
            {
                protected int baseCounter = 3;
            }

            public abstract class BlockConstructorBase : InitializerOnlyBase
            {
                protected BlockConstructorBase()
                {
                    baseCounter = 7;
                }
            }

            public sealed class BrowserService
            {
                public int Value { get; } = 7;
            }

            [ECMAScriptModule("./components/service-constructor")]
            public sealed class ServiceConstructorComponent : ComponentBase, IVueComponent
            {
                private readonly BrowserService service;

                public ServiceConstructorComponent(BrowserService service)
                {
                    this.service = service;
                }

                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.AddContent(0, service.Value);
                }
            }

            [ECMAScriptModule("./components/initialized")]
            public sealed class InitializedComponent : BlockConstructorBase, IVueComponent
            {
                private int counter;

                public InitializedComponent() => counter = baseCounter + 1;

                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.AddContent(0, counter);
                }
            }

            [ECMAScriptModule("./components/no-constructor")]
            public sealed class NoConstructorComponent : ComponentBase, IVueComponent
            {
                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.AddContent(0, "none");
                }
            }

            [ECMAScriptModule("./components/importing-constructor")]
            public sealed class ImportingConstructorComponent : ComponentBase, IVueComponent
            {
                private IVueRef<int>? @ref;

                public ImportingConstructorComponent()
                {
                    @ref = Ref(3);
                }

                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.AddContent(0, @ref!.Value);
                }
            }
            """,
            new CSharpParseOptions(LanguageVersion.Preview),
            path: "ComponentInitializationContracts.razor.g.cs");
        var compilation = CSharpCompilation.Create(
            "Jazor.RazorVue.ComponentInitialization.Contracts",
            [source],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = RazorSgTestHost.GetCompilationErrors(compilation);
        Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors));

        var initializedType = compilation.GetTypeByMetadataName("InitializationContracts.InitializedComponent");
        var noConstructorType = compilation.GetTypeByMetadataName("InitializationContracts.NoConstructorComponent");
        var importingType = compilation.GetTypeByMetadataName("InitializationContracts.ImportingConstructorComponent");
        var serviceConstructorType = compilation.GetTypeByMetadataName("InitializationContracts.ServiceConstructorComponent");
        Assert.IsNotNull(initializedType);
        Assert.IsNotNull(noConstructorType);
        Assert.IsNotNull(importingType);
        Assert.IsNotNull(serviceConstructorType);
        Assert.IsTrue(
            GeneratedCSharpBinder.TryBindFinalCompilation(
            compilation,
                ImmutableArray.Create(initializedType!, noConstructorType!, importingType!, serviceConstructorType!),
                out var binding,
                out var bindingFailure),
            bindingFailure);
        Assert.IsNotNull(binding);

        var components = binding!.Components.ToDictionary(
            static component => component.ComponentSymbol.Name,
            StringComparer.Ordinal);
        return new Fixture(
            CreateComponentFixture(compilation, binding, components[initializedType.Name]),
            CreateComponentFixture(compilation, binding, components[noConstructorType.Name]),
            CreateComponentFixture(compilation, binding, components[importingType.Name]),
            CreateComponentFixture(compilation, binding, components[serviceConstructorType.Name]));
    }

    private static ComponentFixture CreateComponentFixture(
        Compilation compilation,
        GeneratedCSharpBinding binding,
        BoundComponent component)
    {
        Assert.IsTrue(
            MemberClosureBuilder.TryBuild(binding, component, out var closure, out var failure),
            failure);
        Assert.IsNotNull(closure);
        return new ComponentFixture(compilation, closure!);
    }

    private static T Invoke<T>(string methodName, params object?[] arguments)
    {
        var method = typeof(ComponentInitializationLowerer)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
            .Single(candidate => candidate.Name == methodName && candidate.GetParameters().Length == arguments.Length);
        return (T)method.Invoke(null, arguments)!;
    }

    private static void InvokeVoid(string methodName, params object?[] arguments)
        => _ = Invoke<object?>(methodName, arguments);

    private sealed record Fixture(
        ComponentFixture Initialized,
        ComponentFixture NoConstructor,
        ComponentFixture Importing,
        ComponentFixture ServiceConstructor);

    private sealed record ComponentFixture(
        Compilation Compilation,
        MemberClosure Closure);
}
