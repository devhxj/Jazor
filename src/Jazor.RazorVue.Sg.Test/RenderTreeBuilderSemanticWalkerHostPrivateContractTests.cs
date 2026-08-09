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
public sealed class RenderTreeBuilderSemanticWalkerHostPrivateContractTests
{
    [TestMethod]
    public void RenderContextHelpers_ClassifySupportedBuilderAndMarkupShapes()
    {
        var fixture = CreateFixture();
        var host = new RenderTreeBuilderSemanticWalkerHost();
        var staticMarkup = GetVariableInitializer(fixture, "StaticMarkup", "staticMarkup");
        var convertedMarkup = GetVariableInitializer(fixture, "StaticMarkup", "convertedMarkup");
        var dynamicMarkup = GetVariableInitializer(fixture, "StaticMarkup", "dynamicMarkup");
        var builderCreation = GetVariableInitializer(fixture, "StaticMarkup", "nestedBuilder");
        var unrelatedCreation = GetVariableInitializer(fixture, "StaticMarkup", "unrelated");
        var openElement = GetInvocation(fixture, "SupportedCalls", static invocation => invocation.TargetMethod.Name == "OpenElement");
        var openGeneric = GetInvocation(
            fixture,
            "SupportedCalls",
            static invocation => invocation.TargetMethod.Name == "OpenComponent" && invocation.TargetMethod.IsGenericMethod);
        var openType = GetInvocation(
            fixture,
            "SupportedCalls",
            static invocation => invocation.TargetMethod.Name == "OpenComponent" && !invocation.TargetMethod.IsGenericMethod);
        var renderFragment = GetInvocation(
            fixture,
            "SupportedCalls",
            static invocation => invocation.TargetMethod.Name == "AddContent" && invocation.Arguments.Length == 2 &&
                                 invocation.TargetMethod.Parameters[1].Type.Name == "RenderFragment");
        var genericRenderFragment = GetInvocation(
            fixture,
            "SupportedCalls",
            static invocation => invocation.TargetMethod.Name == "AddContent" && invocation.Arguments.Length == 3);
        var componentSlot = GetInvocation(
            fixture,
            "SupportedCalls",
            static invocation => invocation.TargetMethod.Name == "AddComponentParameter" &&
                                 invocation.Arguments[1].Value.ConstantValue.Value as string == "Slot");
        var componentScopedSlot = GetInvocation(
            fixture,
            "SupportedCalls",
            static invocation => invocation.TargetMethod.Name == "AddComponentParameter" &&
                                 invocation.Arguments[1].Value.ConstantValue.Value as string == "ScopedSlot");
        var eventModifier = GetInvocation(
            fixture,
            "EventModifierCalls",
            static invocation => invocation.TargetMethod.Name == "AddEventPreventDefaultAttribute");
        var unsupported = GetInvocation(fixture, "UnrelatedCall", static invocation => invocation.TargetMethod.Name == "Helper");

        AssertStaticMarkup(staticMarkup, "<strong>static</strong>");
        AssertStaticMarkup(convertedMarkup, "<em>converted</em>");
        var dynamicMarkupArguments = new object?[] { dynamicMarkup, null };
        Assert.IsFalse(InvokeStatic<bool>("TryGetStaticMarkupString", dynamicMarkupArguments));
        Assert.IsNull(dynamicMarkupArguments[1]);

        Assert.IsInstanceOfType<StringLiteral>(host.RewriteObjectCreationPreorder(
            (IObjectCreationOperation)staticMarkup,
            new SenseArgument()));
        Assert.IsNull(host.RewriteObjectCreationPreorder((IObjectCreationOperation)dynamicMarkup, new SenseArgument()));
        Assert.IsTrue(host.ShouldRewriteObjectCreation((IObjectCreationOperation)staticMarkup));
        Assert.IsTrue(host.ShouldRewriteObjectCreation((IObjectCreationOperation)builderCreation));
        Assert.IsFalse(host.ShouldRewriteObjectCreation((IObjectCreationOperation)unrelatedCreation));

        var rewrittenMarkup = host.RewriteObjectCreation(
            (IObjectCreationOperation)dynamicMarkup,
            new SenseArgument(),
            [new Identifier("markup")]);
        Assert.IsInstanceOfType<Identifier>(rewrittenMarkup);
        Assert.IsInstanceOfType<CallExpression>(host.RewriteObjectCreation(
            (IObjectCreationOperation)builderCreation,
            new SenseArgument(UseImportAliases: true),
            []));
        Assert.IsNull(host.RewriteObjectCreation(
            (IObjectCreationOperation)unrelatedCreation,
            new SenseArgument(),
            []));

        Assert.AreEqual("OpenElement", InvokeStatic<object>("ClassifyRenderContextMethod", openElement.TargetMethod).ToString());
        Assert.AreEqual("OpenGenericComponent", InvokeStatic<object>("ClassifyRenderContextMethod", openGeneric.TargetMethod).ToString());
        Assert.AreEqual("OpenTypeComponent", InvokeStatic<object>("ClassifyRenderContextMethod", openType.TargetMethod).ToString());
        Assert.AreEqual("Unsupported", InvokeStatic<object>("ClassifyRenderContextMethod", unsupported.TargetMethod).ToString());
        Assert.IsTrue(InvokeStatic<bool>("IsSupportedRenderContextMethod", openGeneric));
        Assert.IsTrue(InvokeStatic<bool>("IsSupportedRenderContextMethod", openType));
        Assert.IsFalse(InvokeStatic<bool>("IsSupportedRenderContextMethod", unsupported));
        Assert.IsTrue(InvokeStatic<bool>("IsRenderTreeBuilderMethod", openElement.TargetMethod));
        Assert.IsFalse(InvokeStatic<bool>("IsRenderTreeBuilderMethod", unsupported.TargetMethod));
        Assert.IsTrue(InvokeStatic<bool>("IsOpenComponentTypeMethod", openType.TargetMethod));
        Assert.IsFalse(InvokeStatic<bool>("IsOpenComponentTypeMethod", openGeneric.TargetMethod));
        Assert.IsTrue(InvokeStatic<bool>("IsRenderTreeBuilderEventModifierMethod", eventModifier.TargetMethod));
        Assert.IsFalse(InvokeStatic<bool>("IsRenderTreeBuilderEventModifierMethod", openElement.TargetMethod));
        Assert.IsTrue(InvokeStatic<bool>("IsRenderFragmentComponentParameterValue", componentSlot));
        Assert.IsFalse(InvokeStatic<bool>("IsGenericRenderFragmentComponentParameterValue", componentSlot));
        Assert.IsTrue(InvokeStatic<bool>("IsGenericRenderFragmentComponentParameterValue", componentScopedSlot));
        Assert.IsFalse(InvokeStatic<bool>("IsRenderFragmentComponentParameterValue", componentScopedSlot));
        Assert.IsTrue(InvokeStatic<bool>("IsRenderFragmentOperationValue", renderFragment.Arguments[1].Value));
        Assert.IsTrue(InvokeStatic<bool>("IsGenericRenderFragmentOperationValue", genericRenderFragment.Arguments[1].Value));
        Assert.IsFalse(InvokeStatic<bool>("IsGenericRenderFragmentOperationValue", renderFragment.Arguments[1].Value));
    }

