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
    private const string ECMAScriptModuleAttributeMetadataName = "ECMAScript.ECMAScriptModuleAttribute";

    public override Expression? RewriteInvocationPreorder(IInvocationOperation operation, SenseArgument argument)
    {
        if (!IsRenderTreeBuilderMethod(operation.TargetMethod))
            return null;

        if (IsAddComponentParameterMethod(operation.TargetMethod.OriginalDefinition) &&
            IsGenericRenderFragmentComponentParameterValue(operation))
        {
            throw CreateUnsupportedGenericRenderFragmentComponentParameterException(operation);
        }

        if (!IsSupportedRenderContextMethod(operation))
            throw CreateUnsupportedException(operation);

        return null;
    }

    public override Expression? RewriteInvocation(
        IInvocationOperation operation,
        SenseArgument argument,
        Expression? instance,
        IReadOnlyList<Expression> arguments)
    {
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

            "CloseComponent" when method.Parameters.Length == 0
                => BuildRenderContextCall(operation, instance, arguments, "closeComponent"),

            "AddContent" when IsSupportedAddContentMethod(method)
                => BuildRenderContextCall(operation, instance, arguments, "addContent", ContextCallArgument.FromSource(1)),

            "AddContent" when IsRenderFragmentAddContentMethod(method)
                => BuildRenderFragmentInvoke(operation, instance, arguments),

            "AddMarkupContent" when IsSupportedStaticAddMarkupContentMethod(method, operation)
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

            "AddMultipleAttributes" when IsAddMultipleAttributesMethod(method)
                => BuildRenderContextCall(
                    operation,
                    instance,
                    arguments,
                    "addMultipleAttributes",
                    ContextCallArgument.FromSource(1)),

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
            "OpenComponent" => IsGenericOpenComponentMethod(method) &&
                               TryResolveComponentModulePath(operation.TargetMethod, out _),
            "CloseComponent" => method.Parameters.Length == 0,
            "AddContent" => IsSupportedAddContentMethod(method) || IsRenderFragmentAddContentMethod(method),
            "AddMarkupContent" => IsSupportedStaticAddMarkupContentMethod(method, operation),
            "AddAttribute" => IsAddAttributeWithoutValueMethod(method) || IsAddAttributeWithValueMethod(method),
            "AddMultipleAttributes" => IsAddMultipleAttributesMethod(method),
            "AddComponentParameter" => IsAddComponentParameterMethod(method),
            "SetKey" => IsSetKeyMethod(method),
            "SetUpdatesAttributeName" => IsSetUpdatesAttributeNameMethod(method),
            _ => false
        };
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

    private static bool IsSupportedStaticAddMarkupContentMethod(
        IMethodSymbol method,
        IInvocationOperation operation)
    {
        if (!IsAddMarkupContentSignature(method) ||
            operation.Arguments.Length != 2)
        {
            return false;
        }

        // v1 only accepts compile-time constant markup so createStaticVNode can be used safely.
        // Non-constant markup fails in preorder so side-effecting argument expressions are not lowered.
        return operation.Arguments[1].Value.ConstantValue.HasValue &&
               operation.Arguments[1].Value.ConstantValue.Value is string or null;
    }

    private static bool IsAddAttributeWithoutValueMethod(IMethodSymbol method)
        => method.Parameters.Length == 2 &&
           IsInt32(method.Parameters[0].Type) &&
           IsString(method.Parameters[1].Type);

    private static bool IsAddAttributeWithValueMethod(IMethodSymbol method)
        => method.Parameters.Length == 3 &&
           IsInt32(method.Parameters[0].Type) &&
           IsString(method.Parameters[1].Type);

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

    private static bool IsInt32(ITypeSymbol type)
        => type.SpecialType == SpecialType.System_Int32;

    private static bool IsString(ITypeSymbol type)
        => type.SpecialType == SpecialType.System_String;

    private static bool IsObject(ITypeSymbol type)
        => type.SpecialType == SpecialType.System_Object;

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
        // Match non-generic RenderFragment only; RenderFragment<T> stays unsupported for now.
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
        if (!TryResolveComponentModulePath(operation.TargetMethod, out var modulePath))
            throw CreateUnsupportedException(operation);

        var componentImport = argument.BindImportSpecifier(modulePath, "default");
        return BuildRenderContextCall(
            operation,
            instance,
            translatedArguments,
            "openComponent",
            ContextCallArgument.FromExpression(componentImport));
    }

    private static bool TryResolveComponentModulePath(IMethodSymbol openComponentMethod, out string modulePath)
    {
        modulePath = string.Empty;
        if (openComponentMethod.TypeArguments.Length != 1 ||
            openComponentMethod.TypeArguments[0] is not INamedTypeSymbol componentType)
        {
            return false;
        }

        var exportPath = GetECMAScriptModuleExportPath(componentType);
        if (string.IsNullOrWhiteSpace(exportPath))
            return false;

        modulePath = NormalizeModuleImportPath(exportPath!);
        return true;
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

    private static string NormalizeModuleImportPath(string path)
    {
        var normalized = path.Replace('\\', '/').Trim();
        while (normalized.StartsWith("./", StringComparison.Ordinal))
            normalized = normalized.Substring(2);

        normalized = normalized.TrimStart('/');
        var segments = normalized
            .Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(static segment => segment != ".")
            .ToArray();
        if (segments.Length == 0)
            throw new InvalidOperationException("ECMAScriptModule import path cannot be empty.");
        if (segments.Any(static segment => segment == ".."))
            throw new InvalidOperationException("ECMAScriptModule import path cannot escape the output directory.");

        normalized = string.Join("/", segments);
        if (!normalized.EndsWith(".js", StringComparison.OrdinalIgnoreCase) &&
            !normalized.EndsWith(".mjs", StringComparison.OrdinalIgnoreCase))
        {
            normalized += ".mjs";
        }

        // Component modules are emitted relative to the jazor output root; keep a stable relative import form.
        return "./" + normalized;
    }

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
            return new CallExpression(fragment, NodeList.From(new Expression[] { receiver }), optional: false);
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
            new CallExpression(fragmentParameter, NodeList.From(new Expression[] { receiverParameter }), optional: false),
            expression: true,
            async: false);
        return new CallExpression(arrow, NodeList.From(invocationArguments), optional: false);
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
        => operation.ConstantValue.HasValue;

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
                "' must declare [ECMAScriptModule(\"./path\")] with a non-empty export path for default-import child component lowering.";
        }

        return new OperationTransformationException(
            operation.Kind,
            "RenderTreeBuilder method '" +
            signature +
            "' is not supported by render-context v1 lowering. Supported v1 methods: OpenElement(int, string), CloseElement(), OpenRegion(int), CloseRegion(), OpenComponent<T>(int) for [ECMAScriptModule] components, CloseComponent(), AddContent(int, string/object/RenderFragment), constant AddMarkupContent(int, string), AddAttribute(int, string[, value]), AddMultipleAttributes(int, IEnumerable<KeyValuePair<string, object>>), AddComponentParameter(int, string, object) including non-generic RenderFragment slot parameters, SetKey(object), and SetUpdatesAttributeName(string). Dynamic Type OpenComponent, RenderFragment<T>, and reference capture remain unsupported." +
            detail);
    }

    private static OperationTransformationException CreateUnsupportedGenericRenderFragmentComponentParameterException(
        IInvocationOperation operation)
    {
        var parameterNameValue = operation.Arguments.Length > 1
            ? operation.Arguments[1].Value.ConstantValue
            : default;
        var parameterName = parameterNameValue.HasValue &&
                            parameterNameValue.Value is string name
            ? name
            : "<unknown>";
        return new OperationTransformationException(
            operation,
            "RenderTreeBuilder.AddComponentParameter for component slot '" +
            parameterName +
            "' uses RenderFragment<T>, which is not supported by render-context v1. " +
            "Use a non-generic RenderFragment named slot, or wait for typed slot descriptor lowering so slot parameter values can be represented explicitly.");
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
}
