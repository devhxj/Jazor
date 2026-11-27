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

async function joltVitePlugins(projectDirectory, development, generatedDirectory = path.join(projectDirectory, '.jazor')) {
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
    const ownership = path.join(modules, 'jolt.modules.json');
    const emitOwnership = path.join(modules, 'jazor.modules.json');
    let ownedModules;
    let emitInventory;
    let browserAssetPrefixes;
    let MagicString;
    const logicalModules = new Map();

    function resolveGeneratedModule(file, options, suffix = '') {
        // 依赖扫描直接读取物理文件。路径分隔符要与 Vite 一致，否则 Windows 下绝对和相对导入会各自创建模块实例。
        if (!development || options.scan) return file.replaceAll('\\', '/') + suffix;
        const logical = path.resolve(projectDirectory, path.relative(modules, file)).replaceAll('\\', '/');
        logicalModules.set(logical, file);
        return logical + suffix;
    }

    function inside(root, file) {
        const relative = path.relative(root, file);
        return relative !== '..' && !relative.startsWith('..' + path.sep) && !path.isAbsolute(relative);
    }

    async function checkAuthoredCollision(context, target) {
        const relative = path.relative(modules, target).replaceAll('\\', '/');
        const authored = path.resolve(projectDirectory, relative);
        if (target !== authored && await fs.stat(authored).then(stat => stat.isFile(), error => {
            if (error.code !== 'ENOENT' && error.code !== 'ENOTDIR') throw error;
            return false;
        }))
            context.error(`Generated module '/${relative}' overlaps authored file '${authored}'.`);
    }

    async function readInventory(file, select, empty) {
        try {
            return select(JSON.parse(await fs.readFile(file, 'utf8')));
        } catch (error) {
            if (error.code !== 'ENOENT') throw error;
            return empty;
        }
    }
    const readOwnedModules = () => ownedModules ??= readInventory(ownership, paths => new Set(paths), new Set());
    const readEmitInventory = () => emitInventory ??= readInventory(emitOwnership, manifest => ({
        emitted: new Set([...manifest.modules, ...(manifest.assets ?? [])].map(entry => entry.path)),
        staticAssets: new Set((manifest.assets ?? []).filter(asset => asset.kind === 'static').map(asset => asset.path)),
    }), { emitted: new Set(), staticAssets: new Set() });

    async function owns(file) {
        return inside(modules, file) &&
            (await readOwnedModules()).has(path.relative(modules, file).replaceAll('\\', '/'));
    }

    function findPackageBoundary(directory) {
        const relative = path.relative(projectDirectory, directory);
        if (!relative || (!inside(projectDirectory, directory) && !inside(modules, directory)) ||
            relative.split(path.sep).includes('node_modules'))
            return;
        let pending = packageBoundaries.get(directory);
        if (!pending) {
            pending = readPackageBoundary(directory).catch(error => {
                if (packageBoundaries.get(directory) === pending)
                    packageBoundaries.delete(directory);
                throw error;
            });
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
            if (directory !== modules)
                return await findPackageBoundary(path.dirname(directory));
        }
    }

    return {
        name: 'jolt-components',
        enforce: 'pre',
        async buildStart() {
            packageBoundaries.clear();
            if (!development) {
                ownedModules = emitInventory = undefined;
                await Promise.all([readOwnedModules(), readEmitInventory()]);
            }
        },
        configureServer(server) {
            if (development) {
                // Drop the pending snapshot as well. If an event arrives during a
                // read, the next resolver starts a fresh read of the replacement.
                server.watcher.on('all', (event, file) => {
                    if (event !== 'add' && event !== 'change' && event !== 'unlink') return;
                    const absolute = path.resolve(file);
                    if (absolute === ownership) ownedModules = undefined;
                    if (absolute === emitOwnership) emitInventory = undefined;
                    if (path.basename(absolute) === 'package.json') {
                        const directory = path.dirname(absolute);
                        // Both inherited and missing boundaries can change when a nearer
                        // manifest appears. Unrelated importer directories keep their cache.
                        for (const importerDirectory of packageBoundaries.keys())
                            if (inside(directory, importerDirectory))
                                packageBoundaries.delete(importerDirectory);
                    }
                });
                server.watcher.add([modules, ownership, emitOwnership]);
                const projectRelative = path.relative(server.config.root, modules).replaceAll('\\', '/');
                const physicalRoot = modules.replaceAll('\\', '/');
                const physical = '/@fs/' + physicalRoot.replace(/^\//, '') + '/';
                browserAssetPrefixes = [physical];
                if (inside(server.config.root, modules))
                    browserAssetPrefixes.push('/' + projectRelative + '/');
                const scratch = path.join(projectDirectory, '.jazor');
                server.middlewares.use(async (request, response, next) => {
                    const url = decodeURI(request.url.split('?')[0]);
                    const pathname = url.startsWith(server.config.base) ? '/' + url.slice(server.config.base.length) : url;
                    const file = pathname.startsWith('/@fs/') ? pathname.slice('/@fs'.length).replace(/^\/(?=[a-z]:)/i, '') :
                        path.resolve(projectDirectory, '.' + pathname);
                    if (inside(modules, file) || inside(scratch, file)) {
                        response.statusCode = 404;
                        response.end();
                        return;
                    }
                    if (pathname.startsWith('/') && !pathname.startsWith('/@fs/') && inside(projectDirectory, file)) {
                        const relative = path.relative(projectDirectory, file).replaceAll('\\', '/');
                        if ((await readEmitInventory()).staticAssets.has(relative)) {
                            // The browser resolves import.meta.url against the logical
                            // module URL. Let Vite serve its inventoried relative asset.
                            const query = request.url.indexOf('?');
                            request.url = (url.startsWith(server.config.base) ? server.config.base : '/') +
                                encodeURI(physical.slice(1) + relative) + (query < 0 ? '' : request.url.slice(query));
                        }
                    }
                    next();
                });
            }
        },
        transform: {
            order: 'post',
            async handler(code, id) {
                if (!development || !browserAssetPrefixes) return;
                const assets = (await readEmitInventory()).staticAssets;
                if (!assets.size) return;
                let rewritten;
                const update = (start, end, replacement) => {
                    // Use Vite's source-map library to preserve earlier mappings.
                    if (!MagicString) {
                        const viteRequire = createRequire(requireFromProject.resolve('vite/package.json'));
                        MagicString = viteRequire('magic-string');
                    }
                    rewritten ??= new MagicString(code);
                    rewritten.update(start, end, replacement);
                };
                for (const prefix of browserAssetPrefixes) {
                    const escaped = prefix.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
                    const urls = new RegExp(escaped + "([^\\s\"'?#()<>\\\\]+)", 'g');
                    for (const match of code.matchAll(urls)) {
                        const asset = /%[0-9a-f]{2}/i.test(match[1]) ? decodeURI(match[1]) : match[1];
                        if (!assets.has(asset)) continue;
                        // Run after Vite computes imports and CSS URLs, preserving its
                        // asset pipeline and module graph while exposing logical URLs.
                        update(match.index, match.index + match[0].length, '/' + match[1]);
                    }
                }
                if (inside(modules, id) && id.endsWith('.css')) {
                    const physicalId = JSON.stringify(id.replaceAll('\\', '/'));
                    const styleId = 'const __vite__id = ' + physicalId;
                    const offset = code.indexOf(styleId);
                    const relative = path.relative(modules, id).replaceAll('\\', '/');
                    if (offset >= 0 && assets.has(relative))
                        update(offset, offset + styleId.length, 'const __vite__id = ' + JSON.stringify('/' + relative));
                }
                if (rewritten)
                    return { code: rewritten.toString(), map: rewritten.generateMap({ hires: true }) };
            },
        },
        async handleHotUpdate(context) {
            if (!inside(modules, context.file)) return;
            const relative = path.relative(modules, context.file).replaceAll('\\', '/');
            if ((await readEmitInventory()).staticAssets.has(relative)) {
                // The request middleware resolves logical asset URLs to disk.
                // HMR must send clients back through that logical route.
                for (const module of context.modules) {
                    const query = module.url.indexOf('?');
                    module.url = '/' + relative + (query < 0 ? '' : module.url.slice(query));
                }
            }
            const logical = path.resolve(projectDirectory, path.relative(modules, context.file)).replaceAll('\\', '/');
            if (logicalModules.has(logical)) {
                const affected = context.server.moduleGraph.getModulesByFile(logical);
                if (affected) return [...new Set([...context.modules, ...affected])];
            }
        },
        config() {
            if (development) {
                // The compiler creates and deletes its metadata runners while Vite is live.
                const scratch = path.resolve(projectDirectory, '.jazor');
                return { resolve: { dedupe: ['vue'] }, server: {
                    fs: { allow: [projectDirectory, modules] },
                    watch: { ignored: file => {
                        if (inside(modules, file)) {
                            const part = path.relative(modules, file).split(path.sep)[0];
                            return ['cache', 'development-build', 'dist', 'npm-metadata', 'node_modules', 'pnpm-store'].includes(part);
                        }
                        return inside(scratch, file);
                    } },
                } };
            }
            return { resolve: { dedupe: ['vue'] } };
        },
        async resolveId(source, importer, options) {
            if (/^\/[^/]/.test(source)) {
                const query = source.search(/[?#]/);
                const relative = source.slice(1, query < 0 ? undefined : query);
                const inventory = await readEmitInventory();
                if (inventory.staticAssets.has(relative)) {
                    const target = path.resolve(modules, relative);
                    await checkAuthoredCollision(this, target);
                    // Keep the physical filename here so Vite selects the appropriate
                    // CSS or binary asset transform, including query variants.
                    return target.replaceAll('\\', '/') + (query < 0 ? '' : source.slice(query));
                }
                if (/\.(?:m?js|jolt\.css)$/.test(relative) &&
                    (inventory.emitted.has(relative) || (await readOwnedModules()).has(relative))) {
                    const target = path.resolve(modules, relative);
                    await checkAuthoredCollision(this, target);
                    return resolveGeneratedModule(target, options, query < 0 ? '' : source.slice(query));
                }
            }
            const bare = /^(?<name>@[^/?#]+\/[^/?#]+|[^@./#?\0][^/?#]*)/.exec(source)?.groups?.name;
            const importerId = importer?.split('?')[0];
            const filename = logicalModules.get(importerId) ?? importerId;
            if (filename !== importerId && source.startsWith('.')) {
                const query = source.search(/[?#]/);
                const suffix = query < 0 ? '' : source.slice(query);
                const target = path.resolve(path.dirname(filename), query < 0 ? source : source.slice(0, query));
                const relative = path.relative(modules, target).replaceAll('\\', '/');
                if (/\.(?:m?js|jolt\.css)$/.test(target) && inside(modules, target) &&
                    ((await readOwnedModules()).has(relative) || (await readEmitInventory()).emitted.has(relative))) {
                    await checkAuthoredCollision(this, target);
                    return resolveGeneratedModule(target, options, suffix);
                }
                return await this.resolve(source, filename, { ...options, skipSelf: true }) ??
                    this.resolve(source, importer, { ...options, skipSelf: true });
            }
            if (bare && !bare.includes(':') && !bare.includes('\\') && !isBuiltin(source) && filename &&
                path.isAbsolute(filename) &&
                ((await findPackageBoundary(path.dirname(filename)))?.has(bare) ||
                 !inside(projectDirectory, filename) && await owns(filename))) {
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
            // Match the ID used by generated imports so Vite tracks one HMR boundary.
            const generated = path.join(modules, relative.replace(/\.jazor$/i, '.mjs'));
            await checkAuthoredCollision(this, generated);
            return resolveGeneratedModule(generated, options);
        },
        transformIndexHtml: {
            order: 'pre',
            async handler(html, { filename }) {
                if (development) return;
                const { staticAssets } = await readEmitInventory();
                if (!staticAssets.size) return;
                // Native Vite HTML handling reads URL attributes relative to the HTML
                // source. Rebase the inventoried paths before Vite hashes those files.
                return html.replace(/<!--[\s\S]*?-->|<(script|style)\b(?:[^"'<>]|"[^"]*"|'[^']*')*>[\s\S]*?<\/\1\s*>|<(?:img|source|video|audio|track|input|link|object|embed)\b(?:[^"'<>]|"[^"]*"|'[^']*')*>/gi, tag => {
                    if (/^<(?:!|script\b|style\b)/i.test(tag)) return tag;
                    return tag.replace(/\s([\w:-]+)(\s*=\s*)("[^"]*"|'[^']*'|[^\s>]+)/g, (attr, name, equals, value) => {
                        if (!/^(?:src|href|poster|data)$/i.test(name)) return attr;
                        const url = /^["']/.test(value) ? value.slice(1, -1) : value;
                        if (!/^\/[^/]/.test(url)) return attr;
                        const query = url.search(/[?#]/);
                        const relative = decodeURI(url.slice(1, query < 0 ? undefined : query));
                        if (!staticAssets.has(relative)) return attr;
                        const target = path.resolve(modules, relative);
                        const file = path.relative(path.dirname(filename), target).replaceAll('\\', '/');
                        const suffix = query < 0 ? '' : url.slice(query);
                        return ' ' + name + equals + '"' + (file.startsWith('.') ? '' : './') +
                            file.split('/').map(encodeURIComponent).join('/') + suffix.replaceAll('"', '&quot;') + '"';
                    });
                });
            },
        },
        async load(id) {
            const disk = logicalModules.get(id.split('?')[0]);
            if (disk) id = disk;
            const relative = path.relative(modules, id);
            if (!await owns(id)) {
                if (disk) {
                    const code = await fs.readFile(id, 'utf8');
                    const match = /\/\/# sourceMappingURL=([^\s]+)\s*$/.exec(code);
                    if (match?.[1] === path.basename(id) + '.map') {
                        const map = JSON.parse(await fs.readFile(id + '.map', 'utf8'));
                        const sourceRoot = path.resolve(path.dirname(id), map.sourceRoot ?? '');
                        map.sources = map.sources.map(source =>
                            /^[a-z][a-z\d+.-]*:\/\//i.test(source) ? source : path.resolve(sourceRoot, source).replaceAll('\\', '/'));
                        delete map.sourceRoot;
                        return { code, map };
                    }
                    return code;
                }
                return;
            }
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
                return disk ? fs.readFile(id, 'utf8') : undefined;
            this.addWatchFile(id);
            let [code, map] = await Promise.all([fs.readFile(id, 'utf8'), fs.readFile(id + '.map', 'utf8')]);
            const sourceMap = JSON.parse(map);
            sourceMap.sources = sourceMap.sources.map(source => path.resolve(projectDirectory, source).replaceAll('\\', '/'));
            if (development) {
                const body = this.parse(code).body;
                const exported = body.find(node => node.type === 'ExportDefaultDeclaration');
                if (exported) {
                    const stateClass = body.find(node => node.type === 'ClassDeclaration' &&
                        node.body.body.some(member => member.type === 'MethodDefinition' && member.key.name === '__JoltRender'));
                    const render = stateClass?.body.body.find(member => member.type === 'MethodDefinition' && member.key.name === '__JoltRender');
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
export const $joltHmrState = ${stateClass ? stateClass.id.name + '.prototype' : 'undefined'};
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
    if (import.meta.hot.data.joltSignature !== $joltHmrSignature)
        import.meta.hot.data.joltState = $joltHmrState;
    import.meta.hot.data.joltSignature = $joltHmrSignature;
    import.meta.hot.data.joltImports = $joltImports;
    ${component}.__hmrId = ${hmrId};
    globalThis.__VUE_HMR_RUNTIME__?.createRecord(${hmrId}, ${component});
    import.meta.hot.accept(updated => {
        if (!updated) return;
        const runtime = globalThis.__VUE_HMR_RUNTIME__;
        if ($joltHmrSignature && $joltHmrSignature === updated.$joltHmrSignature) {
            // Vue keeps setup state on rerender. Refresh its prototype methods so
            // subsequent handlers close over the updated ordinary module imports.
            const methods = Object.getOwnPropertyDescriptors(updated.$joltHmrState);
            delete methods.constructor;
            Object.defineProperties(import.meta.hot.data.joltState, methods);
            runtime?.rerender(${hmrId}, updated.default.render);
        } else {
            runtime?.reload(${hmrId}, updated.default);
        }
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
    const { projectDirectory, generatedDirectory } = JSON.parse(requestJson);
    const vite = await require(toolchain).loadVite(projectDirectory);
    await vite.build({ root: projectDirectory, plugins: await joltVitePlugins(projectDirectory, false, generatedDirectory),
        build: { outDir: path.join(generatedDirectory, 'dist'), emptyOutDir: true } });
    return JSON.stringify({ operation: 'build', status: 'completed', pid: process.pid });
};

exports.buildJoltSsr = async (requestJson, toolchain) => {
    const { projectDirectory, generatedDirectory, serverEntry, outputDirectory } = JSON.parse(requestJson);
    const vite = await require(toolchain).loadVite(projectDirectory);
    const client = path.join(outputDirectory, 'client');
    const server = path.join(outputDirectory, 'server');
    const entryFileName = 'entry-server.mjs';
    const manifestFileName = '.vite/manifest.json';

    const clientResult = await vite.build({
        root: projectDirectory,
        plugins: await joltVitePlugins(projectDirectory, false, generatedDirectory),
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
        plugins: await joltVitePlugins(projectDirectory, false, generatedDirectory),
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