    [TestMethod]
    public void OpenComponentHelpers_ResolveTypeImportsAndLocalLifetimeRules()
    {
        var fixture = CreateFixture();
        var host = new RenderTreeBuilderSemanticWalkerHost();
        var moduleChild = GetNamedType(fixture, "ModuleChild");
        var libraryChild = GetNamedType(fixture, "LibraryChild");
        var invalidLibraryChild = GetNamedType(fixture, "InvalidLibraryChild");
        var plainChild = GetNamedType(fixture, "PlainChild");
        var directType = GetInvocation(fixture, "DirectTypeComponent", static invocation => invocation.TargetMethod.Name == "OpenComponent");
        var localType = GetInvocation(fixture, "LocalTypeComponent", static invocation => invocation.TargetMethod.Name == "OpenComponent");
        var reassignedType = GetInvocation(fixture, "ReassignedTypeComponent", static invocation => invocation.TargetMethod.Name == "OpenComponent");
        var escapedType = GetInvocation(fixture, "EscapedTypeComponent", static invocation => invocation.TargetMethod.Name == "OpenComponent");
        var dynamicType = GetInvocation(fixture, "DynamicTypeComponent", static invocation => invocation.TargetMethod.Name == "OpenComponent");
        var localDeclarator = GetVariableDeclarator(fixture, "LocalTypeComponent", "componentType");
        var reassignedDeclarator = GetVariableDeclarator(fixture, "ReassignedTypeComponent", "componentType");
        var escapedDeclarator = GetVariableDeclarator(fixture, "EscapedTypeComponent", "componentType");

        AssertResolvedComponentType(directType, "ModuleChild");
        AssertResolvedComponentType(localType, "ModuleChild");
        AssertUnresolvedComponentType(reassignedType);
        AssertResolvedComponentType(escapedType, "ModuleChild");
        AssertUnresolvedComponentType(dynamicType);

        var localReference = UnwrapConversion(localType.Arguments[1].Value) as ILocalReferenceOperation;
        Assert.IsNotNull(localReference);
        Assert.IsTrue(InvokeStatic<bool>("IsOpenComponentTypeArgumentReference", localReference));
        Assert.IsTrue(InvokeStatic<bool>("IsLocalReference", localReference, localDeclarator.Symbol));
        Assert.IsFalse(InvokeStatic<bool>("IsLocalReference", directType.Arguments[1].Value, localDeclarator.Symbol));
        Assert.IsTrue(InvokeStatic<bool>("AllLocalReferencesAreOpenComponentTypeArguments", localType, localDeclarator.Symbol));
        Assert.IsFalse(InvokeStatic<bool>("AllLocalReferencesAreOpenComponentTypeArguments", escapedType, escapedDeclarator.Symbol));
        Assert.IsFalse(InvokeStatic<bool>("ContainsLocalAssignment", GetMethodBody(fixture, "LocalTypeComponent"), localDeclarator.Symbol));
        Assert.IsTrue(InvokeStatic<bool>("ContainsLocalAssignment", GetMethodBody(fixture, "ReassignedTypeComponent"), reassignedDeclarator.Symbol));
        Assert.IsTrue(host.ShouldSkipVariableDeclarator(localDeclarator, new SenseArgument()));
        Assert.IsFalse(host.ShouldSkipVariableDeclarator(reassignedDeclarator, new SenseArgument()));
        Assert.IsFalse(host.ShouldSkipVariableDeclarator(escapedDeclarator, new SenseArgument()));

        Assert.AreEqual("./components/module-child", InvokeStatic<string?>("GetECMAScriptModuleExportPath", moduleChild));
        Assert.IsNull(InvokeStatic<string?>("GetECMAScriptModuleExportPath", libraryChild));
        Assert.AreEqual("./components/module-child.mjs", InvokeStatic<string>("NormalizeModuleImportPath", "./components/module-child"));
        AssertComponentImport(moduleChild, "./components/module-child.mjs", "default");
        AssertComponentImport(libraryChild, "tdesign-vue-next", "TButton");
        AssertNoComponentImport(invalidLibraryChild);
        AssertNoComponentImport(plainChild);
        Assert.IsNotNull(InvokeStatic<ObjectExpression?>("BuildComponentParameterNameMapExpression", moduleChild));
        Assert.IsNull(InvokeStatic<ObjectExpression?>("BuildComponentParameterNameMapExpression", plainChild));

        var generic = GetInvocation(
            fixture,
            "GenericComponent",
            static invocation => invocation.TargetMethod.Name == "OpenComponent" && invocation.TargetMethod.IsGenericMethod);
        var genericImportArguments = new object?[] { generic.TargetMethod, null, null };
        Assert.IsTrue(InvokeStatic<bool>("TryResolveComponentImport", genericImportArguments));
        AssertComponentImportDescriptor(genericImportArguments[1], "./components/module-child.mjs", "default");
        Assert.AreEqual("ModuleChild", ((INamedTypeSymbol)genericImportArguments[2]!).Name);
    }

