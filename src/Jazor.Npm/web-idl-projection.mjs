import path from 'node:path';

export function webIdlProjection(ts, program, source, host, names) {
    const checker = program.getTypeChecker();
    const defaultLibraryDirectory = path.dirname(ts.getDefaultLibFilePath(program.getCompilerOptions()));
    function domSymbol(name) {
        const symbol = checker.resolveName(name, source, ts.SymbolFlags.Type, false);
        const declarations = symbol?.declarations ?? [];
        if (declarations.some(node => path.basename(node.getSourceFile().fileName) === 'lib.dom.d.ts') &&
            declarations.every(isDefaultLibraryDeclaration))
            return symbol;
    }

    function isDefaultLibraryDeclaration(node) {
        const file = node.getSourceFile();
        return program.isSourceFileDefaultLibrary(file) &&
            host.getCanonicalFileName(path.dirname(path.normalize(file.fileName))) ===
            host.getCanonicalFileName(path.normalize(defaultLibraryDirectory));
    }

    function bindWebTypes() {
        const carriers = new Map();
        return type => {
            if (!(type.flags & ts.TypeFlags.Object) || !(type.objectFlags & ts.ObjectFlags.Interface) &&
                !(type.objectFlags & ts.ObjectFlags.Reference))
                return;
            const symbol = type.getSymbol();
            if (!symbol)
                return;
            if (!carriers.has(symbol)) {
                const name = Object.hasOwn(names, symbol.name) && names[symbol.name];
                // A generic interface or global augmentation can promise members
                // outside the fixed CLR carrier. Preserve the TS declaration boundary.
                const unmodified = candidate => candidate?.declarations?.some(ts.isInterfaceDeclaration) &&
                    candidate.declarations.every(node => isDefaultLibraryDeclaration(node) &&
                        (ts.isVariableDeclaration(node) || ts.isInterfaceDeclaration(node)));
                const standard = name && domSymbol(symbol.name) === symbol && unmodified(symbol) &&
                    !symbol.declarations.some(node => ts.isInterfaceDeclaration(node) && node.typeParameters?.length) &&
                    !type.getConstructSignatures().length &&
                    // Generic standard-library mixins (Worker's MessageEventTarget<Worker>)
                    // are part of the fixed instance contract, even though the carrier itself is closed.
                    (type.getBaseTypes() ?? []).every(function standardBase(base) {
                        return unmodified(base.getSymbol()) && (base.getBaseTypes() ?? []).every(standardBase);
                    });
                carriers.set(symbol, standard ? { kind: 'web', name } : undefined);
            }
            return carriers.get(symbol);
        };
    }
    return { domSymbol, webType: bindWebTypes() };
}
