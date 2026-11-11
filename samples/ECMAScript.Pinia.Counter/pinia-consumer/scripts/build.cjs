const fs = require('node:fs/promises');
const path = require('node:path');

exports.build = async (requestJson) => {
    const { consumerRoot, generatedRoot, bundleRoot, distRoot } = JSON.parse(requestJson);
    const source = path.join(bundleRoot, 'dist');
    await fs.access(path.join(source, 'bundle.js'));
    await fs.rm(distRoot, { recursive: true, force: true });
    const assets = path.join(distRoot, 'assets');
    await fs.cp(source, assets, { recursive: true });
    await fs.copyFile(path.join(consumerRoot, 'src/style.css'), path.join(assets, 'style.css'));
    const template = await fs.readFile(path.join(consumerRoot, 'index.html'), 'utf8');
    await fs.writeFile(path.join(distRoot, 'index.html'), template
        .replace('</head>', '  <link rel="stylesheet" href="./assets/style.css" />\n</head>')
        .replace('  <script type="module" src="/src/main.js"></script>',
            '  <script type="module">import { Boot } from "./assets/bundle.js"; Boot("#app");</script>'));
    return JSON.stringify({ pid: process.pid, status: 'built' });
};
