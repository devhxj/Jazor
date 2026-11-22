using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using System.Text.RegularExpressions;

namespace Jazor.Compiler;

/// <summary>Validates a Roslyn selector for a compile-time JavaScript property path.</summary>
public static class PropertyPathSelector
{
    private const string AttributeName = "ECMAScript.ECMAScriptPropertyPathAttribute";

    public const string DirectCallRequired = "A JavaScript property path parameter requires a direct call with an inline selector.";

    public static bool IsPathParameter(IParameterSymbol parameter) => parameter.GetAttributes()
        .Any(IsSelectorAttribute);

    private static bool IsSelectorAttribute(AttributeData attribute) =>
        Util.IsAttribute(attribute, AttributeName);

    internal static bool IsSingleKeyParameter(IParameterSymbol parameter) =>
        IsSingleKey(parameter.GetAttributes().FirstOrDefault(IsSelectorAttribute));

    private static bool IsSingleKey(AttributeData? attribute) =>
        attribute?.ConstructorArguments.FirstOrDefault().Value is true;

    private static bool HasFlag(ImmutableArray<KeyValuePair<string, TypedConstant>> options, string name) =>
        options.Any(option => option.Key == name && option.Value.Value is true);

    /// <summary>Checks a bound argument whose parameter has the property path attribute.</summary>
    public static string? GetError(IArgumentOperation argument) => Read(argument, out _);

    internal static string? Read(IArgumentOperation argument, out string path)
    {
        path = string.Empty;
        // The compiler and analyzer check the attribute before lowering a bound selector.
        var parameter = argument.Parameter!;
        var attribute = parameter.GetAttributes().First(IsSelectorAttribute);
        var singleKey = IsSingleKey(attribute);
        var options = attribute.NamedArguments;
        if (!TrySelect(argument.Value, out var properties))
            return "A JavaScript property path requires a lambda selecting structural record data properties.";
        if (singleKey && properties.Count != 1)
            return "A JavaScript property key requires a lambda selecting exactly one structural record data property.";

        // C# can infer a shared base type from both arguments. The JS setter
        // expects exactly the field type selected from the record.
        var selectedProperty = properties.Last();
        var selectedType = selectedProperty.Type.WithNullableAnnotation(selectedProperty.NullableAnnotation);
        var delegateType = (INamedTypeSymbol)parameter.Type;
        var selector = delegateType.DelegateInvokeMethod!;
        var returnType = selector.ReturnType.WithNullableAnnotation(selector.ReturnNullableAnnotation);
        var invocation = argument.Parent as IInvocationOperation;
        var originalDelegate = parameter.OriginalDefinition.Type as INamedTypeSymbol;
        var slot = originalDelegate?.DelegateInvokeMethod?.ReturnType as ITypeParameterSymbol;
        if (singleKey && invocation is not null)
        {
            var refResult = HasFlag(options, "IndirectRead");
            if ((refResult || SymbolEqualityComparer.Default.Equals(invocation.TargetMethod.OriginalDefinition.ReturnType, slot)) &&
                Util.HasRequiredRead(selectedProperty))
                return $"Read '{selectedProperty.Name}' directly so the JavaScript property can be checked for presence.";
            if (refResult && Util.HasOptionalRead(selectedProperty))
                return $"Read optional property '{selectedProperty.Name}' directly so an undefined value can be projected as null.";
        }
        if (!SymbolEqualityComparer.IncludeNullability.Equals(selectedType, returnType))
            return $"The property selector returns '{selectedType.ToDisplayString()}', but the call expects '{returnType.ToDisplayString()}'.";
        if (HasFlag(options, "RequirePlainRecord") &&
            GetPlainRecordError(selectedType, selectedProperty.Name, new(SymbolEqualityComparer.Default)) is { } invalidMember)
            return $"Passive Vue model property '{invalidMember}' must be part of a plain record with only nested records and string, double, or bool values. Vue unwraps nested refs.";
        if (selectedType.IsReferenceType && selectedType.NullableAnnotation == NullableAnnotation.NotAnnotated &&
            invocation is not null && slot is not null &&
            invocation.Arguments.Any(value => SymbolEqualityComparer.Default.Equals(value.Parameter?.OriginalDefinition.Type, slot) &&
                value.Value.SemanticModel?.GetTypeInfo(value.Value.Syntax).Nullability.FlowState == NullableFlowState.MaybeNull))
            return $"The property '{selectedProperty.Name}' requires a non-null value.";

        if (singleKey)
        {
            path = Util.GetConfigOrSymbolName(selectedProperty);
            return null;
        }

        // GetConfigOrSymbolName falls back to the declared property name for an empty alias.
        var names = properties.Select(Util.GetConfigOrSymbolName).ToArray();
        if (names.Length == 1 && HasFlag(options, "VeeValidatePath"))
        {
            var name = names[0];
            // VeeValidate's literal wrapper strips all brackets. Its ordinary path
            // parser preserves embedded brackets except for numeric index segments.
            if (name.IndexOf('.') >= 0)
            {
                if (name.IndexOf('[') < 0 && name.IndexOf(']') < 0 && !Regex.IsMatch(name, "[\\r\\n\\u2028\\u2029]"))
                {
                    path = $"[{name}]";
                    return null;
                }
            }
            else if (!Regex.IsMatch(name, @"\[[0-9]+\]|^\[[^\r\n\u2028\u2029]+\]$"))
            {
                path = name;
                return null;
            }

            return "This JavaScript property name cannot be represented as a literal form field path.";
        }
        if (names.Any(name => name.IndexOf('.') >= 0 || name.IndexOf('[') >= 0 || name.IndexOf(']') >= 0))
            return "JavaScript property path segments must be nonempty and cannot contain '.', '[' or ']'.";

        path = string.Join(".", names);
        return null;
    }

