function readOptionalSlot(ts, checker, field, read) {
    const branches = field.isUnion() ? field.types : [field];
    // exactOptionalPropertyTypes distinguishes the checker's missing-property
    // marker from authored undefined, which has a different write contract.
    if (branches.includes(checker.getUndefinedType()))
        return;
    const value = branches.filter(branch => !(branch.flags & (ts.TypeFlags.Null | ts.TypeFlags.Undefined)));
    if (value.length !== 1 || !(value[0].flags & ts.TypeFlags.TypeParameter))
        return;
    const elementType = read(value[0]);
    if (elementType?.kind === 'typeParameter')
        return { kind: 'optionalSlot', elementType, name: branches.some(branch => branch.flags & ts.TypeFlags.Null) ? 'nullable' : undefined };
}

export function readNamedRecord(ts, checker, program, type, suppliesValue, activeRecords, read, finiteArgument, definition, recordIdentity, referenceOnly) {
    if (!(type.flags & ts.TypeFlags.Object) || type.getCallSignatures().length ||
        type.getConstructSignatures().length || checker.getIndexInfosOfType(type).length)
        return;

    // A type alias of an instantiated interface still has the interface target.
    const target = type.target?.getSymbol();
    const namedInterface = target?.flags & ts.SymbolFlags.Interface ? target : undefined;
    const symbol = namedInterface ?? type.aliasSymbol;
    const [declaration] = symbol?.declarations ?? [];
    const formals = declaration?.typeParameters;
    if (symbol?.declarations?.length !== 1 || !formals?.length ||
        program.isSourceFileDefaultLibrary(declaration.getSourceFile()) ||
        !(ts.isSourceFile(declaration.parent) || ts.isModuleBlock(declaration.parent)) ||
        !(ts.isInterfaceDeclaration(declaration) || ts.isTypeAliasDeclaration(declaration) && ts.isTypeLiteralNode(declaration.type)) ||
        formals.some(parameter => parameter.constraint))
        return;

    const actuals = namedInterface ? checker.getTypeArguments(type) : type.aliasTypeArguments;
    if (actuals?.length !== formals.length)
        return;

    const finiteArguments = actuals.every(finiteArgument);
    const reference = {
        kind: 'namedRecord', name: declaration.name.text, typeDefinition: definition(declaration), typeIdentity: recordIdentity(declaration),
        typeParameters: formals.map(formal => formal.name.text),
    };
    if (activeRecords.has(symbol)) {
        // 每条递归边只允许替换为原有类型槽或标量，确保封闭类型图有穷。
        // The first frame's incoming edge lies outside the cycle.
        const stack = [...activeRecords];
        const start = stack.findIndex(([record]) => record === symbol);
        if (!finiteArguments || stack.slice(start + 1).some(([, finite]) => !finite))
            return;
        return { ...reference, typeArguments: actuals.map(argument => read(argument, false)) };
    }
    if (referenceOnly)
        return;

    const template = checker.getDeclaredTypeOfSymbol(symbol);
    const slots = formals.map(formal => checker.getTypeAtLocation(formal.name));
    const scope = new Set(slots);
    const evidence = new Set();
    const fields = template.getProperties();
    if (!fields.length || fields.length !== type.getProperties().length)
        return;

    const members = [];
    activeRecords.set(symbol, finiteArguments);
    try {
        for (const field of fields) {
            const [property] = field.declarations ?? [];
            if (field.declarations?.length !== 1 || !ts.isPropertySignature(property) ||
                (!ts.isIdentifier(property.name) && !ts.isStringLiteral(property.name)))
                return;

            const optional = Boolean(field.flags & ts.SymbolFlags.Optional);
            const readOnly = Boolean(ts.getCombinedModifierFlags(property) & ts.ModifierFlags.Readonly);
            const value = checker.getTypeOfPropertyOfType(template, field.name);
            // One C# declaration handles input construction and result reads.
            // A typed record argument supplies its generic argument even when
            // the optional field is omitted. Target-typed new still needs context.
            const fieldType = optional
                ? readOptionalSlot(ts, checker, value, slot => read(slot, true, scope, evidence))
                : read(value, true, scope, evidence);
            if (!fieldType)
                return;

            members.push({ name: field.name, type: fieldType, readOnly, optional, definition: definition(property) });
        }
    } finally {
        activeRecords.delete(symbol);
    }

    // A formal absent from value positions cannot infer the caller's payload.
    const argumentsList = actuals.map((argument, index) => read(argument, suppliesValue && evidence.has(slots[index])));
    if (argumentsList.some(argument => !argument))
        return;

    return { ...reference, typeArguments: argumentsList, members };
}

