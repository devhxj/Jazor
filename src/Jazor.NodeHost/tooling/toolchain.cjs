const fs = require('node:fs/promises');
const path = require('node:path');
const crypto = require('node:crypto');
const { createRequire, isBuiltin } = require('node:module');
const { pathToFileURL } = require('node:url');
const { inspect } = require('node:util');

const exists = (file) => fs.access(file).then(() => true, () => false);
const hash = (value) => crypto.createHash('sha256').update(value).digest('hex');

exports.render = async (requestJson) => {
    const { projectDirectory, modulePath, state, url } = JSON.parse(requestJson);
    if (url !== null) {
        const { render } = await import(pathToFileURL(path.join(projectDirectory, 'entry-server.mjs')).href);
        const { html, teleports } = await render({ url, state });
        // Render hooks can finalize the hydration snapshot, for example with
        // Pinia state populated by server prefetch. Return that same snapshot.
        return JSON.stringify({ html, teleports, state });
    }
    const module = await import(pathToFileURL(path.join(projectDirectory, 'ssr-entry.js')).href);
    return module.render({ modulePath, state });
};

exports.executeModule = async (requestJson) => {
    const { projectDirectory, modulePath, arguments: args, imports } = JSON.parse(requestJson);
    if (imports) {
        // Only test fixtures need an import map for their local host doubles.
        // Register an environment-owned ESM hook; production modules retain
        // standard package resolution and are never rewritten by this runner.
        const loader = `
            let imports, baseURL;
            export function initialize(data) { ({ imports, baseURL } = data); }
            export async function resolve(specifier, context, nextResolve) {
                const key = Object.keys(imports).sort((a, b) => b.length - a.length)
                    .find(key => key === specifier || key.endsWith('/') && specifier.startsWith(key));
                if (!key) return nextResolve(specifier, context);
                const target = imports[key] + specifier.slice(key.length);
                return nextResolve(target, { ...context, parentURL: baseURL });
            }
        `;
        const { register } = require('node:module');
        register('data:text/javascript,' + encodeURIComponent(loader), {
            data: { imports, baseURL: pathToFileURL(path.join(projectDirectory, 'package.json')).href }
        });
    }
    const tests = [];
    const cleanups = [];
    const output = [];
    const { format } = require('node:util');
    console.log = (...values) => output.push(format(...values));
    globalThis.__jazorArguments = args;
    // Runtime fixtures register their existing assertions here. This is a
    // test-owned function, not a Deno compatibility object or a production API.
    globalThis.__jazorTest = (name, body) => tests.push({ name, body });
    // Embedded environments have no browser unload event. Release fixture-owned
    // DOM windows after assertions, including import and async test failures.
    globalThis.__jazorAfterAll = (cleanup) => cleanups.push(cleanup);
    try {
        await import(pathToFileURL(modulePath).href);
        for (const test of tests) {
            try { await test.body(); }
            catch (error) { throw new Error(test.name + ': ' + (error.stack ?? error)); }
        }
    } finally {
        for (const cleanup of cleanups.reverse()) await cleanup();
    }
    return JSON.stringify({ tests: tests.length, pid: process.pid, output: output.join('\n') });
};

