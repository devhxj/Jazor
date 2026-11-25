// Load the designer and its stylesheet together when the user first opens it.
export async function loadHiprint() {
    const [runtime] = await Promise.all([
        import('vue-plugin-hiprint/dist/vue-plugin-hiprint.js'),
        import('vue-plugin-hiprint/dist/print-lock.css'),
    ])
    return runtime
}
