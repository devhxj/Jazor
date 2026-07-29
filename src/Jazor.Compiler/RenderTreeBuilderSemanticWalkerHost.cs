using System;
using System.Collections.Generic;
using System.Linq;
using Acornima.Ast;
using Jazor.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.Compiler;

/// <summary>
/// Opt-in host seam that lowers the supported
/// <c>Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder</c> slice to the
/// Jazor Vue render-context protocol while leaving ordinary C# expressions to
/// <see cref="SemanticWalker"/>.
/// </summary>
public sealed class RenderTreeBuilderSemanticWalkerHost : SemanticWalkerHost
{
    private const string RenderTreeBuilderMetadataName = "Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder";
    private const string RenderTreeBuilderExtensionsMetadataName = "Microsoft.AspNetCore.Components.Web.WebRenderTreeBuilderExtensions";
    private const string RenderContextModulePath = "@jazor/vue-runtime/render-context.mjs";
    private const string ECMAScriptModuleAttributeMetadataName = "ECMAScript.ECMAScriptModuleAttribute";
    private const string VueLibraryComponentAttributeMetadataName = "ECMAScript.VueContract.VueLibraryComponentAttribute";
    private const string VuePropAttributeMetadataName = "ECMAScript.VueContract.VuePropAttribute";
    private const string VueLibraryEmitAttributeMetadataName = "ECMAScript.VueContract.VueLibraryEmitAttribute";
    private const string VueSlotAttributeMetadataName = "ECMAScript.VueContract.VueSlotAttribute";

    public override Expression? RewriteObjectCreationPreorder(IObjectCreationOperation operation, SenseArgument argument)
        => TryGetStaticMarkupString(operation, out var markup)
            ? CreateStringLiteral(markup ?? string.Empty)
            : null;

    public override bool ShouldRewriteObjectCreation(IObjectCreationOperation operation)
        => operation.Type is not null &&
           (IsMarkupString(operation.Type) ||
            IsRenderTreeBuilder(operation.Type));

    public override Expression? RewriteObjectCreation(
        IObjectCreationOperation operation,
        SenseArgument argument,
        IReadOnlyList<Expression> arguments)
    {
        if (operation.Type is not null &&
            operation.Constructor is not null &&
            IsMarkupString(operation.Type) &&
            operation.Constructor.Parameters.Length == 1 &&
            arguments.Count == 1)
        {
            return arguments[0];
        }

        if (operation.Type is not null &&
            operation.Constructor is not null &&
            IsRenderTreeBuilder(operation.Type) &&
            operation.Constructor.Parameters.Length == 0 &&
            arguments.Count == 0)
        {
            var createRenderContext = argument.BindImportSpecifier(RenderContextModulePath, "createRenderContext");
            return new CallExpression(
                createRenderContext,
                NodeList.From(new Expression[] { new Identifier("h") }),
                optional: false);
        }

        return null;
    }

    public override bool ShouldSkipVariableDeclarator(
        IVariableDeclaratorOperation operation,
        SenseArgument argument)
        => operation.Symbol.Type is not null &&
           IsSystemType(operation.Symbol.Type) &&
           operation.Initializer?.Value is { } initializer &&
           TryResolveTypeOfExpression(initializer, out _) &&
           TryResolveLocalTypeOfInitializer(operation, operation.Symbol, out _) &&
           AllLocalReferencesAreOpenComponentTypeArguments(operation, operation.Symbol);

    public override Expression? RewriteInvocationPreorder(IInvocationOperation operation, SenseArgument argument)
    {
        if (!IsRenderTreeBuilderMethod(operation.TargetMethod) &&
            !IsRenderTreeBuilderEventModifierMethod(operation.TargetMethod))
            return null;

        if (IsRenderTreeBuilderEventModifierMethod(operation.TargetMethod))
            return null;

        if (!IsSupportedRenderContextMethod(operation))
            throw CreateUnsupportedException(operation);

        return null;
    }

    public override Expression? RewriteInvocationArgumentPreorder(
        IInvocationOperation operation,
        IArgumentOperation argumentOperation,
        int argumentIndex,
        SenseArgument argument)
    {
        var method = operation.TargetMethod.OriginalDefinition;
        if (argumentIndex == 1 &&
            IsRenderTreeBuilderMethod(operation.TargetMethod) &&
            string.Equals(method.Name, "OpenComponent", StringComparison.Ordinal) &&
            IsOpenComponentTypeMethod(method) &&
            TryResolveComponentTypeArgument(operation, out _))
        {
            return new NullLiteral("null");
        }

        return null;
    }

