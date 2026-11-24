import path from 'node:path';
import { createRequire } from 'node:module';
import { contractTypes, readComponent } from './vue-contract.mjs';
import { webIdlProjection } from './web-idl-projection.mjs';
import { templateTextSignature } from './vue-template-text.mjs';
import { readTypeScriptInput } from './typescript-input.mjs';
import { declarationDocumentation } from './declaration-documentation.mjs';

const [typescript, root, pathsJson, overlaysJson, declarationsJson, carriersJson] = globalThis.__jazorArguments;
const carriers = JSON.parse(carriersJson);
const require = createRequire(typescript);
const ts = require(typescript);
const toolRequire = createRequire(path.resolve(typescript, '../../../package.json'));
const vue = toolRequire('@vue/language-core');
const { proxyCreateProgram } = toolRequire('@volar/typescript');

const paths = JSON.parse(pathsJson);
const normalize = file => ts.sys.resolvePath(file);
const key = file => ts.sys.useCaseSensitiveFileNames ? normalize(file) : normalize(file).toLowerCase();
const overlays = new Map(Object.entries(JSON.parse(overlaysJson)).map(([file, content]) => [key(file), content]));
const declarations = new Map(Object.entries(JSON.parse(declarationsJson)).map(([file, content]) => [key(path.join(root, file + '.d.mts')), content]));
const inputs = new Set();
const readInputs = new Map();
const missing = new Set();
const missingDirectories = new Set();
const sys = {
    ...ts.sys,
    readFile(file) {
        if (overlays.has(key(file)))
            return overlays.get(key(file));
        const resolved = path.resolve(file);
        const identity = key(file);
        // Bind the fingerprint to the bytes consumed by TS. A save during the
        // query must invalidate its result when the next watcher event arrives.
        if (!readInputs.has(identity)) {
            const input = readTypeScriptInput(file);
            if (!input) {
                missing.add(resolved);
                return undefined;
            }
            readInputs.set(identity, input);
        }
        return readInputs.get(identity).text;
    },
    fileExists(file) {
        if (overlays.has(key(file)))
            return true;
        const exists = ts.sys.fileExists(file);
        (exists ? inputs : missing).add(path.resolve(file));
        return exists;
    },
    directoryExists(directory) {
        if (ts.sys.directoryExists(directory) || [...overlays.keys()].some(file => file.startsWith(key(directory) + '/')))
            return true;
        missingDirectories.add(path.resolve(directory));
        return false;
    },
};

const configFile = ts.findConfigFile(root, sys.fileExists);
const parsed = configFile && vue.createParsedCommandLine(ts, sys, configFile);
const options = {
    target: ts.ScriptTarget.ES2022,
    module: ts.ModuleKind.ESNext,
    moduleResolution: ts.ModuleResolutionKind.Bundler,
    strict: true,
    exactOptionalPropertyTypes: true,
    skipLibCheck: true,
    ...parsed?.options,
    noEmit: true,
};
const host = ts.createCompilerHost(options, true);
host.directoryExists = sys.directoryExists;
host.getCurrentDirectory = () => root;

const entryFor = (_, index) => path.join(root, 'jolt-vue-contract-' + index + '.mts');
const entries = new Map(paths.map((relative, index) => [key(entryFor(relative, index)), `
    import Component from ${JSON.stringify('./' + relative)};
    import type { VNodeProps, AllowedComponentProps, ComponentCustomProps } from 'vue';
    type VueProps = keyof (VNodeProps & AllowedComponentProps & ComponentCustomProps);
    ${contractTypes("typeof Component")}
`]));
host.readFile = file => entries.get(key(file)) ?? declarations.get(key(file)) ?? sys.readFile(file);
host.fileExists = file => entries.has(key(file)) || declarations.has(key(file)) || sys.fileExists(file);
host.getSourceFile = (file, languageVersion) => {
    const text = host.readFile(file);
    if (text !== undefined)
        return ts.createSourceFile(file, text, languageVersion, true);
};
// 公共类型读取使用当前 Roslyn 投影。探测到 .jazor 后由项目提供内存声明，避免读到旧构建产物。
let usesJolt = false;
// Resolve each package once per query. Negative lookups are rebuilt on each snapshot.
const moduleResolutions = ts.createModuleResolutionCache(root, host.getCanonicalFileName, options);
host.resolveModuleNameLiterals = (literals, file, redirected, compilerOptions, source) => literals.map(literal => {
    const name = literal.text;
    if ((name.startsWith('.') || name.startsWith('/')) && name.endsWith('.jazor')) {
        const source = name.startsWith('/') ? path.resolve(root, name.slice(1)) : path.resolve(path.dirname(file), name);
        const relative = path.relative(root, source);
        if (!relative.startsWith('..' + path.sep) && !path.isAbsolute(relative)) {
            usesJolt = true;
            const declaration = source + '.d.mts';
            if (declarations.has(key(declaration)))
                return { resolvedModule: { resolvedFileName: declaration, extension: ts.Extension.Dmts } };
            return { resolvedModule: undefined };
        }
    }
    return ts.resolveModuleName(name, file, compilerOptions, host, moduleResolutions, redirected,
        ts.getModeForUsageLocation(source, literal, compilerOptions));
});