exports.restore = async (requestJson) => {
    const request = JSON.parse(requestJson);
    const root = request.projectDirectory;
    const manifestText = await fs.readFile(path.join(root, 'package.json'), 'utf8');
    const manifest = JSON.parse(manifestText);
    if (manifest.dependencies?.vite || manifest.devDependencies?.vite) {
        // Lightning CSS's napi-rs v2 Windows addon resolves symbols only from
        // the executable, which aborts inside a .NET host. Select its official
        // same-version WASM implementation for Vite on every platform. This is
        // a fixed dependency choice, recorded for ordinary pnpm reproduction.
        const selector = 'vite>lightningcss';
        const cssPackage = 'npm:lightningcss-wasm@' + request.lightningCssWasmVersion;
        const overrides = (manifest.pnpm ??= {}).overrides ??= {};
        if (overrides[selector] && overrides[selector] !== cssPackage)
            throw new Error('Embedded Vite requires pnpm.overrides["' + selector + '"] = "' + cssPackage + '".');
        if (overrides[selector] !== cssPackage) {
            overrides[selector] = cssPackage;
            await fs.writeFile(path.join(root, 'package.json'), JSON.stringify(manifest, null, 2) + '\n');
        }
    }
    const lockPath = path.join(root, 'pnpm-lock.yaml');
    const statePath = path.join(root, 'node_modules', '.jazor-restore-state');
    const graph = JSON.stringify({ dependencies: manifest.dependencies, devDependencies: manifest.devDependencies,
        optionalDependencies: manifest.optionalDependencies, overrides: manifest.pnpm?.overrides });
    const identity = hash(JSON.stringify({ graph, engine: request.engineVersion, node: process.version, platform: process.platform, arch: process.arch, linker: 'hoisted' }));
    const lock = await exists(lockPath) ? await fs.readFile(lockPath, 'utf8') : '';
    const expected = identity + '\n' + hash(lock);
    const local = Object.values({ ...manifest.dependencies, ...manifest.devDependencies }).some(value => value.startsWith('file:'));
    if (!local && await exists(statePath) && await fs.readFile(statePath, 'utf8') === expected) {
        return JSON.stringify({ operation: 'restore', status: 'skipped', pid: process.pid });
    }
    let pnpm;
    try {
        pnpm = createRequire(path.join(request.bootstrap, 'package.json'))('@pnpm/napi');
    } catch (error) {
        // NodeApi's managed exception does not carry Error.cause. Preserve the
        // loader's native failure so missing symbols/dependencies stay visible.
        throw new Error(inspect(error, { depth: 5 }));
    }
    const frozen = !!lock && await rootMatchesLock(pnpm, root, manifest);
    await fs.rm(statePath, { force: true });
    const config = pnpm.readConfig({ dir: root });
    await fs.mkdir(request.storeDirectory, { recursive: true });
    const result = await pnpm.install({
        dir: root,
        projects: [{ rootDir: root, manifest }],
        registries: Object.fromEntries(config.registries.map(registry => [registry.name, registry.url])),
        authHeaderByUri: config.authHeaderByUri,
        proxyConfig: { httpProxy: config.httpProxy, httpsProxy: config.httpsProxy, noProxy: config.noProxy },
        networkConfig: { ca: config.ca, cert: config.cert, key: config.key, strictSsl: config.strictSsl },
        storeDir: request.storeDirectory,
        cacheDir: path.join(request.storeDirectory, 'cache'),
        nodeLinker: 'hoisted',
        overrides: manifest.pnpm?.overrides,
        nodeVersion: process.versions.node,
        autoInstallPeers: true,
        frozenLockfile: frozen,
        ignoreScripts: true,
        reporter: { appendOnly: true },
    }, undefined, undefined, chunk => process.stderr.write(chunk));
    if (!local) await fs.writeFile(statePath, identity + '\n' + hash(await fs.readFile(lockPath, 'utf8')));
    return JSON.stringify({ operation: 'restore', status: 'completed', pid: process.pid, frozen, ...result });
};

async function rootMatchesLock(pnpm, root, manifest) {
    // The engine owns the format. Inspect its parsed root importer rather than
    // parsing YAML or trying a failed frozen install followed by a retry.
    const lock = await pnpm.readLockfile({ dir: root });
    const importer = lock?.importers?.['.'];
    if (!importer) return false;
    const orderedOverrides = value => JSON.stringify(Object.entries(value ?? {}).sort(([a], [b]) => a < b ? -1 : a > b ? 1 : 0));
    if (orderedOverrides(lock.overrides) !== orderedOverrides(manifest.pnpm?.overrides)) return false;
    for (const group of ['dependencies', 'devDependencies', 'optionalDependencies']) {
        const expected = manifest[group] ?? {};
        const actual = importer[group] ?? {};
        if (Object.keys(expected).length !== Object.keys(actual).length) return false;
        for (const [name, specifier] of Object.entries(expected)) if (actual[name]?.specifier !== specifier) return false;
    }
    return true;
}