    public override Expression? RewriteInvocation(
        IInvocationOperation operation,
        SenseArgument argument,
        Expression? instance,
        IReadOnlyList<Expression> arguments)
    {
        if (IsRenderTreeBuilderEventModifierMethod(operation.TargetMethod))
            return BuildEventModifierCall(operation, instance, arguments);

        if (!IsRenderTreeBuilderMethod(operation.TargetMethod))
            return null;

        if (instance is null)
            throw CreateUnsupportedException(operation);

        var method = operation.TargetMethod.OriginalDefinition;
        return method.Name switch
        {
            "OpenElement" when IsSequenceStringMethod(method, expectedParameterCount: 2)
                => BuildRenderContextCall(operation, instance, arguments, "openElement", ContextCallArgument.FromSource(1)),

            "CloseElement" when method.Parameters.Length == 0
                => BuildRenderContextCall(operation, instance, arguments, "closeElement"),

            "OpenRegion" when IsSequenceOnlyMethod(method)
                => BuildRenderContextCall(operation, instance, arguments, "openRegion"),

            "CloseRegion" when method.Parameters.Length == 0
                => BuildRenderContextCall(operation, instance, arguments, "closeRegion"),

            "OpenComponent" when IsGenericOpenComponentMethod(method)
                => BuildOpenComponentCall(operation, argument, instance, arguments),

            "OpenComponent" when IsOpenComponentTypeMethod(method)
                => BuildOpenComponentTypeCall(operation, argument, instance, arguments),

            "CloseComponent" when method.Parameters.Length == 0
                => BuildRenderContextCall(operation, instance, arguments, "closeComponent"),

            "AddContent" when IsSupportedAddContentMethod(method)
                => BuildRenderContextCall(operation, instance, arguments, "addContent", ContextCallArgument.FromSource(1)),

            "AddContent" when IsRenderFragmentAddContentMethod(method)
                => BuildRenderFragmentInvoke(operation, instance, arguments),

            "AddContent" when IsGenericRenderFragmentAddContentMethod(method)
                => BuildGenericRenderFragmentInvoke(operation, instance, arguments),

            "AddContent" when IsMarkupStringAddContentMethod(method)
                => BuildRenderContextCall(operation, instance, arguments, "addMarkupContent", ContextCallArgument.FromSource(1)),

            "AddMarkupContent" when IsAddMarkupContentSignature(method)
                => BuildRenderContextCall(operation, instance, arguments, "addMarkupContent", ContextCallArgument.FromSource(1)),

            "AddAttribute" when IsAddAttributeWithoutValueMethod(method)
                => BuildRenderContextCall(
                    operation,
                    instance,
                    arguments,
                    "addAttribute",
                    ContextCallArgument.FromSource(1),
                    ContextCallArgument.FromExpression(new BooleanLiteral(true, "true"))),

            "AddAttribute" when IsAddAttributeWithValueMethod(method)
                => BuildRenderContextCall(
                    operation,
                    instance,
                    arguments,
                    "addAttribute",
                    ContextCallArgument.FromSource(1),
                    ContextCallArgument.FromSource(2)),

            "AddAttribute" when IsAddAttributeFrameMethod(method)
                => BuildRenderContextCall(
                    operation,
                    instance,
                    arguments,
                    "addAttributeFrame",
                    ContextCallArgument.FromSource(1)),

            "AddMultipleAttributes" when IsAddMultipleAttributesMethod(method)
                => BuildRenderContextCall(
                    operation,
                    instance,
                    arguments,
                    "addMultipleAttributes",
                    ContextCallArgument.FromSource(1)),

            "AddComponentParameter" when IsAddComponentParameterMethod(method) &&
                                        IsGenericRenderFragmentComponentParameterValue(operation)
                => BuildRenderContextCall(
                    operation,
                    instance,
                    arguments,
                    "addComponentScopedSlot",
                    ContextCallArgument.FromSource(1),
                    ContextCallArgument.FromSource(2)),

            "AddComponentParameter" when IsAddComponentParameterMethod(method) &&
                                        IsRenderFragmentComponentParameterValue(operation)
                => BuildRenderContextCall(
                    operation,
                    instance,
                    arguments,
                    "addComponentSlot",
                    ContextCallArgument.FromSource(1),
                    ContextCallArgument.FromSource(2)),

            "AddComponentParameter" when IsAddComponentParameterMethod(method)
                => BuildRenderContextCall(
                    operation,
                    instance,
                    arguments,
                    "addComponentParameter",
                    ContextCallArgument.FromSource(1),
                    ContextCallArgument.FromSource(2)),

            "SetKey" when IsSetKeyMethod(method)
                => BuildRenderContextCall(operation, instance, arguments, "setKey", ContextCallArgument.FromSource(0)),

            "SetUpdatesAttributeName" when IsSetUpdatesAttributeNameMethod(method)
                => BuildRenderContextCall(operation, instance, arguments, "setUpdatesAttributeName", ContextCallArgument.FromSource(0)),

            "SetAttributeValue" when IsSetAttributeValueMethod(method)
                => BuildRenderContextCall(operation, instance, arguments, "setAttributeValue", ContextCallArgument.FromSource(1)),

            "AddNamedEvent" when IsAddNamedEventMethod(method)
                => BuildRenderContextCall(
                    operation,
                    instance,
                    arguments,
                    "addNamedEvent",
                    ContextCallArgument.FromSource(0),
                    ContextCallArgument.FromSource(1)),

            "AddElementReferenceCapture" when IsAddElementReferenceCaptureMethod(method)
                => BuildRenderContextCall(operation, instance, arguments, "addElementReferenceCapture", ContextCallArgument.FromSource(1)),

            "AddComponentReferenceCapture" when IsAddComponentReferenceCaptureMethod(method)
                => BuildRenderContextCall(operation, instance, arguments, "addComponentReferenceCapture", ContextCallArgument.FromSource(1)),

            "AddComponentRenderMode" when IsAddComponentRenderModeMethod(method)
                => BuildRenderContextCall(operation, instance, arguments, "addComponentRenderMode", ContextCallArgument.FromSource(0)),

            "Clear" when method.Parameters.Length == 0
                => BuildRenderContextCall(operation, instance, arguments, "clear"),

            "GetFrames" when method.Parameters.Length == 0
                => BuildRenderContextCall(operation, instance, arguments, "getFrames"),

            "Dispose" when method.Parameters.Length == 0
                => BuildRenderContextCall(operation, instance, arguments, "dispose"),

            _ => throw CreateUnsupportedException(operation)
        };
    }

    private static bool IsSupportedRenderContextMethod(IInvocationOperation operation)
    {
        var method = operation.TargetMethod.OriginalDefinition;
        return method.Name switch
        {
            "OpenElement" => IsSequenceStringMethod(method, expectedParameterCount: 2),
            "CloseElement" => method.Parameters.Length == 0,
            "OpenRegion" => IsSequenceOnlyMethod(method),
            "CloseRegion" => method.Parameters.Length == 0,
            "OpenComponent" => (IsGenericOpenComponentMethod(method) &&
                                TryResolveComponentImport(operation.TargetMethod, out _, out _)) ||
                               (IsOpenComponentTypeMethod(method) &&
                                TryResolveComponentTypeArgument(operation, out var componentType) &&
                                TryResolveComponentImport(componentType, out _)),
            "CloseComponent" => method.Parameters.Length == 0,
            "AddContent" => IsSupportedAddContentMethod(method) ||
                            IsRenderFragmentAddContentMethod(method) ||
                            IsGenericRenderFragmentAddContentMethod(method) ||
                            IsMarkupStringAddContentMethod(method),
            "AddMarkupContent" => IsAddMarkupContentSignature(method),
            "AddAttribute" => IsAddAttributeWithoutValueMethod(method) ||
                              IsAddAttributeWithValueMethod(method) ||
                              IsAddAttributeFrameMethod(method),
            "AddMultipleAttributes" => IsAddMultipleAttributesMethod(method),
            "AddComponentParameter" => IsAddComponentParameterMethod(method),
            "SetKey" => IsSetKeyMethod(method),
            "SetUpdatesAttributeName" => IsSetUpdatesAttributeNameMethod(method),
            "SetAttributeValue" => IsSetAttributeValueMethod(method),
            "AddNamedEvent" => IsAddNamedEventMethod(method),
            "AddElementReferenceCapture" => IsAddElementReferenceCaptureMethod(method),
            "AddComponentReferenceCapture" => IsAddComponentReferenceCaptureMethod(method),
            "AddComponentRenderMode" => IsAddComponentRenderModeMethod(method),
            "Clear" => method.Parameters.Length == 0,
            "GetFrames" => method.Parameters.Length == 0,
            "Dispose" => method.Parameters.Length == 0,
            _ => false
        };
    }

