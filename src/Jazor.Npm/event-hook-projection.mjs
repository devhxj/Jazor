export function eventHookProbes(aliases, callsOf, selected) {
    const probes = [];
    for (const alias of aliases) {
        if (!selected.has(alias.name))
            continue;
        const calls = callsOf(alias);
        const [factory] = calls;
        if (calls.length !== 1 || factory.parameters.length || factory.thisParameter ||
            factory.typeParameters?.length !== 1)
            continue;
        const properties = factory.getReturnType().getProperties();
        if (properties.length < 3 || properties.length > 4)
            continue;
        // A one-element tuple keeps T in a single callback position even if
        // T itself is an array. Type arguments disappear at the JS boundary.
        probes.push({
            name: alias.name,
            expression: `<TPayload,>() => packageExports[${JSON.stringify(alias.name)}]<[TPayload]>()`,
        });
    }
    return probes;
}

export function readEventHookProjections(ts, checker, statements, probes, diagnostics, promise, definition) {
    const results = new Map();
    if (!promise)
        return results;
    const voidType = type => type.flags === ts.TypeFlags.Void;
    const signatures = type => type?.getCallSignatures() ?? [];
    const soleCall = type => {
        const calls = signatures(type);
        return calls.length === 1 && !calls[0].thisParameter && !calls[0].typeParameters?.length
            ? calls[0] : undefined;
    };
    const symbolType = symbol => checker.getTypeOfSymbol(symbol);
    const members = type => type.getProperties().map(symbol => ({
        name: symbol.name,
        signature: symbol.flags & ts.SymbolFlags.Optional ? undefined : soleCall(checker.getTypeOfPropertyOfType(type, symbol.name)),
        definition: definition(symbol.valueDeclaration ?? symbol.declarations?.[0]),
    }));
    const zero = signature => signature.parameters.length === 0 && voidType(signature.getReturnType());
    for (const [index, probe] of probes.entries()) {
        const statement = statements[index];
        if (diagnostics.some(diagnostic => diagnostic.start >= statement.pos && diagnostic.start < statement.end))
            continue;
        const arrow = statement.declarationList.declarations[0].initializer;
        const origin = checker.getResolvedSignature(arrow.body);
        const [payload] = checker.getSignatureFromDeclaration(arrow).typeParameters;
        const fields = members(origin.getReturnType());
        if (fields.some(field => !field.signature || !field.definition))
            continue;

        // Resolve both the listener and dispatch rest tuples before deciding
        // whether the callback and trigger share exactly one payload position.
        const takesPayload = signature => {
            if (signature.parameters.length !== 1)
                return false;
            const type = symbolType(signature.parameters[0]);
            if (!signature.declaration?.parameters[0]?.dotDotDotToken)
                return signature.minArgumentCount === 1 && type === payload;
            return checker.isTupleType(type) &&
                type.target.elementFlags.length === 1 && type.target.elementFlags[0] === ts.ElementFlags.Required &&
                checker.getTypeArguments(type)[0] === payload;
        };
        const starts = [];
        const stops = [];
        const dispatches = [];
        const resets = [];
        for (const field of fields) {
            const { signature } = field;
            const output = signature.getReturnType();
            if (zero(signature)) {
                resets.push(field);
            } else if (output.target?.getSymbol() === promise && takesPayload(signature)) {
                const [values] = checker.getTypeArguments(output);
                if (values && checker.isArrayType(values) &&
                    checker.getTypeArguments(values)[0]?.flags === ts.TypeFlags.Unknown)
                    dispatches.push(field);
            } else if (signature.parameters.length === 1 && signature.minArgumentCount === 1 &&
                !checker.hasEffectiveRestParameter(signature)) {
                const listener = symbolType(signature.parameters[0]);
                const callback = soleCall(listener);
                if (!callback || listener.getProperties().length || listener.getConstructSignatures().length ||
                    !voidType(callback.getReturnType()) || !takesPayload(callback))
                    continue;
                if (voidType(output)) {
                    stops.push({ field, listener, callback });
                } else {
                    const [cancel, ...extra] = members(output);
                    if (cancel?.signature && cancel.definition && !extra.length && zero(cancel.signature))
                        starts.push({ field, listener, callback, cancel });
                }
            }
        }
        if (starts.length !== 1 || stops.length !== 1 || dispatches.length !== 1 || resets.length !== fields.length - 3 ||
            starts[0].listener !== stops[0].listener)
            continue;
        const [start] = starts;
        const [stop] = stops;
        const [dispatch] = dispatches;
        const entry = { kind: 'typeParameter', name: payload.symbol.name };
        const noValue = { kind: 'void' };
        const parameter = (symbol, type) => ({ name: symbol.name, optional: false, type });
        const method = (field, parameters, returnType) => ({
            name: field.name,
            readOnly: true,
            definition: field.definition,
            type: { kind: 'method', signatures: [{ parameters, returnType, definition: field.definition }] },
        });
        // Both operations take the same delegate type so Off receives the
        // original JS callback identity. The Promise is awaitable for completion.
        const callback = { kind: 'action', signatures: [{
            parameters: [parameter(start.callback.parameters[0], entry)], returnType: noValue,
        }] };
        const subscription = { kind: 'object', members: [method(start.cancel, [], noValue)] };
        results.set(probe.name, {
            parameters: [],
            typeParameters: [entry.name],
            definition: definition(origin.declaration),
            returnType: { kind: 'object', members: [
                method(start.field, [parameter(start.field.signature.parameters[0], callback)], subscription),
                method(stop.field, [parameter(stop.field.signature.parameters[0], callback)], noValue),
                method(dispatch, [parameter(dispatch.signature.parameters[0], entry)], { kind: 'promise', elementType: noValue }),
                ...resets.map(field => method(field, [], noValue)),
            ] },
        });
    }
    return results;
}
