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
public sealed class CurrentComponentSemanticWalkerHostPrivateContractTests
{
    [TestMethod]
    public void TypeAndPropertyHelpers_ClassifyCurrentComponentContractsFromRoslynSymbols()
    {
        var fixture = CreateFixture();
        var component = GetNamedType(fixture, "ComponentUnderTest");
        var changed = GetProperty(component, "Changed");
        var genericChanged = GetProperty(component, "GenericChanged");
        var title = GetProperty(component, "Title");
        var count = GetProperty(component, "Count");
        var computed = GetProperty(component, "Computed");
        var operations = GetMethod(fixture, "ComponentUnderTest", "Operations");
        var propertyReference = GetVariableInitializer(fixture, operations, "property");
        var convertedPropertyReference = GetVariableInitializer(fixture, operations, "convertedProperty");
        var staticPropertyReference = GetVariableInitializer(fixture, operations, "staticValue");
        var sameNamedOtherType = fixture.Compilation.GetTypeByMetadataName("CurrentComponentPrivateContracts.Other+EventCallback");
        Assert.IsNotNull(sameNamedOtherType);

        Assert.IsTrue(InvokeStatic<bool>("IsEventCallbackType", changed.Type));
        Assert.IsTrue(InvokeStatic<bool>("IsEventCallbackType", genericChanged.Type));
        Assert.IsFalse(InvokeStatic<bool>("IsEventCallbackType", title.Type));
        Assert.IsFalse(InvokeStatic<bool>("IsEventCallbackType", sameNamedOtherType!));
        Assert.IsFalse(InvokeStatic<bool>("IsEventCallbackType", GetNamedType(fixture, "GenericHolder`1").TypeParameters.Single()));

        Assert.IsTrue(InvokeStatic<bool>("IsParameterProperty", changed));
        Assert.IsTrue(InvokeStatic<bool>("IsParameterProperty", title));
        Assert.IsFalse(InvokeStatic<bool>("IsParameterProperty", count));
        Assert.IsTrue(InvokeStatic<bool>("IsAutoProperty", count));
        Assert.IsFalse(InvokeStatic<bool>("IsAutoProperty", computed));

        Assert.AreSame(count, InvokeStatic<IPropertySymbol?>("UnwrapPropertyReference", propertyReference));
        Assert.AreSame(count, InvokeStatic<IPropertySymbol?>("UnwrapPropertyReference", convertedPropertyReference));
        Assert.IsNull(InvokeStatic<IPropertySymbol?>("UnwrapPropertyReference", GetVariableInitializer(fixture, operations, "literal")));
        Assert.IsNotNull(InvokeStatic<IOperation?>("GetPropertyInstance", propertyReference));
        Assert.IsNotNull(InvokeStatic<IOperation?>("GetPropertyInstance", convertedPropertyReference));
        Assert.IsNull(InvokeStatic<IOperation?>("GetPropertyInstance", staticPropertyReference));
        Assert.IsNull(InvokeStatic<IOperation?>("GetPropertyInstance", new object?[] { null }));

        var eventCallback = fixture.Compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Components.EventCallback");
        var eventCallbackFactory = fixture.Compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Components.EventCallbackFactory");
        var bindConverter = fixture.Compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Components.BindConverter");
        var runtimeHelpers = fixture.Compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers");
        Assert.IsNotNull(eventCallback);
        Assert.IsNotNull(eventCallbackFactory);
        Assert.IsNotNull(bindConverter);
        Assert.IsNotNull(runtimeHelpers);

        var invokeAsync = eventCallback!.GetMembers("InvokeAsync").OfType<IMethodSymbol>().First();
        var factoryCreate = eventCallbackFactory!.GetMembers("Create").OfType<IMethodSymbol>().First();
        var factoryCreateBinder = GetSingleInvocation(fixture, "ComponentUnderTest", "BinderFactory").TargetMethod;
        var formatValue = bindConverter!.GetMembers("FormatValue").OfType<IMethodSymbol>().First();
        var typeCheck = GetSingleInvocation(fixture, "ComponentUnderTest", "TypeCheckCall").TargetMethod;
        var unrelated = GetMethodSymbol(fixture, "ComponentUnderTest", "Other");

        Assert.IsTrue(InvokeStatic<bool>("IsEventCallbackInvoke", invokeAsync));
        Assert.IsFalse(InvokeStatic<bool>("IsEventCallbackInvoke", unrelated));
        Assert.IsTrue(InvokeStatic<bool>("IsEventCallbackFactoryCreate", factoryCreate));
        Assert.IsFalse(InvokeStatic<bool>("IsEventCallbackFactoryCreate", factoryCreateBinder));
        Assert.IsTrue(InvokeStatic<bool>("IsEventCallbackFactoryCreateBinder", factoryCreateBinder));
        Assert.IsFalse(InvokeStatic<bool>("IsEventCallbackFactoryCreateBinder", factoryCreate));
        Assert.IsTrue(InvokeStatic<bool>("IsBindConverterFormatValue", formatValue));
        Assert.IsFalse(InvokeStatic<bool>("IsBindConverterFormatValue", unrelated));
        Assert.IsTrue(InvokeStatic<bool>("IsRazorRuntimeHelpersMethod", typeCheck, "TypeCheck"));
        Assert.IsFalse(InvokeStatic<bool>("IsRazorRuntimeHelpersMethod", typeCheck, "Other"));
        Assert.IsTrue(InvokeStatic<bool>("IsRazorRuntimeHelpersTypeCheck", typeCheck));
        Assert.IsFalse(InvokeStatic<bool>("IsRazorRuntimeHelpersTypeCheck", unrelated));
    }