    private static string? GetPlainRecordError(ITypeSymbol type, string path, HashSet<ITypeSymbol> activeRecords)
    {
        // Ordinary Vue proxies unwrap refs at object keys. A closed structural
        // payload with scalar leaves preserves the C# shape on both read and write.
        if (type is not INamedTypeSymbol { IsRecord: true, IsSealed: true, Arity: 0, TypeKind: TypeKind.Class,
                BaseType.SpecialType: SpecialType.System_Object } record ||
            type.NullableAnnotation == NullableAnnotation.Annotated || !activeRecords.Add(type))
            return path;

        foreach (var member in record.GetMembers())
        {
            if (member is IFieldSymbol { IsStatic: false, IsImplicitlyDeclared: false })
                return path + "." + member.Name;
            if (member is not IPropertySymbol { IsStatic: false, IsImplicitlyDeclared: false } property)
                continue;
            var memberPath = path + "." + property.Name;
            if (!Common.StructuralRecordSupport.IsStructuralRecordMember(property) ||
                Util.GetConfigOrSymbolName(property) is "__proto__" or "__v_isRef" or "__v_raw" or
                    "__v_isShallow" or "__v_isReactive" or "__v_isReadonly" or "__v_skip" ||
                Util.HasRequiredRead(property) || Util.HasOptionalRead(property))
                return memberPath;
            if (property.Type.SpecialType is SpecialType.System_String or SpecialType.System_Double or SpecialType.System_Boolean)
                continue;
            var propertyType = property.Type.WithNullableAnnotation(property.NullableAnnotation);
            if (GetPlainRecordError(propertyType, memberPath, activeRecords) is { } error)
                return error;
        }

        activeRecords.Remove(type);
        return null;
    }

    internal static bool TrySelect(IOperation selector, out Stack<IPropertySymbol> path)
    {
        path = new Stack<IPropertySymbol>();
        if (selector is not IDelegateCreationOperation { Target: IAnonymousFunctionOperation lambda } ||
            lambda.Symbol.Parameters.Length != 1 || lambda.Body.Operations.Length != 1 ||
            lambda.Body.Operations[0] is not IReturnOperation { ReturnedValue: { } value })
            return false;

        while (value is IConversionOperation { IsImplicit: true } conversion)
            value = conversion.Operand;

        while (value is IPropertyReferenceOperation { Instance: { } instance } segment &&
            Common.StructuralRecordSupport.IsStructuralRecordMember(segment.Property))
        {
            path.Push(segment.Property);
            value = instance;
        }

        return path.Count > 0 && value is IParameterReferenceOperation receiver &&
            SymbolEqualityComparer.Default.Equals(receiver.Parameter, lambda.Symbol.Parameters[0]);
    }
}
