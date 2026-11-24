export function readDynamicLocaleMessage(ts, checker, calls, definition) {
    const [call] = calls;
    if (!call || calls.some((candidate, index) => index > 0 && candidate.minArgumentCount <= 1) ||
        call.parameters.length !== 1 || call.minArgumentCount !== 1 || call.thisParameter ||
        checker.hasEffectiveRestParameter(call) || call.typeParameters?.length !== 5)
        return;
    const [key, keys, , , result] = call.typeParameters;
    const str = checker.getStringType();
    const input = checker.getTypeOfSymbol(call.parameters[0]);
    if (checker.getBaseConstraintOfType(key) !== str || key.getDefault() ||
        input !== key && (!input.isUnion() || input.types.length !== 2 || !input.types.includes(key) || !input.types.includes(keys)) ||
        call.getReturnType() !== result)
        return;
    const nodes = call.declaration.typeParameters;
    const [keysNode, localeNode, targetNode, resultNode] = nodes.slice(1).map(node => node.default);
    if (!keysNode || !localeNode || !targetNode || !resultNode ||
        !ts.isTypeReferenceNode(keysNode) || keysNode.typeArguments?.length !== 1 ||
        !ts.isTypeReferenceNode(localeNode) || localeNode.typeArguments?.length !== 1 ||
        !ts.isConditionalTypeNode(targetNode) || !ts.isConditionalTypeNode(resultNode))
        return;
    const symbol = node => ts.isTypeReferenceNode(node) ? checker.getSymbolAtLocation(node.typeName) : checker.getTypeFromTypeNode(node).symbol;
    const unwrap = node => {
        if (!ts.isTypeReferenceNode(node) || node.typeArguments?.length !== 1)
            return;
        const utility = checker.getSymbolAtLocation(node.typeName);
        if (utility?.name === 'NonNullable' && utility.declarations?.some(declaration => declaration.getSourceFile().hasNoDefaultLib))
            return symbol(node.typeArguments[0]);
    };
    const resource = symbol(keysNode.typeArguments[0]);
    const test = targetNode.checkType;
    const indexed = targetNode.trueType;
    if (!resource || !call.declaration.parent.typeParameters?.some(slot => checker.getSymbolAtLocation(slot.name) === resource) ||
        unwrap(localeNode.typeArguments[0]) !== resource ||
        !ts.isTypeReferenceNode(test) || test.typeArguments?.length !== 1 || symbol(test.typeArguments[0]) !== resource ||
        checker.getTypeFromTypeNode(targetNode.extendsType) !== checker.getFalseType() ||
        !ts.isIndexedAccessTypeNode(indexed) || unwrap(indexed.objectType) !== resource || symbol(indexed.indexType) !== checker.getSymbolAtLocation(nodes[2].name) ||
        symbol(resultNode.checkType) !== checker.getSymbolAtLocation(nodes[1].name) ||
        !ts.isTypeReferenceNode(resultNode.extendsType) || resultNode.extendsType.typeArguments?.length !== 1 ||
        symbol(resultNode.extendsType.typeArguments[0]) !== checker.getSymbolAtLocation(nodes[3].name) ||
        !ts.isTypeReferenceNode(resultNode.trueType) || resultNode.trueType.typeArguments?.length !== 2 ||
        symbol(resultNode.trueType.typeArguments[0]) !== checker.getSymbolAtLocation(nodes[3].name) || symbol(resultNode.trueType.typeArguments[1]) !== checker.getSymbolAtLocation(nodes[1].name))
        return;
    const fallback = checker.getTypeFromTypeNode(resultNode.falseType);
    const index = checker.getIndexInfoOfType(fallback, ts.IndexKind.String);
    if (!index || !(index.type.flags & (ts.TypeFlags.Any | ts.TypeFlags.Unknown)) || fallback.getProperties().length)
        return;
    // Erase the resource result as-is. The runtime resolver and locale fallback
    // determine its shape, which the consumer can inspect through UnknownValue.
    return { parameters: [{ name: call.parameters[0].name, type: { kind: 'string' }, optional: false }],
        returnType: { kind: 'unknownValue' }, definition: definition(call.declaration) };
}

