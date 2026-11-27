export const contractHelpers = `
    import type { ExtractPropTypes as JoltVueExtractProps, ExtractPublicPropTypes as JoltVueExtractPublicProps,
        PropType as JoltVuePropType } from 'vue';
    type Shape<C> = C extends abstract new (...args: any[]) => infer I
        ? I extends { $props: infer P; $emit: infer E; $slots: infer S }
            ? { props: P; emit: E; slots: S }
            : never
        : C extends (props: infer P, ...args: any[]) => { __ctx?: infer Context }
            ? NonNullable<Context> extends { emit: infer E; slots: infer S }
                ? { props: P; emit: E; slots: S }
                : never
            : never;
`;
export const contractAliases = componentType => `
    type Instance = Shape<${componentType}>;
    type Props = Omit<Instance['props'], VueProps>;
    type PropValues = Required<Props>;
    type Emit = Instance['emit'];
    type Slots = Instance['slots'];
`;
export const contractTypes = componentType => contractHelpers + contractAliases(componentType);

export function readComponent(exportSymbol, { ts, source, checker, typeOf, display, optional, definition, identity, exportName, webType,
    scope = source }) {
    const types = Object.fromEntries(scope.statements.filter(ts.isTypeAliasDeclaration)
        .map(alias => [alias.name.text, checker.getTypeAtLocation(alias.type)]));
    types.VueProps ??= checker.getDeclaredTypeOfSymbol(checker.resolveName('VueProps', source, ts.SymbolFlags.Type, false));
    const vueType = name => unalias(checker.resolveName(name, source, ts.SymbolFlags.Type, false));
    const vuePropType = vueType('JoltVuePropType');
    const vueExtractors = new Set([vueType('JoltVueExtractProps'), vueType('JoltVueExtractPublicProps')].filter(Boolean));
    function unalias(symbol) {
        return symbol?.flags & ts.SymbolFlags.Alias ? checker.getAliasedSymbol(symbol) : symbol;
    }
    function propValue(prop, type) {
        if (!(type.flags & ts.TypeFlags.Object) || type.isIntersection())
            return type;
        const wrapped = checker.getTypeOfPropertyOfType(type, 'type');
        if (!vuePropType || wrapped?.aliasSymbol !== vuePropType || wrapped.aliasTypeArguments?.length !== 1)
            return type;
        // Under exactOptionalPropertyTypes, Vue's extractor can retain the
        // option when the validator explicitly admits undefined. Prove this
        // originated in Vue's runtime options, since a prop can itself be an
        // ordinary object with the same shape.
        const extracted = prop.declarations?.some(declaration => {
            const options = declaration.parent;
            const extraction = options.parent;
            return ts.isTypeLiteralNode(options) && ts.isTypeReferenceNode(extraction) &&
                extraction.typeArguments?.[0] === options &&
                vueExtractors.has(unalias(checker.getSymbolAtLocation(extraction.typeName))) &&
                ts.isPropertySignature(declaration) && declaration.type &&
                checker.getTypeFromTypeNode(declaration.type) === type;
        });
        return extracted ? wrapped.aliasTypeArguments[0] : type;
    }
    function primitiveKinds(branches) {
        const string = branches.includes(checker.getStringType());
        const number = branches.includes(checker.getNumberType());
        const boolean = branches.includes(checker.getBooleanType()) ||
            branches.includes(checker.getFalseType()) && branches.includes(checker.getTrueType());
        return { string, number, boolean };
    }
    function scalarUnion(type) {
        if (!type.isUnion())
            return;
        const { string, number, boolean } = primitiveKinds(type.types);
        if (type.types.every(branch => branch === checker.getStringType() || branch === checker.getNumberType() ||
            boolean && (branch === checker.getFalseType() || branch === checker.getTrueType())))
            return [boolean && 'boolean', number && 'number', string && 'string'].filter(Boolean).join('|');
    }
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
    function scalarArray(element) {
        return element === checker.getNumberType() ? 'number[]' :
            element === checker.getStringType() ? 'string[]' :
            element === checker.getBooleanType() ? 'boolean[]' : undefined;
    }
    const describeParameter = (name, type, variadic = false) => {
        const members = type.flags & ts.TypeFlags.Object && !checker.isArrayType(type) && !checker.isTupleType(type)
            ? type.getProperties()
            : [];
        return {
            name,
            type: display(type),
            untypedRest: variadic && checker.isArrayType(type) &&
                Boolean(checker.getTypeArguments(type)[0].flags & ts.TypeFlags.Any),
            webType: webType(type)?.name,
            // A listener can mutate a C# array. Only TS mutable arrays can
            // cross this boundary, and a variadic call delivers separate values.
            valueDomain: !variadic && checker.isArrayType(type) && type.target.getSymbol().name === 'Array'
                ? scalarArray(checker.getTypeArguments(type)[0]) : scalarUnion(type),
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
        return [describeParameter(parameter.name, type, isRest)];
    });
    const events = Object.create(null);
    for (const signature of types.Emit.getCallSignatures()) {
        const [event] = signature.parameters;
        const eventType = event && typeOf(event);
        const names = eventType?.isUnion() ? eventType.types : [eventType];
        for (const name of names.filter(type => type?.isStringLiteral())) {
            const item = events[name.value] ??= { signatures: [], definition: null };
            item.signatures.push(parameters(signature, 1));
            const listener = types.Props.getProperty(listenerName(name.value)) ?? types.Props.getProperty(rawListenerName(name.value));
            const listenerDefinition = definition(listener?.declarations?.[0]);
            const emitDefinition = listenerDefinition?.documentation ? null : definition(signature.declaration);
            const location = listenerDefinition ?? emitDefinition;
            item.definition ??= location;
            if (item.definition && !item.definition.documentation && emitDefinition?.documentation)
                item.definition.documentation = emitDefinition.documentation;
        }
    }
    const listeners = new Set(Object.keys(events).flatMap(name => [listenerName(name), rawListenerName(name)]));
    const vueProps = types.VueProps.isUnion() ? types.VueProps.types.map(type => type.value) : [types.VueProps.value];
    const props = Object.fromEntries(types.Props.getProperties()
        .filter(prop => !listeners.has(prop.name) && !vueProps.includes(prop.name))
        .map(prop => {
            const declared = typeParameter
                ? checker.getNonNullableType(typeOf(prop))
                : checker.getTypeOfPropertyOfType(types.PropValues, prop.name);
            const type = propValue(prop, declared);
            const branches = type.isUnion() ? type.types : [type];
            const values = branches.filter(branch => branch.isStringLiteral()).map(branch => branch.value);
            const { string, number, boolean } = primitiveKinds(branches);
            // Only expose branches a C# author can supply without widening the
            // declared domain. An unsupported input arm remains unavailable.
            const primitiveInput = [boolean && 'boolean', number && 'number',
                (string || values.length > 0) && 'string'].filter(Boolean).join('|');
            const present = checker.getNonNullableType(type);
            const [item] = checker.isArrayType(present) ? checker.getTypeArguments(present) : [];
            const recordFields = item && present.target?.getSymbol()?.name === 'ReadonlyArray' &&
                present.target.getSymbol().declarations?.some(node => node.getSourceFile().hasNoDefaultLib) &&
                !item.isUnionOrIntersection() && item.flags & ts.TypeFlags.Object &&
                !item.getSymbol()?.declarations?.some(ts.isClassDeclaration) &&
                !item.getCallSignatures().length && !item.getConstructSignatures().length &&
                !checker.getIndexInfosOfType(item).length ? item.getProperties() : [];
            const fields = recordFields.map(member => {
                const value = checker.getTypeOfPropertyOfType(item, member.name);
                const readonly = Boolean(ts.getCheckFlags(member) & ts.CheckFlags.Readonly) ||
                    member.declarations?.some(node => ts.getCombinedModifierFlags(node) & ts.ModifierFlags.Readonly);
                const nullable = value?.isUnion() && value.types.includes(checker.getNullType());
                const primitive = nullable ? checker.getNonNullableType(value) : value;
                const kind = primitive === checker.getStringType() ? 'string' : primitive === checker.getBooleanType() ? 'boolean' :
                    primitive === checker.getNumberType() ? 'number' : undefined;
                if (nullable && value.types.includes(checker.getUndefinedType())) return false;
                return readonly && kind && !optional(member) && !member.name.startsWith('__@') && [member.name, nullable ? kind + '?' : kind];
            });
            const inputRecordFields = fields.length && fields.every(Boolean) ? Object.fromEntries(fields) : undefined;
            const inputType = scalarArray(item) || primitiveInput || undefined;
            return [prop.name, {
                type: display(type),
                required: !optional(prop),
                literalValues: string ? [] : values,
                inputType,
                inputRecordFields,
                inputNullable: Boolean(inputType && type.isUnion() && branches.includes(checker.getNullType())),
                definition: definition(prop.declarations?.[0]),
            }];
        }));
    function slotParameters(signature) {
        const [scope] = signature.parameters;
        const type = scope && signature.parameters.length === 1 && typeOf(scope);
        // An empty Vue slot scope has no field the caller needs to consume.
        if (type && type.flags === ts.TypeFlags.Object && !type.isUnionOrIntersection() &&
            !type.getProperties().length && !checker.getIndexInfosOfType(type).length &&
            !type.getCallSignatures().length && !type.getConstructSignatures().length &&
            !type.getSymbol()?.declarations?.some(ts.isClassDeclaration))
            return [];
        return parameters(signature);
    }
    const slots = Object.fromEntries(types.Slots.getProperties().map(slot => [slot.name, {
        required: !optional(slot),
        signatures: checker.getNonNullableType(typeOf(slot)).getCallSignatures().map(slotParameters),
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
    return rawListenerName(name.replace(/-(\w)/g, (_, letter) => letter.toUpperCase()));
}

function rawListenerName(name) {
    return `on${name.charAt(0).toUpperCase()}${name.slice(1)}`;
}