    [TestMethod]
    public void BinderAndCurrentComponentHelpers_RecognizeBoundOperationShapes()
    {
        var fixture = CreateFixture();
        var component = GetNamedType(fixture, "ComponentUnderTest");
        var singleBinder = GetMethodBody(fixture, "ComponentUnderTest", "SingleBinder");
        var multipleBinder = GetMethodBody(fixture, "ComponentUnderTest", "MultipleBinder");
        var returnBinder = GetMethodBody(fixture, "ComponentUnderTest", "ReturnBinder");
        var emptyReturn = GetReturnOperation(fixture, "ComponentUnderTest", "EmptyReturn");
        var operations = GetMethod(fixture, "ComponentUnderTest", "Operations");
        var literal = GetVariableInitializer(fixture, operations, "literal");

        Assert.IsNotNull(InvokeStatic<ISimpleAssignmentOperation?>("TryGetSingleBinderAssignment", singleBinder));
        Assert.IsNull(InvokeStatic<ISimpleAssignmentOperation?>("TryGetSingleBinderAssignment", multipleBinder));
        Assert.IsNotNull(InvokeStatic<ISimpleAssignmentOperation?>("TryGetSingleBinderAssignment", returnBinder));
        Assert.IsNotNull(InvokeStatic<ISimpleAssignmentOperation?>("TryGetBinderAssignment", singleBinder.Operations[0]));
        Assert.IsNotNull(InvokeStatic<ISimpleAssignmentOperation?>("TryGetBinderAssignment", returnBinder.Operations[0]));
        Assert.IsNull(InvokeStatic<ISimpleAssignmentOperation?>("TryGetBinderAssignment", literal));
        Assert.IsTrue(InvokeStatic<bool>("IsEmptyReturn", emptyReturn));
        Assert.IsFalse(InvokeStatic<bool>("IsEmptyReturn", returnBinder.Operations[0]));
        Assert.IsFalse(InvokeStatic<bool>("IsEmptyReturn", literal));

        var host = new CurrentComponentSemanticWalkerHost(component);
        var stateChangedInvocation = GetOperation<IInvocationOperation>(
            fixture,
            GetMethod(fixture, "ComponentUnderTest", "StateChangedCaller")
                .DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Single());
        Assert.IsNotNull(stateChangedInvocation.Instance);
        Assert.IsTrue(InvokeInstance<bool>(host, "IsStateHasChangedInvocation", stateChangedInvocation.TargetMethod, stateChangedInvocation.Instance));
        Assert.IsTrue(InvokeInstance<bool>(host, "IsStateHasChangedInvocation", stateChangedInvocation.TargetMethod, null));
        Assert.IsFalse(InvokeInstance<bool>(host, "IsStateHasChangedInvocation", stateChangedInvocation.TargetMethod, literal));
        Assert.IsFalse(InvokeInstance<bool>(host, "IsStateHasChangedInvocation", GetMethodSymbol(fixture, "ComponentUnderTest", "StateHasChanged", 1, isStatic: false), stateChangedInvocation.Instance));
        Assert.IsFalse(InvokeInstance<bool>(host, "IsStateHasChangedInvocation", GetMethodSymbol(fixture, "ComponentUnderTest", "StateHasChanged", 1, isStatic: true), null));
        var invokeAsyncInvocation = GetSingleInvocation(fixture, "ComponentUnderTest", "InvokeAsyncCaller");
        Assert.IsTrue(InvokeInstance<bool>(host, "IsComponentBaseInvokeAsyncInvocation", invokeAsyncInvocation.TargetMethod, invokeAsyncInvocation.Instance));
        Assert.IsFalse(InvokeInstance<bool>(host, "IsComponentBaseInvokeAsyncInvocation", GetMethodSymbol(fixture, "ComponentUnderTest", "InvokeAsync", 1), invokeAsyncInvocation.Instance));
        Assert.IsTrue(InvokeInstance<bool>(host, "IsCurrentComponentReceiver", stateChangedInvocation.Instance));
        Assert.IsFalse(InvokeInstance<bool>(host, "IsCurrentComponentReceiver", literal));
        Assert.IsTrue(InvokeInstance<bool>(host, "IsCurrentComponentInstance", true, null));
        Assert.IsFalse(InvokeInstance<bool>(host, "IsCurrentComponentInstance", true, stateChangedInvocation.Instance));
        Assert.IsTrue(InvokeInstance<bool>(host, "IsCurrentComponentInstance", false, null));
        Assert.IsTrue(InvokeInstance<bool>(host, "IsCurrentComponentInstance", false, stateChangedInvocation.Instance));

        var fieldBinderInvocation = GetSingleInvocation(fixture, "ComponentUnderTest", "BinderFactoryWithField");
        var binderArguments = new object?[] { fieldBinderInvocation, null, null };
        Assert.IsTrue(InvokeStatic<bool>("TryGetCreateBinderReceiverAndHandler", binderArguments));
        Assert.IsInstanceOfType<IConversionOperation>(binderArguments[1]);
        Assert.IsInstanceOfType<IInstanceReferenceOperation>(((IConversionOperation)binderArguments[1]!).Operand);
        Assert.IsInstanceOfType<IDelegateCreationOperation>(binderArguments[2]);
        Assert.IsFalse(InvokeStatic<bool>(
            "TryGetCreateBinderReceiverAndHandler",
            new object?[] { GetSingleInvocation(fixture, "ComponentUnderTest", "FactoryCreate"), null, null }));
        var binderDiagnostic = InvokeStatic<Exception>(
            "CreateUnsupportedEventCallbackFactoryCreateBinderException",
            fieldBinderInvocation);
        StringAssert.Contains(binderDiagnostic.Message, "Handler operation kind: DelegateCreation", StringComparison.Ordinal);
        StringAssert.Contains(binderDiagnostic.Message, "Anonymous body operation kinds", StringComparison.Ordinal);
    }

