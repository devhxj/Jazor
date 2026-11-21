export function readCorrelatedRefCall(checker, signatures, typeOf, vueRef, promise, definition) {
    for (const [ordinal, signature] of signatures.entries()) {
        const sorted = readArrayRef(checker, signature, ordinal, typeOf, vueRef, definition);
        if (sorted)
            return [sorted];
        if (signature.typeParameters?.length !== 1 || signature.minArgumentCount !== 2 ||
            signature.thisParameter || checker.hasEffectiveRestParameter(signature))
            continue;
        const [value] = signature.typeParameters;
        const [evaluate, seed] = signature.parameters;
        if (typeOf(seed) !== value || checker.getBaseConstraintOfType(value))
            continue;

        const callbackType = typeOf(evaluate);
        const [callback] = callbackType.getCallSignatures();
        if (!callback || callbackType.getCallSignatures().length !== 1 || callbackType.getProperties().length ||
            callback.typeParameters?.length || callback.thisParameter || checker.hasEffectiveRestParameter(callback))
            continue;
        const result = callback.getReturnType();
        if (!result.isUnion() || !result.types.some(branch => branch === value) ||
            !result.types.some(branch => branch.target?.getSymbol() === promise && checker.getTypeArguments(branch)[0] === value))
            continue;
        const ref = vueRef(signature.getReturnType());
        if (!ref || ref.element !== value)
            continue;
        // A zero-parameter callback ignores any supplied context. The exact
        // seed/result parameter preserves inference through both C# inputs.
        if (signatures.slice(0, ordinal).some(candidate =>
            candidate.minArgumentCount <= 2 && candidate.parameters.length >= 2))
            continue;
        const elementType = { kind: 'typeParameter', name: value.symbol.name };
        const returnType = { kind: 'ref', readOnly: true, elementType };
        return [{
            parameters: [
                { name: evaluate.name, optional: false, type: { kind: 'function', signatures: [{
                    parameters: [], returnType: { kind: 'promise', elementType },
                }] } },
                { name: seed.name, optional: false, type: elementType },
            ],
            returnType,
            typeParameters: [elementType.name],
            definition: definition(signature.declaration),
        }];
    }
}

export function asyncStateProbes(checker, aliases, callsOf, requested, typeOf, promise) {
    if (!promise)
        return [];
    return aliases.flatMap(alias => {
        if (!requested.has(alias.name))
            return [];
        const [signature] = callsOf(alias);
        if (!signature?.typeParameters?.length || signature.minArgumentCount !== 2 ||
            signature.thisParameter || checker.hasEffectiveRestParameter(signature))
            return [];
        const [value] = signature.typeParameters;
        const [load, initial] = signature.parameters.map(typeOf);
        if (!load.isUnion() || !load.types.some(branch =>
            branch.target?.getSymbol() === promise && checker.getTypeArguments(branch)[0] === value) ||
            !(initial === value || initial.isUnion() && initial.types.includes(value)))
            return [];
        return [false, true].map(options => ({
            name: alias.name, state: true, options,
            expression: `packageExports[${JSON.stringify(alias.name)}]({} as Promise<JoltAsyncStateValue>, {} as JoltAsyncStateValue${options ? ', {}' : ''})`,
        }));
    });
}

export function refUpdaterProbes(checker, aliases, callsOf, requested, typeOf, vueRef) {
    return aliases.flatMap(alias => {
        if (!requested.has(alias.name))
            return [];
        const [signature] = callsOf(alias);
        if (!signature?.typeParameters?.length || signature.minArgumentCount !== 1 ||
            signature.thisParameter || checker.hasEffectiveRestParameter(signature))
            return [];
        const input = typeOf(signature.parameters[0]);
        const ref = !input.isUnion() && vueRef(input);
        const [call] = signature.getReturnType().getCallSignatures();
        if (!ref || ref.readOnly || !signature.typeParameters.includes(ref.element) ||
            call?.parameters.length !== 1 || call.minArgumentCount !== 0 || call.getReturnType() !== ref.element)
            return [];
        const callee = `packageExports[${JSON.stringify(alias.name)}]`;
        return [
            { name: alias.name, updater: true, expression: `${callee}({} as import('vue').Ref<boolean>)` },
            ...[false, true].map(hasSeed => ({ name: alias.name, tuple: true, hasSeed,
                expression: `${callee}(${hasSeed ? 'false as boolean' : ''})` })),
        ];
    });
}

