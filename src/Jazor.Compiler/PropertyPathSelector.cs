using System.Collections.Immutable;
using System.Globalization;
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
        var arrayIndices = !singleKey && HasFlag(options, "ArrayIndices");
        var segments = new Stack<Segment>();
        var error = WalkSelector(argument.Value, arrayIndices, segment => segments.Push(segment));
        if (error is not null)
            return error;
        if (singleKey && segments.Count != 1)
            return "A JavaScript property key requires a lambda selecting exactly one structural record data property.";

        // C# can infer a shared base type from both arguments. The JS setter
        // expects exactly the field type selected from the record.
        var selected = segments.Last();
        var selectedProperty = selected.Property;
        var selectedType = selected.Type;
        var selectedName = selectedProperty?.Name ?? $"[{selected.Index}]";
        var delegateType = (INamedTypeSymbol)parameter.Type;
        var selector = delegateType.DelegateInvokeMethod!;
        var returnType = selector.ReturnType.WithNullableAnnotation(selector.ReturnNullableAnnotation);
        var invocation = argument.Parent as IInvocationOperation;
        var originalDelegate = parameter.OriginalDefinition.Type as INamedTypeSymbol;
        var slot = originalDelegate?.DelegateInvokeMethod?.ReturnType as ITypeParameterSymbol;
        if (singleKey && selectedProperty is not null && invocation is not null)
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
        var requireRecordView = HasFlag(options, "RequireRecordView");
        var requirePlainRecord = HasFlag(options, "RequirePlainRecord");
        if (requireRecordView || requirePlainRecord)
        {
            var vue = argument.SemanticModel!.Compilation.SourceModule.ReferencedAssemblySymbols
                .FirstOrDefault(assembly => assembly.Identity.Name == "ECMAScript.Vue")
                ?.GetTypeByMetadataName("ECMAScript.Vue");
            var vueProps = vue?.GetTypeMembers("VueProps").SingleOrDefault();
            if (requireRecordView && !IsRecordViewRoot(selectedType, vueProps))
                return $"Vue unwrapped view root '{selectedName}' requires a non-null structural record with ordinary data keys.";
            if (requirePlainRecord &&
                GetStableProxyPayloadError(selectedType, selectedName, vue, vueProps, new(SymbolEqualityComparer.Default), root: true) is { } invalidMember)
                return $"Passive Vue model property '{invalidMember}' must preserve its authored type under Vue's deep proxy reads. Vue unwraps refs at ordinary object keys.";
        }
        if (selectedType.IsReferenceType && selectedType.NullableAnnotation == NullableAnnotation.NotAnnotated &&
            invocation is not null && slot is not null &&
            invocation.Arguments.Any(value => SymbolEqualityComparer.Default.Equals(value.Parameter?.OriginalDefinition.Type, slot) &&
                value.Value.SemanticModel?.GetTypeInfo(value.Value.Syntax).Nullability.FlowState == NullableFlowState.MaybeNull))
            return $"The property '{selectedName}' requires a non-null value.";

        if (singleKey)
        {
            path = Util.GetConfigOrSymbolName(selectedProperty!);
            return null;
        }

        // GetConfigOrSymbolName falls back to the declared property name for an empty alias.
        var veeValidatePath = HasFlag(options, "VeeValidatePath");
        if (segments.Count == 1 && veeValidatePath)
        {
            var name = Util.GetConfigOrSymbolName(segments.Peek().Property!);
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
        var builder = new System.Text.StringBuilder();
        // The indexed path grammar stops before expanding an array already visited.
        var traversedArrays = arrayIndices ? new List<IArrayTypeSymbol>() : null;
        var repeatedArray = false;
        foreach (var segment in segments)
        {
            if (segment.Property is null && repeatedArray)
                return "This indexed path cannot index an array type already traversed by the selector.";
            repeatedArray = false;
            if (traversedArrays is not null && segment.Type is IArrayTypeSymbol array)
            {
                repeatedArray = traversedArrays.Any(previous => SameArrayShape(previous, array, new()));
                traversedArrays.Add(array);
            }
            if (segment.Property is null)
            {
                builder.Append('[').Append(segment.Index).Append(']');
                continue;
            }
            var name = Util.GetConfigOrSymbolName(segment.Property);
            if (name.IndexOf('.') >= 0 || name.IndexOf('[') >= 0 || name.IndexOf(']') >= 0)
                return "JavaScript property path segments must be nonempty and cannot contain '.', '[' or ']'.";
            if (builder.Length > 0)
                builder.Append('.');
            builder.Append(name);
        }
        path = builder.ToString();
        return null;
    }

    private static bool SameArrayShape(ITypeSymbol first, ITypeSymbol second, List<(ITypeSymbol First, ITypeSymbol Second)> active)
    {
        var comparer = SymbolEqualityComparer.IncludeNullability;
        if (comparer.Equals(first, second))
            return true;
        if (first.NullableAnnotation != second.NullableAnnotation)
            return false;
        if (first is IArrayTypeSymbol { IsSZArray: true } left && second is IArrayTypeSymbol { IsSZArray: true } right)
            return SameArrayShape(left.ElementType.WithNullableAnnotation(left.ElementNullableAnnotation),
                right.ElementType.WithNullableAnnotation(right.ElementNullableAnnotation), active);
        // Generic recursive declarations can expand into endlessly distinct
        // constructed symbols. Compare those by their exact type identity.
        if (first is not INamedTypeSymbol { IsRecord: true, Arity: 0 } firstRecord ||
            second is not INamedTypeSymbol { IsRecord: true, Arity: 0 } secondRecord)
            return false;
        if (active.Any(pair => comparer.Equals(pair.First, first) && comparer.Equals(pair.Second, second)))
            return true;

        var firstProperties = DataProperties(firstRecord);
        var secondProperties = DataProperties(secondRecord);
        if (firstProperties is null || secondProperties is null || firstProperties.Count != secondProperties.Count)
            return false;
        // Compare the emitted property names and close recursive pairs as they recur.
        active.Add((first, second));
        var same = firstProperties.All(pair => secondProperties.TryGetValue(pair.Key, out var property) &&
            pair.Value.IsReadOnly == property.IsReadOnly &&
            SameArrayShape(pair.Value.Type.WithNullableAnnotation(pair.Value.NullableAnnotation),
                property.Type.WithNullableAnnotation(property.NullableAnnotation), active));
        active.RemoveAt(active.Count - 1);
        return same;

        static Dictionary<string, IPropertySymbol>? DataProperties(INamedTypeSymbol record)
        {
            var properties = new Dictionary<string, IPropertySymbol>(StringComparer.Ordinal);
            for (var current = record; current is not null && current.SpecialType != SpecialType.System_Object; current = current.BaseType)
            {
                if (current.ToDisplayString() == "ECMAScript.Vue.VueProps")
                    break;
                if (!current.IsRecord)
                    return null;
                foreach (var member in current.GetMembers())
                {
                    if (member.IsImplicitlyDeclared || member.IsStatic ||
                        member is IMethodSymbol { MethodKind: MethodKind.Constructor or MethodKind.PropertyGet or MethodKind.PropertySet })
                        continue;
                    if (member is not IPropertySymbol property || property.DeclaredAccessibility != Accessibility.Public ||
                        !Common.StructuralRecordSupport.IsStructuralRecordMember(property) ||
                        Util.HasOptionalRead(property) || Util.HasRequiredRead(property) ||
                        properties.ContainsKey(Util.GetConfigOrSymbolName(property)))
                        return null;
                    properties.Add(Util.GetConfigOrSymbolName(property), property);
                }
            }
            return properties;
        }
    }

    private static bool IsRecordViewRoot(ITypeSymbol type, INamedTypeSymbol? vueProps)
    {
        if (type.NullableAnnotation != NullableAnnotation.NotAnnotated || !TryGetProxyRecord(type, vueProps, out var record))
            return false;
        var keys = new HashSet<string>(StringComparer.Ordinal);
        foreach (var member in record.GetMembers())
        {
            if (member is IFieldSymbol { IsStatic: false, IsImplicitlyDeclared: false })
                return false;
            if (member is IPropertySymbol { IsStatic: false, IsImplicitlyDeclared: false } property &&
                (!Common.StructuralRecordSupport.IsStructuralRecordMember(property) ||
                    IsVueReservedKey(Util.GetConfigOrSymbolName(property)) || !keys.Add(Util.GetConfigOrSymbolName(property))))
                return false;
        }
        return keys.Count > 0;
    }

    private static bool TryGetProxyRecord(ITypeSymbol type, INamedTypeSymbol? vueProps, out INamedTypeSymbol record)
    {
        record = (type as INamedTypeSymbol)!;
        // VueProps is the empty structural host. Other bases can add data keys
        // beyond the record's declared members.
        return record is { IsRecord: true, IsSealed: true, Arity: 0, TypeKind: TypeKind.Class, BaseType: { } baseType } &&
            (baseType.SpecialType == SpecialType.System_Object || SymbolEqualityComparer.Default.Equals(baseType, vueProps));
    }

    private static bool IsVueReservedKey(string key) =>
        key is "__proto__" or "__v_isRef" or "__v_raw" or "__v_isShallow" or
            "__v_isReactive" or "__v_isReadonly" or "__v_skip";

    private static string? GetStableProxyPayloadError(ITypeSymbol type, string path,
        INamedTypeSymbol? vue, INamedTypeSymbol? vueProps, HashSet<ITypeSymbol> activeRecords, bool root = false, bool arrayElement = false)
    {
        if (type is IArrayTypeSymbol { IsSZArray: true } array)
            return GetStableProxyPayloadError(array.ElementType, path + "[]", vue, vueProps, activeRecords, arrayElement: true);
        if (!root && type is INamedTypeSymbol nullable && nullable.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
            type = nullable.TypeArguments[0];
        if (!root && type.SpecialType is SpecialType.System_String or SpecialType.System_Double or SpecialType.System_Boolean)
            return null;
        // Vue returns the ref itself at an integer array index. This also keeps
        // the authored value of a shallow ref opaque to the surrounding proxy.
        if (arrayElement && type is INamedTypeSymbol reference && IsVueRef(reference, vue))
            return null;
        if (!TryGetProxyRecord(type, vueProps, out var record) || !activeRecords.Add(type))
            return path;

        foreach (var member in record.GetMembers())
        {
            if (member is IFieldSymbol { IsStatic: false, IsImplicitlyDeclared: false })
                return path + "." + member.Name;
            if (member is not IPropertySymbol { IsStatic: false, IsImplicitlyDeclared: false } property)
                continue;
            var memberPath = path + "." + property.Name;
            if (!Common.StructuralRecordSupport.IsStructuralRecordMember(property) ||
                IsVueReservedKey(Util.GetConfigOrSymbolName(property)) ||
                Util.HasRequiredRead(property))
                return memberPath;
            var propertyType = property.Type.WithNullableAnnotation(property.NullableAnnotation);
            if (Util.HasOptionalRead(property) && propertyType.NullableAnnotation != NullableAnnotation.Annotated &&
                (propertyType is not INamedTypeSymbol optional ||
                    optional.OriginalDefinition.SpecialType != SpecialType.System_Nullable_T))
                return memberPath;
            if (GetStableProxyPayloadError(propertyType, memberPath, vue, vueProps, activeRecords) is { } error)
                return error;
        }

        activeRecords.Remove(type);
        return null;
    }

    private static bool IsVueRef(INamedTypeSymbol type, INamedTypeSymbol? vue)
    {
        var definition = type.OriginalDefinition;
        if (vue is null || !SymbolEqualityComparer.Default.Equals(definition.ContainingType, vue))
            return false;
        for (var current = definition; current is not null; current = current.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(current.ContainingType, vue) &&
                current.MetadataName is "IVueRef`1" or "IVueSplitRef`2" or "VueReadonlyRef`1" or
                    "VueReadonlyOptionalRef`1" or "VueOptionalRef`1" or "VueSplitOptionalRef`2")
                return true;
        }
        foreach (var contract in definition.AllInterfaces)
            if (SymbolEqualityComparer.Default.Equals(contract.ContainingType, vue) &&
                contract.MetadataName is "IVueRef`1" or "IVueSplitRef`2")
                return true;
        return false;
    }

    internal static bool TrySelect(IOperation selector, out Stack<IPropertySymbol> path)
    {
        var properties = new Stack<IPropertySymbol>();
        path = properties;
        return WalkSelector(selector, false, segment => properties.Push(segment.Property!)) is null;
    }

    private readonly record struct Segment(IPropertySymbol? Property, string? Index, ITypeSymbol Type);

    private static string? WalkSelector(IOperation selector, bool arrayIndices, Action<Segment> push)
    {
        const string invalidSelector = "A JavaScript property path requires a lambda selecting structural record data properties.";
        if (selector is not IDelegateCreationOperation { Target: IAnonymousFunctionOperation lambda } ||
            lambda.Symbol.Parameters.Length != 1 || lambda.Body.Operations.Length != 1 ||
            lambda.Body.Operations[0] is not IReturnOperation { ReturnedValue: { } value })
            return invalidSelector;

        while (value is IConversionOperation { IsImplicit: true } conversion)
            value = conversion.Operand;

        var rootIsProperty = false;
        while (true)
        {
            if (value is IPropertyReferenceOperation { Instance: { } instance } segment &&
                Common.StructuralRecordSupport.IsStructuralRecordMember(segment.Property))
            {
                var type = segment.Property.Type.WithNullableAnnotation(segment.Property.NullableAnnotation);
                push(new(segment.Property, null, type));
                rootIsProperty = true;
                value = instance;
            }
            else if (value is IArrayElementReferenceOperation element)
            {
                if (!arrayIndices)
                    return "This operation does not support indexed paths.";
                if (element.ArrayReference.Type is not IArrayTypeSymbol { IsSZArray: true } array ||
                    element.Indices.Length != 1 || !element.Indices[0].ConstantValue.HasValue ||
                    !TryFormatIndex(element.Indices[0].ConstantValue.Value, out var text))
                    return "An indexed path requires a nonnegative integral constant JavaScript array index.";
                // Bind only the intrinsic one-dimensional array access. The
                // delegate is erased, so evaluating a custom indexer is unsound.
                push(new(null, text, array.ElementType.WithNullableAnnotation(array.ElementNullableAnnotation)));
                rootIsProperty = false;
                value = element.ArrayReference;
            }
            else
                break;
        }

        return rootIsProperty && value is IParameterReferenceOperation receiver &&
            SymbolEqualityComparer.Default.Equals(receiver.Parameter, lambda.Symbol.Parameters[0]) ? null : invalidSelector;
    }

    private static bool TryFormatIndex(object? value, out string? index)
    {
        // The last uint value is a JS property key, outside the array-index range.
        index = value switch
        {
            int number when number >= 0 => number.ToString(CultureInfo.InvariantCulture),
            uint number when number < uint.MaxValue => number.ToString(CultureInfo.InvariantCulture),
            long number when number >= 0 && number < uint.MaxValue => number.ToString(CultureInfo.InvariantCulture),
            ulong number when number < uint.MaxValue => number.ToString(CultureInfo.InvariantCulture),
            _ => null,
        };
        return index is not null;
    }
}
