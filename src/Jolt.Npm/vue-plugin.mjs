export function projectVuePlugin(ts, checker, contract, type) {
    if (!contract || type.flags & (ts.TypeFlags.Any | ts.TypeFlags.Unknown) ||
        !checker.isTypeAssignableTo(type, contract))
        return;
    const install = type.getProperty('install');
    if (!install || install.flags & ts.SymbolFlags.Optional)
        return;
    const calls = checker.getTypeOfPropertyOfType(type, 'install').getCallSignatures();
    const [expected] = checker.getTypeOfPropertyOfType(contract, 'install').getCallSignatures();
    // Vue can invoke this projection with only its App argument. The upstream
    // declaration still controls whether that receiver and call arity are legal.
    if (calls.length !== 1)
        return;
    const [call] = calls;
    const [receiver] = call.parameters;
    if (receiver && call.minArgumentCount <= 1 && !call.thisParameter &&
        call.getReturnType().flags & ts.TypeFlags.Void &&
        checker.isTypeAssignableTo(checker.getTypeOfSymbol(expected.parameters[0]), checker.getTypeOfSymbol(receiver)))
        return { kind: 'vuePlugin' };
}

function projectOption(ts, checker, type, stringDictionary, ancestors = []) {
    if (type.flags & (ts.TypeFlags.Any | ts.TypeFlags.Unknown | ts.TypeFlags.Never) || ancestors.includes(type))
        return;
    const indexed = type.getStringIndexType();
    if (indexed) {
        const element = projectOption(ts, checker, indexed, stringDictionary, [...ancestors, type]);
        if (element)
            return {
                type: { kind: 'dictionary', elementType: element.type },
                annotation: `Record<string, ${element.annotation}>`,
            };
    }

    // A carrier covers its entire scalar domain. Narrow literal options and
    // ambiguous unions have no single scalar carrier.
    const scalars = [
        ['boolean', checker.getBooleanType()],
        ['string', checker.getStringType()],
        ['number', checker.getNumberType()],
    ].filter(([, scalar]) => checker.isTypeAssignableTo(scalar, type));
    if (scalars.length === 1) {
        const [[kind]] = scalars;
        if (kind === 'string' && ancestors.length && type.isUnion() &&
            checker.isTypeAssignableTo(stringDictionary, type))
            return {
                type: { kind: 'union', variants: [
                    { kind: 'string' }, { kind: 'dictionary', elementType: { kind: 'string' } },
                ] },
                annotation: 'string | Record<string, string>',
            };
        return { type: { kind }, annotation: kind };
    }
    if (type.isUnion() && type.types.every(branch => branch.flags & ts.TypeFlags.StringLiteral)) {
        const stringLiterals = type.types.map(branch => branch.value).sort();
        return {
            type: { kind: 'stringEnum', name: type.aliasSymbol?.name, stringLiterals },
            annotation: stringLiterals.map(value => JSON.stringify(value)).join(' | '),
        };
    }
}

export function pluginFactoryProbes(ts, checker, aliases, callsOf, requested, typeOf, contract, defaults, stringDictionary) {
    const probes = [];
    for (const alias of aliases) {
        if (!requested.has(alias.name))
            continue;
        for (const signature of callsOf(alias)) {
            if (signature.parameters.length !== 1 || !signature.typeParameters?.length ||
                signature.thisParameter || checker.hasEffectiveRestParameter(signature))
                continue;
            const plugin = projectVuePlugin(ts, checker, contract, signature.getReturnType());
            if (!plugin && (signature.minArgumentCount || !defaults.includes(alias)))
                continue;
            const options = typeOf(signature.parameters[0]).getNonNullableType();
            const members = [];
            const fields = [];
            let unsupportedRequired = false;
            for (const property of options.getProperties()) {
                const type = checker.getTypeOfSymbolAtLocation(property, signature.declaration).getNonNullableType();
                const optional = Boolean(property.flags & ts.SymbolFlags.Optional);
                const projected = projectOption(ts, checker, type, stringDictionary);
                if (!projected) {
                    unsupportedRequired ||= !optional;
                    continue;
                }
                members.push({ name: property.name, optional, readOnly: false, type: projected.type });
                fields.push({ name: property.name, optional, annotation: projected.annotation });
            }
            if (unsupportedRequired || !plugin && !members.length)
                continue;
            // Supply the entire projected options shape, including optional
            // fields, so contextual typing cannot narrow the tested values.
            const add = (mode, fixed) => {
                const annotations = fields.map(field => field.name === mode
                    ? `${JSON.stringify(mode)}: ${fixed}`
                    : `${JSON.stringify(field.name)}${field.optional ? '?' : ''}: ${field.annotation}`);
                const expression = `packageExports[${JSON.stringify(alias.name)}]({} as { ${annotations.join('; ')} })`;
                probes.push({ name: alias.name, declaration: signature.declaration, plugin: Boolean(plugin), members, expression, mode, fixed });
            };
            add();
            if (plugin && signature.declaration.type && ts.isConditionalTypeNode(signature.declaration.type)) {
                // Probe only a directly indexed input discriminator declared by the
                // conditional return. Independent boolean options do not get variants.
                const check = signature.declaration.type.checkType;
                if (ts.isIndexedAccessTypeNode(check) && checker.getTypeAtLocation(check.objectType) === typeOf(signature.parameters[0]) &&
                    (checker.getTypeAtLocation(check.objectType).flags & ts.TypeFlags.TypeParameter) &&
                    ts.isLiteralTypeNode(check.indexType) &&
                    ts.isStringLiteral(check.indexType.literal) &&
                    fields.some(field => field.name === check.indexType.literal.text && field.annotation === 'boolean')) {
                    add(check.indexType.literal.text, false);
                    add(check.indexType.literal.text, true);
                }
            }
            break;
        }
    }
    return probes;
}

