export function readSubmissionProjection(ts, checker, calls, typeOf, promise, domSymbol, definition) {
    const [factory] = calls;
    if (calls.length !== 1 || factory.minArgumentCount !== 1 || factory.parameters.length !== 1 ||
        factory.typeParameters?.length !== 1 || !promise)
        return;
    const [values] = factory.typeParameters;
    const constraint = checker.getBaseConstraintOfType(values);
    const index = constraint && checker.getIndexTypeOfType(constraint, ts.IndexKind.String);
    if (!index || !(index.flags & ts.TypeFlags.Unknown))
        return;
    const callbacks = typeOf(factory.parameters[0]).getCallSignatures();
    const invocations = factory.getReturnType().getCallSignatures();
    if (callbacks.length !== 1 || invocations.length !== 1)
        return;
    const [handler] = callbacks;
    const [submit] = invocations;
    if (handler.parameters.length !== 2 || typeOf(handler.parameters[0]) !== values ||
        !(handler.getReturnType().flags & ts.TypeFlags.Unknown) ||
        submit.minArgumentCount !== 0 || submit.parameters.length !== 1)
        return;
    const event = checker.getNonNullableType(typeOf(submit.parameters[0]));
    const payload = submit.getReturnType();
    if (event.getSymbol() !== domSymbol('Event') || payload.target?.getSymbol() !== promise ||
        !(checker.getTypeArguments(payload)[0]?.flags & ts.TypeFlags.Unknown))
        return;
    const parameter = (name, type, optional = false) => ({ name, type, optional });
    const callable = (parameters, returnType) => ({ kind: 'function', signatures: [{ parameters, returnType }] });
    const name = values.symbol.name;
    return {
        // A one-argument callback can ignore the contextual argument. The
        // injected form may be absent, and the Promise payload is unconstrained.
        parameters: [parameter(factory.parameters[0].name, callable([
            parameter(handler.parameters[0].name, { kind: 'recordTypeParameter', name }),
        ], { kind: 'void' }))],
        returnType: callable([
            parameter(submit.parameters[0].name,
                { kind: 'undefinedable', elementType: { kind: 'web', name: 'EventRef' } }, true),
        ], { kind: 'undefinedable', elementType: { kind: 'promise', elementType: { kind: 'void' } } }),
        typeParameters: [name],
        definition: definition(factory.declaration),
    };
}