    [TestMethod]
    public void InvocationLowering_PreservesSingleEvaluationForIgnoredBuilderArguments()
    {
        var fixture = CreateFixture();
        var host = new RenderTreeBuilderSemanticWalkerHost();
        var directFragment = GetInvocation(fixture, "DirectFragment", static invocation => invocation.TargetMethod.Name == "AddContent");
        var effectfulFragment = GetInvocation(fixture, "EffectfulFragment", static invocation => invocation.TargetMethod.Name == "AddContent");
        var directGenericFragment = GetInvocation(fixture, "DirectGenericFragment", static invocation => invocation.TargetMethod.Name == "AddContent");
        var effectfulGenericFragment = GetInvocation(fixture, "EffectfulGenericFragment", static invocation => invocation.TargetMethod.Name == "AddContent");
        var directModifier = GetInvocation(fixture, "DirectModifier", static invocation => invocation.TargetMethod.Name == "AddEventPreventDefaultAttribute");
        var effectfulModifier = GetInvocation(fixture, "EffectfulModifier", static invocation => invocation.TargetMethod.Name == "AddEventStopPropagationAttribute");
        var openElement = GetInvocation(fixture, "SupportedCalls", static invocation => invocation.TargetMethod.Name == "OpenElement");
        var unsupportedComponent = GetInvocation(
            fixture,
            "UnsupportedComponent",
            static invocation => invocation.TargetMethod.Name == "OpenComponent");

        AssertDirectCall(host.RewriteInvocation(
            directFragment,
            new SenseArgument(),
            new Identifier("ctx"),
            [new Identifier("seq"), new Identifier("fragment")]));
        AssertSingleEvaluationCall(host.RewriteInvocation(
            effectfulFragment,
            new SenseArgument(),
            new Identifier("ctx"),
            [new Identifier("seq"), new Identifier("fragment")]));
        AssertDirectCall(host.RewriteInvocation(
            directGenericFragment,
            new SenseArgument(),
            new Identifier("ctx"),
            [new Identifier("seq"), new Identifier("fragment"), new Identifier("value")]));
        AssertSingleEvaluationCall(host.RewriteInvocation(
            effectfulGenericFragment,
            new SenseArgument(),
            new Identifier("ctx"),
            [new Identifier("seq"), new Identifier("fragment"), new Identifier("value")]));
        AssertDirectCall(host.RewriteInvocation(
            directModifier,
            new SenseArgument(),
            null,
            [new Identifier("ctx"), new Identifier("seq"), new Identifier("name"), new Identifier("enabled")]));
        AssertSingleEvaluationCall(host.RewriteInvocation(
            effectfulModifier,
            new SenseArgument(),
            null,
            [new Identifier("ctx"), new Identifier("seq"), new Identifier("name"), new Identifier("enabled")]));

        Assert.IsInstanceOfType<NullLiteral>(host.RewriteInvocationArgumentPreorder(
            GetInvocation(fixture, "DirectTypeComponent", static invocation => invocation.TargetMethod.Name == "OpenComponent"),
            GetInvocation(fixture, "DirectTypeComponent", static invocation => invocation.TargetMethod.Name == "OpenComponent").Arguments[1],
            1,
            new SenseArgument()));
        Assert.IsNull(host.RewriteInvocationArgumentPreorder(
            openElement,
            openElement.Arguments[0],
            0,
            new SenseArgument()));
        Assert.ThrowsExactly<OperationTransformationException>(() =>
            host.RewriteInvocationPreorder(unsupportedComponent, new SenseArgument()));
        Assert.ThrowsExactly<OperationTransformationException>(() =>
            host.RewriteInvocation(
                openElement,
                new SenseArgument(),
                null,
                [new Identifier("seq"), new Identifier("tag")]));
    }