    [TestMethod]
    public void RewriteHooks_ProjectCurrentComponentStorageCallbacksAndFrameworkCalls()
    {
        var fixture = CreateFixture();
        var component = GetNamedType(fixture, "ComponentUnderTest");
        var count = GetProperty(component, "Count");
        var title = GetProperty(component, "Title");
        var computed = GetProperty(component, "Computed");
        var field = GetField(component, "_countField");
        var currentMethod = GetMethodSymbol(fixture, "ComponentUnderTest", "CurrentMethod");
        var host = new CurrentComponentSemanticWalkerHost(
            component,
            parameterRuntimeNames: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["Title"] = "data-title"
            },
            memberRuntimeNames: new Dictionary<ISymbol, string>(SymbolEqualityComparer.Default)
            {
                [count.OriginalDefinition] = "countState",
                [computed.OriginalDefinition] = "readComputed",
                [field.OriginalDefinition] = "fieldState",
                [currentMethod.OriginalDefinition] = "currentMethod"
            });

        var reads = GetMethod(fixture, "ComponentUnderTest", "ReadMembers");
        var countReference = GetVariableInitializer(fixture, reads, "count");
        var titleReference = GetVariableInitializer(fixture, reads, "title");
        var computedReference = GetVariableInitializer(fixture, reads, "computed");
        var fieldReference = GetVariableInitializer(fixture, reads, "field");
        var countProjection = host.RewritePropertyReference(
            (IPropertyReferenceOperation)countReference,
            new SenseArgument(),
            new Identifier("this"),
            []);
        var titleProjection = host.RewritePropertyReference(
            (IPropertyReferenceOperation)titleReference,
            new SenseArgument(),
            new Identifier("this"),
            []);
        var computedProjection = host.RewritePropertyReference(
            (IPropertyReferenceOperation)computedReference,
            new SenseArgument(),
            new Identifier("this"),
            []);
        var fieldProjection = host.RewriteFieldReference(
            (IFieldReferenceOperation)fieldReference,
            new SenseArgument(),
            new Identifier("this"));

