export function callbackProbes(ts, program, checker, aliases, callsOf, selected) {
    const probes = [];
    const symbol = checker.resolveName('Promise', undefined, ts.SymbolFlags.Type, false);
    const promise = symbol?.declarations?.every(node => program.isSourceFileDefaultLibrary(node.getSourceFile())) ? symbol : undefined;
    const context = { ts, checker, promise };
    for (const alias of aliases) {
        if (!selected.has(alias.name))
            continue;
        const calls = callsOf(alias);
        for (const [ordinal, signature] of calls.entries()) {
            if (!signature.typeParameters?.length || signature.minArgumentCount !== 1)
                continue;
            const options = checker.getTypeOfSymbol(signature.parameters[0]);
            for (const branch of alternatives(options)) {
                for (const member of branch.getProperties()) {
                    const type = checker.getTypeOfPropertyOfType(branch, member.name);
                    const match = alternatives(type).flatMap(type => singleCall(type))
                        .map(callback => callbackParameters(context, signature, callback))
                        .find(Boolean);
                    if (!match)
                        continue;
                    const argumentsList = signature.typeParameters.map(parameter => {
                        if (match.parameters.includes(parameter))
                            return marker(parameter);
                        const fallback = parameter.getDefault();
                        if (!fallback)
                            return;
                        if (match.parameters.includes(fallback))
                            return marker(fallback);
                        if (checker.isArrayType(fallback) && fallback.target?.getSymbol()?.name === 'ReadonlyArray' &&
                            checker.getTypeArguments(fallback)[0]?.flags & ts.TypeFlags.Unknown)
                            return 'readonly (string | number | boolean | object)[]';
                        const symbol = fallback.getSymbol();
                        const global = symbol && checker.resolveName(symbol.name, undefined, ts.SymbolFlags.Type, false);
                        if (symbol === global && symbol)
                            return symbol.name;
                        return checker.typeToString(fallback, undefined, ts.TypeFormatFlags.NoTruncation);
                    });
                    if (!argumentsList.every(Boolean))
                        continue;
                    probes.push({ exportName: alias.name, ordinal, member: member.name,
                        names: match.parameters.map(marker), result: marker(match.result),
                        expression: `packageExports[${JSON.stringify(alias.name)}]<${argumentsList.join(', ')}>` });
                }
            }
        }
    }
    return [...new Map(probes.map(probe => [JSON.stringify(probe), probe])).values()];
}

function marker(parameter) {
    return `JoltCallback_${parameter.symbol.name}`;
}

function callbackParameters({ ts, checker, promise }, signature, callback) {
    const result = callback.getReturnType();
    const returned = alternatives(result).map(type => promise && type.target?.getSymbol() === promise ?
        checker.getTypeArguments(type)[0] : type);
    const returnedParameter = returned.find(type => signature.typeParameters.includes(type));
    if (!returnedParameter)
        return;
    const parameters = signature.typeParameters.filter(parameter => parameter === returnedParameter ||
        callback.parameters.some(value => checker.getTypeOfSymbol(value) === parameter));
    if (parameters.some(parameter => {
        const constraint = checker.getBaseConstraintOfType(parameter);
        return constraint && !(constraint.flags & ts.TypeFlags.Unknown);
    }))
        return;
    return { parameters, result: returnedParameter };
}

export function singleCall(type) {
    const calls = type.getCallSignatures();
    return calls.length === 1 ? calls : [];
}

export function hasConditionalAnnotation(ts, checker, type, member) {
    const seenMembers = new Set();
    return visitsMember(member) || (type.isUnionOrIntersection() && type.types.some(branch => {
        const property = checker.getPropertyOfType(branch, member.name);
        return property && visitsMember(property);
    }));

    function visitsMember(property) {
        if (seenMembers.has(property))
            return false;
        seenMembers.add(property);
        // TS synthesizes mapped properties without declarations. Its pinned
        // checker retains the template and any copied property in symbol links.
        const mapped = property.links?.mappedType?.declaration;
        const origin = property.links?.syntheticOrigin;
        return property.declarations?.some(declaration => declaration.type && visitsConditional(declaration.type)) ||
            mapped && [mapped.type, mapped.nameType, mapped.typeParameter.constraint]
                .some(annotation => annotation && visitsConditional(annotation, mapped)) ||
            origin && visitsMember(origin);
    }

    function visitsConditional(annotation, mapped) {
        const seen = new Set();
        const key = mapped && checker.getSymbolAtLocation(mapped.typeParameter.name);
        return visit(annotation);

        function visit(node) {
            if (ts.isConditionalTypeNode(node)) {
                // A comparison between the mapped key and fixed literal names
                // dispatches on the projected member name.
                if (!key || !ts.isTypeReferenceNode(node.checkType) ||
                    checker.getSymbolAtLocation(node.checkType.typeName) !== key ||
                    !literalKeys(node.extendsType))
                    return true;
                return visit(node.trueType) || visit(node.falseType);
            }
            if (ts.isFunctionTypeNode(node) || ts.isCallSignatureDeclaration(node))
                return visit(node.type);
            if (ts.isTypeReferenceNode(node)) {
                const symbol = checker.getSymbolAtLocation(node.typeName);
                const target = symbol?.flags & ts.SymbolFlags.Alias ? checker.getAliasedSymbol(symbol) : symbol;
                if (target && !seen.has(target)) {
                    seen.add(target);
                    if (target.declarations?.some(declaration => ts.isTypeAliasDeclaration(declaration) &&
                        !isVueMappedRef(declaration, mapped) && visit(declaration.type)))
                        return true;
                }
            }
            return Boolean(ts.forEachChild(node, visit));
        }
    }

    function literalKeys(node) {
        return ts.isUnionTypeNode(node) ? node.types.every(literalKeys) :
            ts.isLiteralTypeNode(node) && ts.isStringLiteral(node.literal);
    }

    function isVueMappedRef(declaration, mapped) {
        // Vue's own ToRefs uses ToRef for its stable property-ref wrapper.
        return mapped && declaration.name.text === 'ToRef' &&
            mapped.parent.name?.text === 'ToRefs' &&
            declaration.getSourceFile() === mapped.getSourceFile() &&
            declaration.getSourceFile().fileName.replaceAll('\\', '/').includes('/node_modules/@vue/reactivity/');
    }
}

export function alternatives(type) {
    return !type ? [] : type.isUnion() ? type.types : [type];
}
