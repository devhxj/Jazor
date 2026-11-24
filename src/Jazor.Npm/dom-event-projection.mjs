import { alternatives } from './generic-callbacks.mjs';

export function domEventProbes(ts, getChecker, aliases, callsOf, requested, typeOf, domSymbol, webType) {
    const isDom = (type, name) => {
        const symbol = domSymbol(name);
        return symbol && type?.getSymbol() === symbol;
    };
    const isFallback = signature => signature.minArgumentCount === 3 && signature.typeParameters?.length === 1 &&
        isDom(signature.typeParameters[0].getDefault(), 'Event') &&
        alternatives(typeOf(signature.parameters[0])).some(type => isDom(type, 'EventTarget'));
    const isElementMap = signature => {
        const parameter = signature.typeParameters?.[0]?.symbol.declarations?.[0];
        const constraint = parameter?.constraint;
        const map = domSymbol('HTMLElementEventMap');
        return map && signature.minArgumentCount === 3 && signature.typeParameters?.length === 1 &&
            constraint && ts.isTypeOperatorNode(constraint) && constraint.operator === ts.SyntaxKind.KeyOfKeyword &&
            getChecker().getTypeFromTypeNode(constraint.type).getSymbol() === map &&
            alternatives(typeOf(signature.parameters[0])).some(type => isDom(type, 'HTMLElement'));
    };
    return aliases.filter(alias => requested.has(alias.name) && callsOf(alias).some(isFallback)).flatMap(alias => {
        const callee = `packageExports[${JSON.stringify(alias.name)}]`;
        const probes = [{
            name: alias.name, matches: isFallback,
            expression: `${callee}<Event>({} as EventTarget, '' as string, (_: Event): void => {})`,
        }];
        if (!callsOf(alias).some(isElementMap))
            return probes;
        const map = getChecker().getDeclaredTypeOfSymbol(domSymbol('HTMLElementEventMap'));
        const groups = new Map();
        for (const property of map.getProperties()) {
            const type = typeOf(property);
            const carrier = webType(type);
            if (!carrier || isDom(type, 'Event'))
                continue;
            const names = groups.get(carrier.name) ?? [];
            names.push(property.name);
            groups.set(carrier.name, names);
        }
        const typed = [...groups.keys()].sort().map(carrier => {
            const names = groups.get(carrier).sort();
            // An enum preserves the finite key domain in C#. The indexed access
            // also lets TypeScript verify that the selected npm overload accepts it.
            const keys = names.map(name => JSON.stringify(name)).join(' | ');
            return {
                name: alias.name, matches: isElementMap, names, enumName: `${carrier}Name`,
                expression: `${callee}<${keys}>({} as HTMLElement, '' as ${keys}, (_: HTMLElementEventMap[${keys}]): void => {})`,
            };
        });
        return [...typed, ...probes];
    });
}

export function readDomEventProjections(checker, probes, exportName, statements, diagnostics, readType, readReturn, definition, unsupported) {
    const results = [];
    for (const [index, probe] of probes.entries()) {
        if (probe.name !== exportName)
            continue;
        const statement = statements[index];
        if (diagnostics.some(diagnostic => diagnostic.start >= statement.pos && diagnostic.start < statement.end))
            continue;
        const expression = statement.declarationList.declarations[0].initializer;
        const signature = checker.getResolvedSignature(expression);
        if (!signature?.declaration || !probe.matches(checker.getSignatureFromDeclaration(signature.declaration)))
            continue;
        const returnType = readReturn(signature, new Set(), 'output', false, unsupported);
        if (returnType.kind !== 'function' || returnType.signatures.length !== 1 ||
            returnType.signatures[0].parameters.length || returnType.signatures[0].returnType.kind !== 'void')
            continue;
        const parameters = expression.arguments.map((argument, index) => ({
            name: signature.parameters[index].name,
            type: index === 1 && probe.names
                ? { kind: 'stringEnum', name: probe.enumName, stringLiterals: probe.names }
                : readType(checker.getTypeAtLocation(argument), new Set(), 'input', false, unsupported),
            optional: false,
        }));
        results.push({ parameters, returnType, definition: definition(signature.declaration) });
    }
    return results;
}