        Assert.IsInstanceOfType<MemberExpression>(countProjection);
        Assert.IsFalse(((MemberExpression)countProjection!).Computed);
        Assert.IsInstanceOfType<MemberExpression>(titleProjection);
        Assert.IsTrue(((MemberExpression)titleProjection!).Computed);
        Assert.IsInstanceOfType<CallExpression>(computedProjection);
        Assert.IsInstanceOfType<MemberExpression>(fieldProjection);

        var assignCount = GetAssignment(fixture, "AssignCount");
        var assignTitle = GetAssignment(fixture, "AssignTitle");
        var assignIndexer = GetAssignment(fixture, "AssignIndexer");
        Assert.IsNull(host.RewriteSimpleAssignmentPreorder(assignCount, new SenseArgument()));
        Assert.ThrowsExactly<OperationTransformationException>(() =>
            host.RewriteSimpleAssignmentPreorder(assignTitle, new SenseArgument()));
        Assert.IsInstanceOfType<AssignmentExpression>(host.RewriteSimpleAssignmentPostorder(
            assignCount,
            new SenseArgument(),
            new Identifier("value")));
        Assert.IsNull(host.RewriteSimpleAssignmentPostorder(
            assignIndexer,
            new SenseArgument(),
            new Identifier("value")));

        var indexer = GetVariableInitializer(fixture, GetMethod(fixture, "ComponentUnderTest", "ReadMembers"), "indexed");
        Assert.ThrowsExactly<OperationTransformationException>(() =>
            host.RewritePropertyReference(
                (IPropertyReferenceOperation)indexer,
                new SenseArgument(),
                new Identifier("this"),
                [new Identifier("index")]));

