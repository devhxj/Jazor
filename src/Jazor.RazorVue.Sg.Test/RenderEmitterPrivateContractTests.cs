using System.Reflection;
using System.Collections.Immutable;
using Acornima.Ast;
using Jazor.RazorVue.RazorSdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RenderEmitterPrivateContractTests
{
    [TestMethod]
    public void RoslynOperationHelpers_ResolveLoopConstantsOmissionAndTerminationShapes()
    {
        var fixture = CreateFixture();
        var inputs = GetMethod(fixture, "OperationShapes", "Inputs");
        var loopLocalDeclarator = GetVariableDeclarator(inputs, "loopLocal");
        var loopLocal = GetOperation<IVariableDeclaratorOperation>(fixture, loopLocalDeclarator).Symbol;
        var loopLocalReference = GetVariableInitializer(fixture, inputs, "omitLocal");
        var loopLocalConversion = GetOperation<IConversionOperation>(fixture, inputs
            .DescendantNodes()
            .OfType<CastExpressionSyntax>()
            .Single(cast => cast.Expression is IdentifierNameSyntax { Identifier.ValueText: "loopLocal" }));
        var loopLocalDeclaration = GetOperation<IVariableDeclarationOperation>(fixture, loopLocalDeclarator.Parent!);
        var loopLocalGroup = GetOperation<IVariableDeclarationGroupOperation>(fixture, loopLocalDeclarator.Parent!.Parent!);
        var literal = GetVariableInitializer(fixture, inputs, "literal");
        var convertedLiteral = GetOperation<IConversionOperation>(fixture, inputs
            .DescendantNodes()
            .OfType<CastExpressionSyntax>()
            .Single(cast => cast.Expression is LiteralExpressionSyntax { Token.ValueText: "converted" }));
        var dynamicText = GetVariableInitializer(fixture, inputs, "dynamicTextValue");
        var nonOmittable = GetVariableInitializer(fixture, inputs, "nonOmittable");

        AssertResolvedLoopVariable(loopLocalReference, loopLocal);
        AssertResolvedLoopVariable(loopLocalConversion, loopLocal);
        AssertResolvedLoopVariable(GetOperation<IVariableDeclaratorOperation>(fixture, loopLocalDeclarator), loopLocal);
        AssertResolvedLoopVariable(loopLocalDeclaration, loopLocal);
        AssertResolvedLoopVariable(loopLocalGroup, loopLocal);
        Assert.IsFalse(Invoke<bool>("TryResolveLoopControlVariable", new object?[] { GetVariableInitializer(fixture, inputs, "notLoopVariable"), null }));

        AssertConstantString(literal, "constant");
        AssertConstantString(convertedLiteral, "converted");
        var nonConstantArguments = new object?[] { dynamicText, null };
        Assert.IsFalse(Invoke<bool>("TryGetConstantString", nonConstantArguments));
        Assert.AreEqual(string.Empty, nonConstantArguments[1]);

        Assert.IsTrue(Invoke<bool>("CanOmit", literal));
        Assert.IsTrue(Invoke<bool>("CanOmit", GetVariableInitializer(fixture, inputs, "omitParameter")));
        Assert.IsTrue(Invoke<bool>("CanOmit", loopLocalReference));
        Assert.IsTrue(Invoke<bool>("CanOmit", GetVariableInitializer(fixture, inputs, "omitField")));
        Assert.IsTrue(Invoke<bool>("CanOmit", GetVariableInitializer(fixture, inputs, "omitProperty")));
        Assert.IsTrue(Invoke<bool>("CanOmit", GetVariableInitializer(fixture, inputs, "omittableBinary")));
        Assert.IsFalse(Invoke<bool>("CanOmit", nonOmittable));
        Assert.IsTrue(Invoke<bool>("CanOmit", loopLocalConversion));

        Invoke<object?>("EnsureSignature", literal, true);
        var signatureFailure = Assert.Throws<TargetInvocationException>(() =>
            Invoke<object?>("EnsureSignature", literal, false));
        StringAssert.Contains(signatureFailure.InnerException!.Message, "Unsupported RenderTreeBuilder overload", StringComparison.Ordinal);

        var returnWithoutValue = GetReturnOperation(fixture, "OperationShapes", "VoidReturn");
        var returnWithValue = GetReturnOperation(fixture, "OperationShapes", "ValueReturn");
        var emptyBlock = GetMethodBody(fixture, "OperationShapes", "Empty");
        var singleReturnBlock = GetMethodBody(fixture, "OperationShapes", "VoidReturn");
        var lastReturnBlock = GetMethodBody(fixture, "OperationShapes", "LastReturn");
        var terminatingConditional = GetIfOperation(fixture, "OperationShapes", "ConditionalBoth");
        var nonTerminatingConditional = GetIfOperation(fixture, "OperationShapes", "ConditionalOne");

        Assert.IsFalse(Invoke<bool>("IsTerminatingWithoutOutput", new object?[] { null }));
        Assert.IsTrue(Invoke<bool>("IsTerminatingWithoutOutput", returnWithoutValue));
        Assert.IsFalse(Invoke<bool>("IsTerminatingWithoutOutput", returnWithValue));
        Assert.IsTrue(Invoke<bool>("IsTerminatingWithoutOutput", singleReturnBlock));
        Assert.IsFalse(Invoke<bool>("IsTerminatingWithoutOutput", lastReturnBlock));

        Assert.IsFalse(Invoke<bool>("IsTerminatingOperation", new object?[] { null }));
        Assert.IsTrue(Invoke<bool>("IsTerminatingOperation", returnWithoutValue));
        Assert.IsFalse(Invoke<bool>("IsTerminatingOperation", returnWithValue));
        Assert.IsFalse(Invoke<bool>("IsTerminatingOperation", emptyBlock));
        Assert.IsTrue(Invoke<bool>("IsTerminatingOperation", lastReturnBlock));
        Assert.IsTrue(Invoke<bool>("IsTerminatingOperation", terminatingConditional));
        Assert.IsFalse(Invoke<bool>("IsTerminatingOperation", nonTerminatingConditional));

        Assert.IsTrue(Invoke<bool>("IsNoOutputOperation", new object?[] { null }));
        Assert.IsTrue(Invoke<bool>("IsNoOutputOperation", emptyBlock));
        Assert.IsFalse(Invoke<bool>("IsNoOutputOperation", singleReturnBlock));
        Assert.IsFalse(Invoke<bool>("IsNoOutputOperation", returnWithoutValue));

        var renderTreeBuilder = fixture.Compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder");
        Assert.IsNotNull(renderTreeBuilder);
        Assert.IsTrue(Invoke<bool>("IsRenderTreeBuilderMethod", renderTreeBuilder!
            .GetMembers("AddContent")
            .OfType<IMethodSymbol>()
            .First()));
        Assert.IsFalse(Invoke<bool>("IsRenderTreeBuilderMethod", GetMethodSymbol(fixture, "OperationShapes", "GetValue")));
    }

    [TestMethod]
    public void TypeAndImportHelpers_ClassifyDirectRenderContractsFromRoslynSymbols()
    {
        var fixture = CreateFixture();
        var inputs = GetMethod(fixture, "OperationShapes", "Inputs");
        var fragment = GetVariableInitializer(fixture, inputs, "fragmentValue");
        var genericFragment = GetVariableInitializer(fixture, inputs, "genericFragmentValue");
        var markup = GetVariableInitializer(fixture, inputs, "markupValue");
        var nullableMarkup = GetVariableInitializer(fixture, inputs, "nullableMarkupValue");
        var text = GetVariableInitializer(fixture, inputs, "dynamicTextValue");

        Assert.IsTrue(Invoke<bool>("IsRenderFragmentOperationValue", fragment));
        Assert.IsFalse(Invoke<bool>("IsRenderFragmentOperationValue", genericFragment));
        Assert.IsTrue(Invoke<bool>("IsGenericRenderFragmentOperationValue", genericFragment));
        Assert.IsFalse(Invoke<bool>("IsGenericRenderFragmentOperationValue", fragment));
        Assert.IsTrue(Invoke<bool>("IsMarkupStringOperationValue", markup));
        Assert.IsTrue(Invoke<bool>("IsMarkupStringOperationValue", nullableMarkup));
        Assert.IsFalse(Invoke<bool>("IsMarkupStringOperationValue", text));
        Assert.IsFalse(Invoke<bool>("IsMarkupStringOperationValue", GetMethodBody(fixture, "OperationShapes", "Empty")));
        Assert.IsTrue(Invoke<bool>("IsNullableMarkupStringOperationValue", nullableMarkup));
        Assert.IsFalse(Invoke<bool>("IsNullableMarkupStringOperationValue", markup));
        Assert.IsFalse(Invoke<bool>("IsNullableMarkupStringOperationValue", GetMethodBody(fixture, "OperationShapes", "Empty")));
        Assert.IsFalse(Invoke<bool>("IsGenericRenderFragmentOperationValue", GetMethodBody(fixture, "OperationShapes", "Empty")));

        Assert.IsTrue(Invoke<bool>("IsRenderFragmentType", fragment.Type));
        Assert.IsFalse(Invoke<bool>("IsRenderFragmentType", genericFragment.Type));
        Assert.IsTrue(Invoke<bool>("IsGenericRenderFragmentType", genericFragment.Type));
        Assert.IsFalse(Invoke<bool>("IsGenericRenderFragmentType", fragment.Type));
        Assert.IsFalse(Invoke<bool>("IsGenericRenderFragmentType", text.Type));
        Assert.IsFalse(Invoke<bool>("IsGenericRenderFragmentType", new object?[] { null }));

        Assert.IsTrue(Invoke<bool>("IsRenderFragmentDescriptorType", GetNamedType(fixture, "ValidDescriptor")));
        Assert.IsFalse(Invoke<bool>("IsRenderFragmentDescriptorType", GetNamedType(fixture, "FieldDescriptor")));
        Assert.IsFalse(Invoke<bool>("IsRenderFragmentDescriptorType", GetNamedType(fixture, "PropertyDescriptor")));
        Assert.IsFalse(Invoke<bool>("IsRenderFragmentDescriptorType", GetNamedType(fixture, "IndexedDescriptor")));
        Assert.IsFalse(Invoke<bool>("IsRenderFragmentDescriptorType", GetNamedType(fixture, "StaticDescriptor")));
        Assert.IsFalse(Invoke<bool>("IsRenderFragmentDescriptorType", GetNamedType(fixture, "TypeParameterOwner`1").TypeParameters.Single()));
        Assert.IsFalse(Invoke<bool>("IsRenderFragmentDescriptorType", fixture.Compilation.GetSpecialType(SpecialType.System_Object)));

        Assert.AreEqual(" ./components/module ", Invoke<string?>("GetECMAScriptModuleExportPath", GetNamedType(fixture, "ModuleComponent")));
        Assert.IsNull(Invoke<string?>("GetECMAScriptModuleExportPath", GetNamedType(fixture, "LibraryComponent")));
        Assert.IsNull(Invoke<string?>("GetECMAScriptModuleExportPath", GetNamedType(fixture, "NoImportComponent")));
        AssertComponentImport(GetNamedType(fixture, "ModuleComponent"), "./components/module.mjs", "default");
        AssertComponentImport(GetNamedType(fixture, "LibraryComponent"), "tdesign-vue-next", "Button");
        var importFailure = Assert.Throws<TargetInvocationException>(() =>
            Invoke<object>("ResolveComponentImport", GetNamedType(fixture, "NoImportComponent")));
        StringAssert.Contains(importFailure.InnerException!.Message, "must declare", StringComparison.Ordinal);
        var invalidLibraryImport = Assert.Throws<TargetInvocationException>(() =>
            Invoke<object>("ResolveComponentImport", GetNamedType(fixture, "InvalidLibraryComponent")));
        StringAssert.Contains(invalidLibraryImport.InnerException!.Message, "must declare", StringComparison.Ordinal);

        var genericInitializer = GetVariableInitializer(fixture, GetMethod(fixture, "OperationShapes", "FragmentLambdas"), "generic");
        var renderInitializer = GetVariableInitializer(fixture, GetMethod(fixture, "OperationShapes", "FragmentLambdas"), "fragment");
        var renderFragmentArguments = new object?[] { renderInitializer, null, null };
        Assert.IsTrue(Invoke<bool>("TryGetRenderFragmentBody", renderFragmentArguments));
        Assert.IsInstanceOfType<IParameterSymbol>(renderFragmentArguments[1]);
        Assert.IsNotNull(renderFragmentArguments[2] as IOperation);

        var genericRenderFragmentArguments = new object?[] { genericInitializer, null, null, null };
        Assert.IsTrue(Invoke<bool>("TryGetGenericRenderFragmentBody", genericRenderFragmentArguments));
        Assert.IsInstanceOfType<IParameterSymbol>(genericRenderFragmentArguments[1]);
        Assert.IsInstanceOfType<IParameterSymbol>(genericRenderFragmentArguments[2]);
        Assert.IsNotNull(genericRenderFragmentArguments[3] as IOperation);
        Assert.IsFalse(Invoke<bool>("TryGetGenericRenderFragmentBody", new object?[] { text, null, null, null }));

        var convertedRenderInitializer = GetVariableInitializer(fixture, GetMethod(fixture, "OperationShapes", "FragmentLambdas"), "convertedFragment");
        var convertedRenderArguments = new object?[] { convertedRenderInitializer, null, null };
        Assert.IsTrue(Invoke<bool>("TryGetRenderFragmentBody", convertedRenderArguments));
        var convertedGenericInitializer = GetVariableInitializer(fixture, GetMethod(fixture, "OperationShapes", "FragmentLambdas"), "convertedGeneric");
        var convertedGenericArguments = new object?[] { convertedGenericInitializer, null, null, null };
        Assert.IsTrue(Invoke<bool>("TryGetGenericRenderFragmentBody", convertedGenericArguments));
    }

    [TestMethod]
    public void AstAndIdentifierHelpers_KeepDirectRenderNamesAndConditionsStable()
    {
        Assert.AreEqual("fallback", Invoke<string>("SanitizeJavaScriptIdentifierPart", "  ", "fallback"));
        Assert.AreEqual("alpha9_$", Invoke<string>("SanitizeJavaScriptIdentifierPart", "alpha9_$", "fallback"));
        Assert.AreEqual("__value_", Invoke<string>("SanitizeJavaScriptIdentifierPart", "9-value!", "fallback"));
        Assert.AreEqual("$ready", Invoke<string>("SanitizeJavaScriptIdentifierPart", "$ready", "fallback"));

        Assert.AreEqual("class", Invoke<string>("NormalizeDirectElementAttributeName", "class"));
        Assert.AreEqual("onClick", Invoke<string>("NormalizeDirectElementAttributeName", "onclick"));
        Assert.AreEqual("onClick", Invoke<string>("NormalizeDirectElementAttributeName", "onClick"));
        Assert.AreEqual("on", Invoke<string>("NormalizeDirectElementAttributeName", "on"));
        Assert.AreEqual("data-value", Invoke<string>("NormalizeDirectElementAttributeName", "data-value"));
        Assert.AreEqual(string.Empty, Invoke<string>("NormalizeDirectComponentParameterName", string.Empty));
        Assert.AreEqual("title", Invoke<string>("NormalizeDirectComponentParameterName", "Title"));

        var noConditions = Invoke<Expression>("LogicalAnd", new object?[] { Array.Empty<Expression>() });
        Assert.IsInstanceOfType<BooleanLiteral>(noConditions);
        Assert.IsTrue(((BooleanLiteral)noConditions).Value);
        var twoConditions = Invoke<Expression>(
            "LogicalAnd",
            new object?[] { new Expression[] { new Identifier("first"), new Identifier("second") } });
        Assert.IsInstanceOfType<LogicalExpression>(twoConditions);
    }

    [TestMethod]
    public void EmitterStaticHelpers_ClassifyBoundOperationsWithoutSyntheticRoslynNodes()
    {
        var fixture = CreateFixture();
        var inputs = GetMethod(fixture, "OperationShapes", "Inputs");

        Assert.IsTrue(InvokeEmitter<bool>("IsRenderTreeBuilderMetadataMethodName", "AddEventPreventDefaultAttribute"));
        Assert.IsTrue(InvokeEmitter<bool>("IsRenderTreeBuilderMetadataMethodName", "AddEventStopPropagationAttribute"));
        Assert.IsTrue(InvokeEmitter<bool>("IsRenderTreeBuilderMetadataMethodName", "AddNamedEvent"));
        Assert.IsFalse(InvokeEmitter<bool>("IsRenderTreeBuilderMetadataMethodName", "AddContent"));

        var dictionaryInitializers = GetMethod(fixture, "OperationShapes", "DictionaryInitializers");
        var assignment = GetOperation<ISimpleAssignmentOperation>(
            fixture,
            dictionaryInitializers.DescendantNodes()
                .OfType<AssignmentExpressionSyntax>()
                .Single(expression => expression.Left.ToString().Contains("\"assignment\"", StringComparison.Ordinal)));
        var addInvocation = GetOperation<IInvocationOperation>(
            fixture,
            dictionaryInitializers.DescendantNodes().OfType<InvocationExpressionSyntax>().Single());
        var convertedAssignment = GetOperation<IConversionOperation>(
            fixture,
            dictionaryInitializers.DescendantNodes()
                .OfType<CastExpressionSyntax>()
                .Single(cast => cast.Expression is ParenthesizedExpressionSyntax));
        var convertedIndexer = GetOperation<IConversionOperation>(
            fixture,
            dictionaryInitializers.DescendantNodes()
                .OfType<CastExpressionSyntax>()
                .Single(cast => cast.Expression is ElementAccessExpressionSyntax));

        AssertAttributeInitializer(assignment, "assignment");
        AssertAttributeInitializer(addInvocation, "add");
        AssertAttributeInitializer(convertedAssignment, "converted");
        Assert.IsFalse(InvokeEmitter<bool>(
            "TryGetAttributeInitializer",
            new object?[] { GetVariableInitializer(fixture, dictionaryInitializers, "notInitializer"), null, null }));

        AssertIndexerKey(assignment.Target, "assignment");
        AssertIndexerKey(convertedIndexer, "indexer");
        Assert.IsFalse(InvokeEmitter<bool>("TryGetIndexerKey", new object?[] { GetVariableInitializer(fixture, inputs, "literal"), null }));

        var deconstruction = GetMethod(fixture, "OperationShapes", "Deconstruction");
        var assignments = deconstruction.DescendantNodes()
            .OfType<AssignmentExpressionSyntax>()
            .Select(expression => GetOperation<IDeconstructionAssignmentOperation>(fixture, expression))
            .ToArray();

        Assert.IsTrue(InvokeEmitter<bool>("IsDiscardDeconstructionTarget", assignments[0].Target));
        Assert.IsFalse(InvokeEmitter<bool>("IsDiscardDeconstructionTarget", assignments[1].Target));
        Assert.IsTrue(InvokeEmitter<bool>("IsCompileTimeOnlyDeconstructionValue", assignments[0].Value));
        Assert.IsFalse(InvokeEmitter<bool>("IsCompileTimeOnlyDeconstructionValue", assignments[2].Value));
        Assert.IsTrue(InvokeEmitter<bool>("IsPureDiscardDeconstructionAssignment", assignments[0]));
        Assert.IsFalse(InvokeEmitter<bool>("IsPureDiscardDeconstructionAssignment", assignments[1]));
        Assert.IsFalse(InvokeEmitter<bool>("IsPureDiscardDeconstructionAssignment", assignments[2]));

        Assert.IsNotNull(InvokeEmitter<object?>("TryGetSingleReturnValue", GetMethodBody(fixture, "OperationShapes", "SingleReturnValue")));
        Assert.IsNull(InvokeEmitter<object?>("TryGetSingleReturnValue", GetMethodBody(fixture, "OperationShapes", "VoidReturn")));
        Assert.IsNull(InvokeEmitter<object?>("TryGetSingleReturnValue", GetMethodBody(fixture, "OperationShapes", "LastReturn")));
        Assert.IsNull(InvokeEmitter<object?>("TryGetSingleReturnValue", GetVariableInitializer(fixture, inputs, "literal")));

        var factoryArguments = new object?[]
        {
            GetMethodBody(fixture, "OperationShapes", "FragmentFactory"),
            null,
            null
        };
        Assert.IsTrue(InvokeEmitter<bool>("TryGetRenderFragmentFactoryReturn", factoryArguments));
        Assert.IsNotNull(factoryArguments[1] as IOperation);
        Assert.AreEqual(1, ((ImmutableArray<IVariableDeclarationGroupOperation>)factoryArguments[2]!).Length);
        Assert.IsFalse(InvokeEmitter<bool>(
            "TryGetRenderFragmentFactoryReturn",
            new object?[] { GetMethodBody(fixture, "OperationShapes", "InvalidFragmentFactory"), null, null }));
        Assert.IsFalse(InvokeEmitter<bool>(
            "TryGetRenderFragmentFactoryReturn",
            new object?[] { GetVariableInitializer(fixture, inputs, "literal"), null, null }));
        Assert.IsFalse(InvokeEmitter<bool>(
            "TryGetRenderFragmentFactoryReturn",
            new object?[] { GetMethodBody(fixture, "OperationShapes", "FragmentFactoryWithoutReturn"), null, null }));

        var attributeBody = GetMethodBody(fixture, "OperationShapes", "AttributeInvocations");
        var attributeInvocation = ((IExpressionStatementOperation)attributeBody.Operations[0]).Operation;
        var attributeInvocations = new object?[] { attributeBody, null };
        Assert.IsTrue(Invoke<bool>("TryGetAttributeInvocations", attributeInvocations));
        Assert.AreEqual(2, ((ImmutableArray<IInvocationOperation>)attributeInvocations[1]!).Length);
        var singleAttributeInvocation = new object?[] { attributeInvocation, null };
        Assert.IsTrue(Invoke<bool>("TryGetAttributeInvocations", singleAttributeInvocation));
        Assert.AreEqual(1, ((ImmutableArray<IInvocationOperation>)singleAttributeInvocation[1]!).Length);
        var directAttributeInvocation = new object?[] { attributeBody.Operations[0], null };
        Assert.IsTrue(Invoke<bool>("TryGetAttributeInvocation", directAttributeInvocation));
        Assert.IsInstanceOfType<IInvocationOperation>(directAttributeInvocation[1]);
        Assert.IsFalse(Invoke<bool>(
            "TryGetAttributeInvocations",
            new object?[] { GetMethodBody(fixture, "OperationShapes", "NonAttributeInvocation"), null }));
        var nonAttributeInvocation = ((IExpressionStatementOperation)GetMethodBody(
            fixture,
            "OperationShapes",
            "NonAttributeInvocation").Operations[0]).Operation;
        Assert.IsFalse(Invoke<bool>(
            "TryGetAttributeInvocations",
            new object?[] { nonAttributeInvocation, null }));

        var helperInvocation = GetOperation<IInvocationOperation>(
            fixture,
            GetMethod(fixture, "OperationShapes", "RuntimeHelpersCall")
                .DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Single());
        Assert.IsTrue(InvokeEmitter<bool>("IsRazorRuntimeHelpersTypeCheck", helperInvocation.TargetMethod));
        Assert.IsFalse(InvokeEmitter<bool>("IsRazorRuntimeHelpersTypeCheck", GetMethodSymbol(fixture, "OperationShapes", "GetValue")));

        var content = new Identifier("content");
        var directFragment = CreateDirectRenderFragment(content, parameterName: null);
        Assert.AreSame(content, Invoke<Expression>("InvokeRenderFragment", directFragment, new Identifier("argument")));
    }

    [TestMethod]
    public void EmitterMemberResolutionHelpers_ValidateGetterFactoryAndConstructorShapes()
    {
        var fixture = CreateFixture();
        var host = GetNamedType(fixture, "EmitterHost");
        var emitter = CreateEmitter(fixture, host);
        var expressionProperty = GetProperty(host, "ExpressionFragment");
        var blockProperty = GetProperty(host, "BlockFragment");
        var autoProperty = GetProperty(host, "AutoFragment");
        var writeOnlyProperty = GetProperty(host, "WriteOnlyFragment");

        AssertReturnedPropertyValue(emitter, expressionProperty);
        AssertReturnedPropertyValue(emitter, blockProperty);
        Assert.IsFalse(InvokeEmitterInstance<bool>(emitter, "TryGetReturnedPropertyValue", new object?[] { autoProperty, null }));
        Assert.IsFalse(InvokeEmitterInstance<bool>(emitter, "TryGetReturnedPropertyValue", new object?[] { writeOnlyProperty, null }));

        AssertReturnedRenderFragmentBody(emitter, GetMethodSymbol(fixture, "EmitterHost", "ExpressionFactory"));
        AssertReturnedRenderFragmentBody(emitter, GetMethodSymbol(fixture, "EmitterHost", "BlockLiteralFactory"));
        Assert.IsFalse(InvokeEmitterInstance<bool>(
            emitter,
            "TryGetReturnedRenderFragmentBody",
            new object?[] { GetMethodSymbol(fixture, "EmitterHost", "BlockFactory"), null }));
        Assert.IsFalse(InvokeEmitterInstance<bool>(
            emitter,
            "TryGetReturnedRenderFragmentBody",
            new object?[] { GetMethodSymbol(fixture, "EmitterHost", "InvalidFactory"), null }));
        Assert.IsFalse(InvokeEmitterInstance<bool>(
            emitter,
            "TryGetReturnedRenderFragmentBody",
            new object?[] { GetMethodSymbol(fixture, "EmitterHost", "FactoryWithoutReturn"), null }));
        Assert.IsFalse(InvokeEmitterInstance<bool>(
            emitter,
            "TryGetReturnedRenderFragmentBody",
            new object?[] { GetMethodSymbol(fixture, "EmitterHost", "NonFragmentFactory"), null }));

        AssertConstructorMap(emitter, GetNamedType(fixture, "MappedCarrier"), expected: true);
        AssertConstructorMap(emitter, GetNamedType(fixture, "MixedCarrier"), expected: true);
        AssertConstructorMap(emitter, GetNamedType(fixture, "UnmappedCarrier"), expected: false);
        AssertConstructorMap(emitter, GetNamedType(fixture, "ExpressionCarrier"), expected: false);
    }

    private static void AssertResolvedLoopVariable(IOperation operation, ILocalSymbol expected)
    {
        var arguments = new object?[] { operation, null };
        Assert.IsTrue(Invoke<bool>("TryResolveLoopControlVariable", arguments));
        Assert.IsTrue(SymbolEqualityComparer.Default.Equals(expected, arguments[1] as ILocalSymbol));
    }

    private static void AssertConstantString(IOperation operation, string expected)
    {
        var arguments = new object?[] { operation, null };
        Assert.IsTrue(Invoke<bool>("TryGetConstantString", arguments));
        Assert.AreEqual(expected, arguments[1]);
    }

    private static void AssertComponentImport(INamedTypeSymbol componentType, string importSpecifier, string exportName)
    {
        var descriptor = Invoke<object>("ResolveComponentImport", componentType);
        var descriptorType = descriptor.GetType();
        var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        Assert.AreEqual(importSpecifier, descriptorType.GetProperty("ImportSpecifier", flags)!.GetValue(descriptor));
        Assert.AreEqual(exportName, descriptorType.GetProperty("ExportName", flags)!.GetValue(descriptor));
    }

    private static T Invoke<T>(string methodName, params object?[] arguments)
    {
        var method = typeof(RenderEmitter)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
            .Single(candidate => candidate.Name == methodName && candidate.GetParameters().Length == arguments.Length);
        return (T)method.Invoke(null, arguments)!;
    }

    private static T InvokeEmitter<T>(string methodName, params object?[] arguments)
    {
        var emitterType = typeof(RenderEmitter).GetNestedType("Emitter", BindingFlags.NonPublic);
        Assert.IsNotNull(emitterType);
        var method = emitterType!
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
            .Single(candidate => candidate.Name == methodName && candidate.GetParameters().Length == arguments.Length);
        return (T)method.Invoke(null, arguments)!;
    }

    private static object CreateEmitter(Fixture fixture, INamedTypeSymbol componentSymbol)
    {
        var emitterType = typeof(RenderEmitter).GetNestedType("Emitter", BindingFlags.NonPublic);
        Assert.IsNotNull(emitterType);
        var constructor = emitterType!
            .GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Single(candidate => candidate.GetParameters().Length == 4);
        return constructor.Invoke(
        [
            fixture.Compilation,
            componentSymbol,
            null,
            VueInjectRegistry.ForCompilation(fixture.Compilation)
        ]);
    }

    private static T InvokeEmitterInstance<T>(object emitter, string methodName, params object?[] arguments)
    {
        var method = emitter.GetType()
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
            .Single(candidate => candidate.Name == methodName && candidate.GetParameters().Length == arguments.Length);
        return (T)method.Invoke(emitter, arguments)!;
    }

    private static void AssertReturnedPropertyValue(object emitter, IPropertySymbol property)
    {
        var arguments = new object?[] { property, null };
        Assert.IsTrue(InvokeEmitterInstance<bool>(emitter, "TryGetReturnedPropertyValue", arguments));
        Assert.IsNotNull(arguments[1]);
    }

    private static void AssertReturnedRenderFragmentBody(object emitter, IMethodSymbol method)
    {
        var arguments = new object?[] { method, null };
        Assert.IsTrue(InvokeEmitterInstance<bool>(emitter, "TryGetReturnedRenderFragmentBody", arguments));
        Assert.IsNotNull(arguments[1]);
    }

    private static void AssertConstructorMap(object emitter, INamedTypeSymbol type, bool expected)
    {
        var constructor = type.InstanceConstructors.Single(candidate => !candidate.IsImplicitlyDeclared);
        var arguments = new object?[] { constructor, null };
        Assert.AreEqual(expected, InvokeEmitterInstance<bool>(emitter, "TryBuildConstructorRenderFragmentPropertyMap", arguments));
        if (expected)
            Assert.IsNotNull(arguments[1]);
    }

    private static void AssertAttributeInitializer(IOperation operation, string key)
    {
        var arguments = new object?[] { operation, null, null };
        Assert.IsTrue(InvokeEmitter<bool>("TryGetAttributeInitializer", arguments));
        AssertConstantString((IOperation)arguments[1]!, key);
        Assert.IsNotNull(arguments[2] as IOperation);
    }

    private static void AssertIndexerKey(IOperation operation, string key)
    {
        var arguments = new object?[] { operation, null };
        Assert.IsTrue(InvokeEmitter<bool>("TryGetIndexerKey", arguments));
        AssertConstantString((IOperation)arguments[1]!, key);
    }

    private static object CreateDirectRenderFragment(Expression renderExpression, string? parameterName)
    {
        var fragmentType = typeof(RenderEmitter).GetNestedType("DirectRenderFragment", BindingFlags.NonPublic);
        Assert.IsNotNull(fragmentType);
        var constructor = fragmentType!
            .GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Single(candidate => candidate.GetParameters().Length == 8);
        return constructor.Invoke(new object?[]
        {
            renderExpression,
            parameterName,
            false,
            false,
            null,
            null,
            null,
            false
        });
    }

    private static Fixture CreateFixture()
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(
            """
            using System;
            using System.Collections.Generic;
            using ECMAScript;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace RenderEmitterPrivateContracts;

            [Obsolete]
            [ECMAScriptModule(" ./components/module ")]
            public sealed class ModuleComponent;

            [ECMAScriptModule(" ")]
            [VueLibraryComponent(" tdesign-vue-next ", " Button ")]
            public sealed class LibraryComponent;

            [Obsolete]
            public sealed class NoImportComponent;

            [VueLibraryComponent(" ", "Button")]
            public sealed class InvalidLibraryComponent;

            public class BaseDescriptor
            {
                public RenderFragment BaseSlot { get; set; } = default!;
                public static string? Ignored { get; set; }
            }

            public sealed class ValidDescriptor : BaseDescriptor
            {
                public RenderFragment<int> ItemTemplate { get; set; } = default!;
            }

            public sealed class FieldDescriptor
            {
                public RenderFragment Slot { get; set; } = default!;
                public int State;
            }

            public sealed class PropertyDescriptor
            {
                public string Name { get; set; } = string.Empty;
            }

            public sealed class IndexedDescriptor
            {
                public RenderFragment this[int index] => default!;
            }

            public sealed class StaticDescriptor
            {
                public static RenderFragment? Slot { get; set; }
            }

            public sealed class TypeParameterOwner<T>;

            public sealed class EmitterHost : ComponentBase
            {
                public RenderFragment ExpressionFragment => builder => { };

                public RenderFragment BlockFragment
                {
                    get
                    {
                        return builder => { };
                    }
                }

                public RenderFragment AutoFragment { get; set; } = default!;

                public RenderFragment WriteOnlyFragment
                {
                    set { }
                }

                public RenderFragment ExpressionFactory() => builder => { };

                public RenderFragment BlockLiteralFactory()
                {
                    return builder => { };
                }

                public RenderFragment BlockFactory()
                {
                    RenderFragment local = builder => { };
                    return local;
                }

                public RenderFragment InvalidFactory()
                {
                    var state = 1;
                    return builder => { };
                }

                public void FactoryWithoutReturn()
                {
                    RenderFragment local = builder => { };
                }

                public int NonFragmentFactory() => 1;
            }

            public sealed class MappedCarrier
            {
                public MappedCarrier(RenderFragment header)
                {
                    Header = header;
                }

                public RenderFragment Header { get; set; } = default!;
            }

            public sealed class MixedCarrier
            {
                public MixedCarrier(int ignored, RenderFragment header)
                {
                    Header = header;
                }

                public RenderFragment Header { get; set; } = default!;
            }

            public sealed class UnmappedCarrier
            {
                public UnmappedCarrier(RenderFragment header)
                {
                }

                public RenderFragment Header { get; set; } = default!;
            }

            public sealed class ExpressionCarrier
            {
                public ExpressionCarrier(RenderFragment header) => Header = header;

                public RenderFragment Header { get; set; } = default!;
            }

            public sealed class OperationShapes
            {
                public int Field;
                public int Property { get; set; }

                private static int GetValue() => 1;

                public void VoidReturn() { return; }
                public int ValueReturn() { return 1; }
                public int SingleReturnValue() { return 7; }
                public void Empty() { }
                public void LastReturn() { var intermediate = 0; return; }
                public void ConditionalBoth(bool condition) { if (condition) { return; } else { return; } }
                public void ConditionalOne(bool condition) { if (condition) { return; } else { var intermediate = 0; } }

                public void Inputs(
                    int parameter,
                    string dynamicText,
                    RenderFragment fragment,
                    RenderFragment<int> genericFragment,
                    MarkupString markup,
                    MarkupString? nullableMarkup)
                {
                    int loopLocal = 1;
                    object convertedLoopLocal = (object)loopLocal;
                    var literal = "constant";
                    object convertedLiteral = (object)"converted";
                    var dynamicTextValue = dynamicText;
                    var omitParameter = parameter;
                    var omitLocal = loopLocal;
                    var omitField = Field;
                    var omitProperty = Property;
                    var omittableBinary = parameter + loopLocal;
                    var nonOmittable = parameter + GetValue();
                    var fragmentValue = fragment;
                    var genericFragmentValue = genericFragment;
                    var markupValue = markup;
                    var nullableMarkupValue = nullableMarkup;
                    var notLoopVariable = 999;
                }

                public void DictionaryInitializers()
                {
                    var attributes = new Dictionary<string, object>();
                    attributes["assignment"] = 1;
                    attributes.Add("add", 2);
                    object convertedAssignment = (object)(attributes["converted"] = 3);
                    object convertedIndexer = (object)attributes["indexer"];
                    var notInitializer = 1;
                }

                public void Deconstruction()
                {
                    var kept = 0;
                    (_, _) = (nameof(Field), 1);
                    (_, kept) = (nameof(Field), 1);
                    (_, _) = (nameof(Field), GetValue());
                }

                public void FragmentLambdas()
                {
                    RenderFragment fragment = builder => { };
                    RenderFragment<int> generic = value => builder => { };
                    RenderFragment convertedFragment = (RenderFragment)(builder => { });
                    RenderFragment<int> convertedGeneric = (RenderFragment<int>)(value => builder => { });
                }

                public RenderFragment FragmentFactory()
                {
                    RenderFragment local = builder => { };
                    return local;
                }

                public RenderFragment InvalidFragmentFactory()
                {
                    var local = 1;
                    return builder => { };
                }

                public void FragmentFactoryWithoutReturn()
                {
                    RenderFragment local = builder => { };
                }

                public void AttributeInvocations(RenderTreeBuilder builder)
                {
                    builder.AddAttribute(0, "class", "value");
                    builder.AddComponentParameter(1, "Value", 2);
                }

                public void NonAttributeInvocation(RenderTreeBuilder builder)
                {
                    builder.AddContent(0, "value");
                }

                public void RuntimeHelpersCall()
                {
                    var checkedValue = Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(1);
                }
            }
            """,
            new CSharpParseOptions(LanguageVersion.Preview),
            path: "RenderEmitterPrivateContracts.cs");
        var compilation = CSharpCompilation.Create(
            "RazorVue.RenderEmitterPrivateContracts",
            [syntaxTree],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = RazorSgTestHost.GetCompilationErrors(compilation);
        Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors));
        return new Fixture(compilation, syntaxTree, compilation.GetSemanticModel(syntaxTree));
    }

    private static INamedTypeSymbol GetNamedType(Fixture fixture, string metadataName)
    {
        var type = fixture.Compilation.GetTypeByMetadataName("RenderEmitterPrivateContracts." + metadataName);
        Assert.IsNotNull(type, metadataName);
        return type!;
    }

    private static MethodDeclarationSyntax GetMethod(Fixture fixture, string typeName, string methodName)
        => fixture.SyntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Single(@class => @class.Identifier.ValueText == typeName)
            .Members
            .OfType<MethodDeclarationSyntax>()
            .Single(method => method.Identifier.ValueText == methodName);

    private static IMethodSymbol GetMethodSymbol(Fixture fixture, string typeName, string methodName)
    {
        var symbol = fixture.SemanticModel.GetDeclaredSymbol(GetMethod(fixture, typeName, methodName));
        Assert.IsNotNull(symbol, methodName);
        return symbol!;
    }

    private static IPropertySymbol GetProperty(INamedTypeSymbol type, string name)
        => type.GetMembers(name).OfType<IPropertySymbol>().Single();

    private static IBlockOperation GetMethodBody(Fixture fixture, string typeName, string methodName)
        => GetOperation<IBlockOperation>(fixture, GetMethod(fixture, typeName, methodName).Body!);

    private static IReturnOperation GetReturnOperation(Fixture fixture, string typeName, string methodName)
        => GetOperation<IReturnOperation>(
            fixture,
            GetMethod(fixture, typeName, methodName)
                .DescendantNodes()
                .OfType<ReturnStatementSyntax>()
                .Single());

    private static IConditionalOperation GetIfOperation(Fixture fixture, string typeName, string methodName)
        => GetOperation<IConditionalOperation>(
            fixture,
            GetMethod(fixture, typeName, methodName)
                .DescendantNodes()
                .OfType<IfStatementSyntax>()
                .Single());

    private static VariableDeclaratorSyntax GetVariableDeclarator(MethodDeclarationSyntax method, string name)
        => method.DescendantNodes()
            .OfType<VariableDeclaratorSyntax>()
            .Single(declarator => declarator.Identifier.ValueText == name);

    private static IOperation GetVariableInitializer(Fixture fixture, MethodDeclarationSyntax method, string name)
        => GetOperation<IOperation>(fixture, GetVariableDeclarator(method, name).Initializer!.Value);

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
