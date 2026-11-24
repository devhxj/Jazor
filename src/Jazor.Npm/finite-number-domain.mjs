export function finiteNumberLiterals(ts, arms) {
    if (arms.length && arms.every(arm => arm.flags === ts.TypeFlags.NumberLiteral && Number.isFinite(arm.value)))
        return arms.map(arm => arm.value).sort((a, b) => a - b);
}

export function namedLiteralDomain(ts, type, definition, recordIdentity) {
    const alias = type?.aliasSymbol?.declarations?.find(node =>
        ts.isTypeAliasDeclaration(node) && !node.typeParameters?.length);
    if (alias)
        return { name: alias.name.text, typeDefinition: definition(alias), typeIdentity: recordIdentity(alias) };
}

export function namedNumberDomain(ts, checker, type, annotation, numbers, definition, recordIdentity) {
    const domain = namedLiteralDomain(ts, type, definition, recordIdentity);
    if (domain || !annotation || !ts.isTypeReferenceNode(annotation))
        return domain;
    // Removing undefined can rebuild the union without its alias. Recover it
    // from the property declaration only for the same instantiated value set.
    const annotated = checker.getTypeFromTypeNode(annotation);
    const declared = annotated.isUnion() && finiteNumberLiterals(ts, annotated.types);
    if (declared && declared.length === numbers.length && declared.every((number, i) => number === numbers[i]))
        return namedLiteralDomain(ts, annotated, definition, recordIdentity);
}