    private static bool IsRenderTreeBuilderEventModifierMethod(IMethodSymbol method)
    {
        var originalMethod = (method.ReducedFrom ?? method).OriginalDefinition;
        return (string.Equals(originalMethod.Name, "AddEventPreventDefaultAttribute", StringComparison.Ordinal) ||
                string.Equals(originalMethod.Name, "AddEventStopPropagationAttribute", StringComparison.Ordinal)) &&
               string.Equals(
                   originalMethod.ContainingType?.OriginalDefinition.ToDisplayString(Format.NameFormat),
                   RenderTreeBuilderExtensionsMetadataName,
                   StringComparison.Ordinal) &&
               originalMethod.Parameters.Length == 4 &&
               string.Equals(
                   originalMethod.Parameters[0].Type.OriginalDefinition.ToDisplayString(Format.NameFormat),
                   RenderTreeBuilderMetadataName,
                   StringComparison.Ordinal) &&
               IsInt32(originalMethod.Parameters[1].Type) &&
               IsString(originalMethod.Parameters[2].Type) &&
               originalMethod.Parameters[3].Type.SpecialType == SpecialType.System_Boolean;
    }

    private static bool IsRenderTreeBuilderMethod(IMethodSymbol method)
        => string.Equals(
            method.ContainingType?.OriginalDefinition.ToDisplayString(Format.NameFormat),
            RenderTreeBuilderMetadataName,
            StringComparison.Ordinal);

    private static bool IsSequenceStringMethod(IMethodSymbol method, int expectedParameterCount)
        => method.Parameters.Length == expectedParameterCount &&
           IsInt32(method.Parameters[0].Type) &&
           IsString(method.Parameters[1].Type);

    private static bool IsSequenceOnlyMethod(IMethodSymbol method)
        => method.Parameters.Length == 1 &&
           IsInt32(method.Parameters[0].Type);

    private static bool IsGenericOpenComponentMethod(IMethodSymbol method)
        => method.IsGenericMethod &&
           method.TypeParameters.Length == 1 &&
           method.Parameters.Length == 1 &&
           IsInt32(method.Parameters[0].Type);

    private static bool IsOpenComponentTypeMethod(IMethodSymbol method)
        => method.Parameters.Length == 2 &&
           IsInt32(method.Parameters[0].Type) &&
           IsSystemType(method.Parameters[1].Type);

    private static bool IsSupportedAddContentMethod(IMethodSymbol method)
    {
        if (method.Parameters.Length != 2 ||
            !IsInt32(method.Parameters[0].Type))
        {
            return false;
        }

        var contentType = method.Parameters[1].Type;
        if (IsRenderFragment(contentType))
            return false;

        return IsString(contentType) ||
               IsObject(contentType);
    }

    private static bool IsRenderFragmentAddContentMethod(IMethodSymbol method)
        => method.Parameters.Length == 2 &&
           IsInt32(method.Parameters[0].Type) &&
           IsRenderFragment(method.Parameters[1].Type);

    private static bool IsGenericRenderFragmentAddContentMethod(IMethodSymbol method)
        => method.Parameters.Length == 3 &&
           IsInt32(method.Parameters[0].Type) &&
           IsGenericRenderFragment(method.Parameters[1].Type);

    private static bool IsMarkupStringAddContentMethod(IMethodSymbol method)
        => method.Parameters.Length == 2 &&
           IsInt32(method.Parameters[0].Type) &&
           IsMarkupStringLike(method.Parameters[1].Type);

    private static bool IsRenderFragmentComponentParameterValue(IInvocationOperation operation)
        => operation.Arguments.Length == 3 &&
           IsRenderFragmentOperationValue(operation.Arguments[2].Value);

    private static bool IsGenericRenderFragmentComponentParameterValue(IInvocationOperation operation)
        => operation.Arguments.Length == 3 &&
           IsGenericRenderFragmentOperationValue(operation.Arguments[2].Value);

    private static bool IsRenderFragmentOperationValue(IOperation operation)
        => operation switch
        {
            IConversionOperation conversion => IsRenderFragmentOperationValue(conversion.Operand),
            _ => operation.Type is not null && IsRenderFragment(operation.Type)
        };

    private static bool IsGenericRenderFragmentOperationValue(IOperation operation)
        => operation switch
        {
            IConversionOperation conversion => IsGenericRenderFragmentOperationValue(conversion.Operand),
            _ => operation.Type is not null && IsGenericRenderFragment(operation.Type)
        };

    private static bool IsAddMarkupContentSignature(IMethodSymbol method)
        => method.Parameters.Length == 2 &&
           IsInt32(method.Parameters[0].Type) &&
           IsString(method.Parameters[1].Type);

    private static bool IsAddAttributeWithoutValueMethod(IMethodSymbol method)
        => method.Parameters.Length == 2 &&
           IsInt32(method.Parameters[0].Type) &&
           IsString(method.Parameters[1].Type);

    private static bool IsAddAttributeWithValueMethod(IMethodSymbol method)
        => method.Parameters.Length == 3 &&
           IsInt32(method.Parameters[0].Type) &&
           IsString(method.Parameters[1].Type);

    private static bool IsAddAttributeFrameMethod(IMethodSymbol method)
        => method.Parameters.Length == 2 &&
           IsInt32(method.Parameters[0].Type) &&
           IsRenderTreeFrame(method.Parameters[1].Type);

    private static bool IsAddMultipleAttributesMethod(IMethodSymbol method)
        => method.Parameters.Length == 2 &&
           IsInt32(method.Parameters[0].Type) &&
           IsEnumerableOfStringObjectKeyValuePair(method.Parameters[1].Type);

    private static bool IsAddComponentParameterMethod(IMethodSymbol method)
        => method.Parameters.Length == 3 &&
           IsInt32(method.Parameters[0].Type) &&
           IsString(method.Parameters[1].Type) &&
           IsObject(method.Parameters[2].Type);

    private static bool IsSetKeyMethod(IMethodSymbol method)
        => method.Parameters.Length == 1 &&
           IsObject(method.Parameters[0].Type);

    private static bool IsSetUpdatesAttributeNameMethod(IMethodSymbol method)
        => method.Parameters.Length == 1 &&
           IsString(method.Parameters[0].Type);

    private static bool IsSetAttributeValueMethod(IMethodSymbol method)
        => method.Parameters.Length == 2 &&
           IsInt32(method.Parameters[0].Type) &&
           IsObject(method.Parameters[1].Type);