    [TestMethod]
    public void ComponentImportAndInvocationEdges_CoverUnmappedAndEffectfulBuilderShapes()
    {
        var fixture = CreateFixture();
        var host = new RenderTreeBuilderSemanticWalkerHost();
        var noArgumentModuleChild = GetNamedType(fixture, "NoArgumentModuleChild");
        var noisyModuleChild = GetNamedType(fixture, "NoisyModuleChild");
        var plainChild = GetNamedType(fixture, "PlainChild");
        var genericPlain = GetInvocation(
            fixture,
            "GenericPlainComponent",
            static invocation => invocation.TargetMethod.Name == "OpenComponent");
        var directType = GetInvocation(
            fixture,
            "DirectTypeComponent",
            static invocation => invocation.TargetMethod.Name == "OpenComponent");
        var noPropsType = GetInvocation(
            fixture,
            "NoPropsTypeComponent",
            static invocation => invocation.TargetMethod.Name == "OpenComponent");
        var effectfulType = GetInvocation(
            fixture,
            "EffectfulTypeComponent",
            static invocation => invocation.TargetMethod.Name == "OpenComponent");
        var eventModifier = GetInvocation(
            fixture,
            "EventModifierCalls",
            static invocation => invocation.TargetMethod.Name == "AddEventPreventDefaultAttribute");
        var compoundLocal = GetVariableDeclarator(fixture, "CompoundLocal", "counter");

        Assert.IsNull(InvokeStatic<string?>("GetECMAScriptModuleExportPath", noArgumentModuleChild));
        Assert.AreEqual("./components/noisy", InvokeStatic<string?>("GetECMAScriptModuleExportPath", noisyModuleChild));
        AssertNoComponentImport(noArgumentModuleChild);
        AssertNoComponentImport(plainChild);

        var genericPlainImport = new object?[] { genericPlain.TargetMethod, null, null };
        Assert.IsFalse(InvokeStatic<bool>("TryResolveComponentImport", genericPlainImport));
        Assert.AreEqual("PlainChild", ((INamedTypeSymbol)genericPlainImport[2]!).Name);

        AssertDirectCall(host.RewriteInvocation(
            directType,
            new SenseArgument(),
            new Identifier("ctx"),
            [new Identifier("sequence"), new Identifier("componentType")]));
        AssertDirectCall(host.RewriteInvocation(
            noPropsType,
            new SenseArgument(),
            new Identifier("ctx"),
            [new Identifier("sequence"), new Identifier("componentType")]));
        AssertSingleEvaluationCall(host.RewriteInvocation(
            effectfulType,
            new SenseArgument(),
            new Identifier("ctx"),
            [new Identifier("sequence"), new Identifier("componentType")]));

        AssertSingleEvaluationCall(InvokeStatic<Expression>(
            "BuildEventModifierCall",
            eventModifier,
            new Identifier("ctx"),
            new Expression[]
            {
                new Identifier("builder"),
                new Identifier("sequence"),
                new Identifier("name"),
                new Identifier("enabled")
            }));
        Assert.IsTrue(InvokeStatic<bool>(
            "ContainsLocalAssignment",
            GetMethodBody(fixture, "CompoundLocal"),
            compoundLocal.Symbol));
    }

