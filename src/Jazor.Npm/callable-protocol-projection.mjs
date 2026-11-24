import { alternatives, singleCall } from './generic-callbacks.mjs';

// The direct array arm accepts the original middleware array unchanged.
// Check the element declaration, including any explicit falsy entries.
export function readProtocolArray(checker, ts, readProtocolAlias, type) {
    for (const arm of alternatives(type)) {
        if (!checker.isArrayType(arm))
            continue;
        const [element] = checker.getTypeArguments(arm);
        const present = alternatives(element).filter(branch => !(branch.flags & (ts.TypeFlags.Null | ts.TypeFlags.Undefined)) &&
            (!(branch.flags & ts.TypeFlags.BooleanLiteral) || branch.intrinsicName !== 'false'));
        const handle = present.length === 1 && readProtocolAlias(present[0]);
        if (handle)
            return { kind: 'inputArray', elementType: handle };
    }
}

export function readCallableProtocol(checker, ts, optional, definition, typeOf, type) {
    if (!(type.flags & ts.TypeFlags.Object) || type.aliasSymbol || !(type.getSymbol()?.flags & ts.SymbolFlags.TypeLiteral) || type.getCallSignatures().length ||
        checker.getIndexInfosOfType(type).length)
        return;
    const members = type.getProperties();
    const required = members.filter(member => !optional(member) &&
        !(checker.getTypeOfSymbol(member).flags & ts.TypeFlags.Any));
    if (required.length !== 1 || members.some(member => member !== required[0] &&
        !(checker.getTypeOfSymbol(member).flags & ts.TypeFlags.Any)))
        return;
    const [call] = singleCall(checker.getTypeOfPropertyOfType(type, required[0].name));
    if (!call || call.parameters.length !== 1 || call.minArgumentCount !== 1 || call.thisParameter ||
        call.typeParameters?.length || checker.hasEffectiveRestParameter(call))
        return;
    const parameter = typeOf(call.parameters[0]);
    const result = call.getReturnType();
    const parameterDeclaration = (parameter.aliasSymbol ?? parameter.getSymbol())?.declarations?.[0];
    const inheritedDeclarations = parameter.getProperties().flatMap(member => member.declarations ?? [])
        .map(node => node.parent).filter(ts.isInterfaceDeclaration);
    const inherited = inheritedDeclarations.find(node => node !== parameterDeclaration &&
        checker.isTypeAssignableTo(parameter, checker.getTypeAtLocation(node)) &&
        checker.isTypeAssignableTo(checker.getTypeAtLocation(node), parameter));
    const source = definition(inherited ?? parameterDeclaration);
    const resultLocation = definition((result.aliasSymbol ?? result.getSymbol())?.declarations?.[0]);
    if (source && resultLocation && (parameter.aliasSymbol ?? parameter.getSymbol())?.declarations?.length === 1 &&
        (result.aliasSymbol ?? result.getSymbol())?.declarations?.length === 1)
        return { kind: 'handle', name: 'CallableProtocol', typeDefinition: source,
            protocolKey: JSON.stringify([required[0].name, declarationIdentity(source, inherited ?? parameterDeclaration),
                declarationIdentity(resultLocation, (result.aliasSymbol ?? result.getSymbol()).declarations[0])]) };
}

function declarationIdentity(location, declaration) {
    const path = location.path.replaceAll('\\', '/');
    const relative = path.split('/node_modules/').at(-1);
    return [relative === path ? path.split('/').at(-1) : relative, declaration.name?.getText()];
}