let language;
const vueOptions = parsed?.vueOptions ?? vue.getDefaultCompilerOptions();
const createProgram = proxyCreateProgram(ts, ts.createProgram, () => ({
    languagePlugins: [vue.createVueLanguagePlugin(ts, options, vueOptions, file => file)],
    setup(value) { language = value; },
}));
const program = createProgram({ rootNames: paths.map(entryFor), options, host });
const checker = program.getTypeChecker();
const formatFlags = ts.TypeFormatFlags.NoTruncation | ts.TypeFormatFlags.UseAliasDefinedOutsideCurrentScope;
const optional = symbol => Boolean(symbol.flags & ts.SymbolFlags.Optional);

const contracts = paths.map((relative, index) => {
    try {
        const modulePath = normalize(path.join(root, relative));
        const source = program.getSourceFile(normalize(entryFor(relative, index)));
        const typeOf = symbol => checker.getTypeOfSymbolAtLocation(symbol, source);
        const display = type => checker.typeToString(type, source, formatFlags);
        const defaults = source.statements[0].importClause.name;
        const symbol = checker.getAliasedSymbol(checker.getSymbolAtLocation(defaults));
        const type = typeOf(symbol);
        const instance = type.getConstructSignatures()[0]?.getReturnType();
        const context = type.getCallSignatures()[0]?.getReturnType().getProperty('__ctx');
        const sourceScript = language.scripts.get(modulePath);
        const parse = sourceScript?.generated?.root.vueSfc;
        if (parse?.errors?.length)
            throw new Error(parse.errors.map(error => error.message ?? String(error)).join('\n'));
        if (!instance?.getProperty('$props') && !context)
            throw new Error('The default export has no Vue component type.');
        const identity = { moduleSpecifier: './' + relative, packageName: '', version: '', modulePath, declarationPath: modulePath };
        const { webType } = webIdlProjection(ts, program, source, host, carriers);
        const component = readComponent(symbol, { ts, source, checker, typeOf, display, optional, definition, identity, exportName: 'default', webType, webType });
        const setup = sourceScript?.generated?.root.sfc.scriptSetup;
        component.definition ??= { path: path.resolve(modulePath), start: setup?.start ?? 0, length: 0 };
        if (setup) {
            for (const model of vue.parseScriptSetupRanges(ts, setup.ast, vueOptions).defineModel) {
                const name = model.name ? setup.content.slice(model.name.start + 1, model.name.end - 1) : 'modelValue';
                const target = model.name ?? model.localName ?? model.type;
                if (!target)
                    continue;
                const location = { path: path.resolve(modulePath), start: setup.startTagEnd + target.start, length: target.end - target.start };
                if (component.props[name]) component.props[name].definition ??= location;
                if (component.events['update:' + name]) component.events['update:' + name].definition ??= location;
            }
        }
        return { path: relative, component };
    } catch (error) {
        return { path: relative, error: error.message };
    }
});
const definitionEnds = new Map();
for (const contract of contracts)
    collectDefinitions(contract.component);
const templates = {};
if (vueOptions.target >= 3 && vueOptions.plugins.length === 0) {
    for (const relative of paths) {
        const file = path.resolve(root, relative);
        const snapshot = language.scripts.get(normalize(file))?.snapshot;
        if (!snapshot)
            continue;
        const source = snapshot.getText(0, snapshot.getLength());
        const definitionEnd = definitionEnds.get(key(file)) ?? 0;
        const signature = templateTextSignature(typescript, source, definitionEnd);
        if (signature !== null)
            templates[file] = { definitionEnd, signature };
    }
}
const fingerprints = Object.fromEntries([...readInputs.values()].map(input => [input.path, input.hash]));
console.log(JSON.stringify({ contracts, inputs: [...inputs], fingerprints, missing: [...missing], missingDirectories: [...missingDirectories], usesJolt, templates }));

function collectDefinitions(value) {
    if (!value || typeof value !== 'object')
        return;
    if (typeof value.path === 'string' && Number.isInteger(value.start) && Number.isInteger(value.length)) {
        const file = key(value.path);
        definitionEnds.set(file, Math.max(definitionEnds.get(file) ?? 0, value.start + value.length));
        return;
    }
    for (const child of Object.values(value))
        collectDefinitions(child);
}

function definition(node) {
    if (!node)
        return null;
    const target = node.name ?? node;
    const file = target.getSourceFile().fileName;
    if (declarations.has(key(file)))
        return null;
    const start = target.getStart();
    const end = target.getEnd();
    if (!file.endsWith('.vue'))
        return { path: path.resolve(file), start, length: end - start, documentation: declarationDocumentation(ts, node) };
    const script = language.scripts.get(file);
    const service = script?.generated?.languagePlugin.typescript.getServiceScript(script.generated.root);
    if (!service)
        return null;
    const map = language.maps.get(service.code, script);
    const leading = service.preventLeadingOffset ? 0 : script.snapshot.getLength();
    const [range] = map.toSourceRange(start - leading, end - leading, false);
    return range ? { path: path.resolve(file), start: range[0], length: range[1] - range[0],
        documentation: declarationDocumentation(ts, node) } : null;
}
