// Bind the JavaScript Date surface through TypeScript's own signatures. The
// instantiation expressions are type queries only; runtime calls still use
// the original npm export and receive the original JavaScript values.
export function dateArguments(ts, program, checker, source) {
    const date = checker.resolveName('Date', source, ts.SymbolFlags.Type, false);
    const standardDate = date?.declarations?.length &&
        date.declarations.every(node => program.isSourceFileDefaultLibrary(node.getSourceFile()));
    const isDate = type => Boolean(standardDate && type.getSymbol() === date);

    return {
        isDate,
        predicateInputs(signature, index, type, singleSignature) {
            if (signature.parameters.length !== 1 || !(type.flags & ts.TypeFlags.Unknown))
                return;
            const predicate = checker.getTypePredicateOfSignature(signature);
            // An identifier predicate names a supported subset of its unknown
            // input. The binding still returns the function's boolean result.
            if (predicate?.kind === ts.TypePredicateKind.Identifier &&
                predicate.parameterIndex === index && isDate(predicate.type))
                return ['date', 'string', 'number', 'boolean'].map(kind => ({ kind }));
            if (!predicate && singleSignature && standardDate &&
                signature.getReturnType().flags & ts.TypeFlags.Boolean)
                // The declaration accepts every value. Expose the host's closed
                // Date and primitive subset without erasing it to object.
                return ['date', 'string', 'number', 'boolean'].map(kind => ({ kind }));
        },
        argumentsFor(signature, includeOptions = false) {
            if (!signature.typeParameters?.length)
                return;
            let hasDateArgument = false;
            const argumentsList = signature.typeParameters.map(parameter => {
                const constraint = checker.getBaseConstraintOfType(parameter);
                if (!constraint)
                    return;
                if (isDate(constraint)) {
                    hasDateArgument = true;
                    return 'Date';
                }
                const structure = dateStructure(constraint);
                if (structure) {
                    hasDateArgument = true;
                    return structure;
                }
                if (parameter.getDefault()?.flags & ts.TypeFlags.Undefined)
                    return includeOptions && scalarOptions(constraint) || 'undefined';
                if (!constraint.isUnion() || !constraint.types.some(isDate))
                    return;
                hasDateArgument = true;
                const branches = constraint.types.map(branch => {
                    if (isDate(branch))
                        return 'Date';
                    if (branch.flags & ts.TypeFlags.String)
                        return 'string';
                    if (branch.flags & ts.TypeFlags.Number)
                        return 'number';
                });
                // A date-like generic may also accept a custom constructor.
                // The host projection binds to the standard Date branch.
                return branches.every(Boolean) ? branches.join(' | ') : 'Date';
            });
            return hasDateArgument && argumentsList.every(Boolean) ? argumentsList.join(', ') : undefined;
        },
    };

    function dateStructure(constraint) {
        if (!(constraint.flags & (ts.TypeFlags.Object | ts.TypeFlags.Intersection)))
            return;
        const fields = constraint.getProperties().filter(property => {
            if (!(property.flags & ts.SymbolFlags.Property) || property.flags & ts.SymbolFlags.Optional ||
                !property.declarations?.every(node => node.name && (ts.isIdentifier(node.name) || ts.isStringLiteral(node.name))))
                return false;
            const type = checker.getTypeOfPropertyOfType(constraint, property.name);
            return type && (isDate(type) || type.isUnion() && type.types.some(isDate));
        });
        // TypeScript checks the synthesized argument against the original
        // constraint, including required non-date fields and brands.
        if (fields.length)
            return `{ ${fields.map(field => `${JSON.stringify(field.name)}: Date`).join('; ')} }`;
    }

    function scalarOptions(constraint) {
        if (!constraint.isUnion())
            return;
        const objects = constraint.types.filter(branch => !(branch.flags & ts.TypeFlags.Undefined));
        if (objects.length !== 1 || !(objects[0].flags & ts.TypeFlags.Object))
            return;
        const fields = [];
        for (const property of objects[0].getProperties()) {
            const type = checker.getTypeOfPropertyOfType(objects[0], property.name);
            const value = property.flags & ts.SymbolFlags.Optional ? checker.getNonNullableType(type) : type;
            const kind = value.flags & ts.TypeFlags.Number ? 'number' :
                value.flags & ts.TypeFlags.String ? 'string' :
                value.flags & ts.TypeFlags.Boolean ? 'boolean' : undefined;
            if (kind)
                fields.push(`${JSON.stringify(property.name)}${property.flags & ts.SymbolFlags.Optional ? '?' : ''}: ${kind}`);
        }
        // Keep the declared default as a branch. The checker evaluates both
        // branches of any conditional return type before metadata projection.
        if (fields.length)
            return `undefined | { ${fields.join('; ')} }`;
    }
}

export function dateProjection(aliases, callsOf, selected, argumentsFor) {
    const projections = [];
    for (const alias of aliases) {
        if (!selected.has(alias.name))
            continue;
        const typeArguments = new Set(callsOf(alias).flatMap(signature =>
            [argumentsFor(signature), argumentsFor(signature, true)]).filter(Boolean));
        for (const argumentsList of typeArguments) {
            projections.push({
                exportName: alias.name,
                argumentsList,
                expression: `packageExports[${JSON.stringify(alias.name)}]<${argumentsList}>`,
            });
        }
    }
    return projections;
}

export function readDateProjection(checker, statements, projections, argumentsFor, diagnostics) {
    const signaturesByExport = new Map();
    for (const [index, projection] of projections.entries()) {
        const statement = statements[index];
        // Independently instantiated overload groups can contain constraints
        // that fail. Keep those signatures on the ordinary unsupported path.
        if (diagnostics.some(diagnostic => diagnostic.start >= statement.pos && diagnostic.start < statement.end))
            continue;
        const initializer = statement.declarationList.declarations[0].initializer;
        const signatures = checker.getTypeAtLocation(initializer).getCallSignatures();
        const byDeclaration = signaturesByExport.get(projection.exportName) ?? new Map();
        for (const signature of signatures) {
            const original = signature.declaration && checker.getSignatureFromDeclaration(signature.declaration);
            if (signature.typeParameters?.length || !original)
                continue;
            const baseline = argumentsFor(original);
            const previous = byDeclaration.get(signature.declaration);
            // A broader options parameter also accepts the omitted call. Only
            // replace the baseline if the checker proves the same result type.
            const result = signature.getReturnType();
            const previousResult = previous?.getReturnType();
            if (baseline === projection.argumentsList || previousResult &&
                argumentsFor(original, true) === projection.argumentsList &&
                checker.isTypeAssignableTo(result, previousResult) &&
                checker.isTypeAssignableTo(previousResult, result))
                byDeclaration.set(signature.declaration, signature);
        }
        signaturesByExport.set(projection.exportName, byDeclaration);
    }
    return {
        signatures(exportName, signatures) {
            const byDeclaration = signaturesByExport.get(exportName);
            return signatures.map(signature => signature.typeParameters?.length
                ? byDeclaration?.get(signature.declaration) ?? signature
                : signature);
        },
    };
}
