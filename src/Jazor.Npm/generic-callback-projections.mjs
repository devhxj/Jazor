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
        const valueMembers = result.getProperties().filter(property => {
            const member = checker.getTypeOfPropertyOfType(result, property.name);
            return checker.getTypeOfPropertyOfType(member, 'value') === typeParameter;
        });
        if (!callbacks.length || initial.length !== 1 || !valueMembers.length)
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
        const flags = settings.getProperties().filter(member =>
            checker.getNonNullableType(checker.getTypeOfPropertyOfType(settings, member.name)) === checker.getBooleanType());
        const optionsType = `{ ${property}?: TValue; ${flags.map(member => `${JSON.stringify(member.name)}?: boolean`).join('; ')} }`;
        for (const keyType of keys) {
            const add = (parameters, args, { optionalCallback = false, omittedRules = false, managedNameSuffix } = {}) => projections.push({
                exportName: alias.name,
                expression: `<TValue,>(path: ${keyType}${parameters}) => ${exportAccess}<TValue>(path${args})`,
                optionalValue: valueMembers.map(member => member.name),
                optionalCallback,
                omittedRules,
                managedNameSuffix,
            });
            if (signature.minArgumentCount <= 1 && checker.isTypeAssignableTo(checker.getStringType(), handler)) {
                add('', '');
                add(', rules: string', ', rules');
                add(`, options: ${optionsType}`, ', undefined, options', { omittedRules: true });
                add(`, rules: string, options: ${optionsType}`, ', rules, options');
            }
            for (const returnType of returns) {
                add(`, rules: (value: TValue | undefined) => ${returnType}`, ', rules', { optionalCallback: true });
                add(`, rules: (value: TValue | undefined) => ${returnType}, options: ${optionsType}`, ', rules, options',
                    { optionalCallback: true, managedNameSuffix: 'WithOptionalValue' });
                projections.push({
                    exportName: alias.name,
                    expression: `<TValue,>(path: ${keyType}, rules: (value: TValue) => ${returnType}, opts: { ${property}: TValue }) =>
                        ${exportAccess}<TValue>(path, rules, opts)`,
                });
            }
        }
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
        results.push({ signature, declaration: resolved.declaration, ...projection });
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