        var stateChanged = GetSingleInvocation(fixture, "ComponentUnderTest", "StateChangedCaller");
        var invokeAsync = GetSingleInvocation(fixture, "ComponentUnderTest", "InvokeAsyncCaller");
        var typeCheck = GetSingleInvocation(fixture, "ComponentUnderTest", "TypeCheckCall");
        var eventCallback = GetSingleInvocation(fixture, "ComponentUnderTest", "EventCallbackCaller");
        var currentMethodCall = GetSingleInvocation(fixture, "ComponentUnderTest", "CurrentMethodCaller");
        Assert.IsInstanceOfType<CallExpression>(host.RewriteInvocationPreorder(stateChanged, new SenseArgument()));
        Assert.IsInstanceOfType<CallExpression>(host.RewriteInvocation(
            stateChanged,
            new SenseArgument(),
            new Identifier("this"),
            []));
        Assert.IsInstanceOfType<CallExpression>(host.RewriteInvocation(
            invokeAsync,
            new SenseArgument(),
            new Identifier("this"),
            [new Identifier("work")]));
        Assert.IsInstanceOfType<Identifier>(
            host.RewriteInvocation(typeCheck, new SenseArgument(), null, [new Identifier("value")]));
        Assert.IsInstanceOfType<CallExpression>(host.RewriteInvocation(
            eventCallback,
            new SenseArgument(),
            new Identifier("changed"),
            [new Identifier("value")]));
        Assert.IsInstanceOfType<CallExpression>(host.RewriteInvocation(
            currentMethodCall,
            new SenseArgument(),
            new Identifier("this"),
            []));

        var methodReference = GetOperation<IMethodReferenceOperation>(
            fixture,
            GetMethod(fixture, "ComponentUnderTest", "MethodGroup")
                .DescendantNodes()
                .OfType<IdentifierNameSyntax>()
                .Single(identifier => identifier.Identifier.ValueText == "CurrentMethod"));
        Assert.IsInstanceOfType<Identifier>(host.RewriteMethodReference(
            methodReference,
            new SenseArgument(),
            new Identifier("this")));