async function loadVite(projectDirectory) {
        const requireFromProject = createRequire(path.join(projectDirectory, 'package.json'));
        if (process.platform === 'win32' && process.arch === 'x64' && process.config.variables.node_target_type === 'shared_library') {
            // Rolldown's x64 loader mistakes the MSVC libnode shared_library for MinGW.
            // Use its explicit native-binding contract only while loading rolldown.
            // Replacing this environment's env object avoids writing the OS-wide
            // environment through Node's special process.env setter properties.
            const inheritedEnvironment = process.env;
            process.env = { ...inheritedEnvironment, NAPI_RS_NATIVE_LIBRARY_PATH: requireFromProject.resolve('@rolldown/binding-win32-x64-msvc') };
            try {
                await import(pathToFileURL(requireFromProject.resolve('rolldown')).href);
            } finally {
                process.env = inheritedEnvironment;
            }
        }
        return import(pathToFileURL(requireFromProject.resolve('vite')).href);
}

function compileJoltStyle(requireFromProject, stylesheet, filename) {
    const header = /^\/\* (data-v-[0-9a-f]+) \*\/\n/.exec(stylesheet);
    if (!header) throw new Error('Missing Jolt scope ID in ' + filename);
    const { compileStyle } = requireFromProject('vue/compiler-sfc');
    const result = compileStyle({ source: stylesheet.slice(header[0].length), filename, id: header[1], scoped: true });
    if (result.errors.length > 0)
        throw new Error(filename + ': ' + result.errors.map(error => error.message ?? error).join('\n'));
    return result.code;
}

