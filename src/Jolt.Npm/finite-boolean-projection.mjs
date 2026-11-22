export function finiteBooleanProbes(ts, checker, aliases, callsOf, requested, typeOf) {
    return aliases.flatMap(alias => {
        if (!requested.has(alias.name))
            return [];
        const calls = callsOf(alias);
        const [call] = calls;
        const slot = call?.typeParameters?.[0];
        if (calls.length !== 1 || call.typeParameters?.length !== 1 ||
            checker.getBaseConstraintOfType(slot) !== checker.getBooleanType() ||
            call.thisParameter || checker.hasEffectiveRestParameter(call))
            return [];
        const result = call.getReturnType();
        if (!(result.flags & ts.TypeFlags.Conditional) || result.checkType !== slot ||
            ![checker.getTrueType(), checker.getFalseType()].includes(result.extendsType))
            return [];
        const isOptional = parameter => checker.isOptionalParameter(parameter.valueDeclaration);
        const fixedParameters = call.parameters.filter(parameter => {
            const type = typeOf(parameter);
            return type === slot || isOptional(parameter) && type.isUnion() && type.types.includes(slot) &&
                type.types.every(branch => branch === slot || branch.flags & ts.TypeFlags.Undefined);
        });
        if (fixedParameters.length !== 1)
            return [];
        const fixedIndex = call.parameters.indexOf(fixedParameters[0]);
        const name = fixedParameters[0].name;
        const probes = [false, true].map(fixed => {
            // 明确实例化分支，其余参数只作为探针占位，实际签名使用解析后的参数类型。
            const args = call.parameters.map((_, index) => index === fixedIndex ? String(fixed) : 'undefined as never');
            return { name: alias.name, fixed, fixedIndex, parameterName: name,
                expression: `packageExports[${JSON.stringify(alias.name)}]<${fixed}>(${args.join(', ')})` };
        });
        if (isOptional(fixedParameters[0]) && call.parameters.slice(fixedIndex).every(isOptional)) {
            // Let the checker select the omitted call's type argument, including any
            // declared default. Stand-ins only fill parameters before the omitted slot.
            const args = call.parameters.slice(0, fixedIndex).map(() => 'undefined as never');
            probes.push({ name: alias.name, fixedIndex, expression:
                `packageExports[${JSON.stringify(alias.name)}](${args.join(', ')})` });
        }
        return probes;
    });
}

export function readFiniteBooleanSignatures(ts, checker, probes, statements, diagnostics, name, original, typeOf,
    omitUndefined, readInput, readResult, definition) {
    const resolved = [];
    for (const [index, probe] of probes.entries()) {
        if (probe.name !== name)
            continue;
        const statement = statements[index];
        if (diagnostics.some(diagnostic => diagnostic.start >= statement.pos && diagnostic.start < statement.end))
            continue;
        const call = checker.getResolvedSignature(statement.declarationList.declarations[0].initializer);
        if (call?.declaration !== original.declaration)
            continue;
        resolved.push({ probe, call });
    }
    const literalCalls = resolved.filter(({ probe }) => probe.fixed !== undefined);
    if (literalCalls.length !== 2)
        return;
    const signatures = [];
    for (const { probe, call } of resolved) {
        const omitted = probe.fixed === undefined;
        // A second input mentioning the flag could affect inference on omission
        // or admit a conflicting literal after C# type projection.
        if (call.parameters.some((parameter, index) => index !== probe.fixedIndex &&
            (!omitted || index < probe.fixedIndex) &&
            [original, ...literalCalls.map(branch => branch.call)]
                .some(signature => typeOf(signature.parameters[index]) !== typeOf(parameter))))
            continue;
        const parameters = call.parameters.flatMap((parameter, index) => {
            if (omitted && index >= probe.fixedIndex)
                return [];
            if (index === probe.fixedIndex)
                return [{ name: parameter.name, type: { kind: 'boolean' }, optional: false, fixedBoolean: probe.fixed }];
            const optional = checker.isOptionalParameter(original.parameters[index].valueDeclaration);
            const type = typeOf(parameter);
            return [{ name: parameter.name, type: readInput(optional ? omitUndefined(type) : type), optional }];
        });
        const returnType = readResult(call);
        if (parameters.some(parameter => !parameter.type) || !returnType)
            continue;
        const suffixName = probe.parameterName?.replace(/^[a-z]/, character => character.toUpperCase());
        signatures.push({ parameters, returnType, definition: definition(call.declaration),
            ...(omitted ? {} : { managedNameSuffix: `With${suffixName}${probe.fixed ? 'True' : 'False'}` }) });
    }
    return signatures.length ? signatures : undefined;
}
