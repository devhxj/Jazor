export const contractTypes = componentType => `
    type Shape<C> = C extends abstract new (...args: any[]) => infer I
        ? I extends { $props: infer P; $emit: infer E; $slots: infer S }
            ? { props: P; emit: E; slots: S }
            : never
        : C extends (props: infer P, ...args: any[]) => { __ctx?: infer Context }
            ? NonNullable<Context> extends { emit: infer E; slots: infer S }
                ? { props: P; emit: E; slots: S }
                : never
            : never;
    type Instance = Shape<${componentType}>;
    type Props = Omit<Instance['props'], VueProps>;
    type PropValues = Required<Props>;
    type Emit = Instance['emit'];
    type Slots = Instance['slots'];
`;

export function readComponent(exportSymbol, { ts, source, checker, typeOf, display, optional, definition, identity, exportName }) {
    const types = Object.fromEntries(source.statements.filter(ts.isTypeAliasDeclaration)
        .map(alias => [alias.name.text, checker.getTypeAtLocation(alias.type)]));
    // Conditional inference through Shape widens generic parameters to their
    // bounds. Read the declaration's own call signature for the array-inferred
    // slice so props, callbacks and scoped slots retain the same type identity.
    let typeParameter;
    const [call] = typeOf(exportSymbol).getCallSignatures();
    if (call?.typeParameters?.length === 1) {
        const [parameter] = call.typeParameters;
        const context = call.getReturnType().getProperty('__ctx');
        const authoredProps = typeOf(call.parameters[0]);
        const isCollection = prop => {
            const type = typeOf(prop);
            return checker.isArrayType(type) && checker.getTypeArguments(type)[0] === parameter;
        };
        if (context && authoredProps.getProperties().some(prop => !optional(prop) && isCollection(prop))) {
            const publicContext = checker.getNonNullableType(typeOf(context));
            types.Props = authoredProps;
            types.Slots = typeOf(publicContext.getProperty('slots'));
            types.Emit = typeOf(publicContext.getProperty('emit'));
            const constraint = checker.getBaseConstraintOfType(parameter);
            const branches = constraint ? constraint.isUnion() ? constraint.types : [constraint] : [];
            const primitiveKinds = constraint ? [
                [ts.TypeFlags.String, 'string'],
                [ts.TypeFlags.Number, 'number'],
                [ts.TypeFlags.Boolean, 'boolean'],
            ].filter(([flag]) => branches.some(type => type.flags & flag)).map(([, name]) => name)
                : ['string', 'number', 'boolean'];
            typeParameter = { name: display(parameter), constraint: constraint ? display(constraint) : null, primitiveKinds };
        }
    }
    const describeParameter = (name, type) => {
        const members = type.flags & ts.TypeFlags.Object && !checker.isArrayType(type) && !checker.isTupleType(type)
            ? type.getProperties()
            : [];
        return {
            name,
            type: display(type),
            properties: members.length ? Object.fromEntries(members.map(member => [member.name, display(typeOf(member))])) : null,
            propertyDefinitions: members.length ? Object.fromEntries(members.flatMap(member => {
                const location = definition(member.declarations?.[0]);
                return location ? [[member.name, location]] : [];
            })) : null,
        };
    };
    const parameters = (signature, first = 0) => signature.parameters.slice(first).flatMap(parameter => {
        const type = typeOf(parameter);
        const declaration = parameter.valueDeclaration ?? parameter.declarations?.[0];
        // Vue encodes emit payloads as rest tuples. Expand fixed tuples so each
        // call argument retains its own checked type and declaration label.
        const isRest = declaration?.dotDotDotToken ||
            (signature.parameters.at(-1) === parameter && checker.hasEffectiveRestParameter(signature));
        if (isRest && checker.isTupleType(type) && type.target.elementFlags.every(flag => flag === ts.ElementFlags.Required)) {
            return checker.getTypeArguments(type).map((element, index) =>
                describeParameter(type.target.labeledElementDeclarations?.[index]?.name.getText() ?? `arg${index + 1}`, element));
        }
        return [describeParameter(parameter.name, type)];
    });
    const events = Object.create(null);
    for (const signature of types.Emit.getCallSignatures()) {
        const [event] = signature.parameters;
        const eventType = event && typeOf(event);
        const names = eventType?.isUnion() ? eventType.types : [eventType];
        for (const name of names.filter(type => type?.isStringLiteral())) {
            const item = events[name.value] ??= { signatures: [], definition: null };
            item.signatures.push(parameters(signature, 1));
            const listener = types.Props.getProperty(listenerName(name.value));
            item.definition ??= definition(listener?.declarations?.[0] ?? signature.declaration);
        }
    }
    const listeners = new Set(Object.keys(events).map(listenerName));
    const vueProps = types.VueProps.isUnion() ? types.VueProps.types.map(type => type.value) : [types.VueProps.value];
    const props = Object.fromEntries(types.Props.getProperties()
        .filter(prop => !listeners.has(prop.name) && !vueProps.includes(prop.name))
        .map(prop => {
            const type = typeParameter
                ? checker.getNonNullableType(typeOf(prop))
                : checker.getTypeOfPropertyOfType(types.PropValues, prop.name);
            const branches = type.isUnion() ? type.types : [type];
            const values = branches.filter(branch => branch.isStringLiteral()).map(branch => branch.value);
            return [prop.name, {
                type: display(type),
                required: !optional(prop),
                literalValues: values,
                definition: definition(prop.declarations?.[0]),
            }];
        }));
    const slots = Object.fromEntries(types.Slots.getProperties().map(slot => [slot.name, {
        required: !optional(slot),
        signatures: checker.getNonNullableType(typeOf(slot)).getCallSignatures().map(signature => parameters(signature)),
        definition: definition(slot.declarations?.[0]),
    }]));

    return {
        ...identity,
        exportName,
        definition: definition(exportSymbol.valueDeclaration ?? exportSymbol.declarations?.[0]),
        props, events, slots, typeParameter,
    };
}

function listenerName(name) {
    return `on${name.charAt(0).toUpperCase()}${name.slice(1)}`;
}