export function readStaticLocaleWriter(ts, checker, calls, composerSymbol, methodName, resources) {
    const [call] = calls;
    const node = call?.declaration;
    const merge = methodName === 'mergeLocaleMessage';
    if (!composerSymbol || (!merge && methodName !== 'setLocaleMessage') || calls.length !== 1 || !node ||
        !ts.isMethodSignature(node) || node.name.getText() !== methodName ||
        checker.getSymbolAtLocation(node.parent.name) !== composerSymbol ||
        call.parameters.length !== 2 || call.minArgumentCount !== 2 || call.thisParameter ||
        checker.hasEffectiveRestParameter(call) || call.getReturnType() !== checker.getVoidType() ||
        call.typeParameters?.length !== (merge ? 4 : 5))
        return false;

    const [schema, locale, localeKey, payload, message] = call.typeParameters;
    const asType = syntax => syntax && checker.getTypeFromTypeNode(syntax);
    const matches = (syntax, slot) => asType(syntax)?.symbol === slot.symbol;
    const [localeParameter, messageParameter] = call.parameters.map(parameter => checker.getTypeOfSymbol(parameter));
    const string = checker.getStringType();
    if (schema.getDefault()?.flags !== ts.TypeFlags.Never ||
        locale.getDefault() !== string || checker.getBaseConstraintOfType(locale) !== string ||
        !localeParameter.isUnion() || localeParameter.types.length !== 2 ||
        !localeParameter.types.includes(locale) || !localeParameter.types.includes(localeKey) ||
        messageParameter !== (message ?? payload))
        return false;

    const fallback = node.typeParameters[3].default;
    if (!fallback || !ts.isConditionalTypeNode(fallback) ||
        !checksNever(ts, checker, fallback.checkType, schema) ||
        asType(fallback.extendsType) !== checker.getTrueType() || !matches(fallback.falseType, schema))
        return false;

    // Resolve the instantiated branch against the concrete Composer receiver.
    const defaults = payload.getDefault();
    defaults?.getProperties();
    const branch = defaults?.resolvedTrueType;
    if (!branch)
        return false;

    if (merge) {
        const record = branch;
        const [index, ...others] = checker.getIndexInfosOfType(record);
        const valid = index?.keyType === string && !others.length && index.type.flags === ts.TypeFlags.Any &&
            !record.getProperties().length && !record.getCallSignatures().length && !record.getConstructSignatures().length;
        return valid ? () => true : undefined;
    }

    const localeDefault = localeKey.getDefault();
    const localeConstraint = checker.getBaseConstraintOfType(localeKey);
    if (!resources || !localeDefault || !localeConstraint ||
        message.getDefault() !== payload || !matches(node.typeParameters[4].constraint, payload) ||
        !(branch.flags & ts.TypeFlags.IndexedAccess) || branch.objectType !== resources || branch.indexType !== localeKey)
        return;
    // A resource map may also contain numeric keys outside the setter's string domain.
    return name => {
        const key = checker.getStringLiteralType(name);
        return checker.isTypeAssignableTo(key, localeDefault) && checker.isTypeAssignableTo(key, localeConstraint);
    };
}

