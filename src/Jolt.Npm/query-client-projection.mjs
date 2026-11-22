export function readQueryFetch(ts, checker, calls, promise, definition, readKey) {
    if (!promise)
        return;

    const branches = type => type.isUnion() ? type.types : [type];
    const unconstrained = type => !checker.getBaseConstraintOfType(type) &&
        !type.symbol?.declarations?.some(declaration => declaration.constraint);
    const promiseOf = (type, element) => type.target?.getSymbol() === promise &&
        checker.getTypeArguments(type)[0] === element;
    const typeOf = symbol => checker.getTypeOfSymbol(symbol);
    for (const [index, call] of calls.entries()) {
        if (call.parameters.length !== 1 || call.minArgumentCount !== 1 ||
            call.thisParameter || checker.hasEffectiveRestParameter(call))
            continue;
        const result = call.getReturnType();
        if (result.target?.getSymbol() !== promise)
            continue;
        const [element] = checker.getTypeArguments(result);
        if (!call.typeParameters?.includes(element) || !unconstrained(element))
            continue;

        const options = typeOf(call.parameters[0]);
        // A plain options overload establishes the object contract independently
        // of the broader MaybeRefDeep and getter overloads.
        if (options.isUnion() || options.getCallSignatures().length ||
            checker.getIndexInfosOfType(options).length)
            continue;
        const properties = options.getProperties();
        const keys = properties.filter(property => !(property.flags & ts.SymbolFlags.Optional) &&
            checker.isArrayType(checker.getBaseConstraintOfType(typeOf(property)) ?? typeOf(property)));
        if (keys.length !== 1)
            continue;
        const [key] = keys;
        const keyType = typeOf(key);
        const keySlot = call.typeParameters.includes(keyType) ? keyType :
            keyType.isIntersection() ? keyType.types.find(branch => call.typeParameters.includes(branch)) : undefined;
        // Required mapped properties can add an empty object to remove
        // undefined. A branded key intersection still requires its brand.
        if (keySlot && keyType.isIntersection() && keyType.types.some(branch => branch !== keySlot &&
            (!(branch.flags & ts.TypeFlags.Object) || branch.getProperties().length ||
                branch.getCallSignatures().length || branch.getConstructSignatures().length ||
                checker.getIndexInfosOfType(branch).length)))
            continue;
        const projectedKey = readKey(keySlot ? keySlot.getDefault() : keyType,
            keySlot ? checker.getBaseConstraintOfType(keySlot) : keyType);
        if (!projectedKey)
            continue;
        const queries = properties.filter(property => branches(typeOf(property)).some(branch =>
            branch.getCallSignatures().some(callback => callback.parameters.length <= 1 && branches(callback.getReturnType()).some(outcome =>
                outcome.target?.getSymbol() === promise))));
        if (queries.length !== 1)
            continue;
        const [query] = queries;
        if (properties.some(property => property !== key && property !== query &&
            !(property.flags & ts.SymbolFlags.Optional)))
            continue;
        // A preceding overload with no distinguishing required member may
        // intercept this exact two-property call.
        if (calls.slice(0, index).some(earlier => earlier.minArgumentCount <= 1 && earlier.parameters.length &&
            branches(typeOf(earlier.parameters[0])).some(branch => !branch.getProperties().some(property =>
                !(property.flags & ts.SymbolFlags.Optional) && property.name !== key.name && property.name !== query.name))))
            continue;
        const variants = branches(typeOf(query));
        const callable = variants.filter(branch => branch.getCallSignatures().length);
        if (callable.length !== 1 || variants.some(branch => branch !== callable[0] &&
            !(branch.flags & (ts.TypeFlags.Undefined | ts.TypeFlags.UniqueESSymbol))))
            continue;
        const callbacks = callable[0].getCallSignatures();
        const [callback] = callbacks;
        if (callbacks.length !== 1 || callback.thisParameter || callback.typeParameters?.length ||
            checker.hasEffectiveRestParameter(callback) || callable[0].getConstructSignatures().length ||
            checker.getIndexInfosOfType(callable[0]).length || callable[0].getProperties().some(property =>
                !(property.flags & ts.SymbolFlags.Optional)))
            continue;
        const outcomes = branches(callback.getReturnType());
        const callbackPromise = outcomes.find(branch => branch.target?.getSymbol() === promise);
        const callbackElement = callbackPromise && checker.getTypeArguments(callbackPromise)[0];
        if (!call.typeParameters.includes(callbackElement) || !unconstrained(callbackElement) ||
            element !== callbackElement && element.getDefault() !== callbackElement)
            continue;
        // The same unconstrained slot must flow from a promised callback
        // result to the method's promise, including when it is a value type.
        if (outcomes.some(branch => branch !== callbackElement && !promiseOf(branch, callbackElement)) ||
            call.typeParameters.some(slot => slot !== callbackElement && !slot.getDefault()))
            continue;
        const name = element.symbol.name;
        const returnType = { kind: 'promise', elementType: { kind: 'typeParameter', name } };
        return {
            parameters: [{ name: call.parameters[0].name, optional: false, type: {
                kind: 'object', members: [
                    { name: key.name, type: projectedKey, optional: false, readOnly: true },
                    { name: query.name, type: { kind: 'function', signatures: [{ parameters: [], returnType }] },
                        optional: false, readOnly: true },
                ],
            } }],
            returnType,
            typeParameters: [name],
            definition: definition(call.declaration),
        };
    }
}
