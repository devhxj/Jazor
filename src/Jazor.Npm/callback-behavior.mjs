export function behaviorProbes(ts, checker, aliases, callsOf, requested, typeOf) {
    return aliases.filter(alias => requested.has(alias.name)).flatMap(alias => {
        const callee = `packageExports[${JSON.stringify(alias.name)}]`;
        const probes = [];
        for (const signature of callsOf(alias)) {
            if (signature.typeParameters?.length !== 1 || signature.minArgumentCount > 2 || signature.parameters.length < 2)
                continue;
            const [generic] = signature.typeParameters;
            if (typeOf(signature.parameters[0]) === generic &&
                checker.getBaseConstraintOfType(generic)?.getCallSignatures().length &&
                checker.isTypeAssignableTo(checker.getNumberType(), typeOf(signature.parameters[1]))) {
                const callable = signature.getReturnType().getCallSignatures().length > 0;
                const results = callable ? ['void', 'value', 'promise'] : ['void'];
                for (const count of [0, 1]) for (const resultKind of results) {
                    const args = count ? 'value: JoltCallback_TValue' : '';
                    const result = resultKind === 'promise' ? 'Promise<JoltCallback_TResult>' :
                        resultKind === 'value' ? 'JoltCallback_TResult' : 'void';
                    const body = resultKind === 'void' ? ' => {}' : ` => ({} as ${result})`;
                    probes.push({ name: alias.name, ordinal: callsOf(alias).indexOf(signature), count, callable, resultKind,
                        expression: `${callee}((${args}): ${result}${body}, 10 as number)` });
                }
            } else if (signature.minArgumentCount === 2 &&
                typeOf(signature.parameters[1]).getCallSignatures().length &&
                signature.getReturnType().getCallSignatures().some(call => call.parameters.length === 0 &&
                    call.getReturnType().flags & ts.TypeFlags.Void)) {
                probes.push({ name: alias.name, ordinal: callsOf(alias).indexOf(signature), outside: true,
                    expression: `${callee}({} as HTMLElement, (_): void => {})` });
            }
        }
        return probes;
    });
}

export function readBehaviorProjections(ts, checker, probes, statements, diagnostics, signatures, exportName,
    readType, readReturn, definition, unsupported, standardPromise) {
    const results = [];
    for (const [index, probe] of probes.entries()) {
        if (probe.name !== exportName)
            continue;
        const statement = statements[index];
        if (diagnostics.some(diagnostic => diagnostic.start >= statement.pos && diagnostic.start < statement.end))
            continue;
        const expression = statement.declarationList.declarations[0].initializer;
        const signature = checker.getResolvedSignature(expression);
        if (signature?.declaration !== signatures[probe.ordinal]?.declaration)
            continue;
        const location = definition(signature.declaration);
        const shape = type => readType(type, new Set(), 'input', false, unsupported);
        const typeAt = node => checker.getTypeAtLocation(node);
        const propertyType = (type, name) => checker.getTypeOfPropertyOfType(type, name);
        const parameters = expression.arguments.map((arg, i) => ({
            name: signature.parameters[i].name, optional: false,
            type: probe.outside && i === 1 ? undefined : shape(typeAt(arg)),
        }));
        const returned = signature.getReturnType();
        if (probe.outside) {
            const contextual = checker.getContextualType(expression.arguments[1])?.getCallSignatures()[0];
            const eventParameter = contextual?.parameters[0];
            const payload = eventParameter && checker.getTypeOfSymbolAtLocation(eventParameter, expression);
            if (!payload || shape(payload).kind !== 'web')
                continue;
            parameters[1].type = { kind: 'function', signatures: [{
                parameters: [{ name: 'event', type: shape(payload), optional: false }], returnType: { kind: 'void' },
            }] };
            results.push({ parameters, returnType: readReturn(signature, new Set(), 'output', false, unsupported), definition: location });
            continue;
        }
        const callback = parameters[0].type.signatures[0];
        const typeParameters = probe.count ? ['TValue'] : [];
        if (probe.callable) {
            const [invoke] = returned.getCallSignatures();
            const promise = invoke?.getReturnType();
            const invokeParameterTypes = invoke?.parameters.map(parameter => checker.getTypeOfSymbolAtLocation(parameter, expression));
            const invokeArgs = invokeParameterTypes?.length === 1 && checker.isTupleType(invokeParameterTypes[0])
                ? checker.getTypeArguments(invokeParameterTypes[0]) : invokeParameterTypes;
            const payload = promise?.target?.getSymbol() === standardPromise && checker.getTypeArguments(promise)[0];
            const returnsValue = probe.resultKind !== 'void';
            if (!payload || (returnsValue
                    ? payload.getSymbol()?.name !== 'JoltCallback_TResult'
                    : !(payload.flags & ts.TypeFlags.Void)) ||
                invokeArgs.length !== probe.count ||
                invokeArgs.some(parameter => parameter.getSymbol()?.name !== 'JoltCallback_TValue'))
                continue;
            if (returnsValue)
                typeParameters.push('TResult');
            // Suppressed calls resolve without a value in the default options.
            const value = probe.resultKind === 'promise' ? callback.returnType.elementType : callback.returnType;
            const result = { kind: 'promise', elementType: returnsValue
                ? { kind: 'undefinedable', elementType: value } : value };
            const invokeShape = { parameters: callback.parameters, returnType: result };
            const members = controlMembers(returned, propertyType, shape, ['cancel', 'flush', 'isPending']);
            const returnType = members.length
                ? { kind: 'callableHandle', members, signatures: [invokeShape] }
                : { kind: 'function', signatures: [invokeShape] };
            // A Promise can itself infer TResult for the synchronous delegate.
            // Prefer the more specific callback shape to expose the flattened result.
            results.push({ parameters, returnType, typeParameters, definition: location,
                overloadPriority: probe.resultKind === 'promise' ? 1 : undefined });
        } else {
            const start = propertyType(returned, 'start')?.getCallSignatures()[0];
            const startParameter = start?.parameters[0];
            const rest = startParameter && checker.getTypeOfSymbolAtLocation(startParameter, expression);
            const branches = rest?.isUnion() ? rest.types : [rest];
            if (branches.some(type => !type || !checker.isTupleType(type)) ||
                !branches.some(type => checker.getTypeArguments(type).length === probe.count &&
                    checker.getTypeArguments(type).every(arg => arg.getSymbol()?.name === 'JoltCallback_TValue')))
                continue;
            const members = controlMembers(returned, propertyType, shape, ['stop', 'isPending']);
            if (members.length !== 2)
                continue;
            members.push({ name: 'start', readOnly: true, type: { kind: 'method', signatures: [{
                parameters: callback.parameters, returnType: { kind: 'void' },
            }] } });
            results.push({ parameters, returnType: { kind: 'object', members }, typeParameters, definition: location });
        }
    }
    return results;
}

function controlMembers(type, propertyType, read, names) {
    return names.flatMap(name => {
        const property = propertyType(type, name);
        return property ? [{ name, type: read(property), readOnly: true }] : [];
    });
}
