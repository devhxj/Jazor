import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import ts from 'typescript';

const [packageName, exportName] = globalThis.__jazorArguments;
const resolved = import.meta.resolve(packageName);
const exports = await import(packageName);
if (!Object.hasOwn(exports, exportName))
    throw new Error(`Missing runtime export ${exportName} from ${packageName}`);

const root = path.dirname(path.dirname(fileURLToPath(import.meta.url)));
const options = {
    target: ts.ScriptTarget.ES2022,
    module: ts.ModuleKind.ESNext,
    moduleResolution: ts.ModuleResolutionKind.Bundler,
    strict: true,
    exactOptionalPropertyTypes: true,
    skipLibCheck: true,
};
const entry = path.join(root, 'component-probe.mts');
const host = ts.createCompilerHost(options);
const formatFlags = ts.TypeFormatFlags.NoTruncation | ts.TypeFormatFlags.UseAliasDefinedOutsideCurrentScope;
// The import is checked where a consumer would use it. Vue's public instance
// exposes props, emitted event overloads, and slots for constructor-typed components.
const source = ts.createSourceFile(entry, `
    import * as packageExports from ${JSON.stringify(packageName)};
    import type { VNodeProps, AllowedComponentProps, ComponentCustomProps } from 'vue';
    type Component = (typeof packageExports)[${JSON.stringify(exportName)}];
    type Instance = InstanceType<Component>;
    type Props = Omit<Instance['$props'], keyof (VNodeProps & AllowedComponentProps & ComponentCustomProps)>;
    type PropValues = Required<Props>;
    type Emit = Instance['$emit'];
    type Slots = Instance['$slots'];
`, options.target, true);
const getSourceFile = host.getSourceFile.bind(host);
host.getSourceFile = (file, ...args) => file === entry ? source : getSourceFile(file, ...args);
const fileExists = host.fileExists.bind(host);
host.fileExists = file => file === entry || fileExists(file);
const program = ts.createProgram([entry], options, host);
const diagnostics = ts.getPreEmitDiagnostics(program);
if (diagnostics.length)
    throw new Error(ts.formatDiagnosticsWithColorAndContext(diagnostics, host));

const checker = program.getTypeChecker();
const aliases = Object.fromEntries(source.statements.filter(ts.isTypeAliasDeclaration)
    .map(alias => [alias.name.text, checker.getTypeAtLocation(alias.type)]));
const display = type => checker.typeToString(type, source, formatFlags);

function parameters(signature) {
    return signature.parameters.map(parameter => ({
        name: parameter.name,
        type: display(checker.getTypeOfSymbolAtLocation(parameter, source)),
    }));
}

const events = Object.create(null);
for (const signature of aliases.Emit.getCallSignatures()) {
    const [event] = signature.parameters;
    const eventType = event && checker.getTypeOfSymbolAtLocation(event, source);
    if (!eventType?.isStringLiteral())
        continue;
    (events[eventType.value] ??= []).push(parameters(signature).slice(1));
}
const listenerNames = new Set(Object.keys(events).map(event => `on${event.charAt(0).toUpperCase()}${event.slice(1)}`));
const props = Object.fromEntries(checker.getPropertiesOfType(aliases.Props)
    .filter(prop => !listenerNames.has(prop.name))
    .map(prop => [prop.name, {
        type: display(checker.getTypeOfPropertyOfType(aliases.PropValues, prop.name)),
        required: !(prop.flags & ts.SymbolFlags.Optional),
    }]));
const slots = Object.fromEntries(checker.getPropertiesOfType(aliases.Slots).map(slot => [slot.name, {
    required: !(slot.flags & ts.SymbolFlags.Optional),
    signatures: checker.getNonNullableType(checker.getTypeOfSymbolAtLocation(slot, source)).getCallSignatures().map(parameters),
}]));

const declaration = ts.resolveModuleName(packageName, entry, options, host).resolvedModule;
let manifest;
for (let directory = path.dirname(fileURLToPath(resolved)); directory !== path.dirname(directory); directory = path.dirname(directory)) {
    const file = path.join(directory, 'package.json');
    if (fs.existsSync(file)) {
        const candidate = JSON.parse(fs.readFileSync(file, 'utf8'));
        if (candidate.name && (packageName === candidate.name || packageName.startsWith(`${candidate.name}/`))) {
            manifest = candidate;
            break;
        }
    }
}
if (!manifest || !declaration)
    throw new Error(`Missing package or declaration metadata for ${packageName}`);

console.log(JSON.stringify({
    packageName, version: manifest.version, exportName,
    nodeVersion: process.version,
    module: path.relative(root, fileURLToPath(resolved)).replaceAll(path.sep, '/'),
    declaration: path.relative(root, declaration.resolvedFileName).replaceAll(path.sep, '/'),
    props, events, slots,
}));