    [TestMethod]
    public void RenderContextFailureEdges_RejectNonFrameworkFragmentsAndUnresolvableComponents()
    {
        var fixture = CreateFixture();
        var host = new RenderTreeBuilderSemanticWalkerHost();
        var customFragment = GetNamedType(fixture, "Other+RenderFragment");
        var customGenericFragment = GetNamedType(fixture, "Other+RenderFragment`1");
        var invalidLibraryExport = GetNamedType(fixture, "InvalidLibraryExportChild");
        var dynamicType = GetInvocation(
            fixture,
            "DynamicTypeComponent",
            static invocation => invocation.TargetMethod.Name == "OpenComponent");
        var unsupportedComponent = GetInvocation(
            fixture,
            "UnsupportedComponent",
            static invocation => invocation.TargetMethod.Name == "OpenComponent");
        var openElement = GetInvocation(
            fixture,
            "SupportedCalls",
            static invocation => invocation.TargetMethod.Name == "OpenElement");
        var eventModifier = GetInvocation(
            fixture,
            "EventModifierCalls",
            static invocation => invocation.TargetMethod.Name == "AddEventPreventDefaultAttribute");
        var unrelatedCreation = GetVariableInitializer(fixture, "StaticMarkup", "unrelated");

        Assert.IsFalse(InvokeStatic<bool>("IsRenderFragment", customFragment));
        Assert.IsFalse(InvokeStatic<bool>("IsGenericRenderFragment", customGenericFragment));
        AssertNoComponentImport(invalidLibraryExport);
        Assert.IsFalse(InvokeStatic<bool>("IsRenderFragmentComponentParameterValue", openElement));
        Assert.IsFalse(InvokeStatic<bool>("IsGenericRenderFragmentComponentParameterValue", openElement));
        Assert.IsFalse(InvokeStatic<bool>("IsRenderFragmentOperationValue", openElement.Arguments[1].Value));
        Assert.IsFalse(InvokeStatic<bool>("IsGenericRenderFragmentOperationValue", openElement.Arguments[1].Value));
        var unrelatedMarkup = new object?[] { unrelatedCreation, null };
        Assert.IsFalse(InvokeStatic<bool>("TryGetStaticMarkupString", unrelatedMarkup));

        Assert.ThrowsExactly<OperationTransformationException>(() =>
            host.RewriteInvocation(
                dynamicType,
                new SenseArgument(),
                new Identifier("ctx"),
                [new Identifier("sequence"), new Identifier("componentType")]));
        Assert.ThrowsExactly<OperationTransformationException>(() =>
            host.RewriteInvocation(
                unsupportedComponent,
                new SenseArgument(),
                new Identifier("ctx"),
                [new Identifier("sequence")]));
        Assert.Throws<TargetInvocationException>(() => InvokeStatic<Expression>(
            "BuildEventModifierCall",
            eventModifier,
            null,
            new Expression[] { new Identifier("builder"), new Identifier("sequence"), new Identifier("name") }));
    }

    [TestMethod]
    public void RenderContextPrivateEdges_HandleConvertedFragmentsAndIncompleteTypeShapes()
    {
        var fixture = CreateFixture();
        var host = new RenderTreeBuilderSemanticWalkerHost();
        var convertedFragment = GetInvocation(
            fixture,
            "ConvertedFragments",
            static invocation => invocation.TargetMethod.Name == "AddContent" && invocation.Arguments.Length == 2);
        var convertedGenericFragment = GetInvocation(
            fixture,
            "ConvertedFragments",
            static invocation => invocation.TargetMethod.Name == "AddContent" && invocation.Arguments.Length == 3);
        var genericTypeParameter = GetInvocation(
            fixture,
            "GenericTypeParameterComponent",
            static invocation => invocation.TargetMethod.Name == "OpenComponent");
        var clear = GetInvocation(
            fixture,
            "NoOpCalls",
            static invocation => invocation.TargetMethod.Name == "Clear");
        var dynamicLocalType = GetInvocation(
            fixture,
            "DynamicLocalTypeComponent",
            static invocation => invocation.TargetMethod.Name == "OpenComponent");
        var dynamicLocal = GetVariableDeclarator(fixture, "DynamicLocalTypeComponent", "componentType");
        var uninitializedLocal = GetVariableDeclarator(fixture, "UninitializedTypeLocal", "componentType");
        var whitespaceModuleChild = GetNamedType(fixture, "WhitespaceModuleChild");

        Assert.IsInstanceOfType<IConversionOperation>(convertedFragment.Arguments[1].Value);
        Assert.IsInstanceOfType<IConversionOperation>(convertedGenericFragment.Arguments[1].Value);
        Assert.IsTrue(InvokeStatic<bool>("IsRenderFragmentOperationValue", convertedFragment.Arguments[1].Value));
        Assert.IsTrue(InvokeStatic<bool>("IsGenericRenderFragmentOperationValue", convertedGenericFragment.Arguments[1].Value));
        AssertUnresolvedComponentType(clear);
        AssertUnresolvedComponentType(genericTypeParameter);
        AssertUnresolvedComponentType(dynamicLocalType);

        var genericTypeParameterImport = new object?[] { genericTypeParameter.TargetMethod, null, null };
        Assert.IsFalse(InvokeStatic<bool>("TryResolveComponentImport", genericTypeParameterImport));
        Assert.IsNull(genericTypeParameterImport[2]);

        var nonGenericImport = new object?[] { clear.TargetMethod, null, null };
        Assert.IsFalse(InvokeStatic<bool>("TryResolveComponentImport", nonGenericImport));
        Assert.IsNull(nonGenericImport[2]);

        var uninitializedInitializer = new object?[]
        {
            GetMethodBody(fixture, "UninitializedTypeLocal"),
            uninitializedLocal.Symbol,
            null
        };
        Assert.IsFalse(InvokeStatic<bool>("TryFindLocalInitializer", uninitializedInitializer));
        Assert.IsNull(uninitializedInitializer[2]);
        Assert.IsFalse(host.ShouldSkipVariableDeclarator(uninitializedLocal, new SenseArgument()));
        Assert.IsFalse(host.ShouldSkipVariableDeclarator(dynamicLocal, new SenseArgument()));

        Assert.IsNull(InvokeStatic<string?>("GetECMAScriptModuleExportPath", whitespaceModuleChild));
        AssertNoComponentImport(whitespaceModuleChild);
    }