export function readLocaleMessageMethod(ts, checker, calls, definition) {
    if (calls.length !== 1)
        return;
    const [call] = calls;
    const [schema, locale, localeKey, payload, message] = call.typeParameters ?? [];
    if (!payload || call.typeParameters.length > 5 || call.thisParameter ||
        checker.hasEffectiveRestParameter(call) || call.minArgumentCount !== call.parameters.length ||
        schema.getDefault()?.flags !== ts.TypeFlags.Never || locale.getDefault()?.flags !== ts.TypeFlags.String ||
        checker.getBaseConstraintOfType(locale)?.flags !== ts.TypeFlags.String || !localeKey.getDefault())
        return;
    const constraint = checker.getBaseConstraintOfType(schema);
    const index = constraint && checker.getIndexInfoOfType(constraint, ts.IndexKind.String);
    if (!index || constraint.getProperties().length || constraint.getCallSignatures().length)
        return;
    const [localeParameter, messageParameter] = call.parameters;
    const localeType = localeParameter && checker.getTypeOfSymbol(localeParameter);
    if (!localeType?.isUnion() || !localeType.types.includes(locale) || !localeType.types.includes(localeKey) ||
        localeType.types.length !== 2)
        return;

    // The never schema default selects the receiver's message schema. On an
    // unspecialized receiver it can be empty or open, and missing locales
    // return an empty object, so expose only its record contract.
    const declaration = payload.symbol.declarations[0];
    if (!declaration.default || !ts.isConditionalTypeNode(declaration.default) ||
        checker.getTypeFromTypeNode(declaration.default.falseType).symbol !== schema.symbol ||
        declaration.default.extendsType.kind !== ts.SyntaxKind.LiteralType ||
        declaration.default.extendsType.literal.kind !== ts.SyntaxKind.TrueKeyword ||
        !checksNever(ts, checker, declaration.default.checkType, schema))
        return;
    const result = call.getReturnType();
    const isRead = call.parameters.length === 1 && !message && result === payload;
    const isWrite = call.parameters.length === 2 && result.flags === ts.TypeFlags.Void &&
        checker.getTypeOfSymbol(messageParameter) === (message ?? payload) &&
        (!message || message.getDefault() === payload && message.symbol.declarations[0].constraint &&
            checker.getTypeFromTypeNode(message.symbol.declarations[0].constraint).symbol === payload.symbol);
    if (!isRead && !isWrite)
        return;
    const defaults = payload.getDefault();
    if (!defaults || defaults.flags !== ts.TypeFlags.Conditional ||
        defaults.getProperties().length || defaults.getCallSignatures().length || defaults.getConstructSignatures().length)
        return;
    // Use the instantiated conditional branch. The syntax branch can still
    // refer to the uninstantiated receiver's generic message map.
    const defaultBranch = defaults.resolvedTrueType;
    if (!defaultBranch || !(defaultBranch.flags & ts.TypeFlags.Object) ||
        defaultBranch.getProperties().length ||
        defaultBranch.getCallSignatures().length || defaultBranch.getConstructSignatures().length ||
        checker.getIndexInfosOfType(defaultBranch).some(info =>
            !(info.type.flags & (ts.TypeFlags.Any | ts.TypeFlags.Unknown))))
        return;
    const record = { kind: 'plainRecord' };
    const parameters = [{ name: localeParameter.name, type: { kind: 'string' }, optional: false }];
    if (isWrite)
        parameters.push({ name: messageParameter.name, type: record, optional: false });
    return { parameters, returnType: isRead ? record : { kind: 'void' }, definition: definition(call.declaration) };
}