    private static bool IsAddNamedEventMethod(IMethodSymbol method)
        => method.Parameters.Length == 2 &&
           IsString(method.Parameters[0].Type) &&
           IsString(method.Parameters[1].Type);

    private static bool IsAddElementReferenceCaptureMethod(IMethodSymbol method)
        => method.Parameters.Length == 2 &&
           IsInt32(method.Parameters[0].Type) &&
           IsActionOfElementReference(method.Parameters[1].Type);

    private static bool IsAddComponentReferenceCaptureMethod(IMethodSymbol method)
        => method.Parameters.Length == 2 &&
           IsInt32(method.Parameters[0].Type) &&
           IsActionOfObject(method.Parameters[1].Type);

    private static bool IsAddComponentRenderModeMethod(IMethodSymbol method)
        => method.Parameters.Length == 1 &&
           string.Equals(
               method.Parameters[0].Type.OriginalDefinition.ToDisplayString(Format.NameFormat),
               "Microsoft.AspNetCore.Components.IComponentRenderMode",
               StringComparison.Ordinal);

    private static bool IsInt32(ITypeSymbol type)
        => type.SpecialType == SpecialType.System_Int32;

    private static bool IsString(ITypeSymbol type)
        => type.SpecialType == SpecialType.System_String;

    private static bool IsObject(ITypeSymbol type)
        => type.SpecialType == SpecialType.System_Object;

    private static bool IsSystemType(ITypeSymbol type)
        => string.Equals(
            type.OriginalDefinition.ToDisplayString(Format.NameFormat),
            "System.Type",
            StringComparison.Ordinal);

    private static bool IsRenderTreeBuilder(ITypeSymbol type)
        => string.Equals(
            type.OriginalDefinition.ToDisplayString(Format.NameFormat),
            RenderTreeBuilderMetadataName,
            StringComparison.Ordinal);

    private static bool IsMarkupString(ITypeSymbol type)
        => string.Equals(
            type.OriginalDefinition.ToDisplayString(Format.NameFormat),
            "Microsoft.AspNetCore.Components.MarkupString",
            StringComparison.Ordinal);

    private static bool IsMarkupStringLike(ITypeSymbol type)
        => IsMarkupString(type) ||
           type is INamedTypeSymbol namedType &&
           string.Equals(
               namedType.OriginalDefinition.ToDisplayString(Format.NameFormat),
               "System.Nullable<T>",
               StringComparison.Ordinal) &&
           namedType.TypeArguments.Length == 1 &&
           IsMarkupString(namedType.TypeArguments[0]);

    private static bool IsRenderTreeFrame(ITypeSymbol type)
        => string.Equals(
            type.OriginalDefinition.ToDisplayString(Format.NameFormat),
            "Microsoft.AspNetCore.Components.RenderTree.RenderTreeFrame",
            StringComparison.Ordinal);

    private static bool TryGetStaticMarkupString(IOperation operation, out string? markup)
    {
        markup = null;
        switch (operation)
        {
            case IConversionOperation conversion:
                return TryGetStaticMarkupString(conversion.Operand, out markup);

            case IObjectCreationOperation
            {
                Constructor.Parameters.Length: 1,
                Arguments.Length: 1
            } creation when IsMarkupString(creation.Type!) &&
                            creation.Arguments[0].Value.ConstantValue.HasValue &&
                            creation.Arguments[0].Value.ConstantValue.Value is string value:
                markup = value;
                return true;

            default:
                return false;
        }
    }

    private static bool IsActionOfElementReference(ITypeSymbol type)
        => type is INamedTypeSymbol namedType &&
           IsSystemAction(namedType) &&
           namedType.TypeArguments.Length == 1 &&
           string.Equals(
               namedType.TypeArguments[0].OriginalDefinition.ToDisplayString(Format.NameFormat),
               "Microsoft.AspNetCore.Components.ElementReference",
               StringComparison.Ordinal);

    private static bool IsActionOfObject(ITypeSymbol type)
        => type is INamedTypeSymbol namedType &&
           IsSystemAction(namedType) &&
           namedType.TypeArguments.Length == 1 &&
           IsObject(namedType.TypeArguments[0]);

    private static bool IsSystemAction(INamedTypeSymbol type)
        => string.Equals(type.OriginalDefinition.Name, "Action", StringComparison.Ordinal) &&
           string.Equals(type.OriginalDefinition.ContainingNamespace?.ToDisplayString(), "System", StringComparison.Ordinal) &&
           type.OriginalDefinition.TypeArguments.Length == 1;

    private static bool IsEnumerableOfStringObjectKeyValuePair(ITypeSymbol type)
    {
        if (type is not INamedTypeSymbol namedType)
            return false;

        return IsEnumerableOfStringObjectKeyValuePair(namedType) ||
               namedType.AllInterfaces.Any(IsEnumerableOfStringObjectKeyValuePair);
    }

    private static bool IsEnumerableOfStringObjectKeyValuePair(INamedTypeSymbol type)
        => string.Equals(type.Name, "IEnumerable", StringComparison.Ordinal) &&
           string.Equals(type.ContainingNamespace?.ToDisplayString(), "System.Collections.Generic", StringComparison.Ordinal) &&
           type.TypeArguments.Length == 1 &&
           IsStringObjectKeyValuePair(type.TypeArguments[0]);

    private static bool IsStringObjectKeyValuePair(ITypeSymbol type)
        => type is INamedTypeSymbol namedType &&
           string.Equals(namedType.Name, "KeyValuePair", StringComparison.Ordinal) &&
           string.Equals(namedType.ContainingNamespace?.ToDisplayString(), "System.Collections.Generic", StringComparison.Ordinal) &&
           namedType.TypeArguments.Length == 2 &&
           IsString(namedType.TypeArguments[0]) &&
           IsObject(namedType.TypeArguments[1]);

    private static bool IsRenderFragment(ITypeSymbol type)
    {
        var current = type;
        if (current is INamedTypeSymbol { OriginalDefinition: { } original })
            current = original;

        if (current is not INamedTypeSymbol named ||
            named.TypeKind != TypeKind.Delegate ||
            named.TypeParameters.Length != 0 ||
            !string.Equals(named.Name, "RenderFragment", StringComparison.Ordinal))
        {
            return false;
        }

        return string.Equals(
                   named.ContainingNamespace?.ToDisplayString(),
                   "Microsoft.AspNetCore.Components",
                   StringComparison.Ordinal) ||
               string.Equals(
                   named.ToDisplayString(Format.NameFormat),
                   "Microsoft.AspNetCore.Components.RenderFragment",
                   StringComparison.Ordinal);
    }

