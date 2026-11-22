export function readQueryData(ts, checker, calls, definition, readKey) {
    const projections = [];
    const typeOf = symbol => checker.getTypeOfSymbol(symbol);
    const branches = type => type.isUnion() ? type.types : [type];
    // TypeScript encodes its intrinsic NoInfer<T> as a substitution with
    // unknown as the constraint. Other substitutions can narrow the value.
    const unwrap = type => type.flags & ts.TypeFlags.Substitution && type.constraint?.flags === ts.TypeFlags.Unknown
        ? type.baseType : type;
    const [call] = calls;
    if (!call?.typeParameters?.length)
        return;
    const [keySymbol, updater] = call.parameters;
    if (!keySymbol || call.minArgumentCount > 2)
        return;
    const keySlot = typeOf(keySymbol);
    const typeParameters = call.typeParameters;
    const genericKey = typeParameters.includes(keySlot);
    const keyConstraint = genericKey ? checker.getBaseConstraintOfType(keySlot) : undefined;
    const keyType = genericKey ? keyConstraint ?? checker.getUnknownType() : keySlot;
    if (!checker.isArrayType(keyType))
        return;
    const key = readKey(keyType);
    if (!key)
        return;
    const [payload] = typeParameters;
    // This declaration accepts the untagged key. If its payload protocol
    // cannot be proven, projecting a later overlapping overload is unsafe.
    if (payload === keySlot || checker.getBaseConstraintOfType(payload) ||
        payload.symbol.declarations?.some(declaration => declaration.constraint) || call.thisParameter ||
        checker.hasEffectiveRestParameter(call) || ![1, 2].includes(call.minArgumentCount))
        return;
    if (genericKey && keySlot.getDefault() !== keyConstraint)
        return;

    // The projected key uses the untagged array domain. A conditional tag
    // lookup is admissible only when its declared false branch forwards T.
    function forwards(type) {
        type = unwrap(type);
        if (type === payload)
            return true;
        if (typeParameters.includes(type) && type !== keySlot && type.getDefault())
            return forwards(type.getDefault());
        if (!(type.flags & ts.TypeFlags.Conditional) || type.checkType !== keySlot || !type.extendsType)
            return false;
        const requiredTag = type.extendsType.getProperties().some(property =>
            !(property.flags & ts.SymbolFlags.Optional) && !keyType.getProperty(property.name) &&
            property.declarations?.some(({ name }) => name && ts.isComputedPropertyName(name) &&
                checker.getTypeAtLocation(name.expression).flags & ts.TypeFlags.UniqueESSymbol));
        // Align the alias's own false-branch parameter with this instantiation.
        // The payload and key may be declared in either order on the alias.
        const fallback = checker.getTypeAtLocation(type.root.node.falseType);
        const alias = type.aliasSymbol?.declarations?.find(ts.isTypeAliasDeclaration);
        const fallbackIndex = alias?.typeParameters?.findIndex(parameter =>
            checker.getTypeAtLocation(parameter.name) === fallback) ?? -1;
        return requiredTag && !checker.isTypeAssignableTo(keyType, type.extendsType) &&
            type.aliasTypeArguments?.[fallbackIndex] === payload;
    }
    const optionalPayload = type => {
        const values = branches(type);
        return values.length === 2 && values.some(value => value.flags === ts.TypeFlags.Undefined) &&
            values.some(forwards);
    };
    if (!optionalPayload(call.getReturnType()))
        return;
    const name = payload.symbol.name;
    const slot = { kind: 'typeParameter', name };
    const parameters = [{ name: keySymbol.name, optional: false, type: key }];
    if (call.minArgumentCount === 2) {
        const values = branches(typeOf(updater));
        const callbackType = values.find(value => value.getCallSignatures().length === 1);
        const callback = callbackType?.getCallSignatures()[0];
        if (values.length !== 3 || !values.some(value => value.flags === ts.TypeFlags.Undefined) ||
            !values.some(forwards) || !callback || callback.parameters.length !== 1 ||
            callbackType.getProperties().length || callbackType.getConstructSignatures().length ||
            checker.getIndexInfosOfType(callbackType).length ||
            callback.minArgumentCount !== 1 || callback.typeParameters?.length || callback.thisParameter ||
            checker.hasEffectiveRestParameter(callback) ||
            !optionalPayload(typeOf(callback.parameters[0])) || !optionalPayload(callback.getReturnType()))
            return;
        for (const kind of ['string', 'number', 'boolean']) {
            const scalar = { kind };
            projections.push({
                parameters: [...parameters, { name: updater.name, optional: false, type: scalar }],
                returnType: { kind: 'undefinedable', elementType: scalar },
                definition: definition(call.declaration),
            });
        }
        const element = { kind: 'undefinedable', elementType: slot };
        const record = { kind: 'recordTypeParameter', name };
        projections.push({
            parameters: [...parameters, { name: updater.name, optional: false, type: record }],
            returnType: { kind: 'undefinedable', elementType: record },
            typeParameters: [name],
            definition: definition(call.declaration),
        });
        const update = { kind: 'function', signatures: [{
            parameters: [{ name: callback.parameters[0].name, optional: false, type: element }],
            returnType: element,
        }] };
        const updaterParameter = { name: updater.name, optional: false, optionalCallback: true, type: update };
        const location = definition(call.declaration);
        projections.push({ parameters: [...parameters, updaterParameter],
            returnType: element, typeParameters: [name], definition: location });
        const valueUpdater = { ...updaterParameter, type: { ...update, valueTypeParameters: [name] } };
        projections.push({ parameters: [{ ...parameters[0], readOnlyReference: true }, valueUpdater],
            returnType: element, typeParameters: [name], valueTypeParameters: [name], definition: location });
        return projections;
    } else if (call.parameters.length !== 1) {
        return;
    }
    const location = definition(call.declaration);
    const returnType = { kind: 'undefinedable', elementType: slot };
    projections.push({ parameters, returnType, typeParameters: [name], definition: location });
    // Readonly-reference passing differentiates the C# signatures without
    // changing the JavaScript argument list.
    projections.push({ parameters: [{ ...parameters[0], readOnlyReference: true }],
        returnType, typeParameters: [name], valueTypeParameters: [name], definition: location });
    return projections;
}