export function localeMessageMaps(ts, checker, properties, definition) {
    const result = new Set();
    for (const property of properties) {
        for (const method of property.declarations ?? []) {
            if (!ts.isMethodSignature(method) || method.parameters.length !== 1 ||
                !method.typeParameters?.[3]?.default)
                continue;
            const calls = checker.getTypeOfSymbol(property).getCallSignatures();
            if (readLocaleMessageMethod(ts, checker, calls, definition)?.returnType.kind !== 'plainRecord')
                continue;
            const branch = method.typeParameters[3].default.trueType;
            const index = branch && ts.isConditionalTypeNode(branch) && branch.falseType;
            if (!index || !ts.isIndexedAccessTypeNode(index) || !ts.isTypeReferenceNode(index.objectType) ||
                !ts.isTypeReferenceNode(index.indexType) ||
                checker.getSymbolAtLocation(index.indexType.typeName) !== checker.getSymbolAtLocation(method.typeParameters[2].name))
                continue;
            const unwrap = index.objectType;
            const nonNullable = checker.getSymbolAtLocation(unwrap.typeName);
            if (nonNullable?.name !== 'NonNullable' || !nonNullable.declarations?.some(node => node.getSourceFile().hasNoDefaultLib))
                continue;
            const resource = unwrap.typeArguments?.[0];
            const resourceSymbol = resource && checker.getTypeFromTypeNode(resource).symbol;
            if (!resourceSymbol || !method.parent.typeParameters?.some(slot => checker.getSymbolAtLocation(slot.name) === resourceSymbol))
                continue;
            // Correlate the getter's indexed fallback with the exact receiver
            // slot used by the computed map. Format maps use different slots.
            for (const member of method.parent.members) {
                if (!ts.isPropertySignature(member) || !member.type || !ts.isTypeReferenceNode(member.type))
                    continue;
                const map = member.type.typeArguments?.[0];
                const constraint = map && ts.isMappedTypeNode(map) && map.typeParameter.constraint;
                if (constraint && ts.isTypeOperatorNode(constraint) && constraint.operator === ts.SyntaxKind.KeyOfKeyword &&
                    checker.getTypeFromTypeNode(constraint.type).symbol === resourceSymbol &&
                    map.type && ts.isIndexedAccessTypeNode(map.type) &&
                    checker.getTypeFromTypeNode(map.type.objectType).symbol === resourceSymbol &&
                    checker.getTypeFromTypeNode(map.type.indexType).symbol === checker.getSymbolAtLocation(map.typeParameter.name))
                    result.add(member);
            }
        }
    }
    return result;
}

export function readStringMessageCall(ts, checker, calls, definition) {
    // Later formatting overloads require a second argument. Every signature
    // that can receive the plain string call must preserve a string result.
    const singleArgument = calls.filter(call => call.minArgumentCount <= 1);
    if (!singleArgument.length || singleArgument.some(call => call.parameters.length !== 1 ||
        call.minArgumentCount !== 1 || call.typeParameters?.length || call.thisParameter ||
        checker.hasEffectiveRestParameter(call) || call.getReturnType().flags !== ts.TypeFlags.String))
        return;
    const str = checker.getStringType();
    const call = singleArgument.find(call => {
        const input = checker.getTypeOfSymbol(call.parameters[0]);
        return input.isUnion() && input.types.includes(str) &&
            input.types.some(branch => branch.getCallSignatures().length);
    });
    if (!call)
        return;
    const type = { kind: 'string' };
    return { parameters: [{ name: call.parameters[0].name, type, optional: false }],
        returnType: type, definition: definition(call.declaration) };
}

function checksNever(ts, checker, expression, schema) {
    if (!ts.isTypeReferenceNode(expression) || expression.typeArguments?.length !== 1 ||
        checker.getTypeFromTypeNode(expression.typeArguments[0]).symbol !== schema.symbol)
        return false;
    const reference = checker.getSymbolAtLocation(expression.typeName);
    const symbol = reference?.flags & ts.SymbolFlags.Alias ? checker.getAliasedSymbol(reference) : reference;
    const alias = symbol?.declarations?.[0];
    if (!alias || !ts.isTypeAliasDeclaration(alias) || alias.typeParameters?.length !== 1 ||
        !ts.isConditionalTypeNode(alias.type))
        return false;
    const { checkType, extendsType, trueType, falseType } = alias.type;
    return ts.isTupleTypeNode(checkType) && checkType.elements.length === 1 &&
        checker.getTypeFromTypeNode(checkType.elements[0]).symbol === checker.getSymbolAtLocation(alias.typeParameters[0].name) &&
        ts.isTupleTypeNode(extendsType) && extendsType.elements.length === 1 &&
        extendsType.elements[0].kind === ts.SyntaxKind.NeverKeyword &&
        ts.isLiteralTypeNode(trueType) && trueType.literal.kind === ts.SyntaxKind.TrueKeyword &&
        ts.isLiteralTypeNode(falseType) && falseType.literal.kind === ts.SyntaxKind.FalseKeyword;
}