    private static bool IsGenericRenderFragment(ITypeSymbol type)
    {
        var current = type;
        if (current is INamedTypeSymbol { OriginalDefinition: { } original })
            current = original;

        if (current is not INamedTypeSymbol named ||
            named.TypeKind != TypeKind.Delegate ||
            named.TypeParameters.Length != 1 ||
            !string.Equals(named.Name, "RenderFragment", StringComparison.Ordinal))
        {
            return false;
        }

        return string.Equals(
                   named.ContainingNamespace?.ToDisplayString(),
                   "Microsoft.AspNetCore.Components",
                   StringComparison.Ordinal) ||
               string.Equals(
                   named.ToDisplayString(Format.NameFormat),
                   "Microsoft.AspNetCore.Components.RenderFragment<TValue>",
                   StringComparison.Ordinal);
    }

    private static Expression BuildOpenComponentCall(
        IInvocationOperation operation,
        SenseArgument argument,
        Expression instance,
        IReadOnlyList<Expression> translatedArguments)
    {
        if (!TryResolveComponentImport(operation.TargetMethod, out var componentImportDescriptor, out var componentType))
            throw CreateUnsupportedException(operation);

        var componentImport = argument.BindImportSpecifier(
            componentImportDescriptor.ImportSpecifier,
            componentImportDescriptor.ExportName);
        var parameterNameMap = BuildComponentParameterNameMapExpression(componentType);
        return parameterNameMap is null
            ? BuildRenderContextCall(
                operation,
                instance,
                translatedArguments,
                "openComponent",
                ContextCallArgument.FromExpression(componentImport))
            : BuildRenderContextCall(
                operation,
                instance,
                translatedArguments,
                "openComponent",
                ContextCallArgument.FromExpression(componentImport),
                ContextCallArgument.FromExpression(parameterNameMap));
    }

    private static Expression BuildOpenComponentTypeCall(
        IInvocationOperation operation,
        SenseArgument argument,
        Expression instance,
        IReadOnlyList<Expression> translatedArguments)
    {
        if (!TryResolveComponentTypeArgument(operation, out var componentType) ||
            !TryResolveComponentImport(componentType, out var componentImportDescriptor))
        {
            throw CreateUnsupportedException(operation);
        }

        var componentImport = argument.BindImportSpecifier(
            componentImportDescriptor.ImportSpecifier,
            componentImportDescriptor.ExportName);
        var parameterNameMap = BuildComponentParameterNameMapExpression(componentType);
        var renderContextArguments = parameterNameMap is null
            ? new[] { ContextCallArgument.FromExpression(componentImport) }
            : new[]
            {
                ContextCallArgument.FromExpression(componentImport),
                ContextCallArgument.FromExpression(parameterNameMap)
            };

        if (CanOmitArgumentEvaluation(operation.Arguments[0].Value))
            return BuildCall(
                instance,
                "openComponent",
                renderContextArguments.Select(argument => argument.Resolve(translatedArguments)).ToArray());

        var receiverParameter = new Identifier("__rtb");
        var sequenceParameter = new Identifier("__arg0");
        var bodyArguments = renderContextArguments
            .Select(argument => argument.Resolve(translatedArguments))
            .ToArray();
        var arrow = new ArrowFunctionExpression(
            NodeList.From<Node>([receiverParameter, sequenceParameter]),
            BuildCall(receiverParameter, "openComponent", bodyArguments),
            expression: true,
            async: false);
        return new CallExpression(
            arrow,
            NodeList.From(new Expression[] { instance, translatedArguments[0] }),
            optional: false);
    }

    private static bool TryResolveComponentTypeArgument(
        IInvocationOperation operation,
        out INamedTypeSymbol componentType)
    {
        componentType = null!;
        if (operation.Arguments.Length != 2)
            return false;

        var value = operation.Arguments[1].Value;
        while (value is IConversionOperation conversion)
            value = conversion.Operand;

        if (TryResolveTypeOfExpression(value, out componentType))
            return true;

        if (value is ILocalReferenceOperation localReference &&
            TryResolveLocalTypeOfInitializer(operation, localReference.Local, out componentType))
        {
            return true;
        }

        return false;
    }

    private static bool TryResolveTypeOfExpression(
        IOperation operation,
        out INamedTypeSymbol componentType)
    {
        componentType = null!;
        while (operation is IConversionOperation conversion)
            operation = conversion.Operand;

        if (operation is not ITypeOfOperation typeOf ||
            typeOf.TypeOperand is not INamedTypeSymbol namedType)
        {
            return false;
        }

        componentType = namedType;
        return true;
    }

    private static bool TryResolveLocalTypeOfInitializer(
        IOperation usage,
        ILocalSymbol local,
        out INamedTypeSymbol componentType)
    {
        componentType = null!;
        var root = usage;
        while (root.Parent is not null)
            root = root.Parent;

        if (ContainsLocalAssignment(root, local))
            return false;

        return TryFindLocalInitializer(root, local, out componentType);
    }

    private static bool TryFindLocalInitializer(
        IOperation operation,
        ILocalSymbol local,
        out INamedTypeSymbol componentType)
    {
        componentType = null!;
        if (operation is IVariableDeclaratorOperation declarator &&
            SymbolEqualityComparer.Default.Equals(declarator.Symbol, local) &&
            declarator.Initializer?.Value is { } initializer)
        {
            return TryResolveTypeOfExpression(initializer, out componentType);
        }

        foreach (var child in operation.ChildOperations)
        {
            if (TryFindLocalInitializer(child, local, out componentType))
                return true;
        }

        return false;
    }

    private static bool ContainsLocalAssignment(IOperation operation, ILocalSymbol local)
    {
        if (operation is ISimpleAssignmentOperation assignment &&
            IsLocalReference(assignment.Target, local))
        {
            return true;
        }

        if (operation is ICompoundAssignmentOperation compoundAssignment &&
            IsLocalReference(compoundAssignment.Target, local))
        {
            return true;
        }

        foreach (var child in operation.ChildOperations)
        {
            if (ContainsLocalAssignment(child, local))
                return true;
        }

        return false;
    }

    private static bool AllLocalReferencesAreOpenComponentTypeArguments(
        IOperation usage,
        ILocalSymbol local)
    {
        var root = usage;
        while (root.Parent is not null)
            root = root.Parent;

        return AllLocalReferencesAreOpenComponentTypeArgumentsCore(root, local);
    }

