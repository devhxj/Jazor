const fs = require('node:fs/promises');
const path = require('node:path');
const crypto = require('node:crypto');
const { createRequire, isBuiltin } = require('node:module');
const { pathToFileURL } = require('node:url');

const hash = (value) => crypto.createHash('sha256').update(value).digest('hex');

function compileJoltStyle(requireFromProject, stylesheet, filename) {
    const blocksHeader = '/* jolt-style-blocks */\n';
    if (stylesheet.startsWith(blocksHeader))
        return JSON.parse(stylesheet.slice(blocksHeader.length)).map(block => block.scope === null
            ? block.css : compileJoltStyle(requireFromProject, '/* ' + block.scope + ' */\n' + block.css, filename)).join('\n');
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
    const applicationManifest = path.join(projectDirectory, 'package.json');
    const packageBoundaries = new Map();

    function findPackageBoundary(directory) {
        const relative = path.relative(projectDirectory, directory);
        if (!relative || relative === '..' || relative.startsWith('..' + path.sep) || path.isAbsolute(relative) ||
            relative.split(path.sep).includes('node_modules'))
            return;
        let pending = packageBoundaries.get(directory);
        if (!pending) {
            pending = readPackageBoundary(directory);
            packageBoundaries.set(directory, pending);
        }
        return pending;
    }

    async function readPackageBoundary(directory) {
        try {
            const manifest = JSON.parse(await fs.readFile(path.join(directory, 'package.json'), 'utf8'));
            // The nearest package boundary owns the importer. Only Emit-managed external
            // dependencies are resolved from the application.
            if (manifest.name === '@jazor/generated')
                return new Set(Object.values(manifest.jazor?.packages ?? {})
                    .filter(entry => entry.source === 'npm' || entry.source === 'jsr')
                    .flatMap(entry => [entry.specifier, entry.canonicalName]));
        } catch (error) {
            if (error.code !== 'ENOENT') throw error;
            return await findPackageBoundary(path.dirname(directory));
        } finally {
            // Share lookups within a build. In dev, share only concurrent lookups so a
            // regenerated, added, or removed package.json is visible on the next resolution.
            if (development) packageBoundaries.delete(directory);
        }
    }

    return {
        name: 'jolt-components',
        enforce: 'pre',
        buildStart() {
            packageBoundaries.clear();
        },
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
            const bare = /^(?<name>@[^/?#]+\/[^/?#]+|[^@./#?\0][^/?#]*)/.exec(source)?.groups?.name;
            const filename = importer?.split('?')[0];
            if (bare && !bare.includes(':') && !bare.includes('\\') && !isBuiltin(source) && filename &&
                path.isAbsolute(filename) && (await findPackageBoundary(path.dirname(filename)))?.has(bare)) {
                const resolved = await this.resolve(source, applicationManifest, { ...options, skipSelf: true });
                if (!resolved)
                    this.error('Cannot resolve ' + JSON.stringify(source) + ' from the Jolt application for ' + importer);
                return resolved;
            }
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

exports.plugins = joltVitePlugins;

exports.build = async (requestJson, toolchain) => {
    const { projectDirectory } = JSON.parse(requestJson);
    const vite = await require(toolchain).loadVite(projectDirectory);
    await vite.build({ root: projectDirectory, plugins: await joltVitePlugins(projectDirectory, false) });
    return JSON.stringify({ operation: 'build', status: 'completed', pid: process.pid });
};

exports.buildJoltSsr = async (requestJson, toolchain) => {
    const { projectDirectory, serverEntry, outputDirectory } = JSON.parse(requestJson);
    const vite = await require(toolchain).loadVite(projectDirectory);
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

exports.buildJoltLibrary = async (requestJson, toolchain) => {
    const { projectDirectory, generatedDirectory, outputDirectory, modulePaths } = JSON.parse(requestJson);
    const manifest = JSON.parse(await fs.readFile(path.join(projectDirectory, 'package.json'), 'utf8'));
    const packages = Object.assign({}, manifest.dependencies, manifest.peerDependencies, manifest.optionalDependencies);
    const input = Object.fromEntries(modulePaths.map(file => [
        'components/' + file.slice(0, -'.mjs'.length), path.join(generatedDirectory, file)
    ]));
    const vite = await require(toolchain).loadVite(projectDirectory);
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
