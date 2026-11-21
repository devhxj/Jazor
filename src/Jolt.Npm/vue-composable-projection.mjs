export function vueComposableProjection(ts, checker, typeOf, vueRef, omitUndefined) {
    function getter(type) {
        const calls = type.getCallSignatures();
        if (!type.getProperties().length && calls.length === 1 && !calls[0].parameters.length &&
            !calls[0].typeParameters?.length && !calls[0].thisParameter)
            return calls[0];
    }

    function forwardsReadonlyRef(signature) {
        if (signature.typeParameters?.length !== 1 || signature.minArgumentCount < 1)
            return false;
        const [parameter] = signature.typeParameters;
        const constraint = checker.getBaseConstraintOfType(parameter);
        if (constraint && !(constraint.flags & ts.TypeFlags.Unknown))
            return false;
        const returned = signature.getReturnType();
        if (returned.isUnion())
            return false;
        const result = vueRef(returned);
        if (!result?.readOnly || !(result.plain || result.name === 'VueComputedRef') ||
            omitUndefined(result.element) !== parameter)
            return false;
        const input = typeOf(signature.parameters[0]);
        if (!input.isUnion())
            return getter(input)?.getReturnType() === parameter;
        // Every arm of a reactive source must carry the exact same generic slot.
        return input.types.includes(parameter) && input.types.every(arm => arm === parameter ||
            getter(arm)?.getReturnType() === parameter || vueRef(arm)?.element === parameter);
    }

    function unknownGetter(type, output) {
        if (getter(type)?.getReturnType().flags & ts.TypeFlags.Unknown &&
            output.kind === 'ref' && output.readOnly && output.elementType.kind === 'boolean') {
            // The upstream accepts any callback result. Expose its boolean subset
            // without changing JavaScript callback behavior or generating Func<object>.
            return { kind: 'function', signatures: [{ parameters: [], returnType: { kind: 'boolean' } }] };
        }
    }

    function contextualComputed(signature) {
        const [slot] = signature.typeParameters ?? [];
        if (!slot || signature.typeParameters.length !== 1 || signature.parameters.length !== 1 ||
            signature.minArgumentCount || checker.getBaseConstraintOfType(slot) ||
            slot.getDefault() && !(slot.getDefault().flags & ts.TypeFlags.Unknown))
            return false;
        const result = vueRef(signature.getReturnType());
        if (result?.name !== 'VueComputedRef' || !result.readOnly || omitUndefined(result.element) !== slot)
            return false;
        const selector = omitUndefined(typeOf(signature.parameters[0]));
        const variants = selector.isUnion() ? selector.types : [selector];
        const compatible = type => Boolean(type?.flags & ts.TypeFlags.String);
        return variants.some(compatible) && variants.every(type =>
            compatible(type) || compatible(vueRef(type)?.element) || compatible(getter(type)?.getReturnType()));
    }

    function constrainedRefDefault(signature) {
        if (signature.minArgumentCount || signature.typeParameters?.length !== 1)
            return false;
        const [parameter] = signature.typeParameters;
        const constraint = checker.getBaseConstraintOfType(parameter);
        const returned = signature.getReturnType();
        if (returned.isUnion())
            return false;
        const result = vueRef(returned);
        if (!constraint || !(constraint.flags & ts.TypeFlags.Object) || !result)
            return false;
        const element = omitUndefined(result.element);
        return element === parameter || element.isUnion() && element.types.every(arm =>
            arm === parameter || arm.flags & ts.TypeFlags.Null) && element.types.includes(parameter);
    }

    return { forwardsReadonlyRef, unknownGetter, contextualComputed, constrainedRefDefault };
}

export function omittedOptionsProbes(ts, checker, aliases, callsOf, requested, typeOf, isDate) {
    return aliases.filter(alias => requested.has(alias.name)).flatMap(alias => {
        const signatures = callsOf(alias);
        const signature = signatures[0];
        if (signature?.typeParameters?.length !== 1 || signature.minArgumentCount !== 1 ||
            signature.parameters.length < 2 || signature.thisParameter || checker.hasEffectiveRestParameter(signature))
            return [];
        const [parameter] = signature.typeParameters;
        const input = typeOf(signature.parameters[0]);
        const constraint = checker.getBaseConstraintOfType(parameter);
        // Probe distinct supported inputs through the actual call. Keep the
        // resulting inferred type only for this arity, where options are absent.
        const candidates = input.isUnion() ? input.types : [input];
        const literals = candidates.every(type => type.flags & ts.TypeFlags.StringLiteral) ?
            candidates.map(type => type.value).sort() : undefined;
        const samples = [...new Set(candidates.map(type => {
            if (literals)
                return JSON.stringify(type.value);
            if (parameter.getDefault() && constraint?.flags & ts.TypeFlags.String)
                return type.flags & ts.TypeFlags.String ? '"" as string' :
                    type.flags & ts.TypeFlags.Number ? '0 as number' : undefined;
        }).filter(Boolean))];
        if (parameter.getDefault() && candidates.some(isDate))
            samples.push('{} as Date');
        return samples.map(argument => ({
            name: alias.name,
            expression: `packageExports[${JSON.stringify(alias.name)}](${argument})`,
            domain: literals,
        }));
    });
}
