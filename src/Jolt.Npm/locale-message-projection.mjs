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