    private static bool AllLocalReferencesAreOpenComponentTypeArgumentsCore(
        IOperation operation,
        ILocalSymbol local)
    {
        if (operation is ILocalReferenceOperation reference &&
            SymbolEqualityComparer.Default.Equals(reference.Local, local) &&
            !IsOpenComponentTypeArgumentReference(reference))
        {
            return false;
        }

        foreach (var child in operation.ChildOperations)
        {
            if (!AllLocalReferencesAreOpenComponentTypeArgumentsCore(child, local))
                return false;
        }

        return true;
    }

    private static bool IsOpenComponentTypeArgumentReference(ILocalReferenceOperation reference)
    {
        var current = reference.Parent;
        while (current is IConversionOperation)
            current = current.Parent;

        return current is IArgumentOperation { Parent: IInvocationOperation invocation } argument &&
               argument.Parameter is not null &&
               argument.Parameter.Ordinal == 1 &&
               string.Equals(invocation.TargetMethod.OriginalDefinition.Name, "OpenComponent", StringComparison.Ordinal) &&
               IsOpenComponentTypeMethod(invocation.TargetMethod.OriginalDefinition);
    }

    private static bool IsLocalReference(IOperation operation, ILocalSymbol local)
    {
        while (operation is IConversionOperation conversion)
            operation = conversion.Operand;

        return operation is ILocalReferenceOperation reference &&
               SymbolEqualityComparer.Default.Equals(reference.Local, local);
    }

    private static bool TryResolveComponentImport(
        IMethodSymbol openComponentMethod,
        out ComponentImportDescriptor componentImport,
        out INamedTypeSymbol componentType)
    {
        componentImport = default;
        componentType = null!;
        if (openComponentMethod.TypeArguments.Length != 1 ||
            openComponentMethod.TypeArguments[0] is not INamedTypeSymbol resolvedComponentType)
        {
            return false;
        }

        componentType = resolvedComponentType;
        return TryResolveComponentImport(componentType, out componentImport);
    }

    private static bool TryResolveComponentImport(
        INamedTypeSymbol componentType,
        out ComponentImportDescriptor componentImport)
    {
        componentImport = default;
        var exportPath = GetECMAScriptModuleExportPath(componentType);
        if (!string.IsNullOrWhiteSpace(exportPath))
        {
            componentImport = new ComponentImportDescriptor(
                NormalizeModuleImportPath(exportPath!),
                "default");
            return true;
        }

        return TryGetVueLibraryComponentImport(componentType, out componentImport);
    }

    private static string? GetECMAScriptModuleExportPath(INamedTypeSymbol componentType)
    {
        foreach (var attribute in componentType.GetAttributes())
        {
            if (!string.Equals(
                    attribute.AttributeClass?.ToDisplayString(),
                    ECMAScriptModuleAttributeMetadataName,
                    StringComparison.Ordinal))
            {
                continue;
            }

            if (attribute.ConstructorArguments.Length == 1 &&
                attribute.ConstructorArguments[0].Value is string exportPath &&
                !string.IsNullOrWhiteSpace(exportPath))
            {
                return exportPath;
            }
        }

        return null;
    }

    private static bool TryGetVueLibraryComponentImport(
        INamedTypeSymbol componentType,
        out ComponentImportDescriptor componentImport)
    {
        componentImport = default;
        foreach (var attribute in componentType.GetAttributes())
        {
            if (!string.Equals(
                    attribute.AttributeClass?.ToDisplayString(),
                    VueLibraryComponentAttributeMetadataName,
                    StringComparison.Ordinal))
            {
                continue;
            }

            if (attribute.ConstructorArguments.Length != 2 ||
                attribute.ConstructorArguments[0].Value is not string importSpecifier ||
                attribute.ConstructorArguments[1].Value is not string exportName ||
                string.IsNullOrWhiteSpace(importSpecifier) ||
                string.IsNullOrWhiteSpace(exportName))
            {
                return false;
            }

            componentImport = new ComponentImportDescriptor(
                importSpecifier.Trim(),
                exportName.Trim());
            return true;
        }

        return false;
    }

    private static string NormalizeModuleImportPath(string path)
        => ECMAScriptModulePath.NormalizeRootRelativeImportSpecifier(path);

    private static ObjectExpression? BuildComponentParameterNameMapExpression(INamedTypeSymbol componentType)
    {
        var names = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var attribute in componentType.GetAttributes())
        {
            var attributeName = attribute.AttributeClass?.ToDisplayString();
            if (string.Equals(attributeName, VuePropAttributeMetadataName, StringComparison.Ordinal))
            {
                AddDescriptorName(attribute, names, listener: false);
                continue;
            }

            if (string.Equals(attributeName, VueLibraryEmitAttributeMetadataName, StringComparison.Ordinal))
            {
                AddDescriptorName(attribute, names, listener: true);
                continue;
            }

            if (string.Equals(attributeName, VueSlotAttributeMetadataName, StringComparison.Ordinal))
                AddSlotDescriptorName(attribute, names);
        }

        if (names.Count == 0)
            return null;