export function readFiniteMappedRecord(ts, checker, program, type, suppliesValue, read, omitUndefined, definition, recordIdentity, annotation) {
    if (!(type.objectFlags & ts.ObjectFlags.Mapped) || checker.getIndexInfosOfType(type).length)
        return;

    // Inspect the checker template, which retains the standard utility symbol
    // when a package aliases an instantiated Pick or Partial.
    const standardAlias = (candidate, name) => candidate?.aliasSymbol?.name === name &&
        candidate.aliasSymbol.declarations?.every(node => program.isSourceFileDefaultLibrary(node.getSourceFile()));
    const mappedDefinition = candidate => candidate?.declaration?.parent?.symbol;
    const standardMapped = (candidate, name) => standardAlias(candidate, name) ||
        mappedDefinition(candidate)?.name === name &&
        program.isSourceFileDefaultLibrary(candidate.declaration.getSourceFile());
    const properties = type.getProperties();
    const [outerArgument] = type.aliasTypeArguments ?? [];
    const pick = standardMapped(type, 'Partial') ? type.modifiersType ?? outerArgument : type;
    if (!standardMapped(pick, 'Pick'))
        return;
    if (!properties.length || checker.getIndexInfosOfType(pick).length)
        return;
    const members = [];
    for (const property of properties) {
        const [declaration] = property.declarations ?? [];
        // Computed symbol keys have checker-internal names, not string wire keys.
        if (!declaration || !ts.isPropertySignature(declaration) ||
            !(ts.isIdentifier(declaration.name) || ts.isStringLiteral(declaration.name) || ts.isNumericLiteral(declaration.name)))
            return;
        const optional = Boolean(property.flags & ts.SymbolFlags.Optional);
        const field = checker.getTypeOfPropertyOfType(type, property.name);
        const optionalSlot = optional && readOptionalSlot(ts, checker, field, slot => read(slot, suppliesValue));
        const defined = optional ? omitUndefined(field) : field;
        const branches = defined.isUnion() ? defined.types : [];
        const admitsNull = branches.some(branch => branch.flags & ts.TypeFlags.Null);
        const nonNull = admitsNull ? branches.filter(branch => !(branch.flags & ts.TypeFlags.Null)) : [];
        const underlying = optionalSlot || (admitsNull
            ? nonNull.length && read(checker.getUnionType(nonNull), suppliesValue && !optional)
            : read(defined, suppliesValue && !optional));
        // Direct optional slots use a dedicated carrier. Required T|null still
        // needs a separate carrier because unconstrained T? cannot represent it.
        if (!underlying || (optional || admitsNull) && underlying.kind === 'typeParameter')
            return;
        const projected = admitsNull && !optionalSlot ? { kind: 'nullable', elementType: underlying } : underlying;
        const readOnly = Boolean(ts.getCheckFlags(property) & ts.CheckFlags.Readonly) ||
            Boolean(ts.getCombinedModifierFlags(declaration) & ts.ModifierFlags.Readonly);
        members.push({ name: property.name, type: projected, optional, readOnly, definition: definition(declaration) });
    }
    const shape = { kind: 'object', members };
    let symbol = annotation && ts.isTypeReferenceNode(annotation) && checker.getSymbolAtLocation(annotation.typeName);
    if (symbol && symbol.flags & ts.SymbolFlags.Alias)
        symbol = checker.getAliasedSymbol(symbol);
    const [alias] = (symbol || type.aliasSymbol)?.declarations ?? [];
    if (!alias || !ts.isTypeAliasDeclaration(alias) || program.isSourceFileDefaultLibrary(alias.getSourceFile()))
        return shape;
    const actuals = symbol ? (annotation.typeArguments ?? []).map(node => checker.getTypeFromTypeNode(node)) : type.aliasTypeArguments ?? [];
    if (actuals.length !== (alias.typeParameters?.length ?? 0))
        return shape;
    const typeArguments = actuals.map(argument => read(argument, false));
    if (typeArguments.some(argument => !argument))
        return shape;
    const identity = recordIdentity(alias);
    const keys = members.map(member => member.name).sort();
    return {
        ...shape, name: alias.name.text, typeDefinition: definition(alias),
        typeIdentity: identity && JSON.stringify([identity, keys]), typeArguments,
    };
}
