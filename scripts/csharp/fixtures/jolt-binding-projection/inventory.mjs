import ts from 'typescript';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const [specifier] = globalThis.__jazorArguments;
const entry = fileURLToPath(new URL('inventory-entry.mts', import.meta.url));
const options = {
    target: ts.ScriptTarget.ES2022,
    module: ts.ModuleKind.ESNext,
    moduleResolution: ts.ModuleResolutionKind.Bundler,
    strict: true,
    types: [],
};
const host = ts.createCompilerHost(options);
const getSourceFile = host.getSourceFile.bind(host);
host.getSourceFile = (file, ...args) => path.normalize(file) === entry
    ? ts.createSourceFile(entry, `import * as exports from ${JSON.stringify(specifier)};`, options.target, true)
    : getSourceFile(file, ...args);
const program = ts.createProgram([entry], options, host);
const source = program.getSourceFile(entry);
const checker = program.getTypeChecker();
const moduleSymbol = checker.getSymbolAtLocation(source.statements[0].moduleSpecifier);
if (!moduleSymbol)
    throw new Error(`Cannot resolve declaration exports for ${specifier}`);
const kinds = Object.fromEntries(checker.getExportsOfModule(moduleSymbol).map(alias => {
    const symbol = alias.flags & ts.SymbolFlags.Alias ? checker.getAliasedSymbol(alias) : alias;
    const type = checker.getTypeOfSymbolAtLocation(symbol, source);
    const kind = checker.getTypeOnlyAliasDeclaration(alias) || !(symbol.flags & ts.SymbolFlags.Value)
        ? 'type only'
        : symbol.flags & ts.SymbolFlags.ValueModule ? 'namespace'
        : type.getConstructSignatures().length ? 'constructor'
        : type.getCallSignatures().length ? 'function' : 'value';
    const members = kind === 'namespace' ? type.getProperties().filter(member =>
        checker.getTypeOfSymbolAtLocation(member, source).getCallSignatures().length).map(member => member.name) : [];
    return [alias.name, { kind, members }];
}));
console.log(JSON.stringify(kinds));