        var properties = names.Select(static pair => (Node)new ObjectProperty(
            PropertyKind.Init,
            key: CreateStringLiteral(pair.Key),
            value: CreateStringLiteral(pair.Value),
            computed: false,
            shorthand: false,
            method: false));
        return new ObjectExpression(NodeList.From(properties));
    }

    private static void AddDescriptorName(
        AttributeData attribute,
        SortedDictionary<string, string> names,
        bool listener)
    {
        if (attribute.ConstructorArguments.Length == 0 ||
            attribute.ConstructorArguments[0].Value is not string publicName ||
            string.IsNullOrWhiteSpace(publicName))
        {
            return;
        }

        var name = GetNamedString(attribute, "Name");
        if (string.IsNullOrWhiteSpace(name))
            return;

        names[publicName] = listener
            ? ToVueListenerPropName(name!)
            : name!;
    }

    private static void AddSlotDescriptorName(
        AttributeData attribute,
        SortedDictionary<string, string> names)
    {
        if (attribute.ConstructorArguments.Length == 0 ||
            attribute.ConstructorArguments[0].Value is not string publicName ||
            string.IsNullOrWhiteSpace(publicName))
        {
            return;
        }

        if (GetNamedBoolean(attribute, "PatternOnly") == true)
            return;

        var name = GetNamedBoolean(attribute, "IsDefault") == true
            ? "default"
            : GetNamedString(attribute, "Name");
        if (string.IsNullOrWhiteSpace(name))
            return;

        names[publicName] = name!;
    }

    private static string? GetNamedString(AttributeData attribute, string name)
    {
        foreach (var argument in attribute.NamedArguments)
        {
            if (string.Equals(argument.Key, name, StringComparison.Ordinal) &&
                argument.Value.Value is string value)
            {
                return value.Trim();
            }
        }

        return null;
    }

    private static bool? GetNamedBoolean(AttributeData attribute, string name)
    {
        foreach (var argument in attribute.NamedArguments)
        {
            if (string.Equals(argument.Key, name, StringComparison.Ordinal) &&
                argument.Value.Value is bool value)
            {
                return value;
            }
        }

        return null;
    }

    private static string ToVueListenerPropName(string eventName)
    {
        if (eventName.Length == 0)
            return eventName;

        return eventName.StartsWith("on", StringComparison.Ordinal) &&
               eventName.Length > 2 &&
               char.IsUpper(eventName[2])
            ? eventName
            : "on" + char.ToUpperInvariant(eventName[0]) + eventName.Substring(1);
    }

    private static StringLiteral CreateStringLiteral(string value)
        => JavaScriptAstFactory.CreateStringLiteral(value);

    private static Expression BuildRenderFragmentInvoke(
        IInvocationOperation operation,
        Expression receiver,
        IReadOnlyList<Expression> translatedArguments)
    {
        // RenderFragment is Action<RenderTreeBuilder>: invoke the fragment against the current builder.
        // Sequence is erased when constant; otherwise preserve single evaluation of receiver/sequence/fragment.
        if (CanUseDirectRenderContextCall(operation, [ContextCallArgument.FromSource(1)]))
        {
            var fragment = ContextCallArgument.FromSource(1).Resolve(translatedArguments);
            return new CallExpression(fragment, NodeList.From(new Expression[] { receiver }), optional: true);
        }

        var receiverParameter = new Identifier("__rtb");
        var parameterNodes = new List<Node>(translatedArguments.Count + 1)
        {
            receiverParameter
        };
        var invocationArguments = new List<Expression>(translatedArguments.Count + 1)
        {
            receiver
        };
        var argumentParameters = new Identifier[translatedArguments.Count];
        for (var index = 0; index < translatedArguments.Count; index++)
        {
            var parameter = new Identifier($"__arg{index}");
            argumentParameters[index] = parameter;
            parameterNodes.Add(parameter);
            invocationArguments.Add(translatedArguments[index]);
        }

        var fragmentParameter = argumentParameters[1];
        var arrow = new ArrowFunctionExpression(
            NodeList.From(parameterNodes),
            new CallExpression(fragmentParameter, NodeList.From(new Expression[] { receiverParameter }), optional: true),
            expression: true,
            async: false);
        return new CallExpression(arrow, NodeList.From(invocationArguments), optional: false);
    }

    private static Expression BuildGenericRenderFragmentInvoke(
        IInvocationOperation operation,
        Expression receiver,
        IReadOnlyList<Expression> translatedArguments)
    {
        // RenderFragment<T> is Func<T, RenderFragment>: invoke with the value,
        // then invoke the returned fragment against the current builder.
        if (CanUseDirectRenderContextCall(
                operation,
                [ContextCallArgument.FromSource(1), ContextCallArgument.FromSource(2)]))
        {
            var fragment = ContextCallArgument.FromSource(1).Resolve(translatedArguments);
            var value = ContextCallArgument.FromSource(2).Resolve(translatedArguments);
            var directFragmentFactoryCall = new CallExpression(
                fragment,
                NodeList.From(new[] { value }),
                optional: true);
            return new CallExpression(
                directFragmentFactoryCall,
                NodeList.From(new Expression[] { receiver }),
                optional: true);
        }

        var receiverParameter = new Identifier("__rtb");
        var parameterNodes = new List<Node>(translatedArguments.Count + 1)
        {
            receiverParameter
        };
        var invocationArguments = new List<Expression>(translatedArguments.Count + 1)
        {
            receiver
        };
        var argumentParameters = new Identifier[translatedArguments.Count];
        for (var index = 0; index < translatedArguments.Count; index++)
        {
            var parameter = new Identifier($"__arg{index}");
            argumentParameters[index] = parameter;
            parameterNodes.Add(parameter);
            invocationArguments.Add(translatedArguments[index]);
        }

        var fragmentParameter = argumentParameters[1];
        var valueParameter = argumentParameters[2];
        var fragmentFactoryCall = new CallExpression(
            fragmentParameter,
            NodeList.From(new Expression[] { valueParameter }),
            optional: true);
        var arrow = new ArrowFunctionExpression(
            NodeList.From(parameterNodes),
            new CallExpression(fragmentFactoryCall, NodeList.From(new Expression[] { receiverParameter }), optional: true),
            expression: true,
            async: false);
        return new CallExpression(arrow, NodeList.From(invocationArguments), optional: false);
    }

    private static Expression BuildEventModifierCall(
        IInvocationOperation operation,
        Expression? instance,
        IReadOnlyList<Expression> translatedArguments)
    {
        var renderContextMethod = string.Equals(
            operation.TargetMethod.Name,
            "AddEventPreventDefaultAttribute",
            StringComparison.Ordinal)
            ? "addEventPreventDefaultAttribute"
            : "addEventStopPropagationAttribute";

        if (instance is not null)
        {
            return BuildRenderContextCall(
                operation,
                instance,
                translatedArguments,
                renderContextMethod,
                ContextCallArgument.FromSource(1),
                ContextCallArgument.FromSource(2));
        }

        if (translatedArguments.Count != 4)
            throw CreateUnsupportedException(operation);

        var renderContextArguments =
            new[] { ContextCallArgument.FromSource(2), ContextCallArgument.FromSource(3) };
        if (CanUseDirectRenderContextCall(
                operation,
                [ContextCallArgument.FromSource(0), ..renderContextArguments]))
        {
            return BuildCall(
                translatedArguments[0],
                renderContextMethod,
                renderContextArguments.Select(argument => argument.Resolve(translatedArguments)).ToArray());
        }

        return BuildSingleEvaluationCall(
            translatedArguments,
            receiverSourceIndex: 0,
            renderContextMethod,
            renderContextArguments);
    }

    private static Expression BuildRenderContextCall(
        IInvocationOperation operation,
        Expression receiver,
        IReadOnlyList<Expression> translatedArguments,
        string renderContextMethod,
        params ContextCallArgument[] renderContextArguments)
    {
        if (CanUseDirectRenderContextCall(operation, renderContextArguments))
        {
            return BuildCall(
                receiver,
                renderContextMethod,
                renderContextArguments.Select(argument => argument.Resolve(translatedArguments)).ToArray());
        }

        return BuildSingleEvaluationCall(
            receiver,
            translatedArguments,
            renderContextMethod,
            renderContextArguments);
    }

    private static Expression BuildSingleEvaluationCall(
        IReadOnlyList<Expression> translatedArguments,
        int receiverSourceIndex,
        string renderContextMethod,
        IReadOnlyList<ContextCallArgument> renderContextArguments)
    {
        var parameterNodes = new List<Node>(translatedArguments.Count);
        var invocationArguments = new List<Expression>(translatedArguments.Count);
        var argumentParameters = new Identifier[translatedArguments.Count];
        for (var index = 0; index < translatedArguments.Count; index++)
        {
            var parameter = new Identifier($"__arg{index}");
            argumentParameters[index] = parameter;
            parameterNodes.Add(parameter);
            invocationArguments.Add(translatedArguments[index]);
        }

        var bodyArguments = renderContextArguments
            .Select(argument => argument.Resolve(argumentParameters))
            .ToArray();
        var arrow = new ArrowFunctionExpression(
            NodeList.From(parameterNodes),
            BuildCall(argumentParameters[receiverSourceIndex], renderContextMethod, bodyArguments),
            expression: true,
            async: false);
        return new CallExpression(arrow, NodeList.From(invocationArguments), optional: false);
    }

    private static bool CanUseDirectRenderContextCall(
        IInvocationOperation operation,
        IReadOnlyCollection<ContextCallArgument> renderContextArguments)
    {
        var usedArgumentIndexes = new HashSet<int>(
            renderContextArguments
                .Where(static argument => argument.SourceIndex >= 0)
                .Select(static argument => argument.SourceIndex));

        for (var index = 0; index < operation.Arguments.Length; index++)
        {
            if (usedArgumentIndexes.Contains(index))
                continue;

            if (!CanOmitArgumentEvaluation(operation.Arguments[index].Value))
                return false;
        }

        return true;
    }

    private static bool CanOmitArgumentEvaluation(IOperation operation)
        => operation.ConstantValue.HasValue ||
           operation.Kind == OperationKind.TypeOf;

    private static Expression BuildSingleEvaluationCall(
        Expression receiver,
        IReadOnlyList<Expression> translatedArguments,
        string renderContextMethod,
        IReadOnlyList<ContextCallArgument> renderContextArguments)
    {
        var receiverParameter = new Identifier("__rtb");
        var parameterNodes = new List<Node>(translatedArguments.Count + 1)
        {
            receiverParameter
        };
        var invocationArguments = new List<Expression>(translatedArguments.Count + 1)
        {
            receiver
        };
        var argumentParameters = new Identifier[translatedArguments.Count];
        for (var index = 0; index < translatedArguments.Count; index++)
        {
            var parameter = new Identifier($"__arg{index}");
            argumentParameters[index] = parameter;
            parameterNodes.Add(parameter);
            invocationArguments.Add(translatedArguments[index]);
        }

        var bodyArguments = renderContextArguments
            .Select(argument => argument.Resolve(argumentParameters))
            .ToArray();
        var arrow = new ArrowFunctionExpression(
            NodeList.From(parameterNodes),
            BuildCall(receiverParameter, renderContextMethod, bodyArguments),
            expression: true,
            async: false);
        return new CallExpression(arrow, NodeList.From(invocationArguments), optional: false);
    }

    private static CallExpression BuildCall(
        Expression receiver,
        string renderContextMethod,
        IReadOnlyList<Expression> arguments)
        => new(
            new MemberExpression(receiver, new Identifier(renderContextMethod), computed: false, optional: false),
            NodeList.From(arguments),
            optional: false);

    private static OperationTransformationException CreateUnsupportedException(IInvocationOperation operation)
    {
        var signature = operation.TargetMethod.OriginalDefinition.ToDisplayString(Format.NameFormat);
        var detail = string.Empty;
        if (string.Equals(operation.TargetMethod.Name, "OpenComponent", StringComparison.Ordinal) &&
            operation.TargetMethod.TypeArguments.Length == 1)
        {
            var componentType = operation.TargetMethod.TypeArguments[0]
                .OriginalDefinition
                .ToDisplayString(Format.NameFormat);
            detail =
                " Component type '" +
                componentType +
                "' must declare [ECMAScriptModule(\"./path\")] for local RazorVue module lowering or [VueLibraryComponent(\"package\", \"Export\")] for Vue library component lowering.";
        }

        return new OperationTransformationException(
            operation,
            "RenderTreeBuilder method '" +
            signature +
                "' is not supported by render-context v1 lowering. Supported v1 methods: OpenElement(int, string), CloseElement(), OpenRegion(int), CloseRegion(), OpenComponent<T>(int) and OpenComponent(int, typeof(T)) for [ECMAScriptModule] components, CloseComponent(), AddContent(int, string/object/RenderFragment/MarkupString), AddContent<TValue>(int, RenderFragment<TValue>, TValue), AddMarkupContent(int, string), AddAttribute(int, string[, value]), AddAttribute(int, RenderTreeFrame), AddMultipleAttributes(int, IEnumerable<KeyValuePair<string, object>>), AddComponentParameter(int, string, object) including RenderFragment and RenderFragment<T> slot parameters, SetKey(object), SetUpdatesAttributeName(string), SetAttributeValue(int, object), AddNamedEvent(string, string), AddElementReferenceCapture(int, Action<ElementReference>), AddComponentReferenceCapture(int, Action<object>), AddComponentRenderMode(IComponentRenderMode), Clear(), GetFrames(), Dispose(), and RenderTreeBuilder(). Dynamic Type OpenComponent remains unsupported." +
            detail);
    }

    private readonly struct ContextCallArgument
    {
        private readonly Expression? _expression;

        private ContextCallArgument(int sourceIndex, Expression? expression)
        {
            SourceIndex = sourceIndex;
            _expression = expression;
        }

        public int SourceIndex { get; }

        public static ContextCallArgument FromSource(int sourceIndex)
            => new(sourceIndex, null);

        public static ContextCallArgument FromExpression(Expression expression)
            => new(-1, expression);

        public Expression Resolve(IReadOnlyList<Expression> translatedArguments)
            => SourceIndex >= 0
                ? translatedArguments[SourceIndex]
                : _expression ?? throw new InvalidOperationException("Render-context synthetic argument was missing.");

        public Expression Resolve(IReadOnlyList<Identifier> argumentParameters)
            => SourceIndex >= 0
                ? argumentParameters[SourceIndex]
                : _expression ?? throw new InvalidOperationException("Render-context synthetic argument was missing.");
    }

    private readonly record struct ComponentImportDescriptor(
        string ImportSpecifier,
        string ExportName);
}
