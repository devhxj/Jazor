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
        const configuration = signature.parameters[2] && typeOf(signature.parameters[2]).getNonNullableType();
        const mode = configuration?.getProperties().filter(property => signature.typeParameters.slice(1).some(parameter =>
            checker.getTypeOfPropertyOfType(configuration, property.name)?.getNonNullableType() === parameter &&
            checker.getBaseConstraintOfType(parameter) === checker.getBooleanType()));
        const modeName = mode?.length === 1 ? mode[0].name : undefined;
        const marker = "JoltAsyncStateValue & { joltNested: import('vue').Ref<number> }";
        return ['default', 'options', ...(modeName ? [true, false] : [])].map(variant => {
            const config = variant === 'default' ? '' :
                variant === 'options' ? ', {}' : `, { ${JSON.stringify(modeName)}: ${variant} }`;
            return {
                name: alias.name, state: true, variant, modeName,
                expression: `packageExports[${JSON.stringify(alias.name)}]({} as Promise<${marker}>, {} as ${marker}${config})`,
            };
        });
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

export function readAsyncStateProbes(ts, checker, probes, statements, diagnostics, vueRef, readUnknownRefOutput, definition) {
    const projected = new Map();
    for (const [index, probe] of probes.entries()) {
        const statement = statements[index];
        if (diagnostics.some(diagnostic => diagnostic.start >= statement.pos && diagnostic.start < statement.end))
            continue;
        const invocation = statement.declarationList.declarations[0].initializer;
        const call = checker.getResolvedSignature(invocation);
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
        const value = checker.getTypeAtLocation(invocation.arguments[1]);
        const originalCall = call.target ?? call;
        const originalResult = originalCall.getReturnType();
        const dataSlot = originalCall.typeParameters?.[0];
        const modeSlot = originalCall.typeParameters?.slice(1).find(parameter => probe.modeName &&
            checker.getTypeOfPropertyOfType(checker.getTypeOfSymbol(originalCall.parameters[2]).getNonNullableType(),
                probe.modeName)?.getNonNullableType() === parameter);
        const witness = checker.getTypeOfPropertyOfType(value, 'joltNested');
        // A private marker alone may survive UnwrapRef. A nested Vue ref
        // witnesses whether the state actually preserves the caller's DTO.
        const exactValue = type => type && checker.getTypeOfPropertyOfType(type, 'joltNested') === witness &&
            checker.isTypeAssignableTo(value, type) && checker.isTypeAssignableTo(type, value);
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
            const unknown = readUnknownRefOutput(ref);
            if (unknown)
                return [{ name: property.name, readOnly: true, type: unknown }];
            const same = exactValue(ref?.element) && preservesDataSlot(ts, checker, vueRef,
                originalResult, property.name, dataSlot, modeSlot, probe.variant);
            if (same || ref?.element === boolean)
                return [{ name: property.name, readOnly: true, type: {
                    kind: 'ref', readOnly: true, elementType: same ? elementType : { kind: 'boolean' },
                } }];
            const [method] = original.getCallSignatures();
            const resolved = method && checker.getPromisedTypeOfPromise(method.getReturnType());
            if (!method || method.thisParameter || method.minArgumentCount || !resolved?.isUnion() ||
                !resolved.types.some(exactValue) || !resolved.types.every(arm => exactValue(arm) || arm.flags & ts.TypeFlags.Undefined))
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
        if (probe.variant !== 'default') {
            const options = readScalarOptions(ts, checker, config && checker.getTypeOfSymbol(config), probe);
            if (!options)
                continue;
            parameters.push({ name: config.name, optional: false, type: options });
        }
        const signature = {
            parameters,
            returnType: { kind: 'object', members },
            typeParameters: ['T'],
            scalarResultTypeParameter: 'T',
            managedNameSuffix: typeof probe.variant === 'boolean' ?
                `With${probe.modeName[0].toUpperCase()}${probe.modeName.slice(1)}${probe.variant ? 'True' : 'False'}` : undefined,
            definition: definition(call.declaration),
        };
        const signatures = projected.get(probe.name) ?? [];
        signatures.push(signature);
        projected.set(probe.name, signatures);
    }
    return projected;
}

function preservesDataSlot(ts, checker, vueRef, result, name, data, mode, variant) {
    const member = result.getProperty(name);
    const type = checker.getTypeOfPropertyOfType(result, name);
    const ref = vueRef(type);
    if (ref?.canonical && ref.element === data)
        return true;
    if (!mode || (ref?.canonical ? ref.element : type)?.checkType !== mode)
        return false;
    const value = typeof variant === 'boolean' ?
        (variant ? checker.getTrueType() : checker.getFalseType()) : mode.getDefault();
    if (value !== checker.getTrueType() && value !== checker.getFalseType())
        return false;
    const declaration = member?.declarations?.find(ts.isPropertySignature);
    const annotation = declaration?.type;
    const condition = ref?.canonical && annotation && ts.isTypeReferenceNode(annotation) ?
        annotation.typeArguments?.[0] : annotation;
    if (!condition || !ts.isConditionalTypeNode(condition) || !ts.isLiteralTypeNode(condition.extendsType) ||
        ![ts.SyntaxKind.TrueKeyword, ts.SyntaxKind.FalseKeyword].includes(condition.extendsType.literal.kind))
        return false;
    const expected = checker.getTypeFromTypeNode(condition.extendsType);
    const branch = checker.getTypeAtLocation(value === expected ? condition.trueType : condition.falseType);
    const branchRef = ref?.canonical ? undefined : vueRef(branch);
    const element = ref?.canonical ? branch : branchRef?.canonical ? branchRef.element : undefined;
    // A named return object can declare its own type parameters. Follow
    // the instantiated intersection arm back to the function data slot.
    return (result.isIntersection() ? result.types : [result]).some(arm => {
        if (!arm.target?.typeParameters || arm.getProperty(name)?.declarations?.[0] !== declaration)
            return false;
        const index = arm.target.typeParameters.indexOf(element);
        return index >= 0 && checker.getTypeArguments(arm)[index] === data;
    }) || element === data;
}

function readScalarOptions(ts, checker, type, { variant, modeName }) {
    const options = type?.getNonNullableType();
    const properties = options?.getProperties();
    if (!properties?.length || properties.some(property => !(property.flags & ts.SymbolFlags.Optional)))
        return;
    const members = properties.flatMap(property => {
        const domain = checker.getTypeOfPropertyOfType(options, property.name).getNonNullableType();
        if (property.name === modeName)
            return variant === 'options' ? [] : [{ name: property.name, optional: false,
                type: { kind: 'boolean' }, fixedBoolean: variant }];
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
