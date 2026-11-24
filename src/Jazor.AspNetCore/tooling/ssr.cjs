const path = require('node:path');
const { pathToFileURL } = require('node:url');

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
