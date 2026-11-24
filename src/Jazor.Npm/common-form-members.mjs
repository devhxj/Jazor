export function readCommonFormMembers(ts, checker, result, concrete, claimed, {
    pathRoute, veeValidatePath, arrayIndices, vueRef, member, callable, isInputErrorMap, input,
}) {
    const string = { kind: 'string' };
    const nothing = { kind: 'void' };
    const scalar = type => type.flags === ts.TypeFlags.String ? string :
        type === checker.getBooleanType() || type.flags === ts.TypeFlags.Boolean ? { kind: 'boolean' } :
        type.flags === ts.TypeFlags.Number ? { kind: 'number' } : undefined;
    const textArray = type => checker.isArrayType(type) &&
        checker.getElementTypeOfArrayType(type) === checker.getStringType();
    const arrayOfStrings = { kind: 'array', elementType: string };
    const messageType = { kind: 'union', variants: [arrayOfStrings, string] };
    const field = { kind: 'typeParameter', name: 'TField' };
    const resolve = node => {
        const symbol = checker.getSymbolAtLocation(node);
        return symbol?.flags & ts.SymbolFlags.Alias ? checker.getAliasedSymbol(symbol) : symbol;
    };
    function matchesPathMap(type, resolved) {
        const [map] = type.aliasTypeArguments ?? [];
        const [root, entry] = map?.aliasTypeArguments ?? [];
        if (!pathRoute?.aliasSymbol || !root || !resolved ||
            !isInputErrorMap(resolved, value => value.isUnion() &&
                value.types.length === 2 && value.types.includes(checker.getStringType()) && value.types.some(textArray)) ||
            !entry?.isUnion() || entry.types.length !== 3 ||
            !entry.types.some(arm => arm.flags === ts.TypeFlags.Undefined))
            return false;
        const alias = map.aliasSymbol?.declarations?.find(ts.isTypeAliasDeclaration);
        const outer = type.aliasSymbol?.declarations?.find(ts.isTypeAliasDeclaration);
        if (!alias || !outer || alias.typeParameters?.length !== 2 ||
            !ts.isMappedTypeNode(alias.type) || !ts.isMappedTypeNode(outer.type) ||
            !outer.type.questionToken || outer.type.questionToken.kind === ts.SyntaxKind.MinusToken ||
            alias.type.nameType || outer.type.nameType)
            return false;
        const key = alias.type.typeParameter.constraint;
        const value = alias.type.type;
        const outerKey = outer.type.typeParameter.constraint;
        const outerValue = outer.type.type;
        return key && ts.isTypeReferenceNode(key) && resolve(key.typeName) === pathRoute.aliasSymbol &&
            key.typeArguments?.length === 1 && ts.isTypeReferenceNode(key.typeArguments[0]) &&
            resolve(key.typeArguments[0].typeName) === resolve(alias.typeParameters[0].name) &&
            value && ts.isTypeReferenceNode(value) && resolve(value.typeName) === resolve(alias.typeParameters[1].name) &&
            outerKey && ts.isTypeOperatorNode(outerKey) && outerKey.operator === ts.SyntaxKind.KeyOfKeyword &&
            outerValue && ts.isIndexedAccessTypeNode(outerValue) &&
            checker.getTypeAtLocation(outerKey.type) === checker.getTypeAtLocation(outerValue.objectType) &&
            ts.isTypeReferenceNode(outerValue.indexType) &&
            resolve(outerValue.indexType.typeName) === resolve(outer.type.typeParameter.name);
    }
    function readObject(type, input = false) {
        if (!(type.flags & ts.TypeFlags.Object) || type.getCallSignatures().length ||
            type.getConstructSignatures().length || checker.getIndexInfosOfType(type).length)
            return;
        const properties = type.getProperties().flatMap(property => {
            const optional = Boolean(property.flags & ts.SymbolFlags.Optional);
            const propertyType = checker.getTypeOfPropertyOfType(type, property.name);
            const nullable = propertyType.isUnion() && propertyType.types.some(arm => arm.flags === ts.TypeFlags.Null);
            const value = scalar(optional && !nullable ? checker.getNonNullableType(propertyType) : propertyType);
            return value ? [member(property, value, optional)] : [];
        });
        if (properties.length && (!input || type.getProperties().every(property =>
            property.flags & ts.SymbolFlags.Optional || properties.some(projected => projected.name === property.name))))
            return { kind: 'object', members: properties };
    }
    return result.getProperties().flatMap(symbol => {
        if (claimed.has(symbol.name) || symbol.flags & ts.SymbolFlags.Optional)
            return [];
        const type = checker.getTypeOfPropertyOfType(result, symbol.name);
        const value = scalar(type);
        if (value)
            return [member(symbol, value)];
        const ref = vueRef(type);
        const concreteRef = vueRef(checker.getTypeOfPropertyOfType(concrete, symbol.name));
        if (ref && (concreteRef?.canonical || ref.canonical || ref.name === 'VueComputedRef')) {
            // Inherited refs can retain a partially instantiated Vue type.
            // Read the declared payload so marker keys and scalar field types
            // from the concrete form probe cannot leak into this view.
            const annotation = symbol.declarations?.find(ts.isPropertySignature)?.type;
            const declared = annotation && vueRef(checker.getTypeAtLocation(annotation));
            const payload = declared?.element ?? ref.element;
            const mapped = payload.objectFlags & ts.ObjectFlags.Mapped;
            const elementType = scalar(payload) ?? (!mapped && readObject(payload));
            return elementType ? [member(symbol, { kind: 'ref', name: ref.name, readOnly: true, elementType })] : [];
        }
        const [call, ...other] = type.getCallSignatures();
        if (!call || other.length || call.typeParameters?.length || call.thisParameter ||
            checker.hasEffectiveRestParameter(call) || call.getReturnType().flags !== ts.TypeFlags.Void)
            return [];
        const parameters = [];
        let hasPath = false;
        const resolvedCall = checker.getTypeOfPropertyOfType(concrete, symbol.name)?.getCallSignatures()[0];
        for (const [index, slot] of call.parameters.entries()) {
            const optional = index >= call.minArgumentCount;
            const slotType = checker.getTypeOfSymbol(slot);
            const arms = slotType.isUnion() ? slotType.types : [slotType];
            const present = arms.filter(arm => !(arm.flags & ts.TypeFlags.Undefined));
            const argument = optional && !present.some(arm => arm.flags === ts.TypeFlags.Null) ?
                checker.getNonNullableType(slotType) : slotType;
            const route = argument.aliasTypeArguments;
            // Inherited FormActions can retain its own TValues symbol.
            // Instantiate the same method with the form probe to verify its input.
            const resolvedSlot = resolvedCall?.parameters[index];
            const resolved = resolvedSlot && checker.getTypeOfSymbol(resolvedSlot);
            const path = index === 0 && pathRoute?.aliasSymbol && argument.aliasSymbol === pathRoute.aliasSymbol &&
                route?.length === 1 && (route[0] === pathRoute.aliasTypeArguments?.[0] ||
                    resolved?.isStringLiteral() && resolved.value === 'joltInput');
            const message = hasPath && present.length === 2 && present.includes(checker.getStringType()) && present.some(textArray);
            const pathMap = index === 0 && !optional && matchesPathMap(argument, resolved);
            // The mapped bag and the path parameters share Path<TInput>.
            const shape = path ? { kind: 'propertyPath', elementType: input, variants: [field], veeValidatePath, arrayIndices } :
                pathMap ? { kind: 'formPathMap', typeArguments: [input],
                    elementType: { kind: 'undefinedable', elementType: messageType }, veeValidatePath, arrayIndices } :
                scalar(argument) ?? (optional ? readObject(argument, true) : undefined) ??
                (message ? (!optional && present.length !== arms.length ?
                    { kind: 'undefinedable', elementType: messageType } : messageType) : undefined);
            if (!shape)
                break;
            parameters.push({ name: slot.name, type: shape, optional });
            hasPath ||= path;
        }
        if (parameters.length < call.minArgumentCount)
            return [];
        const method = callable('method', parameters, nothing, call.declaration);
        if (hasPath)
            method.signatures[0].typeParameters = [field.name];
        return [member(symbol, method)];
    });
}