function visitJoltStyleUrls(requireFromProject, code, projectDirectory, filename, visit) {
    const vueRequire = createRequire(requireFromProject.resolve('vue/package.json'));
    const sfcRequire = createRequire(vueRequire.resolve('@vue/compiler-sfc'));
    const postcss = sfcRequire('postcss');
    const css = postcss.parse(code, { from: filename });
    const urls = /url\(\s*(?:"((?:\\.|[^"\\])*)"|'((?:\\.|[^'\\])*)'|((?:\\.|[^\s)\\])*))\s*\)/gi;
    css.walkDecls(declaration => {
        declaration.value = declaration.value.replace(urls, (original, doubleQuoted, singleQuoted, bare) => {
            const url = (doubleQuoted ?? singleQuoted ?? bare).replace(/\\([0-9a-f]{1,6})\s?|\\(.)/gi,
                (_, hex, escaped) => hex ? String.fromCodePoint(parseInt(hex, 16)) : escaped);
            if (!url || /^(?:[a-z][a-z\d+.-]*:|\/|#|\?)/i.test(url)) return original;
            const suffixAt = url.search(/[?#]/);
            const file = suffixAt < 0 ? url : url.slice(0, suffixAt);
            const absolute = path.resolve(path.dirname(filename), decodeURIComponent(file));
            const relative = path.relative(projectDirectory, absolute).split(path.sep).join('/');
            if (relative === '..' || relative.startsWith('../') || path.isAbsolute(relative))
                throw new Error(filename + ': stylesheet asset must be inside the project: ' + url);
            return visit(relative, suffixAt < 0 ? '' : url.slice(suffixAt)) ?? original;
        });
    });
    return css.toString();
}

exports.compileJoltStyles = async (requestJson) => {
    const { projectDirectory, styles } = JSON.parse(requestJson);
    const requireFromProject = createRequire(path.join(projectDirectory, 'package.json'));
    return JSON.stringify(Object.fromEntries(Object.entries(styles).map(([file, stylesheet]) => {
        const filename = path.join(projectDirectory, file);
        const assets = new Set();
        const code = compileJoltStyle(requireFromProject, stylesheet, filename);
        visitJoltStyleUrls(requireFromProject, code, projectDirectory, filename, relative => { assets.add(relative); });
        return [file, { code, assets: [...assets].sort() }];
    })));
};

async function joltVitePlugins(projectDirectory, development, generatedDirectory = path.join(projectDirectory, '.jolt')) {
    const requireFromProject = createRequire(path.join(projectDirectory, 'package.json'));
    const plugins = [joltVitePlugin(projectDirectory, development, requireFromProject, generatedDirectory)];
    let vuePath;
    try {
        vuePath = requireFromProject.resolve('@vitejs/plugin-vue');
    } catch (error) {
        // Projects with only Jolt components can omit the SFC compiler plugin.
        if (error.code !== 'MODULE_NOT_FOUND') throw error;
    }
    if (vuePath) {
        const { default: vue } = await import(pathToFileURL(vuePath).href);
        const plugin = vue();
        const apply = plugin.apply;
        plugin.apply = (config, env) => {
            const hasVue = [config.plugins].flat(Infinity).some(other => other?.name === plugin.name && other !== plugin);
            return !hasVue && (typeof apply === 'function' ? apply(config, env) : !apply || apply === env.command);
        };
        plugins.push(plugin);
    }
    return plugins;
}

function joltVitePlugin(projectDirectory, development, requireFromProject, modules) {
    return {
        name: 'jolt-components',
        enforce: 'pre',
        config() {
            if (development) {
                // The compiler creates and deletes its metadata runners while Vite is live.
                const scratch = path.resolve(projectDirectory, '.jazor');
                return { resolve: { dedupe: ['vue'] }, server: { watch: { ignored: file => {
                    const relative = path.relative(scratch, file);
                    return relative === '' || (!relative.startsWith('..' + path.sep) && relative !== '..' && !path.isAbsolute(relative));
                } } } };
            }
            return { resolve: { dedupe: ['vue'] } };
        },
        async resolveId(source, importer, options) {
            if (!source.toLowerCase().endsWith('.jazor'))
                return;
            const authored = await this.resolve(source, importer, { ...options, skipSelf: true });
            if (!authored || authored.external)
                return;
            const relative = path.relative(projectDirectory, authored.id);
            if (relative === '..' || relative.startsWith('..' + path.sep) || path.isAbsolute(relative))
                return;
            // .jolt mirrors the authored directory layout. Return the same physical
            // module used by generated imports so Vite tracks one HMR boundary.
            const generated = path.join(modules, relative.replace(/\.jazor$/i, '.mjs'));
            return this.resolve(generated, importer, { ...options, skipSelf: true });
        },
        async load(id) {
            const relative = path.relative(modules, id);
            if (relative.startsWith('..' + path.sep) || path.isAbsolute(relative))
                return;
            if (relative.endsWith('.jolt.css')) {
                const stylesheet = await fs.readFile(id, 'utf8');
                const filename = path.join(projectDirectory, relative.replace(/\.jolt\.css$/, '.jazor'));
                const code = compileJoltStyle(requireFromProject, stylesheet, filename);
                // Vite resolves URLs from the generated stylesheet. Rebase authored
                // relative URLs so both the build and the dev watcher use source assets.
                return visitJoltStyleUrls(requireFromProject, code, projectDirectory, filename, (asset, suffix) =>
                    'url(' + JSON.stringify('/' + asset.split('/').map(encodeURIComponent).join('/') + suffix) + ')');
            }
            // The route table is generated from component metadata and has no map.
            if (!relative.endsWith('.mjs') || relative === '_routes.mjs')
                return;
            let [code, map] = await Promise.all([fs.readFile(id, 'utf8'), fs.readFile(id + '.map', 'utf8')]);
            const sourceMap = JSON.parse(map);
            sourceMap.sources = sourceMap.sources.map(source => path.resolve(projectDirectory, source).replaceAll('\\', '/'));
            if (development) {
                const body = this.parse(code).body;
                const exported = body.find(node => node.type === 'ExportDefaultDeclaration');
                if (exported) {
                    const render = body
                        .filter(node => node.type === 'ClassDeclaration')
                        .flatMap(node => node.body.body)
                        .find(node => node.type === 'MethodDefinition' && node.key.name === '__JoltRender');
                    // Vue can reuse setup state when the generated script and
                    // component metadata are identical outside the render body.
                    const signature = render ? hash(code.slice(0, render.start) + code.slice(render.end)) : null;
                    const templateImports = body
                        .filter(node => node.type === 'ImportDeclaration')
                        .flatMap(node => node.specifiers)
                        .filter(specifier => specifier.local.name.startsWith('$joltComponent'));
                    // Replace only the export prefix, keeping mapped offsets intact.
                    const prefix = code.slice(exported.start, exported.declaration.start);
                    const component = '$jCmp';
                    const binding = ('const ' + component + '=').padEnd(prefix.length);
                    const hmrId = JSON.stringify('jolt:' + relative.replaceAll('\\', '/'));
                    code = code.slice(0, exported.start) + binding + code.slice(exported.declaration.start);
                    const aliases = [];
                    const importReaders = [];
                    // A refreshed Vite URL can re-evaluate an SFC import even though
                    // Vue already patched that component. A render-only update must
                    // keep using the original definition for existing child VNodes.
                    for (const { local } of templateImports.reverse()) {
                        const original = local.name;
                        const imported = original.replace('$joltComponent', '$joltImported_');
                        code = code.slice(0, local.start) + imported + code.slice(local.end);
                        aliases.push(original);
                        importReaders.push(`get ${original}() { return ${imported}; }`);
                    }
                    code += `\nexport default ${component};
export const $joltHmrSignature = ${JSON.stringify(signature)};
const $joltPreviousImports = $joltHmrSignature && import.meta.hot?.data.joltSignature === $joltHmrSignature
    ? import.meta.hot.data.joltImports : undefined;
const $joltImports = $joltPreviousImports ?? { ${importReaders.join(', ')} };
${aliases.length === 0 ? '' : `
let ${aliases.join(', ')};
const $joltRender = ${component}.render;
${component}.render = (...args) => {
    ({ ${aliases.join(', ')} } = $joltImports);
    return $joltRender(...args);
};`}
if (import.meta.hot) {
    import.meta.hot.data.joltSignature = $joltHmrSignature;
    import.meta.hot.data.joltImports = $joltImports;
    ${component}.__hmrId = ${hmrId};
    globalThis.__VUE_HMR_RUNTIME__?.createRecord(${hmrId}, ${component});
    import.meta.hot.accept(updated => {
        if (!updated) return;
        const runtime = globalThis.__VUE_HMR_RUNTIME__;
        if ($joltHmrSignature && $joltHmrSignature === updated.$joltHmrSignature)
            runtime?.rerender(${hmrId}, updated.default.render);
        else
            runtime?.reload(${hmrId}, updated.default);
    });
}\n`;
                }
            }
            return { code, map: sourceMap };
        },
    };
}

exports.build = async (requestJson) => {
    const { projectDirectory, ssr, jolt } = JSON.parse(requestJson);
    const vite = await loadVite(projectDirectory);
    await vite.build({ root: projectDirectory, configFile: jolt ? undefined : path.join(projectDirectory, ssr ? 'ssr.vite.config.js' : 'vite.config.js'),
        plugins: jolt ? await joltVitePlugins(projectDirectory, false) : [] });
    return JSON.stringify({ operation: 'build', ssr, status: 'completed', pid: process.pid });
};

exports.buildJoltSsr = async (requestJson) => {
    const { projectDirectory, serverEntry, outputDirectory } = JSON.parse(requestJson);
    const vite = await loadVite(projectDirectory);
    const client = path.join(outputDirectory, 'client');
    const server = path.join(outputDirectory, 'server');
    const entryFileName = 'entry-server.mjs';
    const manifestFileName = '.vite/manifest.json';

    const clientResult = await vite.build({
        root: projectDirectory,
        plugins: await joltVitePlugins(projectDirectory, false),
        build: {
            ssr: false,
            lib: false,
            watch: null,
            write: true,
            outDir: client,
            manifest: manifestFileName,
        },
    });
    const clientFiles = [clientResult].flat().flatMap(result => result.output.map(file => file.fileName));
    for (const file of ['index.html', manifestFileName]) {
        if (!clientFiles.includes(file))
            throw new Error(`The Jolt SSR client build did not emit '${file}'. Check the client input and output in your Vite configuration.`);
        await fs.access(path.join(client, file));
    }

    const serverResult = await vite.build({
        root: projectDirectory,
        plugins: await joltVitePlugins(projectDirectory, false),
        // Bundle npm dependencies for deployment outside the authoring project.
        ssr: { target: 'node', noExternal: true },
        build: {
            ssr: serverEntry,
            lib: false,
            watch: null,
            write: true,
            outDir: server,
            copyPublicDir: false,
            manifest: false,
            ssrManifest: false,
            rolldownOptions: {
                preserveEntrySignatures: 'strict',
                output: {
                    format: 'es',
                    entryFileNames: entryFileName,
                    chunkFileNames: 'chunks/[name]-[hash].mjs',
                },
            },
        },
    });
    const serverFiles = [serverResult].flat().flatMap(result => result.output);
    const entry = serverFiles.find(file => file.fileName === entryFileName);
    if (entry?.type !== 'chunk' || !entry.isEntry || !entry.exports.includes('render'))
        throw new Error(`Jolt SSR entry '${serverEntry}' must emit '${entryFileName}' with a named render export.`);
    const emitted = new Set(serverFiles.map(file => file.fileName));
    for (const chunk of serverFiles.filter(file => file.type === 'chunk')) {
        for (const specifier of [...chunk.imports, ...chunk.dynamicImports]) {
            const relative = specifier.startsWith('./') || specifier.startsWith('../');
            // Rolldown reports generated chunk names relative to the output root.
            // User externals can survive noExternal and must be resolvable on deployment.
            if (isBuiltin(specifier) || emitted.has(specifier) ||
                (relative && emitted.has(path.posix.join(path.posix.dirname(chunk.fileName), specifier))))
                continue;
            throw new Error(`Jolt SSR chunk '${chunk.fileName}' imports '${specifier}' outside the server bundle. Remove it from Vite's SSR or Rolldown externals so the deployment can run without node_modules.`);
        }
    }
    await fs.access(path.join(server, entryFileName));
    return JSON.stringify({ operation: 'buildJoltSsr', status: 'completed', pid: process.pid });
};

exports.buildJoltLibrary = async (requestJson) => {
    const { projectDirectory, generatedDirectory, outputDirectory, modulePaths } = JSON.parse(requestJson);
    const manifest = JSON.parse(await fs.readFile(path.join(projectDirectory, 'package.json'), 'utf8'));
    const packages = Object.assign({}, manifest.dependencies, manifest.peerDependencies, manifest.optionalDependencies);
    const input = Object.fromEntries(modulePaths.map(file => [
        'components/' + file.slice(0, -'.mjs'.length), path.join(generatedDirectory, file)
    ]));
    const vite = await loadVite(projectDirectory);
    await vite.build({
        root: projectDirectory,
        base: './',
        plugins: [
            ...await joltVitePlugins(projectDirectory, false, generatedDirectory),
            {
                name: 'jolt-library-styles',
                generateBundle: {
                    order: 'post',
                    handler(_, bundle) {
                        for (const chunk of Object.values(bundle)) {
                            if (chunk.type !== 'chunk') continue;
                            for (const css of chunk.viteMetadata?.importedCss ?? []) {
                                let relative = path.posix.relative(path.posix.dirname(chunk.fileName), css);
                                if (!relative.startsWith('.')) relative = './' + relative;
                                // Retain CSS from SFCs and Jolt styles in the consumer's
                                // module graph, including CSS owned by shared chunks.
                                chunk.code += '\nimport ' + JSON.stringify(relative) + ';\n';
                            }
                        }
                    },
                },
            },
        ],
        build: {
            lib: { entry: input, formats: ['es'] },
            outDir: outputDirectory,
            copyPublicDir: false,
            cssCodeSplit: true,
            sourcemap: true,
            rolldownOptions: {
                external(id) {
                    const segments = id.split('/');
                    const name = id.startsWith('@') ? segments.slice(0, 2).join('/') : segments[0];
                    return name === 'vue' || Object.hasOwn(packages, name);
                },
                output: {
                    entryFileNames: '[name].mjs',
                    chunkFileNames: 'chunks/[name]-[hash].mjs',
                },
            },
        },
    });
    return JSON.stringify({ operation: 'buildJoltLibrary', status: 'completed', pid: process.pid });
};

exports.check = async (requestJson) => {
    const { projectDirectory, entryPaths } = JSON.parse(requestJson);
    const vite = await loadVite(projectDirectory);
    const server = await vite.createServer({
        root: projectDirectory,
        appType: 'custom',
        server: { middlewareMode: true, watch: null, hmr: false, ws: false },
        optimizeDeps: { noDiscovery: true, include: [] }
    });
    try {
        const visited = new Set();
        async function visit(url) {
            if (visited.has(url)) return;
            visited.add(url);
            await server.transformRequest(url);
            const module = await server.moduleGraph.getModuleByUrl(url);
            for (const dependency of module?.importedModules ?? []) {
                // Transform authored local modules without evaluating their
                // initializers. Package exports resolve through Vite; package
                // implementations are checked by the eventual build.
                if (dependency.file && !dependency.file.includes('/node_modules/'))
                    await visit(dependency.url);
            }
        }
        for (const entry of entryPaths)
            await visit('/' + path.relative(projectDirectory, entry).split(path.sep).join('/'));
        return JSON.stringify({ operation: 'check', modules: visited.size, pid: process.pid });
    } finally {
        await server.close();
    }
};

let ownedViteServer;
exports.startVite = async (requestJson) => {
    const { projectDirectory, host, port, base, jolt } = JSON.parse(requestJson);
    const vite = await loadVite(projectDirectory);
    ownedViteServer = await vite.createServer({ root: projectDirectory, base, server: { host, port, strictPort: true, open: false },
        plugins: jolt ? await joltVitePlugins(projectDirectory, true) : [] });
    try {
        await ownedViteServer.listen();
        return JSON.stringify({ operation: 'vite', status: 'listening', pid: process.pid, origin: ownedViteServer.resolvedUrls.local[0] });
    } catch (error) {
        await ownedViteServer.close();
        ownedViteServer = undefined;
        throw error;
    }
};

exports.closeVite = async () => {
    if (ownedViteServer) await ownedViteServer.close();
    ownedViteServer = undefined;
    return JSON.stringify({ operation: 'vite', status: 'closed' });
};

exports.verifyVite = async (projectDirectory) => {
    try {
        const vite = await loadVite(projectDirectory);
        const server = await vite.createServer({ root: projectDirectory, configFile: false, server: { port: 0, host: '127.0.0.1' } });
        try {
            await server.listen();
            const response = await fetch(server.resolvedUrls.local[0] + '@vite/client');
            if (!response.ok) throw new Error('Vite client failed: ' + response.status);
        } finally {
            await server.close();
        }
        await vite.build({ root: projectDirectory, configFile: false, build: { outDir: path.join(projectDirectory, 'dist'), rolldownOptions: { input: path.join(projectDirectory, 'entry.js') } } });
        return JSON.stringify({ operation: 'vite', status: 'passed', pid: process.pid, node: process.version });
    } catch (error) {
        throw new Error(inspect(error, { depth: 8 }));
    }
};