    [TestMethod]
    public void ContextCallArgument_ResolvesSourceAndSyntheticArgumentsAcrossBothOverloads()
    {
        var contextCallArgumentType = typeof(RenderTreeBuilderSemanticWalkerHost)
            .GetNestedType("ContextCallArgument", BindingFlags.NonPublic);
        Assert.IsNotNull(contextCallArgumentType);

        var fromSource = contextCallArgumentType.GetMethod(
            "FromSource",
            BindingFlags.Public | BindingFlags.Static);
        var fromExpression = contextCallArgumentType.GetMethod(
            "FromExpression",
            BindingFlags.Public | BindingFlags.Static);
        Assert.IsNotNull(fromSource);
        Assert.IsNotNull(fromExpression);

        var resolveExpressions = contextCallArgumentType
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Single(method =>
                method.Name == "Resolve" &&
                method.GetParameters()[0].ParameterType.GetGenericArguments()[0] == typeof(Expression));
        var resolveIdentifiers = contextCallArgumentType
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Single(method =>
                method.Name == "Resolve" &&
                method.GetParameters()[0].ParameterType.GetGenericArguments()[0] == typeof(Identifier));

        var sourceArgument = fromSource.Invoke(null, [1])!;
        var syntheticExpression = new Identifier("synthetic");
        var syntheticArgument = fromExpression.Invoke(null, [syntheticExpression])!;
        var translatedArguments = new Expression[] { new Identifier("first"), new Identifier("second") };
        var argumentParameters = new Identifier[] { new Identifier("first"), new Identifier("second") };

        Assert.AreSame(translatedArguments[1], resolveExpressions.Invoke(sourceArgument, [translatedArguments]));
        Assert.AreSame(syntheticExpression, resolveExpressions.Invoke(syntheticArgument, [translatedArguments]));
        Assert.AreSame(argumentParameters[1], resolveIdentifiers.Invoke(sourceArgument, [argumentParameters]));
        Assert.AreSame(syntheticExpression, resolveIdentifiers.Invoke(syntheticArgument, [argumentParameters]));

        var constructor = contextCallArgumentType.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic).Single();
        var missingSyntheticArgument = constructor.Invoke([-1, null]);
        var expressionFailure = Assert.Throws<TargetInvocationException>(() =>
            resolveExpressions.Invoke(missingSyntheticArgument, [translatedArguments]));
        var identifierFailure = Assert.Throws<TargetInvocationException>(() =>
            resolveIdentifiers.Invoke(missingSyntheticArgument, [argumentParameters]));
        Assert.IsInstanceOfType<InvalidOperationException>(expressionFailure.InnerException);
        Assert.IsInstanceOfType<InvalidOperationException>(identifierFailure.InnerException);
    }

    private static void AssertStaticMarkup(IOperation operation, string expected)
    {
        var arguments = new object?[] { operation, null };
        Assert.IsTrue(InvokeStatic<bool>("TryGetStaticMarkupString", arguments));
        Assert.AreEqual(expected, arguments[1]);
    }

    private static void AssertResolvedComponentType(IInvocationOperation operation, string expectedName)
    {
        var arguments = new object?[] { operation, null };
        Assert.IsTrue(InvokeStatic<bool>("TryResolveComponentTypeArgument", arguments));
        Assert.AreEqual(expectedName, ((INamedTypeSymbol)arguments[1]!).Name);
    }

    private static void AssertUnresolvedComponentType(IInvocationOperation operation)
    {
        var arguments = new object?[] { operation, null };
        Assert.IsFalse(InvokeStatic<bool>("TryResolveComponentTypeArgument", arguments));
        Assert.IsNull(arguments[1]);
    }

    private static void AssertComponentImport(INamedTypeSymbol componentType, string importSpecifier, string exportName)
    {
        var arguments = new object?[] { componentType, null };
        Assert.IsTrue(InvokeStatic<bool>("TryResolveComponentImport", arguments));
        AssertComponentImportDescriptor(arguments[1], importSpecifier, exportName);
    }

    private static void AssertNoComponentImport(INamedTypeSymbol componentType)
    {
        var arguments = new object?[] { componentType, null };
        Assert.IsFalse(InvokeStatic<bool>("TryResolveComponentImport", arguments));
    }

    private static void AssertComponentImportDescriptor(object? descriptor, string importSpecifier, string exportName)
    {
        Assert.IsNotNull(descriptor);
        var type = descriptor.GetType();
        Assert.AreEqual(importSpecifier, type.GetProperty("ImportSpecifier")!.GetValue(descriptor));
        Assert.AreEqual(exportName, type.GetProperty("ExportName")!.GetValue(descriptor));
    }

    private static void AssertDirectCall(Expression? expression)
    {
        Assert.IsInstanceOfType<CallExpression>(expression);
        Assert.IsNotInstanceOfType<ArrowFunctionExpression>(((CallExpression)expression!).Callee);
    }

    private static void AssertSingleEvaluationCall(Expression? expression)
    {
        Assert.IsInstanceOfType<CallExpression>(expression);
        Assert.IsInstanceOfType<ArrowFunctionExpression>(((CallExpression)expression!).Callee);
    }

    private static T InvokeStatic<T>(string methodName, params object?[] arguments)
    {
        var method = typeof(RenderTreeBuilderSemanticWalkerHost)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
            .Single(candidate => candidate.Name == methodName && candidate.GetParameters().Length == arguments.Length);
        return (T)method.Invoke(null, arguments)!;
    }

    private static Fixture CreateFixture()
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(
            """
            using System;
            using ECMAScript;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;
            using Microsoft.AspNetCore.Components.Web;

            namespace RenderTreeBuilderPrivateContracts;

            [ECMAScriptModule("./components/module-child")]
            public sealed class ModuleChild : ComponentBase
            {
                [Parameter, ECMAScriptName("data-title")]
                public string? Title { get; set; }
            }

            [ECMAScriptModule]
            public sealed class NoArgumentModuleChild : ComponentBase;

            [Obsolete, ECMAScriptModule("./components/noisy")]
            public sealed class NoisyModuleChild : ComponentBase;

            [ECMAScriptModule("./components/no-props")]
            public sealed class NoPropsModuleChild : ComponentBase;

            [ECMAScriptModule(" ")]
            public sealed class WhitespaceModuleChild : ComponentBase;

            [VueLibraryComponent("tdesign-vue-next", "TButton")]
            public sealed class LibraryChild : ComponentBase;

            [VueLibraryComponent("", "TBad")]
            public sealed class InvalidLibraryChild : ComponentBase;

            [VueLibraryComponent("tdesign-vue-next", " ")]
            public sealed class InvalidLibraryExportChild : ComponentBase;

            public sealed class PlainChild : ComponentBase;

            public static class Other
            {
                public delegate void RenderFragment(RenderTreeBuilder builder);
                public delegate void RenderFragment<TValue>(TValue value);
            }

            public sealed class Host
            {
                private static int NextSequence() => 1;
                private static Type DynamicType() => typeof(ModuleChild);
                private static void Helper() { }
                private static void Escape(Type value) { }

                public void StaticMarkup()
                {
                    var staticMarkup = new MarkupString("<strong>static</strong>");
                    object convertedMarkup = new MarkupString("<em>converted</em>");
                    var dynamicMarkup = new MarkupString(DateTime.UtcNow.ToString());
                    var nestedBuilder = new RenderTreeBuilder();
                    var unrelated = new DateTime();
                }

                public void SupportedCalls(RenderTreeBuilder builder, RenderFragment fragment, RenderFragment<int> genericFragment)
                {
                    builder.OpenElement(0, "div");
                    builder.OpenComponent<ModuleChild>(1);
                    builder.OpenComponent(2, typeof(ModuleChild));
                    builder.AddContent(3, fragment);
                    builder.AddContent(4, genericFragment, 5);
                    builder.AddComponentParameter(5, "Slot", fragment);
                    builder.AddComponentParameter(6, "ScopedSlot", genericFragment);
                }

                public void GenericComponent(RenderTreeBuilder builder)
                    => builder.OpenComponent<ModuleChild>(0);

                public void GenericPlainComponent(RenderTreeBuilder builder)
                    => builder.OpenComponent<PlainChild>(0);

                public void GenericTypeParameterComponent<TComponent>(RenderTreeBuilder builder)
                    where TComponent : IComponent
                    => builder.OpenComponent<TComponent>(0);

                public void DirectTypeComponent(RenderTreeBuilder builder)
                    => builder.OpenComponent(0, typeof(ModuleChild));

                public void NoPropsTypeComponent(RenderTreeBuilder builder)
                    => builder.OpenComponent(0, typeof(NoPropsModuleChild));

                public void EffectfulTypeComponent(RenderTreeBuilder builder)
                    => builder.OpenComponent(NextSequence(), typeof(ModuleChild));

                public void LocalTypeComponent(RenderTreeBuilder builder)
                {
                    Type componentType = typeof(ModuleChild);
                    builder.OpenComponent(0, componentType);
                }

                public void ReassignedTypeComponent(RenderTreeBuilder builder)
                {
                    Type componentType = typeof(ModuleChild);
                    componentType = typeof(PlainChild);
                    builder.OpenComponent(0, componentType);
                }

                public void EscapedTypeComponent(RenderTreeBuilder builder)
                {
                    Type componentType = typeof(ModuleChild);
                    Escape(componentType);
                    builder.OpenComponent(0, componentType);
                }

                public void DynamicTypeComponent(RenderTreeBuilder builder)
                    => builder.OpenComponent(0, DynamicType());

                public void DynamicLocalTypeComponent(RenderTreeBuilder builder)
                {
                    Type componentType = DynamicType();
                    builder.OpenComponent(0, componentType);
                }

                public void UninitializedTypeLocal()
                {
                    Type componentType;
                }

                public void ConvertedFragments(RenderTreeBuilder builder, RenderFragment fragment, RenderFragment<int> genericFragment)
                {
                    builder.AddContent(0, (RenderFragment)fragment);
                    builder.AddContent(1, (RenderFragment<int>)genericFragment, 2);
                }

                public void NoOpCalls(RenderTreeBuilder builder)
                    => builder.Clear();

                public void EventModifierCalls(RenderTreeBuilder builder)
                    => builder.AddEventPreventDefaultAttribute(0, "click", true);

                public void DirectFragment(RenderTreeBuilder builder, RenderFragment fragment)
                    => builder.AddContent(0, fragment);

                public void EffectfulFragment(RenderTreeBuilder builder, RenderFragment fragment)
                    => builder.AddContent(NextSequence(), fragment);

                public void DirectGenericFragment(RenderTreeBuilder builder, RenderFragment<int> fragment)
                    => builder.AddContent(0, fragment, 1);

                public void EffectfulGenericFragment(RenderTreeBuilder builder, RenderFragment<int> fragment)
                    => builder.AddContent(NextSequence(), fragment, 1);

                public void DirectModifier(RenderTreeBuilder builder)
                    => builder.AddEventPreventDefaultAttribute(0, "click", true);

                public void EffectfulModifier(RenderTreeBuilder builder)
                    => builder.AddEventStopPropagationAttribute(NextSequence(), "click", true);

                public void UnsupportedComponent(RenderTreeBuilder builder)
                    => builder.OpenComponent<PlainChild>(0);

                public void CompoundLocal()
                {
                    var counter = 0;
                    counter += 1;
                }

                public void UnrelatedCall() => Helper();
            }
            """,
            new CSharpParseOptions(LanguageVersion.Preview),
            path: "RenderTreeBuilderPrivateContracts.cs");
        var compilation = CSharpCompilation.Create(
            "RazorVue.RenderTreeBuilderPrivateContracts",
            [syntaxTree],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = RazorSgTestHost.GetCompilationErrors(compilation);
        Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors));
        return new Fixture(compilation, syntaxTree, compilation.GetSemanticModel(syntaxTree));
    }

    private static INamedTypeSymbol GetNamedType(Fixture fixture, string name)
    {
        var type = fixture.Compilation.GetTypeByMetadataName("RenderTreeBuilderPrivateContracts." + name);
        Assert.IsNotNull(type, name);
        return type!;
    }

    private static MethodDeclarationSyntax GetMethod(Fixture fixture, string name)
        => fixture.SyntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Single(method => method.Identifier.ValueText == name);

    private static IBlockOperation GetMethodBody(Fixture fixture, string methodName)
        => GetOperation<IBlockOperation>(fixture, GetMethod(fixture, methodName).Body!);

    private static IInvocationOperation GetInvocation(
        Fixture fixture,
        string methodName,
        Func<IInvocationOperation, bool> predicate)
        => GetMethod(fixture, methodName)
            .DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Select(syntax => GetOperation<IInvocationOperation>(fixture, syntax))
            .Single(predicate);

    private static IVariableDeclaratorOperation GetVariableDeclarator(
        Fixture fixture,
        string methodName,
        string variableName)
        => GetOperation<IVariableDeclaratorOperation>(
            fixture,
            GetMethod(fixture, methodName)
                .DescendantNodes()
                .OfType<VariableDeclaratorSyntax>()
                .Single(declarator => declarator.Identifier.ValueText == variableName));

    private static IOperation GetVariableInitializer(Fixture fixture, string methodName, string variableName)
    {
        var initializer = GetMethod(fixture, methodName)
            .DescendantNodes()
            .OfType<VariableDeclaratorSyntax>()
            .Single(declarator => declarator.Identifier.ValueText == variableName)
            .Initializer;
        Assert.IsNotNull(initializer, variableName);
        return GetOperation<IOperation>(fixture, initializer!.Value);
    }

    private static IOperation UnwrapConversion(IOperation operation)
    {
        while (operation is IConversionOperation conversion)
            operation = conversion.Operand;

        return operation;
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
