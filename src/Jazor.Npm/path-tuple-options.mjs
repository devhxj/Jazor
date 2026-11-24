export function readPathTupleOptions(ts, checker, signature, typeOf, definition, valueSlot, extrasSlot) {
    const config = signature.parameters[1];
    if (signature.parameters.length !== 2 || !config.valueDeclaration ||
        !checker.isOptionalParameter(config.valueDeclaration))
        return;

    const domain = typeOf(config);
    const branches = (domain.isUnion() ? domain.types : [domain])
        .filter(branch => !(branch.flags & ts.TypeFlags.Undefined));
    const records = branches.filter(branch => !branch.getCallSignatures().length);
    const functions = branches.filter(branch => branch.getCallSignatures().length);
    const staticOptions = records.length === 1 ? readOptions(records[0], false) : [];
    const lazyCall = functions.length === 1 ? readStateCallback(functions[0]) : undefined;
    const lazyOptions = lazyCall ? readOptions(lazyCall.call.getReturnType(), true) : [];
    const options = (members, callback) => {
        const record = { kind: 'object', members };
        return { name: config.name, optional: false, type: callback ? callbackType(callback, record) : record };
    };
    return [...staticOptions.map(({ members, hasExtras }) => ({ parameter: options(members), hasExtras })),
        ...lazyOptions.map(({ members, hasExtras }) => ({ parameter: options(members, lazyCall), hasExtras,
            // The static record owns target-typed new(); a lambda can only bind to this overload.
            overloadPriority: -1 }))];

    function readOptions(record, lazy) {
        if (!(record.flags & ts.TypeFlags.Object) || record.getCallSignatures().length ||
            record.getConstructSignatures().length || checker.getIndexInfosOfType(record).length)
            return [];
        const properties = record.getProperties();
        if (!properties.every(property => property.flags & ts.SymbolFlags.Optional))
            return [];

        const string = checker.getStringType();
        const boolean = checker.getBooleanType();
        const extras = { kind: 'recordTypeParameter', name: 'TProps' };
        const scalars = [];
        const extrasMembers = [];
        for (const property of properties) {
            const [declaration] = property.declarations ?? [];
            if (property.declarations?.length !== 1 || !ts.isPropertySignature(declaration) ||
                !ts.isIdentifier(declaration.name) && !ts.isStringLiteral(declaration.name))
                continue;
            const type = checker.getTypeOfPropertyOfType(record, property.name);
            const arms = type.isUnion() ? type.types : [type];
            const kind = type === boolean || arms.includes(checker.getTrueType()) && arms.includes(checker.getFalseType())
                ? 'boolean' : arms.includes(string) ? 'string' : undefined;
            const metadata = { name: property.name, readOnly: false, definition: definition(declaration) };
            if (kind) {
                scalars.push({ ...metadata, type: { kind }, optional: true });
                continue;
            }
            if (!extrasSlot)
                continue;
            const candidate = checker.getNonNullableType(type);
            if (lazy) {
                if (candidate === extrasSlot)
                    extrasMembers.push({ ...metadata, type: extras, optional: false });
            } else {
                const callback = readStateCallback(candidate);
                if (callback?.call.getReturnType() === extrasSlot)
                    extrasMembers.push({ ...metadata, type: callbackType(callback, extras), optional: false });
            }
        }
        // Omitted extras keep the base events. A typed props result needs a
        // required props member in each value the author can return.
        return [...(scalars.length ? [{ members: scalars, hasExtras: false }] : []),
            ...(extrasMembers.length === 1 ? [{ members: [...scalars, extrasMembers[0]], hasExtras: true }] : [])];
    }

    function readStateCallback(callback) {
        const [call] = callback.getCallSignatures();
        if (callback.getCallSignatures().length !== 1 || callback.getProperties().length || callback.getConstructSignatures().length ||
            checker.getIndexInfosOfType(callback).length || call.typeParameters?.length || call.thisParameter ||
            checker.hasEffectiveRestParameter(call) || call.parameters.length !== 1 || call.minArgumentCount !== 1)
            return;
        const state = typeOf(call.parameters[0]);
        const projectedState = readFieldState(ts, checker, state, valueSlot, definition);
        if (projectedState)
            return { call, state: projectedState };
    }

    function callbackType({ call, state }, returnType) {
        return { kind: 'function', signatures: [{
            parameters: [{ name: call.parameters[0].name, type: state, optional: false }],
            returnType, definition: definition(call.declaration),
        }] };
    }
}

function readFieldState(ts, checker, state, valueSlot, definition) {
    if (!(state.flags & ts.TypeFlags.Object) || state.getCallSignatures().length ||
        state.getConstructSignatures().length || checker.getIndexInfosOfType(state).length)
        return;

    const string = checker.getStringType();
    const boolean = checker.getBooleanType();
    const field = { kind: 'typeParameter', name: 'TField' };
    const members = state.getProperties().flatMap(property => {
        const [declaration] = property.declarations ?? [];
        if (property.declarations?.length !== 1 || !ts.isPropertySignature(declaration) ||
            !ts.isIdentifier(declaration.name) && !ts.isStringLiteral(declaration.name))
            return [];

        const type = checker.getTypeOfPropertyOfType(state, property.name);
        const branches = type.isUnion() ? type.types : [type];
        const payload = branches.filter(branch => !(branch.flags & ts.TypeFlags.Undefined));
        const optional = Boolean(property.flags & ts.SymbolFlags.Optional) || payload.length !== branches.length;
        // Read a payload only when this occurrence uses the exact model slot.
        // Keep scalar state such as dirty/touched and the list of errors typed.
        const value = payload.length === 1 && payload[0];
        const projected = value === valueSlot ? { kind: 'nullableSlot', elementType: field } :
            value === string ? { kind: 'string' } : type === boolean ? { kind: 'boolean' } :
                value && checker.isArrayType(value) && checker.getElementTypeOfArrayType(value) === string
                    ? { kind: 'array', readOnly: true, elementType: { kind: 'string' } } : undefined;
        if (!projected)
            return [];
        return [{ name: property.name, type: projected, optional: optional || value === valueSlot,
            readOnly: true, definition: definition(declaration) }];
    });
    if (members.some(member => member.type.kind === 'nullableSlot'))
        return { kind: 'object', members };
}