export function readPluginFactory(ts, checker, { probe, signature }, readPlugin, definition, readMember, managedName,
    earlierReturns) {
    const result = signature?.getReturnType();
    const plugin = result && readPlugin(result);
    const original = signature?.declaration && checker.getSignatureFromDeclaration(signature.declaration);
    const genericResult = original?.getReturnType();
    // A widened argument can make a conditional return pick one branch.
    // Verify the unspecialized return as well so every inferred mode is legal.
    if (!plugin || !original?.typeParameters?.length || !readPlugin(genericResult) ||
        earlierReturns.some(type => !readPlugin(type)) ||
        probe.mode && (probe.declaration.getSourceFile().fileName !== signature.declaration.getSourceFile().fileName ||
            probe.declaration.pos !== signature.declaration.pos))
        return;
    // A narrower overload can accept part of the wider C# input even when the
    // checker selects the generic overload for the full option record.
    const members = (earlierReturns.length ? [] : genericResult.getProperties()).flatMap(property => {
        const declarations = property.declarations ?? [];
        if (declarations.length && !(property.flags & ts.SymbolFlags.Optional) &&
            declarations.every(declaration => ts.isPropertySignature(declaration) &&
                declaration.modifiers?.some(modifier => modifier.kind === ts.SyntaxKind.ReadonlyKeyword))) {
            const type = checker.getTypeOfPropertyOfType(genericResult, property.name);
            if (type.isUnion() && type.types.every(branch => branch.flags & ts.TypeFlags.StringLiteral))
                return [{ name: property.name, readOnly: true, definition: definition(declarations[0]),
                    type: { kind: 'stringEnum', name: type.aliasSymbol?.name ?? property.name,
                        stringLiterals: type.types.map(branch => branch.value).sort() } }];
            if (probe.mode) {
                const specialized = checker.getTypeOfPropertyOfType(result, property.name);
                if (specialized !== type && !(specialized.flags & (ts.TypeFlags.Any | ts.TypeFlags.Unknown | ts.TypeFlags.Conditional))) {
                    const projected = readMember(specialized);
                    if (projected?.members?.length)
                        return [{ name: property.name, readOnly: true, definition: definition(declarations[0]), type: projected }];
                }
            }
        }
        if (!declarations.length || !declarations.every(declaration =>
            (ts.isMethodSignature(declaration) || ts.isMethodDeclaration(declaration)) &&
            (ts.isIdentifier(declaration.name) || ts.isStringLiteral(declaration.name)) &&
            !(ts.getCombinedModifierFlags(declaration) & (ts.ModifierFlags.Private | ts.ModifierFlags.Protected))) ||
            property.flags & ts.SymbolFlags.Optional)
            return [];
        const calls = checker.getTypeOfPropertyOfType(genericResult, property.name).getCallSignatures();
        const [call] = calls;
        if (calls.length !== 1 || call.parameters.length || call.typeParameters?.length || call.thisParameter ||
            !(call.getReturnType().flags & ts.TypeFlags.Void))
            return [];
        const location = definition(call.declaration);
        return [{ name: property.name, readOnly: true, type: { kind: 'method', signatures: [
            { parameters: [], returnType: { kind: 'void' }, definition: location },
        ] }, definition: location }];
    });
    if (probe.mode && !members.some(member => member.type.kind !== 'method' && member.type.kind !== 'stringEnum'))
        return;
    const parameters = probe.members.map(member => member.name === probe.mode
        ? { ...member, optional: false, fixedBoolean: probe.fixed } : member);
    const managedNameSuffix = probe.mode && `With${managedName(probe.mode)}${probe.fixed ? 'True' : 'False'}`;
    return {
        parameters: [{ name: signature.parameters[0].name, optional: false, type: { kind: 'object', members: parameters } }],
        returnType: { ...plugin, name: result.getSymbol()?.name, members },
        definition: definition(signature.declaration),
        managedNameSuffix,
    };
}