        var parameterReference = GetVariableInitializer(
            fixture,
            GetMethod(fixture, "ComponentUnderTest", "LocalAndParameter"),
            "local");
        var localReference = GetVariableInitializer(
            fixture,
            GetMethod(fixture, "ComponentUnderTest", "LocalAndParameter"),
            "copy");
        var rewriteHost = new CurrentComponentSemanticWalkerHost(
            component,
            parameterReferenceRewriter: static (_, _) => new Identifier("parameter"),
            localReferenceRewriter: static (_, _) => new Identifier("local"));
        Assert.IsInstanceOfType<Identifier>(rewriteHost.RewriteParameterReference(
            (IParameterReferenceOperation)parameterReference,
            new SenseArgument()));
        Assert.IsInstanceOfType<Identifier>(rewriteHost.RewriteLocalReference(
            (ILocalReferenceOperation)localReference,
            new SenseArgument()));
    }

    [TestMethod]
    public void CallbackHelperEdges_KeepInferenceAndFailureContractsExplicit()
    {
        var fixture = CreateFixture();
        var component = GetNamedType(fixture, "ComponentUnderTest");
        var host = new CurrentComponentSemanticWalkerHost(component);
        var stateChanged = GetSingleInvocation(fixture, "ComponentUnderTest", "StateChangedCaller");
        var invokeAsync = GetSingleInvocation(fixture, "ComponentUnderTest", "InvokeAsyncCaller");
        var typeCheck = GetSingleInvocation(fixture, "ComponentUnderTest", "TypeCheckCall");
        var inferredEvent = GetSingleInvocation(fixture, "ComponentUnderTest", "InferredEventCallback");
        var directBinder = GetSingleInvocation(fixture, "ComponentUnderTest", "BinderFactory");
        var setterReference = GetOperation<IMethodReferenceOperation>(
            fixture,
            GetMethod(fixture, "ComponentUnderTest", "SetterMethodGroup")
                .DescendantNodes()
                .OfType<IdentifierNameSyntax>()
                .Single(identifier => identifier.Identifier.ValueText == "SetCount"));
        var foreignSetterReference = GetOperation<IMethodReferenceOperation>(
            fixture,
            GetMethod(fixture, "ComponentUnderTest", "ForeignSetterMethodGroup")
                .DescendantNodes()
                .OfType<MemberAccessExpressionSyntax>()
                .Single(access => access.Name.Identifier.ValueText == "Set"));

        var inferredArguments = new object?[] { inferredEvent, null };
        Assert.IsTrue(InvokeInstance<bool>(host, "TryGetInferredEventCallbackHandler", inferredArguments));
        Assert.IsNotNull(inferredArguments[1]);
        Assert.IsFalse(InvokeInstance<bool>(
            host,
            "TryGetInferredEventCallbackHandler",
            new object?[] { stateChanged, null }));
        Assert.IsInstanceOfType<ArrowFunctionExpression>(InvokeInstance<Expression?>(
            host,
            "RewriteEventCallbackHandler",
            inferredEvent,
            new SenseArgument()));

        Assert.IsInstanceOfType<ArrowFunctionExpression>(InvokeInstance<Expression?>(
            host,
            "RewriteInferredBindSetterMethodReference",
            setterReference,
            new SenseArgument()));
        Assert.IsNull(InvokeInstance<Expression?>(
            host,
            "RewriteInferredBindSetterMethodReference",
            foreignSetterReference,
            new SenseArgument()));

        var directBinderArguments = new object?[] { directBinder, null, null };
        Assert.IsTrue(InvokeStatic<bool>("TryGetCreateBinderReceiverAndHandler", directBinderArguments));
        Assert.IsInstanceOfType<IDelegateCreationOperation>(directBinderArguments[2]);
        var noHandlerDiagnostic = InvokeStatic<Exception>(
            "CreateUnsupportedEventCallbackFactoryCreateBinderException",
            stateChanged);
        StringAssert.Contains(noHandlerDiagnostic.Message, "Handler operation kind: <missing>", StringComparison.Ordinal);

        Assert.Throws<TargetInvocationException>(() => InvokeStatic<Expression>(
            "RewriteStateHasChanged",
            invokeAsync));
        Assert.Throws<TargetInvocationException>(() => InvokeStatic<Expression>(
            "RewriteInvokeAsync",
            invokeAsync,
            new Expression[] { new Identifier("first"), new Identifier("second") }));
        Assert.Throws<TargetInvocationException>(() => InvokeStatic<Expression>(
            "RewriteRazorRuntimeHelpersTypeCheck",
            typeCheck,
            Array.Empty<Expression>()));
        Assert.IsInstanceOfType<CallExpression>(InvokeStatic<Expression>(
            "RewriteRazorRuntimeHelpersInvokeAsynchronousDelegate",
            stateChanged,
            new Expression[] { new Identifier("callback") }));
        Assert.Throws<TargetInvocationException>(() => InvokeStatic<Expression>(
            "RewriteRazorRuntimeHelpersInvokeAsynchronousDelegate",
            stateChanged,
            Array.Empty<Expression>()));

        var nullReceiverDiagnostic = InvokeStatic<Exception>(
            "CreateUnsupportedIndirectCurrentComponentDispatchException",
            stateChanged,
            stateChanged.TargetMethod,
            null);
        var literalReceiverDiagnostic = InvokeStatic<Exception>(
            "CreateUnsupportedIndirectCurrentComponentDispatchException",
            stateChanged,
            stateChanged.TargetMethod,
            GetVariableInitializer(fixture, GetMethod(fixture, "ComponentUnderTest", "Operations"), "literal"));
        StringAssert.Contains(nullReceiverDiagnostic.Message, "<unknown>", StringComparison.Ordinal);
        Assert.AreNotEqual(nullReceiverDiagnostic.Message, literalReceiverDiagnostic.Message);
    }

    private static T InvokeStatic<T>(string methodName, params object?[] arguments)
    {
        var method = typeof(CurrentComponentSemanticWalkerHost)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
            .Single(candidate => candidate.Name == methodName && candidate.GetParameters().Length == arguments.Length);
        return (T)method.Invoke(null, arguments)!;
    }

    private static T InvokeInstance<T>(CurrentComponentSemanticWalkerHost host, string methodName, params object?[] arguments)
    {
        var method = typeof(CurrentComponentSemanticWalkerHost)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
            .Single(candidate => candidate.Name == methodName && candidate.GetParameters().Length == arguments.Length);
        return (T)method.Invoke(host, arguments)!;
    }

    private static Fixture CreateFixture()
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(
            """
            using System;
            using Microsoft.AspNetCore.Components;

            namespace CurrentComponentPrivateContracts;

            public sealed class ComponentUnderTest : ComponentBase
            {
                private EventCallbackFactory _factory;
                private int _countField;
                [Parameter] public EventCallback Changed { get; set; }
                [Parameter] public EventCallback<int> GenericChanged { get; set; }
                [Parameter] public string? Title { get; set; }
                public int Count { get; set; }
                public int Computed => Count;
                public static int StaticValue => 1;
                public string this[int index]
                {
                    get => Title ?? string.Empty;
                    set => Count = value.Length;
                }

                private static int Other() => 1;

                public void Operations()
                {
                    var property = Count;
                    object convertedProperty = (object)Count;
                    var staticValue = StaticValue;
                    var literal = 1;
                }

                public void ReadMembers()
                {
                    var count = Count;
                    var title = Title;
                    var computed = Computed;
                    var field = _countField;
                    var indexed = this[0];
                }

                public void AssignCount() { Count = 1; }
                public void AssignTitle() { Title = "title"; }
                public void AssignIndexer() { this[0] = "value"; }
                private void CurrentMethod() { }
                public void CurrentMethodCaller() { CurrentMethod(); }
                public void MethodGroup() { Action callback = CurrentMethod; }
                public void LocalAndParameter(int value)
                {
                    var local = value;
                    var copy = local;
                }

                private void SetCount(int value) { Count = value; }
                public void SetterMethodGroup() { Action<int> callback = SetCount; }
                public void ForeignSetterMethodGroup() { Action<int> callback = OtherHandlers.Set; }

                public void SingleBinder(int value)
                {
                    Count = value;
                    return;
                }

                public void MultipleBinder(int value)
                {
                    Count = value;
                    Count = value;
                }

                public int ReturnBinder(int value)
                {
                    return Count = value;
                }

                public void EmptyReturn() { return; }
                public void StateChangedCaller() { StateHasChanged(); }
                public void InvokeAsyncCaller() { _ = InvokeAsync(() => { }); }
                public void InvokeAsync(int value) { }
                public void StateHasChanged(int value) { }
                public static void StateHasChanged(string value) { }

                public void BinderFactory()
                {
                    var binder = EventCallback.Factory.CreateBinder(this, (int value) => Count = value, Count);
                }

                public void BinderFactoryWithField()
                {
                    var binder = _factory.CreateBinder(this, (int value) => Count = value, Count);
                }

                public void FactoryCreate()
                {
                    var callback = EventCallback.Factory.Create(this, (System.Action)(() => { }));
                }

                public void TypeCheckCall()
                {
                    var typeChecked = Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(Count);
                }

                public void InferredEventCallback()
                {
                    var callback = Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.CreateInferredEventCallback(
                        this,
                        (int value) => Count = value,
                        Count);
                }

                public void EventCallbackCaller()
                {
                    _ = Changed.InvokeAsync(Count);
                }
            }

            public sealed class GenericHolder<T>;

            public static class Other
            {
                public sealed class EventCallback;
            }

            public static class OtherHandlers
            {
                public static void Set(int value) { }
            }
            """,
            new CSharpParseOptions(LanguageVersion.Preview),
            path: "CurrentComponentPrivateContracts.cs");
        var compilation = CSharpCompilation.Create(
            "RazorVue.CurrentComponentPrivateContracts",
            [syntaxTree],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = RazorSgTestHost.GetCompilationErrors(compilation);
        Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors));
        return new Fixture(compilation, syntaxTree, compilation.GetSemanticModel(syntaxTree));
    }

    private static INamedTypeSymbol GetNamedType(Fixture fixture, string metadataName)
    {
        var type = fixture.Compilation.GetTypeByMetadataName("CurrentComponentPrivateContracts." + metadataName);
        Assert.IsNotNull(type, metadataName);
        return type!;
    }

    private static IPropertySymbol GetProperty(INamedTypeSymbol type, string name)
        => type.GetMembers(name).OfType<IPropertySymbol>().Single();

    private static IFieldSymbol GetField(INamedTypeSymbol type, string name)
        => type.GetMembers(name).OfType<IFieldSymbol>().Single();

    private static MethodDeclarationSyntax GetMethod(Fixture fixture, string typeName, string methodName)
        => fixture.SyntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Single(@class => @class.Identifier.ValueText == typeName)
            .Members
            .OfType<MethodDeclarationSyntax>()
            .Single(method => method.Identifier.ValueText == methodName);

    private static IMethodSymbol GetMethodSymbol(
        Fixture fixture,
        string typeName,
        string methodName,
        int? parameterCount = null,
        bool? isStatic = null)
    {
        var methods = GetNamedType(fixture, typeName).GetMembers(methodName).OfType<IMethodSymbol>();
        if (parameterCount is not null)
            methods = methods.Where(method => method.Parameters.Length == parameterCount.Value);
        if (isStatic is not null)
            methods = methods.Where(method => method.IsStatic == isStatic.Value);
        return methods.Single();
    }

    private static IBlockOperation GetMethodBody(Fixture fixture, string typeName, string methodName)
        => GetOperation<IBlockOperation>(fixture, GetMethod(fixture, typeName, methodName).Body!);

    private static IInvocationOperation GetSingleInvocation(Fixture fixture, string typeName, string methodName)
        => GetOperation<IInvocationOperation>(
            fixture,
            GetMethod(fixture, typeName, methodName)
                .DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Single());

    private static IReturnOperation GetReturnOperation(Fixture fixture, string typeName, string methodName)
        => GetOperation<IReturnOperation>(
            fixture,
            GetMethod(fixture, typeName, methodName)
                .DescendantNodes()
                .OfType<ReturnStatementSyntax>()
                .Single());

    private static ISimpleAssignmentOperation GetAssignment(Fixture fixture, string methodName)
        => GetOperation<ISimpleAssignmentOperation>(
            fixture,
            GetMethod(fixture, "ComponentUnderTest", methodName)
                .DescendantNodes()
                .OfType<AssignmentExpressionSyntax>()
                .Single());

    private static IOperation GetVariableInitializer(Fixture fixture, MethodDeclarationSyntax method, string name)
    {
        var initializer = method.DescendantNodes()
            .OfType<VariableDeclaratorSyntax>()
            .Single(declarator => declarator.Identifier.ValueText == name)
            .Initializer;
        Assert.IsNotNull(initializer, name);
        return GetOperation<IOperation>(fixture, initializer!.Value);
    }

    private static T GetOperation<T>(Fixture fixture, SyntaxNode syntax)
        where T : class, IOperation
    {
        var operation = fixture.SemanticModel.GetOperation(syntax) as T;
        Assert.IsNotNull(operation, typeof(T).Name + ": " + syntax);
        return operation!;
    }

    private sealed record Fixture(
        CSharpCompilation Compilation,
        SyntaxTree SyntaxTree,
        SemanticModel SemanticModel);
}
