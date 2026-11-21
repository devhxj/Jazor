export function genericCallbackProjections(ts, program, checker, source, aliases, callsOf, selected, typeOf) {
    const projections = [];
    const promise = checker.resolveName('Promise', source, ts.SymbolFlags.Type, false);
    const standardPromise = promise?.declarations?.length &&
        promise.declarations.every(node => program.isSourceFileDefaultLibrary(node.getSourceFile()));
    for (const alias of aliases) {
        if (!selected.has(alias.name))
            continue;
        const calls = callsOf(alias);
        if (calls.length !== 1)
            continue;
        const [signature] = calls;
        if (signature.typeParameters?.length !== 1 || signature.parameters.length !== 3 ||
            signature.thisParameter || checker.hasEffectiveRestParameter(signature))
            continue;
        const [typeParameter] = signature.typeParameters;
        const [key, handler, options] = signature.parameters.map(typeOf);
        const keys = scalarBranches(ts, checker, key);
        if (!keys.length)
            continue;
        const callbacks = branches(handler).flatMap(type => type.getCallSignatures()).filter(call =>
            call.minArgumentCount > 0 && typeOf(call.parameters[0]) === typeParameter);
        const settings = checker.getNonNullableType(options);
        const initial = settings.getProperties().filter(property => {
            const values = branches(checker.getTypeOfPropertyOfType(settings, property.name));
            // A getter option can describe a future value instead of
            // initializing the carrier at the call site.
            return values.includes(typeParameter) && !values.some(type => type.getCallSignatures().length);
        });
        const result = signature.getReturnType();
        const carriesValue = result.getProperties().some(property => {
            const member = checker.getTypeOfPropertyOfType(result, property.name);
            return checker.getTypeOfPropertyOfType(member, 'value') === typeParameter;
        });
        if (!callbacks.length || initial.length !== 1 || !carriesValue)
            continue;
        // The upstream call is type-checked with the same generic type in the
        // callback and the initial-value option. Requiring both at the C#
        // boundary preserves inference and excludes uninitialized fields.
        const property = JSON.stringify(initial[0].name);
        const exportAccess = `packageExports[${JSON.stringify(alias.name)}]`;
        // Check the declared synchronous and promised results independently.
        // Emit only complete scalar domains C# can offer to the callback.
        const returns = new Set();
        for (const callback of callbacks) {
            const result = callback.getReturnType();
            const immediate = scalarBranches(ts, checker, result);
            if (immediate.length)
                returns.add(immediate.join(' | '));
            if (!standardPromise)
                continue;
            for (const branch of branches(result)) {
                if (branch.target?.getSymbol() !== promise)
                    continue;
                const [element] = checker.getTypeArguments(branch);
                const deferred = scalarBranches(ts, checker, element);
                if (deferred.length)
                    returns.add(`Promise<${deferred.join(' | ')}>`);
            }
        }
        for (const keyType of keys)
            for (const returnType of returns)
                projections.push({
                    exportName: alias.name,
                    expression: `<TValue,>(path: ${keyType}, rules: (value: TValue) => ${returnType}, opts: { ${property}: TValue }) =>
                        ${exportAccess}<TValue>(path, rules, opts)`,
                });
    }
    return projections;
}

export function readGenericCallbackProjections(checker, statements, projections, diagnostics) {
    const byExport = new Map();
    for (const [index, projection] of projections.entries()) {
        const statement = statements[index];
        if (diagnostics.some(diagnostic => diagnostic.start >= statement.pos && diagnostic.start < statement.end))
            continue;
        const initializer = statement.declarationList.declarations[0].initializer;
        const signature = checker.getSignatureFromDeclaration(initializer);
        const resolved = checker.getResolvedSignature(initializer.body);
        if (!resolved || !signature)
            continue;
        const results = byExport.get(projection.exportName) ?? [];
        results.push({ signature, declaration: resolved.declaration });
        byExport.set(projection.exportName, results);
    }
    return byExport;
}

function branches(type) {
    return type.isUnion() ? type.types : [type];
}

function scalarBranches(ts, checker, type) {
    const members = branches(type);
    const types = [];
    if (members.some(member => member.flags & ts.TypeFlags.String))
        types.push('string');
    if (members.some(member => member.flags & ts.TypeFlags.Number))
        types.push('number');
    // A lone true/false literal cannot use a C# bool parameter or result.
    if (members.some(member => member.flags & (ts.TypeFlags.Boolean | ts.TypeFlags.BooleanLiteral)) &&
        checker.isTypeAssignableTo(checker.getBooleanType(), type))
        types.push('boolean');
    return types;
}