export function readAsyncStateProbes(ts, checker, probes, statements, diagnostics, vueRef, definition) {
    const projected = new Map();
    for (const [index, probe] of probes.entries()) {
        const statement = statements[index];
        if (diagnostics.some(diagnostic => diagnostic.start >= statement.pos && diagnostic.start < statement.end))
            continue;
        const call = checker.getResolvedSignature(statement.declarationList.declarations[0].initializer);
        if (!call?.declaration)
            continue;
        if (probe.tuple) {
            const tuple = call.getReturnType();
            if (!checker.isTupleType(tuple))
                continue;
            const [state, updater] = checker.getTypeArguments(tuple);
            const boolean = checker.getBooleanType();
            const ref = state && vueRef(state);
            const [update] = updater?.getCallSignatures() ?? [];
            if (tuple.target.minLength !== 2 || tuple.target.fixedLength !== 2 ||
                tuple.target.hasRestElement || !ref || ref.readOnly || ref.element !== boolean ||
                !update || update.minArgumentCount !== 0 || update.getReturnType() !== boolean)
                continue;
            const elementType = { kind: 'boolean' };
            const signature = {
                parameters: probe.hasSeed ? [{ name: call.parameters[0].name, optional: false, type: elementType }] : [],
                returnType: { kind: 'tupleView', members: [
                    { name: 'Item1', readOnly: true, type: { kind: 'ref', elementType } },
                    { name: 'Item2', readOnly: true, type: { kind: 'function', signatures: [{ parameters: [], returnType: elementType }] } },
                ] },
                definition: definition(call.declaration),
            };
            const signatures = projected.get(probe.name) ?? [];
            signatures.push(signature);
            projected.set(probe.name, signatures);
            continue;
        }
        if (probe.updater) {
            const boolean = checker.getBooleanType();
            const ref = vueRef(checker.getTypeOfSymbol(call.parameters[0]));
            const [update] = call.getReturnType().getCallSignatures();
            if (!ref || ref.readOnly || ref.element !== boolean || !update || update.getReturnType() !== boolean)
                continue;
            const elementType = { kind: 'boolean' };
            projected.set(probe.name, [{
                parameters: [{ name: call.parameters[0].name, optional: false, type: { kind: 'ref', elementType } }],
                returnType: { kind: 'function', signatures: [{ parameters: [], returnType: elementType }] },
                definition: definition(call.declaration),
            }]);
            continue;
        }
        const marker = checker.resolveName('JoltAsyncStateValue', statement, ts.SymbolFlags.Type, false);
        const value = checker.getDeclaredTypeOfSymbol(marker);
        const elementType = { kind: 'typeParameter', name: 'T' };
        const boolean = checker.getBooleanType();
        const result = call.getReturnType();
        // The return may be thenable. Project its live ref properties and keep
        // the original object, including the upstream then/execute behavior.
        const members = result.getProperties().flatMap(property => {
            if (property.flags & ts.SymbolFlags.Optional)
                return [];
            const original = checker.getTypeOfPropertyOfType(result, property.name);
            const ref = vueRef(original);
            if (ref?.element === value || ref?.element === boolean)
                return [{ name: property.name, readOnly: true, type: {
                    kind: 'ref', readOnly: true, elementType: ref.element === value ? elementType : { kind: 'boolean' },
                } }];
            const [method] = original.getCallSignatures();
            const resolved = method && checker.getPromisedTypeOfPromise(method.getReturnType());
            if (!method || method.thisParameter || method.minArgumentCount || !resolved?.isUnion() ||
                !resolved.types.includes(value) || !resolved.types.every(arm => arm === value || arm.flags & ts.TypeFlags.Undefined))
                return [];
            const returnType = { kind: 'promise', elementType: { kind: 'undefinedable', elementType } };
            const signatures = [{ parameters: [], returnType }];
            const delay = method.parameters[0];
            if (delay && !method.declaration?.parameters[0]?.dotDotDotToken &&
                checker.getTypeOfSymbol(delay).getNonNullableType() === checker.getNumberType())
                signatures.push({ parameters: [{ name: delay.name, type: { kind: 'number' }, optional: false }], returnType });
            return [{ name: property.name, readOnly: true, type: { kind: 'method', signatures } }];
        });
        if (!members.some(member => member.type.kind === 'ref' && member.type.elementType === elementType))
            continue;
        const [load, initial, config] = call.parameters;
        const parameters = [{ name: load.name, optional: false, type: { kind: 'promise', elementType } },
            { name: initial.name, optional: false, type: elementType }];
        if (probe.options) {
            const options = readScalarOptions(ts, checker, config && checker.getTypeOfSymbol(config));
            if (!options)
                continue;
            parameters.push({ name: config.name, optional: false, type: options });
        }
        const signature = {
            parameters,
            returnType: { kind: 'object', members },
            typeParameters: ['T'],
            scalarResultTypeParameter: 'T',
            definition: definition(call.declaration),
        };
        const signatures = projected.get(probe.name) ?? [];
        signatures.push(signature);
        projected.set(probe.name, signatures);
    }
    return projected;
}

function readScalarOptions(ts, checker, type) {
    const options = type?.getNonNullableType();
    const properties = options?.getProperties();
    if (!properties?.length || properties.some(property => !(property.flags & ts.SymbolFlags.Optional)))
        return;
    const members = properties.flatMap(property => {
        const domain = checker.getTypeOfPropertyOfType(options, property.name).getNonNullableType();
        const kind = domain === checker.getNumberType() ? 'number' :
            domain === checker.getBooleanType() ? 'boolean' : domain === checker.getStringType() ? 'string' : undefined;
        return kind ? [{ name: property.name, optional: true, type: { kind } }] : [];
    });
    if (members.length)
        return { kind: 'object', members };
}

function readArrayRef(checker, signature, ordinal, typeOf, vueRef, definition) {
    if (ordinal !== 0 || signature.typeParameters?.length !== 1 || signature.minArgumentCount !== 1 ||
        signature.thisParameter || checker.hasEffectiveRestParameter(signature))
        return;
    const [value] = signature.typeParameters;
    if (checker.getBaseConstraintOfType(value))
        return;
    const [source] = signature.parameters;
    const input = typeOf(source);
    const result = vueRef(signature.getReturnType());
    const array = type => type && checker.isArrayType(type) && checker.getTypeArguments(type)[0] === value;
    if (!array(result?.element) || !input.isUnion() || !input.types.some(array))
        return;
    // The direct array arm passes the caller's array through to the composable.
    const elementType = { kind: 'typeParameter', name: value.symbol.name };
    const type = { kind: 'array', elementType };
    return {
        parameters: [{ name: source.name, optional: false, type }],
        returnType: { kind: 'ref', readOnly: true, elementType: type },
        typeParameters: [elementType.name],
        definition: definition(signature.declaration),
    };
}
