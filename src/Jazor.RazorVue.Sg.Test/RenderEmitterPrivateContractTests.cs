using System.Reflection;
using System.Collections.Immutable;
using Acornima;
using Acornima.Ast;
using Jazor.Compiler;
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
        var nonStringConstantArguments = new object?[] { GetVariableInitializer(fixture, inputs, "staticInt"), null };
        Assert.IsFalse(Invoke<bool>("TryGetConstantString", nonStringConstantArguments));
        Assert.AreEqual(string.Empty, nonStringConstantArguments[1]);

        var assignedLocalArguments = new object?[] { loopLocalConversion, null };
        Assert.IsTrue(InvokeEmitter<bool>("TryGetAssignedLocal", assignedLocalArguments));
        Assert.IsTrue(SymbolEqualityComparer.Default.Equals(loopLocal, assignedLocalArguments[1] as ILocalSymbol));
        var deduplicatedLoopLocals = InvokeEmitter<ImmutableArray<ILocalSymbol>>(
            "GetLoopControlLocals",
            GetVariableInitializer(fixture, inputs, "repeatedLoopLocal"));
        Assert.HasCount(1, deduplicatedLoopLocals);
        Assert.IsTrue(SymbolEqualityComparer.Default.Equals(loopLocal, deduplicatedLoopLocals[0]));

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
        Assert.IsNull(Invoke<string?>("GetECMAScriptModuleExportPath", GetNamedType(fixture, "NoArgumentModuleComponent")));
        Assert.IsNull(Invoke<string?>("GetECMAScriptModuleExportPath", GetNamedType(fixture, "NullModuleComponent")));
        Assert.IsNull(Invoke<string?>("GetECMAScriptModuleExportPath", GetNamedType(fixture, "NoImportComponent")));
        AssertComponentImport(fixture, GetNamedType(fixture, "ModuleComponent"), "./components/module.mjs", "default");
        AssertComponentImport(fixture, GetNamedType(fixture, "LibraryComponent"), "tdesign-vue-next", "Button");
        var slotMapComponent = GetNamedType(fixture, "SlotMapComponent");
        var slotNames = Invoke<ImmutableDictionary<IPropertySymbol, string>>(
            "BuildComponentSlotNameMap",
            slotMapComponent);
        Assert.AreEqual("header-slot", slotNames[GetProperty(slotMapComponent, "Header")]);
        var noArgumentImport = Assert.Throws<TargetInvocationException>(() =>
            Invoke<object>("ResolveComponentImport", fixture.Compilation, GetNamedType(fixture, "NoArgumentModuleComponent")));
        StringAssert.Contains(noArgumentImport.InnerException!.Message, "must declare", StringComparison.Ordinal);
        var nullModuleImport = Assert.Throws<TargetInvocationException>(() =>
            Invoke<object>("ResolveComponentImport", fixture.Compilation, GetNamedType(fixture, "NullModuleComponent")));
        StringAssert.Contains(nullModuleImport.InnerException!.Message, "must declare", StringComparison.Ordinal);
        var importFailure = Assert.Throws<TargetInvocationException>(() =>
            Invoke<object>("ResolveComponentImport", fixture.Compilation, GetNamedType(fixture, "NoImportComponent")));
        StringAssert.Contains(importFailure.InnerException!.Message, "must declare", StringComparison.Ordinal);
        var allowMarkerFailure = Assert.Throws<TargetInvocationException>(() =>
            Invoke<object>("ResolveComponentImport", fixture.Compilation, GetNamedType(fixture, "AllowMarkerComponent")));
        StringAssert.Contains(allowMarkerFailure.InnerException!.Message, "Transform.Component", StringComparison.Ordinal);
        var importMarkerFailure = Assert.Throws<TargetInvocationException>(() =>
            Invoke<object>("ResolveComponentImport", fixture.Compilation, GetNamedType(fixture, "ImportMarkerComponent")));
        StringAssert.Contains(importMarkerFailure.InnerException!.Message, "Transform.Component", StringComparison.Ordinal);
        var invalidLibraryImport = Assert.Throws<TargetInvocationException>(() =>
            Invoke<object>("ResolveComponentImport", fixture.Compilation, GetNamedType(fixture, "InvalidLibraryComponent")));
        StringAssert.Contains(invalidLibraryImport.InnerException!.Message, "must declare", StringComparison.Ordinal);

        var contractFailure = Assert.Throws<TargetInvocationException>(() =>
            Invoke<object>("EnsureRazorVueComponentContract", fixture.Compilation, GetNamedType(fixture, "SlotMapComponent")));
        StringAssert.Contains(contractFailure.InnerException!.Message, "must derive", StringComparison.Ordinal);

        Assert.IsFalse(Invoke<bool>("IsChangeEventCallbackType", new object?[] { null }));

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
    public void RoslynConversionOperations_UnwrapDirectCastSyntaxAcrossRenderHelpers()
    {
        var fixture = CreateFixture();
        var inputs = GetMethod(fixture, "OperationShapes", "Inputs");
        var fragmentLambdas = GetMethod(fixture, "OperationShapes", "FragmentLambdas");
        var convertedFragment = GetOperation<IConversionOperation>(
            fixture,
            GetVariableDeclarator(fragmentLambdas, "convertedFragmentAsObject").Initializer!.Value);
        var convertedGenericFragment = GetOperation<IConversionOperation>(
            fixture,
            GetVariableDeclarator(fragmentLambdas, "convertedGenericFragmentAsObject").Initializer!.Value);
        var convertedNullableMarkup = GetOperation<IConversionOperation>(
            fixture,
            inputs.DescendantNodes()
                .OfType<CastExpressionSyntax>()
                .Single(cast => cast.Type.ToString() == "MarkupString?"));
        var convertedFragmentReference = GetOperation<IConversionOperation>(
            fixture,
            GetVariableDeclarator(fragmentLambdas, "convertedFragmentReference").Initializer!.Value);
        var convertedGenericFragmentReference = GetOperation<IConversionOperation>(
            fixture,
            GetVariableDeclarator(fragmentLambdas, "convertedGenericFragmentReference").Initializer!.Value);

        var fragmentArguments = new object?[] { convertedFragment, null, null };
        Assert.IsTrue(Invoke<bool>("TryGetRenderFragmentBody", fragmentArguments));
        Assert.IsNotNull(fragmentArguments[1]);
        Assert.IsNotNull(fragmentArguments[2]);
        Assert.IsTrue(Invoke<bool>("IsRenderFragmentOperationValue", convertedFragmentReference));

        var genericArguments = new object?[] { convertedGenericFragment, null, null, null };
        Assert.IsTrue(Invoke<bool>("TryGetGenericRenderFragmentBody", genericArguments));
        Assert.IsNotNull(genericArguments[1]);
        Assert.IsNotNull(genericArguments[2]);
        Assert.IsNotNull(genericArguments[3]);
        Assert.IsTrue(Invoke<bool>("IsGenericRenderFragmentOperationValue", convertedGenericFragmentReference));

        Assert.IsTrue(Invoke<bool>("IsNullableMarkupStringOperationValue", convertedNullableMarkup));

        var bindingMethod = GetMethod(fixture, "OperationShapes", "BuilderBindings");
        var castReceiverInvocation = bindingMethod.DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Select(syntax => GetOperation<IInvocationOperation>(fixture, syntax))
            .Single(invocation => invocation.Instance is IConversionOperation);
        var binding = GetMethodSymbol(fixture, "OperationShapes", "BuilderBindings").Parameters[0];
        var context = CreateEmitContext(binding);
        Assert.IsFalse(Invoke<bool>("IsSecondaryBuilderInvocation", castReceiverInvocation, context));
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

        var noConditions = Invoke<Expression>("LogicalAnd", new object?[] { Array.Empty<Expression>() });
        Assert.IsInstanceOfType<BooleanLiteral>(noConditions);
        Assert.IsTrue(((BooleanLiteral)noConditions).Value);
        var twoConditions = Invoke<Expression>(
            "LogicalAnd",
            new object?[] { new Expression[] { new Identifier("first"), new Identifier("second") } });
        Assert.IsInstanceOfType<LogicalExpression>(twoConditions);
    }

    [TestMethod]
    public void StaticTextEligibility_AcceptsOnlyConstantScalarPayloads()
    {
        var fixture = CreateFixture();
        var inputs = GetMethod(fixture, "OperationShapes", "Inputs");
        var allowedConstants = new[]
        {
            "staticNull",
            "staticString",
            "staticBoolean",
            "staticChar",
            "staticSByte",
            "staticByte",
            "staticShort",
            "staticUShort",
            "staticInt",
            "staticUInt",
            "staticLong",
            "staticULong",
            "staticFloat",
            "staticDouble",
            "staticDecimal",
            "constantSByte",
            "constantByte",
            "constantShort",
            "constantUShort",
            "constantUInt",
            "constantLong",
            "constantULong",
            "constantFloat",
            "constantDouble",
            "constantDecimal"
        };

        foreach (var name in allowedConstants)
            Assert.IsTrue(Invoke<bool>("IsStaticTextContent", GetVariableInitializer(fixture, inputs, name)), name);

        // Conversion is transparent for literal staticness; this also guards the unwrap loop.
        Assert.IsTrue(Invoke<bool>("IsStaticTextContent", GetVariableInitializer(fixture, inputs, "convertedLiteral")));
        var dynamicText = GetVariableInitializer(fixture, inputs, "dynamicTextValue");
        Assert.IsFalse(Invoke<bool>("IsStaticTextContent", dynamicText));
        Assert.IsTrue(Invoke<bool>("IsGuaranteedStringTextContent", GetVariableInitializer(fixture, inputs, "staticString")));
        Assert.IsFalse(Invoke<bool>("IsGuaranteedStringTextContent", GetVariableInitializer(fixture, inputs, "staticInt")));
        Assert.IsFalse(Invoke<bool>("IsGuaranteedStringTextContent", GetMethodBody(fixture, "OperationShapes", "Empty")));
    }

    [TestMethod]
    public void EventModifierAndSignatureHelpers_PreserveImmediateAndConditionalProtocols()
    {
        var fixture = CreateFixture();
        var inputs = GetMethod(fixture, "OperationShapes", "Inputs");
        var immediate = new List<Statement>();
        var conditional = new List<Statement>();
        var skipped = new List<Statement>();
        var absent = new List<Statement>();

        Invoke<object?>(
            "AddDirectEventModifierStatement",
            immediate,
            new Identifier("event"),
            new BooleanLiteral(true, "true"),
            "preventDefault");
        Invoke<object?>(
            "AddDirectEventModifierStatement",
            conditional,
            new Identifier("event"),
            new Identifier("enabled"),
            "stopPropagation");
        Invoke<object?>(
            "AddDirectEventModifierStatement",
            skipped,
            new Identifier("event"),
            new BooleanLiteral(false, "false"),
            "preventDefault");
        Invoke<object?>(
            "AddDirectEventModifierStatement",
            absent,
            new Identifier("event"),
            null,
            "preventDefault");
        Invoke<object?>(
            "EnsureSignature",
            GetVariableInitializer(fixture, inputs, "literal"),
            true);

        Assert.HasCount(1, immediate);
        Assert.IsInstanceOfType<NonSpecialExpressionStatement>(immediate[0]);
        Assert.HasCount(1, conditional);
        Assert.IsInstanceOfType<IfStatement>(conditional[0]);
        Assert.IsEmpty(skipped);
        Assert.IsEmpty(absent);
    }

    [TestMethod]
    public void StaticMarkupProbe_SeparatesRawStringsMarkupObjectsAndDynamicPayloads()
    {
        var fixture = CreateFixture();
        var inputs = GetMethod(fixture, "OperationShapes", "Inputs");

        AssertStaticMarkup(
            GetVariableInitializer(fixture, inputs, "rawStaticMarkup"),
            allowRawStringLiteral: true,
            expected: true,
            "<strong>raw</strong>");
        AssertStaticMarkup(
            GetVariableInitializer(fixture, inputs, "rawStaticMarkup"),
            allowRawStringLiteral: false,
            expected: false,
            string.Empty);
        AssertStaticMarkup(
            GetVariableInitializer(fixture, inputs, "markupStatic"),
            allowRawStringLiteral: false,
            expected: true,
            "<em>markup</em>");
        AssertStaticMarkup(
            GetVariableInitializer(fixture, inputs, "markupDynamic"),
            allowRawStringLiteral: true,
            expected: false,
            string.Empty);

        static void AssertStaticMarkup(
            IOperation operation,
            bool allowRawStringLiteral,
            bool expected,
            string expectedMarkup)
        {
            var arguments = new object?[] { operation, allowRawStringLiteral, null };
            Assert.AreEqual(expected, Invoke<bool>("TryGetStaticMarkupText", arguments));
            Assert.AreEqual(expectedMarkup, arguments[2]);
        }
    }

    [TestMethod]
    public void StaticPropHoisting_RejectsDynamicIdentityAndRenderScopedProps()
    {
        var fixture = CreateFixture();
        var emitter = CreateEmitter(fixture, GetNamedType(fixture, "ModuleComponent"));

        Assert.IsTrue(InvokeEmitterInstance<bool>(
            emitter,
            "CanHoistStaticProps",
            Props(
                Property("title", new StringLiteral("fixed", "\"fixed\"")),
                Property("enabled", new BooleanLiteral(true, "true")),
                Property("count", new NumericLiteral(1, "1")),
                Property("empty", new NullLiteral("null")))));
        Assert.IsTrue(InvokeEmitterInstance<bool>(
            emitter,
            "CanHoistStaticProps",
            new ObjectExpression(NodeList.From<Node>(
                new ObjectProperty(
                    PropertyKind.Init,
                    new StringLiteral("data-id", "\"data-id\""),
                    new StringLiteral("fixed", "\"fixed\""),
                    computed: false,
                    shorthand: false,
                    method: false)))));
        Assert.IsTrue(InvokeEmitterInstance<bool>(
            emitter,
            "CanHoistStaticProps",
            Props(Property("data-id", new StringLiteral("fixed", "\"fixed\"")))));
        Assert.IsFalse(InvokeEmitterInstance<bool>(emitter, "CanHoistStaticProps", Props()));
        Assert.IsFalse(InvokeEmitterInstance<bool>(
            emitter,
            "CanHoistStaticProps",
            Props(Property("title", new Identifier("runtimeValue")))));
        Assert.IsFalse(InvokeEmitterInstance<bool>(
            emitter,
            "CanHoistStaticProps",
            Props(Property("title", new StringLiteral("fixed", "\"fixed\""), computed: true))));
        Assert.IsFalse(InvokeEmitterInstance<bool>(
            emitter,
            "CanHoistStaticProps",
            Props(Property("key", new StringLiteral("fixed", "\"fixed\"")))));
        Assert.IsFalse(InvokeEmitterInstance<bool>(
            emitter,
            "CanHoistStaticProps",
            Props(Property("ref", new StringLiteral("fixed", "\"fixed\"")))));
        Assert.IsFalse(InvokeEmitterInstance<bool>(
            emitter,
            "CanHoistStaticProps",
            Props(Property("onClick", new StringLiteral("fixed", "\"fixed\"")))));

        SetNestedPrivateField(emitter, "_nonHoistableRenderScopeDepth", 1);
        Assert.IsFalse(InvokeEmitterInstance<bool>(
            emitter,
            "CanHoistStaticProps",
            Props(Property("title", new StringLiteral("fixed", "\"fixed\"")))));

        static ObjectExpression Props(params Node[] members)
            => new(NodeList.From<Node>(members));

        static ObjectProperty Property(string name, Expression value, bool computed = false)
            => new(
                PropertyKind.Init,
                new Identifier(name),
                value,
                computed,
                shorthand: false,
                method: false);
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

        Assert.IsTrue(Invoke<bool>(
            "IsStaticTextContent",
            GetVariableInitializer(fixture, inputs, "literal")));
        Assert.IsFalse(Invoke<bool>(
            "IsStaticTextContent",
            GetVariableInitializer(fixture, inputs, "dynamicTextValue")));

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
        Assert.IsNotNull(Invoke<IOperation?>("TryGetSingleReturnedValue", GetMethodBody(fixture, "OperationShapes", "SingleReturnValue")));
        Assert.IsNull(Invoke<IOperation?>("TryGetSingleReturnedValue", GetMethodBody(fixture, "OperationShapes", "VoidReturn")));
        Assert.IsNull(Invoke<IOperation?>("TryGetSingleReturnedValue", GetMethodBody(fixture, "OperationShapes", "LastReturn")));
        Assert.IsNull(Invoke<IOperation?>("TryGetSingleReturnedValue", GetMethodBody(fixture, "OperationShapes", "SingleStatementWithoutReturn")));
        Assert.IsNull(Invoke<IOperation?>("TryGetSingleReturnedValue", GetVariableInitializer(fixture, inputs, "literal")));

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
        Assert.IsFalse(InvokeEmitter<bool>("IsRazorRuntimeHelpersTypeCheck", GetMethodSymbol(fixture, "RuntimeHelpers", "TypeCheck")));

        var content = new Identifier("content");
        var directFragment = CreateDirectRenderFragment(content, parameterName: null);
        Assert.AreSame(content, Invoke<Expression>("InvokeRenderFragment", directFragment, new Identifier("argument")));
    }

    [TestMethod]
    public void EmitterEdgeHelpers_UseRoslynConversionAndNoContextOperationShapes()
    {
        var fixture = CreateFixture();
        var inputs = GetMethod(fixture, "OperationShapes", "Inputs");
        var convertedLoopLocal = GetOperation<IConversionOperation>(fixture, inputs
            .DescendantNodes()
            .OfType<CastExpressionSyntax>()
            .Single(cast => cast.Expression is IdentifierNameSyntax { Identifier.ValueText: "loopLocal" }));
        var convertedLiteral = GetOperation<IConversionOperation>(fixture, inputs
            .DescendantNodes()
            .OfType<CastExpressionSyntax>()
            .Single(cast => cast.Expression is LiteralExpressionSyntax { Token.ValueText: "converted" }));

            Assert.IsFalse(InvokeEmitter<bool>("IsDiscardDeconstructionTarget", convertedLoopLocal));
        Assert.IsTrue(InvokeEmitter<bool>("IsCompileTimeOnlyDeconstructionValue", convertedLiteral));

        var loopLocals = InvokeEmitter<ImmutableArray<ILocalSymbol>>(
            "GetLoopControlLocals",
            GetVariableInitializer(fixture, inputs, "omitLocal"));
        Assert.AreEqual(1, loopLocals.Length);
        Assert.AreEqual("loopLocal", loopLocals[0].Name);

        var collectionInitializer = GetOperation<IObjectOrCollectionInitializerOperation>(
            fixture,
            GetMethod(fixture, "OperationShapes", "NonAttributeCollectionInitializer")
                .DescendantNodes()
                .OfType<InitializerExpressionSyntax>()
                .Single(static initializer => initializer.IsKind(SyntaxKind.CollectionInitializerExpression)));
        Assert.IsFalse(InvokeEmitter<bool>(
            "TryGetAttributeInitializer",
            new object?[] { collectionInitializer.Initializers.Single(), null, null }));

        var host = GetNamedType(fixture, "EmitterHost");
        var emitter = CreateEmitter(fixture, host);
        var parameterReference = GetVariableInitializer(fixture, inputs, "omitParameter") as IParameterReferenceOperation;
        var localReference = GetVariableInitializer(fixture, inputs, "omitLocal") as ILocalReferenceOperation;
        Assert.IsNotNull(parameterReference);
        Assert.IsNotNull(localReference);
        Assert.IsNull(InvokeEmitterInstance<Expression?>(emitter, "RewriteDirectParameterReference", parameterReference!, new SenseArgument()));
        Assert.IsNull(InvokeEmitterInstance<Expression?>(emitter, "RewriteDirectLocalReference", localReference!, new SenseArgument()));

        var localFunction = GetMethod(fixture, "OperationShapes", "LocalFragmentFactory")
            .DescendantNodes()
            .OfType<LocalFunctionStatementSyntax>()
            .Single();
        var localFunctionSymbol = fixture.SemanticModel.GetDeclaredSymbol(localFunction);
        Assert.IsNotNull(localFunctionSymbol);
        Assert.IsFalse(InvokeEmitterInstance<bool>(
            emitter,
            "TryGetReturnedRenderFragmentBody",
            new object?[] { localFunctionSymbol!, null }));
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
        AssertReturnedRenderFragmentBody(emitter, GetMethodSymbol(fixture, "EmitterHost", "BlockFactory"));
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
        AssertConstructorMap(emitter, GetNamedType(fixture, "NoisyCarrier"), expected: true);
        AssertConstructorMap(emitter, GetNamedType(fixture, "UnmappedCarrier"), expected: false);
        AssertConstructorMap(emitter, GetNamedType(fixture, "ExpressionCarrier"), expected: false);
        AssertConstructorMap(emitter, host.GetTypeMembers("RecordCarrier").Single(), expected: false);
    }

    [TestMethod]
    public void EmitterPrivateHelpers_ResolveBuilderFragmentsAndRenderObjectProvenance()
    {
        var fixture = CreateFixture();
        var host = GetNamedType(fixture, "EmitterHost");
        var emitter = CreateEmitter(fixture, host);
        var helperMethod = GetMethod(fixture, "EmitterHost", "HelperInvocationShapes");
        var helperSymbol = GetMethodSymbol(fixture, "EmitterHost", "HelperInvocationShapes");
        var context = CreateEmitContext(helperSymbol.Parameters[0]);

        var builderContent = GetInvocation(fixture, helperMethod, "AddContent", ordinal: 0);
        var otherContent = GetInvocation(fixture, helperMethod, "AddContent", ordinal: 1);
        var instanceHelper = GetInvocation(fixture, helperMethod, "InstanceBuilderHelper");
        var staticHelper = GetInvocation(fixture, helperMethod, "StaticBuilderHelper");
        var localHelper = GetInvocation(fixture, helperMethod, "LocalBuilderHelper");
        var externalHelper = GetInvocation(fixture, helperMethod, "Write");
        var foreignInstanceHelper = GetInvocation(fixture, helperMethod, "Write", ordinal: 1);
        var expressionHelper = GetInvocation(fixture, helperMethod, "ExpressionBuilderHelper");
        var mismatchedHelper = GetInvocation(fixture, helperMethod, "InstanceBuilderHelper", ordinal: 1);
        var nonBuilderHelper = GetInvocation(fixture, helperMethod, "NonBuilderHelper");
        var noArgumentHelper = GetInvocation(fixture, helperMethod, "StaticNoBuilder");

        AssertRenderTreeBuilderReceiver(emitter, builderContent, context, expected: true);
        AssertRenderTreeBuilderReceiver(emitter, otherContent, context, expected: false);
        AssertRenderTreeBuilderReceiver(emitter, instanceHelper, context, expected: false);
        AssertRenderTreeBuilderReceiver(emitter, foreignInstanceHelper, context, expected: false);
        AssertRenderTreeBuilderReceiver(emitter, staticHelper, context, expected: true);
        AssertRenderTreeBuilderReceiver(emitter, noArgumentHelper, context, expected: false);
        Assert.AreEqual(
            0,
            InvokeEmitter<int>("GetRenderTreeBuilderReceiverArgumentOffset", noArgumentHelper));

        var state = CreateRenderState();
        Assert.IsTrue(InvokeEmitterInstance<bool>(emitter, "TryEmitHelperInvocation", instanceHelper, context, state));
        Assert.IsTrue(InvokeEmitterInstance<bool>(emitter, "TryEmitHelperInvocation", staticHelper, context, state));
        Assert.IsTrue(InvokeEmitterInstance<bool>(emitter, "TryEmitHelperInvocation", externalHelper, context, state));
        Assert.IsFalse(InvokeEmitterInstance<bool>(emitter, "TryEmitHelperInvocation", foreignInstanceHelper, context, state));
        Assert.IsTrue(InvokeEmitterInstance<bool>(emitter, "TryEmitHelperInvocation", expressionHelper, context, state));
        Assert.IsFalse(InvokeEmitterInstance<bool>(emitter, "TryEmitHelperInvocation", localHelper, context, state));
        Assert.IsFalse(InvokeEmitterInstance<bool>(emitter, "TryEmitHelperInvocation", mismatchedHelper, context, state));
        Assert.IsFalse(InvokeEmitterInstance<bool>(emitter, "TryEmitHelperInvocation", nonBuilderHelper, context, state));
        Assert.IsFalse(InvokeEmitterInstance<bool>(emitter, "TryEmitHelperInvocation", noArgumentHelper, context, state));

        var referenceMethod = GetMethod(fixture, "EmitterHost", "RenderFragmentReferences");
        var instanceMethodGroup = GetVariableInitializer(fixture, referenceMethod, "instance");
        var staticMethodGroup = GetVariableInitializer(fixture, referenceMethod, "staticGroup");
        var invalidMethodGroup = GetVariableInitializer(fixture, referenceMethod, "invalid");
        AssertResolvedRenderFragmentMethodReference(emitter, instanceMethodGroup, context, expected: true);
        AssertResolvedRenderFragmentMethodReference(emitter, staticMethodGroup, context, expected: true);
        AssertResolvedRenderFragmentMethodReference(emitter, invalidMethodGroup, context, expected: false);

        var objectMethod = GetMethod(fixture, "EmitterHost", "RenderObjectReferences");
        var blockObjectHelper = GetInvocation(fixture, objectMethod, "CreateCarrier");
        var expressionObjectHelper = GetInvocation(fixture, objectMethod, "CreateExpressionCarrier");
        var ignoredObjectHelper = GetInvocation(fixture, objectMethod, "CreateIgnoredCarrier");
        AssertResolvedRenderObjectHelper(emitter, blockObjectHelper, context, expected: true);
        AssertResolvedRenderObjectHelper(emitter, expressionObjectHelper, context, expected: true);
        AssertResolvedRenderObjectHelper(emitter, ignoredObjectHelper, context, expected: false);

        var blockCarrierBody = GetMethodBody(fixture, "EmitterHost", "CreateCarrier");
        var renderedObject = new object?[] { blockCarrierBody, context, null };
        Assert.IsTrue(InvokeEmitterInstance<bool>(emitter, "TryResolveReturnedRenderObject", renderedObject));
        Assert.IsNotNull(renderedObject[2]);

        var declaration = blockCarrierBody.Operations.OfType<IVariableDeclarationGroupOperation>().Single();
        var trackedContext = InvokeEmitterInstance<object>(emitter, "TrackRenderProvenanceDeclarationGroup", declaration, context);
        Assert.IsNotNull(trackedContext);

        var factoryMethod = GetMethodSymbol(fixture, "EmitterHost", "NamedFragmentFactory");
        var helperBody = new object?[] { factoryMethod, null };
        Assert.IsTrue(InvokeEmitterInstance<bool>(emitter, "TryGetReturnedRenderFragmentBody", helperBody));
        Assert.IsNotNull(helperBody[1]);
        var firstHelperFunction = InvokeEmitterInstance<object>(
            emitter,
            "EnsureRenderFragmentHelperFunction",
            factoryMethod,
            helperBody[1],
            context);
        var cachedHelperFunction = InvokeEmitterInstance<object>(
            emitter,
            "EnsureRenderFragmentHelperFunction",
            factoryMethod,
            helperBody[1],
            context);
        Assert.AreEqual(
            GetRecordProperty<string>(firstHelperFunction, "FunctionName"),
            GetRecordProperty<string>(cachedHelperFunction, "FunctionName"));

        var multiRootMethod = GetMethodSymbol(fixture, "EmitterHost", "MultiRootFragmentFactory");
        var multiRootBody = new object?[] { multiRootMethod, null };
        Assert.IsTrue(InvokeEmitterInstance<bool>(emitter, "TryGetReturnedRenderFragmentBody", multiRootBody));
        var multiRootFunction = InvokeEmitterInstance<object>(
            emitter,
            "EnsureRenderFragmentHelperFunction",
            multiRootMethod,
            multiRootBody[1],
            context);
        Assert.IsTrue(GetRecordProperty<bool>(multiRootFunction, "UsesFragment"));

        var markupMethod = GetMethodSymbol(fixture, "EmitterHost", "MarkupFragmentFactory");
        var markupBody = new object?[] { markupMethod, null };
        Assert.IsTrue(InvokeEmitterInstance<bool>(emitter, "TryGetReturnedRenderFragmentBody", markupBody));
        var markupFunction = InvokeEmitterInstance<object>(
            emitter,
            "EnsureRenderFragmentHelperFunction",
            markupMethod,
            markupBody[1],
            context);

        var trailingMethod = GetMethodSymbol(fixture, "EmitterHost", "ExpressionFactory");
        var trailingBody = new object?[] { trailingMethod, null };
        Assert.IsTrue(InvokeEmitterInstance<bool>(emitter, "TryGetReturnedRenderFragmentBody", trailingBody));
        var trailingFunction = InvokeEmitterInstance<object>(
            emitter,
            "EnsureRenderFragmentHelperFunction",
            trailingMethod,
            trailingBody[1],
            context);

        var genericContentMethod = GetMethod(fixture, "EmitterHost", "GenericContentInvocation");
        var genericContentSymbol = GetMethodSymbol(fixture, "EmitterHost", "GenericContentInvocation");
        var genericContentState = CreateRenderState();
        InvokeEmitterInstance<object?>(
            emitter,
            "EmitAddContent",
            GetInvocation(fixture, genericContentMethod, "AddContent"),
            CreateEmitContext(genericContentSymbol.Parameters[0]),
            genericContentState);
        InvokeEmitterInstance<object?>(
            emitter,
            "EmitAddContent",
            GetInvocation(fixture, genericContentMethod, "AddContent"),
            CreateEmitContext(genericContentSymbol.Parameters[0]),
            genericContentState);
        Assert.IsTrue(GetRecordProperty<bool>(genericContentState, "UsesFragment"));
    }

    [TestMethod]
    public void EmitterRoslynOperationEdges_LowerConditionalAndDeconstructedFramesExplicitly()
    {
        var fixture = CreateFixture();
        var host = GetNamedType(fixture, "EmitterHost");
        var emitter = CreateEmitter(fixture, host);
        var directRender = GetMethodSymbol(fixture, "EmitterHost", "DirectRenderEdgeShapes");
        var context = CreateEmitContext(directRender.Parameters[0]);

        Assert.IsNotNull(InvokeEmitterInstance<object>(
            emitter,
            "EmitOperation",
            GetMethodBody(fixture, "EmitterHost", "DirectRenderEdgeShapes"),
            context,
            CreateRenderState()));

        var runtimeLocal = GetMethodSymbol(fixture, "EmitterHost", "RuntimeLocalInsideOpenFrame");
        var runtimeLocalFailure = Assert.Throws<TargetInvocationException>(() =>
            InvokeEmitterInstance<object>(
                emitter,
                "EmitOperation",
                GetMethodBody(fixture, "EmitterHost", "RuntimeLocalInsideOpenFrame"),
                CreateEmitContext(runtimeLocal.Parameters[0]),
                CreateRenderState()));
        StringAssert.Contains(
            runtimeLocalFailure.InnerException!.Message,
            "Runtime local declarations",
            StringComparison.Ordinal);

        var inputs = GetMethod(fixture, "OperationShapes", "Inputs");
        var convertedLiteral = GetOperation<IConversionOperation>(
            fixture,
            GetVariableDeclarator(inputs, "convertedLiteral").Initializer!.Value);
        Assert.Throws<TargetInvocationException>(() =>
            InvokeEmitterInstance<object>(
                emitter,
                "EmitExpressionStatement",
                convertedLiteral,
                context,
                CreateRenderState()));
        Assert.Throws<TargetInvocationException>(() =>
            InvokeEmitterInstance<object>(
                emitter,
                "EmitOperation",
                GetVariableInitializer(fixture, inputs, "literal"),
                context,
                CreateRenderState()));

        var storageReferences = GetMethod(fixture, "EmitterHost", "StorageReferences");
        var ownStorage = new object?[] { GetVariableInitializer(fixture, storageReferences, "own"), null };
        var staticStorage = new object?[] { GetVariableInitializer(fixture, storageReferences, "staticValue"), null };
        Assert.IsTrue(InvokeEmitterInstance<bool>(emitter, "TryGetCurrentComponentStorageMember", ownStorage));
        Assert.IsFalse(InvokeEmitterInstance<bool>(emitter, "TryGetCurrentComponentStorageMember", staticStorage));
        var fieldStorage = new object?[] { GetVariableInitializer(fixture, storageReferences, "field"), null };
        Assert.IsTrue(InvokeEmitterInstance<bool>(emitter, "TryGetCurrentComponentStorageMember", fieldStorage));

        var inputsSymbol = GetMethodSymbol(fixture, "OperationShapes", "Inputs");
        var parameter = inputsSymbol.Parameters.Single(parameter => parameter.Name == "parameter");
        var parameterReference = GetMethodBody(fixture, "OperationShapes", "Inputs")
            .Descendants()
            .OfType<IParameterReferenceOperation>()
            .First(reference => SymbolEqualityComparer.Default.Equals(reference.Parameter, parameter));
        var literal = GetVariableInitializer(fixture, inputs, "literal");
        var substitutionContext = CreateEmitContext(
            runtimeLocal.Parameters[0],
            substitutions: ImmutableDictionary<IParameterSymbol, IOperation>.Empty
                .WithComparers(SymbolEqualityComparer.Default)
                .Add(parameter, literal));
        StringAssert.Contains(
            InvokeEmitterInstance<Expression>(emitter, "LowerExpression", parameterReference, substitutionContext).ToKnRECMAScript(),
            "constant",
            StringComparison.Ordinal);
        var aliasContext = CreateEmitContext(
            runtimeLocal.Parameters[0],
            parameterAliases: ImmutableDictionary<IParameterSymbol, string>.Empty
                .WithComparers(SymbolEqualityComparer.Default)
                .Add(parameter, "parameterAlias"));
        Assert.AreEqual(
            "parameterAlias",
            InvokeEmitterInstance<Expression>(emitter, "LowerExpression", parameterReference, aliasContext).ToKnRECMAScript());
    }

    [TestMethod]
    public void EmitterResidualOperationContracts_KeepSupportedLoopAndUnsupportedMetadataBranchesExplicit()
    {
        var fixture = CreateFixture();
        var host = GetNamedType(fixture, "EmitterHost");
        var emitter = CreateEmitter(fixture, host);
        var metadataMethod = GetMethodSymbol(fixture, "EmitterHost", "MetadataInvocation");
        var metadataContext = CreateEmitContext(metadataMethod.Parameters[0]);

        var metadataInvocation = GetInvocation(fixture, GetMethod(fixture, "EmitterHost", "MetadataInvocation"), "AddEventPreventDefaultAttribute");
        AssertRenderTreeBuilderReceiver(emitter, metadataInvocation, metadataContext, expected: true);

        var booleanAttributes = GetMethodSymbol(fixture, "EmitterHost", "BooleanAndConditionalAttributes");
        var booleanAttributesContext = CreateEmitContext(booleanAttributes.Parameters[0]);

        Assert.IsNotNull(InvokeEmitterInstance<object>(
            emitter,
            "EmitOperation",
            GetMethodBody(fixture, "EmitterHost", "BooleanAndConditionalAttributes"),
            booleanAttributesContext,
            CreateRenderState()));

        var runtimeLocal = GetMethodSymbol(fixture, "EmitterHost", "RuntimeLocalOutsideFrame");
        var noPrelude = CreateEmitContext(runtimeLocal.Parameters[0], allowPreludeDeclarations: false);
        var runtimeLocalFailure = Assert.Throws<TargetInvocationException>(() =>
            InvokeEmitterInstance<object>(
                emitter,
                "EmitOperation",
                GetMethodBody(fixture, "EmitterHost", "RuntimeLocalOutsideFrame"),
                noPrelude,
                CreateRenderState()));
        StringAssert.Contains(runtimeLocalFailure.InnerException!.Message, "Runtime local declarations", StringComparison.Ordinal);

        var noArgumentHelper = GetInvocation(fixture, GetMethod(fixture, "EmitterHost", "HelperInvocationShapes"), "StaticNoBuilder");
        Assert.Throws<TargetInvocationException>(() =>
            InvokeEmitterInstance<object?>(
                emitter,
                "EmitExpressionStatement",
                noArgumentHelper,
                noPrelude,
                CreateRenderState()));
        Assert.Throws<TargetInvocationException>(() =>
            InvokeEmitterInstance<object?>(
                emitter,
                "EmitAddEventModifier",
                noArgumentHelper,
                metadataContext,
                CreateRenderState(),
                true,
                false));

        var supportedLoop = GetMethodBody(fixture, "EmitterHost", "SupportedLoop")
            .Operations
            .OfType<IWhileLoopOperation>()
            .Single();
        Assert.IsNotNull(InvokeEmitterInstance<object>(
            emitter,
            "EmitOperation",
            supportedLoop,
            metadataContext,
            CreateRenderState()));

        var storageReferences = GetMethod(fixture, "EmitterHost", "StorageReferences");
        var foreignStorage = new object?[] { GetVariableInitializer(fixture, storageReferences, "foreign"), null };
        var nonMemberStorage = new object?[] { GetVariableInitializer(fixture, storageReferences, "scalar"), null };
        Assert.IsFalse(InvokeEmitterInstance<bool>(emitter, "TryGetCurrentComponentStorageMember", foreignStorage));
        Assert.IsFalse(InvokeEmitterInstance<bool>(emitter, "TryGetCurrentComponentStorageMember", nonMemberStorage));
    }

    [TestMethod]
    public void EmitterEdgePaths_KeepRecursiveAndUnsupportedRoslynShapesExplicit()
    {
        var fixture = CreateFixture();
        var host = GetNamedType(fixture, "EmitterHost");
        var emitter = CreateEmitter(fixture, host);
        var context = CreateEmitContext(GetMethodSymbol(fixture, "EmitterHost", "HelperInvocationShapes").Parameters[0]);

        var references = GetMethod(fixture, "EmitterHost", "RenderFragmentReferences");
        AssertResolvedRenderFragmentMethodReference(
            emitter,
            GetVariableInitializer(fixture, references, "converted"),
            context,
            expected: true);
        var recursiveReferenceArguments = new object?[]
        {
            GetVariableInitializer(fixture, references, "recursive"),
            context,
            null
        };
        var recursiveReferenceFailure = Assert.Throws<TargetInvocationException>(() =>
            InvokeEmitterInstance<bool>(emitter, "TryResolveRenderFragmentMethodReference", recursiveReferenceArguments));
        Assert.IsInstanceOfType<OperationTransformationException>(recursiveReferenceFailure.InnerException);

        var objectReferences = GetMethod(fixture, "EmitterHost", "RenderObjectReferences");
        var localObjectFactory = GetInvocation(fixture, objectReferences, "LocalObjectFactory");
        AssertResolvedRenderObjectHelper(emitter, localObjectFactory, context, expected: false);
        var recursiveObjectFactory = GetInvocation(fixture, objectReferences, "RecursiveObjectFactory");
        var recursiveObjectArguments = new object?[] { recursiveObjectFactory, context, null };
        var recursiveObjectFailure = Assert.Throws<TargetInvocationException>(() =>
            InvokeEmitterInstance<bool>(emitter, "TryResolveRenderObjectHelperInvocation", recursiveObjectArguments));
        Assert.IsInstanceOfType<OperationTransformationException>(recursiveObjectFailure.InnerException);

        var unexpectedObjectArguments = new object?[]
        {
            GetMethodBody(fixture, "EmitterHost", "UnexpectedObjectFactory"),
            context,
            null
        };
        Assert.IsFalse(InvokeEmitterInstance<bool>(emitter, "TryResolveReturnedRenderObject", unexpectedObjectArguments));
        var noReturnObjectArguments = new object?[]
        {
            GetMethodBody(fixture, "EmitterHost", "FactoryWithoutReturn"),
            context,
            null
        };
        Assert.IsFalse(InvokeEmitterInstance<bool>(emitter, "TryResolveReturnedRenderObject", noReturnObjectArguments));

        var convertedCarrier = GetVariableInitializer(fixture, objectReferences, "convertedCarrier");
        Assert.IsInstanceOfType<IConversionOperation>(convertedCarrier);
        var convertedCarrierArguments = new object?[] { convertedCarrier, context, null };
        Assert.IsTrue(InvokeEmitterInstance<bool>(emitter, "TryResolveReturnedRenderObject", convertedCarrierArguments));
        Assert.IsNotNull(convertedCarrierArguments[2]);

        var unclosedMethod = GetMethodSymbol(fixture, "EmitterHost", "UnclosedFragmentFactory");
        var unclosedBody = new object?[] { unclosedMethod, null };
        Assert.IsTrue(InvokeEmitterInstance<bool>(emitter, "TryGetReturnedRenderFragmentBody", unclosedBody));
        var unclosedFailure = Assert.Throws<TargetInvocationException>(() =>
            InvokeEmitterInstance<object>(emitter, "EnsureRenderFragmentHelperFunction", unclosedMethod, unclosedBody[1], context));
        Assert.IsInstanceOfType<OperationTransformationException>(unclosedFailure.InnerException);

        var slotConversion = GetVariableInitializer(fixture, GetMethod(fixture, "EmitterHost", "ComponentSlotReferences"), "boxedHeader");
        var slotArguments = new object?[] { slotConversion, null, null };
        Assert.IsTrue(InvokeEmitterInstance<bool>(emitter, "TryResolveComponentSlot", slotArguments));
        Assert.AreEqual("Header", slotArguments[1]);
        Assert.IsFalse((bool)slotArguments[2]!);

        var dynamicParameter = GetInvocation(fixture, GetMethod(fixture, "EmitterHost", "DynamicComponentParameter"), "AddComponentParameter");
        var dynamicState = CreateRenderState();
        PushFrame(dynamicState, CreateComponentFrame(ImmutableDictionary<string, string>.Empty));
        var dynamicParameterFailure = Assert.Throws<TargetInvocationException>(() =>
            InvokeEmitterInstance<object?>(emitter, "EmitAddComponentParameter", dynamicParameter, context, dynamicState));
        Assert.IsInstanceOfType<OperationTransformationException>(dynamicParameterFailure.InnerException);

        var invalidGenericContent = GetInvocation(fixture, GetMethod(fixture, "EmitterHost", "InvalidGenericContent"), "AddContent");
        var invalidContentFailure = Assert.Throws<TargetInvocationException>(() =>
            InvokeEmitterInstance<object?>(emitter, "EmitAddContent", invalidGenericContent, context, CreateRenderState()));
        Assert.IsInstanceOfType<OperationTransformationException>(invalidContentFailure.InnerException);

        var collectionCreation = GetOperation<IObjectCreationOperation>(
            fixture,
            GetMethod(fixture, "OperationShapes", "NonAttributeCollectionInitializer")
                .DescendantNodes()
                .OfType<ObjectCreationExpressionSyntax>()
                .Single());
        Assert.IsFalse(InvokeEmitterInstance<bool>(
            emitter,
            "TryEmitKnownMultipleAttributes",
            collectionCreation,
            context,
            CreateElementFrame()));

        var uninitializedDeclaration = GetMethodBody(fixture, "EmitterHost", "UninitializedFragmentLocal")
            .Operations
            .OfType<IVariableDeclarationGroupOperation>()
            .Single();
        Assert.IsNotNull(InvokeEmitterInstance<object>(emitter, "TrackRenderProvenanceDeclarationGroup", uninitializedDeclaration, context));
    }

    [TestMethod]
    public void EmitterFrameAndBuilderBindingHelpers_PreserveDirectRenderStateTransitions()
    {
        var fixture = CreateFixture();
        var bindingMethod = GetMethod(fixture, "OperationShapes", "BuilderBindings");
        var bindingMethodSymbol = GetMethodSymbol(fixture, "OperationShapes", "BuilderBindings");
        var builderParameter = bindingMethodSymbol.Parameters.Single(parameter => parameter.Name == "builder");
        var otherParameter = bindingMethodSymbol.Parameters.Single(parameter => parameter.Name == "other");
        var localDeclarator = GetVariableDeclarator(bindingMethod, "local");
        var localSymbol = GetOperation<IVariableDeclaratorOperation>(fixture, localDeclarator).Symbol;
        var builderReference = GetVariableInitializer(fixture, bindingMethod, "local");
        var otherReference = GetVariableInitializer(fixture, bindingMethod, "otherAlias");
        var convertedBuilderReference = GetVariableInitializer(fixture, bindingMethod, "converted");
        var localInvocation = bindingMethod.DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Single(invocation => invocation.Expression.ToString().StartsWith("local.", StringComparison.Ordinal));
        var localReference = GetOperation<IInvocationOperation>(fixture, localInvocation).Instance!;
        var substitutions = ImmutableDictionary<IParameterSymbol, IOperation>.Empty
            .WithComparers(SymbolEqualityComparer.Default);
        var builderBinding = CreateBuilderBinding(builderParameter);
        var localBinding = CreateBuilderBinding(localSymbol);

        Assert.IsTrue(InvokeNestedInstance<bool>(builderBinding, "Matches", builderReference, substitutions));
        Assert.IsFalse(InvokeNestedInstance<bool>(builderBinding, "Matches", otherReference, substitutions));
        Assert.IsTrue(InvokeNestedInstance<bool>(builderBinding, "Matches", convertedBuilderReference, substitutions));
        Assert.IsTrue(InvokeNestedInstance<bool>(
            builderBinding,
            "Matches",
            otherReference,
            substitutions.Add(otherParameter, builderReference)));
        Assert.IsTrue(InvokeNestedInstance<bool>(localBinding, "Matches", localReference, substitutions));
        Assert.IsFalse(InvokeNestedInstance<bool>(builderBinding, "Matches", localReference, substitutions));

        var elementFrame = CreateElementFrame();
        Assert.IsFalse(InvokeNestedInstance<bool>(elementFrame, "AddMultipleAttributes", new NullLiteral("null")));
        Assert.IsFalse(InvokeNestedInstance<bool>(elementFrame, "AddMultipleAttributes", new Identifier("undefined")));
        Assert.IsFalse(InvokeNestedInstance<bool>(elementFrame, "TrySetLastAttributeValue", new Identifier("missing")));

        InvokeNestedInstance<object?>(
            elementFrame,
            "AddAttribute",
            CreateDirectAttribute("onClick", new Identifier("handler")));
        Assert.IsTrue(InvokeNestedInstance<bool>(elementFrame, "TrySetLastAttributeValue", new Identifier("replacementHandler")));
        InvokeNestedInstance<object?>(elementFrame, "SetUpdatesAttributeName", "value");
        InvokeNestedInstance<object?>(elementFrame, "SetEventModifier", "onclick", new Identifier("first"), true, false);
        InvokeNestedInstance<object?>(elementFrame, "SetEventModifier", "onclick", new Identifier("second"), true, false);
        InvokeNestedInstance<object?>(elementFrame, "SetEventModifier", "onclick", new BooleanLiteral(true, "true"), true, false);
        InvokeNestedInstance<object?>(elementFrame, "SetEventModifier", "onclick", new BooleanLiteral(false, "false"), true, false);
        Assert.IsTrue(InvokeNestedInstance<bool>(elementFrame, "AddMultipleAttributes", new Identifier("attrs")));
        Assert.IsFalse(InvokeNestedInstance<bool>(elementFrame, "TrySetLastAttributeValue", new Identifier("afterMultiple")));
        InvokeNestedInstance<object?>(elementFrame, "AddReferenceCapture", new NullLiteral("null"));
        InvokeNestedInstance<object?>(elementFrame, "AddReferenceCapture", new Identifier("undefined"));
        InvokeNestedInstance<object?>(elementFrame, "AddReferenceCapture", new Identifier("captureFirst"));
        InvokeNestedInstance<object?>(elementFrame, "AddReferenceCapture", new Identifier("captureSecond"));

        // A malformed sequence can leave a stale last-name marker. The frame must
        // reject it after scanning rather than updating a different attribute.
        SetNestedPrivateField(elementFrame, "_lastAttributeName", "missingAttribute");
        Assert.IsFalse(InvokeNestedInstance<bool>(elementFrame, "TrySetLastAttributeValue", new Identifier("missingReplacement")));

        var rendered = InvokeNestedInstance<Expression>(elementFrame, "ToRenderExpression").ToKnRECMAScript();
        StringAssert.Contains(rendered, "mergeProps", StringComparison.Ordinal);
        StringAssert.Contains(rendered, "onClick", StringComparison.Ordinal);
        StringAssert.Contains(rendered, "captureFirst", StringComparison.Ordinal);
        StringAssert.Contains(rendered, "captureSecond", StringComparison.Ordinal);

        var conditionalFrame = CreateElementFrame();
        InvokeNestedInstance<object?>(
            conditionalFrame,
            "AddAttribute",
            CreateDirectAttribute("class", new StringLiteral("base", "\"base\"")));
        InvokeNestedInstance<object?>(
            conditionalFrame,
            "AddConditionalAttributes",
            new Identifier("enabled"),
            CreateDirectAttributeArray(CreateDirectAttribute("checked", new BooleanLiteral(true, "true"))),
            CreateDirectAttributeArray(CreateDirectAttribute("readonly", new BooleanLiteral(true, "true"))));
        var conditionalRendered = InvokeNestedInstance<Expression>(conditionalFrame, "ToRenderExpression").ToKnRECMAScript();
        StringAssert.Contains(conditionalRendered, "enabled", StringComparison.Ordinal);
        StringAssert.Contains(conditionalRendered, "checked", StringComparison.Ordinal);
        StringAssert.Contains(conditionalRendered, "readonly", StringComparison.Ordinal);
        StringAssert.Contains(conditionalRendered, "mergeProps", StringComparison.Ordinal);
    }

    [TestMethod]
    public void PropFramePatchMetadata_PreservesElementComponentAndFullPropsUpdateContracts()
    {
        var staticFrame = CreateElementFrame();
        InvokeNestedInstance<object?>(
            staticFrame,
            "AddAttribute",
            CreateDirectAttribute("title", new StringLiteral("static", "\"static\"")));
        var staticPatch = InvokeNestedInstance<object>(staticFrame, "BuildPatchMetadata", false, false, 0);
        Assert.IsFalse(GetRecordProperty<bool>(staticPatch, "RequiresBlock"));
        Assert.AreEqual(0, GetRecordProperty<int>(staticPatch, "Flag"));
        Assert.IsNull(GetRecordProperty<object?>(staticPatch, "DynamicProps"));

        var elementFrame = CreateElementFrame();
        InvokeNestedInstance<object?>(
            elementFrame,
            "AddAttribute",
            CreateDirectAttribute("class", new Identifier("className")));
        InvokeNestedInstance<object?>(
            elementFrame,
            "AddAttribute",
            CreateDirectAttribute("style", new Identifier("styleValue")));
        InvokeNestedInstance<object?>(
            elementFrame,
            "AddAttribute",
            CreateDirectAttribute("id", new Identifier("idValue")));
        var elementPatch = InvokeNestedInstance<object>(elementFrame, "BuildPatchMetadata", false, false, 0);
        Assert.IsTrue(GetRecordProperty<bool>(elementPatch, "RequiresBlock"));
        var elementDynamicProps = Assert.IsInstanceOfType<ImmutableArray<string>>(
            GetRecordProperty<object?>(elementPatch, "DynamicProps"));
        CollectionAssert.AreEqual(new[] { "id" }, elementDynamicProps.ToArray());

        var componentFrame = CreateElementFrame();
        InvokeNestedInstance<object?>(
            componentFrame,
            "AddAttribute",
            CreateDirectAttribute("class", new Identifier("className")));
        InvokeNestedInstance<object?>(
            componentFrame,
            "AddAttribute",
            CreateDirectAttribute("style", new Identifier("styleValue")));
        var componentPatch = InvokeNestedInstance<object>(componentFrame, "BuildPatchMetadata", false, true, 0);
        var componentDynamicProps = Assert.IsInstanceOfType<ImmutableArray<string>>(
            GetRecordProperty<object?>(componentPatch, "DynamicProps"));
        CollectionAssert.AreEqual(new[] { "class", "style" }, componentDynamicProps.ToArray());

        var keyedFrame = CreateElementFrame();
        InvokeNestedInstance<object?>(
            keyedFrame,
            "AddAttribute",
            CreateDirectAttribute("key", new Identifier("itemKey")));
        var keyedPatch = InvokeNestedInstance<object>(keyedFrame, "BuildPatchMetadata", false, false, 0);
        Assert.IsTrue(GetRecordProperty<bool>(keyedPatch, "RequiresBlock"));
        Assert.IsNull(GetRecordProperty<object?>(keyedPatch, "DynamicProps"));

        var splatAndRefFrame = CreateElementFrame();
        Assert.IsTrue(InvokeNestedInstance<bool>(splatAndRefFrame, "AddMultipleAttributes", new Identifier("attrs")));
        InvokeNestedInstance<object?>(splatAndRefFrame, "AddReferenceCapture", new Identifier("capture"));
        var splatAndRefPatch = InvokeNestedInstance<object>(splatAndRefFrame, "BuildPatchMetadata", false, false, 0);
        Assert.IsTrue(GetRecordProperty<bool>(splatAndRefPatch, "RequiresBlock"));
        Assert.IsNull(GetRecordProperty<object?>(splatAndRefPatch, "DynamicProps"));
    }

    [TestMethod]
    public void PropFrame_SeparatesRenderListEligibilityFromEmptyAndReferenceProps()
    {
        var bareFrame = CreateElementFrame();
        Assert.IsTrue(InvokeNestedInstance<bool>(bareFrame, "get_CanUseRenderList"));
        InvokeNestedInstance<object?>(
            bareFrame,
            "AddConditionalAttributes",
            new Identifier("enabled"),
            CreateDirectAttributeArray(),
            CreateDirectAttributeArray());
        StringAssert.Contains(
            InvokeNestedInstance<Expression>(bareFrame, "ToRenderExpression").ToKnRECMAScript(),
            "h(\"div\", null, null)",
            StringComparison.Ordinal);

        var eventFrame = CreateElementFrame();
        InvokeNestedInstance<object?>(
            eventFrame,
            "AddAttribute",
            CreateDirectAttribute("onClick", new Identifier("handler")));
        Assert.IsFalse(InvokeNestedInstance<bool>(eventFrame, "get_CanUseRenderList"));

        var referenceFrame = CreateElementFrame();
        InvokeNestedInstance<object?>(referenceFrame, "AddReferenceCapture", new Identifier("capture"));
        Assert.IsFalse(InvokeNestedInstance<bool>(referenceFrame, "get_CanUseRenderList"));
        StringAssert.Contains(
            InvokeNestedInstance<Expression>(referenceFrame, "ToRenderExpression").ToKnRECMAScript(),
            "ref: capture",
            StringComparison.Ordinal);
    }

    [TestMethod]
    public void EmitterMemberAndComponentFrameHelpers_ResolveBoundaryShapesDeterministically()
    {
        var fixture = CreateFixture();
        var host = GetNamedType(fixture, "EmitterHost");
        var emitter = CreateEmitter(fixture, host);
        var indexer = host.GetMembers().OfType<IPropertySymbol>().Single(static property => property.IsIndexer);

        AssertReturnedPropertyValue(emitter, GetProperty(host, "AccessorExpressionFragment"));
        Assert.IsFalse(InvokeEmitterInstance<bool>(
            emitter,
            "TryGetReturnedPropertyValue",
            new object?[] { indexer, null }));
        AssertReturnedRenderFragmentBody(emitter, GetMethodSymbol(fixture, "EmitterHost", "GenericExpressionFactory"));
        AssertReturnedRenderFragmentBody(emitter, GetMethodSymbol(fixture, "EmitterHost", "GenericBlockLiteralFactory"));

        var parameterNames = ImmutableDictionary<string, string>.Empty
            .Add("TitleContent", "title");
        var componentFrame = CreateComponentFrame(parameterNames);
        Assert.AreEqual("title", InvokeNestedInstance<string>(componentFrame, "NormalizeSlotName", "TitleContent"));
        Assert.AreEqual("ChildContent", InvokeNestedInstance<string>(componentFrame, "NormalizeSlotName", "ChildContent"));
        Assert.AreEqual("FooterContent", InvokeNestedInstance<string>(componentFrame, "NormalizeSlotName", "FooterContent"));
        Assert.AreEqual("Title", InvokeNestedInstance<string>(componentFrame, "NormalizeAttributeName", "Title"));
        StringAssert.Contains(
            InvokeNestedInstance<Expression>(componentFrame, "ToRenderExpression").ToKnRECMAScript(),
            "h(component, null)",
            StringComparison.Ordinal);

        AddFrameChild(componentFrame, new Identifier("first"));
        StringAssert.Contains(
            InvokeNestedInstance<Expression>(componentFrame, "ToRenderExpression").ToKnRECMAScript(),
            "first",
            StringComparison.Ordinal);
        AddFrameChild(componentFrame, new Identifier("second"));
        StringAssert.Contains(
            InvokeNestedInstance<Expression>(componentFrame, "ToRenderExpression").ToKnRECMAScript(),
            "second",
            StringComparison.Ordinal);

        var slotFrame = CreateComponentFrame(parameterNames);
        AddComponentSlot(slotFrame, "header", CreateDirectRenderFragment(new Identifier("headerContent"), parameterName: null));
        var stableSlots = InvokeNestedInstance<Expression>(slotFrame, "ToRenderExpression").ToKnRECMAScript();
        StringAssert.Contains(stableSlots, "withCtx", StringComparison.Ordinal);
        StringAssert.Contains(stableSlots, "_: 1", StringComparison.Ordinal);
        StringAssert.Contains(stableSlots, "createBlock", StringComparison.Ordinal);
        Assert.IsFalse(stableSlots.Contains("createSlots", StringComparison.Ordinal), stableSlots);

        var withCtxCalls = 0;
        var createSlotsCalls = 0;
        var unstableSlotFrame = CreateConfiguredComponentFrame(
            ImmutableDictionary<string, string>.Empty,
            useWithCtx: () => withCtxCalls++,
            useCreateSlots: () => createSlotsCalls++,
            slotsAreInStableScope: false);
        AddComponentSlot(
            unstableSlotFrame,
            "header",
            CreateDirectRenderFragment(new Identifier("headerContent"), parameterName: null));
        var unstableSlots = InvokeNestedInstance<Expression>(unstableSlotFrame, "ToRenderExpression").ToKnRECMAScript();
        StringAssert.Contains(unstableSlots, "createSlots", StringComparison.Ordinal);
        Assert.AreEqual(1, withCtxCalls);
        Assert.AreEqual(1, createSlotsCalls);
    }

    [TestMethod]
    public void EmitterResidualSymbolAndFrameBranches_KeepAlternateBoundShapesExplicit()
    {
        var fixture = CreateFixture();
        var host = GetNamedType(fixture, "EmitterHost");
        var emitter = CreateEmitter(fixture, host);
        var inputs = GetMethod(fixture, "OperationShapes", "Inputs");
        var foreignGenericFragment = GetVariableInitializer(fixture, inputs, "foreignGenericFragment");

        Assert.IsFalse(Invoke<bool>("IsGenericRenderFragmentOperationValue", foreignGenericFragment));
        Assert.IsFalse(Invoke<bool>("IsGenericRenderFragmentType", foreignGenericFragment.Type));

        var openComponentShapes = GetMethod(fixture, "EmitterHost", "OpenComponentShapes");
        var openComponentSymbol = GetMethodSymbol(fixture, "EmitterHost", "OpenComponentShapes");
        var openComponentContext = CreateEmitContext(openComponentSymbol.Parameters[0]);
        var genericOpenComponent = GetInvocation(fixture, openComponentShapes, "OpenComponent", ordinal: 0);
        var typeOpenComponent = GetInvocation(fixture, openComponentShapes, "OpenComponent", ordinal: 1);
        Assert.AreEqual(
            "ModuleComponent",
            Invoke<INamedTypeSymbol>("ResolveOpenComponentType", genericOpenComponent, openComponentContext).Name);
        Assert.AreEqual(
            "LibraryComponent",
            Invoke<INamedTypeSymbol>("ResolveOpenComponentType", typeOpenComponent, openComponentContext).Name);
        var genericTypeParameterOpenComponent = GetInvocation(
            fixture,
            GetMethod(fixture, "EmitterHost", "GenericOpenComponentShape"),
            "OpenComponent");
        Assert.Throws<TargetInvocationException>(() =>
            Invoke<INamedTypeSymbol>("ResolveOpenComponentType", genericTypeParameterOpenComponent, openComponentContext));

        var builderBindings = GetMethod(fixture, "OperationShapes", "BuilderBindings");
        var builderBindingsSymbol = GetMethodSymbol(fixture, "OperationShapes", "BuilderBindings");
        var builderParameter = builderBindingsSymbol.Parameters.Single(parameter => parameter.Name == "builder");
        var localSymbol = GetOperation<IVariableDeclaratorOperation>(
            fixture,
            GetVariableDeclarator(builderBindings, "local")).Symbol;
        var localInvocation = builderBindings.DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Single(invocation => invocation.Expression.ToString().StartsWith("local.", StringComparison.Ordinal));
        var secondaryBuilderContext = CreateEmitContext(
            builderParameter,
            secondaryBuilders: ImmutableHashSet<ILocalSymbol>.Empty
                .WithComparer(SymbolEqualityComparer.Default)
                .Add(localSymbol));
        Assert.IsTrue(Invoke<bool>(
            "IsSecondaryBuilderInvocation",
            GetOperation<IInvocationOperation>(fixture, localInvocation),
            secondaryBuilderContext));
        Assert.IsFalse(InvokeNestedInstance<bool>(
            CreateBuilderBinding(builderParameter),
            "Matches",
            GetVariableInitializer(fixture, inputs, "literal"),
            ImmutableDictionary<IParameterSymbol, IOperation>.Empty.WithComparers(SymbolEqualityComparer.Default)));

        var noEventFrame = CreateElementFrame();
        InvokeNestedInstance<object?>(noEventFrame, "SetUpdatesAttributeName", "value");
        StringAssert.Contains(
            InvokeNestedInstance<Expression>(noEventFrame, "ToRenderExpression").ToKnRECMAScript(),
            "h(\"div\"",
            StringComparison.Ordinal);

        var selectedSlotFrame = CreateComponentFrame(ImmutableDictionary<string, string>.Empty);
        var selectedWhenTrue = CreateDirectRenderFragment(new Identifier("whenTrue"), parameterName: null);
        var selectedWhenFalse = CreateDirectRenderFragment(new Identifier("whenFalse"), parameterName: null);
        var selection = CreateConditionalRenderFragmentSelection(
            new Identifier("showHeader"),
            selectedWhenTrue,
            selectedWhenFalse);
        AddComponentSlot(
            selectedSlotFrame,
            "header",
            CreateDirectRenderFragment(new Identifier("fallback"), parameterName: null, selection: selection));
        StringAssert.Contains(
            InvokeNestedInstance<Expression>(selectedSlotFrame, "ToRenderExpression").ToKnRECMAScript(),
            "createSlots",
            StringComparison.Ordinal);

        var unavailableSlotFrame = CreateComponentFrame(ImmutableDictionary<string, string>.Empty);
        AddComponentSlot(
            unavailableSlotFrame,
            "unavailable",
            CreateDirectRenderFragment(
                new Identifier("unavailableContent"),
                parameterName: null,
                availabilityCondition: new BooleanLiteral(false, "false")));
        var unavailableSlots = InvokeNestedInstance<Expression>(unavailableSlotFrame, "ToRenderExpression").ToKnRECMAScript();
        Assert.IsFalse(unavailableSlots.Contains("unavailable", StringComparison.Ordinal));

        var availableSlotFrame = CreateComponentFrame(ImmutableDictionary<string, string>.Empty);
        AddComponentSlot(
            availableSlotFrame,
            "available",
            CreateDirectRenderFragment(
                new Identifier("availableContent"),
                parameterName: null,
                availabilityCondition: new BooleanLiteral(true, "true")));
        StringAssert.Contains(
            InvokeNestedInstance<Expression>(availableSlotFrame, "ToRenderExpression").ToKnRECMAScript(),
            "available",
            StringComparison.Ordinal);

        var methodReferences = GetMethod(fixture, "EmitterHost", "AdditionalRenderFragmentReferences");
        var methodReferenceContext = CreateEmitContext(
            GetMethodSymbol(fixture, "EmitterHost", "AdditionalRenderFragmentReferences").Parameters[0]);
        AssertResolvedRenderFragmentMethodReference(
            emitter,
            GetVariableInitializer(fixture, methodReferences, "expression"),
            methodReferenceContext,
            expected: true);
        AssertResolvedRenderFragmentMethodReference(
            emitter,
            GetVariableInitializer(fixture, methodReferences, "local"),
            methodReferenceContext,
            expected: false);

        var propertyReferences = GetMethod(fixture, "EmitterHost", "AdditionalPropertyReferences");
        var staticPropertyReference = GetVariableInitializer(fixture, propertyReferences, "staticHeader");
        var staticPropertyArguments = new object?[] { staticPropertyReference, methodReferenceContext, null };
        Assert.IsTrue(InvokeEmitterInstance<bool>(emitter, "TryResolveRenderFragmentExpression", staticPropertyArguments));
        Assert.IsNotNull(staticPropertyArguments[2]);
        var foreignPropertyArguments = new object?[]
        {
            GetVariableInitializer(fixture, propertyReferences, "foreignHeader"),
            methodReferenceContext,
            null
        };
        Assert.IsFalse(InvokeEmitterInstance<bool>(emitter, "TryResolveRenderFragmentExpression", foreignPropertyArguments));
        var foreignInstancePropertyArguments = new object?[]
        {
            GetVariableInitializer(fixture, propertyReferences, "foreignInstanceHeader"),
            methodReferenceContext,
            null
        };
        Assert.IsFalse(InvokeEmitterInstance<bool>(emitter, "TryResolveRenderFragmentExpression", foreignInstancePropertyArguments));

        var provenance = GetMethodBody(fixture, "EmitterHost", "ProvenanceObject")
            .Operations
            .OfType<IVariableDeclarationGroupOperation>()
            .Single();
        Assert.IsNotNull(InvokeEmitterInstance<object>(
            emitter,
            "TrackRenderProvenanceDeclarationGroup",
            provenance,
            methodReferenceContext));

        var helperFactory = GetInvocation(
            fixture,
            GetMethod(fixture, "EmitterHost", "HelperFactoryReferences"),
            "MultiRootFragmentFactory");
        var helperArguments = new object?[] { helperFactory, methodReferenceContext, null };
        Assert.IsTrue(InvokeEmitterInstance<bool>(emitter, "TryResolveRenderFragmentHelperInvocation", helperArguments));
        Assert.IsNotNull(helperArguments[2]);
        var namedHelperFactory = GetInvocation(
            fixture,
            GetMethod(fixture, "EmitterHost", "HelperFactoryReferences"),
            "NamedFragmentFactory");
        var namedHelperArguments = new object?[] { namedHelperFactory, methodReferenceContext, null };
        Assert.IsTrue(InvokeEmitterInstance<bool>(emitter, "TryResolveRenderFragmentHelperInvocation", namedHelperArguments));
        Assert.IsNotNull(namedHelperArguments[2]);

        var loopHelperFactory = GetInvocation(
            fixture,
            GetMethod(fixture, "EmitterHost", "HelperFactoryReferences"),
            "LoopFragmentFactory");
        var loopHelperArguments = new object?[] { loopHelperFactory, methodReferenceContext, null };
        Assert.IsTrue(InvokeEmitterInstance<bool>(emitter, "TryResolveRenderFragmentHelperInvocation", loopHelperArguments));
        Assert.IsTrue(GetRecordProperty<bool>(loopHelperArguments[2]!, "UsesFragment"));
        var loopHelperMethod = GetMethodSymbol(fixture, "EmitterHost", "LoopFragmentFactory");
        var loopHelperBody = new object?[] { loopHelperMethod, null };
        Assert.IsTrue(InvokeEmitterInstance<bool>(emitter, "TryGetReturnedRenderFragmentBody", loopHelperBody));
        var loopHelperFunction = InvokeEmitterInstance<object>(
            emitter,
            "EnsureRenderFragmentHelperFunction",
            loopHelperMethod,
            loopHelperBody[1],
            methodReferenceContext);
        Assert.IsTrue(GetRecordProperty<bool>(loopHelperFunction, "UsesFragment"));

        var externMethodGroup = GetVariableInitializer(fixture, GetMethod(fixture, "EmitterHost", "RenderFragmentReferences"), "externGroup");
        AssertResolvedRenderFragmentMethodReference(emitter, externMethodGroup, methodReferenceContext, expected: false);

        var nativeCarrier = GetInvocation(fixture, GetMethod(fixture, "EmitterHost", "RenderObjectReferences"), "NativeCarrier");
        AssertResolvedRenderObjectHelper(emitter, nativeCarrier, methodReferenceContext, expected: false);

        Assert.IsFalse(InvokeEmitter<bool>(
            "TryGetRenderFragmentFactoryReturn",
            new object?[] { GetMethodBody(fixture, "EmitterHost", "UninitializedFragmentLocal"), null, null }));

        var attributeInvocation = GetInvocation(fixture, GetMethod(fixture, "OperationShapes", "AttributeInvocations"), "AddAttribute");
        Assert.Throws<TargetInvocationException>(() =>
            InvokeEmitterInstance<object?>(emitter, "EmitAddAttribute", attributeInvocation, openComponentContext, CreateRenderState()));
        var attributeContext = CreateEmitContext(
            GetMethodSymbol(fixture, "OperationShapes", "AttributeInvocations").Parameters[0]);
        var attributeState = CreateRenderState();
        var attributeFrame = CreateElementFrame();
        PushFrame(attributeState, attributeFrame);
        InvokeEmitterInstance<object?>(emitter, "EmitAddAttribute", attributeInvocation, attributeContext, attributeState);
        InvokeNestedInstance<object?>(attributeState, "AddChild", new Identifier("child"));
        Assert.Throws<TargetInvocationException>(() =>
            InvokeEmitterInstance<object?>(emitter, "EmitAddAttribute", attributeInvocation, attributeContext, attributeState));

        var invalidAttribute = GetInvocation(fixture, GetMethod(fixture, "EmitterHost", "InvalidAddAttribute"), "AddAttribute");
        Assert.Throws<TargetInvocationException>(() =>
            InvokeEmitterInstance<object?>(emitter, "EmitAddAttribute", invalidAttribute, openComponentContext, CreateRenderState()));
        var invalidConditionalAttribute = Assert.Throws<TargetInvocationException>(() =>
            InvokeEmitterInstance<object>(
                emitter,
                "BuildConditionalAttributes",
                ImmutableArray.Create(invalidAttribute),
                openComponentContext,
                CreateElementFrame()));
        Assert.IsInstanceOfType<OperationTransformationException>(invalidConditionalAttribute.InnerException);

        var twoArgumentModifier = GetInvocation(fixture, GetMethod(fixture, "EmitterHost", "TwoArgumentEventModifier"), "AddEventPreventDefaultAttribute");
        var modifierState = CreateRenderState();
        PushFrame(modifierState, CreateElementFrame());
        InvokeEmitterInstance<object?>(
            emitter,
            "EmitAddEventModifier",
            twoArgumentModifier,
            openComponentContext,
            modifierState,
            true,
            false);

        var dynamicConditional = GetMethodSymbol(fixture, "EmitterHost", "DynamicConditionalAttributes");
        Assert.Throws<TargetInvocationException>(() =>
            InvokeEmitterInstance<object>(
                emitter,
                "EmitOperation",
                GetMethodBody(fixture, "EmitterHost", "DynamicConditionalAttributes"),
                CreateEmitContext(dynamicConditional.Parameters[0]),
                CreateRenderState()));

        var lateAttribute = GetMethodSymbol(fixture, "EmitterHost", "AttributeAfterChild");
        var lateAttributeFailure = Assert.Throws<TargetInvocationException>(() =>
            InvokeEmitterInstance<object>(
                emitter,
                "EmitOperation",
                GetMethodBody(fixture, "EmitterHost", "AttributeAfterChild"),
                CreateEmitContext(lateAttribute.Parameters[0]),
                CreateRenderState()));
        StringAssert.Contains(
            lateAttributeFailure.InnerException!.Message,
            "Attributes must be added before children",
            StringComparison.Ordinal);
    }

    [TestMethod]
    public void InstrumentedFrames_ApplyHoistHandlerCacheBindAndRegionProtocols()
    {
        var hoistCount = 0;
        var cacheCount = 0;
        var blockCount = 0;
        var textVNodeCount = 0;
        var frame = CreateConfiguredElementFrame(
            _ =>
            {
                hoistCount++;
                return new Identifier("hoistedProps");
            },
            _ => true,
            handler =>
            {
                cacheCount++;
                return new Identifier("cachedHandler");
            },
            static handler => handler is Identifier { Name: "stableHandler" },
            static handler => handler is Identifier { Name: "stableHandler" },
            () => blockCount++,
            () => textVNodeCount++);
        InvokeNestedInstance<object?>(
            frame,
            "AddAttribute",
            CreateDirectAttribute("title", new StringLiteral("fixed", "\"fixed\"")));
        var hoisted = InvokeNestedInstance<Expression>(frame, "ToRenderExpression").ToKnRECMAScript();
        StringAssert.Contains(hoisted, "hoistedProps", StringComparison.Ordinal);
        Assert.AreEqual(1, hoistCount);

        var cachedFrame = CreateConfiguredElementFrame(
            static value => value,
            static _ => false,
            handler =>
            {
                cacheCount++;
                return new Identifier("cachedHandler");
            },
            static handler => handler is Identifier { Name: "stableHandler" },
            static handler => handler is Identifier { Name: "stableHandler" },
            () => blockCount++,
            () => textVNodeCount++);
        InvokeNestedInstance<object?>(
            cachedFrame,
            "AddAttribute",
            CreateDirectAttribute("onClick", new Identifier("stableHandler")));
        InvokeNestedInstance<object?>(
            cachedFrame,
            "AddAttribute",
            CreateDirectAttribute("id", new Identifier("dynamicId")));
        var cached = InvokeNestedInstance<Expression>(cachedFrame, "ToRenderExpression").ToKnRECMAScript();
        StringAssert.Contains(cached, "cachedHandler", StringComparison.Ordinal);
        StringAssert.Contains(cached, "createElementBlock", StringComparison.Ordinal);
        Assert.AreEqual(1, cacheCount);
        Assert.AreEqual(1, blockCount);

        var bindFrame = CreateConfiguredElementFrame(
            static value => value,
            static _ => false,
            static handler => handler,
            static _ => true,
            static _ => true,
            null,
            null);
        var binderValue = new Identifier("value");
        var binder = new ArrowFunctionExpression(
            NodeList.From<Node>(binderValue),
            new AssignmentExpression(
                Operator.Assignment,
                new MemberExpression(new Identifier("state"), new Identifier("text"), computed: false, optional: false),
                binderValue),
            expression: true,
            async: false);
        InvokeNestedInstance<object?>(
            bindFrame,
            "AddAttribute",
            CreateDirectAttribute("onInput", binder, "String"));
        InvokeNestedInstance<object?>(bindFrame, "SetUpdatesAttributeName", "value");
        var fusedBind = InvokeNestedInstance<Expression>(bindFrame, "ToRenderExpression").ToKnRECMAScript();
        StringAssert.Contains(fusedBind, "target[\"value\"]", StringComparison.Ordinal);

        var checkedFrame = CreateConfiguredElementFrame(
            static value => value,
            static _ => false,
            static handler => handler,
            static _ => true,
            static _ => true,
            null,
            null);
        var checkedValue = new Identifier("checked");
        var checkedBinder = new ArrowFunctionExpression(
            NodeList.From<Node>(checkedValue),
            new AssignmentExpression(
                Operator.Assignment,
                new MemberExpression(new Identifier("state"), new Identifier("enabled"), computed: false, optional: false),
                checkedValue),
            expression: true,
            async: false);
        InvokeNestedInstance<object?>(
            checkedFrame,
            "AddAttribute",
            CreateDirectAttribute("onChange", checkedBinder, "Boolean"));
        InvokeNestedInstance<object?>(checkedFrame, "SetUpdatesAttributeName", "checked");
        StringAssert.Contains(
            InvokeNestedInstance<Expression>(checkedFrame, "ToRenderExpression").ToKnRECMAScript(),
            "target[\"checked\"]",
            StringComparison.Ordinal);

        var directiveFrame = CreateElementFrame();
        InvokeNestedInstance<object?>(
            directiveFrame,
            "AddAttribute",
            CreateDirectAttribute("@custom", new Identifier("directiveValue")));
        Assert.IsFalse(InvokeNestedInstance<bool>(directiveFrame, "get_CanUseRenderList"));

        var inlineValueFrame = CreateElementFrame();
        InvokeNestedInstance<object?>(
            inlineValueFrame,
            "AddAttribute",
            CreateDirectAttribute(
                "data-handler",
                new ArrowFunctionExpression(
                    NodeList.Empty<Node>(),
                    new Identifier("value"),
                    expression: true,
                    async: false)));
        Assert.IsFalse(InvokeNestedInstance<bool>(inlineValueFrame, "get_CanUseRenderList"));

        var modifierFrame = CreateConfiguredElementFrame(
            static value => value,
            static _ => false,
            static handler => handler,
            static _ => true,
            static _ => true,
            null,
            null);
        InvokeNestedInstance<object?>(
            modifierFrame,
            "AddAttribute",
            CreateDirectAttribute("onClick", new Identifier("stableHandler")));
        InvokeNestedInstance<object?>(
            modifierFrame,
            "SetEventModifier",
            "onclick",
            new BooleanLiteral(true, "true"),
            true,
            false);
        var modified = InvokeNestedInstance<Expression>(modifierFrame, "ToRenderExpression").ToKnRECMAScript();
        StringAssert.Contains(modified, "preventDefault", StringComparison.Ordinal);

        var textFrame = CreateConfiguredElementFrame(
            static value => value,
            static _ => false,
            null,
            null,
            null,
            null,
            () => textVNodeCount++);
        AddFramePlan(textFrame, "DynamicText", new Identifier("dynamicText"));
        AddFramePlan(textFrame, "Static", new StringLiteral("suffix", "\"suffix\""), false, false);
        var dynamicText = InvokeNestedInstance<Expression>(textFrame, "ToRenderExpression").ToKnRECMAScript();
        StringAssert.Contains(dynamicText, "createElementBlock", StringComparison.Ordinal);
        StringAssert.Contains(dynamicText, "createTextVNode", StringComparison.Ordinal);
        Assert.AreEqual(1, textVNodeCount);

        var emptyRegion = CreateRegionFrame();
        Assert.IsFalse(InvokeNestedInstance<bool>(emptyRegion, "get_CreatesFragment"));
        StringAssert.Contains(
            InvokeNestedInstance<Expression>(emptyRegion, "ToRenderExpression").ToKnRECMAScript(),
            "null",
            StringComparison.Ordinal);

        var singleRegion = CreateRegionFrame();
        AddFramePlan(singleRegion, "Static", new Identifier("single"), false, true);
        Assert.IsTrue(InvokeNestedInstance<bool>(singleRegion, "TrySetImplicitRootKey", "branch"));
        Assert.IsFalse(InvokeNestedInstance<bool>(singleRegion, "get_CreatesFragment"));
        Assert.AreEqual(
            "single",
            InvokeNestedInstance<Expression>(singleRegion, "ToRenderExpression").ToKnRECMAScript());

        var multiRegion = CreateRegionFrame();
        AddFramePlan(multiRegion, "Static", new Identifier("first"), false, true);
        AddFramePlan(multiRegion, "Static", new Identifier("second"), false, true);
        Assert.IsTrue(InvokeNestedInstance<bool>(multiRegion, "TrySetImplicitRootKey", "branch"));
        Assert.IsTrue(InvokeNestedInstance<bool>(multiRegion, "get_CreatesFragment"));
        var fragment = InvokeNestedInstance<Expression>(multiRegion, "ToRenderExpression").ToKnRECMAScript();
        StringAssert.Contains(fragment, "key: \"branch\"", StringComparison.Ordinal);
    }

    [TestMethod]
    public void FrameFallbackProtocols_KeepConservativeBindAndBlockPaths()
    {
        var noCacheFrame = CreateElementFrame();
        Assert.IsFalse(InvokeNestedInstance<bool>(
            noCacheFrame,
            "CanCacheStableEventHandler",
            new Identifier("handler")));
        Assert.IsFalse(InvokeNestedInstance<bool>(
            noCacheFrame,
            "IsStableEventHandler",
            new Identifier("handler")));

        var value = new Identifier("value");
        var assignment = new AssignmentExpression(
            Operator.Assignment,
            new MemberExpression(new Identifier("state"), new Identifier("text"), computed: false, optional: false),
            value);
        var directBinder = new ArrowFunctionExpression(
            NodeList.From<Node>(value),
            assignment,
            expression: true,
            async: false);

        var genericBindFrame = CreateElementFrame();
        InvokeNestedInstance<object?>(
            genericBindFrame,
            "AddAttribute",
            CreateDirectAttribute("onInput", directBinder, "None"));
        InvokeNestedInstance<object?>(genericBindFrame, "SetUpdatesAttributeName", "value");
        StringAssert.Contains(
            InvokeNestedInstance<Expression>(genericBindFrame, "ToRenderExpression").ToKnRECMAScript(),
            "eventOrValue",
            StringComparison.Ordinal);

        var invalidBinderFrame = CreateElementFrame();
        var invalidBinder = new ArrowFunctionExpression(
            NodeList.Empty<Node>(),
            new AssignmentExpression(
                Operator.Assignment,
                new MemberExpression(new Identifier("state"), new Identifier("text"), computed: false, optional: false),
                new Identifier("unbound")),
            expression: true,
            async: false);
        InvokeNestedInstance<object?>(
            invalidBinderFrame,
            "AddAttribute",
            CreateDirectAttribute("onInput", invalidBinder, "String"));
        InvokeNestedInstance<object?>(invalidBinderFrame, "SetUpdatesAttributeName", "value");
        StringAssert.Contains(
            InvokeNestedInstance<Expression>(invalidBinderFrame, "ToRenderExpression").ToKnRECMAScript(),
            "eventOrValue",
            StringComparison.Ordinal);

        var textVNodeCount = 0;
        var dynamicTextFrame = CreateConfiguredElementFrame(
            null,
            null,
            null,
            null,
            null,
            null,
            () => textVNodeCount++);
        InvokeNestedInstance<object?>(
            dynamicTextFrame,
            "AddAttribute",
            CreateDirectAttribute("id", new Identifier("dynamicId")));
        AddFramePlan(dynamicTextFrame, "DynamicText", new Identifier("dynamicText"));
        StringAssert.Contains(
            InvokeNestedInstance<Expression>(dynamicTextFrame, "ToRenderExpression").ToKnRECMAScript(),
            "dynamicText",
            StringComparison.Ordinal);

        var staticPrimitiveFrame = CreateConfiguredElementFrame(
            null,
            null,
            null,
            null,
            null,
            null,
            () => textVNodeCount++);
        InvokeNestedInstance<object?>(
            staticPrimitiveFrame,
            "AddAttribute",
            CreateDirectAttribute("id", new Identifier("dynamicId")));
        AddFramePlan(staticPrimitiveFrame, "Static", new NumericLiteral(1, "1"), false, false);
        StringAssert.Contains(
            InvokeNestedInstance<Expression>(staticPrimitiveFrame, "ToRenderExpression").ToKnRECMAScript(),
            "createTextVNode(1)",
            StringComparison.Ordinal);
        // A single dynamic text child uses Vue's direct TEXT patch path. Only the static
        // primitive inside a block needs an explicit createTextVNode wrapper.
        Assert.AreEqual(1, textVNodeCount);

        var emptyKeyedRegion = CreateRegionFrame();
        Assert.IsTrue(InvokeNestedInstance<bool>(emptyKeyedRegion, "TrySetImplicitRootKey", "empty"));
        Assert.IsFalse(InvokeNestedInstance<bool>(emptyKeyedRegion, "get_CreatesFragment"));

        var dynamicRegion = CreateRegionFrame();
        AddFrameChild(dynamicRegion, new Identifier("first"));
        AddFrameChild(dynamicRegion, new Identifier("second"));
        StringAssert.Contains(
            InvokeNestedInstance<Expression>(dynamicRegion, "ToRenderExpression").ToKnRECMAScript(),
            "Fragment",
            StringComparison.Ordinal);

        var directReferenceFrame = CreateElementFrame();
        InvokeNestedInstance<object?>(
            directReferenceFrame,
            "AddAttribute",
            CreateDirectAttribute("ref", new Identifier("capture")));
        var directReferencePatch = InvokeNestedInstance<object>(
            directReferenceFrame,
            "BuildPatchMetadata",
            false,
            false,
            0);
        Assert.AreNotEqual(0, GetRecordProperty<int>(directReferencePatch, "Flag"));

        // A standalone frame is a valid caller when module-level feature tracking is unavailable.
        // It must still emit the dynamic-text block without attempting to invoke a null tracker.
        var uninstrumentedBlockFrame = CreateElementFrame();
        InvokeNestedInstance<object?>(
            uninstrumentedBlockFrame,
            "AddAttribute",
            CreateDirectAttribute("id", new Identifier("dynamicId")));
        AddFramePlan(uninstrumentedBlockFrame, "Static", new StringLiteral("text", "\"text\""), false, false);
        StringAssert.Contains(
            InvokeNestedInstance<Expression>(uninstrumentedBlockFrame, "ToRenderExpression").ToKnRECMAScript(),
            "createElementBlock",
            StringComparison.Ordinal);
    }

    [TestMethod]
    public void RenderFragmentPreludePruning_PreservesTransitiveDependenciesAndRejectsMalformedTrackedDeclarations()
    {
        var fixture = CreateFixture();
        var emitter = CreateEmitter(fixture, GetNamedType(fixture, "EmitterHost"));
        var trackedField = emitter.GetType().GetField(
            "_renderFragmentPreludeDeclarations",
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(trackedField);
        var tracked = trackedField!.GetValue(emitter) as System.Collections.IList;
        Assert.IsNotNull(tracked);

        var upstream = CreateRenderFragmentPreludeDeclaration("upstream", new Identifier("source"));
        var rendered = CreateRenderFragmentPreludeDeclaration("rendered", new Identifier("upstream"));
        var dead = CreateRenderFragmentPreludeDeclaration("dead", new Identifier("unused"));
        var prelude = new List<Statement> { upstream, rendered, dead };
        tracked!.Add(upstream);
        tracked.Add(rendered);
        tracked.Add(dead);

        InvokeEmitterInstance<object?>(
            emitter,
            "PruneUnreferencedRenderFragmentDeclarations",
            prelude,
            new Identifier("rendered"));

        CollectionAssert.AreEqual(
            new[] { "upstream", "rendered" },
            prelude
                .Cast<VariableDeclaration>()
                .Select(static declaration => ((Identifier)declaration.Declarations[0].Id).Name)
                .ToArray());

        var uninitializedEmitter = CreateEmitter(fixture, GetNamedType(fixture, "EmitterHost"));
        var uninitializedTracked = uninitializedEmitter.GetType().GetField(
                "_renderFragmentPreludeDeclarations",
                BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(uninitializedEmitter) as System.Collections.IList;
        Assert.IsNotNull(uninitializedTracked);
        var uninitialized = new VariableDeclaration(
            VariableDeclarationKind.Let,
            NodeList.From(new VariableDeclarator(new Identifier("deferred"), null)));
        var uninitializedPrelude = new List<Statement> { uninitialized };
        uninitializedTracked!.Add(uninitialized);
        InvokeEmitterInstance<object?>(
            uninitializedEmitter,
            "PruneUnreferencedRenderFragmentDeclarations",
            uninitializedPrelude,
            new Identifier("deferred"));
        Assert.HasCount(1, uninitializedPrelude);

        var malformed = new VariableDeclaration(
            VariableDeclarationKind.Const,
            NodeList.From(
                new VariableDeclarator(new Identifier("first"), null),
                new VariableDeclarator(new Identifier("second"), null)));
        var malformedPrelude = new List<Statement> { malformed };
        tracked.Add(malformed);
        var exception = Assert.Throws<TargetInvocationException>(() =>
            InvokeEmitterInstance<object?>(
                emitter,
                "PruneUnreferencedRenderFragmentDeclarations",
                malformedPrelude,
                new Identifier("first")));
        StringAssert.Contains(
            exception.InnerException!.Message,
            "must contain one identifier binding",
            StringComparison.Ordinal);
    }

    [TestMethod]
    public void DirectRenderLoopAndDispatcherEdges_KeepOperationProtocolsExplicit()
    {
        var fixture = CreateFixture();
        var component = GetNamedType(fixture, "EmitterHost");
        var emitter = CreateEmitter(fixture, component);
        var unsupportedBranch = GetOperation<IBranchOperation>(
            fixture,
            GetMethod(fixture, "EmitterHost", "UnsupportedBranch")
                .DescendantNodes()
                .OfType<BreakStatementSyntax>()
                .Single());
        var unsupportedBranchContext = CreateEmitContext(
            GetMethodSymbol(fixture, "EmitterHost", "UnsupportedBranch").Parameters[0]);

        var unsupported = Assert.Throws<TargetInvocationException>(() =>
            InvokeEmitterInstance<object>(
                emitter,
                "EmitOperation",
                unsupportedBranch,
                unsupportedBranchContext,
                CreateRenderState()));
        StringAssert.Contains(
            unsupported.InnerException!.Message,
            "only supports straight-line RenderTreeBuilder statements",
            StringComparison.Ordinal);

        foreach (var methodName in new[] { "ForLoopWithExpressionUpdate", "ForLoopWithIncrementUpdate" })
        {
            var method = GetMethod(fixture, "EmitterHost", methodName);
            var loop = GetOperation<IForLoopOperation>(
                fixture,
                method.DescendantNodes().OfType<ForStatementSyntax>().Single());
            Assert.IsNotNull(InvokeEmitterInstance<object>(
                emitter,
                "EmitOperation",
                loop,
                CreateEmitContext(GetMethodSymbol(fixture, "EmitterHost", methodName).Parameters[0]),
                CreateRenderState()));
        }

        var whileMethod = GetMethod(fixture, "EmitterHost", "WhileLoopStaticContent");
        var whileLoop = GetOperation<IWhileLoopOperation>(
            fixture,
            whileMethod.DescendantNodes().OfType<WhileStatementSyntax>().Single());
        Assert.IsNotNull(InvokeEmitterInstance<object>(
            emitter,
            "EmitOperation",
            whileLoop,
            CreateEmitContext(GetMethodSymbol(fixture, "EmitterHost", "WhileLoopStaticContent").Parameters[0]),
            CreateRenderState()));

        // Exercise the complete block path as well as the private operation seam. The full
        // path owns loop-local declarations and branch-state flushing that an isolated operation
        // call intentionally bypasses.
        foreach (var methodName in new[]
                 {
                     "ForLoopWithMultipleUpdates",
                     "DirectForEachLoop",
                     "BranchingRootThenSideEffect"
                 })
        {
            var method = GetMethodSymbol(fixture, "EmitterHost", methodName);
            var body = GetMethodBody(fixture, "EmitterHost", methodName);
            var emitted = RenderEmitter.TryEmit(
                fixture.Compilation,
                component,
                method,
                body,
                declaredNames: null,
                VueInjectRegistry.ForCompilation(fixture.Compilation),
                out _,
                out var failure);
            Assert.IsTrue(emitted, methodName + ": " + failure);
        }
    }

    [TestMethod]
    public void DirectRenderLoopHelpers_HandleExpressionUpdatesAndRejectNonLoopBranches()
    {
        var fixture = CreateFixture();
        var host = GetNamedType(fixture, "EmitterHost");
        var emitter = CreateEmitter(fixture, host);
        var helperBody = GetMethodBody(fixture, "EmitterHost", "LoopHelperShapes");
        var helperContext = CreateEmitContext(
            GetMethodSymbol(fixture, "EmitterHost", "LoopHelperShapes").Parameters[0]);
        var variableDeclaration = helperBody.Operations.OfType<IVariableDeclarationGroupOperation>().Single();
        var assignment = helperBody.Operations.OfType<IExpressionStatementOperation>().Single();

        var initializerStatements = InvokeEmitterInstance<ImmutableArray<Statement>>(
            emitter,
            "LowerForLoopInitializers",
            ImmutableArray.Create<IOperation>(assignment),
            helperContext);
        Assert.HasCount(1, initializerStatements);
        var update = InvokeEmitterInstance<Expression>(
            emitter,
            "LowerForLoopUpdates",
            ImmutableArray.Create<IOperation>(assignment),
            helperContext);
        StringAssert.Contains(update.ToKnRECMAScript(), "state", StringComparison.Ordinal);
        Assert.IsNull(InvokeEmitterInstance<Expression?>(
            emitter,
            "LowerForLoopUpdates",
            ImmutableArray<IOperation>.Empty,
            helperContext));
        // A Roslyn loop update can arrive as a bare operation when it was synthesized by a
        // caller rather than parsed from an expression statement. Preserve that shape too.
        Assert.IsNotNull(InvokeEmitterInstance<Expression?>(
            emitter,
            "LowerForLoopUpdates",
            ImmutableArray.Create<IOperation>(GetVariableInitializer(
                fixture,
                GetMethod(fixture, "OperationShapes", "Inputs"),
                "literal")),
            helperContext));

        Assert.IsFalse(InvokeEmitter<bool>("IsLoopSideEffectOperation", variableDeclaration));
        Assert.IsTrue(InvokeEmitter<bool>("IsLoopSideEffectOperation", assignment));
        var builderInvocation = GetInvocation(
            fixture,
            GetMethod(fixture, "EmitterHost", "HelperInvocationShapes"),
            "AddContent");
        Assert.IsFalse(InvokeEmitter<bool>("IsLoopSideEffectOperation", builderInvocation));

        var loopMethod = GetMethod(fixture, "EmitterHost", "UnsupportedBranch");
        var loop = GetOperation<IWhileLoopOperation>(
            fixture,
            loopMethod.DescendantNodes().OfType<WhileStatementSyntax>().Single());
        var breakBranch = GetOperation<IBranchOperation>(
            fixture,
            loopMethod.DescendantNodes().OfType<BreakStatementSyntax>().Single());
        var continueMethod = GetMethod(fixture, "EmitterHost", "ContinueBranch");
        var continueBranch = GetOperation<IBranchOperation>(
            fixture,
            continueMethod.DescendantNodes().OfType<ContinueStatementSyntax>().Single());
        var gotoMethod = GetMethod(fixture, "EmitterHost", "GotoBranch");
        var gotoBranch = GetOperation<IBranchOperation>(
            fixture,
            gotoMethod.DescendantNodes().OfType<GotoStatementSyntax>().Single());

        Assert.IsInstanceOfType<BreakStatement>(
            InvokeEmitterInstance<Statement>(emitter, "LowerLoopBranch", breakBranch, helperContext));
        Assert.IsInstanceOfType<ContinueStatement>(
            InvokeEmitterInstance<Statement>(emitter, "LowerLoopBranch", continueBranch, helperContext));
        Assert.IsTrue(InvokeEmitter<bool>("IsBranchTargetingLoop", breakBranch, loop));
        Assert.IsFalse(InvokeEmitter<bool>("IsBranchTargetingLoop", gotoBranch, loop));
        var unsupported = Assert.Throws<TargetInvocationException>(() =>
            InvokeEmitterInstance<Statement>(emitter, "LowerLoopBranch", gotoBranch, helperContext));
        StringAssert.Contains(unsupported.InnerException!.Message, "Goto statements are not supported", StringComparison.Ordinal);

        var metadataInvocation = GetInvocation(
            fixture,
            GetMethod(fixture, "EmitterHost", "MetadataInvocation"),
            "AddEventPreventDefaultAttribute");
        Assert.AreEqual(1, InvokeEmitter<int>("GetRenderTreeBuilderReceiverArgumentOffset", metadataInvocation));
        Assert.AreEqual(0, InvokeEmitter<int>("GetRenderTreeBuilderReceiverArgumentOffset", builderInvocation));
    }

    [TestMethod]
    public void DirectRenderLoopStateBoundaries_FlushPreludeAndTrackScopedLocals()
    {
        var fixture = CreateFixture();
        var host = GetNamedType(fixture, "EmitterHost");
        var emitter = CreateEmitter(fixture, host);
        var loopMethod = GetMethod(fixture, "EmitterHost", "BranchingRootThenSideEffect");
        var loop = GetOperation<IWhileLoopOperation>(
            fixture,
            loopMethod.DescendantNodes().OfType<WhileStatementSyntax>().Single());
        var context = CreateEmitContext(
            GetMethodSymbol(fixture, "EmitterHost", "BranchingRootThenSideEffect").Parameters[0]);

        // A runtime local in a branching loop must be declared in the loop body prelude.
        // This exercises the scoped argument path distinct from straight-line loops.
        Assert.IsNotNull(InvokeEmitterInstance<object>(
            emitter,
            "EmitBranchingLoopBody",
            loop,
            context,
            new Identifier("result")));

        var state = CreateRenderState();
        var pending = GetRecordProperty<List<Statement>>(state, "PendingPreludeStatements");
        pending.Add(new NonSpecialExpressionStatement(new Identifier("sideEffect")));
        var statements = new List<Statement>();
        var factsType = typeof(RenderEmitter).GetNestedType("BranchingLoopFacts", BindingFlags.NonPublic)!;
        var facts = Activator.CreateInstance(factsType)!;
        var flush = typeof(RenderEmitter).GetNestedType("Emitter", BindingFlags.NonPublic)!
            .GetMethod("FlushBranchingLoopRenderState", BindingFlags.Static | BindingFlags.NonPublic);
        Assert.IsNotNull(flush);
        flush!.Invoke(null, [
            GetMethodBody(fixture, "EmitterHost", "BranchingRootThenSideEffect"),
            state,
            new Identifier("result"),
            statements,
            facts]);
        Assert.HasCount(1, statements);
    }

    [TestMethod]
    public void DirectRenderLoopHelpers_CoverConvertedSideEffectsAndTerminatedBranches()
    {
        var fixture = CreateFixture();
        var host = GetNamedType(fixture, "EmitterHost");
        var emitter = CreateEmitter(fixture, host);
        var loopMethod = GetMethod(fixture, "EmitterHost", "LoopWithScopedSideEffects");
        var loop = GetOperation<IWhileLoopOperation>(
            fixture,
            loopMethod.DescendantNodes().OfType<WhileStatementSyntax>().Single());
        var loopSymbol = GetMethodSymbol(fixture, "EmitterHost", "LoopWithScopedSideEffects");
        var context = CreateEmitContext(loopSymbol.Parameters[0]);
        Assert.IsNotNull(InvokeEmitterInstance<object>(
            emitter,
            "EmitLoopIterationBody",
            loop.Body,
            context));

        var converted = GetVariableInitializer(
            fixture,
            GetMethod(fixture, "OperationShapes", "Inputs"),
            "convertedLiteral");
        Assert.IsFalse(InvokeEmitter<bool>("IsLoopSideEffectOperation", converted));

        var coalesce = GetOperation<IOperation>(
            fixture,
            GetMethod(fixture, "EmitterHost", "LoopWithScopedSideEffects")
                .DescendantNodes()
                .OfType<AssignmentExpressionSyntax>()
                .Single(expression => expression.IsKind(SyntaxKind.CoalesceAssignmentExpression)));
        InvokeEmitterInstance<object?>(emitter, "TrackMutableRenderLocal", coalesce);

        var terminatedContext = CreateEmitContext(loopSymbol.Parameters[0], isTerminated: true);
        var state = CreateRenderState();
        var statements = new List<Statement>();
        var factsType = typeof(RenderEmitter).GetNestedType("BranchingLoopFacts", BindingFlags.NonPublic)!;
        var facts = Activator.CreateInstance(factsType)!;
        InvokeEmitterInstance<object>(
            emitter,
            "EmitBranchingLoopOperation",
            loop.Body,
            loop,
            terminatedContext,
            state,
            new Identifier("result"),
            statements,
            facts);

        var frame = typeof(RenderEmitter).GetNestedType("Frame", BindingFlags.NonPublic)!;
        var tryKey = frame.GetMethod("TrySetImplicitRootKey", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        Assert.IsNotNull(tryKey);
        Assert.IsTrue((bool)tryKey!.Invoke(CreateRegionFrame(), ["key"])!);
    }

    [TestMethod]
    public void RenderFragmentHelperRegistration_HandlesReentrantRegistrationAndInvalidReturn()
    {
        var fixture = CreateFixture();
        var host = GetNamedType(fixture, "EmitterHost");
        var emitter = CreateEmitter(fixture, host);
        var context = CreateEmitContext(
            GetMethodSymbol(fixture, "EmitterHost", "HelperInvocationShapes").Parameters[0]);
        var method = GetMethodSymbol(fixture, "EmitterHost", "NamedFragmentFactory");
        var bodyArguments = new object?[] { method, null };
        Assert.IsTrue(InvokeEmitterInstance<bool>(emitter, "TryGetReturnedRenderFragmentBody", bodyArguments));

        var emitting = emitter.GetType().GetField(
            "_emittingRenderFragmentHelperFunctions",
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(emitting);
        var set = emitting!.GetValue(emitter);
        var add = set!.GetType().GetMethod("Add", BindingFlags.Instance | BindingFlags.Public);
        Assert.IsNotNull(add);
        add!.Invoke(set, [method.OriginalDefinition]);
        var reentrant = InvokeEmitterInstance<object>(
            emitter,
            "EnsureRenderFragmentHelperFunction",
            method,
            bodyArguments[1]!,
            context);
        Assert.IsNotNull(reentrant);

        var invalid = GetMethodSymbol(fixture, "EmitterHost", "InvalidFactory");
        Assert.IsFalse(InvokeEmitterInstance<bool>(
            emitter,
            "TryGetReturnedRenderFragmentBody",
            new object?[] { invalid, null }));
    }

    [TestMethod]
    public void DirectRenderConditionalSlotAndPropertyBoundaries_KeepProvenanceAndRejectAmbiguousShapes()
    {
        var fixture = CreateFixture();
        var host = GetNamedType(fixture, "EmitterHost");
        var emitter = CreateEmitter(fixture, host);
        var slotMethod = GetMethod(fixture, "EmitterHost", "ConditionalSlotInvocations");
        var slotSymbol = GetMethodSymbol(fixture, "EmitterHost", "ConditionalSlotInvocations");
        var context = CreateEmitContext(slotSymbol.Parameters[0]);

        var normalHeader = GetInvocation(fixture, slotMethod, "AddComponentParameter", 0);
        var genericHeader = GetInvocation(fixture, slotMethod, "AddComponentParameter", 1);
        var genericTitle = GetInvocation(fixture, slotMethod, "AddComponentParameter", 2);
        var dynamicName = GetInvocation(fixture, slotMethod, "AddComponentParameter", 3);
        var scalarValue = GetInvocation(fixture, slotMethod, "AddComponentParameter", 4);
        var multiRootHeader = GetInvocation(fixture, slotMethod, "AddComponentParameter", 5);
        var genericMultiRootHeader = GetInvocation(fixture, slotMethod, "AddComponentParameter", 6);
        var normalAttribute = GetInvocation(fixture, slotMethod, "AddAttribute", 0);
        var booleanAttribute = GetInvocation(fixture, slotMethod, "AddAttribute", 1);
        var nonSlotContent = GetInvocation(fixture, slotMethod, "AddContent");

        // The resolver's empty, valid, wrong-method, wrong-arity, dynamic-name, and
        // non-fragment paths are all observable contracts for conditional slot lowering.
        var resolved = new object?[] { ImmutableArray<IInvocationOperation>.Empty, context, null, null };
        Assert.IsTrue(InvokeEmitterInstance<bool>(
            emitter,
            "TryResolveConditionalComponentSlotInvocation",
            resolved[0],
            resolved[1],
            resolved[2],
            resolved[3]));

        Assert.IsTrue(InvokeEmitterInstance<bool>(
            emitter,
            "TryResolveConditionalComponentSlotInvocation",
            ImmutableArray.Create(normalHeader),
            context,
            null,
            null));
        Assert.IsTrue(InvokeEmitterInstance<bool>(
            emitter,
            "TryResolveConditionalComponentSlotInvocation",
            ImmutableArray.Create(normalAttribute),
            context,
            null,
            null));
        Assert.IsFalse(InvokeEmitterInstance<bool>(
            emitter,
            "TryResolveConditionalComponentSlotInvocation",
            ImmutableArray.Create(nonSlotContent),
            context,
            null,
            null));
        Assert.IsFalse(InvokeEmitterInstance<bool>(
            emitter,
            "TryResolveConditionalComponentSlotInvocation",
            ImmutableArray.Create(booleanAttribute),
            context,
            null,
            null));
        Assert.IsFalse(InvokeEmitterInstance<bool>(
            emitter,
            "TryResolveConditionalComponentSlotInvocation",
            ImmutableArray.Create(dynamicName),
            context,
            null,
            null));
        Assert.IsFalse(InvokeEmitterInstance<bool>(
            emitter,
            "TryResolveConditionalComponentSlotInvocation",
            ImmutableArray.Create(scalarValue),
            context,
            null,
            null));
        Assert.IsFalse(InvokeEmitterInstance<bool>(
            emitter,
            "TryResolveConditionalComponentSlotInvocation",
            ImmutableArray.Create(normalHeader, normalAttribute),
            context,
            null,
            null));

        Assert.IsTrue(InvokeEmitterInstance<bool>(
            emitter,
            "TryResolveConditionalComponentSlotInvocation",
            ImmutableArray.Create(multiRootHeader),
            context,
            null,
            null));
        Assert.IsTrue(InvokeEmitterInstance<bool>(
            emitter,
            "TryResolveConditionalComponentSlotInvocation",
            ImmutableArray.Create(genericMultiRootHeader),
            context,
            null,
            null));

        var condition = new Identifier("enabled");
        var empty = ImmutableArray<IInvocationOperation>.Empty;
        var normal = ImmutableArray.Create(normalHeader);
        var generic = ImmutableArray.Create(genericHeader);
        var typedTitle = ImmutableArray.Create(genericTitle);
        var mappedNames = ImmutableDictionary<string, string>.Empty.Add("Header", "header-slot");

        // Matching RenderFragment values use the direct slot path, including a declared
        // runtime alias. The empty/normal combination preserves conditional slot absence.
        Assert.IsTrue(InvokeEmitterInstance<bool>(
            emitter,
            "TryEmitConditionalComponentSlot",
            CreateComponentFrame(mappedNames),
            condition,
            normal,
            normal,
            context,
            CreateRenderState()));
        Assert.IsTrue(InvokeEmitterInstance<bool>(
            emitter,
            "TryEmitConditionalComponentSlot",
            CreateComponentFrame(ImmutableDictionary<string, string>.Empty),
            condition,
            empty,
            normal,
            context,
            CreateRenderState()));
        Assert.IsTrue(InvokeEmitterInstance<bool>(
            emitter,
            "TryEmitConditionalComponentSlot",
            CreateComponentFrame(ImmutableDictionary<string, string>.Empty),
            condition,
            normal,
            empty,
            context,
            CreateRenderState()));
        Assert.IsTrue(InvokeEmitterInstance<bool>(
            emitter,
            "TryEmitConditionalComponentSlot",
            CreateComponentFrame(ImmutableDictionary<string, string>.Empty),
            condition,
            generic,
            generic,
            context,
            CreateRenderState()));
        Assert.IsTrue(InvokeEmitterInstance<bool>(
            emitter,
            "TryEmitConditionalComponentSlot",
            CreateComponentFrame(ImmutableDictionary<string, string>.Empty),
            condition,
            ImmutableArray.Create(multiRootHeader),
            ImmutableArray.Create(multiRootHeader),
            context,
            CreateRenderState()));
        Assert.IsTrue(InvokeEmitterInstance<bool>(
            emitter,
            "TryEmitConditionalComponentSlot",
            CreateComponentFrame(ImmutableDictionary<string, string>.Empty),
            condition,
            normal,
            ImmutableArray.Create(multiRootHeader),
            context,
            CreateRenderState()));
        Assert.IsTrue(InvokeEmitterInstance<bool>(
            emitter,
            "TryEmitConditionalComponentSlot",
            CreateComponentFrame(ImmutableDictionary<string, string>.Empty),
            condition,
            ImmutableArray.Create(genericMultiRootHeader),
            ImmutableArray.Create(genericMultiRootHeader),
            context,
            CreateRenderState()));
        Assert.IsTrue(InvokeEmitterInstance<bool>(
            emitter,
            "TryEmitConditionalComponentSlot",
            CreateComponentFrame(ImmutableDictionary<string, string>.Empty),
            condition,
            empty,
            generic,
            context,
            CreateRenderState()));

        // Different names, same-name generic/non-generic values, both empty branches, and
        // multiple invocations are intentionally kept on the conservative attribute path.
        Assert.IsFalse(InvokeEmitterInstance<bool>(
            emitter,
            "TryEmitConditionalComponentSlot",
            CreateComponentFrame(ImmutableDictionary<string, string>.Empty),
            condition,
            normal,
            typedTitle,
            context,
            CreateRenderState()));

        // The conservative attribute path still validates both RenderTreeBuilder overload
        // families. Exercise the generic RenderFragment rejection and a scalar value that is
        // lowered as an ordinary conditional prop.
        var conditionalComponent = CreateComponentFrame(ImmutableDictionary<string, string>.Empty);
        Assert.Throws<TargetInvocationException>(() =>
            InvokeEmitterInstance<object>(
                emitter,
                "BuildConditionalAttributes",
                ImmutableArray.Create(genericHeader),
                context,
                conditionalComponent));
        var loweredAttributes = InvokeEmitterInstance<object>(
            emitter,
            "BuildConditionalAttributes",
            ImmutableArray.Create(scalarValue),
            context,
            conditionalComponent);
        Assert.IsNotNull(loweredAttributes);
        Assert.IsFalse(InvokeEmitterInstance<bool>(
            emitter,
            "TryEmitConditionalComponentSlot",
            CreateComponentFrame(ImmutableDictionary<string, string>.Empty),
            condition,
            normal,
            generic,
            context,
            CreateRenderState()));
        Assert.IsFalse(InvokeEmitterInstance<bool>(
            emitter,
            "TryEmitConditionalComponentSlot",
            CreateComponentFrame(ImmutableDictionary<string, string>.Empty),
            condition,
            empty,
            empty,
            context,
            CreateRenderState()));
        Assert.IsFalse(InvokeEmitterInstance<bool>(
            emitter,
            "TryEmitConditionalComponentSlot",
            CreateComponentFrame(ImmutableDictionary<string, string>.Empty),
            condition,
            ImmutableArray.Create(normalHeader, normalAttribute),
            normal,
            context,
            CreateRenderState()));

        var expressionProperty = GetProperty(host, "ExpressionFragment");
        var blockProperty = GetProperty(host, "BlockFragment");
        var accessorExpressionProperty = GetProperty(host, "AccessorExpressionFragment");
        var autoProperty = GetProperty(host, "AutoFragment");
        var writeOnlyProperty = GetProperty(host, "WriteOnlyFragment");
        var localProperty = GetProperty(host, "LocalBlockFragment");
        var invalidLocalProperty = GetProperty(host, "InvalidLocalBlockFragment");
        var indexer = host.GetMembers().OfType<IPropertySymbol>().Single(property => property.IsIndexer);

        foreach (var property in new[] { expressionProperty, blockProperty, accessorExpressionProperty })
            AssertReturnedPropertyValue(emitter, property);
        foreach (var property in new[] { autoProperty, writeOnlyProperty, indexer })
            Assert.IsFalse(InvokeEmitterInstance<bool>(emitter, "TryGetReturnedPropertyValue", new object?[] { property, null }));

        Assert.IsTrue(InvokeEmitterInstance<bool>(
            emitter,
            "TryGetLocalRenderFragmentPropertyValue",
            localProperty,
            null));
        foreach (var property in new[] { invalidLocalProperty, expressionProperty, autoProperty, indexer })
            Assert.IsFalse(InvokeEmitterInstance<bool>(
                emitter,
                "TryGetLocalRenderFragmentPropertyValue",
                property,
                null));

        // Follow a local RenderFragment declaration, and verify that an unrelated declaration
        // cannot be guessed as its initializer.
        var factoryBody = GetMethodBody(fixture, "EmitterHost", "BlockFactory");
        var factoryResult = new object?[] { factoryBody, null, null };
        Assert.IsTrue(InvokeEmitter<bool>("TryGetRenderFragmentFactoryReturn", factoryResult));
        var returned = (IOperation)factoryResult[1]!;
        var declarations = (ImmutableArray<IVariableDeclarationGroupOperation>)factoryResult[2]!;
        var unwrapResult = new object?[] { returned, declarations, null };
        Assert.IsTrue(InvokeEmitter<bool>("TryUnwrapLocalRenderFragmentFactoryReturn", unwrapResult));
        var unrelatedDeclarations = GetMethodBody(fixture, "EmitterHost", "UninitializedFragmentLocal")
            .Operations
            .OfType<IVariableDeclarationGroupOperation>()
            .ToImmutableArray();
        Assert.IsFalse(InvokeEmitter<bool>(
            "TryUnwrapLocalRenderFragmentFactoryReturn",
            returned,
            unrelatedDeclarations,
            null));

        var multiLoop = GetOperation<IForLoopOperation>(
            fixture,
            GetMethod(fixture, "EmitterHost", "ForLoopWithMultipleUpdates")
                .DescendantNodes().OfType<ForStatementSyntax>().Single());
        var loopContext = CreateEmitContext(GetMethodSymbol(fixture, "EmitterHost", "ForLoopWithMultipleUpdates").Parameters[0]);
        // Going through EmitOperation supplies the loop-local aliases exactly as the production
        // path does, and exercises multi-declaration initialization plus sequence updates.
        Assert.IsNotNull(InvokeEmitterInstance<object>(
            emitter,
            "EmitOperation",
            multiLoop,
            loopContext,
            CreateRenderState()));
        var literal = GetVariableInitializer(fixture, GetMethod(fixture, "OperationShapes", "Inputs"), "literal");
        Assert.Throws<TargetInvocationException>(() =>
            InvokeEmitterInstance<object>(
                emitter,
                "LowerForLoopInitializers",
                ImmutableArray.Create(literal),
                loopContext));
    }

    [TestMethod]
    public void DirectRenderFrameBranchBoundaries_PreserveEmptyAndPreexistingFragmentState()
    {
        var fixture = CreateFixture();
        var host = GetNamedType(fixture, "EmitterHost");
        var emitter = CreateEmitter(fixture, host);

        StringAssert.Contains(
            InvokeNestedInstance<Expression>(CreateElementFrame(), "FormatChildrenExpression").ToKnRECMAScript(),
            "null",
            StringComparison.Ordinal);
        var nonEmptyElement = CreateElementFrame();
        AddFrameChild(nonEmptyElement, new Identifier("child"));
        StringAssert.Contains(
            InvokeNestedInstance<Expression>(nonEmptyElement, "FormatChildrenExpression").ToKnRECMAScript(),
            "[child]",
            StringComparison.Ordinal);

        var blockPatch = InvokeNestedInstance<object>(
            CreateElementFrame(),
            "BuildPatchMetadata",
            true,
            false,
            0);
        Assert.IsTrue(GetRecordProperty<bool>(blockPatch, "RequiresBlock"));
        Assert.AreEqual(0, GetRecordProperty<int>(blockPatch, "Flag"));

        var slotMethod = GetMethod(fixture, "EmitterHost", "ConditionalSlotInvocations");
        var slotSymbol = GetMethodSymbol(fixture, "EmitterHost", "ConditionalSlotInvocations");
        var context = CreateEmitContext(slotSymbol.Parameters[0]);
        var normal = ImmutableArray.Create(GetInvocation(fixture, slotMethod, "AddComponentParameter", 0));
        var generic = ImmutableArray.Create(GetInvocation(fixture, slotMethod, "AddComponentParameter", 1));
        var condition = new Identifier("enabled");
        var state = CreateRenderState();
        var usesFragment = state.GetType().GetProperty("UsesFragment", BindingFlags.Instance | BindingFlags.Public);
        Assert.IsNotNull(usesFragment);
        usesFragment!.SetValue(state, true);
        Assert.IsTrue(InvokeEmitterInstance<bool>(
            emitter,
            "TryEmitConditionalComponentSlot",
            CreateComponentFrame(ImmutableDictionary<string, string>.Empty),
            condition,
            normal,
            normal,
            context,
            state));
        Assert.IsTrue(GetRecordProperty<bool>(state, "UsesFragment"));

        // The reverse generic/ordinary pairing exercises the opposite parameter-name mismatch
        // and must remain on the conservative conditional-attribute path.
        Assert.IsFalse(InvokeEmitterInstance<bool>(
            emitter,
            "TryEmitConditionalComponentSlot",
            CreateComponentFrame(ImmutableDictionary<string, string>.Empty),
            condition,
            generic,
            normal,
            context,
            CreateRenderState()));

        var noHoistFrame = CreateElementFrame();
        InvokeNestedInstance<object?>(
            noHoistFrame,
            "AddAttribute",
            CreateDirectAttribute("title", new StringLiteral("fixed", "\"fixed\"")));
        StringAssert.Contains(
            InvokeNestedInstance<Expression>(noHoistFrame, "ToRenderExpression").ToKnRECMAScript(),
            "title",
            StringComparison.Ordinal);

        // A published hoist callback without a predicate must remain a normal object literal;
        // this is the null-predicate branch of PropFrame.FormatPropsExpression.
        var callbackOnlyFrame = CreateConfiguredElementFrame(
            static _ => new Identifier("hoisted"),
            canHoistStaticProps: null,
            cacheStableEventHandler: null,
            canCacheStableEventHandler: null,
            isStableEventHandler: null,
            useBlockTree: null,
            useTextVNode: null);
        InvokeNestedInstance<object?>(
            callbackOnlyFrame,
            "AddAttribute",
            CreateDirectAttribute("title", new StringLiteral("fixed", "\"fixed\"")));
        StringAssert.Contains(
            InvokeNestedInstance<Expression>(callbackOnlyFrame, "FormatPropsExpression").ToKnRECMAScript(),
            "title",
            StringComparison.Ordinal);
    }

    [TestMethod]
    public void DirectRenderFrameAndLoopBoundaries_PreserveEmptyChildrenAndBranchingProtocols()
    {
        var fixture = CreateFixture();
        var host = GetNamedType(fixture, "EmitterHost");
        var emitter = CreateEmitter(fixture, host);

        var emptyComponent = CreateComponentFrame(ImmutableDictionary<string, string>.Empty);
        var emptyComponentOutput = InvokeNestedInstance<Expression>(emptyComponent, "ToRenderExpression").ToKnRECMAScript();
        StringAssert.Contains(emptyComponentOutput, "h(component", StringComparison.Ordinal);

        var childComponent = CreateComponentFrame(ImmutableDictionary<string, string>.Empty);
        AddFrameChild(childComponent, new Identifier("child"));
        var childComponentOutput = InvokeNestedInstance<Expression>(childComponent, "ToRenderExpression").ToKnRECMAScript();
        StringAssert.Contains(childComponentOutput, "h(component", StringComparison.Ordinal);

        // A component with a dynamic prop but no children must take the block path even
        // though it has no slot/child frame. This is the complement of the static empty case.
        var dynamicComponent = CreateComponentFrame(ImmutableDictionary<string, string>.Empty);
        InvokeNestedInstance<object?>(
            dynamicComponent,
            "AddAttribute",
            CreateDirectAttribute("id", new Identifier("dynamicId")));
        StringAssert.Contains(
            InvokeNestedInstance<Expression>(dynamicComponent, "ToRenderExpression").ToKnRECMAScript(),
            "createBlock",
            StringComparison.Ordinal);

        var branchMethod = GetMethod(fixture, "EmitterHost", "BranchingRootThenSideEffect");
        var branchLoop = GetOperation<IWhileLoopOperation>(
            fixture,
            branchMethod.DescendantNodes().OfType<WhileStatementSyntax>().Single());
        Assert.IsNotNull(InvokeEmitterInstance<object>(
            emitter,
            "EmitOperation",
            branchLoop,
            CreateEmitContext(GetMethodSymbol(fixture, "EmitterHost", "BranchingRootThenSideEffect").Parameters[0]),
            CreateRenderState()));

        var foreachMethod = GetMethod(fixture, "EmitterHost", "DirectRenderEdgeShapes");
        var foreachLoop = GetOperation<IForEachLoopOperation>(
            fixture,
            foreachMethod.DescendantNodes().OfType<ForEachVariableStatementSyntax>().Single());
        var foreachContext = CreateEmitContext(GetMethodSymbol(fixture, "EmitterHost", "DirectRenderEdgeShapes").Parameters[0]);
        Assert.IsNotNull(InvokeEmitterInstance<object>(emitter, "LowerForEachLoopBinding", foreachLoop, foreachContext));

        var simpleForeachMethod = GetMethod(fixture, "EmitterHost", "DirectForEachLoop");
        var simpleForeach = GetOperation<IForEachLoopOperation>(
            fixture,
            simpleForeachMethod.DescendantNodes().OfType<ForEachStatementSyntax>().Single());
        Assert.IsNotNull(InvokeEmitterInstance<object>(emitter, "LowerForEachLoopBinding", simpleForeach, foreachContext));

        foreach (var methodName in new[] { "FactoryWithoutReturn", "InvalidFactory", "NonFragmentFactory" })
        {
            Assert.IsFalse(InvokeEmitterInstance<bool>(
                emitter,
                "TryGetReturnedRenderFragmentBody",
                new object?[] { GetMethodSymbol(fixture, "EmitterHost", methodName), null }));
        }
    }

    [TestMethod]
    public void ExpressionAndReceiverHelpers_PreserveExplicitBindingAndRejectNonPropertyProtocols()
    {
        var fixture = CreateFixture();
        var host = GetNamedType(fixture, "EmitterHost");
        var emitter = CreateEmitter(fixture, host);
        var inputs = GetMethod(fixture, "OperationShapes", "Inputs");
        var inputSymbol = GetMethodSymbol(fixture, "OperationShapes", "Inputs");
        var parameter = inputSymbol.Parameters[0];
        var parameterReference = GetVariableInitializer(fixture, inputs, "omitParameter");
        var literal = GetVariableInitializer(fixture, inputs, "literal");

        var substitutions = ImmutableDictionary<IParameterSymbol, IOperation>.Empty
            .WithComparers(SymbolEqualityComparer.Default)
            .Add(parameter, literal);
        StringAssert.Contains(
            InvokeEmitterInstance<Expression>(
                emitter,
                "LowerExpression",
                parameterReference,
                CreateEmitContext(parameter, substitutions: substitutions)).ToKnRECMAScript(),
            "constant",
            StringComparison.Ordinal);

        var aliases = ImmutableDictionary<IParameterSymbol, string>.Empty
            .WithComparers(SymbolEqualityComparer.Default)
            .Add(parameter, "boundParameter");
        Assert.AreEqual(
            "boundParameter",
            InvokeEmitterInstance<Expression>(
                emitter,
                "LowerExpression",
                parameterReference,
                CreateEmitContext(parameter, parameterAliases: aliases)).ToKnRECMAScript());

        var metadataMethod = GetMethod(fixture, "EmitterHost", "MetadataInvocation");
        var metadataInvocation = GetInvocation(fixture, metadataMethod, "AddEventPreventDefaultAttribute");
        var metadataContext = CreateEmitContext(GetMethodSymbol(fixture, "EmitterHost", "MetadataInvocation").Parameters[0]);
        AssertRenderTreeBuilderReceiver(emitter, metadataInvocation, metadataContext, expected: true);

        var helperMethod = GetMethod(fixture, "EmitterHost", "HelperInvocationShapes");
        var helperContext = CreateEmitContext(GetMethodSymbol(fixture, "EmitterHost", "HelperInvocationShapes").Parameters[0]);
        AssertRenderTreeBuilderReceiver(
            emitter,
            GetInvocation(fixture, helperMethod, "Write", ordinal: 0),
            helperContext,
            expected: true);
        AssertRenderTreeBuilderReceiver(
            emitter,
            GetInvocation(fixture, helperMethod, "Write", ordinal: 1),
            helperContext,
            expected: false);
        AssertRenderTreeBuilderReceiver(
            emitter,
            GetInvocation(fixture, helperMethod, "InstanceBuilderHelper", ordinal: 1),
            helperContext,
            expected: false);

        var indexer = host.GetMembers().OfType<IPropertySymbol>().Single(static property => property.IsIndexer);
        Assert.IsFalse(InvokeEmitterInstance<bool>(
            emitter,
            "TryGetReturnedPropertyValue",
            new object?[] { indexer, null }));

        foreach (var propertyName in new[] { "ExpressionFragment", "BlockFragment", "AccessorExpressionFragment" })
        {
            Assert.IsTrue(InvokeEmitterInstance<bool>(
                emitter,
                "TryGetReturnedPropertyValue",
                new object?[]
                {
                    host.GetMembers(propertyName).OfType<IPropertySymbol>().Single(),
                    null
                }));
        }

        foreach (var methodName in new[] { "ExpressionFactory", "BlockLiteralFactory", "GenericExpressionFactory" })
        {
            Assert.IsTrue(InvokeEmitterInstance<bool>(
                emitter,
                "TryGetReturnedRenderFragmentBody",
                new object?[] { GetMethodSymbol(fixture, "EmitterHost", methodName), null }));
        }

        var implicitConstructor = host.InstanceConstructors.Single(static constructor => constructor.IsImplicitlyDeclared);
        Assert.IsFalse(InvokeEmitterInstance<bool>(
            emitter,
            "TryBuildConstructorRenderFragmentPropertyMap",
            new object?[] { implicitConstructor, null }));

        Assert.Throws<TargetInvocationException>(() =>
            InvokeEmitterInstance<object>(
                emitter,
                "LowerExpression",
                GetMethodBody(fixture, "OperationShapes", "Empty"),
                metadataContext));
        Assert.Throws<TargetInvocationException>(() =>
            InvokeEmitterInstance<object>(
                emitter,
                "LowerExpression",
                GetReturnOperation(fixture, "OperationShapes", "VoidReturn"),
                metadataContext));
    }

    [TestMethod]
    public void LoopHelperLowering_PreservesMultipleUpdatesAndRejectsNonExpressionSideEffects()
    {
        var fixture = CreateFixture();
        var emitter = CreateEmitter(fixture, GetNamedType(fixture, "EmitterHost"));
        var method = GetMethod(fixture, "EmitterHost", "ForLoopWithMultipleUpdates");
        var methodSymbol = GetMethodSymbol(fixture, "EmitterHost", "ForLoopWithMultipleUpdates");
        var loop = GetOperation<IForLoopOperation>(
            fixture,
            method.DescendantNodes().OfType<ForStatementSyntax>().Single());
        var updates = InvokeEmitterInstance<Expression>(
            emitter,
            "LowerForLoopUpdates",
            loop.AtLoopBottom,
            CreateEmitContext(methodSymbol.Parameters[0]));
        Assert.IsInstanceOfType<SequenceExpression>(updates);

        foreach (var update in loop.AtLoopBottom)
            _ = InvokeEmitterInstance<object?>(emitter, "TrackMutableRenderLocal", update);

        var invalidSideEffect = GetMethodBody(fixture, "EmitterHost", "LoopHelperShapes")
            .Operations
            .OfType<IVariableDeclarationGroupOperation>()
            .Single();
        var exception = Assert.Throws<TargetInvocationException>(() =>
            InvokeEmitterInstance<ImmutableArray<Statement>>(
                emitter,
                "LowerLoopSideEffectStatements",
                ImmutableArray.Create<IOperation>(invalidSideEffect),
                CreateEmitContext(methodSymbol.Parameters[0])));
        StringAssert.Contains(
            exception.InnerException!.Message,
            "ordinary expression statement",
            StringComparison.Ordinal);
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

    private static void AssertComponentImport(
        Fixture fixture,
        INamedTypeSymbol componentType,
        string importSpecifier,
        string exportName)
    {
        var descriptor = Invoke<object>("ResolveComponentImport", fixture.Compilation, componentType);
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
            .Single(candidate => candidate.GetParameters().Length == 7);
        return constructor.Invoke(
        [
            fixture.Compilation,
            componentSymbol,
            null,
            null,
            VueInjectRegistry.ForCompilation(fixture.Compilation),
            null,
            null
        ]);
    }

    private static T InvokeEmitterInstance<T>(object emitter, string methodName, params object?[] arguments)
    {
        var method = emitter.GetType()
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
            .Single(candidate => candidate.Name == methodName && candidate.GetParameters().Length == arguments.Length);
        return (T)method.Invoke(emitter, arguments)!;
    }

    private static T InvokeNestedInstance<T>(object instance, string methodName, params object?[] arguments)
    {
        var method = instance.GetType()
            .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Single(candidate =>
            {
                if (candidate.Name != methodName)
                    return false;

                var parameters = candidate.GetParameters();
                if (parameters.Length != arguments.Length)
                    return false;

                for (var index = 0; index < parameters.Length; index++)
                {
                    var argument = arguments[index];
                    var parameterType = parameters[index].ParameterType;
                    if (argument is null)
                    {
                        if (parameterType.IsValueType && Nullable.GetUnderlyingType(parameterType) is null)
                            return false;
                    }
                    else if (!parameterType.IsInstanceOfType(argument))
                    {
                        return false;
                    }
                }

                return true;
            });
        return (T)method.Invoke(instance, arguments)!;
    }

    private static void SetNestedPrivateField(object instance, string fieldName, object? value)
    {
        FieldInfo? field = null;
        for (var type = instance.GetType(); type is not null; type = type.BaseType)
        {
            field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            if (field is not null)
                break;
        }

        Assert.IsNotNull(field, fieldName);
        field!.SetValue(instance, value);
    }

    private static void AssertRenderTreeBuilderReceiver(
        object emitter,
        IInvocationOperation invocation,
        object context,
        bool expected)
    {
        var arguments = new object?[] { invocation, context, null };
        Assert.AreEqual(expected, InvokeEmitter<bool>("TryGetRenderTreeBuilderReceiver", arguments));
        if (expected)
            Assert.IsNotNull(arguments[2]);
    }

    private static void AssertResolvedRenderFragmentMethodReference(
        object emitter,
        IOperation operation,
        object context,
        bool expected)
    {
        var arguments = new object?[] { operation, context, null };
        Assert.AreEqual(expected, InvokeEmitterInstance<bool>(emitter, "TryResolveRenderFragmentMethodReference", arguments));
        if (expected)
            Assert.IsNotNull(arguments[2]);
    }

    private static void AssertResolvedRenderObjectHelper(
        object emitter,
        IInvocationOperation invocation,
        object context,
        bool expected)
    {
        var arguments = new object?[] { invocation, context, null };
        Assert.AreEqual(expected, InvokeEmitterInstance<bool>(emitter, "TryResolveRenderObjectHelperInvocation", arguments));
        if (expected)
            Assert.IsNotNull(arguments[2]);
    }

    private static IInvocationOperation GetInvocation(
        Fixture fixture,
        MethodDeclarationSyntax method,
        string targetMethodName,
        int ordinal = 0)
        => method.DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Select(syntax => GetOperation<IInvocationOperation>(fixture, syntax))
            .Where(operation => operation.TargetMethod.Name == targetMethodName)
            .ElementAt(ordinal);

    private static object CreateEmitContext(
        ISymbol builderSymbol,
        bool allowPreludeDeclarations = true,
        ImmutableHashSet<ILocalSymbol>? secondaryBuilders = null,
        ImmutableDictionary<IParameterSymbol, IOperation>? substitutions = null,
        ImmutableDictionary<IParameterSymbol, string>? parameterAliases = null,
        bool isTerminated = false)
    {
        var contextType = typeof(RenderEmitter).GetNestedType("EmitContext", BindingFlags.NonPublic);
        var fragmentType = typeof(RenderEmitter).GetNestedType("DirectRenderFragment", BindingFlags.NonPublic);
        var renderObjectType = typeof(RenderEmitter).GetNestedType("DirectRenderObject", BindingFlags.NonPublic);
        Assert.IsNotNull(contextType);
        Assert.IsNotNull(fragmentType);
        Assert.IsNotNull(renderObjectType);

        var constructor = contextType!
            .GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Single(candidate => candidate.GetParameters().Length == 12);
        return constructor.Invoke(
        [
            CreateBuilderBinding(builderSymbol),
            substitutions ?? ImmutableDictionary<IParameterSymbol, IOperation>.Empty.WithComparers(SymbolEqualityComparer.Default),
            parameterAliases ?? ImmutableDictionary<IParameterSymbol, string>.Empty.WithComparers(SymbolEqualityComparer.Default),
            ImmutableDictionary<ILocalSymbol, string>.Empty.WithComparers(SymbolEqualityComparer.Default),
            GetEmptyImmutableDictionary(typeof(ILocalSymbol), fragmentType!),
            GetEmptyImmutableDictionary(typeof(ILocalSymbol), renderObjectType!),
            ImmutableDictionary<ILocalSymbol, INamedTypeSymbol>.Empty.WithComparers(SymbolEqualityComparer.Default),
            secondaryBuilders ?? ImmutableHashSet<ILocalSymbol>.Empty.WithComparer(SymbolEqualityComparer.Default),
            new List<Statement>(),
            allowPreludeDeclarations,
            new SenseArgument(),
            isTerminated
        ]);
    }

    private static object CreateRenderState()
    {
        var stateType = typeof(RenderEmitter).GetNestedType("RenderState", BindingFlags.NonPublic);
        Assert.IsNotNull(stateType);
        var constructor = stateType!
            .GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Single();
        return constructor.Invoke([null]);
    }

    private static void PushFrame(object state, object frame)
    {
        var stack = state.GetType().GetProperty("Stack", BindingFlags.Instance | BindingFlags.Public)?.GetValue(state);
        Assert.IsNotNull(stack);
        var push = stack!.GetType().GetMethod("Push", BindingFlags.Instance | BindingFlags.Public);
        Assert.IsNotNull(push);
        _ = push!.Invoke(stack, [frame]);
    }

    private static object GetEmptyImmutableDictionary(Type keyType, Type valueType)
    {
        var factory = typeof(ImmutableDictionary)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(candidate =>
                candidate.Name == "Create" &&
                candidate.IsGenericMethodDefinition &&
                candidate.GetGenericArguments().Length == 2 &&
                candidate.GetParameters().Length == 0);
        return factory.MakeGenericMethod(keyType, valueType).Invoke(null, null)!;
    }

    private static T GetRecordProperty<T>(object instance, string name)
    {
        var property = instance.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        Assert.IsNotNull(property, name);
        return (T)property!.GetValue(instance)!;
    }

    private static object CreateBuilderBinding(ISymbol symbol)
    {
        var bindingType = typeof(RenderEmitter).GetNestedType("BuilderBinding", BindingFlags.NonPublic);
        Assert.IsNotNull(bindingType);
        var constructor = bindingType!
            .GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Single(candidate => candidate.GetParameters().Length == 1);
        return constructor.Invoke([symbol]);
    }

    private static object CreateElementFrame()
    {
        var elementFrameType = typeof(RenderEmitter).GetNestedType("ElementFrame", BindingFlags.NonPublic);
        Assert.IsNotNull(elementFrameType);
        var constructor = elementFrameType!
            .GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Single(candidate => candidate.GetParameters().Length == 2);
        return constructor.Invoke([new StringLiteral("div", "\"div\""), "div"]);
    }

    private static object CreateConfiguredElementFrame(
        Func<ObjectExpression, Expression>? hoistStaticProps,
        Func<ObjectExpression, bool>? canHoistStaticProps,
        Func<Expression, Expression>? cacheStableEventHandler,
        Func<Expression, bool>? canCacheStableEventHandler,
        Func<Expression, bool>? isStableEventHandler,
        Action? useBlockTree,
        Action? useTextVNode)
    {
        var elementFrameType = typeof(RenderEmitter).GetNestedType("ElementFrame", BindingFlags.NonPublic);
        Assert.IsNotNull(elementFrameType);
        var constructor = elementFrameType!
            .GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Single(candidate => candidate.GetParameters().Length == 9);
        return constructor.Invoke(
        [
            new StringLiteral("input", "\"input\""),
            "input",
            hoistStaticProps,
            canHoistStaticProps,
            cacheStableEventHandler,
            canCacheStableEventHandler,
            isStableEventHandler,
            useBlockTree,
            useTextVNode
        ]);
    }

    private static object CreateRegionFrame()
    {
        var regionFrameType = typeof(RenderEmitter).GetNestedType("RegionFrame", BindingFlags.NonPublic);
        Assert.IsNotNull(regionFrameType);
        var constructor = regionFrameType!
            .GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Single(candidate => candidate.GetParameters().Length == 0);
        return constructor.Invoke(null)!;
    }

    private static object CreateComponentFrame(ImmutableDictionary<string, string> parameterNames)
    {
        var componentFrameType = typeof(RenderEmitter).GetNestedType("ComponentFrame", BindingFlags.NonPublic);
        Assert.IsNotNull(componentFrameType);
        var constructor = componentFrameType!
            .GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Single(candidate => candidate.GetParameters().Length == 2);
        return constructor.Invoke([new Identifier("component"), parameterNames]);
    }

    private static object CreateConfiguredComponentFrame(
        ImmutableDictionary<string, string> parameterNames,
        Action? useWithCtx,
        Action? useCreateSlots,
        bool slotsAreInStableScope)
    {
        var componentFrameType = typeof(RenderEmitter).GetNestedType("ComponentFrame", BindingFlags.NonPublic);
        Assert.IsNotNull(componentFrameType);
        var constructor = componentFrameType!
            .GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Single(candidate => candidate.GetParameters().Length == 13);
        return constructor.Invoke(
        [
            new Identifier("component"),
            parameterNames,
            ImmutableDictionary<string, string>.Empty,
            null,
            null,
            null,
            null,
            null,
            null,
            useWithCtx,
            useCreateSlots,
            slotsAreInStableScope,
            false
        ]);
    }

    private static void AddFrameChild(object frame, Expression child)
    {
        var children = frame.GetType().GetProperty("Children", BindingFlags.Instance | BindingFlags.Public);
        Assert.IsNotNull(children);
        var values = children!.GetValue(frame) as System.Collections.IList;
        Assert.IsNotNull(values);
        var vnodePlanType = typeof(RenderEmitter).GetNestedType("VNodePlan", BindingFlags.NonPublic);
        Assert.IsNotNull(vnodePlanType);
        var opaque = vnodePlanType!
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(candidate => candidate.Name == "Opaque" && candidate.GetParameters().Length == 2);
        Assert.IsNotNull(opaque);
        _ = values!.Add(opaque!.Invoke(null, [child, false]));
    }

    private static void AddFramePlan(object frame, string factoryName, params object[] arguments)
    {
        var children = frame.GetType().GetProperty("Children", BindingFlags.Instance | BindingFlags.Public);
        Assert.IsNotNull(children);
        var values = children!.GetValue(frame) as System.Collections.IList;
        Assert.IsNotNull(values);
        var vnodePlanType = typeof(RenderEmitter).GetNestedType("VNodePlan", BindingFlags.NonPublic);
        Assert.IsNotNull(vnodePlanType);
        var factory = vnodePlanType!
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(candidate => candidate.Name == factoryName && candidate.GetParameters().Length == arguments.Length);
        _ = values!.Add(factory.Invoke(null, arguments));
    }

    private static void AddComponentSlot(object componentFrame, string name, object fragment)
    {
        var slots = componentFrame.GetType().GetProperty("Slots", BindingFlags.Instance | BindingFlags.Public);
        Assert.IsNotNull(slots);
        var values = slots!.GetValue(componentFrame) as System.Collections.IList;
        Assert.IsNotNull(values);
        var slotType = typeof(RenderEmitter).GetNestedType("DirectSlot", BindingFlags.NonPublic);
        Assert.IsNotNull(slotType);
        var constructor = slotType!
            .GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Single(candidate => candidate.GetParameters().Length == 2);
        _ = values!.Add(constructor.Invoke([name, fragment]));
    }

    private static object CreateDirectAttribute(
        string name,
        Expression value,
        string directBinderValueKind = "None")
    {
        var attributeType = typeof(RenderEmitter).GetNestedType("DirectAttribute", BindingFlags.NonPublic);
        Assert.IsNotNull(attributeType);
        var constructor = attributeType!
            .GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Single(candidate => candidate.GetParameters().Length == 3);
        var directBinderValueKindType = typeof(RenderEmitter).Assembly
            .GetType("Jazor.RazorVue.RazorSdk.DirectBinderValueKind", throwOnError: true)!;
        return constructor.Invoke([name, value, Enum.Parse(directBinderValueKindType, directBinderValueKind)]);
    }

    private static object CreateDirectAttributeArray(params object[] attributes)
    {
        var attributeType = typeof(RenderEmitter).GetNestedType("DirectAttribute", BindingFlags.NonPublic);
        Assert.IsNotNull(attributeType);
        var immutableArrayType = typeof(ImmutableArray<>).MakeGenericType(attributeType!);
        var value = immutableArrayType.GetField("Empty", BindingFlags.Public | BindingFlags.Static)!.GetValue(null)!;
        var add = immutableArrayType
            .GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .Single(candidate => candidate.Name == "Add" && candidate.GetParameters().Length == 1);
        foreach (var attribute in attributes)
            value = add.Invoke(value, [attribute])!;
        return value;
    }

    private static VariableDeclaration CreateRenderFragmentPreludeDeclaration(string name, Expression initializer)
        => new(
            VariableDeclarationKind.Const,
            NodeList.From(new VariableDeclarator(new Identifier(name), initializer)));

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

    private static object CreateDirectRenderFragment(
        Expression renderExpression,
        string? parameterName,
        Expression? availabilityCondition = null,
        object? selection = null)
    {
        var fragmentType = typeof(RenderEmitter).GetNestedType("DirectRenderFragment", BindingFlags.NonPublic);
        Assert.IsNotNull(fragmentType);
        var constructor = fragmentType!
            .GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Single(candidate => candidate.GetParameters().Length == 7);
        return constructor.Invoke(new object?[]
        {
            renderExpression,
            parameterName,
            false,
            availabilityCondition,
            null,
            selection,
            false
        });
    }

    private static object CreateConditionalRenderFragmentSelection(
        Expression condition,
        object whenTrue,
        object whenFalse)
    {
        var selectionType = typeof(RenderEmitter).GetNestedType("ConditionalRenderFragmentSelection", BindingFlags.NonPublic);
        Assert.IsNotNull(selectionType);
        var constructor = selectionType!
            .GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Single(candidate => candidate.GetParameters().Length == 3);
        return constructor.Invoke([condition, whenTrue, whenFalse]);
    }

    private static Fixture CreateFixture()
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(
            """
            using System;
            using System.Collections.Generic;
            using System.Runtime.InteropServices;
            using ECMAScript;
            using ECMAScript.Contract;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;
            using static ECMAScript.Vue;

            namespace RenderEmitterPrivateContracts;

            [Obsolete]
            [ECMAScriptModule(" ./components/module ")]
            public sealed class ModuleComponent : ComponentBase, IVueComponent;

            [ECMAScriptModule(" ")]
            [ECMAScript(" tdesign-vue-next ", Transform.Component, " Button ")]
            public sealed class LibraryComponent : ComponentBase, IVueComponent;

            [ECMAScriptModule]
            public sealed class NoArgumentModuleComponent : ComponentBase, IVueComponent;

            [ECMAScriptModule(null)]
            public sealed class NullModuleComponent : ComponentBase, IVueComponent;

            [Obsolete]
            public sealed class NoImportComponent : ComponentBase, IVueComponent;

            [ECMAScript]
            public sealed class AllowMarkerComponent : ComponentBase, IVueComponent;

            [ECMAScript("vue")]
            public sealed class ImportMarkerComponent : ComponentBase, IVueComponent;

            [ECMAScript(" ", Transform.Component, "Button")]
            public sealed class InvalidLibraryComponent : ComponentBase, IVueComponent;

            public sealed class SlotMapComponent : ComponentBase
            {
                [Parameter, ECMAScriptName("header-slot")]
                public RenderFragment Header { get; set; } = default!;

                [Parameter]
                public string? Title { get; set; }
            }

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

            public static class ForeignTypes
            {
                public delegate void RenderFragment<T>(T value);
            }

            public static class RuntimeHelpers
            {
                public static int TypeCheck(int value) => value;
            }

            public sealed class TypeParameterOwner<T>;

            public sealed class EmitterHost : ComponentBase
            {
                private RenderFragment field = default!;

                public static RenderFragment StaticHeader => builder => { };

                public RenderFragment ExpressionFragment => builder => { };

                public RenderFragment BlockFragment
                {
                    get
                    {
                        return builder => { };
                    }
                }

                public RenderFragment LocalBlockFragment
                {
                    get
                    {
                        RenderFragment local = builder => builder.AddContent(0, "local-property");
                        return local;
                    }
                }

                public RenderFragment InvalidLocalBlockFragment
                {
                    get
                    {
                        var local = 1;
                        return builder => builder.AddContent(0, local);
                    }
                }

                public RenderFragment AutoFragment { get; set; } = default!;

                [Parameter] public RenderFragment Header { get; set; } = default!;

                public RenderFragment WriteOnlyFragment
                {
                    set { }
                }

                public RenderFragment ExpressionFactory() => builder => { };

                public RenderFragment AccessorExpressionFragment
                {
                    get => builder => { };
                }

                public RenderFragment this[int index] => builder => { };

                public RenderFragment<int> GenericExpressionFactory() => value => builder => { };

                public RenderFragment<int> GenericBlockLiteralFactory()
                {
                    return value => builder => { };
                }

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

                public void HelperInvocationShapes(RenderTreeBuilder builder, RenderTreeBuilder other)
                {
                    builder.AddContent(0, "receiver-primary");
                    other.AddContent(1, "receiver-secondary");
                    InstanceBuilderHelper(builder, "instance-helper");
                    StaticBuilderHelper(builder, "static-helper");
                    LocalBuilderHelper(builder);
                    ExternalBuilderHelper.Write(builder, "external-helper");
                    ExpressionBuilderHelper(builder);
                    InstanceBuilderHelper(other, "mismatched-helper");
                    NonBuilderHelper("not-a-builder");
                    StaticNoBuilder();
                    new ForeignBuilderHelper().Write(builder);

                    void LocalBuilderHelper(RenderTreeBuilder child)
                    {
                        child.AddContent(0, "local-helper");
                    }
                }

                public void ConditionalSlotInvocations(
                    RenderTreeBuilder builder,
                    string dynamicName)
                {
                    builder.AddComponentParameter(0, "Header", ExpressionFactory());
                    builder.AddComponentParameter(1, "Header", GenericExpressionFactory());
                    builder.AddComponentParameter(2, "Title", GenericExpressionFactory());
                    builder.AddComponentParameter(3, dynamicName, ExpressionFactory());
                    builder.AddComponentParameter(4, "Value", "scalar");
                    builder.AddComponentParameter(5, "Header", MultiRootFragmentFactory());
                    builder.AddComponentParameter(6, "Header", GenericMarkupAndMultiRootFactory());
                    builder.AddAttribute(5, "Header", ExpressionFactory());
                    builder.AddAttribute(6, "Header");
                    builder.AddContent(7, "content");
                }

                public void DirectRenderEdgeShapes(
                    RenderTreeBuilder builder,
                    bool enabled,
                    IEnumerable<(string Name, int Value)> entries)
                {
                    builder.OpenElement(0, "section");
                    if (enabled)
                        builder.AddAttribute(1, "class", "enabled");
                    else
                        builder.AddAttribute(2, "class", "disabled");
                    builder.CloseElement();

                    foreach (var (name, value) in entries)
                    {
                        builder.OpenElement(3, "span");
                        builder.AddContent(4, name);
                        builder.CloseElement();
                    }
                }

                public void DirectForEachLoop(RenderTreeBuilder builder)
                {
                    foreach (var value in new[] { 1, 2 })
                        builder.AddContent(0, value);
                }

                public void BooleanAndConditionalAttributes(RenderTreeBuilder builder, bool enabled)
                {
                    builder.OpenElement(0, "input");
                    builder.AddAttribute(1, "disabled");
                    if (enabled)
                        builder.AddAttribute(2, "checked");
                    else
                        builder.AddAttribute(3, "readonly");
                    builder.CloseElement();
                }

                public void MetadataInvocation(RenderTreeBuilder builder)
                {
                    Microsoft.AspNetCore.Components.Web.WebRenderTreeBuilderExtensions.AddEventPreventDefaultAttribute(
                        builder,
                        0,
                        "onclick",
                        true);
                }

                public void RuntimeLocalInsideOpenFrame(RenderTreeBuilder builder)
                {
                    builder.OpenElement(0, "div");
                    var runtime = GetRuntimeValue();
                    builder.CloseElement();
                }

                private static int GetRuntimeValue() => 1;

                public void RuntimeLocalOutsideFrame(RenderTreeBuilder builder)
                {
                    var runtime = GetRuntimeValue();
                }

                public void SupportedLoop(RenderTreeBuilder builder, bool enabled)
                {
                    while (enabled)
                        break;
                }

                public void UnsupportedBranch(RenderTreeBuilder builder)
                {
                    while (true)
                    {
                        break;
                    }
                }

                public void BranchingRootThenSideEffect(RenderTreeBuilder builder)
                {
                    while (true)
                    {
                        builder.AddContent(0, "root");
                        var marker = 1;
                        marker = marker + 1;
                        break;
                    }
                }

                public void LoopWithScopedSideEffects(RenderTreeBuilder builder)
                {
                    int? local = null;
                    while (local.GetValueOrDefault() < 2)
                    {
                        local ??= 1;
                        local++;
                        builder.AddContent(0, local);
                    }
                }

                public void ContinueBranch(RenderTreeBuilder builder)
                {
                    while (true)
                    {
                        continue;
                    }
                }

                public void GotoBranch(RenderTreeBuilder builder)
                {
                    goto done;
                done:
                    return;
                }

                public void LoopHelperShapes(RenderTreeBuilder builder)
                {
                    var state = 0;
                    state = state + 1;
                }

                public void ForLoopWithExpressionUpdate(RenderTreeBuilder builder)
                {
                    for (var index = 0; index < 2; index = index + 1)
                        builder.AddContent(0, index);
                }

                public void ForLoopWithIncrementUpdate(RenderTreeBuilder builder)
                {
                    for (var index = 0; index < 2; index++)
                        builder.AddContent(0, index);
                }

                public void ForLoopWithMultipleUpdates(RenderTreeBuilder builder)
                {
                    for (int index = 0, remaining = 2; index < remaining; index++, remaining--)
                        builder.AddContent(0, index);
                }

                public void WhileLoopStaticContent(RenderTreeBuilder builder, bool enabled)
                {
                    while (enabled)
                    {
                        builder.OpenElement(0, "span");
                        builder.AddContent(1, "static");
                        builder.CloseElement();
                    }
                }

                public void StorageReferences()
                {
                    var own = Header;
                    var staticValue = StaticHeader;
                    var field = this.field;
                    var foreign = new ForeignStorage().Value;
                    var scalar = GetRuntimeValue();
                }

                public void OpenComponentShapes(RenderTreeBuilder builder)
                {
                    builder.OpenComponent<ModuleComponent>(0);
                    builder.CloseComponent();
                    builder.OpenComponent(1, typeof(LibraryComponent));
                    builder.CloseComponent();
                }

                public void GenericOpenComponentShape<TComponent>(RenderTreeBuilder builder)
                    where TComponent : IComponent
                {
                    builder.OpenComponent<TComponent>(0);
                }

                public void AdditionalRenderFragmentReferences(RenderTreeBuilder builder)
                {
                    RenderFragment expression = ExpressionRenderMethodGroup;
                    RenderFragment local = LocalRenderMethodGroup;

                    void LocalRenderMethodGroup(RenderTreeBuilder child)
                    {
                        child.AddContent(0, "local-method-group");
                    }
                }

                public void ExpressionRenderMethodGroup(RenderTreeBuilder child)
                    => child.AddContent(0, "expression-method-group");

                public void AdditionalPropertyReferences()
                {
                    RenderFragment staticHeader = StaticHeader;
                    RenderFragment foreignHeader = new ForeignStorage().Value;
                    RenderFragment foreignInstanceHeader = new EmitterHost().ExpressionFragment;
                }

                public void ProvenanceObject()
                {
                    var carrier = new FragmentCarrier(child => child.AddContent(0, "carrier"));
                }

                public void HelperFactoryReferences()
                {
                    var fragment = MultiRootFragmentFactory();
                    var named = NamedFragmentFactory("named");
                    var loop = LoopFragmentFactory();
                }

                public RenderFragment MultiRootFragmentFactory()
                    => child =>
                    {
                        child.AddContent(0, "first");
                        child.AddContent(1, "second");
                    };

                public RenderFragment MarkupFragmentFactory()
                    => child => child.AddContent(0, new MarkupString("<em>fragment</em>"));

                public void GenericContentInvocation(RenderTreeBuilder builder, int value)
                {
                    builder.AddContent(0, GenericMarkupAndMultiRootFactory(), value);
                }

                public RenderFragment<int> GenericMarkupAndMultiRootFactory()
                    => value => child =>
                    {
                        child.AddContent(0, new MarkupString("<em>generic</em>"));
                        child.AddContent(1, "second");
                    };

                [DllImport("native")]
                private static extern void NativeRenderMethod(RenderTreeBuilder child);

                [DllImport("native")]
                private static extern FragmentCarrier NativeCarrier();

                public void InvalidAddAttribute(FakeBuilder builder)
                {
                    builder.AddAttribute(0, "class", "value", 1);
                }

                public void AttributeAfterChild(RenderTreeBuilder builder)
                {
                    builder.OpenElement(0, "div");
                    builder.AddContent(1, "child");
                    builder.AddAttribute(2, "class", "late");
                    builder.CloseElement();
                }

                public void TwoArgumentEventModifier(FakeBuilder builder)
                {
                    builder.AddEventPreventDefaultAttribute("onclick", true);
                }

                public void DynamicConditionalAttributes(RenderTreeBuilder builder, bool visible, string name)
                {
                    builder.OpenElement(0, "div");
                    if (visible)
                        builder.AddAttribute(1, name, "visible");
                    else
                        builder.AddAttribute(2, "class", "hidden");
                    builder.CloseElement();
                }

                public void InstanceBuilderHelper(RenderTreeBuilder child, string text)
                {
                    child.AddContent(0, text);
                }

                public static void StaticBuilderHelper(RenderTreeBuilder child, string text)
                {
                    child.AddContent(0, text);
                }

                public void ExpressionBuilderHelper(RenderTreeBuilder child) => child.AddContent(0, "expression-helper");

                public void NonBuilderHelper(string value)
                {
                }

                public static void StaticNoBuilder()
                {
                }

                public void RenderFragmentReferences()
                {
                    RenderFragment instance = RenderMethodGroup;
                    RenderFragment staticGroup = StaticRenderMethodGroup;
                    Action<RenderTreeBuilder, int> invalid = InvalidRenderMethodGroup;
                    object converted = (object)(RenderFragment)RenderMethodGroup;
                    RenderFragment recursive = RecursiveRenderMethodGroup;
                    RenderFragment externGroup = NativeRenderMethod;
                }

                public void RenderMethodGroup(RenderTreeBuilder child)
                {
                    child.AddContent(0, "instance-method-group");
                }

                public static void StaticRenderMethodGroup(RenderTreeBuilder child)
                {
                    child.AddContent(0, "static-method-group");
                }

                public void InvalidRenderMethodGroup(RenderTreeBuilder child, int state)
                {
                }

                public void RecursiveRenderMethodGroup(RenderTreeBuilder child)
                {
                    RecursiveRenderMethodGroup(child);
                }

                public void RenderObjectReferences()
                {
                    var block = CreateCarrier("block-carrier");
                    var expression = CreateExpressionCarrier("expression-carrier");
                    var ignored = CreateIgnoredCarrier();
                    var local = LocalObjectFactory();
                    var recursive = RecursiveObjectFactory();
                    var native = NativeCarrier();
                    object convertedCarrier = (object)CreateCarrier("converted-carrier");

                    FragmentCarrier LocalObjectFactory() => new();
                }

                public FragmentCarrier CreateCarrier(string text)
                {
                    RenderFragment header = child => child.AddContent(0, text);
                    return new FragmentCarrier(header);
                }

                public FragmentCarrier CreateExpressionCarrier(string text)
                    => new(child => child.AddContent(0, text));

                public FragmentCarrier CreateIgnoredCarrier() => new();

                public FragmentCarrier RecursiveObjectFactory() => RecursiveObjectFactory();

                public FragmentCarrier UnexpectedObjectFactory()
                {
                    ObjectSideEffect();
                    return new FragmentCarrier(child => child.AddContent(0, "unexpected"));
                }

                public void ObjectSideEffect()
                {
                }

                public RenderFragment NamedFragmentFactory(string text)
                    => child => child.AddContent(0, text);

                public RenderFragment LoopFragmentFactory()
                    => child =>
                    {
                        foreach (var value in new[] { 1, 2 })
                        {
                            child.AddContent(0, value);
                            child.AddContent(1, value);
                        }
                    };

                public RenderFragment UnclosedFragmentFactory()
                    => child => child.OpenElement(0, "div");

                public void ComponentSlotReferences()
                {
                    object boxedHeader = (object)Header;
                }

                public void DynamicComponentParameter(RenderTreeBuilder builder, string name)
                {
                    builder.AddComponentParameter(0, name, "value");
                }

                public void InvalidGenericContent(FakeBuilder builder)
                {
                    builder.AddContent(0, "not-a-fragment", 1);
                }

                public void UninitializedFragmentLocal()
                {
                    RenderFragment? unassigned;
                }

                public sealed class FragmentCarrier
                {
                    public FragmentCarrier()
                    {
                    }

                    public FragmentCarrier(RenderFragment header)
                    {
                        Header = header;
                    }

                public RenderFragment Header { get; set; } = default!;
            }

            public record RecordCarrier(RenderFragment Header);
            }

            public static class ExternalBuilderHelper
            {
                public static void Write(RenderTreeBuilder child, string text)
                {
                    child.AddContent(0, text);
                }
            }

            public sealed class ForeignBuilderHelper
            {
                public void Write(RenderTreeBuilder child)
                {
                    child.AddContent(0, "foreign-helper");
                }
            }

                public sealed class FakeBuilder
                {
                    public void AddContent<TValue>(int sequence, object value, TValue argument)
                    {
                    }

                    public void AddAttribute(int sequence, string name, object value, int extra)
                    {
                    }

                    public void AddEventPreventDefaultAttribute(string eventName, bool value)
                    {
                    }
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

            public sealed class NoisyCarrier
            {
                public NoisyCarrier(RenderFragment header)
                {
                    Name = "ignored";
                    Header = header;
                }

                public string Name { get; set; } = string.Empty;

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

            public sealed class ForeignStorage
            {
                public RenderFragment Value { get; set; } = default!;
            }

            public sealed class OperationShapes
            {
                private const sbyte ConstantSByte = 1;
                private const byte ConstantByte = 1;
                private const short ConstantShort = 1;
                private const ushort ConstantUShort = 1;
                private const uint ConstantUInt = 1;
                private const long ConstantLong = 1;
                private const ulong ConstantULong = 1;
                private const float ConstantFloat = 1f;
                private const double ConstantDouble = 1d;
                private const decimal ConstantDecimal = 1m;

                public int Field;
                public int Property { get; set; }

                private static int GetValue() => 1;

                public void VoidReturn() { return; }
                public int ValueReturn() { return 1; }
                public int SingleReturnValue() { return 7; }
                public void Empty() { }
                public void SingleStatementWithoutReturn() { var ignored = 0; }
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
                    string? staticNull = null;
                    var staticString = "static";
                    var staticBoolean = true;
                    var staticChar = 'c';
                    sbyte staticSByte = 1;
                    byte staticByte = 1;
                    short staticShort = 1;
                    ushort staticUShort = 1;
                    var staticInt = 1;
                    var repeatedLoopLocal = loopLocal + loopLocal;
                    uint staticUInt = 1;
                    long staticLong = 1;
                    ulong staticULong = 1;
                    float staticFloat = 1;
                    double staticDouble = 1;
                    decimal staticDecimal = 1;
                    var constantSByte = ConstantSByte;
                    var constantByte = ConstantByte;
                    var constantShort = ConstantShort;
                    var constantUShort = ConstantUShort;
                    var constantUInt = ConstantUInt;
                    var constantLong = ConstantLong;
                    var constantULong = ConstantULong;
                    var constantFloat = ConstantFloat;
                    var constantDouble = ConstantDouble;
                    var constantDecimal = ConstantDecimal;
                    var rawStaticMarkup = "<strong>raw</strong>";
                    var markupStatic = new MarkupString("<em>markup</em>");
                    var markupDynamic = new MarkupString(dynamicText);
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
                    ForeignTypes.RenderFragment<int> foreignGenericFragment = value => { };
                    MarkupString? convertedNullableMarkup = (MarkupString?)nullableMarkup;
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

                public void NonAttributeCollectionInitializer()
                {
                    var values = new List<KeyValuePair<string, object>>
                    {
                        new("key", "value")
                    };
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
                    object convertedFragmentAsObject = (object)(RenderFragment)(builder => { });
                    object convertedGenericFragmentAsObject = (object)(RenderFragment<int>)(value => builder => { });
                    object convertedFragmentReference = (object)fragment;
                    object convertedGenericFragmentReference = (object)generic;
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

                public void LocalFragmentFactory()
                {
                    RenderFragment Local() => builder => { };
                    var fragment = Local();
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

                public void BuilderBindings(RenderTreeBuilder builder, RenderTreeBuilder other)
                {
                    RenderTreeBuilder local = builder;
                    RenderTreeBuilder otherAlias = other;
                    RenderTreeBuilder converted = (RenderTreeBuilder)builder;
                    local.AddContent(0, "local");
                    converted.AddContent(1, "converted");
                    ((RenderTreeBuilder)builder).AddContent(2, "cast-receiver");
                }
            }
            """,
            new CSharpParseOptions(LanguageVersion.Preview),
            path: "RenderEmitterPrivateContracts.cs");
        var adapterNamespaceTree = CSharpSyntaxTree.ParseText(
            """
            namespace Microsoft.AspNetCore.Components.Routing
            {
                public sealed class UnregisteredRoutingType { }
            }

            namespace Microsoft.AspNetCore.Components.Forms
            {
                public sealed class UnregisteredFormsType { }
            }

            namespace Microsoft.AspNetCore.Components
            {
                public sealed class UnregisteredRootType { }
            }

            namespace Microsoft.AspNetCore.Components.Web
            {
                public sealed class UnregisteredWebType { }
            }
            """,
            new CSharpParseOptions(LanguageVersion.Preview),
            path: "RenderEmitterPrivateContractAdapterNamespaces.cs");
        var compilation = CSharpCompilation.Create(
            "RazorVue.RenderEmitterPrivateContracts",
            [syntaxTree, adapterNamespaceTree],
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
